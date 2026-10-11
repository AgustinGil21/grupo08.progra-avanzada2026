using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Preacreditado
    {
        private const int ID_ESTADO_VIGENTE = 5;
        private const int ID_ESTADO_INGRESADO = 8;

        private static readonly string[] NOMBRES_DIA =
        {
            "Domingo", "Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado"
        };

        public static DataTable BuscarPreacreditadosHoy()
        {
            var dt = new DataTable();
            string q = @"
            SELECT  pre.id_preacreditado,
                    pre.fecha_hora,
                    pre.id_persona_autorizada,
                    ca.apellido + ', ' + ca.nombre AS PersonaAutorizada,
                    ca.nro_documento,
                    ca.telefono,
                    pa.foto_referencia,
                    lo.id_lote AS Lote,
                    lo.descripcion AS LoteDescripcion,
                    ct.apellido + ', ' + ct.nombre AS PersonaDelLote,
                    lo.otros_lotes AS OtrosLotesDelTitular,
                    cat.descripcion AS Categoria,
                    fr.hora_desde,
                    fr.hora_hasta
            FROM Preacreditado pre
            INNER JOIN Persona pa    ON pa.id_persona = pre.id_persona_autorizada
            INNER JOIN Cuenta ca     ON ca.id_cuenta  = pa.id_cuenta
            INNER JOIN Persona pt    ON pt.id_persona = pre.id_persona_autorizante
            INNER JOIN Cuenta ct     ON ct.id_cuenta  = pt.id_cuenta
            INNER JOIN Categoria cat ON cat.id_categoria = pre.id_categoria
            OUTER APPLY (
                SELECT TOP 1 l.id_lote, l.descripcion,
                       (SELECT COUNT(*) - 1 FROM Lote l2 WHERE l2.id_persona_titular = pt.id_persona) AS otros_lotes
                FROM Lote l
                WHERE l.id_persona_titular = pt.id_persona
                ORDER BY l.id_lote
            ) lo
            OUTER APPLY (
                SELECT TOP 1 cd.hora_desde, cd.hora_hasta
                FROM CategoriaDia cd
                INNER JOIN Dia d ON d.id_dia = cd.id_dia
                WHERE cd.id_categoria = pre.id_categoria
                  AND d.descripcion COLLATE Latin1_General_CI_AI =
                      CASE (DATEDIFF(day, 0, pre.fecha_hora) % 7)
                           WHEN 0 THEN 'Lunes'
                           WHEN 1 THEN 'Martes'
                           WHEN 2 THEN 'Miercoles'
                           WHEN 3 THEN 'Jueves'
                           WHEN 4 THEN 'Viernes'
                           WHEN 5 THEN 'Sabado'
                           ELSE 'Domingo'
                      END
            ) fr
            WHERE CAST(pre.fecha_hora AS date) >= CAST(GETDATE() AS date)
              AND pre.id_estado = @idEstado
            ORDER BY pre.fecha_hora;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idEstado", ID_ESTADO_VIGENTE);

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron listar los preacreditados vigentes: " + ex.Message);
            }
        }

        // El SP valida la franja recien al momento del ingreso. Esto avisa antes para no
        // guardar una preacreditacion que despues va a ser rechazada seguro.
        private static void ValidarFranja(int idCategoria, DateTime fechaHora)
        {
            var dt = new DataTable();
            string q = @"
            SELECT d.descripcion AS dia, cd.hora_desde, cd.hora_hasta
            FROM CategoriaDia cd
            INNER JOIN Dia d ON d.id_dia = cd.id_dia
            WHERE cd.id_categoria = @idCategoria;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idCategoria", idCategoria);

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo comprobar la franja horaria: " + ex.Message);
            }

            // Una categoria sin ninguna franja cargada no tiene restriccion horaria.
            if (dt.Rows.Count == 0)
            {
                return;
            }

            string nombreDia = NOMBRES_DIA[(int)fechaHora.DayOfWeek];
            DataRow franja = null;

            foreach (DataRow fila in dt.Rows)
            {
                if (MismoDia(fila["dia"].ToString(), nombreDia))
                {
                    franja = fila;
                    break;
                }
            }

            if (franja == null)
            {
                throw new Exception("Esa categoria no tiene permitido el ingreso los " + nombreDia + ".");
            }

            var horaDesde = (TimeSpan)franja["hora_desde"];
            var horaHasta = (TimeSpan)franja["hora_hasta"];
            TimeSpan hora = fechaHora.TimeOfDay;

            if (hora < horaDesde || hora > horaHasta)
            {
                throw new Exception("El horario elegido esta fuera de la franja permitida para esa categoria ("
                    + horaDesde.ToString(@"hh\:mm") + " a " + horaHasta.ToString(@"hh\:mm") + ").");
            }
        }

        // Los dias de la base pueden estar cargados con tilde (Miercoles / Miercoles).
        private static bool MismoDia(string unDia, string otroDia)
        {
            return string.Compare(unDia, otroDia, CultureInfo.InvariantCulture,
                CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
        }

        private static int InsertarPreacreditado(int idPersonaAutorizada, int idPersonaAutorizante,
            int idCategoria, DateTime fechaHora)
        {
            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                DECLARE @idPreacreditado INT = (SELECT ISNULL(MAX(id_preacreditado), 0) + 1 FROM Preacreditado);
                SET IDENTITY_INSERT Preacreditado ON;
                INSERT INTO Preacreditado (id_preacreditado, fecha_hora, id_persona_autorizada,
                    id_persona_autorizante, id_categoria, id_estado)
                VALUES (@idPreacreditado, @fechaHora, @idPersonaAutorizada, @idPersonaAutorizante,
                    @idCategoria, @idEstado);
                SET IDENTITY_INSERT Preacreditado OFF;

            COMMIT TRANSACTION;

            SELECT @idPreacreditado;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@fechaHora", fechaHora);
                    cmd.Parameters.AddWithValue("@idPersonaAutorizada", idPersonaAutorizada);
                    cmd.Parameters.AddWithValue("@idPersonaAutorizante", idPersonaAutorizante);
                    cmd.Parameters.AddWithValue("@idCategoria", idCategoria);
                    cmd.Parameters.AddWithValue("@idEstado", ID_ESTADO_VIGENTE);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo crear la preacreditacion: " + ex.Message);
            }
        }

        public static int CrearPreacreditacion(int idPersonaAutorizada, int idPersonaAutorizante,
            int idCategoria, DateTime fechaHora)
        {
            ValidarFranja(idCategoria, fechaHora);

            return InsertarPreacreditado(idPersonaAutorizada, idPersonaAutorizante, idCategoria, fechaHora);
        }

        // La franja se valida antes de crear la persona: si no sirve, no queda una
        // persona cargada de mas sin su preacreditacion.
        public static int CrearPersonaYPreacreditar(string nombre, string apellido, int idPais, string telefono,
            DateTime fechaNacimiento, int idTipoDocumento, string nroDocumento, string fotoReferencia,
            int idCategoria, int idPersonaAutorizante, DateTime fechaHora)
        {
            ValidarFranja(idCategoria, fechaHora);

            int idPersona = Persona.DarDeAltaPersonaConCategorias(nombre, apellido, idPais, telefono,
                fechaNacimiento, idTipoDocumento, nroDocumento, fotoReferencia,
                new List<int> { idCategoria });

            return InsertarPreacreditado(idPersona, idPersonaAutorizante, idCategoria, fechaHora);
        }

        public static void MarcarIngresado(int idPreacreditado)
        {
            string q = @"
            UPDATE Preacreditado
            SET id_estado = @idEstado
            WHERE id_preacreditado = @idPreacreditado;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idEstado", ID_ESTADO_INGRESADO);
                    cmd.Parameters.AddWithValue("@idPreacreditado", idPreacreditado);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo marcar la preacreditacion como ingresada: " + ex.Message);
            }
        }
    }
}
