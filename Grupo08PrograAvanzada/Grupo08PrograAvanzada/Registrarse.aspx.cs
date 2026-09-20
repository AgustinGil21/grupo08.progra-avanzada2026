using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class Registrarse : System.Web.UI.Page
    {
        private int? IdCuentaVerificada
        {
            get { return ViewState["IdCuentaVerificada"] as int?; }
            set { ViewState["IdCuentaVerificada"] = value; }
        }

        // Los controles estan adentro del Card, el designer no los declara.
        private T Buscar<T>(string id) where T : Control
        {
            var control = ControlFinder.BuscarControl(this, id) as T;

            if (control == null)
                throw new InvalidOperationException("No se encontro el control '" + id + "' en Registrarse.aspx.");

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

        protected void btn_verificar_ButtonClick(object sender, EventArgs e)
        {
            MostrarError("");

            var inputDocumento = Buscar<Componentes.Input>("input_documento");

            BIZ.Registro.ResultadoVerificacion cuenta;

            try
            {
                cuenta = BIZ.Registro.VerificarDocumento(inputDocumento.Value);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
                return;
            }

            IdCuentaVerificada = cuenta.IdCuenta;

            Buscar<Label>("lbl_persona").Text =
                "Te vamos a crear el usuario para " + cuenta.NombreCompleto + ".";

            Buscar<Control>("paso_documento").Visible = false;
            Buscar<Control>("paso_credenciales").Visible = true;
        }

        protected void btn_crear_ButtonClick(object sender, EventArgs e)
        {
            MostrarError("");

            if (IdCuentaVerificada == null)
            {
                // La pagina perdio el ViewState (sesion vieja, F5 raro). Volvemos al paso 1.
                Buscar<Control>("paso_credenciales").Visible = false;
                Buscar<Control>("paso_documento").Visible = true;
                MostrarError("Se perdio la verificacion del documento. Empeza de nuevo.");
                return;
            }

            var inputCorreo = Buscar<Componentes.Input>("input_correo");
            var inputPassword = Buscar<Componentes.Input>("input_password");
            var inputPasswordRepetir = Buscar<Componentes.Input>("input_password_repetir");

            try
            {
                BIZ.Registro.CrearUsuario(
                    IdCuentaVerificada.Value,
                    inputCorreo.Value,
                    inputPassword.Value,
                    inputPasswordRepetir.Value);
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
                return;
            }

            IdCuentaVerificada = null;

            Buscar<Control>("paso_credenciales").Visible = false;
            Buscar<Control>("paso_listo").Visible = true;
        }
    }
}
