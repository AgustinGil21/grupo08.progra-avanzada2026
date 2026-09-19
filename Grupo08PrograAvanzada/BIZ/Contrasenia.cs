using System;
using System.Data.SqlClient;

namespace BIZ
{
    /// <summary>
    /// Politica de contrasenias y cambio de contrasenia de una cuenta.
    ///
    /// La tabla Contrasenia guarda el hash como VARBINARY con el texto del hash
    /// codificado en Unicode: es el formato que ya escribia Auth.DarDeAltaUsuario,
    /// y lo respetamos para no invalidar las cuentas que ya existen.
    /// </summary>
    public class Contrasenia
    {
        public const int LargoMinimo = 8;

        /// <summary>
        /// Reglas minimas. Tira excepcion con un mensaje que se puede mostrar
        /// tal cual en pantalla.
        /// </summary>
        public static void ValidarPolitica(string contrasenia)
        {
            if (string.IsNullOrWhiteSpace(contrasenia))
                throw new Exception("La contrasenia no puede estar vacia.");

            if (contrasenia.Length < LargoMinimo)
                throw new Exception("La contrasenia tiene que tener al menos " + LargoMinimo + " caracteres.");

            bool tieneLetra = false;
            bool tieneNumero = false;

            foreach (char c in contrasenia)
            {
                if (char.IsLetter(c)) tieneLetra = true;
                else if (char.IsDigit(c)) tieneNumero = true;
            }

            if (!tieneLetra || !tieneNumero)
                throw new Exception("La contrasenia tiene que combinar letras y numeros.");
        }

        /// <summary>
        /// Convierte el hash guardado (VARBINARY en Unicode) al string del formato
        /// iteraciones.salt.hash que entiende BIZ.Hash.
        /// </summary>
        private static string LeerHashAlmacenado(object valorDeLaColumna)
        {
            if (valorDeLaColumna == null || valorDeLaColumna == DBNull.Value)
                return null;

            var bytes = valorDeLaColumna as byte[];
            if (bytes != null)
                return System.Text.Encoding.Unicode.GetString(bytes);

            return valorDeLaColumna.ToString();
        }

        public static int ObtenerIdCuentaPorUsuario(int idUsuario)
        {
            const string q = @"SELECT id_cuenta FROM Usuario WHERE id_usuario = @idUsuario;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", idUsuario);

                    var resultado = cmd.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                        throw new Exception("El usuario no existe.");

                    return Convert.ToInt32(resultado);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo obtener la cuenta del usuario: " + ex.Message);
            }
        }

        /// <summary>
        /// Devuelve el id_cuenta asociado a un correo de usuario, o null si no
        /// hay ninguna cuenta activa con ese correo.
        /// </summary>
        public static int? ObtenerIdCuentaPorCorreo(string correo)
        {
            const string q = @"
            SELECT cu.id_cuenta
            FROM Usuario u
            INNER JOIN Cuenta cu ON cu.id_cuenta = u.id_cuenta
            WHERE u.correo_electronico = @correo AND cu.id_estado = 1;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@correo", (object)correo ?? DBNull.Value);

                    var resultado = cmd.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                        return null;

                    return Convert.ToInt32(resultado);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo buscar la cuenta: " + ex.Message);
            }
        }

        private static bool ContraseniaActualCoincide(int idCuenta, string contrasenia)
        {
            const string q = @"
            SELECT TOP 1 hash_contrasenia
            FROM Contrasenia
            WHERE id_cuenta = @idCuenta AND id_estado = 1;";

            using (var CN = Conexion.AbrirConexion())
            using (var cmd = new SqlCommand(q, CN))
            {
                cmd.Parameters.AddWithValue("@idCuenta", idCuenta);

                var almacenado = LeerHashAlmacenado(cmd.ExecuteScalar());

                return Hash.VerificarContrasenia(contrasenia, almacenado);
            }
        }

        /// <summary>
        /// Pisa el hash de la contrasenia activa de la cuenta.
        ///
        /// Actualiza la fila existente en vez de insertar una nueva y dar de baja
        /// la vieja: dar de baja requiere saber que id_estado significa "inactiva"
        /// y ese valor no esta confirmado en la base. Si mas adelante quieren
        /// historial de contrasenias, el cambio es aca y en el script de BD.
        /// </summary>
        internal static void EscribirHash(int idCuenta, string contraseniaNueva)
        {
            var hash = Hash.HashearContrasenia(contraseniaNueva);
            byte[] hashBytes = System.Text.Encoding.Unicode.GetBytes(hash);

            const string q = @"
            UPDATE Contrasenia
            SET hash_contrasenia = @hash
            WHERE id_cuenta = @idCuenta AND id_estado = 1;";

            using (var CN = Conexion.AbrirConexion())
            using (var cmd = new SqlCommand(q, CN))
            {
                cmd.Parameters.AddWithValue("@idCuenta", idCuenta);
                cmd.Parameters.Add("@hash", System.Data.SqlDbType.VarBinary).Value = hashBytes;

                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("La cuenta no tiene una contrasenia activa para reemplazar.");
            }
        }

        /// <summary>
        /// Cambio de contrasenia con sesion iniciada: pide la actual para que
        /// alguien que agarre la sesion abierta no pueda secuestrar la cuenta.
        /// </summary>
        public static void Cambiar(int idUsuario, string contraseniaActual, string contraseniaNueva, string repetirContraseniaNueva)
        {
            if (contraseniaNueva != repetirContraseniaNueva)
                throw new Exception("Las contrasenias nuevas no coinciden.");

            ValidarPolitica(contraseniaNueva);

            if (contraseniaActual == contraseniaNueva)
                throw new Exception("La contrasenia nueva tiene que ser distinta de la actual.");

            int idCuenta = ObtenerIdCuentaPorUsuario(idUsuario);

            if (!ContraseniaActualCoincide(idCuenta, contraseniaActual))
                throw new Exception("La contrasenia actual no es correcta.");

            EscribirHash(idCuenta, contraseniaNueva);
        }
    }
}
