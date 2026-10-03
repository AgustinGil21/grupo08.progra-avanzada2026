using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada.Componentes
{
    public partial class Table : System.Web.UI.UserControl
    {
        //ColumnName es una clase que contiene el nombre del campo de la tabla y
        //el titulo que se mostrara en el header
        private List<ColumnName> column_Names;

        public void CargarDatosColumnas(DataTable dataTable, List<ColumnName> columnNames)
        {
            column_Names = columnNames;

            // para el header
            rpt_header.DataSource = column_Names;
            rpt_header.DataBind();

            // para las filas
            rpt_filas.DataSource = dataTable;
            rpt_filas.DataBind();

            // CAMBIO 1: lb_vacio existía pero nunca se mostraba. Ahora aparece cuando no hay filas.
            lb_vacio.Visible = dataTable == null || dataTable.Rows.Count == 0;
        }

        protected void rpt_filas_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            DataRowView fila = (DataRowView)e.Item.DataItem;
            PlaceHolder ph = (PlaceHolder)e.Item.FindControl("ph_celdas");

            foreach (var col in column_Names)
            {
                var td = new HtmlGenericControl("td");

                // CAMBIO 2: si la columna no existe en el DataTable, se muestra "—" en vez de tirar
                // una excepción (que cortaba la fila después de la primera celda).
                td.InnerText = fila.DataView.Table.Columns.Contains(col.CampoDato)
                    ? Convert.ToString(fila[col.CampoDato])
                    : "\u2014";

                ph.Controls.Add(td);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}
