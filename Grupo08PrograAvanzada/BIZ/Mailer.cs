using System;
using System.Configuration;
using System.Net.Mail;

namespace BIZ
{
    // Host, puerto y remitente salen de Web.config. No hay credenciales aca.
    public class Mailer
    {
        // Ponerlo en true recien cuando el relay de correo este levantado.
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

                    // Sin parametros toma host y puerto de Web.config.
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

        // BIZ no referencia System.Web, por eso no usamos HttpUtility.
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
