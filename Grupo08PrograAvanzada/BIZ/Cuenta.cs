using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Cuenta
    {
        public static DataSet ListarTiposDeDocumentos()
        {
            var ds = new DataSet();
            string q = @"SELECT * FROM TipoDocumento";
            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar tipos de documentos: " + ex.Message);
            }
        }

        public static DataTable BuscarCuentas(string texto)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  c.id_cuenta,
                    c.nombre + ' ' + c.apellido AS Persona,
                    c.nombre,
                    c.apellido,
                    td.descripcion AS TipoDocumento,
                    c.nro_documento,
                    c.telefono,
                    o.descripcion AS Estado
            FROM Cuenta c
            LEFT JOIN TipoDocumento td ON td.id_tipo_documento = c.id_tipo_documento
            LEFT JOIN Estado e ON e.id_estado = c.id_estado
            LEFT JOIN Objeto o ON o.id_objeto = e.id_objeto
            WHERE (@texto = ''
                   OR c.nombre LIKE '%' + @texto + '%'
                   OR c.apellido LIKE '%' + @texto + '%'
                   OR c.nro_documento LIKE '%' + @texto + '%')
            ORDER BY c.apellido, c.nombre;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@texto", texto ?? "");

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron buscar las cuentas: " + ex.Message);
            }
        }
    }
}
