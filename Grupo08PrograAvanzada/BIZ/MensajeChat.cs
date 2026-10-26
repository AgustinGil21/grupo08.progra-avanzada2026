using System;

namespace BIZ
{
    [Serializable]
    public class MensajeChat
    {
        public const string RolUsuario = "user";
        public const string RolAsistente = "assistant";

        // Tipos de respuesta del asistente. Cada uno tiene su propio estilo de burbuja en el chat.
        public const string TipoTexto = "texto";
        public const string TipoSinResultados = "sin_resultados";
        public const string TipoFueraDeTema = "fuera_de_tema";
        public const string TipoFaltaDato = "falta_dato";
        public const string TipoError = "error";

        // "user" o "assistant", igual que como los espera Ollama
        public string Rol { get; set; }
        public string Contenido { get; set; }
        // Solo para los mensajes del asistente; en los del usuario queda null
        public string Tipo { get; set; }
        public DateTime Hora { get; set; }

        public bool EsUsuario
        {
            get { return Rol == RolUsuario; }
        }

        public static MensajeChat DelUsuario(string contenido)
        {
            return new MensajeChat
            {
                Rol = RolUsuario,
                Contenido = contenido,
                Tipo = null,
                Hora = DateTime.Now
            };
        }

        public static MensajeChat DelAsistente(string tipo, string contenido)
        {
            return new MensajeChat
            {
                Rol = RolAsistente,
                Contenido = contenido,
                Tipo = tipo,
                Hora = DateTime.Now
            };
        }
    }
}
