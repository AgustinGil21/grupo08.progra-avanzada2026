using BIZ;
using Grupo08PrograAvanzada.Componentes;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Administracion
{
    public partial class UsuarioAccesos : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Administrador"; }
        }

        private const int ID_NIVEL_ACCESO_SIN_ASIGNAR = 3;

        private DropdownList cbFiltroNivel;
        private DropdownList cbFiltroEstado;
        private DropdownList cbTipoDocumento;
        private DropdownList cbNacionalidad;
        private TextBox txtNumeroDocumento;
        private TextBox txtApellido;
        private TextBox txtNombres;
        private TextBox txtFechaNacimiento;
        private TextBox txtTelefono;
        private TextBox txtCorreo;
        private TextBox txtContrasena;
        private TextBox txtRepetirContrasena;
        private Label lblError;

        protected void Page_Load(object sender, EventArgs e)
        {
            BuscarControlesDeTemplates();

            if (!IsPostBack)
            {
                CargarCombosFiltro();
                CargarUsuarios(null, "", "");
            }

            CargarCombosFormularioNuevo();
        }

        private void BuscarControlesDeTemplates()
        {
            cbFiltroNivel = (DropdownList)ControlFinder.BuscarControl(this, "cbFiltroNivel");
            cbFiltroEstado = (DropdownList)ControlFinder.BuscarControl(this, "cbFiltroEstado");
            cbTipoDocumento = (DropdownList)ControlFinder.BuscarControl(this, "cbTipoDocumento");
            cbNacionalidad = (DropdownList)ControlFinder.BuscarControl(this, "cbNacionalidad");
            txtNumeroDocumento = (TextBox)ControlFinder.BuscarControl(this, "txtNumeroDocumento");
            txtApellido = (TextBox)ControlFinder.BuscarControl(this, "txtApellido");
            txtNombres = (TextBox)ControlFinder.BuscarControl(this, "txtNombres");
            txtFechaNacimiento = (TextBox)ControlFinder.BuscarControl(this, "txtFechaNacimiento");
            txtTelefono = (TextBox)ControlFinder.BuscarControl(this, "txtTelefono");
            txtCorreo = (TextBox)ControlFinder.BuscarControl(this, "txtCorreo");
            txtContrasena = (TextBox)ControlFinder.BuscarControl(this, "txtContrasena");
            txtRepetirContrasena = (TextBox)ControlFinder.BuscarControl(this, "txtRepetirContrasena");
            lblError = (Label)ControlFinder.BuscarControl(this, "lblError");
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

        private void CargarCombosFormularioNuevo()
        {
            if (cbTipoDocumento.Combo.Items.Count > 0)
                return;

            cbTipoDocumento.Combo.DataSource = Cuenta.ListarTiposDeDocumentos().Tables[0];
            cbTipoDocumento.Combo.DataTextField = "descripcion";        
            cbTipoDocumento.Combo.DataValueField = "id_tipo_documento"; 
            cbTipoDocumento.Combo.DataBind();

            cbNacionalidad.Combo.DataSource = Pais.ListarPaises().Tables[0];
            cbNacionalidad.Combo.DataTextField = "descripcion"; 
            cbNacionalidad.Combo.DataValueField = "id_pais";    
            cbNacionalidad.Combo.DataBind();
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
            var cbNivel = (System.Web.UI.WebControls.DropDownList)e.Item.FindControl("cbNivelAccesoFila");

            cbNivel.DataSource = NivelAcceso.ListarNiveles();
            cbNivel.DataTextField = "descripcion";
            cbNivel.DataValueField = "id_nivel_acceso";
            cbNivel.DataBind();

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
                var cbNivel = (System.Web.UI.WebControls.DropDownList)e.Item.FindControl("cbNivelAccesoFila");
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

        protected void btnCrearUsuario_ButtonClick(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtApellido.Text) || string.IsNullOrWhiteSpace(txtNombres.Text) ||
                string.IsNullOrWhiteSpace(txtFechaNacimiento.Text) || string.IsNullOrWhiteSpace(txtNumeroDocumento.Text))
            {
                lblError.Text = "Tipo/número de documento, apellido, nombres y fecha de nacimiento son obligatorios.";
                return;
            }

            if (txtContrasena.Text != txtRepetirContrasena.Text)
            {
                lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            try
            {
                Auth.DarDeAltaUsuario(
                    txtNombres.Text,
                    txtApellido.Text,
                    int.Parse(cbNacionalidad.Combo.SelectedValue),
                    txtTelefono.Text,
                    DateTime.Parse(txtFechaNacimiento.Text),
                    int.Parse(cbTipoDocumento.Combo.SelectedValue),
                    txtNumeroDocumento.Text,
                    txtCorreo.Text,
                    txtContrasena.Text,
                    ID_NIVEL_ACCESO_SIN_ASIGNAR
                );

                // Limpio el formulario, recargo la tabla y cierro el modal por JS —
                // en vez de Response.Redirect, para no perder los filtros ya aplicados.
                txtNumeroDocumento.Text = "";
                txtApellido.Text = "";
                txtNombres.Text = "";
                txtFechaNacimiento.Text = "";
                txtTelefono.Text = "";
                txtCorreo.Text = "";
                txtContrasena.Text = "";
                txtRepetirContrasena.Text = "";

                CargarUsuarios(buscarUsuario.Input.Text, cbFiltroNivel.Combo.SelectedValue, cbFiltroEstado.Combo.SelectedValue);

                var mdl = (Modal)ControlFinder.BuscarControl(this, "mdlNuevoUsuario");
                ClientScript.RegisterStartupScript(GetType(), "cerrarModalNuevoUsuario",
                    "cerrarModal('" + mdl.OverlayClientID + "');", true);
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo crear el usuario: " + ex.Message;
            }
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/UsuariosAccesos.aspx");
        }
    }
}