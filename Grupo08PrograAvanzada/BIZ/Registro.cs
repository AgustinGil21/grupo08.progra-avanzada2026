using System;
using System.Data;
using System.Data.SqlClient;

namespace BIZ
{
    public class Registro
    {
        // Sin asignar: no tiene filas en NivelAccesoUrl, asi que PuedeAcceder da
        // false hasta que un administrador le ponga el nivel real.
        private const int ID_NIVEL_ACCESO_SIN_ASIGNAR = 3;

        public class ResultadoVerificacion
        {
            public int IdCuenta { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }

            public string NombreCompleto
            {
                get { return (Nombre + " " + Apellido).Trim(); }
            }
        }

        public static bool CorreoTieneFormatoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;
            if (correo.Trim() != correo) return false;

            try
            {
                var direccion = new System.Net.Mail.MailAddress(correo);
                return direccion.Address == correo && correo.Contains(".");
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static DataTable BuscarEnPadron(string nroDocumento)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  c.id_cuenta,
                    c.nombre,
                    c.apellido,
                    c.nro_documento,
                    u.correo_electronico
            FROM dbo.Cuenta c
            LEFT JOIN dbo.Usuario u ON u.id_cuenta = c.id_cuenta
            WHERE c.nro_documento = @nroDocumento;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@nroDocumento", nroDocumento);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo verificar el documento: " + ex.Message);
            }
        }

        private static bool CorreoYaRegistrado(string correo)
        {
            string q = @"SELECT TOP 1 1 FROM dbo.Usuario WHERE correo_electronico = @correo;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo comprobar si la cuenta ya existe: " + ex.Message);
            }
        }

        private static int InsertarUsuarioConContrasenia(int idCuenta, string correo, string contrasenia)
        {
            var hashContrasenia = Hash.HashearContrasenia(contrasenia);
            byte[] hashContraseniaBytes = System.Text.Encoding.Unicode.GetBytes(hashContrasenia);

            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                DECLARE @idUsuario INT = (SELECT ISNULL(MAX(id_usuario), 0) + 1 FROM Usuario);
                SET IDENTITY_INSERT Usuario ON;
                INSERT INTO Usuario (id_usuario, id_cuenta, id_nivel_acceso, correo_electronico)
                VALUES (@idUsuario, @idCuenta, @idNivelAcceso, @correo);
                SET IDENTITY_INSERT Usuario OFF;

                DECLARE @idContrasenia INT = (SELECT ISNULL(MAX(id_contrasenia), 0) + 1 FROM Contrasenia);
                SET IDENTITY_INSERT Contrasenia ON;
                INSERT INTO Contrasenia (id_contrasenia, hash_contrasenia, id_cuenta, id_estado)
                VALUES (@idContrasenia, @hashContrasenia, @idCuenta, 1);
                SET IDENTITY_INSERT Contrasenia OFF;

            COMMIT TRANSACTION;

            SELECT @idUsuario;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idCuenta", idCuenta);
                    cmd.Parameters.AddWithValue("@idNivelAcceso", ID_NIVEL_ACCESO_SIN_ASIGNAR);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.Add("@hashContrasenia", SqlDbType.VarBinary).Value = hashContraseniaBytes;
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta el usuario: " + ex.Message);
            }
        }

        public static ResultadoVerificacion VerificarDocumento(string nroDocumento)
        {
            if (string.IsNullOrWhiteSpace(nroDocumento))
                throw new Exception("Ingresa tu numero de documento.");

            DataTable dt = BuscarEnPadron(nroDocumento.Trim());

            if (dt.Rows.Count == 0)
                throw new Exception(
                    "No encontramos ese documento en el padron. " +
                    "Pedile a un administrador del barrio que te de de alta antes de registrarte.");

            DataRow fila = dt.Rows[0];

            if (fila["correo_electronico"] != DBNull.Value &&
                !string.IsNullOrWhiteSpace(fila["correo_electronico"].ToString()))
            {
                throw new Exception(
                    "Ese documento ya tiene un usuario asociado. " +
                    "Si no te acordas la contrasenia, usa la opcion 'Olvide mi contrasenia'.");
            }

            return new ResultadoVerificacion
            {
                IdCuenta = Convert.ToInt32(fila["id_cuenta"]),
                Nombre = fila["nombre"] == DBNull.Value ? "" : fila["nombre"].ToString(),
                Apellido = fila["apellido"] == DBNull.Value ? "" : fila["apellido"].ToString()
            };
        }

        public static int CrearUsuario(int idCuenta, string correo, string contrasenia, string repetirContrasenia)
        {
            if (!CorreoTieneFormatoValido(correo))
                throw new Exception("El correo no tiene un formato valido.");

            if (contrasenia != repetirContrasenia)
                throw new Exception("Las contrasenias no coinciden.");

            Contrasenia.ValidarPolitica(contrasenia);

            string correoLimpio = correo.Trim();

            if (CorreoYaRegistrado(correoLimpio))
                throw new Exception("Ese correo ya esta usado por otro usuario.");

            return InsertarUsuarioConContrasenia(idCuenta, correoLimpio, contrasenia);
        }

        public static int Registrar(string nroDocumento, string correo, string contrasenia, string repetirContrasenia)
        {
            var cuenta = VerificarDocumento(nroDocumento);

            return CrearUsuario(cuenta.IdCuenta, correo, contrasenia, repetirContrasenia);
        }
    }
}
