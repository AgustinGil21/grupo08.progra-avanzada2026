using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Garita
    {
        public static DataSet ListarGaritas()
        {
            DataSet ds = new DataSet();
            string q = @"SELECT * FROM Garita";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    using (var adapter = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar garitas: " + ex.Message);
            }
        }
    }
}
