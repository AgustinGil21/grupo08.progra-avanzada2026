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
            if (!IsPostBack && Request.QueryString["cambio"] == "1")
            {
                var lblAviso = (Label)ControlFinder.BuscarControl(this, "lbl_aviso_login");
                lblAviso.Visible = true;
                lblAviso.Text = "Tu contraseña se cambió. Iniciá sesión con la nueva.";
            }
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

            DataSet dsUsuario;

            try
            {
                dsUsuario = BIZ.Auth.IniciarSesion(user, password);
            }
            catch (Exception)
            {
                inpPassword.MensajeError = "Correo o contraseña incorrectos";
                return;
            }

            DataRow fila = dsUsuario.Tables[0].Rows[0];
            Session["Id_usuario"] = fila["id_usuario"];
            Session["Usuario"] = fila["correo_electronico"];
            Session["RolRequerido"] = fila["rol"].ToString();

            if (Session["RolRequerido"].ToString() == "Administrador")
            {
                Response.Redirect("~/UsuariosAccesos.aspx");
            }
            else if (Session["RolRequerido"].ToString() == "Guardia")
            {
                
                Session["PasswordTemporal"] = password;

                inpUsuario.Enabled = false;
                inpPassword.Enabled = false;
                btnLogin1.Visible = false;
                divGuardia.Visible = true;

                CargarGaritasYTurnos();
            }
            else
            {
                // Recien registrado: la contrasenia esta bien pero todavia no tiene
                // pantallas asignadas. Sin esto el login no hacia nada.
                Session.Clear();

                var lblErrorLogin = (Label)ControlFinder.BuscarControl(this, "lbl_error_login");
                lblErrorLogin.Visible = true;
                lblErrorLogin.Text = "Tu usuario todavia no esta habilitado. " +
                    "Un administrador del barrio te tiene que asignar un nivel de acceso.";
            }
        }

        private void CargarGaritasYTurnos()
        {
            var ddlGarita = (Componentes.DropdownList)ControlFinder.BuscarControl(this, "ddl_garita");
            var ddlTurno = (Componentes.DropdownList)ControlFinder.BuscarControl(this, "ddl_turno");

            DataSet dsGaritas = BIZ.Garita.ListarGaritas();
            ddlGarita.Combo.DataSource = dsGaritas.Tables[0];
            ddlGarita.Combo.DataTextField = "descripcion";
            ddlGarita.Combo.DataValueField = "id_garita";
            ddlGarita.Combo.DataBind();

            DataSet dsTurnos = BIZ.Turno.ListarTurnos();
            ddlTurno.Combo.DataSource = dsTurnos.Tables[0];
            ddlTurno.Combo.DataTextField = "descripcion";
            ddlTurno.Combo.DataValueField = "id_turno";
            ddlTurno.Combo.DataBind();
        }

        protected void btn_login_2_ButtonClick(object sender, EventArgs e)
        {
            var ddlGarita = (Componentes.DropdownList)ControlFinder.BuscarControl(this, "ddl_garita");
            var ddlTurno = (Componentes.DropdownList)ControlFinder.BuscarControl(this, "ddl_turno");
            var lbl_error_guardia = (Label)ControlFinder.BuscarControl(this, "lbl_error_guardia");

            string correo = Session["Usuario"].ToString();
            string password = Session["PasswordTemporal"].ToString();
            int idGarita = Convert.ToInt32(ddlGarita.Combo.SelectedValue);
            int idTurno = Convert.ToInt32(ddlTurno.Combo.SelectedValue);

            try
            {
                BIZ.Auth.IniciarSesion(correo, password, idGarita, idTurno);
            }
            catch (Exception ex)
            {
                lbl_error_guardia.Visible = true;
                lbl_error_guardia.Text = "No se pudo registrar el turno en esta sesion: " + ex.Message;
                return;
            }

            Session.Remove("PasswordTemporal");

            Session["IdGarita"] = idGarita;
            Session["Turno"] = ddlTurno.Combo.SelectedItem.Text;
            Response.Redirect("PadronPersonas.aspx");
        }
    }
}