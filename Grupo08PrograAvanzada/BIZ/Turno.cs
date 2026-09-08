using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Turno
    {
        public static DataSet ListarTurnos()
        {
            DataSet ds = new DataSet();
            string q = @"SELECT * FROM Turno";

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
                throw new Exception("Error al listar turnos: " + ex.Message);
            }
        }
    }
}
