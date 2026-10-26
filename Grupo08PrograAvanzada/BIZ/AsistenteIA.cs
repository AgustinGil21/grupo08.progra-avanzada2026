using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BIZ
{
    // Une todo en dos pasos:
    //   1. El modelo lee la pregunta y elige una herramienta con sus parametros (no responde nada).
    //   2. Se ejecuta la consulta y el modelo redacta la respuesta usando solo esos datos.
    // Si la pregunta no encaja con ninguna herramienta, se devuelve un mensaje fijo: el modelo nunca
    // contesta de lo que sabe por su cuenta.
    public class AsistenteIA
    {
        private const string MensajeFueraDeTema =
            "Solo puedo responder consultas sobre los movimientos del barrio: ingresos y egresos, " +
            "personas que estan adentro, vehiculos, lotes, categorias, horarios y garitas.";

        // Nunca tira excepcion: si algo falla devuelve un mensaje de tipo error.
        public static MensajeChat Responder(List<MensajeChat> historial, string pregunta)
        {
            try
            {
                ConsultaElegida consulta = Consultar(pregunta, historial);

                if (consulta.Nombre == null)
                    return MensajeChat.DelAsistente(MensajeChat.TipoFueraDeTema, MensajeFueraDeTema);

                // La pregunta anterior solo cuenta si esta depende de ella (ver Contexto)
                MensajeChat anterior = Contexto(historial, pregunta).LastOrDefault(m => m.EsUsuario);

                AsistenteHerramientas.ResultadoHerramienta resultado = AsistenteHerramientas.Ejecutar(
                    consulta, pregunta, anterior == null ? null : anterior.Contenido);

                // Falta un dato o no hubo resultados: el texto ya esta armado, no hace falta el modelo
                if (!resultado.HayDatos)
                    return MensajeChat.DelAsistente(resultado.Tipo, resultado.Texto);

                string respuesta = Redactar(pregunta, consulta, resultado.Texto);

                // Si copio el formato crudo de los datos ("Campo: valor; ...") se le pide una vez mas,
                // y si lo vuelve a copiar se deja legible separando con comas
                if (respuesta.Contains("; "))
                    respuesta = Redactar(pregunta, consulta, resultado.Texto);

                if (respuesta.Contains("; "))
                    respuesta = respuesta.Replace("; ", ", ");

                if (respuesta == "")
                    throw new Exception("El modelo no devolvio ninguna respuesta.");

                return MensajeChat.DelAsistente(MensajeChat.TipoTexto, respuesta);
            }
            catch (Exception ex)
            {
                return MensajeChat.DelAsistente(MensajeChat.TipoError, "No pude responder la consulta. " + ex.Message);
            }
        }

        public static ConsultaElegida Consultar(string pregunta)
        {
            return Consultar(pregunta, null);
        }

        // Paso 1: el modelo elige la herramienta. Si Ollama no responde o tarda mas de 60 s, tira excepcion.
        public static ConsultaElegida Consultar(string pregunta, List<MensajeChat> historial)
        {
            List<MensajeChat> contexto = Contexto(historial, pregunta);
            ConsultaElegida consulta = Elegir(pregunta, contexto);

            // A veces el historial confunde al modelo chico y contesta con texto en vez de elegir una
            // herramienta. Antes de decir que esta fuera de tema, se prueba con la pregunta sola.
            if (consulta.Nombre == null && contexto.Count > 0)
                consulta = Elegir(pregunta, new List<MensajeChat>());

            return consulta;
        }

        private static ConsultaElegida Elegir(string pregunta, List<MensajeChat> historial)
        {
            var mensajes = new JArray
            {
                new JObject
                {
                    ["role"] = "system",
                    ["content"] = InstruccionesParaElegir()
                }
            };

            // El contexto va dentro del mismo mensaje y no como mensajes sueltos: al modelo chico le
            // cuesta menos entender "antes pregunte X, ahora Y" que una conversacion entera
            string contenido = pregunta;
            MensajeChat preguntaAnterior = historial.FirstOrDefault(m => m.EsUsuario);
            MensajeChat pedidoDeDato = historial.FirstOrDefault(m => m.Tipo == MensajeChat.TipoFaltaDato);

            if (preguntaAnterior != null && pedidoDeDato != null)
            {
                contenido = "Antes pregunte: \"" + preguntaAnterior.Contenido + "\". Me pediste: \""
                    + pedidoDeDato.Contenido + "\". Te respondo: \"" + pregunta + "\".";
            }
            else if (preguntaAnterior != null)
            {
                contenido = "Antes pregunte: \"" + preguntaAnterior.Contenido + "\". Ahora pregunto: \"" + pregunta + "\".";
            }

            mensajes.Add(new JObject
            {
                ["role"] = "user",
                ["content"] = contenido
            });

            JObject respuesta = OllamaCliente.Chat(mensajes, AsistenteHerramientas.Definiciones(), 0);
            JArray llamadas = respuesta["tool_calls"] as JArray;

            var consulta = new ConsultaElegida
            {
                Nombre = null,
                Parametros = new Dictionary<string, string>()
            };

            if (llamadas == null || llamadas.Count == 0)
                return consulta;

            // Se usa solo la primera: cada pregunta se responde con una herramienta
            JToken funcion = llamadas[0]["function"];
            string nombre = funcion == null ? null : ((string)funcion["name"] ?? "").Trim();

            if (!AsistenteHerramientas.Existe(nombre))
                return consulta;

            consulta.Nombre = nombre;
            consulta.Parametros = AsistenteHerramientas.LeerParametros(funcion["arguments"]);

            return consulta;
        }

        // Se llama al abrir el chat para que el modelo y las instrucciones queden cargados
        // y la primera pregunta real no tarde de mas. Si falla, no hace nada.
        public static void Calentar()
        {
            try
            {
                Consultar("Quien esta adentro del barrio ahora?");
            }
            catch
            {
            }
        }

        // Paso 2: el modelo arma la respuesta con los datos de la consulta. Se le pasa como si el
        // mismo hubiera llamado a la herramienta y esta le hubiera devuelto el resultado.
        private static string Redactar(string pregunta, ConsultaElegida consulta, string datos)
        {
            var mensajes = new JArray
            {
                new JObject
                {
                    ["role"] = "system",
                    ["content"] = InstruccionesParaResponder()
                },
                new JObject
                {
                    ["role"] = "user",
                    ["content"] = pregunta
                },
                new JObject
                {
                    ["role"] = "assistant",
                    ["content"] = "",
                    ["tool_calls"] = new JArray
                    {
                        new JObject
                        {
                            ["function"] = new JObject
                            {
                                ["name"] = consulta.Nombre,
                                ["arguments"] = JObject.FromObject(consulta.Parametros)
                            }
                        }
                    }
                },
                new JObject
                {
                    ["role"] = "tool",
                    ["tool_name"] = consulta.Nombre,
                    ["content"] = datos
                }
            };

            // Sin herramientas: en este paso solo puede contestar. Temperatura 0 para que no agregue relleno.
            JObject respuesta = OllamaCliente.Chat(mensajes, null, 0);

            return LimpiarRespuesta((string)respuesta["content"]);
        }

        // El historial solo se manda cuando la pregunta depende de la anterior: el asistente acaba de pedir
        // un dato ("Decime la patente...") o es una repregunta como "y ayer?". Si no, la pregunta va sola,
        // porque el historial confunde al modelo chico. Las respuestas redactadas nunca se mandan.
        private static List<MensajeChat> Contexto(List<MensajeChat> historial, string pregunta)
        {
            var contexto = new List<MensajeChat>();

            if (historial == null || historial.Count == 0)
                return contexto;

            MensajeChat preguntaAnterior = historial.LastOrDefault(m => m.EsUsuario);
            MensajeChat ultimo = historial[historial.Count - 1];

            if (preguntaAnterior == null)
                return contexto;

            if (ultimo.Tipo == MensajeChat.TipoFaltaDato)
            {
                contexto.Add(preguntaAnterior);
                contexto.Add(ultimo);
            }
            else if (EsRepregunta(pregunta))
            {
                contexto.Add(preguntaAnterior);
            }

            return contexto;
        }

        // "y hoy?", "y el lote 103?": corta y empieza con "y". "y que categorias entran mas?" ya es otra pregunta.
        private static bool EsRepregunta(string pregunta)
        {
            string texto = (pregunta ?? "").Trim().TrimStart((char)0xBF).Trim().ToLowerInvariant();
            int palabras = texto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            return (texto.StartsWith("y ") || texto.StartsWith("e ")) && palabras <= 4;
        }

        private static string InstruccionesParaElegir()
        {
            return "Sos el asistente del sistema de control de accesos de un barrio cerrado. "
                + "Tu unica tarea es elegir UNA herramienta para responder la pregunta del administrador y completar sus parametros.\n"
                + "Reglas:\n"
                + "- Siempre que la pregunta sea sobre ingresos, egresos, visitas, personas, vehiculos, patentes, lotes, "
                + "categorias, horarios o garitas del barrio, llama a una herramienta. No respondas con texto.\n"
                + "- Si la pregunta no tiene que ver con eso, no llames a ninguna herramienta.\n"
                + "- El parametro periodo solo puede ser hoy, ayer, semana, mes o todo. No calcules fechas. "
                + "Si el usuario no menciona un periodo, no lo completes.\n"
                + "- Copia la patente, el documento, el nombre o el lote tal como los escribio el usuario. Nunca los inventes.\n"
                + "- Si la pregunta es una continuacion de la conversacion (por ejemplo \"y ayer?\" o solo una patente), "
                + "usa los mensajes anteriores para saber que herramienta corresponde.";
        }

        private static string InstruccionesParaResponder()
        {
            return "Sos el asistente del sistema de control de accesos de un barrio cerrado. "
                + "Respondes la pregunta del administrador usando UNICAMENTE el resultado de la herramienta.\n"
                + "Reglas:\n"
                + "- Empeza con una oracion que responda directo la pregunta, con el total si corresponde.\n"
                + "- Si hay varios resultados, despues pone uno por linea empezando con \"- \", redactado en una frase corta. "
                + "Nombra todos los resultados, no solo el primero: si dice \"Cantidad de resultados: N\", "
                + "escribi exactamente N lineas que empiecen con \"- \".\n"
                + "- No copies el formato del resultado: nunca escribas \"Campo: valor\" ni el simbolo ;.\n"
                + "- Inclui todos los numeros que aparecen (cantidades, porcentajes, horas y fechas).\n"
                + "- No inventes, no supongas ni agregues datos que no esten en el resultado.\n"
                + "- No des consejos, opiniones ni explicaciones extra.\n"
                + "- Responde en espanol de Argentina. Texto plano; podes usar **negrita** para nombres o el dato principal.\n"
                + "Ejemplo de resultado:\n"
                + "Cantidad de resultados: 2.\n"
                + "- Persona: Gomez, Ana; Categoria: Visitante; Lote: Lote 7; Entro el: 10/10/2026 09:15; Patente: a pie\n"
                + "- Persona: Diaz, Luis; Categoria: Proveedor; Lote: Lote 3; Entro el: 10/10/2026 11:40; Patente: AA111BB\n"
                + "Ejemplo de respuesta:\n"
                + "Hay 2 personas adentro del barrio:\n"
                + "- **Ana Gomez** (visitante) entro a pie al Lote 7 el 10/10/2026 a las 09:15.\n"
                + "- **Luis Diaz** (proveedor) entro con el vehiculo AA111BB al Lote 3 el 10/10/2026 a las 11:40.";
        }

        // Saca el bloque <think>...</think> que a veces devuelven los modelos qwen
        private static string LimpiarRespuesta(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return "";

            return Regex.Replace(texto, @"<think>[\s\S]*?</think>", "").Trim();
        }
    }
}
