/*
    Tabla de codigos de recuperacion de contrasenia ("Olvide mi contrasenia").

    Ejecutar una sola vez sobre la base BarrioCerradoI. Es idempotente:
    se puede correr de nuevo sin romper nada.

    Decisiones de diseno:
      - El codigo NO se guarda en texto plano. Se guarda el mismo formato
        PBKDF2 que usa BIZ.Hash para las contrasenias. Si alguien lee la
        tabla, no puede usar los codigos pendientes.
      - fecha_expiracion se calcula en C# (BIZ.Recuperacion.MinutosDeVigencia)
        para no depender del reloj del server de base.
      - intentos limita el fuerza bruta sobre un codigo de 6 digitos.
      - Esta tabla usa IDENTITY normal + SCOPE_IDENTITY(), no el patron
        SET IDENTITY_INSERT + MAX(id)+1 del resto del proyecto, que se rompe
        si dos altas ocurren al mismo tiempo.
*/

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
        intentos          INT           NOT NULL,

        CONSTRAINT PK_RecuperacionContrasenia
            PRIMARY KEY CLUSTERED (id_recuperacion),

        CONSTRAINT FK_RecuperacionContrasenia_Cuenta
            FOREIGN KEY (id_cuenta) REFERENCES dbo.Cuenta (id_cuenta),

        CONSTRAINT DF_RecuperacionContrasenia_intentos
            DEFAULT (0) FOR intentos
    );

    /* Busqueda tipica: el codigo pendiente mas nuevo de una cuenta. */
    CREATE INDEX IX_RecuperacionContrasenia_Cuenta_Pendiente
        ON dbo.RecuperacionContrasenia (id_cuenta, fecha_uso, fecha_expiracion DESC);

    PRINT 'Tabla dbo.RecuperacionContrasenia creada.';
END
ELSE
BEGIN
    PRINT 'Tabla dbo.RecuperacionContrasenia ya existia, no se toco.';
END
GO
