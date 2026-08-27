using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public class ItemAgregado
    {
        public string Id { get; set; }
        public string Icono { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
    }

    public class ItemEventArgs : EventArgs
    {
        public string Id { get; set; }
    }
    public partial class AgregarItems : System.Web.UI.UserControl
    {
        public string TextoBoton { get; set; } = "Agregar";

        public event EventHandler AgregarClick;
        public event EventHandler<ItemEventArgs> QuitarClick;
        protected void Page_Load(object sender, EventArgs e)
        {
            btnAgregar.TextButton = TextoBoton;
        }
        public void CargarItems(List<ItemAgregado> items)
        {
            rptItems.DataSource = items;
            rptItems.DataBind();
        }

        protected void BtnQuitar_ButtonClick(object sender, EventArgs e)
        {
            var boton = (Grupo08PrograAvanzada.Componentes.Button)sender;
            var item = (RepeaterItem)boton.NamingContainer;
            var hidId = (HiddenField)item.FindControl("hidId");

            QuitarClick?.Invoke(this, new ItemEventArgs { Id = hidId.Value });
        }

        protected void BtnAgregar_ButtonClick(object sender, EventArgs e)
        {
            AgregarClick?.Invoke(this, e);
        }
    }
}