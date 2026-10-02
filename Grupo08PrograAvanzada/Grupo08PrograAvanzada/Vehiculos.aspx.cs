using BIZ;
using Grupo08PrograAvanzada.Componentes;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Button = Grupo08PrograAvanzada.Componentes.Button;

namespace Grupo08PrograAvanzada
{
    public partial class Vehiculos : PaginaBase
    {
        protected override string RolRequerido
        {
            get { return "Guardia"; }
        }

        public string HdnIdVehiculoClientId
        {
            get { return hdnIdVehiculo.ClientID; }
        }

        private DropdownList cbEstado;
        private Checkbox chkVerSoloVencidos;
        private HiddenField hdnIdVehiculo;
        private DropdownList cbCompaniaSeguroModal;
        private Input txtNumeroPolizaModal;
        private Input txtVigenciaSeguroModal;
        private Label lblErrorModal;

        protected void Page_Load(object sender, EventArgs e)
        {
            cbEstado = (DropdownList)ControlFinder.BuscarControl(this, "cbEstado");
            chkVerSoloVencidos = (Checkbox)ControlFinder.BuscarControl(this, "chkVerSoloVencidos");
            hdnIdVehiculo = (HiddenField)ControlFinder.BuscarControl(this, "hdnIdVehiculo");
            cbCompaniaSeguroModal = (DropdownList)ControlFinder.BuscarControl(this, "cbCompaniaSeguroModal");
            txtNumeroPolizaModal = (Input)ControlFinder.BuscarControl(this, "txtNumeroPolizaModal");
            txtVigenciaSeguroModal = (Input)ControlFinder.BuscarControl(this, "txtVigenciaSeguroModal");
            lblErrorModal = (Label)ControlFinder.BuscarControl(this, "lblErrorModal");

            if (!IsPostBack)
            {
                CargarCombos();
                CargarVehiculos("", "");
            }
        }

        private void CargarCombos()
        {
            DataTable estados = Estado.ListarEstados();
            DataRow[] filas = estados.Select("id_estado IN (5,6)");

            cbEstado.Combo.Items.Clear();
            cbEstado.Combo.Items.Add(new ListItem("Todos", ""));
            foreach (DataRow fila in filas)
            {
                string descripcion = fila["descripcion"].ToString();
                cbEstado.Combo.Items.Add(new ListItem(descripcion, descripcion));
            }

            cbCompaniaSeguroModal.Combo.DataSource = CompaniaSeguro.ListarCompanias();
            cbCompaniaSeguroModal.Combo.DataTextField = "descripcion";
            cbCompaniaSeguroModal.Combo.DataValueField = "id_compania_seguro";
            cbCompaniaSeguroModal.Combo.DataBind();
        }

        private void CargarVehiculos(string patente, string filtroEstado)
        {
            DataTable dt = Vehiculo.BuscarVehiculos(patente, filtroEstado);

            rptVehiculos.DataSource = dt;
            rptVehiculos.DataBind();

            lblVacio.Visible = dt.Rows.Count == 0;
        }

        protected void ucBuscar_BuscarClick(object sender, EventArgs e)
        {
            string filtro = chkVerSoloVencidos.Checked ? "Vencido" : cbEstado.Combo.SelectedValue;
            CargarVehiculos(ucBuscar.Input.Text, filtro);
        }

        protected void btnGuardarPoliza_ButtonClick(object sender, EventArgs e)
        {
            lblErrorModal.Text = "";

            if (string.IsNullOrWhiteSpace(cbCompaniaSeguroModal.Combo.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtNumeroPolizaModal.Value) ||
                string.IsNullOrWhiteSpace(txtVigenciaSeguroModal.Value))
            {
                lblErrorModal.Text = "Completá compañía, número de póliza y vigencia.";
                return;
            }

            try
            {
                int idVehiculo = int.Parse(hdnIdVehiculo.Value);
                int idCompaniaSeguro = int.Parse(cbCompaniaSeguroModal.Combo.SelectedValue);

                Vehiculo.DarDeAltaPolizaSeguro(
                    idVehiculo,
                    idCompaniaSeguro,
                    txtNumeroPolizaModal.Value,
                    DateTime.Parse(txtVigenciaSeguroModal.Value)
                );

                string filtro = chkVerSoloVencidos.Checked ? "Vencido" : cbEstado.Combo.SelectedValue;
                CargarVehiculos(ucBuscar.Input.Text, filtro);

                ScriptManager.RegisterStartupScript(this, GetType(), "cerrarModalPoliza",
                    "document.addEventListener('DOMContentLoaded', function() { cerrarModal('" + mdlActualizarPoliza.OverlayClientID + "'); });", true);
            }
            catch (Exception ex)
            {
                lblErrorModal.Text = "No se pudo actualizar la póliza: " + ex.Message;
            }
        }
    }
}