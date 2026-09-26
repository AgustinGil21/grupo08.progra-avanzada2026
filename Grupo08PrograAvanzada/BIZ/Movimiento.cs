using System;
using System.Data;
using System.Data.SqlClient;

namespace BIZ
{
    public class Movimiento
    {
        public static DataTable TieneMovimientoAbierto(string nroDocumento)
        {
            var dt = new DataTable();
            const string q = @"
            SELECT TOP 1
                   m.id_movimiento,
                   m.fecha_hora_ingreso,
                   m.id_garita_ingreso,
                   g.descripcion AS garita_ingreso
            FROM dbo.Movimiento m
            INNER JOIN dbo.Persona p ON p.id_persona = m.id_persona
            INNER JOIN dbo.Cuenta c  ON c.id_cuenta = p.id_cuenta
            LEFT JOIN dbo.Garita g   ON g.id_garita = m.id_garita_ingreso
            WHERE c.nro_documento = @nroDocumento
              AND m.fecha_hora_ingreso IS NOT NULL
              AND m.fecha_hora_egreso IS NULL
            ORDER BY m.fecha_hora_ingreso DESC;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@nroDocumento", nroDocumento);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo comprobar si hay un movimiento abierto: " + ex.Message);
            }
        }

        public static DataRow IntentarAccederSinVehiculo(string nroDocumento, int idUsuario, int idGarita,
            int? idLote, string nota, bool registrar)
        {
            var dt = new DataTable();

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand("dbo.IntentarAccederSinVehiculo", CN))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nro_documento", nroDocumento);
                    cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@id_garita", idGarita);
                    cmd.Parameters.AddWithValue("@id_lote", (object)idLote ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nota", (object)nota ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@registrar", registrar);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                    throw new Exception("El procedimiento no devolvio ningun resultado.");

                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo verificar el acceso: " + ex.Message);
            }
        }

        public static DataRow IntentarAccederConVehiculo(string patente, string nroDocumento, int idUsuario,
            int idGarita, int? idLote, string nota, bool registrar)
        {
            var dt = new DataTable();

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand("dbo.IntentarAccederConVehiculo", CN))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@patente", patente);
                    cmd.Parameters.AddWithValue("@nro_documento", nroDocumento);
                    cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@id_garita", idGarita);
                    cmd.Parameters.AddWithValue("@id_lote", (object)idLote ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nota", (object)nota ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@registrar", registrar);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                    throw new Exception("El procedimiento no devolvio ningun resultado.");

                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo verificar el acceso: " + ex.Message);
            }
        }

        public static DataRow RegistrarEgreso(int idUsuario, int idGarita, int idMovimiento, string nota)
        {
            var dt = new DataTable();

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand("dbo.RegistrarEgreso", CN))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@id_garita", idGarita);
                    cmd.Parameters.AddWithValue("@id_movimiento", idMovimiento);
                    cmd.Parameters.AddWithValue("@nota", (object)nota ?? DBNull.Value);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                if (dt.Rows.Count == 0)
                    throw new Exception("El procedimiento no devolvio ningun resultado.");

                return dt.Rows[0];
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo registrar el egreso: " + ex.Message);
            }
        }
    }
}
