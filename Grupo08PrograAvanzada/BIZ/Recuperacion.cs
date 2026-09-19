using System;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace BIZ
{
    /// <summary>
    /// "Olvide mi contrasenia": el usuario pide un codigo por mail y lo usa para
    /// elegir una contrasenia nueva sin haber iniciado sesion.
    ///
    /// Requiere la tabla dbo.RecuperacionContrasenia
    /// (ver BaseDeDatos/01_RecuperacionContrasenia.sql).
    ///
    /// Cuidados que toma esta clase:
    ///   - El codigo se guarda hasheado, no en texto plano.
    ///   - Pedir un codigo nuevo invalida los anteriores de esa cuenta.
    ///   - Maximo de intentos por codigo, para que 6 digitos no se puedan
    ///     adivinar a fuerza bruta.
    ///   - SolicitarCodigo() se comporta igual exista o no la cuenta, asi la
    ///     pantalla no sirve para averiguar que correos estan registrados.
    /// </summary>
    public class Recuperacion
    {
        public const int MinutosDeVigencia = 15;
        public const int MaximoDeIntentos = 5;
        private const int DigitosDelCodigo = 6;

        /// <summary>
        /// Genera un codigo numerico de 6 digitos con el generador criptografico.
        /// Rechaza los valores del final del rango del byte para que todos los
        /// digitos salgan con la misma probabilidad.
        /// </summary>
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

        /// <summary>
        /// Datos que necesitamos para armar el mail. Null si el correo no existe.
        /// </summary>
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

        /// <summary>
        /// Genera un codigo y lo manda por mail.
        ///
        /// No avisa si el correo existe o no: si no existe, no hace nada y vuelve
        /// como si todo hubiera salido bien. La pantalla siempre muestra
        /// "si el correo esta registrado, te llega un codigo".
        /// </summary>
        public static void SolicitarCodigo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new Exception("Ingresa tu correo.");

            int idCuenta;
            string nombre;

            try
            {
                if (!BuscarDestinatario(correo.Trim(), out idCuenta, out nombre))
                    return; // cuenta inexistente: silencio deliberado
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo procesar el pedido: " + ex.Message);
            }

            var codigo = GenerarCodigo();

            GuardarCodigo(idCuenta, codigo);

            Mailer.EnviarCodigoRecuperacion(correo.Trim(), nombre, codigo, MinutosDeVigencia);
        }

        /// <summary>
        /// Busca el codigo pendiente de la cuenta y lo valida. Suma un intento
        /// antes de comparar, asi un error no se puede reintentar gratis.
        /// Devuelve el id_recuperacion si el codigo es correcto.
        /// </summary>
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

        /// <summary>
        /// Chequeo del codigo sin cambiar nada todavia. Lo usa la pantalla para
        /// pasar del paso "ingresa el codigo" al paso "elegi tu contrasenia".
        /// Ojo: consume un intento.
        /// </summary>
        public static void VerificarCodigo(string correo, string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new Exception("Ingresa el codigo que te llego por mail.");

            var idCuenta = Contrasenia.ObtenerIdCuentaPorCorreo(correo == null ? null : correo.Trim());

            if (idCuenta == null)
                throw new Exception("El codigo no es correcto.");

            ValidarYObtenerId(idCuenta.Value, codigo.Trim());
        }

        /// <summary>
        /// Paso final: valida el codigo y escribe la contrasenia nueva.
        /// El codigo queda quemado aunque el usuario vuelva atras.
        /// </summary>
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
