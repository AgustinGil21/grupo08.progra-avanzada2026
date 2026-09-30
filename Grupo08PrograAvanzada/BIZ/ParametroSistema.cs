using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class ParametroSistema
    {
        public static DataRow Obtener()
        {
            var dt = new DataTable();
            string q = @"
            SELECT TOP 1 ps.id_parametro_sistema,
                   ps.nombre_barrio,
                   ps.domicilio,
                   ps.zona_horaria,
                   ps.meses_conservar_movimientos,
                   ps.meses_conservar_reportes,
                   ps.fecha_hora_modificacion,
                   u.correo_electronico AS ModificadoPor,
                   ISNULL(c.apellido + ', ' + c.nombre, u.correo_electronico) AS ModificadoPorNombre
            FROM ParametroSistema ps
            LEFT JOIN Usuario u ON u.id_usuario = ps.id_usuario_modificacion
            LEFT JOIN Cuenta c  ON c.id_cuenta  = u.id_cuenta
            ORDER BY ps.id_parametro_sistema;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                {
                    throw new Exception("No hay parametros cargados. Falta correr 02_ParametroSistema.sql.");
                }

                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron obtener los parametros del sistema: " + ex.Message);
            }
        }

        public static void Actualizar(string nombreBarrio, string domicilio, string zonaHoraria,
            int mesesConservarMovimientos, int mesesConservarReportes, int idUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreBarrio))
            {
                throw new Exception("El nombre del barrio es obligatorio");
            }

            string q = @"
            UPDATE ParametroSistema
            SET nombre_barrio               = @nombreBarrio,
                domicilio                   = @domicilio,
                zona_horaria                = @zonaHoraria,
                meses_conservar_movimientos = @mesesMovimientos,
                meses_conservar_reportes    = @mesesReportes,
                fecha_hora_modificacion     = GETDATE(),
                id_usuario_modificacion     = @idUsuario
            WHERE id_parametro_sistema = (SELECT MIN(id_parametro_sistema) FROM ParametroSistema);";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@nombreBarrio", nombreBarrio);
                    cmd.Parameters.AddWithValue("@domicilio",
                        string.IsNullOrWhiteSpace(domicilio) ? (object)DBNull.Value : domicilio);
                    cmd.Parameters.AddWithValue("@zonaHoraria", zonaHoraria);
                    cmd.Parameters.AddWithValue("@mesesMovimientos", mesesConservarMovimientos);
                    cmd.Parameters.AddWithValue("@mesesReportes", mesesConservarReportes);
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron guardar los parametros del sistema: " + ex.Message);
            }
        }
    }
}
