using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Grupo08PrograAvanzada
{
    public class PaginaBase : System.Web.UI.Page
    {
        protected virtual string RolRequerido 
        {
            get { return null; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (Session["RolRequerido"] == null)
            {
                Response.Redirect("~/Login.aspx");
            }
            
            if (RolRequerido != null && Session["RolRequerido"].ToString() != RolRequerido)
            {
                Response.Redirect("~/AccesoDenegado.aspx");
            }
        }
    }
}