using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public enum TipoAviso
    {
        Ingreso,
        Egreso,
        Alerta
    }
    public partial class AvisoEstado : System.Web.UI.UserControl
    {
        public string Titulo { get; set; }
        public string Detalle { get; set; }
        public string TextoBoton { get; set; } = "Cambiar";
        public TipoAviso TipoAviso { get; set; } = TipoAviso.Ingreso;

        public event EventHandler AccionClick;
        protected void Page_Load(object sender, EventArgs e, Button button)
        {
            button.TextButton = TextoBoton;
            button.TypeButton = ObtenerTypeButton();

        }
        protected void btnAccion_Click(object sender, EventArgs e)
        {
            AccionClick?.Invoke(this, e);
        }

        protected TypeButton ObtenerTypeButton()
        {
            switch (TipoAviso)
            {
                case TipoAviso.Alerta: return TypeButton.Alert;
                case TipoAviso.Ingreso:
                case TipoAviso.Egreso:
                default: return TypeButton.Cancel;
            }
        }
    }
}