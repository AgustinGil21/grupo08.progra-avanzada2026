using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    [Serializable]
    public class Usuario
    {
        public static DataTable BuscarUsuarios(string texto, string idNivelAcceso, string idEstado)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  u.id_usuario,
                    u.correo_electronico AS Usuario,
                    cu.nombre + ' ' + cu.apellido AS Persona,
                    cu.nro_documento,
                    na.descripcion AS NivelAcceso,
                    ISNULL(g.descripcion, 'Sin asignar') AS Garita,
                    ult.fecha AS UltimoAcceso,
                    o.descripcion AS Estado
            FROM Usuario u
            INNER JOIN Cuenta cu ON cu.id_cuenta = u.id_cuenta
            LEFT JOIN NivelAcceso na ON na.id_nivel_acceso = u.id_nivel_acceso
            LEFT JOIN Estado e ON e.id_estado = cu.id_estado
            LEFT JOIN Objeto o ON o.id_objeto = e.id_objeto
            OUTER APPLY (
                SELECT TOP 1 gu.fecha, gu.id_garita
                FROM GaritaUsuario gu
                WHERE gu.id_usuario = u.id_usuario
                ORDER BY gu.fecha DESC
            ) ult
            LEFT JOIN Garita g ON g.id_garita = ult.id_garita
            WHERE (@texto = ''
                   OR u.correo_electronico LIKE '%' + @texto + '%'
                   OR cu.nombre LIKE '%' + @texto + '%'
                   OR cu.apellido LIKE '%' + @texto + '%'
                   OR cu.nro_documento LIKE '%' + @texto + '%')
              AND (@idNivelAcceso = '' OR CAST(u.id_nivel_acceso AS varchar(10)) = @idNivelAcceso)
              AND (@idEstado = '' OR CAST(cu.id_estado AS varchar(10)) = @idEstado)
            ORDER BY cu.apellido, cu.nombre;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@texto", texto ?? "");
                    cmd.Parameters.AddWithValue("@idNivelAcceso", idNivelAcceso ?? "");
                    cmd.Parameters.AddWithValue("@idEstado", idEstado ?? "");

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron buscar los usuarios: " + ex.Message);
            }
        }

        public static void CambiarNivelAcceso(int idUsuario, int idNivelAcceso)
        {
            string q = @"UPDATE Usuario SET id_nivel_acceso = @idNivelAcceso WHERE id_usuario = @idUsuario;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idNivelAcceso", idNivelAcceso);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo cambiar el nivel de acceso: " + ex.Message);
            }
        }
    }
}
