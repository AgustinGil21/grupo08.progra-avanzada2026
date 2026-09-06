using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Sidebar : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CargaMenuPorRol();
        }

        private void CargaMenuPorRol()
        {
            string rol = Session["RolRequerido"]?.ToString();
            string nombre = Session["Usuario"]?.ToString() ?? "Usuario";

            pnlAdmin.Visible = (rol == "Administrador");
            panelGuardia.Visible = (rol == "Guardia");

            litNombre.Text = nombre;
            litIniciales.Text = ObtenerIniciales(nombre);

            if (rol == "Administrador")
            {
                litDetalle.Text = "Administrador";
                btnCerrar.Text = "Cerrar sesión";
            }
            else
            {
                litDetalle.Text = "Guardia";
                btnCerrar.Text = "Cerrar turno";
            }
        }

        private string ObtenerIniciales(string nombre)
        {
            var partes = nombre.Split(' ');
            return partes.Length >= 2
                ? $"{partes[0][0]}{partes[1][0]}".ToUpper()
                : nombre.Substring(0, Math.Min(2, nombre.Length)).ToUpper();
        }

        protected void btnCerrar_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("~/Login.aspx");
        }

    }
}