using System;
using System.Collections.Generic;
using System.Linq;

namespace BIZ
{
    // Historial del chat de un administrador. La pagina la guarda en Session y por cada
    // pregunta llama a Enviar, que agrega la pregunta y la respuesta al historial.
    [Serializable]
    public class ConversacionAsistente
    {
        public const int LargoMaximoPregunta = 300;

        public List<MensajeChat> Mensajes { get; private set; }

        public ConversacionAsistente()
        {
            Mensajes = new List<MensajeChat>();
        }

        public static void ValidarPregunta(string pregunta)
        {
            if (string.IsNullOrWhiteSpace(pregunta))
                throw new Exception("Escribi una pregunta.");

            if (pregunta.Trim().Length > LargoMaximoPregunta)
                throw new Exception("La pregunta no puede tener mas de " + LargoMaximoPregunta + " caracteres.");
        }

        // Valida la pregunta (tira excepcion si no sirve) y devuelve la respuesta del asistente.
        // Si el asistente falla, no tira excepcion: devuelve un mensaje de tipo error.
        public MensajeChat Enviar(string pregunta)
        {
            ValidarPregunta(pregunta);

            string limpia = pregunta.Trim();

            // El historial que ve el modelo es el anterior a esta pregunta
            List<MensajeChat> anteriores = Mensajes.ToList();

            Mensajes.Add(MensajeChat.DelUsuario(limpia));

            MensajeChat respuesta = AsistenteIA.Responder(anteriores, limpia);
            Mensajes.Add(respuesta);

            return respuesta;
        }

        // Para el boton Reintentar: saca la respuesta de error y vuelve a mandar la ultima pregunta.
        public MensajeChat Reintentar()
        {
            int indicePregunta = Mensajes.FindLastIndex(m => m.EsUsuario);

            if (indicePregunta < 0)
                throw new Exception("No hay ninguna pregunta para reintentar.");

            string pregunta = Mensajes[indicePregunta].Contenido;
            List<MensajeChat> anteriores = Mensajes.Take(indicePregunta).ToList();

            Mensajes.RemoveRange(indicePregunta + 1, Mensajes.Count - indicePregunta - 1);

            MensajeChat respuesta = AsistenteIA.Responder(anteriores, pregunta);
            Mensajes.Add(respuesta);

            return respuesta;
        }

        public void Limpiar()
        {
            Mensajes.Clear();
        }
    }
}
