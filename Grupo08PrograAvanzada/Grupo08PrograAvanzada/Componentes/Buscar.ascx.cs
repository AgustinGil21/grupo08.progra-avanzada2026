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

        [TemplateContainer(typeof(FiltersContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate Filters { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Filters != null && ph_filtros.Controls.Count == 0)
            {
                var container = new FiltersContainer();
                Filters.InstantiateIn(container);
                ph_filtros.Controls.Add(container);
            }
        }

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

        public class FiltersContainer : PlaceHolder, INamingContainer { }
    }
}