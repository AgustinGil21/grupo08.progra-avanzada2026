using System;
using System.Collections.Generic;

namespace BIZ
{
    // Lo que eligio el modelo: que herramienta usar y con que parametros.
    public class ConsultaElegida
    {
        // Ej. "movimientos_vehiculo"; null si el modelo no eligio ninguna
        public string Nombre { get; set; }
        // Ej. { "patente": "AB123CD" }, con los nombres ya limpios
        public Dictionary<string, string> Parametros { get; set; }

        public string Parametro(string clave)
        {
            string valor;
            return Parametros != null && Parametros.TryGetValue(clave, out valor) ? valor : "";
        }
    }
}
