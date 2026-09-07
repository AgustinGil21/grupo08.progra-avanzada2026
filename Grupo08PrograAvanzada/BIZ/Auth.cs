using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Auth
    {
        private static bool CuentaExiste(string correo)
        {
            string q = @"SELECT TOP 1 1 FROM dbo.Usuario WHERE correo = @correo;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using(var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    return cmd.ExecuteScalar() != null;
                }
            } catch(Exception ex)
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
            WHERE cu.correo = @correo AND c.id_estado = 1;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);

                    var resultado = cmd.ExecuteScalar();
                    return resultado == null || resultado == DBNull.Value
                        ? null
                        : (string)resultado;
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

        public static DataSet IniciarSesion(string correo, string contrasenia) {
            DataSet ds = new DataSet();
            string q = @"
            SELECT u.id_usuario, u.correo, na.descripcion as rol, c.nombre, c.apellido, p.descripcion as pais, c.telefono, c.fecha_alta, td.descripcion as tipo_documento, c.nro_documento, c.fecha_nacimiento
            FROM Usuario u
            INNER JOIN Cuenta c ON c.id_cuenta = u.id_cuenta
            LEFT JOIN NivelAcceso na ON na.id_nivel_acceso = u.id_nivel_acceso
            LEFT JOIN Pais p ON p.id_pais = c.id_pais
            LEFT JOIN TipoDocumento td ON td.id_tipo_documento = c.id_tipo_documento
            WHERE u.correo = @correo AND c.id_estado = 1";

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
            INSERT INTO GaritaUsuario(id_garita_usuario, id_usuario, id_garita, id_turno, fecha)
            VALUES((SELECT ISNULL(MAX(id_garita_usuario), 0) + 1 FROM GaritaUsuario), @idUsuario, @idGarita, @idTurno, GETDATE())";

            try
            {
                var CN = Conexion.AbrirConexion();

                using(var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.Parameters.AddWithValue("@idGarita", idGarita);
                    cmd.Parameters.AddWithValue("@idTurno", idTurno);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        return datosUsuario;
                    }

                    throw new Exception("No se pudo iniciar sesion debido a un problema desconocido");
                }
            } catch(Exception ex)
            {
                throw new Exception("No se pudo iniciar sesion: " + ex.Message);
            }
        }
    }
}
