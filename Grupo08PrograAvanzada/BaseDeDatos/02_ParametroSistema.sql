-- Parametros generales del barrio. Correr una vez sobre BarrioCerradoI.
-- Es una tabla de una sola fila: se inserta la inicial y despues siempre se actualiza.

IF OBJECT_ID('dbo.ParametroSistema', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ParametroSistema
    (
        id_parametro_sistema          INT            IDENTITY(1,1) NOT NULL,
        nombre_barrio                 NVARCHAR(150)  NOT NULL,
        domicilio                     NVARCHAR(250)  NULL,
        zona_horaria                  NVARCHAR(100)  NOT NULL,
        meses_conservar_movimientos   INT            NOT NULL,
        meses_conservar_reportes      INT            NOT NULL,
        fecha_hora_modificacion       DATETIME       NOT NULL,
        id_usuario_modificacion       INT            NULL,

        CONSTRAINT PK_ParametroSistema
            PRIMARY KEY CLUSTERED (id_parametro_sistema),

        CONSTRAINT FK_ParametroSistema_Usuario
            FOREIGN KEY (id_usuario_modificacion) REFERENCES dbo.Usuario (id_usuario)
    );

    SET IDENTITY_INSERT dbo.ParametroSistema ON;

    INSERT INTO dbo.ParametroSistema
        (id_parametro_sistema, nombre_barrio, domicilio, zona_horaria,
         meses_conservar_movimientos, meses_conservar_reportes, fecha_hora_modificacion)
    VALUES
        (1, 'Barrio Cerrado I', NULL, 'GMT-3 - Buenos Aires', 24, 24, GETDATE());

    SET IDENTITY_INSERT dbo.ParametroSistema OFF;

    PRINT 'Tabla dbo.ParametroSistema creada con la fila inicial.';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.ParametroSistema ya existia, no se toco.';
END
GO
