using System;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace BIZ
{
    // Necesita la tabla RecuperacionContrasenia (BaseDeDatos/01_RecuperacionContrasenia.sql).
    public class Recuperacion
    {
        public const int MinutosDeVigencia = 15;
        public const int MaximoDeIntentos = 5;
        private const int DigitosDelCodigo = 6;

        private static string GenerarCodigo()
        {
            var codigo = new char[DigitosDelCodigo];

            using (var rng = new RNGCryptoServiceProvider())
            {
                var buffer = new byte[1];

                for (int i = 0; i < DigitosDelCodigo; i++)
                {
                    do
                    {
                        rng.GetBytes(buffer);
                    }
                    while (buffer[0] >= 250); // 250 = 25 * 10, descarta el sesgo

                    codigo[i] = (char)('0' + (buffer[0] % 10));
                }
            }

            return new string(codigo);
        }

        private static bool BuscarDestinatario(string correo, out int idCuenta, out string nombre)
        {
            idCuenta = 0;
            nombre = null;

            const string q = @"
            SELECT cu.id_cuenta, cu.nombre
            FROM Usuario u
            INNER JOIN Cuenta cu ON cu.id_cuenta = u.id_cuenta
            WHERE u.correo_electronico = @correo AND cu.id_estado = 1;";

            using (var CN = Conexion.AbrirConexion())
            using (var cmd = new SqlCommand(q, CN))
            {
                cmd.Parameters.AddWithValue("@correo", (object)correo ?? DBNull.Value);

                using (var lector = cmd.ExecuteReader())
                {
                    if (!lector.Read())
                        return false;

                    idCuenta = Convert.ToInt32(lector["id_cuenta"]);
                    nombre = lector["nombre"] == DBNull.Value ? "" : lector["nombre"].ToString();
                    return true;
                }
            }
        }

        private static void InvalidarCodigosPendientes(SqlConnection CN, SqlTransaction tx, int idCuenta)
        {
            const string q = @"
            UPDATE RecuperacionContrasenia
            SET fecha_uso = GETDATE()
            WHERE id_cuenta = @idCuenta AND fecha_uso IS NULL;";

            using (var cmd = new SqlCommand(q, CN, tx))
            {
                cmd.Parameters.AddWithValue("@idCuenta", idCuenta);
                cmd.ExecuteNonQuery();
            }
        }

        private static void GuardarCodigo(int idCuenta, string codigo)
        {
            var codigoHash = Hash.HashearContrasenia(codigo);

            const string q = @"
            INSERT INTO RecuperacionContrasenia
                (id_cuenta, codigo_hash, fecha_solicitud, fecha_expiracion, fecha_uso, intentos)
            VALUES
                (@idCuenta, @codigoHash, GETDATE(), @fechaExpiracion, NULL, 0);";

            using (var CN = Conexion.AbrirConexion())
            using (var tx = CN.BeginTransaction())
            {
                try
                {
                    InvalidarCodigosPendientes(CN, tx, idCuenta);

                    using (var cmd = new SqlCommand(q, CN, tx))
                    {
                        cmd.Parameters.AddWithValue("@idCuenta", idCuenta);
                        cmd.Parameters.AddWithValue("@codigoHash", codigoHash);
                        cmd.Parameters.AddWithValue("@fechaExpiracion", DateTime.Now.AddMinutes(MinutosDeVigencia));
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static void SolicitarCodigo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new Exception("Ingresa tu correo.");

            int idCuenta;
            string nombre;

            try
            {
                if (!BuscarDestinatario(correo.Trim(), out idCuenta, out nombre))
                    return; // no avisamos si el correo existe o no, a proposito
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo procesar el pedido: " + ex.Message);
            }

            var codigo = GenerarCodigo();

            GuardarCodigo(idCuenta, codigo);

            Mailer.EnviarCodigoRecuperacion(correo.Trim(), nombre, codigo, MinutosDeVigencia);
        }

        // Suma el intento antes de comparar, si no se puede reintentar gratis.
        private static int ValidarYObtenerId(int idCuenta, string codigo)
        {
            const string qBuscar = @"
            SELECT TOP 1 id_recuperacion, codigo_hash, fecha_expiracion, intentos
            FROM RecuperacionContrasenia
            WHERE id_cuenta = @idCuenta AND fecha_uso IS NULL
            ORDER BY id_recuperacion DESC;";

            int idRecuperacion;
            string codigoHash;
            DateTime fechaExpiracion;
            int intentos;

            using (var CN = Conexion.AbrirConexion())
            {
                using (var cmd = new SqlCommand(qBuscar, CN))
                {
                    cmd.Parameters.AddWithValue("@idCuenta", idCuenta);

                    using (var lector = cmd.ExecuteReader())
                    {
                        if (!lector.Read())
                            throw new Exception("No hay ningun codigo pendiente. Pedi uno nuevo.");

                        idRecuperacion = Convert.ToInt32(lector["id_recuperacion"]);
                        codigoHash = lector["codigo_hash"].ToString();
                        fechaExpiracion = Convert.ToDateTime(lector["fecha_expiracion"]);
                        intentos = Convert.ToInt32(lector["intentos"]);
                    }
                }

                if (fechaExpiracion < DateTime.Now)
                    throw new Exception("El codigo vencio. Pedi uno nuevo.");

                if (intentos >= MaximoDeIntentos)
                    throw new Exception("Se agotaron los intentos para este codigo. Pedi uno nuevo.");

                using (var cmd = new SqlCommand(
                    "UPDATE RecuperacionContrasenia SET intentos = intentos + 1 WHERE id_recuperacion = @id;", CN))
                {
                    cmd.Parameters.AddWithValue("@id", idRecuperacion);
                    cmd.ExecuteNonQuery();
                }
            }

            if (!Hash.VerificarContrasenia(codigo, codigoHash))
                throw new Exception("El codigo no es correcto.");

            return idRecuperacion;
        }

        private static void MarcarComoUsado(int idRecuperacion)
        {
            const string q = @"
            UPDATE RecuperacionContrasenia
            SET fecha_uso = GETDATE()
            WHERE id_recuperacion = @id;";

            using (var CN = Conexion.AbrirConexion())
            using (var cmd = new SqlCommand(q, CN))
            {
                cmd.Parameters.AddWithValue("@id", idRecuperacion);
                cmd.ExecuteNonQuery();
            }
        }

        // Ojo que consume un intento.
        public static void VerificarCodigo(string correo, string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new Exception("Ingresa el codigo que te llego por mail.");

            var idCuenta = Contrasenia.ObtenerIdCuentaPorCorreo(correo == null ? null : correo.Trim());

            if (idCuenta == null)
                throw new Exception("El codigo no es correcto.");

            ValidarYObtenerId(idCuenta.Value, codigo.Trim());
        }

        public static void Restablecer(string correo, string codigo, string contraseniaNueva, string repetirContraseniaNueva)
        {
            if (contraseniaNueva != repetirContraseniaNueva)
                throw new Exception("Las contrasenias no coinciden.");

            Contrasenia.ValidarPolitica(contraseniaNueva);

            if (string.IsNullOrWhiteSpace(codigo))
                throw new Exception("Ingresa el codigo que te llego por mail.");

            var idCuenta = Contrasenia.ObtenerIdCuentaPorCorreo(correo == null ? null : correo.Trim());

            if (idCuenta == null)
                throw new Exception("El codigo no es correcto.");

            var idRecuperacion = ValidarYObtenerId(idCuenta.Value, codigo.Trim());

            Contrasenia.EscribirHash(idCuenta.Value, contraseniaNueva);

            MarcarComoUsado(idRecuperacion);
        }
    }
}
