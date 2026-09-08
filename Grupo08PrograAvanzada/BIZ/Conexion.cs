using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Conexion
    {
        public static string ObtenerCadenaDeConexion()
        {
            return ConfigurationManager.ConnectionStrings["conexion"].ConnectionString;
        }

        public static System.Data.SqlClient.SqlConnection AbrirConexion()
        {
            var CN = new System.Data.SqlClient.SqlConnection(ObtenerCadenaDeConexion());
            CN.Open();
            return CN;
        }
    }
}
