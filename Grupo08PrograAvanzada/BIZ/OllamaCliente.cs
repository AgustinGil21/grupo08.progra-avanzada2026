using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BIZ
{
    // Comunicacion con el servidor de Ollama. Url y modelo salen de Web.config,
    // porque cambian segun la maquina donde se levante.
    public class OllamaCliente
    {
        private const int TimeoutEstadoMs = 2000;
        private const int TimeoutChatMs = 60000;
        // Tope de tokens por respuesta, para que una lista larga no se acerque al timeout
        private const int MaximoTokensRespuesta = 400;

        private static string Url
        {
            get
            {
                var valor = ConfigurationManager.AppSettings["OllamaUrl"];
                string url = string.IsNullOrWhiteSpace(valor) ? "http://127.0.0.1:11434" : valor.Trim().TrimEnd('/');

                // Ollama escucha solo en IPv4 (127.0.0.1). Con "localhost" Windows prueba primero IPv6,
                // falla y pierde ~2 s, que es justo el timeout de EstaConectado.
                return url.Replace("://localhost", "://127.0.0.1");
            }
        }

        private static string Modelo
        {
            get
            {
                var valor = ConfigurationManager.AppSettings["OllamaModelo"];
                return string.IsNullOrWhiteSpace(valor) ? "qwen2.5:3b" : valor.Trim();
            }
        }

        // Nunca tira excepcion: es lo que decide si el chat se muestra conectado o no.
        public static bool EstaConectado()
        {
            try
            {
                Enviar("GET", "/api/version", null, TimeoutEstadoMs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Manda la conversacion a /api/chat y devuelve el mensaje que contesta el modelo.
        // Si se le pasan herramientas, el modelo puede pedir usar una en vez de contestar.
        public static JObject Chat(JArray mensajes, JArray herramientas, double temperatura)
        {
            var body = new JObject
            {
                ["model"] = Modelo,
                ["messages"] = mensajes,
                ["stream"] = false,
                // Sin razonamiento previo: el modelo solo interpreta y contesta, y responde mas rapido
                ["think"] = false,
                // Deja el modelo cargado en memoria para que la siguiente pregunta no tenga que esperarlo
                ["keep_alive"] = "30m",
                ["options"] = new JObject
                {
                    ["temperature"] = temperatura,
                    ["num_predict"] = MaximoTokensRespuesta
                }
            };

            if (herramientas != null && herramientas.Count > 0)
            {
                body["tools"] = herramientas;
            }

            JObject respuesta;

            try
            {
                respuesta = Enviar("POST", "/api/chat", body, TimeoutChatMs);
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo obtener respuesta de Ollama: " + ex.Message);
            }

            var mensaje = respuesta["message"] as JObject;

            if (mensaje == null)
                throw new Exception("Ollama devolvio una respuesta sin mensaje.");

            return mensaje;
        }

        private static JObject Enviar(string metodo, string ruta, JObject body, int timeoutMs)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url + ruta);
            request.Method = metodo;
            request.Accept = "application/json";
            request.Timeout = timeoutMs;
            request.ReadWriteTimeout = timeoutMs;
            // Ollama corre en la misma maquina o en la red local: no hace falta buscar un proxy
            request.Proxy = null;

            if (body != null)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(body.ToString());
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    return JObject.Parse(reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                    throw new Exception("Ollama tardo mas de " + (timeoutMs / 1000) + " segundos en responder.");

                // Si Ollama respondio con error (por ejemplo, modelo no descargado) se muestra su mensaje
                if (ex.Response != null)
                {
                    using (var reader = new StreamReader(ex.Response.GetResponseStream(), Encoding.UTF8))
                    {
                        int codigo = (int)((HttpWebResponse)ex.Response).StatusCode;
                        throw new Exception("Ollama respondio " + codigo + ": " + reader.ReadToEnd());
                    }
                }

                throw new Exception("No se pudo conectar con " + Url + ": " + ex.Message);
            }
        }
    }
}
