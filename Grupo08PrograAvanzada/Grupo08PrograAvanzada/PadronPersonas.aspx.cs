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
    public partial class PadronPersonas : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Guardia"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDDL();
                LlenarTabla();
            }
        }

        private void CargarDDL()
        {
            DropDownList ddl_categoria = (DropDownList)FindControl("ddlCategoria");
            ddl_categoria.Items.Clear();
            ddl_categoria.Items.Add(new ListItem("Seleccione una categoria", ""));

            foreach (DataRow fila Categoria.ListarCategorias().Rows)
            {
                ddl_categoria.Items.Add(new ListItem(fila["descripcion"].ToString(), fila["id_categoria"].ToString()));
            }
        }

        private void LlenarTabla()
        {
            string texto = ucBuscar.Input.Text;
            string idCategoria = ((DropdownList)FindControl("ddlCategoria")).Combo.SelectedValue;

            DataTable datos = Persona.BuscarPadron(texto, idCategoria);

            List<ColumnName> columnas = new List<ColumnName>
            {
                new ColumnName { CampoDato = "Persona",   Titulo = "PERSONA" },
                new ColumnName { CampoDato = "Categoria", Titulo = "CATEGORÍA" },
                new ColumnName { CampoDato = "Estado",    Titulo = "ESTADO" }
            };

            ucTablaPersonasG.CargarDatosColumnas(datos, columnas);
        }

        protected void btnNuevaPersona_ButtonClick()
        {
            LlenarTabla();
        }

        protected void ucBuscar_BuscarClick()
        {
            Response.Redirect("~/NuevaPersona.aspx");
        }
    }
}