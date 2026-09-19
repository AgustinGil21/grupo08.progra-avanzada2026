using BIZ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class NuevaPersonaAdmin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCombos();
                pnlVehiculo.Visible = chkTieneVehiculo.Checked;
            }
        }

        private void CargarCombos()
        {
            cbTipoDocumento.Combo.DataSource = Cuenta.ListarTiposDeDocumentos().Tables[0];
            cbTipoDocumento.Combo.DataTextField = "descripcion";        // TODO: confirmar nombre real de columna
            cbTipoDocumento.Combo.DataValueField = "id_tipo_documento"; // TODO: confirmar nombre real de columna
            cbTipoDocumento.Combo.DataBind();

            cbNacionalidad.Combo.DataSource = Pais.ListarPaises().Tables[0];
            cbNacionalidad.Combo.DataTextField = "descripcion"; // TODO: confirmar nombre real de columna en Pais
            cbNacionalidad.Combo.DataValueField = "id_pais";    // TODO: confirmar nombre real de columna en Pais
            cbNacionalidad.Combo.DataBind();

            chkCategorias.DataSource = Persona.ListarCategorias().Tables[0];
            chkCategorias.DataTextField = "descripcion";   // TODO: confirmar nombre real de columna
            chkCategorias.DataValueField = "id_categoria"; // TODO: confirmar nombre real de columna
            chkCategorias.DataBind();

            cbCompaniaSeguro.Combo.Items.Add(new ListItem("Sin compañía", ""));
            cbCompaniaSeguro.Combo.DataSource = CompaniaSeguro.ListarCompanias();
            cbCompaniaSeguro.Combo.DataTextField = "descripcion";
            cbCompaniaSeguro.Combo.DataValueField = "id_compania_seguro";
            cbCompaniaSeguro.Combo.DataBind();
        }

        protected void chkTieneVehiculo_CheckedChanged(object sender, EventArgs e)
        {
            pnlVehiculo.Visible = chkTieneVehiculo.Checked;
        }

        protected void btnGuardar_ButtonClick(object sender, EventArgs e)
        {
            lblError.Text = "";

            if (string.IsNullOrWhiteSpace(txtFechaNacimiento.Text))
            {
                lblError.Text = "La fecha de nacimiento es obligatoria.";
                return;
            }

            if (chkTieneVehiculo.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtNumeroLicencia.Text) || string.IsNullOrWhiteSpace(txtVigenciaLicencia.Text))
                {
                    lblError.Text = "La licencia de conducir es obligatoria si la persona tiene vehículo.";
                    return;
                }

                if (Vehiculo.ExistePatente(txtPatente.Text))
                {
                    lblError.Text = "Ya existe un vehículo registrado con esa patente.";
                    return;
                }
            }

            try
            {
                List<int> idCategorias = new List<int>();
                foreach (ListItem item in chkCategorias.Items)
                {
                    if (item.Selected)
                    {
                        idCategorias.Add(int.Parse(item.Value));
                    }
                }

                int idPersona = Persona.DarDeAltaPersonaConCategorias(
                    txtNombres.Text,
                    txtApellido.Text,
                    int.Parse(cbNacionalidad.Combo.SelectedValue),
                    txtTelefono.Text,
                    DateTime.Parse(txtFechaNacimiento.Text),
                    int.Parse(cbTipoDocumento.Combo.SelectedValue),
                    txtNumeroDocumento.Text,
                    txtFotoReferencia.Text,
                    idCategorias
                );

                if (chkTieneVehiculo.Checked)
                {
                    int idVehiculo = Vehiculo.DarDeAltaVehiculo(txtPatente.Text, txtMarcaModelo.Text, idPersona);

                    if (!string.IsNullOrWhiteSpace(cbCompaniaSeguro.Combo.SelectedValue) &&
                        !string.IsNullOrWhiteSpace(txtNumeroPoliza.Text))
                    {
                        Vehiculo.DarDeAltaPolizaSeguro(
                            idVehiculo,
                            int.Parse(cbCompaniaSeguro.Combo.SelectedValue),
                            txtNumeroPoliza.Text,
                            DateTime.Parse(txtVigenciaSeguro.Text)
                        );
                    }

                    Persona.DarDeAltaLicencia(idPersona, txtNumeroLicencia.Text, DateTime.Parse(txtVigenciaLicencia.Text));
                }

                Response.Redirect("~/Administracion/PadronDePersonas.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo guardar la persona: " + ex.Message +
                    " Si la persona ya quedó creada pero falló el vehículo o la licencia, revisá el padrón antes de reintentar (el alta no es transaccional de punta a punta).";
            }
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/Administracion/PadronDePersonas.aspx");
        }


    }
}
 