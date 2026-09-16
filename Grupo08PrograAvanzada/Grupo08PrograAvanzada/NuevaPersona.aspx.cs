using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class NuevaPersona : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            CargarCombos();
            pnlVehiculo.Visible = chkTieneVehiculo.Checked;
        }
        private void CargarCombos()
        {
            cbTipoDocumento.Combo.Items.Add(new ListItem("DNI", "DNI"));
            cbTipoDocumento.Combo.Items.Add(new ListItem("LC", "LC"));
            cbTipoDocumento.Combo.Items.Add(new ListItem("CI", "CI"));

            // TODO: reemplazar por consulta real a la tabla PAIS
            cbNacionalidad.Combo.Items.Add(new ListItem("Argentina", "ARG"));
            cbNacionalidad.Combo.Items.Add(new ListItem("Otra", "OTRA"));

            // TODO: reemplazar por consulta real a la tabla LOTE
            cbLote.Combo.Items.Add(new ListItem("Seleccioná un lote...", ""));
            cbLote.Combo.Items.Add(new ListItem("A14 — Sendero de los Tilos 1400", "A14"));
        }

        protected void chkTieneVehiculo_CheckedChanged(object sender, EventArgs e)
        {
            pnlVehiculo.Visible = chkTieneVehiculo.Checked;
        }

        protected void btnGuardar_ButtonClick(object sender, EventArgs e)
        {
            // TODO: validar campos obligatorios y guardar CUENTA + PERSONA + PERSONA_CATEGORIA
            string tipoDocumento = cbTipoDocumento.Combo.SelectedValue;
            string numeroDocumento = txtNumeroDocumento.Text;
            string apellido = txtApellido.Text;
            string nombres = txtNombres.Text;
            string lote = cbLote.Combo.SelectedValue;

            List<string> categoriasSeleccionadas = new List<string>();
            foreach (ListItem item in chkCategorias.Items)
            {
                if (item.Selected)
                    categoriasSeleccionadas.Add(item.Value);
            }

            if (chkTieneVehiculo.Checked)
            {
                // TODO: guardar VEHICULO con estos datos
                string patente = txtPatente.Text;
                string marcaModelo = txtMarcaModelo.Text;
                string companiaSeguro = txtCompaniaSeguro.Text;
                string numeroPoliza = txtNumeroPoliza.Text;
                string vigenciaSeguro = txtVigenciaSeguro.Text;
            }

            // Response.Redirect("~/PadronDePersonas.aspx");
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/PadronDePersonas.aspx");
        }
    }
}