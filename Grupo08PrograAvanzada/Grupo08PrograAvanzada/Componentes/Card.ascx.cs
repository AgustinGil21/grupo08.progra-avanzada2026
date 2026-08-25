using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Card : System.Web.UI.UserControl
    {
        [TemplateContainer (typeof(CardContenedor))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate Contenido { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Contenido != null && ph_body.Controls.Count == 0)
            {
                var contenedor = new CardContenedor();
                Contenido.InstantiateIn(contenedor);
                ph_body.Controls.Add(contenedor);
            }
        }

        public class CardContenedor : Control, INamingContainer
        {

        }
    }
}