using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class RegistrarMovimientos : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Guardia"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}