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

        private DropdownList ddlEstadoFiltroReal;
        private Buscar ucBuscarReal;
        private Repeater rptLotesReal;
        private Label lblSinResultadosReal;
        private Componentes.Button btnVerPlanoReal;

        protected void Page_Load(object sender, EventArgs e)
        {
            ddlEstadoFiltroReal = (DropdownList)ControlFinder.BuscarControl(this, "ddlEstadoFiltro");
            ucBuscarReal = (Buscar)ControlFinder.BuscarControl(this, "ucBuscar");
            rptLotesReal = (Repeater)ControlFinder.BuscarControl(this, "rptLotes");
            lblSinResultadosReal = (Label)ControlFinder.BuscarControl(this, "lblSinResultados");
            btnVerPlanoReal = (Componentes.Button)ControlFinder.BuscarControl(this, "btnVerPlano");

            btnVerPlanoReal.OnClientClick = "verPlanoCompleto(); return false;";

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

            ddlEstadoFiltroReal.Combo.DataSource = dv;
            ddlEstadoFiltroReal.Combo.DataTextField = "descripcion";
            ddlEstadoFiltroReal.Combo.DataValueField = "id_estado";
            ddlEstadoFiltroReal.Combo.DataBind();

            ddlEstadoFiltroReal.Combo.Items.Insert(0, new ListItem("Todos los estados", "0"));
        }

        private void CargarLotes()
        {
            DataSet ds = Lote.ListarLotes();
            DataTable dt = ds.Tables[0];

            ViewState["dtLotes"] = dt;

            var dv = new DataView(dt);
            AplicarBind(dv);
            RegistrarDatosMapa(dt);
        }

        private void AplicarBind(DataView dv)
        {
            rptLotesReal.DataSource = dv;
            rptLotesReal.DataBind();
            lblSinResultadosReal.Visible = dv.Count == 0;
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

        protected void ucBuscar_BuscarClick(object sender, EventArgs e)
        {
            DataTable dt = ViewState["dtLotes"] as DataTable;
            if (dt == null) { CargarLotes(); dt = ViewState["dtLotes"] as DataTable; }

            var dv = new DataView(dt);
            var filtros = new System.Collections.Generic.List<string>();

            string texto = ucBuscarReal.Input.Text?.Trim();
            if (!string.IsNullOrEmpty(texto))
            {
                string textoEscapado = texto.Replace("'", "''");
                filtros.Add($"(descripcion LIKE '%{textoEscapado}%' OR Titular LIKE '%{textoEscapado}%')");
            }

            string idEstado = ddlEstadoFiltroReal.Combo.SelectedValue;
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

        protected void btnCerrarPlano_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Lotes.aspx");
        }
    }
}