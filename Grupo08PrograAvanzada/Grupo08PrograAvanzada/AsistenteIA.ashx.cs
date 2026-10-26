using BIZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace Grupo08PrograAvanzada
{
    // Lo llama el chat del asistente (Scripts/asistente-ia.js) por AJAX:
    //   GET  ?accion=estado      -> { "conectado": true }
    //   POST ?accion=preguntar   body { "pregunta": "..." } -> { "Tipo": "...", "Texto": "..." }
    //   POST ?accion=reintentar  vuelve a responder la última pregunta sin repetirla en el historial
    //
    // La sesión es de solo lectura a propósito: con sesión completa ASP.NET la bloquea mientras Ollama
    // piensa, y si el admin cambia de página se le cuelga hasta que llega la respuesta. Por eso la
    // conversación se guarda en la caché del servidor y no en Session.
    public class AsistenteIAHandler : IHttpHandler, IReadOnlySessionState
    {
        private static readonly TimeSpan DuracionConversacion = TimeSpan.FromMinutes(30);
        // Ollama deja el modelo cargado 30 minutos (keep_alive): se calienta de nuevo un poco antes
        private static readonly TimeSpan CadaCuantoCalentar = TimeSpan.FromMinutes(25);
        private static readonly object bloqueoCalentar = new object();
        private static DateTime ultimoCalentamiento = DateTime.MinValue;

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || Convert.ToString(context.Session["RolRequerido"]) != "Administrador")
            {
                Responder(context, 403, new { error = "Tu sesión venció o no tenés permiso para usar el asistente." });
                return;
            }

            string accion = context.Request.QueryString["accion"];

            switch (accion)
            {
                case "estado":
                    ResponderEstado(context);
                    break;

                case "preguntar":
                case "reintentar":
                    if (context.Request.HttpMethod != "POST")
                    {
                        Responder(context, 405, new { error = "Usá POST para preguntar." });
                        return;
                    }

                    ResponderPregunta(context, accion == "reintentar");
                    break;

                default:
                    Responder(context, 400, new { error = "Acción desconocida." });
                    break;
            }
        }

        private void ResponderEstado(HttpContext context)
        {
            bool conectado = OllamaCliente.EstaConectado();

            if (conectado)
            {
                CalentarSiHaceFalta();
            }

            Responder(context, 200, new { conectado = conectado });
        }

        private void ResponderPregunta(HttpContext context, bool reintento)
        {
            ConversacionAsistente conversacion = ObtenerConversacion(context);
            MensajeChat respuesta;

            try
            {
                // Dos pestañas del mismo admin comparten la conversación: se responde de a una
                lock (conversacion)
                {
                    respuesta = reintento
                        ? conversacion.Reintentar()
                        : conversacion.Enviar(LeerPregunta(context));
                }
            }
            catch (Exception ex)
            {
                // Enviar y Reintentar solo tiran excepción por datos de entrada inválidos (pregunta vacía,
                // de más de 300 caracteres o sin nada que reintentar). Si falla Ollama vuelve un mensaje de tipo error.
                Responder(context, 400, new { error = ex.Message });
                return;
            }

            Responder(context, 200, new { Tipo = respuesta.Tipo, Texto = respuesta.Contenido });
        }

        private static string LeerPregunta(HttpContext context)
        {
            try
            {
                string cuerpo;
                using (var lector = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    cuerpo = lector.ReadToEnd();
                }

                var datos = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(cuerpo);

                object pregunta;
                return datos != null && datos.TryGetValue("pregunta", out pregunta) ? pregunta as string : null;
            }
            catch (Exception)
            {
                // Un cuerpo que no es JSON se trata como pregunta vacía y responde 400
                return null;
            }
        }

        // Una conversación por sesión y por usuario: si en la misma pestaña cierra sesión un admin y
        // entra otro, el segundo no arrastra el contexto del primero.
        private static ConversacionAsistente ObtenerConversacion(HttpContext context)
        {
            string clave = "AsistenteIA:" + context.Session.SessionID + ":" + Convert.ToString(context.Session["Id_usuario"]);

            var conversacion = context.Cache[clave] as ConversacionAsistente;

            if (conversacion == null)
            {
                conversacion = new ConversacionAsistente();
                context.Cache.Insert(clave, conversacion, null, Cache.NoAbsoluteExpiration, DuracionConversacion);
            }

            return conversacion;
        }

        // Deja el modelo cargado para que la primera pregunta no tarde de más. Corre en segundo plano
        // para no demorar la respuesta del estado (el puntito verde).
        private static void CalentarSiHaceFalta()
        {
            lock (bloqueoCalentar)
            {
                if (DateTime.Now - ultimoCalentamiento < CadaCuantoCalentar)
                    return;

                ultimoCalentamiento = DateTime.Now;
            }

            HostingEnvironment.QueueBackgroundWorkItem(cancelacion => BIZ.AsistenteIA.Calentar());
        }

        private static void Responder(HttpContext context, int codigo, object datos)
        {
            context.Response.StatusCode = codigo;
            // Sin esto IIS cambia el cuerpo de los 4xx por su página de error y el chat no puede leer el mensaje
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.Write(new JavaScriptSerializer().Serialize(datos));
        }
    }
}
