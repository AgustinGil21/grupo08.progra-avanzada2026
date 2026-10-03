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
        // Diagnóstico temporal: muestra arriba las columnas que devuelve Persona.BuscarVisitasIngresadasHoy
        // y los valores de la primera fila. Dejar en false cuando ya no haga falta.
        private const bool MOSTRAR_DIAGNOSTICO = false;

        private const string MENSAJE_FECHA_HORA = "Ingresá la fecha y hora con el formato dd/mm/aaaa HHmm (ej.: 05/08/2026 0800).";

        // ------------------------------------------------------------------
        // Controles que viven dentro de un ITemplate (uc:Modal > Content).
        // No generan campo en el .designer.cs: se buscan con ControlFinder
        // al principio de Page_Load (ver BuscarControles).
        // ------------------------------------------------------------------

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
        private Componentes.Input inFechaHoraExistente;
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
        private Componentes.Input inFechaHoraNueva;
        private Label lblErrorNueva;

        // Modal 4: registrar ingreso
        private Literal litIngresoResumen;
        private Literal litIngresoAviso;
        private Panel pnlPendientes;
        private Componentes.Input inTelefonoIngreso;
        private Panel pnlFotoIngreso;
        private HiddenField hdnFotoBase64;          // viene del bloque de foto (puede ser null si todavía no se pegó)
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
        private Componentes.Input inNotaIngreso;
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
            inFechaHoraExistente = Ctl<Componentes.Input>("inFechaHoraExistente");
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
            inFechaHoraNueva = Ctl<Componentes.Input>("inFechaHoraNueva");
            lblErrorNueva = Ctl<Label>("lblErrorNueva");

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
            inNotaIngreso = Ctl<Componentes.Input>("inNotaIngreso");
            lblErrorIngreso = Ctl<Label>("lblErrorIngreso");
        }

        // ------------------------------------------------------------------
        // Estado que tiene que sobrevivir entre postbacks
        // ------------------------------------------------------------------

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
        private int IdLoteIngreso
        {
            get { return Estado("IdLoteIngreso", 0); }
            set { ViewState["IdLoteIngreso"] = value; }
        }
        private bool LicenciaVigenteIngreso
        {
            get { return Estado("LicenciaVigenteIngreso", false); }
            set { ViewState["LicenciaVigenteIngreso"] = value; }
        }

        // ------------------------------------------------------------------
        // Carga de la pantalla
        // ------------------------------------------------------------------

        protected void Page_Load(object sender, EventArgs e)
        {
            BuscarControles();

            if (!IsPostBack)
            {
                CargarCombos();
                CargarPreacreditados();
            }

            // uc:Table arma las celdas en ItemDataBound, así que no sobreviven a un postback:
            // se vuelve a cargar en cada request, con el texto que haya en el buscador.
            CargarIngresadas(buscarIngresadas.Input.Text);
        }

        private void CargarCombos()
        {
            Llenar(cbTipoDocBuscar.Combo, Tabla(Cuenta.ListarTiposDeDocumentos()), "descripcion", "id_tipo_documento");
            Llenar(cbPaisNueva.Combo, Tabla(Pais.ListarPaises()), "descripcion", "id_pais");

            Llenar(cbCompania.Combo, Tabla(CompaniaSeguro.ListarCompanias()), "descripcion", "id_compania_seguro");

            // Categorías para persona nueva: todas menos Propietario (un propietario no necesita pre-acreditación)
            DataView categorias = new DataView(Tabla(Persona.ListarCategorias()));
            categorias.RowFilter = "id_categoria <> " + ID_CATEGORIA_PROPIETARIO;
            rblCategoriaNueva.DataSource = categorias;
            rblCategoriaNueva.DataTextField = "descripcion";
            rblCategoriaNueva.DataValueField = "id_categoria";
            rblCategoriaNueva.DataBind();

            // Lotes: solo los activos y con titular. El valor del combo es el id de la persona
            // titular, porque Preacreditado guarda al autorizante, no el lote (el lote se deduce
            // del titular). id_estado = 1 (Activo) excluye, entre otros, los lotes Bloqueados.
            DataView lotes = new DataView(Tabla(Lote.ListarLotes()));
            lotes.RowFilter = "id_persona_titular IS NOT NULL AND id_estado = " + ID_ESTADO_ACTIVO;
            Llenar(cbLoteExistente.Combo, lotes, "descripcion", "id_persona_titular");
            Llenar(cbLoteNueva.Combo, lotes, "descripcion", "id_persona_titular");
        }

        // >>> ARREGLO 3: columnas Documento y FranjaHoy que la pantalla necesita
        // Preacreditado.BuscarPreacreditadosHoy() devuelve nro_documento, hora_desde y hora_hasta.
        // La pantalla usa "Documento" y "FranjaHoy": se arman acá para no tocar BIZ.
        // Si BIZ ya trae esas columnas, no se pisan.
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
        // <<< FIN ARREGLO 3

        private void CargarPreacreditados()
        {
            try
            {
                DataTable dt = PreacreditadosHoy();   // [ARREGLO 3: usa el helper]
                rptPreacreditados.DataSource = dt;
                rptPreacreditados.DataBind();
                lblSinPreacreditados.Visible = dt.Rows.Count == 0;
            }
            catch (Exception ex)
            {
                MostrarResultado(ex.Message, false);
            }
        }

        // Tabla "Visitas ya ingresadas hoy".
        // NormalizarIngresadas arma las 7 columnas que muestra la pantalla sin depender de cómo se
        // llamen en la consulta de BIZ. Antes, si la consulta no devolvía alguna (por ejemplo
        // "Documento"), uc:Table tiraba una excepción en esa celda y la fila quedaba cortada.
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

                // Filtro por documento: el buscador. Si la consulta de BIZ ya filtró, esto no cambia nada.
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

        // Hora de egreso, en 3 pasos:
        // 1) la columna de la consulta de BIZ (Egreso, fecha_hora_egreso...);
        // 2) cualquier columna cuyo nombre contenga "egres" o "salid";
        // 3) la tabla MOVIMIENTO, buscando por documento y por la hora de ingreso (LeerEgresosHoy, más abajo).
        // Si en ningún lado hay una hora, queda "—": significa que el egreso no guardó la hora.
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

            // Mismo ingreso (por hora); si no coincide y hay un solo egreso de esa persona hoy, ese.
            DataRow elegido = delDocumento.FirstOrDefault(r =>
                    Convert.ToDateTime(r["fecha_hora_ingreso"]).ToString("HH:mm") == ingreso)
                ?? (delDocumento.Count == 1 ? delDocumento[0] : null);

            return elegido == null
                ? "\u2014"
                : Convert.ToDateTime(elegido["fecha_hora_egreso"]).ToString("HH:mm");
        }

        // Lee los egresos de hoy directo de MOVIMIENTO, desde esta pantalla: no hace falta tocar BIZ.
        // Usa Conexion.AbrirConexion() (ya devuelve la conexión abierta) y solo columnas que BIZ ya usa:
        // Movimiento (id_persona, fecha_hora_ingreso, fecha_hora_egreso), Persona.id_cuenta, Cuenta.nro_documento.
        // Si la consulta falla, muestra el motivo arriba y la tabla sigue sin esa columna.
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

        // Categoría de la persona en la tabla. Primero usa la columna de la consulta de BIZ (si existe
        // y trae algo); si no, la busca por documento con los métodos que ya usa la pantalla
        // (Persona.BuscarPersonaPorDocumento y Persona.ListarCategoriasDePersona, sin Propietario).
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
                // Si falla la búsqueda de categoría no se rompe la tabla: queda "—".
            }

            return "\u2014";
        }

        // Devuelve el valor de la primera columna que exista entre los candidatos; "—" si no hay ninguna o está vacía.
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
            // Nada que hacer: Page_Load ya recargó la tabla con el texto que se tipeó.
        }

        // Iniciales para el avatar de la card. El nombre viene como "Apellido, Nombre".
        protected string Iniciales(object nombreCompleto)
        {
            string[] partes = Convert.ToString(nombreCompleto).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string apellido = partes.Length > 0 ? partes[0].Trim() : string.Empty;
            string nombre = partes.Length > 1 ? partes[1].Trim() : string.Empty;

            string iniciales = (apellido.Length > 0 ? apellido.Substring(0, 1) : string.Empty)
                             + (nombre.Length > 0 ? nombre.Substring(0, 1) : string.Empty);
            return iniciales.ToUpper();
        }

        // ------------------------------------------------------------------
        // Nuevo pre-acreditado — Paso 1: buscar a la persona
        // ------------------------------------------------------------------

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

        // Todos los "Cancelar": al volver la página los modales quedan cerrados
        // (se abren solo por JavaScript), así que no hay nada más que hacer.
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

                // 0 filas = la persona no existe (mismo contrato que VerificarDni)
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

        // ------------------------------------------------------------------
        // Paso 2 (persona existente): solo lote y fecha/hora
        // ------------------------------------------------------------------

        private void MostrarPersonaExistente(DataRow fila)
        {
            IdPersonaExistente = Convert.ToInt32(fila["id_persona"]);

            litExistenteNombre.Text = Server.HtmlEncode(fila["apellido"] + ", " + fila["nombre"]);
            litExistenteDetalle.Text = Server.HtmlEncode(TextoTipoDocumentoActual + " " + NroDocumentoActual
                + " · " + fila["Nacionalidad"] + " · Categoría " + fila["Categoria"]);

            inFechaHoraExistente.Value = string.Empty;
            cbLoteExistente.Combo.ClearSelection();
            lblErrorExistente.Text = string.Empty;

            // Categorías válidas de la persona (sin Propietario): 0, 1 o varias
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

                // Con una sola categoría se usa directo; con varias, el guardia elige
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
            if (!TryParseFechaHora(inFechaHoraExistente.Value, out fechaHora))
            {
                ErrorEnModal(lblErrorExistente, MENSAJE_FECHA_HORA, modalExistente);
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

        // ------------------------------------------------------------------
        // Paso 2 (persona nueva): alta rápida + pre-acreditación
        // ------------------------------------------------------------------

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
            inFechaHoraNueva.Value = string.Empty;
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
            if (!TryParseFechaHora(inFechaHoraNueva.Value, out fechaHora))
            {
                ErrorEnModal(lblErrorNueva, MENSAJE_FECHA_HORA, modalNueva);
                return;
            }

            try
            {
                // >>> ARREGLO 2: error "idPersonaAutorizante" y "@telefono not supplied"
                // 11 parámetros: es la firma que tiene Preacreditado.CrearPersonaYPreacreditar.
                // Teléfono y foto van vacíos: se completan al registrar el ingreso en la garita.
                // OJO: no pasar null. AddWithValue con null no envía el parámetro y SQL Server
                // dice "expects the parameter '@telefono', which was not supplied".
                // Vacío cuenta como "falta" en AbrirIngreso (IsNullOrWhiteSpace), así que se vuelve a pedir.
                Preacreditado.CrearPersonaYPreacreditar(
                    nombres,
                    apellido,
                    int.Parse(cbPaisNueva.Combo.SelectedValue),
                    string.Empty,                           // telefono
                    fechaNacimiento,
                    IdTipoDocumentoActual,
                    NroDocumentoActual,
                    string.Empty,                           // fotoReferencia
                    int.Parse(rblCategoriaNueva.SelectedValue),
                    int.Parse(cbLoteNueva.Combo.SelectedValue),
                    fechaHora);
                // <<< FIN ARREGLO 2

                MostrarResultado("Persona creada y pre-acreditada correctamente.", true);
                CargarPreacreditados();
            }
            catch (Exception ex)
            {
                ErrorEnModal(lblErrorNueva, ex.Message, modalNueva);
            }
        }

        // ------------------------------------------------------------------
        // Registrar ingreso (botón de cada card)
        // ------------------------------------------------------------------

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
                DataRow[] filas = PreacreditadosHoy().Select("id_preacreditado = " + idPreacreditado);   // [ARREGLO 3: usa el helper]
                if (filas.Length == 0)
                {
                    MostrarResultado("Esa pre-acreditación ya no está disponible.", false);
                    CargarPreacreditados();
                    return;
                }
                DataRow fila = filas[0];

                IdPreacreditadoActual = idPreacreditado;
                IdPersonaIngreso = Convert.ToInt32(fila["id_persona_autorizada"]);
                NroDocumentoIngreso = Convert.ToString(fila["Documento"]);
                IdLoteIngreso = Convert.ToInt32(fila["Lote"]);   // id_lote: es lo que espera el SP

                litIngresoResumen.Text = Server.HtmlEncode(fila["PersonaAutorizada"] + " · Doc. " + fila["Documento"]
                    + " · " + fila["LoteDescripcion"] + " · " + fila["Categoria"] + " · " + fila["FranjaHoy"]);

                // Limpiar lo que haya quedado de una vez anterior
                lblErrorIngreso.Text = string.Empty;
                inTelefonoIngreso.Value = string.Empty;
                inNotaIngreso.Value = string.Empty;
                inPatenteNueva.Value = string.Empty;
                inMarcaModelo.Value = string.Empty;
                cbCompania.Combo.ClearSelection();
                inNroPoliza.Value = string.Empty;
                inVigenciaPoliza.Value = string.Empty;
                inNroLicencia.Value = string.Empty;
                inVigenciaLicencia.Value = string.Empty;
                if (hdnFotoBase64 != null) hdnFotoBase64.Value = string.Empty;

                // Datos pendientes: solo se pide lo que falta
                bool faltaTelefono = string.IsNullOrWhiteSpace(Convert.ToString(fila["telefono"]));
                bool faltaFoto = string.IsNullOrWhiteSpace(Convert.ToString(fila["foto_referencia"]));

                inTelefonoIngreso.Visible = faltaTelefono;
                pnlFotoIngreso.Visible = faltaFoto;
                pnlPendientes.Visible = faltaTelefono || faltaFoto;
                litIngresoAviso.Text = pnlPendientes.Visible
                    ? "Esta persona llegó pre-acreditada con lo mínimo. Completá lo que falta con lo que ves en la garita antes de registrar el ingreso."
                    : "Los datos de la persona ya están completos. Solo indicá con qué vehículo ingresa.";

                // Vehículo: sus vehículos registrados + "otro" + "a pie"
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
                // 1. Validar TODO antes de escribir en la base o en el disco
                bool patenteEsNueva = false;
                if (!aPie && !eligioVehiculoExistente)
                {
                    if (patenteNueva.Length == 0)
                    {
                        ErrorEnModal(lblErrorIngreso, "Ingresá la patente.", modalIngreso);
                        return;
                    }

                    // Si la patente ya existe (de esta u otra persona) se usa directo: el SP valida su póliza
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
                string nota = string.IsNullOrWhiteSpace(inNotaIngreso.Value) ? null : inNotaIngreso.Value.Trim();

                // 2. Datos pendientes (null = no se completó; ActualizarDatosDeContacto conserva lo que ya había)
                string telefono = inTelefonoIngreso.Visible && !string.IsNullOrWhiteSpace(inTelefonoIngreso.Value)
                    ? inTelefonoIngreso.Value.Trim()
                    : null;
                string rutaFoto = GuardarFotoYObtenerRuta();

                if (telefono != null || rutaFoto != null)
                    Persona.ActualizarDatosDeContacto(IdPersonaIngreso, telefono, rutaFoto);

                // 3. Vehículo, licencia y acceso
                DataRow resultado;
                if (aPie)
                {
                    resultado = Movimiento.IntentarAccederSinVehiculo(NroDocumentoIngreso, idUsuario, idGarita, IdLoteIngreso, nota, true);
                }
                else
                {
                    string patenteFinal = seleccion;   // patente del vehículo elegido de la lista
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

                    resultado = Movimiento.IntentarAccederConVehiculo(patenteFinal, NroDocumentoIngreso, idUsuario, idGarita, IdLoteIngreso, nota, true);
                }

                // 4. Resultado del SP (codigo 0 = ingreso registrado)
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
                    // Rechazado: el mensaje ya viene en español y explica el motivo
                    ErrorEnModal(lblErrorIngreso, mensaje, modalIngreso);
                }
            }
            catch (Exception ex)
            {
                ErrorEnModal(lblErrorIngreso, ex.Message, modalIngreso);
            }
        }

        // Copiado de NuevaPersona.aspx.cs (con guarda por si el bloque de foto todavía no está en el .aspx)
        private string GuardarFotoYObtenerRuta()
        {
            if (hdnFotoBase64 == null)
            {
                return null;
            }

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

        // ------------------------------------------------------------------
        // Utilidades
        // ------------------------------------------------------------------

        // Los métodos de BIZ pueden devolver DataSet o DataTable: esto acepta cualquiera de los dos.
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

        // "36.480.215" -> "36480215" (en la base el documento se guarda sin puntos ni espacios)
        private static string LimpiarDocumento(string texto)
        {
            return (texto ?? string.Empty).Replace(".", string.Empty).Replace(" ", string.Empty).Replace("-", string.Empty).Trim();
        }

        private static bool TryParseFechaHora(string texto, out DateTime fechaHora)
        {
            return DateTime.TryParseExact((texto ?? string.Empty).Trim(), "dd/MM/yyyy HHmm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaHora);
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

        // >>> ARREGLO 1: los botones "no hacían nada" (el modal se abría antes de existir mostrarModal)
        // Modal.ascx expone OverlayClientID. mostrarModal(id) está definido en Site.Master,
        // DESPUÉS del </form>, y un startup script se escribe ANTES del </form>: si se llamara
        // directo, la función todavía no existiría. Por eso se espera al evento load.
        private void AbrirModal(Componentes.Modal modal)
        {
            string js = "(function(){"
                + " var abrir = function(){ mostrarModal('" + modal.OverlayClientID + "'); };"
                + " if (document.readyState === 'complete') { abrir(); }"
                + " else { window.addEventListener('load', abrir); }"
                + "})();";

            ClientScript.RegisterStartupScript(GetType(), "abrir_" + modal.ID, js, true);
        }
        // <<< FIN ARREGLO 1
    }
}
