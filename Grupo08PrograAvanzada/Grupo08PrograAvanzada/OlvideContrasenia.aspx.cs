using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    /// <summary>
    /// "Olvide mi contrasenia", en dos pasos dentro de la misma pagina:
    ///   1. Pide el correo y dispara el mail con el codigo.
    ///   2. Pide codigo + contrasenia nueva y la cambia.
    ///
    /// El paso 1 se comporta igual exista o no la cuenta: siempre pasa al paso 2
    /// con el mismo mensaje. Si la pantalla dijera "ese correo no existe",
    /// cualquiera podria usarla para averiguar quien esta registrado.
    /// </summary>
    public partial class OlvideContrasenia : System.Web.UI.Page
    {
        private string CorreoIngresado
        {
            get { return ViewState["CorreoIngresado"] as string; }
            set { ViewState["CorreoIngresado"] = value; }
        }

        private T Buscar<T>(string id) where T : Control
        {
            var control = ControlFinder.BuscarControl(this, id) as T;

            if (control == null)
                throw new InvalidOperationException("No se encontro el control '" + id + "' en OlvideContrasenia.aspx.");

            return control;
        }

        private void MostrarError(string mensaje)
        {
            var lblError = Buscar<Label>("lbl_error");
            lblError.Text = mensaje;
            lblError.Visible = !string.IsNullOrEmpty(mensaje);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btn_enviar_codigo_ButtonClick(object sender, EventArgs e)
        {
            MostrarError("");

            var correo = Buscar<Componentes.Input>("input_correo").Value;

            if (string.IsNullOrWhiteSpace(correo))
            {
                MostrarError("Ingresá tu correo.");
                return;
            }

            try
            {
                BIZ.Recuperacion.SolicitarCodigo(correo);
            }
            catch (Exception ex)
            {
                // Un fallo de SMTP o de base si se muestra: sin esto el usuario
                // esperaria un mail que nunca va a llegar.
                MostrarError(ex.Message);
                return;
            }

            CorreoIngresado = correo.Trim();

            Buscar<Label>("lbl_aviso_envio").Text =
                "Si " + Server.HtmlEncode(CorreoIngresado) + " corresponde a una cuenta registrada, " +
                "te llegó un código. Vence en " + BIZ.Recuperacion.MinutosDeVigencia + " minutos.";

            Buscar<Control>("paso_correo").Visible = false;
            Buscar<Control>("paso_codigo").Visible = true;
        }

        protected void btn_restablecer_ButtonClick(object sender, EventArgs e)
        {
            MostrarError("");

            if (string.IsNullOrEmpty(CorreoIngresado))
            {
                Buscar<Control>("paso_codigo").Visible = false;
                Buscar<Control>("paso_correo").Visible = true;
                MostrarError("Se perdió el pedido. Empezá de nuevo.");
                return;
            }

            var codigo = Buscar<Componentes.Input>("input_codigo").Value;
            var password = Buscar<Componentes.Input>("input_password").Value;
            var passwordRepetir = Buscar<Componentes.Input>("input_password_repetir").Value;

            try
            {
                BIZ.Recuperacion.Restablecer(CorreoIngresado, codigo, password, passwordRepetir);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
                return;
            }

            CorreoIngresado = null;

            Buscar<Control>("paso_codigo").Visible = false;
            Buscar<Control>("paso_listo").Visible = true;
        }
    }
}
