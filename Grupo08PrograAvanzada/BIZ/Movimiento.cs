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
                   g.descripcion AS garita_ingreso,
                   v.patente
            FROM dbo.Movimiento m
            INNER JOIN dbo.Persona p ON p.id_persona = m.id_persona
            INNER JOIN dbo.Cuenta c  ON c.id_cuenta = p.id_cuenta
            LEFT JOIN dbo.Garita g   ON g.id_garita = m.id_garita_ingreso
            LEFT JOIN dbo.Vehiculo v ON v.id_vehiculo = m.id_vehiculo
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

        public static DataRow IntentarAccederSinVehiculo(string nroDocumento, int idUsuario, int idGarita)
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

        public static DataRow IntentarAccederConVehiculo(string patente, string nroDocumento, int idUsuario, int idGarita)
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

        // patente vacia o null = sale a pie
        public static DataRow RegistrarEgreso(string patente, string nroDocumento, int idUsuario, int idGarita)
        {
            var dt = new DataTable();

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand("dbo.RegistrarEgreso", CN))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@patente",
                        string.IsNullOrWhiteSpace(patente) ? (object)DBNull.Value : patente.Trim());
                    cmd.Parameters.AddWithValue("@nro_documento", nroDocumento);
                    cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@id_garita", idGarita);

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

        // Devuelve una fila por evento: un movimiento con ingreso y egreso sale dos veces.
        // Las columnas las usa la clase del PDF, no cambiarles el nombre.
        public static DataTable ListarParaInforme(DateTime? desde, DateTime? hasta, int? idTipoDocumento,
            string nroDocumento, int? idCategoria, int? idLote, int? idGarita, string tipoMovimiento)
        {
            if (tipoMovimiento != "Ingreso" && tipoMovimiento != "Egreso" && tipoMovimiento != "Ambos")
            {
                throw new Exception("El tipo de movimiento tiene que ser Ingreso, Egreso o Ambos");
            }

            var dt = new DataTable();
            const string q = @"
            SELECT m.id_movimiento,
                   ev.fecha_hora,
                   ISNULL(c.apellido + ', ' + c.nombre, '') AS Persona,
                   ISNULL(td.descripcion + ' ' + c.nro_documento, '') AS Documento,
                   ISNULL(STUFF((SELECT ', ' + cat.descripcion
                                 FROM PersonaCategoria pc
                                 INNER JOIN Categoria cat ON cat.id_categoria = pc.id_categoria
                                 WHERE pc.id_persona = p.id_persona
                                 ORDER BY cat.descripcion
                                 FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''), '') AS Categoria,
                   ISNULL(lo.descripcion, '') AS Lote,
                   ISNULL(g.descripcion, '') AS Garita,
                   ev.Movimiento
            FROM dbo.Movimiento m
            INNER JOIN dbo.Persona p      ON p.id_persona = m.id_persona
            LEFT JOIN dbo.Cuenta c        ON c.id_cuenta = p.id_cuenta
            LEFT JOIN dbo.TipoDocumento td ON td.id_tipo_documento = c.id_tipo_documento
            LEFT JOIN dbo.Lote lo         ON lo.id_lote = m.id_lote
            CROSS APPLY (VALUES (1, 'Ingreso', m.fecha_hora_ingreso, m.id_garita_ingreso),
                                (2, 'Egreso',  m.fecha_hora_egreso,  m.id_garita_egreso)
                        ) ev(orden, Movimiento, fecha_hora, id_garita)
            LEFT JOIN dbo.Garita g        ON g.id_garita = ev.id_garita
            WHERE ev.fecha_hora IS NOT NULL
              AND (@tipoMovimiento = 'Ambos' OR ev.Movimiento = @tipoMovimiento)
              AND ev.fecha_hora >= @desde
              AND ev.fecha_hora <  @hasta
              AND (@idTipoDocumento IS NULL OR c.id_tipo_documento = @idTipoDocumento)
              AND (@nroDocumento IS NULL OR c.nro_documento = @nroDocumento)
              AND (@idCategoria IS NULL OR EXISTS (SELECT 1 FROM PersonaCategoria pc
                                                   WHERE pc.id_persona = p.id_persona
                                                     AND pc.id_categoria = @idCategoria))
              AND (@idLote IS NULL OR m.id_lote = @idLote)
              AND (@idGarita IS NULL OR ev.id_garita = @idGarita)
            ORDER BY ev.fecha_hora, m.id_movimiento, ev.orden;";

            // hasta se pasa como el dia siguiente a las 00:00 para que entre el dia entero
            DateTime desdeFiltro = desde.HasValue ? desde.Value.Date : new DateTime(2000, 1, 1);
            DateTime hastaFiltro = hasta.HasValue ? hasta.Value.Date.AddDays(1) : new DateTime(2100, 1, 1);

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@tipoMovimiento", tipoMovimiento);
                    cmd.Parameters.AddWithValue("@desde", desdeFiltro);
                    cmd.Parameters.AddWithValue("@hasta", hastaFiltro);
                    cmd.Parameters.AddWithValue("@idTipoDocumento", (object)idTipoDocumento ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@nroDocumento",
                        string.IsNullOrWhiteSpace(nroDocumento) ? (object)DBNull.Value : nroDocumento.Trim());
                    cmd.Parameters.AddWithValue("@idCategoria", (object)idCategoria ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idLote", (object)idLote ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idGarita", (object)idGarita ?? DBNull.Value);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo armar el informe de movimientos: " + ex.Message);
            }
        }
    }
}