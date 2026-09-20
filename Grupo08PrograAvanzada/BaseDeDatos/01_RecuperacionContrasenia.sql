-- Codigos de "Olvide mi contrasenia". Correr una vez sobre BarrioCerradoI.
-- El codigo se guarda hasheado igual que las contrasenias, no en texto plano.

IF OBJECT_ID('dbo.RecuperacionContrasenia', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RecuperacionContrasenia
    (
        id_recuperacion   INT           IDENTITY(1,1) NOT NULL,
        id_cuenta         INT           NOT NULL,
        codigo_hash       VARCHAR(200)  NOT NULL,
        fecha_solicitud   DATETIME      NOT NULL,
        fecha_expiracion  DATETIME      NOT NULL,
        fecha_uso         DATETIME      NULL,
        intentos          INT           NOT NULL
            CONSTRAINT DF_RecuperacionContrasenia_intentos DEFAULT (0),

        CONSTRAINT PK_RecuperacionContrasenia
            PRIMARY KEY CLUSTERED (id_recuperacion),

        CONSTRAINT FK_RecuperacionContrasenia_Cuenta
            FOREIGN KEY (id_cuenta) REFERENCES dbo.Cuenta (id_cuenta)
    );

    CREATE INDEX IX_RecuperacionContrasenia_Cuenta_Pendiente
        ON dbo.RecuperacionContrasenia (id_cuenta, fecha_uso, fecha_expiracion DESC);

    PRINT 'Tabla dbo.RecuperacionContrasenia creada.';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.RecuperacionContrasenia ya existia, no se toco.';
END
GO
