using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class ParametrosSistema : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Administrador"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}