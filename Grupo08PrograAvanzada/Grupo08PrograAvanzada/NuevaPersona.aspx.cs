using BIZ;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class NuevaPersona : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Guardia"; }
        }

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
            cbTipoDocumento.Combo.DataTextField = "descripcion";
            cbTipoDocumento.Combo.DataValueField = "id_tipo_documento";
            cbTipoDocumento.Combo.DataBind();

            cbNacionalidad.Combo.DataSource = Pais.ListarPaises().Tables[0];
            cbNacionalidad.Combo.DataTextField = "descripcion";
            cbNacionalidad.Combo.DataValueField = "id_pais";
            cbNacionalidad.Combo.DataBind();

            chkCategorias.DataSource = Persona.ListarCategorias().Tables[0];
            chkCategorias.DataTextField = "descripcion";
            chkCategorias.DataValueField = "id_categoria";
            chkCategorias.DataBind();

            cbCompaniaSeguro.Combo.DataSource = CompaniaSeguro.ListarCompanias();
            cbCompaniaSeguro.Combo.DataTextField = "descripcion";
            cbCompaniaSeguro.Combo.DataValueField = "id_compania_seguro";
            cbCompaniaSeguro.Combo.DataBind();
            cbCompaniaSeguro.Combo.Items.Insert(0, new ListItem("Sin compañía", ""));
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

                if (idCategorias.Count == 0)
                {
                    lblError.Text = "Elegí al menos una categoría para la persona.";
                    return;
                }

                string rutaFoto = GuardarFotoYObtenerRuta();

                // TODO: confirmar firma exacta de DarDeAltaPersonaConCategorias contra BIZ/Persona.cs
                int idPersona = Persona.DarDeAltaPersonaConCategorias(
                    txtNombres.Text,
                    txtApellido.Text,
                    int.Parse(cbNacionalidad.Combo.SelectedValue),
                    txtTelefono.Text,
                    DateTime.Parse(txtFechaNacimiento.Text),
                    int.Parse(cbTipoDocumento.Combo.SelectedValue),
                    txtNumeroDocumento.Text,
                    rutaFoto,
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

                Response.Redirect("~/PadronPersonas.aspx");
            }
            catch (Exception ex)
            {
                lblError.Text = "No se pudo guardar la persona: " + ex.Message +
                    " Si la persona ya quedó creada pero falló el vehículo o la licencia, revisá el padrón antes de reintentar.";
            }
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
            Response.Redirect("~/PadronPersonas.aspx");
        }

        private string GuardarFotoYObtenerRuta()
        {
            string dataUrl = hdnFotoBase64.Value;

            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                return null; // la columna admite NULL, así que sin foto no pasa nada
            }

            int idxComa = dataUrl.IndexOf(',');
            string encabezado = dataUrl.Substring(0, idxComa);
            string base64 = dataUrl.Substring(idxComa + 1);

            string extension = encabezado.Contains("png") ? ".png" : ".jpg";
            byte[] bytes = Convert.FromBase64String(base64);

            string carpeta = Server.MapPath("~/Uploads/Personas/");
            if (!System.IO.Directory.Exists(carpeta))
            {
                System.IO.Directory.CreateDirectory(carpeta);
            }

            string nombreArchivo = Guid.NewGuid().ToString("N") + extension;
            string rutaFisica = System.IO.Path.Combine(carpeta, nombreArchivo);
            System.IO.File.WriteAllBytes(rutaFisica, bytes);

            string rutaRelativa = "/Uploads/Personas/" + nombreArchivo;

            if (rutaRelativa.Length > 500)
            {
                throw new Exception("La ruta generada para la foto es demasiado larga.");
            }

            return rutaRelativa;
        }
    }
}