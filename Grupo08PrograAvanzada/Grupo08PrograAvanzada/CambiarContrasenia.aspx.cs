using System;
using System.Web.UI;

namespace Grupo08PrograAvanzada
{
    /// <summary>
    /// Cambio de contrasenia con la sesion ya iniciada.
    ///
    /// Hereda de PaginaBase sin declarar RolRequerido: cualquier usuario logueado
    /// puede cambiar la suya, pero un visitante sin sesion se va al Login.
    /// </summary>
    public partial class CambiarContrasenia : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnGuardar_ButtonClick(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (Session["Id_usuario"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            int idUsuario = Convert.ToInt32(Session["Id_usuario"]);

            try
            {
                BIZ.Contrasenia.Cambiar(idUsuario, txtActual.Text, txtNueva.Text, txtNuevaRepetir.Text);
            }
            catch (Exception ex)
            {
                lblError.Text = ex.Message;
                return;
            }

            pnlFormulario.Visible = false;
            pnlListo.Visible = true;
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
