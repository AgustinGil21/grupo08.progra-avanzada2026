using System;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class AsistenteIA : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Solo lo ve el admin. El handler vuelve a validar la sesion en cada pedido.
            Visible = Session["RolRequerido"]?.ToString() == "Administrador";
        }
    }
}
