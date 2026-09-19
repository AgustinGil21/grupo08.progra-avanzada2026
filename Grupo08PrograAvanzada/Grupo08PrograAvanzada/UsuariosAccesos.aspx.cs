using BIZ;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Administracion
{
    public partial class UsuarioAccesos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombosFiltro();
                CargarUsuarios(null, "", "");
            }
        }

        private void CargarCombosFiltro()
        {
            cbFiltroNivel.Combo.Items.Add(new ListItem("Todos los niveles", ""));
            cbFiltroNivel.Combo.DataSource = NivelAcceso.ListarNiveles();
            cbFiltroNivel.Combo.DataTextField = "descripcion";
            cbFiltroNivel.Combo.DataValueField = "id_nivel_acceso";
            cbFiltroNivel.Combo.DataBind();

            cbFiltroEstado.Combo.Items.Add(new ListItem("Activos", "1"));
            cbFiltroEstado.Combo.Items.Add(new ListItem("Todos", ""));
        }

        private void CargarUsuarios(string dni, string idNivelAcceso, string idEstado)
        {
            DataTable usuarios = Usuario.BuscarUsuarios(dni, idNivelAcceso, idEstado);
            rptUsuarios.DataSource = usuarios;
            rptUsuarios.DataBind();
        }

        protected void rptUsuarios_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var fila = (DataRowView)e.Item.DataItem;
            var cbNivel = (DropDownList)e.Item.FindControl("cbNivelAccesoFila");

            cbNivel.DataSource = NivelAcceso.ListarNiveles();
            cbNivel.DataTextField = "descripcion";
            cbNivel.DataValueField = "id_nivel_acceso";
            cbNivel.DataBind();

            // BuscarUsuarios devuelve el NOMBRE del nivel, no el id: se preselecciona por texto.
            ListItem actual = cbNivel.Items.FindByText(fila["NivelAcceso"].ToString());
            if (actual != null)
            {
                cbNivel.ClearSelection();
                actual.Selected = true;
            }
        }

        protected void rptUsuarios_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "CambiarNivel")
                return;

            try
            {
                int idUsuario = int.Parse(e.CommandArgument.ToString());
                var cbNivel = (DropDownList)e.Item.FindControl("cbNivelAccesoFila");
                int idNivelAcceso = int.Parse(cbNivel.SelectedValue);

                Usuario.CambiarNivelAcceso(idUsuario, idNivelAcceso);
                CargarUsuarios(buscarUsuario.Input.Text, cbFiltroNivel.Combo.SelectedValue, cbFiltroEstado.Combo.SelectedValue);
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo cambiar el nivel de acceso: " + ex.Message;
            }
        }

        protected void buscarUsuario_BuscarClick(object sender, EventArgs e)
        {
            CargarUsuarios(buscarUsuario.Input.Text, cbFiltroNivel.Combo.SelectedValue, cbFiltroEstado.Combo.SelectedValue);
        }

        protected void btnMostrarNuevoUsuario_Click(object sender, EventArgs e)
        {
            pnlNuevoUsuario.Visible = true;
        }

        protected void btnVerificarDni_ButtonClick(object sender, EventArgs e)
        {
            lblMensajeVerificacion.Text = "";
            pnlDatosUsuario.Visible = false;
            hdnIdCuenta.Value = "";

            string dni = txtDniPersona.Text;
            if (string.IsNullOrWhiteSpace(dni))
            {
                lblMensajeVerificacion.Text = "Ingresá un número de documento.";
                return;
            }

            DataTable resultado = Auth.VerificarDni(dni);

            if (resultado.Rows.Count == 0)
            {
                lblMensajeVerificacion.Text = "No está en el padrón. Primero hay que darla de alta como persona.";
                return;
            }

            DataRow fila = resultado.Rows[0];
            bool tieneUsuario = fila["correo_electronico"] != DBNull.Value;

            if (tieneUsuario)
            {
                lblMensajeVerificacion.Text = string.Format(
                    "{0}, {1} ya tiene un usuario creado ({2}). Si necesitás cambiarle el nivel de acceso, hacelo desde la tabla de arriba.",
                    fila["apellido"], fila["nombre"], fila["correo_electronico"]);
                return;
            }

            hdnIdCuenta.Value = fila["id_cuenta"].ToString();
            lblMensajeVerificacion.Text = string.Format(
                "{0}, {1} está en el padrón y no tiene usuario. Se puede crear.", fila["apellido"], fila["nombre"]);
            pnlDatosUsuario.Visible = true;
        }

        protected void btnCrearUsuario_ButtonClick(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(hdnIdCuenta.Value))
            {
                lblError.Text = "Primero verificá el DNI de la persona.";
                return;
            }

            if (txtContrasena.Text != txtRepetirContrasena.Text)
            {
                lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            try
            {
                int idCuenta = int.Parse(hdnIdCuenta.Value);
                Auth.DarDeAltaUsuarioSobreCuentaExistente(idCuenta, txtCorreo.Text, txtContrasena.Text);
                Response.Redirect("~/Administracion/UsuarioAccesos.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo crear el usuario: " + ex.Message;
            }
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Administracion/UsuarioAccesos.aspx");
        }
    }
}