using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Pais
    {
        public static DataSet ListarPaises()
        {
            var ds = new DataSet();
            string q = @"SELECT * FROM Pais";
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
                throw new Exception("Error al listar países: " + ex.Message);
            }
        }
    }
}
