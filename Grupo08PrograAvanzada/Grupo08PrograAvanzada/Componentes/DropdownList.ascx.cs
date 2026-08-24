using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public enum ComboLayout { OcupaMas, EnFila }
    public partial class DropdownList : System.Web.UI.UserControl
    {
        public string Titulo { get; set; }
        public ComboLayout Layout { get; set; } = ComboLayout.OcupaMas;
        public DropDownList Combo => ddl;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblTitulo.Text = Titulo;
        }
    }
}