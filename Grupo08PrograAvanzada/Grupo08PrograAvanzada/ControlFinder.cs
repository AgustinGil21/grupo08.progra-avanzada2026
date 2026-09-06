using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Grupo08PrograAvanzada
{
    public static class ControlFinder
    {
        // Busca un control por ID en todo el árbol, sin importar cuántos niveles de anidación tenga
        public static Control BuscarControl(Control raiz, string id)
        {
            if (raiz.ID == id) return raiz;

            foreach (Control hijo in raiz.Controls)
            {
                Control encontrado = BuscarControl(hijo, id);
                if (encontrado != null) return encontrado;
            }
            return null;
        }
    }
}