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
    public partial class Lotes : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Guardia"; }
        }

        private DropdownList ddlEstadoFiltro;
        private Literal litResumen;
        private Buscar ucBuscar;
        private Repeater rptLotes;
        private Label lblSinResultados;

        protected void Page_Load(object sender, EventArgs e)
        {
            litResumen = (Literal)ControlFinder.BuscarControl(this, "litResumen");
            ucBuscar = (Buscar)ControlFinder.BuscarControl(this, "ucBuscar");
            rptLotes = (Repeater)ControlFinder.BuscarControl(this, "rptLotes");
            lblSinResultados = (Label)ControlFinder.BuscarControl(this, "lblSinResultados");

            ddlEstadoFiltro = (DropdownList)ControlFinder.BuscarControl(this, "ddlEstadoFiltro");

            if (Page.Header != null)
            {
                Page.Header.Controls.Add(new LiteralControl(
                    "<link rel=\"stylesheet\" href=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.css\" />" +
                    "<script src=\"https://unpkg.com/leaflet@1.9.4/dist/leaflet.js\"></script>"));
            }

            if (!IsPostBack)
            {
                CargarComboEstado();
                CargarLotes();
            }
        }

        private void CargarComboEstado()
        {
            DataTable todosLosEstados = Estado.ListarEstados();

            int[] idsParaLote = { 1, 2, 3 };

            var dv = new DataView(todosLosEstados);
            dv.RowFilter = $"id_estado IN ({string.Join(",", idsParaLote)})";

            foreach (var ddl in new[] { ddlEstadoFiltro.Combo })
            {
                ddl.DataSource = dv;
                ddl.DataTextField = "descripcion";
                ddl.DataValueField = "id_estado";
                ddl.DataBind();
            }

            ddlEstadoFiltro.Combo.SelectedValue = "1";
            ddlEstadoFiltro.Combo.Enabled = false;
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

            string idEstado = ddlEstadoFiltro.Combo.SelectedValue;
            if (idEstado != "0")
                filtros.Add($"id_estado = {idEstado}");

            dv.RowFilter = string.Join(" AND ", filtros);
            AplicarBind(dv);
        }

        protected void rptLotes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var fila = (DataRowView)e.Item.DataItem;
            var litEstado = (Literal)e.Item.FindControl("litEstado");
            litEstado.Text = fila["EstadoDescripcion"].ToString();
        }
    }
}