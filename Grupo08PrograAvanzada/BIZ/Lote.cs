using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Lote
    {
        private const int ID_ESTADO_ACTIVO = 1;

        public static DataSet ListarLotes()
        {
            DataSet ds = new DataSet();
            string q = @"
                SELECT lo.*,
                       ISNULL(c.apellido + ', ' + c.nombre, 'Sin titular') AS Titular,
                       obj.descripcion AS EstadoDescripcion
                FROM Lote lo
                LEFT JOIN Persona p  ON p.id_persona = lo.id_persona_titular
                LEFT JOIN Cuenta c   ON c.id_cuenta  = p.id_cuenta
                LEFT JOIN Estado e   ON e.id_estado  = lo.id_estado
                LEFT JOIN Objeto obj ON obj.id_objeto = e.id_objeto";
            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    using(var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los lotes: " + ex.Message);
            }
        }

        public static bool ExisteDescripcion(string descripcion, int? idLoteAExcluir)
        {
            string q = @"
            SELECT TOP 1 1
            FROM Lote
            WHERE descripcion = @descripcion
              AND (@idLoteAExcluir IS NULL OR id_lote <> @idLoteAExcluir);";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@idLoteAExcluir", (object)idLoteAExcluir ?? DBNull.Value);
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo comprobar si el lote ya existe: " + ex.Message);
            }
        }

        public static int DarDeAltaLote(string descripcion, decimal? latitud, decimal? longitud,
            int? idPersonaTitular)
        {
            if (ExisteDescripcion(descripcion, null))
            {
                throw new Exception("Ya existe un lote con ese numero");
            }

            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                DECLARE @idLote INT = (SELECT ISNULL(MAX(id_lote), 0) + 1 FROM Lote);
                SET IDENTITY_INSERT Lote ON;
                INSERT INTO Lote (id_lote, descripcion, latitud, longitud, id_persona_titular, id_estado)
                VALUES (@idLote, @descripcion, @latitud, @longitud, @idPersonaTitular, @idEstado);
                SET IDENTITY_INSERT Lote OFF;

            COMMIT TRANSACTION;

            SELECT @idLote;";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    var pLatitud = cmd.Parameters.Add("@latitud", System.Data.SqlDbType.Decimal);
                    pLatitud.Precision = 9;
                    pLatitud.Scale = 6;
                    pLatitud.Value = (object)latitud ?? DBNull.Value;

                    var pLongitud = cmd.Parameters.Add("@longitud", System.Data.SqlDbType.Decimal);
                    pLongitud.Precision = 9;
                    pLongitud.Scale = 6;
                    pLongitud.Value = (object)longitud ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@idPersonaTitular", (object)idPersonaTitular ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idEstado", ID_ESTADO_ACTIVO);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta el lote: " + ex.Message);
            }
        }

        public static void ActualizarLote(int idLote, string descripcion, int idEstado, decimal? latitud,
            decimal? longitud)
        {
            if (ExisteDescripcion(descripcion, idLote))
            {
                throw new Exception("Ya existe otro lote con ese numero");
            }

            string q = @"
            UPDATE Lote
            SET descripcion = @descripcion,
                id_estado   = @idEstado,
                latitud     = @latitud,
                longitud    = @longitud
            WHERE id_lote = @idLote;";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idLote", idLote);
                    cmd.Parameters.AddWithValue("@descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@idEstado", idEstado);
                    var pLatitud = cmd.Parameters.Add("@latitud", System.Data.SqlDbType.Decimal);
                    pLatitud.Precision = 9;
                    pLatitud.Scale = 6;
                    pLatitud.Value = (object)latitud ?? DBNull.Value;

                    var pLongitud = cmd.Parameters.Add("@longitud", System.Data.SqlDbType.Decimal);
                    pLongitud.Precision = 9;
                    pLongitud.Scale = 6;
                    pLongitud.Value = (object)longitud ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo actualizar el lote: " + ex.Message);
            }
        }

        public static void AsignarTitular(int idLote, int idPersona)
        {
            string q = @"
            UPDATE Lote
            SET id_persona_titular = @idPersona
            WHERE id_lote = @idLote;";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idLote", idLote);
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo asignar el titular del lote: " + ex.Message);
            }
        }
    }
}
