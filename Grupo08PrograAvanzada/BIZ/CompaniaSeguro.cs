using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    [Serializable]
    public class CompaniaSeguro
    {
        public static DataTable ListarCompanias()
        {
            var dt = new DataTable();
            string q = @"SELECT id_compania_seguro, descripcion FROM CompaniaSeguro ORDER BY descripcion;";

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
                throw new Exception("No se pudieron listar las companias de seguro: " + ex.Message);
            }
        }
    }
}
