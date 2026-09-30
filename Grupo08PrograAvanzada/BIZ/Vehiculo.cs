using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    [Serializable]
    public class Vehiculo
    {
        private const int ID_OBJETO_VIGENTE = 5;
        private const int ID_OBJETO_VENCIDO = 6;

        // No debe interrumpir a quien la llama: si falla, se loguea y listo.
        private static void LogError(string contexto, Exception ex)
        {
            try
            {
                string carpeta = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");
                if (!System.IO.Directory.Exists(carpeta))
                    System.IO.Directory.CreateDirectory(carpeta);

                string linea = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + contexto + " | " + ex.Message
                    + Environment.NewLine;

                System.IO.File.AppendAllText(System.IO.Path.Combine(carpeta, "errores.log"), linea);
            }
            catch
            {
                
            }
        }

        private static void ActualizarEstadoPolizaAVencido(int idVehiculoPolizaSeguro)
        {
            const string q = @"
            UPDATE VehiculoPolizaSeguro
            SET id_estado = @idEstado
            WHERE id_vehiculo_poliza_seguro = @id AND id_estado <> @idEstado;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idEstado", ID_OBJETO_VENCIDO);
                    cmd.Parameters.AddWithValue("@id", idVehiculoPolizaSeguro);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                LogError("No se pudo actualizar el estado de VehiculoPolizaSeguro (id="
                    + idVehiculoPolizaSeguro + ") a Vencido", ex);
            }
        }

        public static bool ExistePatente(string patente)
        {
            string q = @"SELECT TOP 1 1 FROM dbo.Vehiculo WHERE patente = @patente;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@patente", patente);
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo comprobar si la patente ya existe: " + ex.Message);
            }
        }

        // filtroEstadoPoliza: "" (todos), "Vigente" o "Vencido". Se calcula contra
        // la fecha actual.
        public static DataTable BuscarVehiculos(string patente, string filtroEstadoPoliza)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  v.id_vehiculo,
                    v.patente,
                    v.descripcion,
                    c.nombre + ' ' + c.apellido AS Titular,
                    ps.poliza_seguro,
                    ps.vigencia_poliza_seguro,
                    cs.descripcion AS CompaniaSeguro,
                    CASE
                        WHEN ps.vigencia_poliza_seguro IS NULL THEN 'Sin poliza'
                        WHEN ps.vigencia_poliza_seguro < CAST(GETDATE() AS date) THEN 'Vencido'
                        ELSE 'Vigente'
                    END AS EstadoPoliza,
                    ps.id_vehiculo_poliza_seguro,
                    ps.id_estado AS id_estado_poliza
            FROM Vehiculo v
            LEFT JOIN Persona p ON p.id_persona = v.id_persona
            LEFT JOIN Cuenta c  ON c.id_cuenta = p.id_cuenta
            OUTER APPLY (
                SELECT TOP 1 vp.id_vehiculo_poliza_seguro, vp.poliza_seguro, vp.vigencia_poliza_seguro,
                              vp.id_compania_seguro, vp.id_estado
                FROM VehiculoPolizaSeguro vp
                WHERE vp.id_vehiculo = v.id_vehiculo
                ORDER BY vp.vigencia_poliza_seguro DESC, vp.fecha_alta DESC
            ) ps
            LEFT JOIN CompaniaSeguro cs ON cs.id_compania_seguro = ps.id_compania_seguro
            WHERE (@patente = '' OR v.patente LIKE '%' + @patente + '%')
              AND (
                    @filtroEstadoPoliza = ''
                    OR (@filtroEstadoPoliza = 'Vigente' AND ps.vigencia_poliza_seguro >= CAST(GETDATE() AS date))
                    OR (@filtroEstadoPoliza = 'Vencido' AND ps.vigencia_poliza_seguro < CAST(GETDATE() AS date))
                  )
            ORDER BY v.patente;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@patente", patente ?? "");
                    cmd.Parameters.AddWithValue("@filtroEstadoPoliza", filtroEstadoPoliza ?? "");

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                foreach (DataRow fila in dt.Rows)
                {
                    if (fila["EstadoPoliza"].ToString() != "Vencido")
                        continue;

                    if (fila["id_estado_poliza"] != DBNull.Value
                        && Convert.ToInt32(fila["id_estado_poliza"]) == ID_OBJETO_VENCIDO)
                        continue; // ya esta bien marcada, no hace falta pisarla de nuevo

                    ActualizarEstadoPolizaAVencido(Convert.ToInt32(fila["id_vehiculo_poliza_seguro"]));
                }

                dt.Columns.Remove("id_vehiculo_poliza_seguro");
                dt.Columns.Remove("id_estado_poliza");

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron buscar los vehiculos: " + ex.Message);
            }
        }

        // A diferencia de BuscarVehiculos, no toca el estado de las polizas vencidas:
        // se llama al abrir un modal y no corresponde escribir en la base ahi.
        public static DataTable ListarVehiculosDePersona(int idPersona)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  v.id_vehiculo,
                    v.patente,
                    v.descripcion,
                    cs.descripcion AS CompaniaSeguro,
                    ps.vigencia_poliza_seguro,
                    CASE
                        WHEN ps.vigencia_poliza_seguro IS NULL THEN 'Sin poliza'
                        WHEN ps.vigencia_poliza_seguro < CAST(GETDATE() AS date) THEN 'Vencido'
                        ELSE 'Vigente'
                    END AS EstadoPoliza
            FROM Vehiculo v
            OUTER APPLY (
                SELECT TOP 1 vp.poliza_seguro, vp.vigencia_poliza_seguro, vp.id_compania_seguro
                FROM VehiculoPolizaSeguro vp
                WHERE vp.id_vehiculo = v.id_vehiculo
                ORDER BY vp.vigencia_poliza_seguro DESC, vp.fecha_alta DESC
            ) ps
            LEFT JOIN CompaniaSeguro cs ON cs.id_compania_seguro = ps.id_compania_seguro
            WHERE v.id_persona = @idPersona
            ORDER BY v.patente;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron listar los vehiculos de la persona: " + ex.Message);
            }
        }

        public static int DarDeAltaVehiculo(string patente, string descripcion, int idPersona)
        {
            if (ExistePatente(patente))
            {
                throw new Exception("Ya existe un vehiculo con esa patente");
            }

            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                DECLARE @idVehiculo INT = (SELECT ISNULL(MAX(id_vehiculo), 0) + 1 FROM Vehiculo);
                SET IDENTITY_INSERT Vehiculo ON;
                INSERT INTO Vehiculo (id_vehiculo, patente, descripcion, id_persona)
                VALUES (@idVehiculo, @patente, @descripcion, @idPersona);
                SET IDENTITY_INSERT Vehiculo OFF;

            COMMIT TRANSACTION;

            SELECT @idVehiculo;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@patente", patente);
                    cmd.Parameters.AddWithValue("@descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta el vehiculo: " + ex.Message);
            }
        }

        public static int DarDeAltaPolizaSeguro(int idVehiculo, int idCompaniaSeguro, string polizaSeguro,
            DateTime vigenciaPolizaSeguro)
        {
            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                DECLARE @idPoliza INT = (SELECT ISNULL(MAX(id_vehiculo_poliza_seguro), 0) + 1 FROM VehiculoPolizaSeguro);
                SET IDENTITY_INSERT VehiculoPolizaSeguro ON;
                INSERT INTO VehiculoPolizaSeguro (id_vehiculo_poliza_seguro, id_compania_seguro, id_vehiculo,
                    id_estado, poliza_seguro, vigencia_poliza_seguro, fecha_alta)
                VALUES (@idPoliza, @idCompaniaSeguro, @idVehiculo, @idEstado, @polizaSeguro,
                    @vigenciaPolizaSeguro, GETDATE());
                SET IDENTITY_INSERT VehiculoPolizaSeguro OFF;

            COMMIT TRANSACTION;

            SELECT @idPoliza;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idVehiculo", idVehiculo);
                    cmd.Parameters.AddWithValue("@idCompaniaSeguro", idCompaniaSeguro);
                    cmd.Parameters.AddWithValue("@polizaSeguro", polizaSeguro);
                    cmd.Parameters.AddWithValue("@vigenciaPolizaSeguro", vigenciaPolizaSeguro);
                    cmd.Parameters.AddWithValue("@idEstado", ID_OBJETO_VIGENTE);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta la poliza de seguro: " + ex.Message);
            }
        }
    }
}
