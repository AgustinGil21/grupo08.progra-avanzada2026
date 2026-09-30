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
    public partial class LotesAdmin : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Administrador"; }
        }

        private DropdownList ddlEstadoFiltroReal;
        private DropdownList ddlTitularFiltroReal;
        private Input txtNuevoDescripcionReal;
        private DropdownList ddlNuevoEstadoReal;
        private Input txtNuevoLatitudReal;
        private Input txtNuevoLongitudReal;
        private Input txtNuevoDniReal;
        private Literal litNuevoPersonaEncontradaReal;
        private Literal litErrorNuevoLoteReal;

        private HiddenField hdnIdLoteEditarReal;
        private Input txtEditarDescripcionReal;
        private DropdownList ddlEditarEstadoReal;
        private Input txtEditarLatitudReal;
        private Input txtEditarLongitudReal;
        private Literal litEditarTitularActualReal;
        private Literal litErrorEditarLoteReal;

        private HiddenField hdnIdLoteTitularReal;
        private Input txtTitularDniReal;
        private Literal litPersonaEncontradaTitularReal;
        private HiddenField hdnIdPersonaTitularReal;

        private Literal litResumen;
        private Buscar ucBuscar;
        private Repeater rptLotes;
        private Label lblSinResultados;

        private int? idPersonaEncontradaNuevoLote
        {
            get { return ViewState["idPersonaEncontradaNuevoLote"] as int?; }
            set { ViewState["idPersonaEncontradaNuevoLote"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ddlEstadoFiltroReal = (DropdownList)ControlFinder.BuscarControl(this, "ddlEstadoFiltro");
            ddlTitularFiltroReal = (DropdownList)ControlFinder.BuscarControl(this, "ddlTitularFiltro");

            txtNuevoDescripcionReal = (Input)ControlFinder.BuscarControl(this, "txtNuevoDescripcion");
            ddlNuevoEstadoReal = (DropdownList)ControlFinder.BuscarControl(this, "ddlNuevoEstado");
            txtNuevoLatitudReal = (Input)ControlFinder.BuscarControl(this, "txtNuevoLatitud");
            txtNuevoLongitudReal = (Input)ControlFinder.BuscarControl(this, "txtNuevoLongitud");
            txtNuevoDniReal = (Input)ControlFinder.BuscarControl(this, "txtNuevoDni");
            litNuevoPersonaEncontradaReal = (Literal)ControlFinder.BuscarControl(this, "litNuevoPersonaEncontrada");
            litErrorNuevoLoteReal = (Literal)ControlFinder.BuscarControl(this, "litErrorNuevoLote");

            hdnIdLoteEditarReal = (HiddenField)ControlFinder.BuscarControl(this, "hdnIdLoteEditar");
            txtEditarDescripcionReal = (Input)ControlFinder.BuscarControl(this, "txtEditarDescripcion");
            ddlEditarEstadoReal = (DropdownList)ControlFinder.BuscarControl(this, "ddlEditarEstado");
            txtEditarLatitudReal = (Input)ControlFinder.BuscarControl(this, "txtEditarLatitud");
            txtEditarLongitudReal = (Input)ControlFinder.BuscarControl(this, "txtEditarLongitud");
            litEditarTitularActualReal = (Literal)ControlFinder.BuscarControl(this, "litEditarTitularActual");
            litErrorEditarLoteReal = (Literal)ControlFinder.BuscarControl(this, "litErrorEditarLote");

            hdnIdLoteTitularReal = (HiddenField)ControlFinder.BuscarControl(this, "hdnIdLoteTitular");
            txtTitularDniReal = (Input)ControlFinder.BuscarControl(this, "txtTitularDni");
            litPersonaEncontradaTitularReal = (Literal)ControlFinder.BuscarControl(this, "litPersonaEncontradaTitular");
            hdnIdPersonaTitularReal = (HiddenField)ControlFinder.BuscarControl(this, "hdnIdPersonaTitular");

            litResumen = (Literal)ControlFinder.BuscarControl(this, "litResumen");
            ucBuscar = (Buscar)ControlFinder.BuscarControl(this, "ucBuscar");
            rptLotes = (Repeater)ControlFinder.BuscarControl(this, "rptLotes");
            lblSinResultados = (Label)ControlFinder.BuscarControl(this, "lblSinResultados");

            // Leaflet se agrega en cada request (no solo !IsPostBack) para que el <head> siempre lo tenga.
            if (Page.Header != null)
            {
                Page.Header.Controls.Add(new LiteralControl(
                    "<link rel=\"stylesheet\" href=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.css\" />" +
                    "<script src=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.js\"></script>"));
            }

            if (!IsPostBack)
            {
                CargarCombosEstado();
                CargarCombosTitular();
                CargarLotes();
            }
        }

        private void CargarCombosEstado()
        {
            DataTable todosLosEstados = Estado.ListarEstados();

            int[] idsParaLote = { 1, 2, 3 };

            var dv = new DataView(todosLosEstados);
            dv.RowFilter = $"id_estado IN ({string.Join(",", idsParaLote)})";

            foreach (var ddl in new[] { ddlEstadoFiltroReal.Combo, ddlNuevoEstadoReal.Combo, ddlEditarEstadoReal.Combo })
            {
                ddl.DataSource = dv;
                ddl.DataTextField = "descripcion";
                ddl.DataValueField = "id_estado";
                ddl.DataBind();
            }

            ddlNuevoEstadoReal.Combo.SelectedValue = "1";
            ddlNuevoEstadoReal.Combo.Enabled = false;
        }

        private void CargarCombosTitular()
        {
            ddlTitularFiltroReal.Combo.Items.Clear();
            ddlTitularFiltroReal.Combo.Items.Add(new ListItem("Todos", "0"));
            ddlTitularFiltroReal.Combo.Items.Add(new ListItem("Con titular", "1"));
            ddlTitularFiltroReal.Combo.Items.Add(new ListItem("Sin titular", "2"));
        }

        private void CargarLotes()
        {
            DataSet ds = Lote.ListarLotes();
            DataTable dt = ds.Tables[0];
            dt.PrimaryKey = new[] { dt.Columns["id_lote"] };

            ViewState["dtLotes"] = dt;

            var dv = new DataView(dt);
            AplicarBind(dv);

            RegistrarDatosMapa(dt);
        }

        private void AplicarBind(DataView dv)
        {
            rptLotes.DataSource = dv;
            rptLotes.DataBind();

            lblSinResultados.Visible = dv.Count == 0;

            int sinTitular = dv.Cast<DataRowView>().Count(r => r["id_persona_titular"] == DBNull.Value);
            litResumen.Text = $"{dv.Count} lotes · {sinTitular} sin titular asignado · alta y edición desde administración";
        }

        private void RegistrarDatosMapa(DataTable dt)
        {
            var lotesParaMapa = dt.AsEnumerable()
                .Where(r => r["latitud"] != DBNull.Value && r["longitud"] != DBNull.Value)
                .Select(r => new
                {
                    lat = Convert.ToDouble(r["latitud"]),
                    lng = Convert.ToDouble(r["longitud"]),
                    descripcion = r["descripcion"].ToString(),
                    estado = r["EstadoDescripcion"].ToString(),
                    titular = r["Titular"].ToString()
                });

            string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(lotesParaMapa);
            ClientScript.RegisterStartupScript(GetType(), "lotesMapa", $"var lotesParaMapa = {json};", true);
        }

        protected void btnNuevoLote_ButtonClick()
        {
            ClientScript.RegisterStartupScript(GetType(), "abrirNuevo",
                $"mostrarModal('{mdlNuevoLote.OverlayClientID}');", true);
        }

        protected void ucBuscar_BuscarClick()
        {
            DataTable dt = ViewState["dtLotes"] as DataTable;
            if (dt == null) { CargarLotes(); dt = ViewState["dtLotes"] as DataTable; }

            var dv = new DataView(dt);
            var filtros = new System.Collections.Generic.List<string>();

            string texto = ucBuscar.Input.Text?.Trim();
            if (!string.IsNullOrEmpty(texto))
            {
                string textoEscapado = texto.Replace("'", "''");
                filtros.Add($"(descripcion LIKE '%{textoEscapado}%' OR Titular LIKE '%{textoEscapado}%')");
            }

            string idEstado = ddlEstadoFiltroReal.Combo.SelectedValue;
            if (idEstado != "0")
                filtros.Add($"id_estado = {idEstado}");

            string filtroTitular = ddlTitularFiltroReal.Combo.SelectedValue;
            if (filtroTitular == "1") filtros.Add("id_persona_titular IS NOT NULL");
            if (filtroTitular == "2") filtros.Add("id_persona_titular IS NULL");

            dv.RowFilter = string.Join(" AND ", filtros);
            AplicarBind(dv);
        }

        protected void rptLotes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var fila = (DataRowView)e.Item.DataItem;
            var litEstado = (Literal)e.Item.FindControl("litEstado");
            var btnAccion = (LinkButton)e.Item.FindControl("btnAccionFila");

            litEstado.Text = fila["EstadoDescripcion"].ToString();

            bool sinTitular = fila["id_persona_titular"] == DBNull.Value;
            btnAccion.Text = sinTitular ? "Asignar titular" : "Editar";
            btnAccion.CommandArgument = fila["id_lote"].ToString();
        }

        protected void btnCrearLote_ButtonClick()
        {
            string descripcion = txtNuevoDescripcionReal.Value?.Trim();

            if (string.IsNullOrEmpty(descripcion))
            {
                litErrorNuevoLoteReal.Text = "El número de lote es obligatorio.";
                return;
            }

            if (Lote.ExisteDescripcion(descripcion, null))
            {
                litErrorNuevoLoteReal.Text = "Ya existe un lote con ese número.";
                return;
            }

            decimal? latitud = string.IsNullOrEmpty(txtNuevoLatitudReal.Value) ? (decimal?)null : Convert.ToDecimal(txtNuevoLatitudReal.Value);
            decimal? longitud = string.IsNullOrEmpty(txtNuevoLongitudReal.Value) ? (decimal?)null : Convert.ToDecimal(txtNuevoLongitudReal.Value);

            try
            {
                Lote.DarDeAltaLote(descripcion, latitud, longitud, idPersonaEncontradaNuevoLote);
                CargarLotes();
                ClientScript.RegisterStartupScript(GetType(), "cerrarNuevo",
                    $"cerrarModal('{mdlNuevoLote.OverlayClientID}');", true);
            }
            catch (Exception ex)
            {
                litErrorNuevoLoteReal.Text = ex.Message;
            }
        }

        protected void btnNuevoBuscarDni_ButtonClick()
        {
            string dni = txtNuevoDniReal.Value?.Trim();
            DataTable resultado = Persona.BuscarPersonaPorDocumento(dni);

            if (resultado.Rows.Count == 0)
            {
                litNuevoPersonaEncontradaReal.Text = "No se encontró a la persona en el padrón.";
                idPersonaEncontradaNuevoLote = null;
                return;
            }

            DataRow p = resultado.Rows[0];
            litNuevoPersonaEncontradaReal.Text = $"{p["apellido"]}, {p["nombre"]} · {p["Categoria"]} · en el padrón";
            idPersonaEncontradaNuevoLote = Convert.ToInt32(p["id_persona"]);
        }

        protected void btnCambiarTitular_ButtonClick()
        {
            AbrirModalAsignarTitular(int.Parse(hdnIdLoteEditarReal.Value));
        }

        protected void btnGuardarEdicion_ButtonClick()
        {
            int idLote = int.Parse(hdnIdLoteEditarReal.Value);
            string descripcion = txtEditarDescripcionReal.Value?.Trim();
            int idEstado = int.Parse(ddlEditarEstadoReal.Combo.SelectedValue);

            if (Lote.ExisteDescripcion(descripcion, idLote))
            {
                litErrorEditarLoteReal.Text = "Ya existe otro lote con ese número.";
                return;
            }

            decimal? latitud = string.IsNullOrEmpty(txtEditarLatitudReal.Value) ? (decimal?)null : Convert.ToDecimal(txtEditarLatitudReal.Value);
            decimal? longitud = string.IsNullOrEmpty(txtEditarLongitudReal.Value) ? (decimal?)null : Convert.ToDecimal(txtEditarLongitudReal.Value);

            try
            {
                Lote.ActualizarLote(idLote, descripcion, idEstado, latitud, longitud);
                CargarLotes();
                ClientScript.RegisterStartupScript(GetType(), "cerrarEditar",
                    $"cerrarModal('{mdlEditarLote.OverlayClientID}');", true);
            }
            catch (Exception ex)
            {
                litErrorEditarLoteReal.Text = ex.Message;
            }
        }

        protected void btnBuscarDniTitular_ButtonClick()
        {
            string dni = txtTitularDniReal.Value?.Trim();
            DataTable resultado = Persona.BuscarPersonaPorDocumento(dni);

            if (resultado.Rows.Count == 0)
            {
                litPersonaEncontradaTitularReal.Text = "No se encontró a la persona en el padrón.";
                hdnIdPersonaTitularReal.Value = "";
                return;
            }

            DataRow p = resultado.Rows[0];
            litPersonaEncontradaTitularReal.Text = $"{p["apellido"]}, {p["nombre"]} · {p["Categoria"]} · en el padrón";
            hdnIdPersonaTitularReal.Value = p["id_persona"].ToString();
        }

        protected void btnConfirmarTitular_ButtonClick()
        {
            int idLote = int.Parse(hdnIdLoteTitularReal.Value);
            if (string.IsNullOrEmpty(hdnIdPersonaTitularReal.Value))
            {
                return;
            }

            int idPersona = int.Parse(hdnIdPersonaTitularReal.Value);

            Lote.AsignarTitular(idLote, idPersona);
            CargarLotes();
            ClientScript.RegisterStartupScript(GetType(), "cerrarAsignar",
                $"cerrarModal('{mdlAsignarTitular.OverlayClientID}');", true);
        }

        protected void rptLotes_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "AccionFila") return;

            int idLote = int.Parse(e.CommandArgument.ToString());
            DataTable dt = ViewState["dtLotes"] as DataTable;
            DataRow fila = dt.Rows.Find(idLote);

            bool sinTitular = fila["id_persona_titular"] == DBNull.Value;

            if (sinTitular)
                AbrirModalAsignarTitular(idLote);
            else
                AbrirModalEditarLote(fila);
        }

        private void AbrirModalEditarLote(DataRow fila)
        {
            hdnIdLoteEditarReal.Value = fila["id_lote"].ToString();
            txtEditarDescripcionReal.Value = fila["descripcion"].ToString();
            ddlEditarEstadoReal.Combo.SelectedValue = fila["id_estado"].ToString();
            txtEditarLatitudReal.Value = fila["latitud"] == DBNull.Value ? "" : fila["latitud"].ToString();
            txtEditarLongitudReal.Value = fila["longitud"] == DBNull.Value ? "" : fila["longitud"].ToString();
            litEditarTitularActualReal.Text = fila["Titular"].ToString();

            ClientScript.RegisterStartupScript(GetType(), "abrirEditar",
                $"mostrarModal('{mdlEditarLote.OverlayClientID}');", true);
        }

        private void AbrirModalAsignarTitular(int idLote)
        {
            hdnIdLoteTitularReal.Value = idLote.ToString();
            ClientScript.RegisterStartupScript(GetType(), "abrirAsignar",
                $"mostrarModal('{mdlAsignarTitular.OverlayClientID}');", true);
        }
    }
}