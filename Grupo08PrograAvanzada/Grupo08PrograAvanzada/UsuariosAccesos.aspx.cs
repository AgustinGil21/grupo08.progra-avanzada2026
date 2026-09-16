using BIZ;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Administracion
{
    public class UsuarioFila
    {
        public string Usuario { get; set; }
        public string NivelAcceso { get; set; }
        public string Garita { get; set; }
        public string UltimoAcceso { get; set; }
        public string Estado { get; set; }
    }

    public partial class UsuarioAccesos : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombosFiltro();
                CargarCombosFormulario();
                CargarUsuarios(null);
            }
        }

        private void CargarCombosFiltro()
        {
            cbFiltroNivel.Combo.Items.Add(new ListItem("Todos los niveles", ""));
            cbFiltroNivel.Combo.Items.Add(new ListItem("Administrador", "ADMIN"));
            cbFiltroNivel.Combo.Items.Add(new ListItem("Guardia", "GUARDIA"));

            cbFiltroGarita.Combo.Items.Add(new ListItem("Todas", ""));
            // TODO: cargar garitas reales desde la tabla GARITA

            cbFiltroEstado.Combo.Items.Add(new ListItem("Activos", "ACTIVO"));
            cbFiltroEstado.Combo.Items.Add(new ListItem("Suspendidos", "SUSPENDIDO"));
        }

        private void CargarCombosFormulario()
        {
            cbNivelAcceso.Combo.Items.Add(new ListItem("Administrador", "ADMIN"));
            cbNivelAcceso.Combo.Items.Add(new ListItem("Guardia", "GUARDIA"));

            cbGaritaAsignada.Combo.Items.Add(new ListItem("Sin asignar", ""));
            // TODO: cargar garitas reales

            cbTurno.Combo.Items.Add(new ListItem("Mañana", "MANANA"));
            cbTurno.Combo.Items.Add(new ListItem("Tarde", "TARDE"));
            cbTurno.Combo.Items.Add(new ListItem("Noche", "NOCHE"));

            cbEstadoUsuario.Combo.Items.Add(new ListItem("Activo", "ACTIVO"));
            cbEstadoUsuario.Combo.Items.Add(new ListItem("Suspendido", "SUSPENDIDO"));
        }

        private void CargarUsuarios(string filtroTexto)
        {
            // TODO: reemplazar por consulta real a USUARIO + NIVEL_ACCESO + GARITA_USUARIO
            var usuarios = new List<UsuarioFila>
            {
                new UsuarioFila { Usuario = "sferrer", NivelAcceso = "Administradora", Garita = "Todas", UltimoAcceso = "Hoy 21:10", Estado = "Activo" },
                new UsuarioFila { Usuario = "rgutierrez", NivelAcceso = "Guardia", Garita = "Garita 1", UltimoAcceso = "Hoy 22:00", Estado = "En turno" }
            };

            rptUsuarios.DataSource = usuarios;
            rptUsuarios.DataBind();
        }

        protected void buscarUsuario_BuscarClick(object sender, EventArgs e)
        {
            CargarUsuarios(buscarUsuario.Input.Text);
        }

        protected void btnVerificarDni_ButtonClick(object sender, EventArgs e)
        {
            // TODO: buscar en CUENTA por DNI y chequear si ya tiene USUARIO
            string dni = txtDniPersona.Text;

            if (string.IsNullOrWhiteSpace(dni))
            {
                lblMensajeVerificacion.Text = "Ingresá un número de documento.";
                return;
            }

            // ejemplo de resultado, reemplazar por la consulta real
            lblMensajeVerificacion.Text = "Esta persona no tiene correo asociado todavía. Se puede crear.";
        }

        protected void btnCrearUsuario_ButtonClick(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (txtContrasena.Text != txtRepetirContrasena.Text)
            {
                lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            // TODO: crear USUARIO + CONTRASENIA (hash) + GARITA_USUARIO
            // Response.Redirect("~/Administracion/UsuarioAccesos.aspx");
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Administracion/UsuarioAccesos.aspx");
        }
    }
}