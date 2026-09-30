using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Modal : System.Web.UI.UserControl
    {
        [TemplateContainer(typeof(ModalContainer))]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        public ITemplate Content { get; set; }

        public string Tittle
        {
            get { return lit_tittle.Text; }
            set { lit_tittle.Text = value; }
        }

        public string OverlayClientID => overlay.ClientID;

        public string Tamanio { get; set; } = "normal";

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (Tamanio == "grande")
            {
                overlay.Attributes["class"] = "modal-overlay modal-grande";
            }

            if (Content != null && ph_body.Controls.Count == 0)
            {
                var container = new ModalContainer();
                Content.InstantiateIn(container);
                ph_body.Controls.Add(container);
            }
        }

        protected void Page_Load(object sender, EventArgs e) { }

        public class ModalContainer : Control, INamingContainer { }
    }
}