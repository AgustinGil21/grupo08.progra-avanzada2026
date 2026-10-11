-- Agranda Persona.foto_referencia de NVARCHAR(500) a NVARCHAR(MAX). Correr una vez sobre BarrioCerradoI.
-- Solo ensancha la columna: no se pierden datos y se puede volver a correr sin efecto.

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
           WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Persona'
             AND COLUMN_NAME = 'foto_referencia' AND CHARACTER_MAXIMUM_LENGTH <> -1)
BEGIN
    ALTER TABLE dbo.Persona ALTER COLUMN foto_referencia NVARCHAR(MAX) NULL;
    PRINT 'Columna dbo.Persona.foto_referencia pasada a NVARCHAR(MAX).';
END
ELSE
BEGIN
    PRINT 'dbo.Persona.foto_referencia ya era NVARCHAR(MAX), no se toco.';
END
GO
