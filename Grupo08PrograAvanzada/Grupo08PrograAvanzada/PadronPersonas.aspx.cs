using Grupo08PrograAvanzada.Componentes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using BIZ;

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
            DropdownList ddl_categoria = (DropdownList)ControlFinder.BuscarControl(this, "ddlCategoria");
            ddl_categoria.Combo.Items.Clear();
            ddl_categoria.Combo.Items.Add(new ListItem("Seleccione una categoria", ""));

            foreach (DataRow fila in Persona.ListarCategorias().Tables[0].Rows)
            {
                ddl_categoria.Combo.Items.Add(new ListItem(fila["descripcion"].ToString(), fila["id_categoria"].ToString()));
            }
        }

        private void LlenarTabla()
        {
            string texto = ucBuscar.Input.Text;
            string idCategoria = ((DropdownList)ControlFinder.BuscarControl(this, "ddlCategoria")).Combo.SelectedValue;

            DataTable datos = Persona.BuscarPadron(texto, idCategoria);

            List<ColumnName> columnas = new List<ColumnName>
            {
                new ColumnName { CampoDato = "Persona",       Titulo = "PERSONA" },
                new ColumnName { CampoDato = "nro_documento",  Titulo = "DNI" },
                new ColumnName { CampoDato = "Categoria",      Titulo = "CATEGORÍA" },
                new ColumnName { CampoDato = "Estado",         Titulo = "ESTADO" }
            };

            ucTablaPersonasG.CargarDatosColumnas(datos, columnas); // en PadronPersonasAdmin usa ucTablaPersonas, sin la "G"
        }

        protected void btnNuevaPersona_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/NuevaPersona.aspx");
        }

        protected void ucBuscar_BuscarClick(object sender, EventArgs e)
        {
            LlenarTabla();
        }
    }
}