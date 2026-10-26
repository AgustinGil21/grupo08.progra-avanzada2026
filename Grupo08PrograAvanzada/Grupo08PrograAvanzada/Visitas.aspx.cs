using BIZ;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Grupo08PrograAvanzada
{
    public partial class Visitas : PaginaBase
    {
        protected override string RolRequerido { get { return "Guardia"; } }

        private const string VALOR_A_PIE = "APIE";
        private const string VALOR_OTRO = "OTRO";
        private const int ID_CATEGORIA_PROPIETARIO = 1;
        private const int ID_ESTADO_ACTIVO = 1;
        private const bool MOSTRAR_DIAGNOSTICO = false;
        private const int DIAS_FUTUROS_MAX = 30;
        private const int MINUTOS_PASO_HORA = 30;
        private const string MENSAJE_FECHA_HORA = "Elegí la fecha y la hora de la visita.";

        // Modal 1: buscar persona
        private Componentes.DropdownList cbTipoDocBuscar;
        private Componentes.Input inNroDocBuscar;
        private Label lblErrorBuscar;

        // Modal 2: persona existente
        private Literal litExistenteNombre;
        private Literal litExistenteDetalle;
        private Label lblCategoriaExistente;
        private RadioButtonList rblCategoriaExistente;
        private Componentes.DropdownList cbLoteExistente;

        private Label lblErrorExistente;
        private Componentes.Button btnPreacreditar;

        // Modal 3: persona nueva
        private Literal litNuevaMensaje;
        private Literal litNuevaDocumento;
        private Componentes.DropdownList cbPaisNueva;
        private Componentes.Input inApellidoNueva;
        private Componentes.Input inNombresNueva;
        private Componentes.Input inFechaNacNueva;
        private RadioButtonList rblCategoriaNueva;
        private Componentes.DropdownList cbLoteNueva;
        private Label lblErrorNueva;
        private Componentes.DropdownList cbFechaExistente;
        private Componentes.DropdownList cbHoraExistente;
        private Componentes.DropdownList cbFechaNueva;
        private Componentes.DropdownList cbHoraNueva;

        // Modal 4: registrar ingreso
        private Literal litIngresoResumen;
        private Literal litIngresoAviso;
        private Panel pnlPendientes;
        private Componentes.Input inTelefonoIngreso;
        private Panel pnlFotoIngreso;
        private HiddenField hdnFotoBase64;
        private RadioButtonList rblVehiculo;
        private Panel pnlVehiculoNuevo;
        private Componentes.Input inPatenteNueva;
        private Componentes.Input inMarcaModelo;
        private Componentes.DropdownList cbCompania;
        private Componentes.Input inNroPoliza;
        private Componentes.Input inVigenciaPoliza;
        private Panel pnlLicencia;
        private Componentes.Input inNroLicencia;
        private Componentes.Input inVigenciaLicencia;
        private Label lblErrorIngreso;

        private T Ctl<T>(string id) where T : Control
        {
            return (T)ControlFinder.BuscarControl(this, id);
        }

        private void BuscarControles()
        {
            cbTipoDocBuscar = Ctl<Componentes.DropdownList>("cbTipoDocBuscar");
            inNroDocBuscar = Ctl<Componentes.Input>("inNroDocBuscar");
            lblErrorBuscar = Ctl<Label>("lblErrorBuscar");

            litExistenteNombre = Ctl<Literal>("litExistenteNombre");
            litExistenteDetalle = Ctl<Literal>("litExistenteDetalle");
            lblCategoriaExistente = Ctl<Label>("lblCategoriaExistente");
            rblCategoriaExistente = Ctl<RadioButtonList>("rblCategoriaExistente");
            cbLoteExistente = Ctl<Componentes.DropdownList>("cbLoteExistente");
            lblErrorExistente = Ctl<Label>("lblErrorExistente");
            btnPreacreditar = Ctl<Componentes.Button>("btnPreacreditar");

            litNuevaMensaje = Ctl<Literal>("litNuevaMensaje");
            litNuevaDocumento = Ctl<Literal>("litNuevaDocumento");
            cbPaisNueva = Ctl<Componentes.DropdownList>("cbPaisNueva");
            inApellidoNueva = Ctl<Componentes.Input>("inApellidoNueva");
            inNombresNueva = Ctl<Componentes.Input>("inNombresNueva");
            inFechaNacNueva = Ctl<Componentes.Input>("inFechaNacNueva");
            rblCategoriaNueva = Ctl<RadioButtonList>("rblCategoriaNueva");
            cbLoteNueva = Ctl<Componentes.DropdownList>("cbLoteNueva");
            lblErrorNueva = Ctl<Label>("lblErrorNueva");
            cbFechaExistente = Ctl<Componentes.DropdownList>("cbFechaExistente");
            cbHoraExistente = Ctl<Componentes.DropdownList>("cbHoraExistente");
            cbFechaNueva = Ctl<Componentes.DropdownList>("cbFechaNueva");
            cbHoraNueva = Ctl<Componentes.DropdownList>("cbHoraNueva");
            litIngresoResumen = Ctl<Literal>("litIngresoResumen");
            litIngresoAviso = Ctl<Literal>("litIngresoAviso");
            pnlPendientes = Ctl<Panel>("pnlPendientes");
            inTelefonoIngreso = Ctl<Componentes.Input>("inTelefonoIngreso");
            pnlFotoIngreso = Ctl<Panel>("pnlFotoIngreso");
            hdnFotoBase64 = Ctl<HiddenField>("hdnFotoBase64");
            rblVehiculo = Ctl<RadioButtonList>("rblVehiculo");
            pnlVehiculoNuevo = Ctl<Panel>("pnlVehiculoNuevo");
            inPatenteNueva = Ctl<Componentes.Input>("inPatenteNueva");
            inMarcaModelo = Ctl<Componentes.Input>("inMarcaModelo");
            cbCompania = Ctl<Componentes.DropdownList>("cbCompania");
            inNroPoliza = Ctl<Componentes.Input>("inNroPoliza");
            inVigenciaPoliza = Ctl<Componentes.Input>("inVigenciaPoliza");
            pnlLicencia = Ctl<Panel>("pnlLicencia");
            inNroLicencia = Ctl<Componentes.Input>("inNroLicencia");
            inVigenciaLicencia = Ctl<Componentes.Input>("inVigenciaLicencia");
            lblErrorIngreso = Ctl<Label>("lblErrorIngreso");
        }

        private T Estado<T>(string clave, T porDefecto)
        {
            object valor = ViewState[clave];
            return valor is T ? (T)valor : porDefecto;
        }

        private string NroDocumentoActual
        {
            get { return Estado("NroDocumentoActual", string.Empty); }
            set { ViewState["NroDocumentoActual"] = value; }
        }
        private int IdTipoDocumentoActual
        {
            get { return Estado("IdTipoDocumentoActual", 0); }
            set { ViewState["IdTipoDocumentoActual"] = value; }
        }
        private string TextoTipoDocumentoActual
        {
            get { return Estado("TextoTipoDocumentoActual", string.Empty); }
            set { ViewState["TextoTipoDocumentoActual"] = value; }
        }
        private int IdPersonaExistente
        {
            get { return Estado("IdPersonaExistente", 0); }
            set { ViewState["IdPersonaExistente"] = value; }
        }
        private int IdPreacreditadoActual
        {
            get { return Estado("IdPreacreditadoActual", 0); }
            set { ViewState["IdPreacreditadoActual"] = value; }
        }
        private int IdPersonaIngreso
        {
            get { return Estado("IdPersonaIngreso", 0); }
            set { ViewState["IdPersonaIngreso"] = value; }
        }
        private string NroDocumentoIngreso
        {
            get { return Estado("NroDocumentoIngreso", string.Empty); }
            set { ViewState["NroDocumentoIngreso"] = value; }
        }
        private bool LicenciaVigenteIngreso
        {
            get { return Estado("LicenciaVigenteIngreso", false); }
            set { ViewState["LicenciaVigenteIngreso"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            BuscarControles();

            if (!IsPostBack)
            {
                CargarCombos();
                CargarPreacreditados();
            }

            CargarIngresadas(buscarIngresadas.Input.Text);
        }

        private void CargarCombos()
        {
            Llenar(cbTipoDocBuscar.Combo, Tabla(Cuenta.ListarTiposDeDocumentos()), "descripcion", "id_tipo_documento");
            Llenar(cbPaisNueva.Combo, Tabla(Pais.ListarPaises()), "descripcion", "id_pais");

            Llenar(cbCompania.Combo, Tabla(CompaniaSeguro.ListarCompanias()), "descripcion", "id_compania_seguro");

            DataView categorias = new DataView(Tabla(Persona.ListarCategorias()));
            categorias.RowFilter = "id_categoria <> " + ID_CATEGORIA_PROPIETARIO;
            rblCategoriaNueva.DataSource = categorias;
            rblCategoriaNueva.DataTextField = "descripcion";
            rblCategoriaNueva.DataValueField = "id_categoria";
            rblCategoriaNueva.DataBind();

            DataView lotes = new DataView(Tabla(Lote.ListarLotes()));
            lotes.RowFilter = "id_persona_titular IS NOT NULL AND id_estado = " + ID_ESTADO_ACTIVO;
            Llenar(cbLoteExistente.Combo, lotes, "descripcion", "id_persona_titular");
            Llenar(cbLoteNueva.Combo, lotes, "descripcion", "id_persona_titular");

            CargarFechasYHoras(cbFechaExistente, cbHoraExistente);
            CargarFechasYHoras(cbFechaNueva, cbHoraNueva);
        }

        private static DataTable PreacreditadosHoy()
        {
            DataTable dt = Preacreditado.BuscarPreacreditadosHoy();

            if (!dt.Columns.Contains("Documento"))
                dt.Columns.Add("Documento", typeof(string));
            if (!dt.Columns.Contains("FranjaHoy"))
                dt.Columns.Add("FranjaHoy", typeof(string));

            foreach (DataRow fila in dt.Rows)
            {
                if (fila.IsNull("Documento") && dt.Columns.Contains("nro_documento"))
                    fila["Documento"] = Convert.ToString(fila["nro_documento"]);

                if (fila.IsNull("FranjaHoy"))
                {
                    bool hayFranja = dt.Columns.Contains("hora_desde") && dt.Columns.Contains("hora_hasta")
                        && fila["hora_desde"] is TimeSpan && fila["hora_hasta"] is TimeSpan;

                    fila["FranjaHoy"] = hayFranja
                        ? ((TimeSpan)fila["hora_desde"]).ToString(@"hh\:mm") + " a " + ((TimeSpan)fila["hora_hasta"]).ToString(@"hh\:mm")
                        : "Sin franja";
                }
            }

            return dt;
        }

        private void CargarPreacreditados()
        {
            try
            {
                DataTable dt = PreacreditadosHoy();
                rptPreacreditados.DataSource = dt;
                rptPreacreditados.DataBind();
                lblSinPreacreditados.Visible = dt.Rows.Count == 0;
            }
            catch (Exception ex)
            {
                MostrarResultado(ex.Message, false);
            }
        }

        private void CargarIngresadas(string texto)
        {
            try
            {
                DataTable origen = Persona.BuscarVisitasIngresadasHoy(texto);

                if (MOSTRAR_DIAGNOSTICO && origen.Rows.Count > 0)
                {
                    MostrarResultado("Columnas: " + string.Join(", ", origen.Columns.Cast<DataColumn>().Select(c => c.ColumnName))
                        + " | Primera fila: " + string.Join(" ; ", origen.Rows[0].ItemArray.Select(v => Convert.ToString(v))), true);
                }

                DataTable dt = NormalizarIngresadas(origen, texto, LeerEgresosHoy());

                var columnas = new System.Collections.Generic.List<Componentes.ColumnName>
                {
                    new Componentes.ColumnName { CampoDato = "Persona",   Titulo = "PERSONA" },
                    new Componentes.ColumnName { CampoDato = "Documento", Titulo = "DOCUMENTO" },
                    new Componentes.ColumnName { CampoDato = "Lote",      Titulo = "LOTE" },
                    new Componentes.ColumnName { CampoDato = "Categoria", Titulo = "CATEGORÍA" },
                    new Componentes.ColumnName { CampoDato = "Ingreso",   Titulo = "INGRESO" },
                    new Componentes.ColumnName { CampoDato = "Egreso",    Titulo = "EGRESO" },
                    new Componentes.ColumnName { CampoDato = "Estado",    Titulo = "ESTADO" }
                };

                tablaIngresadas.CargarDatosColumnas(dt, columnas);
            }
            catch (Exception ex)
            {
                MostrarResultado(ex.Message, false);
            }
        }

        private static DataTable NormalizarIngresadas(DataTable origen, string texto, DataTable egresos)
        {
            var vista = new DataTable();
            foreach (string nombre in new[] { "Persona", "Documento", "Lote", "Categoria", "Ingreso", "Egreso", "Estado" })
                vista.Columns.Add(nombre, typeof(string));

            string filtro = (texto ?? string.Empty).Trim();

            foreach (DataRow fila in origen.Rows)
            {
                string documento = ValorColumna(origen, fila, "Documento", "nro_documento", "NroDocumento", "DNI");

                if (filtro.Length > 0 && documento.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                string ingreso = ValorColumna(origen, fila, "Ingreso", "fecha_hora_ingreso", "FechaHoraIngreso");

                vista.Rows.Add(
                    ValorColumna(origen, fila, "Persona", "PersonaAutorizada", "NombreCompleto"),
                    documento,
                    ValorColumna(origen, fila, "Lote", "LoteDescripcion", "id_lote"),
                    CategoriaDeLaFila(origen, fila, documento),
                    ingreso,
                    EgresoDeLaFila(origen, fila, egresos, documento, ingreso),
                    ValorColumna(origen, fila, "Estado"));
            }

            return vista;
        }

        private static string EgresoDeLaFila(DataTable origen, DataRow fila, DataTable egresos, string documento, string ingreso)
        {
            string directo = ValorColumna(origen, fila, "Egreso", "fecha_hora_egreso", "FechaHoraEgreso");
            if (directo != "\u2014") return directo;

            foreach (DataColumn columna in origen.Columns)
            {
                string nombre = columna.ColumnName.ToLowerInvariant();
                if (nombre.Contains("egres") || nombre.Contains("salid"))
                {
                    string valor = ValorColumna(origen, fila, columna.ColumnName);
                    if (valor != "\u2014") return valor;
                }
            }

            if (egresos == null || documento == "\u2014") return "\u2014";

            var delDocumento = egresos.Rows.Cast<DataRow>()
                .Where(r => Convert.ToString(r["Documento"]).Trim() == documento)
                .ToList();
            if (delDocumento.Count == 0) return "\u2014";

            DataRow elegido = delDocumento.FirstOrDefault(r =>
                    Convert.ToDateTime(r["fecha_hora_ingreso"]).ToString("HH:mm") == ingreso)
                ?? (delDocumento.Count == 1 ? delDocumento[0] : null);

            return elegido == null
                ? "\u2014"
                : Convert.ToDateTime(elegido["fecha_hora_egreso"]).ToString("HH:mm");
        }

        private DataTable LeerEgresosHoy()
        {
            try
            {
                var dt = new DataTable();
                const string q = @"
                    SELECT c.nro_documento AS Documento,
                           m.fecha_hora_ingreso,
                           m.fecha_hora_egreso
                    FROM MOVIMIENTO m
                    INNER JOIN Persona p ON p.id_persona = m.id_persona
                    INNER JOIN Cuenta c  ON c.id_cuenta  = p.id_cuenta
                    WHERE m.fecha_hora_egreso IS NOT NULL
                      AND CAST(m.fecha_hora_ingreso AS date) = CAST(GETDATE() AS date);";

                using (var cn = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, cn))
                using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }

                return dt;
            }
            catch (Exception ex)
            {
                MostrarResultado("No se pudieron leer los egresos de hoy: " + ex.Message, false);
                return null;
            }
        }

        private static string CategoriaDeLaFila(DataTable origen, DataRow fila, string documento)
        {
            string directa = ValorColumna(origen, fila, "Categoria", "Categoría", "categoria");
            if (directa != "\u2014") return directa;

            if (documento == "\u2014") return "\u2014";

            try
            {
                DataTable persona = Persona.BuscarPersonaPorDocumento(documento);
                if (persona.Rows.Count == 0) return "\u2014";

                DataTable categorias = Persona.ListarCategoriasDePersona(Convert.ToInt32(persona.Rows[0]["id_persona"]));
                if (categorias.Rows.Count > 0)
                    return string.Join(", ", categorias.Rows.Cast<DataRow>().Select(r => Convert.ToString(r["descripcion"])));

                if (persona.Columns.Contains("Categoria"))
                    return Convert.ToString(persona.Rows[0]["Categoria"]);
            }
            catch (Exception)
            {
            }

            return "\u2014";
        }

        private static string ValorColumna(DataTable tabla, DataRow fila, params string[] candidatos)
        {
            foreach (string nombre in candidatos)
            {
                if (!tabla.Columns.Contains(nombre)) continue;

                object valor = fila[nombre];
                if (valor == null || valor == DBNull.Value) return "—";
                if (valor is DateTime) return ((DateTime)valor).ToString("HH:mm");

                string texto = Convert.ToString(valor);
                return texto.Length == 0 ? "—" : texto;
            }
            return "—";
        }

        protected void buscarIngresadas_BuscarClick(object sender, EventArgs e)
        {
        }

        protected string Iniciales(object nombreCompleto)
        {
            string[] partes = Convert.ToString(nombreCompleto).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string apellido = partes.Length > 0 ? partes[0].Trim() : string.Empty;
            string nombre = partes.Length > 1 ? partes[1].Trim() : string.Empty;

            string iniciales = (apellido.Length > 0 ? apellido.Substring(0, 1) : string.Empty)
                             + (nombre.Length > 0 ? nombre.Substring(0, 1) : string.Empty);
            return iniciales.ToUpper();
        }

        protected void btnNuevoPreacreditado_ButtonClick(object sender, EventArgs e)
        {
            AbrirBuscar();
        }

        protected void btnCambiarDocumento_ButtonClick(object sender, EventArgs e)
        {
            AbrirBuscar();
        }

        private void AbrirBuscar()
        {
            inNroDocBuscar.Value = string.Empty;
            lblErrorBuscar.Text = string.Empty;
            AbrirModal(modalBuscar);
        }

        protected void btnCancelar_ButtonClick(object sender, EventArgs e)
        {
        }

        protected void btnBuscarPersona_ButtonClick(object sender, EventArgs e)
        {
            lblErrorBuscar.Text = string.Empty;
            string nroDocumento = LimpiarDocumento(inNroDocBuscar.Value);

            if (nroDocumento.Length == 0 || string.IsNullOrEmpty(cbTipoDocBuscar.Combo.SelectedValue))
            {
                ErrorEnModal(lblErrorBuscar, "Elegí el tipo de documento e ingresá el número.", modalBuscar);
                return;
            }

            try
            {
                NroDocumentoActual = nroDocumento;
                IdTipoDocumentoActual = int.Parse(cbTipoDocBuscar.Combo.SelectedValue);
                TextoTipoDocumentoActual = cbTipoDocBuscar.Combo.SelectedItem.Text;

                DataTable dt = Persona.BuscarPersonaPorDocumento(nroDocumento);

                if (dt.Rows.Count == 0)
                    MostrarPersonaNueva();
                else
                    MostrarPersonaExistente(dt.Rows[0]);
            }
            catch (Exception ex)
            {
                ErrorEnModal(lblErrorBuscar, ex.Message, modalBuscar);
            }
        }

        private void MostrarPersonaExistente(DataRow fila)
        {
            IdPersonaExistente = Convert.ToInt32(fila["id_persona"]);

            litExistenteNombre.Text = Server.HtmlEncode(fila["apellido"] + ", " + fila["nombre"]);
            litExistenteDetalle.Text = Server.HtmlEncode(TextoTipoDocumentoActual + " " + NroDocumentoActual
                + " · " + fila["Nacionalidad"] + " · Categoría " + fila["Categoria"]);

            cbFechaExistente.Combo.ClearSelection();
            cbHoraExistente.Combo.ClearSelection();
            cbLoteExistente.Combo.ClearSelection();
            lblErrorExistente.Text = string.Empty;

            DataTable categorias = Persona.ListarCategoriasDePersona(IdPersonaExistente);

            if (categorias.Rows.Count == 0)
            {
                lblCategoriaExistente.Visible = false;
                rblCategoriaExistente.Visible = false;
                btnPreacreditar.Enabled = false;
                lblErrorExistente.Text = "Esta persona no tiene una categoría que requiera pre-acreditación (Visitante, Proveedor u Obrero).";
            }
            else
            {
                rblCategoriaExistente.DataSource = categorias;
                rblCategoriaExistente.DataTextField = "descripcion";
                rblCategoriaExistente.DataValueField = "id_categoria";
                rblCategoriaExistente.DataBind();
                rblCategoriaExistente.SelectedIndex = 0;

                bool variasCategorias = categorias.Rows.Count > 1;
                lblCategoriaExistente.Visible = variasCategorias;
                rblCategoriaExistente.Visible = variasCategorias;
                btnPreacreditar.Enabled = true;
            }

            AbrirModal(modalExistente);
        }

        protected void btnPreacreditar_ButtonClick(object sender, EventArgs e)
        {
            lblErrorExistente.Text = string.Empty;

            if (string.IsNullOrEmpty(cbLoteExistente.Combo.SelectedValue))
            {
                ErrorEnModal(lblErrorExistente, "Elegí el lote destino.", modalExistente);
                return;
            }

            DateTime fechaHora;
            if (!TryArmarFechaHora(cbFechaExistente.Combo, cbHoraExistente.Combo, out fechaHora))
            {
                ErrorEnModal(lblErrorExistente, MENSAJE_FECHA_HORA, modalExistente);
                return;
            }
            if (fechaHora < DateTime.Now)
            {
                ErrorEnModal(lblErrorExistente, "La fecha y hora no pueden ser anteriores a este momento.", modalExistente);
                return;
            }

            int idCategoria = rblCategoriaExistente.Items.Count == 1
                ? int.Parse(rblCategoriaExistente.Items[0].Value)
                : int.Parse(rblCategoriaExistente.SelectedValue);
            int idAutorizante = int.Parse(cbLoteExistente.Combo.SelectedValue);

            try
            {
                Preacreditado.CrearPreacreditacion(IdPersonaExistente, idAutorizante, idCategoria, fechaHora);

                MostrarResultado("Pre-acreditación creada correctamente.", true);
                CargarPreacreditados();
            }
            catch (Exception ex)
            {
                ErrorEnModal(lblErrorExistente, ex.Message, modalExistente);
            }
        }

        private void MostrarPersonaNueva()
        {
            string documento = TextoTipoDocumentoActual + " " + NroDocumentoActual;

            litNuevaMensaje.Text = Server.HtmlEncode("No hay ninguna persona con " + documento + ". Cargá sus datos para pre-acreditarla.");
            litNuevaDocumento.Text = Server.HtmlEncode(documento);

            cbPaisNueva.Combo.ClearSelection();
            inApellidoNueva.Value = string.Empty;
            inNombresNueva.Value = string.Empty;
            inFechaNacNueva.Value = string.Empty;
            rblCategoriaNueva.ClearSelection();
            cbLoteNueva.Combo.ClearSelection();
            cbFechaNueva.Combo.ClearSelection();
            cbHoraNueva.Combo.ClearSelection();
            lblErrorNueva.Text = string.Empty;

            AbrirModal(modalNueva);
        }

        protected void btnCrearYPreacreditar_ButtonClick(object sender, EventArgs e)
        {
            lblErrorNueva.Text = string.Empty;

            string apellido = inApellidoNueva.Value.Trim();
            string nombres = inNombresNueva.Value.Trim();

            if (string.IsNullOrEmpty(cbPaisNueva.Combo.SelectedValue) || apellido.Length == 0 || nombres.Length == 0)
            {
                ErrorEnModal(lblErrorNueva, "Completá nacionalidad, apellido y nombres.", modalNueva);
                return;
            }

            DateTime fechaNacimiento;
            if (!DateTime.TryParse(inFechaNacNueva.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaNacimiento))
            {
                ErrorEnModal(lblErrorNueva, "Ingresá una fecha de nacimiento válida.", modalNueva);
                return;
            }

            if (rblCategoriaNueva.SelectedIndex < 0)
            {
                ErrorEnModal(lblErrorNueva, "Elegí la categoría de la visita.", modalNueva);
                return;
            }

            if (string.IsNullOrEmpty(cbLoteNueva.Combo.SelectedValue))
            {
                ErrorEnModal(lblErrorNueva, "Elegí el lote destino.", modalNueva);
                return;
            }

            DateTime fechaHora;
            if (!TryArmarFechaHora(cbFechaNueva.Combo, cbHoraNueva.Combo, out fechaHora))
            {
                ErrorEnModal(lblErrorNueva, MENSAJE_FECHA_HORA, modalNueva);
                return;
            }
            if (fechaHora < DateTime.Now)
            {
                ErrorEnModal(lblErrorNueva, "La fecha y hora no pueden ser anteriores a este momento.", modalNueva);
                return;
            }

            try
            {
                Preacreditado.CrearPersonaYPreacreditar(
                    nombres,
                    apellido,
                    int.Parse(cbPaisNueva.Combo.SelectedValue),
                    string.Empty,
                    fechaNacimiento,
                    IdTipoDocumentoActual,
                    NroDocumentoActual,
                    string.Empty,
                    int.Parse(rblCategoriaNueva.SelectedValue),
                    int.Parse(cbLoteNueva.Combo.SelectedValue),
                    fechaHora);

                MostrarResultado("Persona creada y pre-acreditada correctamente.", true);
                CargarPreacreditados();
            }
            catch (Exception ex)
            {
                ErrorEnModal(lblErrorNueva, ex.Message, modalNueva);
            }
        }

        private static void CargarFechasYHoras(Componentes.DropdownList cbFecha, Componentes.DropdownList cbHora)
        {
            var fechas = new DataTable();
            fechas.Columns.Add("texto", typeof(string));
            fechas.Columns.Add("valor", typeof(string));
            for (int i = 0; i <= DIAS_FUTUROS_MAX; i++)
            {
                DateTime dia = DateTime.Today.AddDays(i);
                string texto = dia.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + (i == 0 ? " (hoy)" : string.Empty);
                fechas.Rows.Add(texto, dia.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            }
            Llenar(cbFecha.Combo, fechas, "texto", "valor");

            var horas = new DataTable();
            horas.Columns.Add("texto", typeof(string));
            horas.Columns.Add("valor", typeof(string));
            for (int min = 0; min < 24 * 60; min += MINUTOS_PASO_HORA)
            {
                string hhmm = TimeSpan.FromMinutes(min).ToString(@"hh\:mm");
                horas.Rows.Add(hhmm.Replace(":", string.Empty), hhmm);
            }
            Llenar(cbHora.Combo, horas, "texto", "valor");
        }

        private static bool TryArmarFechaHora(DropDownList cbFecha, DropDownList cbHora, out DateTime fechaHora)
        {
            fechaHora = DateTime.MinValue;
            if (string.IsNullOrEmpty(cbFecha.SelectedValue) || string.IsNullOrEmpty(cbHora.SelectedValue))
                return false;

            return DateTime.TryParseExact(cbFecha.SelectedValue + " " + cbHora.SelectedValue, "yyyy-MM-dd HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaHora);
        }

        protected void btnRegistrarIngreso_ButtonClick(object sender, EventArgs e)
        {
            var boton = (Componentes.Button)sender;
            var item = (RepeaterItem)boton.NamingContainer;
            int idPreacreditado = int.Parse(((HiddenField)item.FindControl("hidIdPreacreditado")).Value);

            AbrirIngreso(idPreacreditado);
        }

        private void AbrirIngreso(int idPreacreditado)
        {
            try
            {
                DataRow[] filas = PreacreditadosHoy().Select("id_preacreditado = " + idPreacreditado);
                if (filas.Length == 0)
                {
                    MostrarResultado("Esa pre-acreditación ya no está disponible.", false);
                    CargarPreacreditados();
                    return;
                }

                DataRow fila = filas[0];

                if (Convert.ToDateTime(fila["fecha_hora"]).Date != DateTime.Today)
                {
                    MostrarResultado("Esta pre-acreditación es para otro día: el ingreso solo se registra el día de la visita.", false);
                    return;
                }

                IdPreacreditadoActual = idPreacreditado;
                IdPersonaIngreso = Convert.ToInt32(fila["id_persona_autorizada"]);
                NroDocumentoIngreso = Convert.ToString(fila["Documento"]);

                litIngresoResumen.Text = Server.HtmlEncode(fila["PersonaAutorizada"] + " · Doc. " + fila["Documento"]
                    + " · " + fila["LoteDescripcion"] + " · " + fila["Categoria"] + " · " + fila["FranjaHoy"]);

                lblErrorIngreso.Text = string.Empty;
                inTelefonoIngreso.Value = string.Empty;
                inPatenteNueva.Value = string.Empty;
                inMarcaModelo.Value = string.Empty;
                cbCompania.Combo.ClearSelection();
                inNroPoliza.Value = string.Empty;
                inVigenciaPoliza.Value = string.Empty;
                inNroLicencia.Value = string.Empty;
                inVigenciaLicencia.Value = string.Empty;
                if (hdnFotoBase64 != null) hdnFotoBase64.Value = string.Empty;

                bool faltaTelefono = string.IsNullOrWhiteSpace(Convert.ToString(fila["telefono"]));
                bool faltaFoto = string.IsNullOrWhiteSpace(Convert.ToString(fila["foto_referencia"]));

                inTelefonoIngreso.Visible = faltaTelefono;
                pnlFotoIngreso.Visible = faltaFoto;
                pnlPendientes.Visible = faltaTelefono || faltaFoto;
                litIngresoAviso.Text = pnlPendientes.Visible
                    ? "Esta persona llegó pre-acreditada con lo mínimo. Completá lo que falta con lo que ves en la garita antes de registrar el ingreso."
                    : "Los datos de la persona ya están completos. Solo indicá con qué vehículo ingresa.";

                DataTable vehiculos = Vehiculo.ListarVehiculosDePersona(IdPersonaIngreso);
                rblVehiculo.Items.Clear();
                foreach (DataRow v in vehiculos.Rows)
                    rblVehiculo.Items.Add(new ListItem(TextoVehiculo(v), Convert.ToString(v["patente"])));

                rblVehiculo.Items.Add(new ListItem(
                    vehiculos.Rows.Count > 0 ? "Otro vehículo (se cargan sus datos)" : "Ingresa con vehículo",
                    VALOR_OTRO));
                rblVehiculo.Items.Add(new ListItem("Ingresa a pie", VALOR_A_PIE));
                rblVehiculo.SelectedValue = VALOR_A_PIE;

                LicenciaVigenteIngreso = Persona.TieneLicenciaVigente(IdPersonaIngreso);
                AplicarVisibilidadVehiculo();

                AbrirModal(modalIngreso);
            }
            catch (Exception ex)
            {
                MostrarResultado(ex.Message, false);
            }
        }

        private static string TextoVehiculo(DataRow v)
        {
            string estado = Convert.ToString(v["EstadoPoliza"]);
            string poliza = estado == "Sin poliza"
                ? "sin póliza"
                : "póliza " + estado.ToLower() + " " + Convert.ToDateTime(v["vigencia_poliza_seguro"]).ToString("MM/yyyy");

            return v["patente"] + " · " + v["descripcion"] + " — " + poliza;
        }

        private string SeleccionVehiculo()
        {
            return string.IsNullOrEmpty(rblVehiculo.SelectedValue) ? VALOR_A_PIE : rblVehiculo.SelectedValue;
        }

        private void AplicarVisibilidadVehiculo()
        {
            string seleccion = SeleccionVehiculo();
            pnlVehiculoNuevo.Visible = seleccion == VALOR_OTRO;
            pnlLicencia.Visible = seleccion != VALOR_A_PIE && !LicenciaVigenteIngreso;
        }

        protected void rblVehiculo_SelectedIndexChanged(object sender, EventArgs e)
        {
            AplicarVisibilidadVehiculo();
            AbrirModal(modalIngreso);
        }

        protected void btnConfirmarIngreso_ButtonClick(object sender, EventArgs e)
        {
            lblErrorIngreso.Text = string.Empty;

            string seleccion = SeleccionVehiculo();
            bool aPie = seleccion == VALOR_A_PIE;
            bool eligioVehiculoExistente = !aPie && seleccion != VALOR_OTRO;

            string patenteNueva = inPatenteNueva.Value.Trim();
            DateTime vigenciaPoliza = DateTime.MinValue;
            DateTime vigenciaLicencia = DateTime.MinValue;

            try
            {
                bool patenteEsNueva = false;
                if (!aPie && !eligioVehiculoExistente)
                {
                    if (patenteNueva.Length == 0)
                    {
                        ErrorEnModal(lblErrorIngreso, "Ingresá la patente.", modalIngreso);
                        return;
                    }

                    patenteEsNueva = !Vehiculo.ExistePatente(patenteNueva);
                    if (patenteEsNueva)
                    {
                        if (inMarcaModelo.Value.Trim().Length == 0
                            || string.IsNullOrEmpty(cbCompania.Combo.SelectedValue)
                            || inNroPoliza.Value.Trim().Length == 0
                            || !DateTime.TryParse(inVigenciaPoliza.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out vigenciaPoliza))
                        {
                            ErrorEnModal(lblErrorIngreso, "Para un vehículo nuevo son obligatorios: marca y modelo, compañía de seguro, número de póliza y vigencia.", modalIngreso);
                            return;
                        }
                    }
                }

                bool pedirLicencia = !aPie && !Persona.TieneLicenciaVigente(IdPersonaIngreso);
                if (pedirLicencia
                    && (inNroLicencia.Value.Trim().Length == 0
                        || !DateTime.TryParse(inVigenciaLicencia.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out vigenciaLicencia)))
                {
                    ErrorEnModal(lblErrorIngreso, "El número y la vigencia de la licencia son obligatorios.", modalIngreso);
                    return;
                }

                int idUsuario = Convert.ToInt32(Session["Id_usuario"]);
                int idGarita = Convert.ToInt32(Session["IdGarita"]);

                string telefono = inTelefonoIngreso.Visible && !string.IsNullOrWhiteSpace(inTelefonoIngreso.Value)
                    ? inTelefonoIngreso.Value.Trim()
                    : null;
                string rutaFoto = GuardarFotoYObtenerRuta();

                if (telefono != null || rutaFoto != null)
                    Persona.ActualizarDatosDeContacto(IdPersonaIngreso, telefono, rutaFoto);

                DataRow resultado;
                if (aPie)
                {
                    resultado = Movimiento.IntentarAccederSinVehiculo(NroDocumentoIngreso, idUsuario, idGarita, null);
                }
                else
                {
                    string patenteFinal = seleccion;
                    if (!eligioVehiculoExistente)
                    {
                        patenteFinal = patenteNueva;
                        if (patenteEsNueva)
                        {
                            int idVehiculo = Vehiculo.DarDeAltaVehiculo(patenteNueva, inMarcaModelo.Value.Trim(), IdPersonaIngreso);
                            Vehiculo.DarDeAltaPolizaSeguro(idVehiculo, int.Parse(cbCompania.Combo.SelectedValue),
                                inNroPoliza.Value.Trim(), vigenciaPoliza);
                        }
                    }

                    if (pedirLicencia)
                        Persona.DarDeAltaLicencia(IdPersonaIngreso, inNroLicencia.Value.Trim(), vigenciaLicencia);

                    resultado = Movimiento.IntentarAccederConVehiculo(patenteFinal, NroDocumentoIngreso, idUsuario, idGarita, null);
                }

                if (resultado == null)
                {
                    ErrorEnModal(lblErrorIngreso, "El sistema no devolvió una respuesta. Probá de nuevo.", modalIngreso);
                    return;
                }

                string mensaje = Convert.ToString(resultado["mensaje"]);
                if (Convert.ToInt32(resultado["codigo"]) == 0)
                {
                    Preacreditado.MarcarIngresado(IdPreacreditadoActual);

                    MostrarResultado(mensaje, true);
                    CargarPreacreditados();
                    CargarIngresadas(buscarIngresadas.Input.Text);
                }
                else
                {
                    ErrorEnModal(lblErrorIngreso, mensaje, modalIngreso);
                }
            }
            catch (Exception ex)
            {
                ErrorEnModal(lblErrorIngreso, ex.Message, modalIngreso);
            }
        }

        private string GuardarFotoYObtenerRuta()
        {
            if (hdnFotoBase64 == null)
            {
                return null;
            }

            string dataUrl = hdnFotoBase64.Value;

            if (string.IsNullOrWhiteSpace(dataUrl))
            {
                return null;
            }

            if (!dataUrl.StartsWith("data:image/") || !dataUrl.Contains(";base64,"))
            {
                throw new Exception("La foto tiene un formato inválido.");
            }

            return dataUrl;
        }

        private static DataTable Tabla(object resultado)
        {
            DataSet ds = resultado as DataSet;
            return ds != null ? ds.Tables[0] : (DataTable)resultado;
        }

        private static void Llenar(DropDownList combo, object origen, string campoTexto, string campoValor)
        {
            combo.DataSource = origen;
            combo.DataTextField = campoTexto;
            combo.DataValueField = campoValor;
            combo.DataBind();
            combo.Items.Insert(0, new ListItem("Seleccioná...", string.Empty));
        }

        private static string LimpiarDocumento(string texto)
        {
            return (texto ?? string.Empty).Replace(".", string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty).Trim();
        }

        private void ErrorEnModal(Label etiqueta, string mensaje, Componentes.Modal modal)
        {
            etiqueta.Text = Server.HtmlEncode(mensaje);
            AbrirModal(modal);
        }

        private void MostrarResultado(string mensaje, bool ok)
        {
            lblResultado.Text = Server.HtmlEncode(mensaje);
            lblResultado.ForeColor = ok
                ? System.Drawing.Color.FromArgb(21, 128, 61)
                : System.Drawing.Color.FromArgb(185, 28, 28);
            lblResultado.Visible = true;
        }

        private void AbrirModal(Componentes.Modal modal)
        {
            string js = "(function(){"
                + " var abrir = function(){ mostrarModal('" + modal.OverlayClientID + "'); };"
                + " if (document.readyState === 'complete') { abrir(); }"
                + " else { window.addEventListener('load', abrir); }"
                + "})();";

            ClientScript.RegisterStartupScript(GetType(), "abrir_" + modal.ID, js, true);
        }
    }
}