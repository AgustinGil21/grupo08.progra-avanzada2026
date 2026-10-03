using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public enum TypeButton
    {
        Save,
        Cancel,
        Alert
    }

    public enum ButtonSize
    {
        Small,
        Large
    }
    public partial class Button : System.Web.UI.UserControl
    {
        public event EventHandler ButtonClick;

        public string TextButton
        {
            get { return lit_texto_btn.Text; }
            set { lit_texto_btn.Text = value; }
        }
        public string OnClientClick
        {
            get { return btn_interno.OnClientClick; }
            set { btn_interno.OnClientClick = value; }
        }

        public TypeButton TypeButton { get; set; } = TypeButton.Save;
        public ButtonSize ButtonSize { get; set; } = ButtonSize.Large;

        public bool Enabled
        {
            get { return btn_interno.Enabled; }
            set { btn_interno.Enabled = value; }
        }

        // >>> ARREGLO 5: "Registrar ingreso" se veía como link azul
        protected void Page_Load(object sender, EventArgs e)
        {
            // Antes: if (!IsPostBack) ApplyStyle();
            // Los botones que se crean después de un postback (los de cada card del Repeater)
            // se saltaban el estilo. Aplicarlo siempre solo vuelve a poner la misma clase CSS.
            ApplyStyle();
        }
        // <<< FIN ARREGLO 5

        private void ApplyStyle()
        {
            string css = "btn ";

            switch (TypeButton)
            {
                case TypeButton.Save:
                    css = css + "btn-save";
                    break;
                case TypeButton.Cancel:
                    css = css + "btn-cancel";
                    break;
                case TypeButton.Alert:
                    css = css + "btn-alert";
                    break;
            }

            if (ButtonSize == ButtonSize.Small)
            {
                css = css + " btn-small";
            }

            btn_interno.CssClass = css;
        }

        protected void btn_interno_Click(object sender, EventArgs e)
        {
            if (ButtonClick != null)
            {
                ButtonClick(this, e);
            }
        }
    }
}
