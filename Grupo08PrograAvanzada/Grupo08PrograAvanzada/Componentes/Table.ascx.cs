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

        public void CargarDatosColumnas (DataTable dataTable, List<ColumnName> columnNames)
        {
            column_Names = columnNames;

            // para el header
            rpt_header.DataSource = column_Names;
            rpt_header.DataBind();

            // para las filas
            rpt_filas.DataSource = dataTable.Rows.Count == 0;
            rpt_filas.DataSource = dataTable;
            rpt_filas.DataBind();
        }

        protected void rpt_filas_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            DataRowView fila = (DataRowView)e.Item.DataItem;
            PlaceHolder ph = (PlaceHolder)e.Item.FindControl("phCeldas");

            foreach (var col in column_Names)
            {
                var td = new HtmlGenericControl("td");
                td.InnerText = fila[col.CampoDato].ToString();
                ph.Controls.Add(td);
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}