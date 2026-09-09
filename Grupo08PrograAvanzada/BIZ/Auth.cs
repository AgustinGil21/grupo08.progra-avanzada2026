using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    [Serializable]
    public class Auth
    {
        private static bool CuentaExiste(string correo)
        {
            string q = @"SELECT TOP 1 1 FROM dbo.Usuario WHERE correo_electronico = @correo;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
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

        private static string ObtenerContraseniaValidaDeCuenta(string correo)
        {
            string q = @"
            SELECT c.hash_contrasenia
            FROM Contrasenia c
            INNER JOIN Cuenta cu ON cu.id_cuenta = c.id_cuenta
            INNER JOIN Usuario u ON u.id_cuenta = cu.id_cuenta
            WHERE u.correo_electronico = @correo AND c.id_estado = 1;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);

                    var resultado = cmd.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                        return null;

                    if (resultado is byte[] bytes)
                        return System.Text.Encoding.Unicode.GetString(bytes);

                    return (string)resultado;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo obtener la contrasenia: " + ex.Message);
            }
        }

        private static bool ContraseniaCorrecta(string correo, string contrasenia)
        {
            var contraseniaValida = ObtenerContraseniaValidaDeCuenta(correo);

            return Hash.VerificarContrasenia(contrasenia, contraseniaValida);
        }

        public static DataSet IniciarSesion(string correo, string contrasenia)
        {
            DataSet ds = new DataSet();
            string q = @"
            SELECT u.id_usuario, u.correo_electronico, na.descripcion as rol, c.nombre, c.apellido, p.descripcion as pais, p.prefijo, c.telefono, c.fecha_alta, td.descripcion as tipo_documento, c.nro_documento, c.fecha_nacimiento
            FROM Usuario u
            INNER JOIN Cuenta c ON c.id_cuenta = u.id_cuenta
            LEFT JOIN NivelAcceso na ON na.id_nivel_acceso = u.id_nivel_acceso
            LEFT JOIN Pais p ON p.id_pais = c.id_pais
            LEFT JOIN TipoDocumento td ON td.id_tipo_documento = c.id_tipo_documento
            WHERE u.correo_electronico = @correo AND c.id_estado = 1";

            if (!CuentaExiste(correo))
            {
                throw new Exception("La cuenta no existe");
            }

            if (!ContraseniaCorrecta(correo, contrasenia))
            {
                throw new Exception("Contrasenia invalida");
            }

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);

                    using (var adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }

                    return ds;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo iniciar sesion: " + ex.Message);
            }
        }

        public static DataSet IniciarSesion(string correo, string contrasenia, int idGarita, int idTurno)
        {
            var datosUsuario = IniciarSesion(correo, contrasenia);

            if (datosUsuario.Tables.Count == 0 || datosUsuario.Tables[0].Rows.Count == 0)
                throw new Exception("Correo o contraseña incorrectos");

            int idUsuario = Convert.ToInt32(datosUsuario.Tables[0].Rows[0]["id_usuario"]);

            string q = @"
            IF NOT EXISTS (
                SELECT 1 FROM GaritaUsuario
                WHERE id_garita = @idGarita AND id_usuario = @idUsuario
                  AND fecha = CAST(GETDATE() AS date) AND id_turno = @idTurno
            )
            INSERT INTO GaritaUsuario(id_garita, id_usuario, fecha, id_turno)
            VALUES(@idGarita, @idUsuario, CAST(GETDATE() AS date), @idTurno)";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idGarita", idGarita);
                    cmd.Parameters.AddWithValue("@idTurno", idTurno);

                    cmd.ExecuteNonQuery();

                    return datosUsuario;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo iniciar sesion: " + ex.Message);
            }
        }



        public static int DarDeAltaCuenta(string nombre, string apellido, int idPais, string telefono,
      DateTime fechaNacimiento, int idTipoDocumento, string nroDocumento)
        {
            string q = @"
              SET XACT_ABORT ON;
              BEGIN TRANSACTION;
                  DECLARE @idCuenta INT = (SELECT ISNULL(MAX(id_cuenta), 0) + 1 FROM Cuenta);
                  SET IDENTITY_INSERT Cuenta ON;
                  INSERT INTO Cuenta (id_cuenta, id_pais, id_estado, nombre, apellido, fecha_alta, fecha_nacimiento,
          id_tipo_documento, nro_documento, telefono)
                  VALUES (@idCuenta, @idPais, 1, @nombre, @apellido, GETDATE(), @fechaNacimiento, @idTipoDocumento,
          @nroDocumento, @telefono);
                  SET IDENTITY_INSERT Cuenta OFF;
              COMMIT TRANSACTION;
              SELECT @idCuenta;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@apellido", apellido);
                    cmd.Parameters.AddWithValue("@idPais", idPais);
                    cmd.Parameters.AddWithValue("@telefono", telefono);
                    cmd.Parameters.AddWithValue("@fechaNacimiento", fechaNacimiento);
                    cmd.Parameters.AddWithValue("@idTipoDocumento", idTipoDocumento);
                    cmd.Parameters.AddWithValue("@nroDocumento", nroDocumento);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta la cuenta: " + ex.Message);
            }
        }

        public static int DarDeAltaUsuario(string nombre, string apellido, int idPais, string telefono,
     DateTime fechaNacimiento, int idTipoDocumento, string nroDocumento,
     string correo, string contrasenia, int idNivelAcceso)
        {
            if (CuentaExiste(correo))
            {
                throw new Exception("La cuenta ya existe");
            }

            int idCuenta = DarDeAltaCuenta(nombre, apellido, idPais, telefono, fechaNacimiento, idTipoDocumento,
        nroDocumento);

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
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idCuenta", idCuenta);
                    cmd.Parameters.AddWithValue("@idNivelAcceso", idNivelAcceso);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.Add("@hashContrasenia", System.Data.SqlDbType.VarBinary).Value = hashContraseniaBytes;
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta el usuario: " + ex.Message);
            }
        }

        public static int DarDeAltaPersona(string nombre, string apellido, int idPais, string telefono,
            DateTime fechaNacimiento, int idTipoDocumento, string nroDocumento, string fotoReferencia)
        {
            int idCuenta = DarDeAltaCuenta(nombre, apellido, idPais, telefono, fechaNacimiento, idTipoDocumento,
        nroDocumento);

            string q = @"
      SET XACT_ABORT ON;
      BEGIN TRANSACTION;

          DECLARE @idPersona INT = (SELECT ISNULL(MAX(id_persona), 0) + 1 FROM Persona);
          SET IDENTITY_INSERT Persona ON;
          INSERT INTO Persona (id_persona, foto_referencia, id_cuenta)
          VALUES (@idPersona, @fotoReferencia, @idCuenta);
          SET IDENTITY_INSERT Persona OFF;

      COMMIT TRANSACTION;

      SELECT @idPersona;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idCuenta", idCuenta);
                    cmd.Parameters.AddWithValue("@fotoReferencia", (object)fotoReferencia ?? DBNull.Value);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta la persona: " + ex.Message);
            }
        }
    }
}
