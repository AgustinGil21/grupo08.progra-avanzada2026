using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public enum InputType
    {
        Text,
        Email,
        Password,
        Number
    }

    public enum InputSize
    {
        Small,
        Large
    }

    public partial class Input : UserControl
    {
        public event EventHandler InputChanged;

        public string Value
        {
            get { return txt_interno.Text; }
            set { txt_interno.Text = value; }
        }

        public string Label
        {
            get { return lblLabel.Text; }
            set { lblLabel.Text = value; }
        }

        public string Placeholder
        {
            get { return txt_interno.Attributes["placeholder"] ?? string.Empty; }
            set { txt_interno.Attributes["placeholder"] = value; }
        }

        public InputType Type { get; set; } = InputType.Text;

        public InputSize Size { get; set; } = InputSize.Large;

        public bool Enabled
        {
            get { return txt_interno.Enabled; }
            set { txt_interno.Enabled = value; }
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
                SetTextMode();
            }
        }

        private void ApplyStyle()
        {
            // Clase base consistente con el CSS grupal
            string css = "input-text ";

            // Variantes por tipo
            switch (Type)
            {
                case InputType.Email:
                    css += "input-email";
                    break;
                case InputType.Password:
                    css += "input-password";
                    break;
                case InputType.Number:
                    css += "input-number";
                    break;
                default:
                    css += "input-text";
                    break;
            }

            // Variantes por tamaño
            css += Size == InputSize.Small ? " input-small" : " input-large";

            txt_interno.CssClass = css;
        }

        private void SetTextMode()
        {
            switch (Type)
            {
                case InputType.Email:
                    txt_interno.TextMode = TextBoxMode.Email;
                    break;
                case InputType.Password:
                    txt_interno.TextMode = TextBoxMode.Password;
                    break;
                case InputType.Number:
                    txt_interno.TextMode = TextBoxMode.Number;
                    break;
                default:
                    txt_interno.TextMode = TextBoxMode.SingleLine;
                    break;
            }
        }

        protected void txt_interno_TextChanged(object sender, EventArgs e)
        {
            InputChanged?.Invoke(this, e);
        }
    }
}
