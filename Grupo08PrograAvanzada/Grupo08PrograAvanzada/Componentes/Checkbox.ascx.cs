using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Checkbox : System.Web.UI.UserControl
    {
        public event EventHandler CheckedChanged;

        public string Label
        {
            get { return lblLabel.Text; }
            set { lblLabel.Text = value; }
        }

        public bool Checked
        {
            get { return chkInterno.Checked; }
            set { chkInterno.Checked = value; }
        }

        public bool Enabled
        {
            get { return chkInterno.Enabled; }
            set { chkInterno.Enabled = value; }
        }

        public bool Required { get; set; }

        public string MensajeError
        {
            get { return litMensajeError.Text; }
            set
            {
                litMensajeError.Text = value;
                litMensajeError.Visible = !string.IsNullOrEmpty(value);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ApplyStyle();
            }
        }

        private void ApplyStyle()
        {
            chkInterno.CssClass = "checkbox-input";
            lblLabel.CssClass = "checkbox-label";
        }

        protected void chkInterno_CheckedChanged(object sender, EventArgs e)
        {
            CheckedChanged?.Invoke(this, e);
        }
    }
}
