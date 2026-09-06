using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class AccesoDenegado : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            if (Session["RolRequerido"].ToString() == "Administrador")
            {
                Response.Redirect("PadronPersonasAdmin.aspx");
            }
            else
            {
                Response.Redirect("PadronPersonas.aspx");
            }
        }
    }
}