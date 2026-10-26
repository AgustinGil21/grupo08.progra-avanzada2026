using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BIZ
{
    // Consultas de solo lectura que usa el asistente IA. Cada metodo corresponde a una
    // herramienta de AsistenteHerramientas; no saben nada del modelo ni del chat.
    public class AsistenteConsultas
    {
        // hasta es exclusivo, igual que en Movimiento.ListarParaInforme
        public static void RangoDePeriodo(string periodo, out DateTime desde, out DateTime hasta)
        {
            DateTime hoy = DateTime.Today;
            hasta = hoy.AddDays(1);

            switch ((periodo ?? "").Trim().ToLowerInvariant())
            {
                case "hoy":
                    desde = hoy;
                    break;

                case "ayer":
                    desde = hoy.AddDays(-1);
                    hasta = hoy;
                    break;

                case "semana":
                    // DayOfWeek arranca en domingo (0), se lleva al lunes de esta semana
                    desde = hoy.AddDays(-(((int)hoy.DayOfWeek + 6) % 7));
                    break;

                case "mes":
                    desde = new DateTime(hoy.Year, hoy.Month, 1);
                    break;

                default:
                    desde = new DateTime(2000, 1, 1);
                    break;
            }
        }

        public static DataTable LotesConMasIngresos(DateTime desde, DateTime hasta, int top)
        {
            string q = @"
            SELECT TOP (@top)
                   lo.descripcion AS Lote,
                   ISNULL(c.apellido + ', ' + c.nombre, 'Sin titular') AS Titular,
                   COUNT(*) AS Ingresos
            FROM Movimiento m
            INNER JOIN Lote lo  ON lo.id_lote = m.id_lote
            LEFT JOIN Persona p ON p.id_persona = lo.id_persona_titular
            LEFT JOIN Cuenta c  ON c.id_cuenta = p.id_cuenta
            WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
            GROUP BY lo.id_lote, lo.descripcion, c.apellido, c.nombre
            ORDER BY COUNT(*) DESC, lo.descripcion;";

            return Consultar(q, "No se pudieron obtener los lotes con mas ingresos",
                new SqlParameter("@top", top),
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta)).Tables[0];
        }

        // La patente se compara en mayusculas y sin espacios ni guiones: "ab 123 cd" encuentra AB123CD.
        public static DataTable MovimientosDeVehiculo(string patente, DateTime desde, DateTime hasta, int top)
        {
            string q = @"
            SELECT TOP (@top)
                   m.fecha_hora_ingreso AS FechaIngreso,
                   m.fecha_hora_egreso  AS FechaEgreso,
                   ISNULL(lo.descripcion, '') AS Lote,
                   ISNULL(cc.apellido + ', ' + cc.nombre, '') AS Conductor,
                   ISNULL(cd.apellido + ', ' + cd.nombre, '') AS DuenioVehiculo,
                   ISNULL(ct.apellido + ', ' + ct.nombre, '') AS TitularLote,
                   ISNULL(cg.apellido + ', ' + cg.nombre, '') AS Guardia,
                   ISNULL(g.descripcion, '') AS Garita
            FROM Movimiento m
            INNER JOIN Vehiculo v ON v.id_vehiculo = m.id_vehiculo
            INNER JOIN Persona pc ON pc.id_persona = m.id_persona
            LEFT JOIN Cuenta cc   ON cc.id_cuenta = pc.id_cuenta
            LEFT JOIN Persona pd  ON pd.id_persona = v.id_persona
            LEFT JOIN Cuenta cd   ON cd.id_cuenta = pd.id_cuenta
            LEFT JOIN Lote lo     ON lo.id_lote = m.id_lote
            LEFT JOIN Persona pt  ON pt.id_persona = lo.id_persona_titular
            LEFT JOIN Cuenta ct   ON ct.id_cuenta = pt.id_cuenta
            LEFT JOIN Usuario u   ON u.id_usuario = m.id_usuario
            LEFT JOIN Cuenta cg   ON cg.id_cuenta = u.id_cuenta
            LEFT JOIN Garita g    ON g.id_garita = m.id_garita_ingreso
            WHERE REPLACE(REPLACE(UPPER(v.patente), ' ', ''), '-', '') = @patente
              AND m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
            ORDER BY m.fecha_hora_ingreso DESC;";

            return Consultar(q, "No se pudieron obtener los movimientos del vehiculo",
                new SqlParameter("@top", top),
                new SqlParameter("@patente", NormalizarPatente(patente)),
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta)).Tables[0];
        }

        // Si persona son solo numeros se busca por DNI; si no, por nombre o apellido.
        // Se devuelven Persona y Documento en cada fila porque "Perez" puede traer a dos personas distintas.
        public static DataTable MovimientosDePersona(string persona, DateTime desde, DateTime hasta, int top)
        {
            string q = @"
            SELECT TOP (@top)
                   m.fecha_hora_ingreso AS FechaIngreso,
                   m.fecha_hora_egreso  AS FechaEgreso,
                   c.apellido + ', ' + c.nombre AS Persona,
                   c.nro_documento AS Documento,
                   ISNULL(lo.descripcion, '') AS Lote,
                   ISNULL(g.descripcion, '') AS Garita,
                   ISNULL(v.patente, '') AS Patente
            FROM Movimiento m
            INNER JOIN Persona p ON p.id_persona = m.id_persona
            INNER JOIN Cuenta c  ON c.id_cuenta = p.id_cuenta
            LEFT JOIN Lote lo    ON lo.id_lote = m.id_lote
            LEFT JOIN Garita g   ON g.id_garita = m.id_garita_ingreso
            LEFT JOIN Vehiculo v ON v.id_vehiculo = m.id_vehiculo
            WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
              AND ((@esDocumento = 1 AND c.nro_documento = @persona)
                OR (@esDocumento = 0
                    AND ((c.nombre + ' ' + c.apellido) COLLATE Latin1_General_CI_AI LIKE '%' + @persona + '%'
                      OR (c.apellido + ' ' + c.nombre) COLLATE Latin1_General_CI_AI LIKE '%' + @persona + '%')))
            ORDER BY m.fecha_hora_ingreso DESC;";

            string documento = (persona ?? "").Replace(".", "").Replace(" ", "").Trim();
            bool esDocumento = documento.Length > 0 && documento.All(char.IsDigit);

            return Consultar(q, "No se pudieron obtener los movimientos de la persona",
                new SqlParameter("@top", top),
                new SqlParameter("@persona", esDocumento ? documento : (persona ?? "").Trim()),
                new SqlParameter("@esDocumento", esDocumento),
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta)).Tables[0];
        }

        // Una persona puede tener mas de una categoria (PersonaCategoria): su ingreso cuenta en cada
        // una, asi que los porcentajes pueden sumar mas de 100%.
        public static DataTable CategoriasMasFrecuentes(DateTime desde, DateTime hasta)
        {
            string q = @"
            WITH Ingresos AS (
                SELECT m.id_persona
                FROM Movimiento m
                WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
            )
            SELECT cat.descripcion AS Categoria,
                   COUNT(*) AS Ingresos,
                   CAST(ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM Ingresos), 1) AS decimal(5,1)) AS Porcentaje
            FROM Ingresos i
            INNER JOIN PersonaCategoria pc ON pc.id_persona = i.id_persona
            INNER JOIN Categoria cat       ON cat.id_categoria = pc.id_categoria
            GROUP BY cat.descripcion
            ORDER BY COUNT(*) DESC, cat.descripcion;";

            return Consultar(q, "No se pudieron obtener las categorias mas frecuentes",
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta)).Tables[0];
        }

        public static DataTable PersonasAdentro()
        {
            string q = @"
            SELECT c.apellido + ', ' + c.nombre AS Persona,
                   c.nro_documento AS Documento,
                   ISNULL(STUFF((SELECT ', ' + cat.descripcion
                                 FROM PersonaCategoria pc
                                 INNER JOIN Categoria cat ON cat.id_categoria = pc.id_categoria
                                 WHERE pc.id_persona = p.id_persona
                                 ORDER BY cat.descripcion
                                 FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''), '') AS Categoria,
                   ISNULL(lo.descripcion, '') AS Lote,
                   m.fecha_hora_ingreso AS FechaIngreso,
                   ISNULL(v.patente, '') AS Patente,
                   CAST(DATEDIFF(MINUTE, m.fecha_hora_ingreso, SYSDATETIME()) / 60.0 AS decimal(8,1)) AS HorasAdentro
            FROM Movimiento m
            INNER JOIN Persona p ON p.id_persona = m.id_persona
            INNER JOIN Cuenta c  ON c.id_cuenta = p.id_cuenta
            LEFT JOIN Lote lo    ON lo.id_lote = m.id_lote
            LEFT JOIN Vehiculo v ON v.id_vehiculo = m.id_vehiculo
            WHERE m.fecha_hora_ingreso IS NOT NULL AND m.fecha_hora_egreso IS NULL
            ORDER BY m.fecha_hora_ingreso;";

            return Consultar(q, "No se pudieron obtener las personas que estan adentro").Tables[0];
        }

        // Acepta "9" o "Lote 9" y compara exacto contra Lote.descripcion, asi "Lote 1" no trae "Lote 101".
        public static DataTable IngresosALote(string lote, DateTime desde, DateTime hasta, int top)
        {
            string q = @"
            SELECT TOP (@top)
                   m.fecha_hora_ingreso AS FechaIngreso,
                   m.fecha_hora_egreso  AS FechaEgreso,
                   c.apellido + ', ' + c.nombre AS Persona,
                   ISNULL(STUFF((SELECT ', ' + cat.descripcion
                                 FROM PersonaCategoria pc
                                 INNER JOIN Categoria cat ON cat.id_categoria = pc.id_categoria
                                 WHERE pc.id_persona = p.id_persona
                                 ORDER BY cat.descripcion
                                 FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''), '') AS Categoria,
                   ISNULL(v.patente, '') AS Patente,
                   ISNULL(cg.apellido + ', ' + cg.nombre, '') AS Guardia
            FROM Movimiento m
            INNER JOIN Lote lo   ON lo.id_lote = m.id_lote
            INNER JOIN Persona p ON p.id_persona = m.id_persona
            INNER JOIN Cuenta c  ON c.id_cuenta = p.id_cuenta
            LEFT JOIN Vehiculo v ON v.id_vehiculo = m.id_vehiculo
            LEFT JOIN Usuario u  ON u.id_usuario = m.id_usuario
            LEFT JOIN Cuenta cg  ON cg.id_cuenta = u.id_cuenta
            WHERE lo.descripcion = @lote
              AND m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
            ORDER BY m.fecha_hora_ingreso DESC;";

            return Consultar(q, "No se pudieron obtener los ingresos al lote",
                new SqlParameter("@top", top),
                new SqlParameter("@lote", NormalizarLote(lote)),
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta)).Tables[0];
        }

        // Tables[0]: Franja, Ingresos. Tables[1]: Garita, Ingresos.
        public static DataSet HorariosYGaritasPico(DateTime desde, DateTime hasta)
        {
            string q = @"
            SELECT RIGHT('0' + CAST(h.hora AS varchar(2)), 2) + ':00-'
                   + RIGHT('0' + CAST((h.hora + 1) % 24 AS varchar(2)), 2) + ':00' AS Franja,
                   h.Ingresos
            FROM (SELECT DATEPART(HOUR, m.fecha_hora_ingreso) AS hora, COUNT(*) AS Ingresos
                  FROM Movimiento m
                  WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
                  GROUP BY DATEPART(HOUR, m.fecha_hora_ingreso)) h
            ORDER BY h.Ingresos DESC, h.hora;

            SELECT g.descripcion AS Garita, COUNT(*) AS Ingresos
            FROM Movimiento m
            INNER JOIN Garita g ON g.id_garita = m.id_garita_ingreso
            WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
            GROUP BY g.descripcion
            ORDER BY COUNT(*) DESC, g.descripcion;";

            return Consultar(q, "No se pudieron obtener los horarios y garitas pico",
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta));
        }

        public static DataRow ResumenDelPeriodo(DateTime desde, DateTime hasta)
        {
            string q = @"
            SELECT (SELECT COUNT(*) FROM Movimiento
                    WHERE fecha_hora_ingreso >= @desde AND fecha_hora_ingreso < @hasta) AS TotalIngresos,
                   (SELECT COUNT(*) FROM Movimiento
                    WHERE fecha_hora_egreso >= @desde AND fecha_hora_egreso < @hasta) AS TotalEgresos,
                   (SELECT COUNT(DISTINCT id_persona) FROM Movimiento
                    WHERE fecha_hora_ingreso >= @desde AND fecha_hora_ingreso < @hasta) AS PersonasDistintas,
                   (SELECT COUNT(DISTINCT id_vehiculo) FROM Movimiento
                    WHERE fecha_hora_ingreso >= @desde AND fecha_hora_ingreso < @hasta) AS VehiculosDistintos,
                   lt.Lote AS LoteTop,
                   lt.Ingresos AS IngresosLoteTop,
                   ct.Categoria AS CategoriaTop,
                   (SELECT COUNT(*) FROM Movimiento
                    WHERE fecha_hora_ingreso IS NOT NULL AND fecha_hora_egreso IS NULL) AS AdentroAhora
            FROM (SELECT 1 AS uno) base
            OUTER APPLY (
                SELECT TOP 1 lo.descripcion AS Lote, COUNT(*) AS Ingresos
                FROM Movimiento m
                INNER JOIN Lote lo ON lo.id_lote = m.id_lote
                WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
                GROUP BY lo.descripcion
                ORDER BY COUNT(*) DESC
            ) lt
            OUTER APPLY (
                SELECT TOP 1 cat.descripcion AS Categoria
                FROM Movimiento m
                INNER JOIN PersonaCategoria pc ON pc.id_persona = m.id_persona
                INNER JOIN Categoria cat       ON cat.id_categoria = pc.id_categoria
                WHERE m.fecha_hora_ingreso >= @desde AND m.fecha_hora_ingreso < @hasta
                GROUP BY cat.descripcion
                ORDER BY COUNT(*) DESC
            ) ct;";

            return Consultar(q, "No se pudo obtener el resumen del periodo",
                new SqlParameter("@desde", desde),
                new SqlParameter("@hasta", hasta)).Tables[0].Rows[0];
        }

        public static string NormalizarPatente(string patente)
        {
            return (patente ?? "").ToUpperInvariant().Replace(" ", "").Replace("-", "").Trim();
        }

        private static string NormalizarLote(string lote)
        {
            string texto = (lote ?? "").Trim();
            return texto.Length > 0 && texto.All(char.IsDigit) ? "Lote " + texto : texto;
        }

        private static DataSet Consultar(string q, string mensajeError, params SqlParameter[] parametros)
        {
            var ds = new DataSet();

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddRange(parametros);

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(mensajeError + ": " + ex.Message);
            }
        }
    }
}
