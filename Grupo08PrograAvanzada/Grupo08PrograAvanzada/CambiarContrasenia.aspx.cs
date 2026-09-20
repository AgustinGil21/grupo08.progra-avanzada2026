using System;
using System.Web.UI;

namespace Grupo08PrograAvanzada
{
    // Sin RolRequerido: cualquiera logueado puede cambiar la suya.
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

            // Se cierra la sesion a proposito: obliga a entrar de nuevo con la
            // contrasenia nueva y no queda una sesion vieja dando vueltas.
            Session.Clear();
            Response.Redirect("~/Login.aspx?cambio=1");
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }
    }
}
