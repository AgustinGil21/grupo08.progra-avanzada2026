using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Estado
    {
        public static DataTable ListarEstados()
        {
            var dt = new DataTable();
            string q = @"
            SELECT e.id_estado, o.descripcion
            FROM Estado e
            INNER JOIN Objeto o ON o.id_objeto = e.id_objeto
            ORDER BY e.id_estado;";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los estados: " + ex.Message);
            }
        }
    }
}
