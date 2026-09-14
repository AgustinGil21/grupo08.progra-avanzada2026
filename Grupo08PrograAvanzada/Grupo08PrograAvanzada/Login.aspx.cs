using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btn_login_1_ButtonClick(object sender, EventArgs e)
        {
            var inpUsuario = (Componentes.Input)ControlFinder.BuscarControl(this, "input_usuario");
            var inpPassword = (Componentes.Input)ControlFinder.BuscarControl(this, "input_password");
            var divGuardia = ControlFinder.BuscarControl(this, "login_datos_guardia");
            var btnLogin1 = (Componentes.Button)ControlFinder.BuscarControl(this, "btn_login_1");

            inpPassword.MensajeError = "";
            string user = inpUsuario.Value;
            string password = inpPassword.Value;

            DataTable dtUsuario = ValidarLoginTemporal(user, password);

            if (dtUsuario == null || dtUsuario.Rows.Count == 0)
            {
                inpPassword.MensajeError = "Usuario o contraseña incorrectos.";
                return;
            }

            DataRow fila = dtUsuario.Rows[0];
            Session["IdUsuario"] = fila["IdUsuario"];
            Session["Usuario"] = fila["Usuario"];
            Session["RolRequerido"] = fila["RolRequerido"].ToString();

            if (Session["RolRequerido"].ToString() == "Administrador")
            {
                Response.Redirect("PadronPersonasAdmin.aspx");
            }
            else if (Session["RolRequerido"].ToString() == "Guardia")
            {
                inpUsuario.Enabled = false;
                inpPassword.Enabled = false;
                btnLogin1.Visible = false;
                divGuardia.Visible = true;
            }
        }

        protected void btn_login_2_ButtonClick(object sender, EventArgs e)
        {
            var ddlGarita = (Componentes.DropdownList)ControlFinder.BuscarControl(this, "ddl_garita");
            var ddlTurno = (Componentes.DropdownList)ControlFinder.BuscarControl(this, "ddl_turno");

            Session["IdGarita"] = ddlGarita.Combo.SelectedValue;
            Session["Turno"] = ddlTurno.Combo.SelectedItem.Text;
            Response.Redirect("PadronPersonas.aspx");
        }

        private DataTable ValidarLoginTemporal(string user, string password)
        {
            var dt = new DataTable();
            dt.Columns.Add("IdUsuario");
            dt.Columns.Add("Usuario");
            dt.Columns.Add("RolRequerido");

            if (user == "rgutierrez@gmail.com" && password == "1234")
            {
                dt.Rows.Add(3, "rgutierrez@gmail.com", "Guardia");
            }
            else if (user == "sferrer@gmail.com" && password == "1234")
            {
                dt.Rows.Add(1, "sferrer@gmail.com", "Administrador");
            }

            return dt;
        }
    }
}