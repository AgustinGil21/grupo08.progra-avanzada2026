using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Hash
    {
        private const int Iteraciones = 100000;
        private const int TamanioSalt = 16;
        private const int TamanioHash = 32;

        public static string HashearContrasenia(string contrasenia)
        {
            if (string.IsNullOrEmpty(contrasenia))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            var salt = new byte[TamanioSalt];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(contrasenia, salt, Iteraciones, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(TamanioHash);
                return string.Format("{0}.{1}.{2}",
                    Iteraciones,
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(hash));
            }
        }

        public static bool VerificarContrasenia(string contrasenia, string almacenado)
        {
            if (string.IsNullOrEmpty(contrasenia) || string.IsNullOrEmpty(almacenado))
                return false;

            var partes = almacenado.Split('.');
            if (partes.Length != 3) return false;

            int iteraciones;
            if (!int.TryParse(partes[0], out iteraciones)) return false;

            var salt = Convert.FromBase64String(partes[1]);
            var esperado = Convert.FromBase64String(partes[2]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(contrasenia, salt, iteraciones, HashAlgorithmName.SHA256))
            {
                var calculado = pbkdf2.GetBytes(esperado.Length);
                return SonIguales(esperado, calculado);
            }
        }

        private static bool SonIguales(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            var diff = 0;
            for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

    }
}
