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
