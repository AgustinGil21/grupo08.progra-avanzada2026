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
        [TemplateContainer (typeof(CardContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate Content { get; set; }

        public string Tittle
        {
            get { return lit_tittle.Text; }
            set 
            { 
                lit_tittle.Text = value; 
                div_titulo.Visible = !string.IsNullOrEmpty(value);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Content != null && ph_body.Controls.Count == 0)
            {
                var container = new CardContainer();
                Content.InstantiateIn(container);
                ph_body.Controls.Add(container);
            }
        }

        public class CardContainer : Control, INamingContainer
        {

        }
    }
}