-- IntentarAccederConVehiculo e IntentarAccederSinVehiculo guardaban la foto en @foto NVARCHAR(500):
-- desde 03_FotoReferenciaMax.sql la columna es NVARCHAR(MAX) y una foto mas larga se cortaba sin avisar.
-- Toma la definicion actual de cada SP y solo cambia esa declaracion a NVARCHAR(MAX); el resto no se toca.
-- Correr una vez sobre BarrioCerradoI, despues de 03. Se puede volver a correr sin efecto.

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @procs TABLE (nombre SYSNAME);
INSERT INTO @procs VALUES (N'dbo.IntentarAccederConVehiculo'), (N'dbo.IntentarAccederSinVehiculo');

DECLARE @nombre SYSNAME, @sql NVARCHAR(MAX), @original NVARCHAR(MAX), @n INT, @pos INT;

WHILE EXISTS (SELECT 1 FROM @procs)
BEGIN
    SELECT TOP 1 @nombre = nombre FROM @procs;
    DELETE FROM @procs WHERE nombre = @nombre;

    SET @original = OBJECT_DEFINITION(OBJECT_ID(@nombre));
    IF @original IS NULL
    BEGIN
        PRINT @nombre + N' no existe, se saltea.';
        CONTINUE;
    END

    -- La declaracion esta alineada con espacios, distintos en cada SP
    SET @sql = @original;
    SET @n = 1;
    WHILE @n <= 40
    BEGIN
        SET @sql = REPLACE(@sql, N'@foto' + REPLICATE(N' ', @n) + N'NVARCHAR(500)',
                                 N'@foto' + REPLICATE(N' ', @n) + N'NVARCHAR(MAX)');
        SET @n = @n + 1;
    END

    IF @sql = @original
    BEGIN
        PRINT @nombre + N' ya tenia @foto en NVARCHAR(MAX), no se toco.';
        CONTINUE;
    END

    SET @pos = PATINDEX(N'%CREATE%PROC%', @sql);
    IF @pos = 0 OR @pos > 200
        RAISERROR(N'No se encontro el CREATE PROCEDURE de %s.', 16, 1, @nombre);

    SET @sql = STUFF(@sql, @pos, 6, N'ALTER');
    EXEC sp_executesql @sql;
    PRINT @nombre + N': @foto pasada a NVARCHAR(MAX).';
END

COMMIT TRANSACTION;
GO
