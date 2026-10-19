using BIZ;
using Grupo08PrograAvanzada.Componentes;
using System;
using System.Collections.Generic;
using System.Data;
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

        private Input txtDocumento;
        private Componentes.Button btnBuscarPersona;

        // Control para la foto de la persona
        private Image imgFotoReferencia;

        private Literal litInfoIngreso;
        private Input txtNotaEgreso;
        private Componentes.Button btnConfirmarEgreso;
        private Literal litResultadoEgreso;

        private CheckBox chkConVehiculo;
        private Panel pnlPatente;
        private Input txtPatente;
        private Componentes.Button btnVerificar;

        private Literal litResultadoVerificacion;

        private DropdownList ddlLoteDestino;
        private Input txtNotaIngreso;
        private Componentes.Button btnConfirmarIngreso;

        protected void Page_Load(object sender, EventArgs e)
        {
            txtDocumento = (Input)ControlFinder.BuscarControl(this, "txtDocumento");
            btnBuscarPersona = (Componentes.Button)ControlFinder.BuscarControl(this, "btnBuscar");

            // Buscamos el control de la imagen en el HTML (cambiar el ID si en el .aspx se llama distinto)
            imgFotoReferencia = (Image)ControlFinder.BuscarControl(this, "imgFotoReferencia");

            litInfoIngreso = (Literal)ControlFinder.BuscarControl(this, "litInfoIngreso");
            txtNotaEgreso = (Input)ControlFinder.BuscarControl(this, "txtNotaEgreso");
            btnConfirmarEgreso = (Componentes.Button)ControlFinder.BuscarControl(this, "btnConfirmarEgreso");
            litResultadoEgreso = (Literal)ControlFinder.BuscarControl(this, "litResultadoEgreso");

            chkConVehiculo = (CheckBox)ControlFinder.BuscarControl(this, "chkConVehiculo");
            pnlPatente = (Panel)ControlFinder.BuscarControl(this, "pnlPatente");
            txtPatente = (Input)ControlFinder.BuscarControl(this, "txtPatente");
            btnVerificar = (Componentes.Button)ControlFinder.BuscarControl(this, "btnVerificar");

            litResultadoVerificacion = (Literal)ControlFinder.BuscarControl(this, "litResultadoVerificacion");

            ddlLoteDestino = (DropdownList)ControlFinder.BuscarControl(this, "ddlLoteDestino");
            txtNotaIngreso = (Input)ControlFinder.BuscarControl(this, "txtNotaIngreso");
            btnConfirmarIngreso = (Componentes.Button)ControlFinder.BuscarControl(this, "btnConfirmarIngreso");

            btnBuscarPersona.ButtonClick += btnBuscar_ButtonClick;
            btnConfirmarEgreso.ButtonClick += btnConfirmarEgreso_ButtonClick;
            btnVerificar.ButtonClick += btnVerificar_ButtonClick;
            btnConfirmarIngreso.ButtonClick += btnConfirmarIngreso_ButtonClick;
            chkConVehiculo.CheckedChanged += chkConVehiculo_CheckedChanged;

            if (!IsPostBack)
            {
                DataSet dsLotes = Lote.ListarLotes();
                ddlLoteDestino.Combo.DataSource = dsLotes.Tables[0];
                ddlLoteDestino.Combo.DataTextField = "descripcion";
                ddlLoteDestino.Combo.DataValueField = "id_lote";
                ddlLoteDestino.Combo.DataBind();
            }
        }

        private int ObtenerIdUsuarioActual()
        {
            return (int)Session["Id_usuario"];
        }

        private int ObtenerIdGaritaActual()
        {
            return (int)Session["IdGarita"];
        }

        protected void chkConVehiculo_CheckedChanged(object sender, EventArgs e)
        {
            pnlPatente.Visible = chkConVehiculo.Checked;
        }

        private void MostrarResultadoVerificacion(DataRow resultado)
        {
            pnlVerificacion.Visible = true;
            bool permitido = (bool)resultado["permitido"];
            string mensaje = resultado["mensaje"].ToString();

            litResultadoVerificacion.Text = permitido
                ? "<div class=\"verificacion-ok\">Acceso permitido — " + mensaje + "</div>"
                : "<div class=\"verificacion-rechazo\">Acceso rechazado — " + mensaje + "</div>";

            pnlDatosMovimiento.Visible = permitido;
        }

        protected void btnConfirmarEgreso_ButtonClick(object sender, EventArgs e)
        {
            string documento = (string)ViewState["Documento"];
            string patente = ViewState["PatenteAbierta"] as string;

            DataRow resultado = Movimiento.RegistrarEgreso(
                patente, documento, ObtenerIdUsuarioActual(), ObtenerIdGaritaActual());

            bool registrado = (bool)resultado["registrado"];
            litResultadoEgreso.Text = registrado
                ? "<div class=\"verificacion-ok\">" + resultado["mensaje"] + "</div>"
                : "<div class=\"verificacion-rechazo\">" + resultado["mensaje"] + "</div>";
        }

        protected void btnBuscar_ButtonClick(object sender, EventArgs e)
        {
            string documento = txtDocumento.Value;
            if (string.IsNullOrWhiteSpace(documento))
            {
                txtDocumento.MensajeError = "Ingresá un número de documento.";
                return;
            }

            ViewState["Documento"] = documento;

            // --- LÓGICA DE FOTO DE REFERENCIA ---
            if (imgFotoReferencia != null)
            {
                string fotoBase64 = Persona.ObtenerFotoReferencia(documento);
                if (!string.IsNullOrEmpty(fotoBase64))
                {
                    imgFotoReferencia.ImageUrl = fotoBase64;
                    imgFotoReferencia.Visible = true;
                }
                else
                {
                    // Si no tiene foto cargada, se oculta el control o podrías asignarle un avatar por defecto
                    imgFotoReferencia.Visible = false;
                }
            }
            // ------------------------------------

            DataTable abierto = Movimiento.TieneMovimientoAbierto(documento);

            pnlVerificacion.Visible = false;
            pnlDatosMovimiento.Visible = false;

            if (abierto.Rows.Count > 0)
            {
                DataRow fila = abierto.Rows[0];
                ViewState["IdMovimientoAbierto"] = fila["id_movimiento"];
                ViewState["PatenteAbierta"] = fila["patente"] != DBNull.Value ? fila["patente"].ToString() : null;

                pnlIngreso.Visible = false;
                pnlEgreso.Visible = true;

                DateTime horaIngreso = (DateTime)fila["fecha_hora_ingreso"];
                litInfoIngreso.Text = string.Format("Ingresó a las {0:HH:mm} por {1}",
                    horaIngreso, fila["garita_ingreso"]);
                return;
            }

            pnlEgreso.Visible = false;
            pnlIngreso.Visible = true;
        }

        protected void btnVerificar_ButtonClick(object sender, EventArgs e)
        {
            string documento = (string)ViewState["Documento"];
            DataRow resultado;

            if (chkConVehiculo.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtPatente.Value))
                {
                    txtPatente.MensajeError = "Ingresá la patente.";
                    return;
                }
                resultado = Movimiento.IntentarAccederConVehiculo(
                    txtPatente.Value, documento, ObtenerIdUsuarioActual(), ObtenerIdGaritaActual());
            }
            else
            {
                resultado = Movimiento.IntentarAccederSinVehiculo(
                    documento, ObtenerIdUsuarioActual(), ObtenerIdGaritaActual());
            }

            MostrarResultadoVerificacion(resultado);
        }

        protected void btnConfirmarIngreso_ButtonClick(object sender, EventArgs e)
        {
            string documento = (string)ViewState["Documento"];
            DataRow resultado;

            if (chkConVehiculo.Checked)
            {
                resultado = Movimiento.IntentarAccederConVehiculo(
                    txtPatente.Value, documento, ObtenerIdUsuarioActual(), ObtenerIdGaritaActual());
            }
            else
            {
                resultado = Movimiento.IntentarAccederSinVehiculo(
                    documento, ObtenerIdUsuarioActual(), ObtenerIdGaritaActual());
            }

            bool permitido = (bool)resultado["permitido"];
            litResultadoVerificacion.Text = permitido
                ? "<div class=\"verificacion-ok\">Ingreso registrado. " + resultado["mensaje"] + "</div>"
                : "<div class=\"verificacion-rechazo\">" + resultado["mensaje"] + "</div>";

            pnlDatosMovimiento.Visible = false;
        }
    }
}