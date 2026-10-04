using BIZ;
using Grupo08PrograAvanzada.Componentes;
using Grupo08PrograAvanzada.Reportes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

        private static readonly string[] ZONAS_HORARIAS = { "GMT-3 - Buenos Aires" };

        private static readonly int[] OPCIONES_MESES = { 6, 12, 24, 36, 60 };

        private DropdownList cbTipoInforme;
        private RadioButtonList rblMovimiento;
        private Panel pnlFiltroPersona;
        private Panel pnlFiltroCategoria;
        private Panel pnlFiltroLote;
        private Panel pnlFiltroFechas;
        private DropdownList cbTipoDocumento;
        private TextBox txtNroDocumento;
        private DropdownList cbCategoriaInforme;
        private DropdownList cbLoteInforme;
        private TextBox txtDesde;
        private TextBox txtHasta;
        private DropdownList cbGaritaInforme;
        private Label lblErrorInforme;

        protected void Page_Load(object sender, EventArgs e)
        {
            BuscarControlesDelModal();

            cbTipoInforme.Combo.AutoPostBack = true;
            cbTipoInforme.Combo.SelectedIndexChanged += cbTipoInforme_SelectedIndexChanged;

            if (!IsPostBack)
            {
                CargarCombosFormulario();
                CargarParametros();
            }

            CargarCombosModal();
            AplicarVisibilidadFiltros();
        }

        private void BuscarControlesDelModal()
        {
            cbTipoInforme = (DropdownList)ControlFinder.BuscarControl(this, "cbTipoInforme");
            rblMovimiento = (RadioButtonList)ControlFinder.BuscarControl(this, "rblMovimiento");
            pnlFiltroPersona = (Panel)ControlFinder.BuscarControl(this, "pnlFiltroPersona");
            pnlFiltroCategoria = (Panel)ControlFinder.BuscarControl(this, "pnlFiltroCategoria");
            pnlFiltroLote = (Panel)ControlFinder.BuscarControl(this, "pnlFiltroLote");
            pnlFiltroFechas = (Panel)ControlFinder.BuscarControl(this, "pnlFiltroFechas");
            cbTipoDocumento = (DropdownList)ControlFinder.BuscarControl(this, "cbTipoDocumento");
            txtNroDocumento = (TextBox)ControlFinder.BuscarControl(this, "txtNroDocumento");
            cbCategoriaInforme = (DropdownList)ControlFinder.BuscarControl(this, "cbCategoriaInforme");
            cbLoteInforme = (DropdownList)ControlFinder.BuscarControl(this, "cbLoteInforme");
            txtDesde = (TextBox)ControlFinder.BuscarControl(this, "txtDesde");
            txtHasta = (TextBox)ControlFinder.BuscarControl(this, "txtHasta");
            cbGaritaInforme = (DropdownList)ControlFinder.BuscarControl(this, "cbGaritaInforme");
            lblErrorInforme = (Label)ControlFinder.BuscarControl(this, "lblErrorInforme");
        }

        private void CargarCombosFormulario()
        {
            foreach (string zona in ZONAS_HORARIAS)
            {
                cbZonaHoraria.Combo.Items.Add(new ListItem(zona, zona));
            }

            foreach (int meses in OPCIONES_MESES)
            {
                cbMesesMovimientos.Combo.Items.Add(new ListItem(TextoMeses(meses), meses.ToString()));
                cbMesesReportes.Combo.Items.Add(new ListItem(TextoMeses(meses), meses.ToString()));
            }
        }

        private static string TextoMeses(int meses)
        {
            return meses + (meses == 1 ? " mes" : " meses");
        }

        private static void SeleccionarOAgregar(DropDownList combo, string valor, string texto)
        {
            ListItem item = combo.Items.FindByValue(valor);
            if (item == null)
            {
                item = new ListItem(texto, valor);
                combo.Items.Add(item);
            }
            combo.ClearSelection();
            item.Selected = true;
        }

        private void CargarParametros()
        {
            try
            {
                DataRow p = ParametroSistema.Obtener();

                txtNombreBarrio.Text = p["nombre_barrio"].ToString();
                txtDomicilio.Text = p["domicilio"] == DBNull.Value ? "" : p["domicilio"].ToString();

                string zona = p["zona_horaria"].ToString();
                SeleccionarOAgregar(cbZonaHoraria.Combo, zona, zona);

                int mesesMov = Convert.ToInt32(p["meses_conservar_movimientos"]);
                int mesesRep = Convert.ToInt32(p["meses_conservar_reportes"]);
                SeleccionarOAgregar(cbMesesMovimientos.Combo, mesesMov.ToString(), TextoMeses(mesesMov));
                SeleccionarOAgregar(cbMesesReportes.Combo, mesesRep.ToString(), TextoMeses(mesesRep));

                string quien = p["ModificadoPorNombre"] == DBNull.Value ? "" : p["ModificadoPorNombre"].ToString();
                DateTime fecha = Convert.ToDateTime(p["fecha_hora_modificacion"]);
                litUltimoCambio.Text = "Último cambio: " + fecha.ToString("dd/MM/yyyy HH:mm")
                    + (quien != "" ? " por " + quien : "");
            }
            catch (Exception ex)
            {
                lblMensajeError.Text = ex.Message;
            }
        }

        protected void btnDescartar_ButtonClick(object sender, EventArgs e)
        {
            lblMensajeError.Text = "";
            lblMensajeOk.Text = "";
            CargarParametros();
        }

        protected void btnGuardar_ButtonClick(object sender, EventArgs e)
        {
            lblMensajeError.Text = "";
            lblMensajeOk.Text = "";

            string nombreBarrio = txtNombreBarrio.Text.Trim();
            if (nombreBarrio == "")
            {
                lblMensajeError.Text = "El nombre del barrio es obligatorio.";
                return;
            }

            if (Session["Id_usuario"] == null)
            {
                lblMensajeError.Text = "No se pudo identificar al usuario que guarda los cambios. Volvé a iniciar sesión.";
                return;
            }

            try
            {
                ParametroSistema.Actualizar(
                    nombreBarrio,
                    txtDomicilio.Text.Trim(),
                    cbZonaHoraria.Combo.SelectedValue,
                    int.Parse(cbMesesMovimientos.Combo.SelectedValue),
                    int.Parse(cbMesesReportes.Combo.SelectedValue),
                    Convert.ToInt32(Session["Id_usuario"])
                );

                CargarParametros();
                lblMensajeOk.Text = "Cambios guardados.";
            }
            catch (Exception ex)
            {
                lblMensajeError.Text = ex.Message;
            }
        }

        protected void cbTipoInforme_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblErrorInforme.Text = "";
            ReabrirModal();
        }

        protected void btnCancelarInforme_ButtonClick(object sender, EventArgs e)
        {
            ClientScript.RegisterStartupScript(GetType(), "cerrarInforme",
                "cerrarModal('" + mdlInforme.OverlayClientID + "');", true);
        }

        protected void btnDescargarPdf_ButtonClick(object sender, EventArgs e)
        {
            lblErrorInforme.Text = "";

            try
            {
                TipoInforme tipo = (TipoInforme)Enum.Parse(typeof(TipoInforme), cbTipoInforme.Combo.SelectedValue);

                string tipoMovimiento = rblMovimiento.SelectedValue;
                string textoMovimiento = rblMovimiento.SelectedItem.Text;

                int? idGarita = null;
                string textoGarita = "Todas";
                if (!string.IsNullOrEmpty(cbGaritaInforme.Combo.SelectedValue))
                {
                    idGarita = int.Parse(cbGaritaInforme.Combo.SelectedValue);
                    textoGarita = cbGaritaInforme.Combo.SelectedItem.Text;
                }

                DateTime? desde = null;
                DateTime? hasta = null;
                int? idTipoDocumento = null;
                string nroDocumento = null;
                int? idCategoria = null;
                int? idLote = null;
                var especificos = new List<KeyValuePair<string, string>>();

                switch (tipo)
                {
                    case TipoInforme.Persona:
                        nroDocumento = txtNroDocumento.Text.Trim();
                        if (nroDocumento == "")
                        {
                            MostrarErrorInforme("Ingresá el número de documento.");
                            return;
                        }
                        idTipoDocumento = int.Parse(cbTipoDocumento.Combo.SelectedValue);
                        especificos.Add(new KeyValuePair<string, string>("Documento",
                            cbTipoDocumento.Combo.SelectedItem.Text + " " + nroDocumento));
                        break;

                    case TipoInforme.Categoria:
                        idCategoria = int.Parse(cbCategoriaInforme.Combo.SelectedValue);
                        especificos.Add(new KeyValuePair<string, string>("Categoría",
                            cbCategoriaInforme.Combo.SelectedItem.Text));
                        break;

                    case TipoInforme.Lote:
                        idLote = int.Parse(cbLoteInforme.Combo.SelectedValue);
                        especificos.Add(new KeyValuePair<string, string>("Lote",
                            cbLoteInforme.Combo.SelectedItem.Text));
                        break;

                    default:
                        DateTime d;
                        DateTime h;
                        if (!DateTime.TryParseExact(txtDesde.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ||
                            !DateTime.TryParseExact(txtHasta.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out h))
                        {
                            MostrarErrorInforme("Completá las fechas Desde y Hasta.");
                            return;
                        }
                        if (d > h)
                        {
                            MostrarErrorInforme("La fecha Desde no puede ser posterior a Hasta.");
                            return;
                        }
                        desde = d;
                        hasta = h;
                        especificos.Add(new KeyValuePair<string, string>("Desde", d.ToString("dd/MM/yyyy")));
                        especificos.Add(new KeyValuePair<string, string>("Hasta", h.ToString("dd/MM/yyyy")));
                        break;
                }

                var filtros = new List<KeyValuePair<string, string>>();
                var filtroMovimiento = new KeyValuePair<string, string>("Movimiento", textoMovimiento);
                if (tipo == TipoInforme.Fechas)
                {
                    filtros.Add(filtroMovimiento);
                    filtros.AddRange(especificos);
                }
                else
                {
                    filtros.AddRange(especificos);
                    filtros.Add(filtroMovimiento);
                }
                filtros.Add(new KeyValuePair<string, string>("Garita", textoGarita));

                DataTable movimientos = BIZ.Movimiento.ListarParaInforme(desde, hasta, idTipoDocumento,
                    nroDocumento, idCategoria, idLote, idGarita, tipoMovimiento);

                DataRow barrio = ParametroSistema.Obtener();
                string nombreBarrio = barrio["nombre_barrio"].ToString();
                string domicilio = barrio["domicilio"] == DBNull.Value ? "" : barrio["domicilio"].ToString();

                string generadoPor = Session["Usuario"] != null ? Session["Usuario"].ToString() : "";

                byte[] pdf = InformeMovimientosPdf.Generar(tipo, movimientos, nombreBarrio, domicilio, generadoPor, filtros);

                EnviarPdf(pdf);
            }
            catch (Exception ex)
            {
                MostrarErrorInforme("No se pudo generar el informe: " + ex.Message);
            }
        }

        private void EnviarPdf(byte[] pdf)
        {
            string nombreArchivo = "informe-movimientos-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".pdf";

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + nombreArchivo + "\"");
            Response.AddHeader("Content-Length", pdf.Length.ToString());
            Response.BinaryWrite(pdf);
            Response.Flush();
            Response.SuppressContent = true;
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        private void CargarCombosModal()
        {
            if (cbTipoInforme.Combo.Items.Count > 0)
                return;

            cbTipoInforme.Combo.Items.Add(new ListItem("Por persona", "Persona"));
            cbTipoInforme.Combo.Items.Add(new ListItem("Por categoría", "Categoria"));
            cbTipoInforme.Combo.Items.Add(new ListItem("Por lote", "Lote"));
            cbTipoInforme.Combo.Items.Add(new ListItem("Por rango de fechas", "Fechas"));
            cbTipoInforme.Combo.SelectedValue = "Fechas";

            cbTipoDocumento.Combo.DataSource = Cuenta.ListarTiposDeDocumentos().Tables[0];
            cbTipoDocumento.Combo.DataTextField = "descripcion";
            cbTipoDocumento.Combo.DataValueField = "id_tipo_documento";
            cbTipoDocumento.Combo.DataBind();

            cbCategoriaInforme.Combo.DataSource = Persona.ListarCategorias().Tables[0];
            cbCategoriaInforme.Combo.DataTextField = "descripcion";
            cbCategoriaInforme.Combo.DataValueField = "id_categoria";
            cbCategoriaInforme.Combo.DataBind();

            cbLoteInforme.Combo.DataSource = Lote.ListarLotes().Tables[0];
            cbLoteInforme.Combo.DataTextField = "descripcion";
            cbLoteInforme.Combo.DataValueField = "id_lote";
            cbLoteInforme.Combo.DataBind();

            cbGaritaInforme.Combo.Items.Add(new ListItem("Todas", ""));
            cbGaritaInforme.Combo.AppendDataBoundItems = true;
            cbGaritaInforme.Combo.DataSource = Garita.ListarGaritas().Tables[0];
            cbGaritaInforme.Combo.DataTextField = "descripcion";
            cbGaritaInforme.Combo.DataValueField = "id_garita";
            cbGaritaInforme.Combo.DataBind();
        }

        private void AplicarVisibilidadFiltros()
        {
            string tipo = cbTipoInforme.Combo.SelectedValue;
            pnlFiltroPersona.Visible = tipo == "Persona";
            pnlFiltroCategoria.Visible = tipo == "Categoria";
            pnlFiltroLote.Visible = tipo == "Lote";
            pnlFiltroFechas.Visible = tipo == "Fechas";
        }

        private void ReabrirModal()
        {
            ClientScript.RegisterStartupScript(GetType(), "reabrirInforme",
                "document.addEventListener('DOMContentLoaded', function() { mostrarModal('" + mdlInforme.OverlayClientID + "'); });", true);
        }

        private void MostrarErrorInforme(string mensaje)
        {
            lblErrorInforme.Text = mensaje;
            ReabrirModal();
        }
    }
}