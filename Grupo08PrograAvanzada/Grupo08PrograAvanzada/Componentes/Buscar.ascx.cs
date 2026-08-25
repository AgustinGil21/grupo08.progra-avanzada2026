using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Buscar : System.Web.UI.UserControl
    {
        public string Titulo { get; set; }
        public string Placeholder { get; set; }
        public TextBox Input => txtBuscar;

        public event EventHandler BuscarClick;
        protected void Page_Load(object sender, EventArgs e)
        {
            lblTitulo.Text = Titulo;
            lblTitulo.Visible = !string.IsNullOrEmpty(Titulo);
            txtBuscar.Attributes["placeholder"] = Placeholder;

        }
        protected void btnBuscar_ButtonClick(object sender, EventArgs e)
        {
            BuscarClick?.Invoke(this, e);
        }
    }
}