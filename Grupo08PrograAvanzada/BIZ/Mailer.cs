using System;
using System.Configuration;
using System.Net.Mail;

namespace BIZ
{
    /// <summary>
    /// Envio de mails del sistema.
    ///
    /// No hay credenciales aca adentro: el host, el puerto y el remitente salen
    /// de Web.config (seccion system.net/mailSettings/smtp y la clave de appSettings
    /// "MailHabilitado"). Asi el mismo binario sirve para el LXC de Postfix del
    /// barrio y para un proveedor externo, cambiando solo configuracion.
    /// </summary>
    public class Mailer
    {
        /// <summary>
        /// Si esta en false, Enviar() no manda nada y no falla. Sirve para que la
        /// app arranque en una maquina de desarrollo sin servidor de mail.
        /// </summary>
        public static bool Habilitado
        {
            get
            {
                var valor = ConfigurationManager.AppSettings["MailHabilitado"];
                if (string.IsNullOrEmpty(valor)) return false;

                bool habilitado;
                return bool.TryParse(valor, out habilitado) && habilitado;
            }
        }

        private static string Remitente
        {
            get
            {
                var valor = ConfigurationManager.AppSettings["MailRemitente"];
                return string.IsNullOrWhiteSpace(valor) ? "no-responder@barriocerrado.local" : valor;
            }
        }

        private static string NombreRemitente
        {
            get
            {
                var valor = ConfigurationManager.AppSettings["MailNombreRemitente"];
                return string.IsNullOrWhiteSpace(valor) ? "Barrio Cerrado I" : valor;
            }
        }

        public static void Enviar(string destinatario, string asunto, string cuerpoHtml)
        {
            if (!Habilitado)
                throw new Exception("El envio de mails esta deshabilitado. Revisa la clave MailHabilitado en Web.config.");

            if (string.IsNullOrWhiteSpace(destinatario))
                throw new Exception("No hay direccion de destino para el mail.");

            try
            {
                using (var mensaje = new MailMessage())
                {
                    mensaje.From = new MailAddress(Remitente, NombreRemitente);
                    mensaje.To.Add(destinatario);
                    mensaje.Subject = asunto;
                    mensaje.Body = cuerpoHtml;
                    mensaje.IsBodyHtml = true;

                    // Sin parametros, SmtpClient toma host/puerto/credenciales
                    // de <system.net><mailSettings><smtp> en Web.config.
                    using (var cliente = new SmtpClient())
                    {
                        cliente.Send(mensaje);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo enviar el mail: " + ex.Message);
            }
        }

        /// <summary>
        /// Mail de "olvide mi contrasenia". El codigo va en el cuerpo porque el
        /// usuario lo tipea en la pagina; no mandamos un link para no tener que
        /// exponer la app a internet.
        /// </summary>
        public static void EnviarCodigoRecuperacion(string destinatario, string nombre, string codigo, int minutosDeVigencia)
        {
            string asunto = "Codigo para recuperar tu contrasenia - Barrio Cerrado I";

            string cuerpo =
                "<div style=\"font-family:Arial,Helvetica,sans-serif;font-size:15px;color:#222\">" +
                "<p>Hola " + CodificarHtml(nombre) + ",</p>" +
                "<p>Pediste recuperar la contrasenia de tu cuenta de <strong>Barrio Cerrado I</strong>. " +
                "Ingresa este codigo en la pagina para elegir una contrasenia nueva:</p>" +
                "<p style=\"font-size:30px;font-weight:bold;letter-spacing:6px;margin:24px 0\">" +
                CodificarHtml(codigo) + "</p>" +
                "<p>El codigo vence en " + minutosDeVigencia + " minutos y se puede usar una sola vez.</p>" +
                "<p style=\"color:#666\">Si no fuiste vos, ignora este mail: tu contrasenia actual sigue funcionando.</p>" +
                "</div>";

            Enviar(destinatario, asunto, cuerpo);
        }

        /// <summary>
        /// Escapado minimo para no romper el HTML del mail con un nombre raro.
        /// BIZ no referencia System.Web, asi que no usamos HttpUtility.
        /// </summary>
        private static string CodificarHtml(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;

            return texto
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}
