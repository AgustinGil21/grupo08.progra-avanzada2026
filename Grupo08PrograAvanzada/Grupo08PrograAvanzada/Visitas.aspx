<%@ Page Title="Visitas" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Visitas.aspx.cs" Inherits="Grupo08PrograAvanzada.Visitas" %>

<%-- Si tu Visitas.aspx ya trae su directiva Page, dejá la tuya y pegá solo lo que sigue. --%>
<asp:Content ID="ContentVisitas" ContentPlaceHolderID="MainContent" runat="server">

    <%-- ============ ENCABEZADO ============ --%>
    <div class="d-flex justify-content-between align-items-start mb-4">
        <div>
            <h1 class="h3 mb-1">Visitas</h1>
            <p class="text-muted mb-0">Ingresos de hoy de personas con categoría Visitante o Proveedor, sin propietarios.</p>
        </div>
        <uc:Button ID="btnNuevoPreacreditado" runat="server" TextButton="Nuevo pre-acreditado" TypeButton="Save"
            OnButtonClick="btnNuevoPreacreditado_ButtonClick" />
    </div>

    <asp:Label ID="lblResultado" runat="server" Visible="false" CssClass="d-block mb-3 fw-semibold" />

    <%-- ============ PRE-ACREDITADOS DE HOY (cards) ============ --%>
    <h2 class="h5 mb-3">Pre-acreditados de hoy</h2>
    <asp:Label ID="lblSinPreacreditados" runat="server" Visible="false" CssClass="text-muted d-block mb-3"
        Text="No hay pre-acreditados para hoy." />
    <div class="row g-3 mb-5">
        <asp:Repeater ID="rptPreacreditados" runat="server">
            <%-- >>> ARREGLO 4: card oscura (sin bg-white / bg-light / text-muted de Bootstrap). Las clases visita-* están en Site.css --%>
            <ItemTemplate>
                <div class="col-12 col-md-6 col-xl-4">
                    <div class="visita-card h-100 d-flex flex-column gap-2">
                        <div class="d-flex align-items-center gap-3">
                            <div class="visita-avatar"><%# Iniciales(Eval("PersonaAutorizada")) %></div>
                            <div class="flex-grow-1">
                                <div class="visita-nombre"><%# Eval("PersonaAutorizada") %></div>
                                <div class="visita-detalle">Doc. <%# Eval("Documento") %> · <%# Eval("LoteDescripcion") %></div>
                            </div>
                        </div>
                        <div class="visita-detalle">
                            Persona del lote<br />
                            <span class="visita-valor"><%# Eval("PersonaDelLote") %></span>
                        </div>
                        <div class="d-flex justify-content-between align-items-center mt-auto">
                            <span class="badge text-bg-success">Vigente</span>
                            <%-- Mismo patrón que AgregarItems: el id viaja en un HiddenField del item --%>
                            <asp:HiddenField ID="hidIdPreacreditado" runat="server" Value='<%# Eval("id_preacreditado") %>' />
                            <uc:Button ID="btnRegistrarIngreso" runat="server" TextButton="Registrar ingreso" TypeButton="Save"
                                ButtonSize="Small" OnButtonClick="btnRegistrarIngreso_ButtonClick" />
                        </div>
                    </div>
                </div>
            </ItemTemplate>
            <%-- <<< FIN ARREGLO 4 --%>
        </asp:Repeater>
    </div>

    <%-- ============ VISITAS YA INGRESADAS HOY (tabla) ============ --%>
    <h2 class="h5 mb-3">Visitas ya ingresadas hoy</h2>
    <uc:Buscar ID="buscarIngresadas" runat="server" Placeholder="Buscar por DNI..."
        OnBuscarClick="buscarIngresadas_BuscarClick" />
    <uc:Table ID="tablaIngresadas" runat="server" />


    <%-- ============ MODAL 1: buscar a la persona (Paso 1) ============ --%>
    <uc:Modal ID="modalBuscar" runat="server" Tittle="Nuevo pre-acreditado">
        <Content>
            <p class="text-muted">Buscá a la persona por su documento para ver si ya está cargada en el padrón.</p>

            <uc:DropdownList ID="cbTipoDocBuscar" runat="server" Titulo="Tipo de documento *" />
            <uc:Input ID="inNroDocBuscar" runat="server" Label="Número de documento *" Placeholder="Ej.: 36480215" />
            <asp:Label ID="lblErrorBuscar" runat="server" CssClass="text-danger d-block" />

            <p class="small text-muted mt-3">
                Si la persona ya está cargada, solo completás lote y fecha y hora. Si no existe, vas a cargar sus datos;
                el vehículo se carga recién en la garita al registrar el ingreso.
            </p>

            <div class="d-flex justify-content-end gap-2 mt-3">
                <uc:Button ID="btnCancelarBuscar" runat="server" TextButton="Cancelar" TypeButton="Cancel"
                    OnButtonClick="btnCancelar_ButtonClick" />
                <uc:Button ID="btnBuscarPersona" runat="server" TextButton="Buscar persona" TypeButton="Save"
                    OnButtonClick="btnBuscarPersona_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>


    <%-- ============ MODAL 2: la persona ya está en el padrón ============ --%>
    <uc:Modal ID="modalExistente" runat="server" Tittle="Nuevo pre-acreditado">
        <Content>
            <div class="d-flex justify-content-between align-items-start mb-2">
                <p class="text-muted mb-0">La persona ya está en el padrón. Completá la pre-acreditación.</p>
                <uc:Button ID="btnCambiarDocExistente" runat="server" TextButton="Cambiar documento" TypeButton="Cancel"
                    ButtonSize="Small" OnButtonClick="btnCambiarDocumento_ButtonClick" />
            </div>

            <div class="visita-card mb-3"> <%-- [ARREGLO 4: recuadro "Ya en el padrón" oscuro] --%>
                <div class="visita-nombre"><asp:Literal ID="litExistenteNombre" runat="server" /></div>
                <div class="visita-detalle"><asp:Literal ID="litExistenteDetalle" runat="server" /></div>
                <span class="badge text-bg-secondary mt-2">Ya en el padrón</span>
            </div>

            <%-- Solo aparece si la persona tiene más de una categoría válida --%>
            <asp:Label ID="lblCategoriaExistente" runat="server" Visible="false" CssClass="d-block fw-semibold mb-1"
                Text="Categoría de la visita *" />
            <asp:RadioButtonList ID="rblCategoriaExistente" runat="server" Visible="false" RepeatLayout="Flow"
                RepeatDirection="Horizontal" CssClass="d-block mb-3" />

            <uc:DropdownList ID="cbLoteExistente" runat="server" Titulo="Lote destino *" />
            <uc:Input ID="inFechaHoraExistente" runat="server" Label="Fecha y hora de la visita *"
                Placeholder="dd/mm/aaaa HHmm (ej.: 05/08/2026 0800)" />
            <asp:Label ID="lblErrorExistente" runat="server" CssClass="text-danger d-block" />

            <div class="d-flex justify-content-end gap-2 mt-3">
                <uc:Button ID="btnCancelarExistente" runat="server" TextButton="Cancelar" TypeButton="Cancel"
                    OnButtonClick="btnCancelar_ButtonClick" />
                <uc:Button ID="btnPreacreditar" runat="server" TextButton="Pre-acreditar" TypeButton="Save"
                    OnButtonClick="btnPreacreditar_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>


    <%-- ============ MODAL 3: la persona NO existe (alta + pre-acreditación) ============ --%>
    <uc:Modal ID="modalNueva" runat="server" Tittle="Nuevo pre-acreditado">
        <Content>
            <div class="d-flex justify-content-between align-items-start mb-2">
                <p class="text-muted mb-0"><asp:Literal ID="litNuevaMensaje" runat="server" /></p>
                <uc:Button ID="btnCambiarDocNueva" runat="server" TextButton="Cambiar documento" TypeButton="Cancel"
                    ButtonSize="Small" OnButtonClick="btnCambiarDocumento_ButtonClick" />
            </div>

            <div class="fw-semibold mb-2">Datos de la persona <span class="small text-muted fw-normal">(se crea en el padrón con estado Activo)</span></div>
            <div class="mb-3">
                <div class="small text-muted">Documento</div>
                <div class="fw-semibold"><asp:Literal ID="litNuevaDocumento" runat="server" /></div>
            </div>

            <uc:DropdownList ID="cbPaisNueva" runat="server" Titulo="Nacionalidad *" />
            <uc:Input ID="inApellidoNueva" runat="server" Label="Apellido *" />
            <uc:Input ID="inNombresNueva" runat="server" Label="Nombres *" />
            <uc:Input ID="inFechaNacNueva" runat="server" Label="Fecha de nacimiento *" Type="Date" />

            <div class="fw-semibold mt-3 mb-1">Pre-acreditación <span class="small text-muted fw-normal">(categoría, lote y franja de la visita)</span></div>
            <asp:RadioButtonList ID="rblCategoriaNueva" runat="server" RepeatLayout="Flow"
                RepeatDirection="Horizontal" CssClass="d-block mb-3" />
            <uc:DropdownList ID="cbLoteNueva" runat="server" Titulo="Lote destino *" />
            <uc:Input ID="inFechaHoraNueva" runat="server" Label="Fecha y hora de la visita *"
                Placeholder="dd/mm/aaaa HHmm (ej.: 05/08/2026 0800)" />

            <p class="small text-muted mt-2">
                El vehículo, el seguro y la licencia no se piden ahora: se cargan cuando la persona llega a la garita
                y se registra su ingreso.
            </p>
            <asp:Label ID="lblErrorNueva" runat="server" CssClass="text-danger d-block" />

            <div class="d-flex justify-content-end gap-2 mt-3">
                <uc:Button ID="btnCancelarNueva" runat="server" TextButton="Cancelar" TypeButton="Cancel"
                    OnButtonClick="btnCancelar_ButtonClick" />
                <uc:Button ID="btnCrearYPreacreditar" runat="server" TextButton="Crear y pre-acreditar" TypeButton="Save"
                    OnButtonClick="btnCrearYPreacreditar_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>


    <%-- ============ MODAL 4: registrar ingreso (botón de cada card) ============ --%>
    <uc:Modal ID="modalIngreso" runat="server" Tittle="Registrar ingreso">
        <Content>
            <div class="fw-semibold"><asp:Literal ID="litIngresoResumen" runat="server" /></div>
            <p class="small text-muted"><asp:Literal ID="litIngresoAviso" runat="server" /></p>

            <%-- Solo se muestra lo que le falta a la persona --%>
            <asp:Panel ID="pnlPendientes" runat="server" Visible="false">
                <div class="fw-semibold mb-2">Datos de la persona pendientes de completar</div>
                <uc:Input ID="inTelefonoIngreso" runat="server" Label="Teléfono" Placeholder="Ej.: 11 4477 2090" />

                <asp:Panel ID="pnlFotoIngreso" runat="server" Visible="false">
                    <label class="busqueda-label">Foto de referencia</label>

                    <div class="foto-tabs">
                        <button type="button" class="btn btn-cancel" onclick="mostrarTabFotoIngreso('archivo')">Subir archivo</button>
                        <button type="button" class="btn btn-cancel" onclick="mostrarTabFotoIngreso('camara')">Usar cámara</button>
                    </div>

                    <div id="panelArchivoIngreso">
                        <input type="file" accept="image/*" onchange="cargarArchivoComoFotoIngreso(this)" />
                    </div>

                    <div id="panelCamaraIngreso" style="display:none;">
                        <video id="videoCamaraIngreso" autoplay="autoplay" playsinline="playsinline" muted="muted" style="max-width:320px; border-radius:8px;"></video>
                        <div style="margin-top:8px;">
                            <button type="button" class="btn btn-cancel" onclick="iniciarCamaraIngreso()">Activar cámara</button>
                            <button type="button" class="btn btn-save" onclick="capturarFotoIngreso()">Capturar</button>
                        </div>
                        <canvas id="canvasCamaraIngreso" style="display:none;"></canvas>
                    </div>

                    <div id="previewFotoIngreso" style="margin-top:10px;"></div>

                    <%-- Wrapper con id plano (no runat="server"): evita depender de
                         <%= hdnFotoBase64.ClientID %> dentro del <Content> de uc:Modal
                         (un ITemplate). El JS busca el input oculto por acá adentro,
                         sin necesitar su ClientID. --%>
                    <div id="wrapperFotoIngreso">
                        <asp:HiddenField ID="hdnFotoBase64" runat="server" />
                    </div>
                </asp:Panel>
            </asp:Panel>

            <div class="fw-semibold mb-2 mt-3">Vehículo</div>
            <asp:RadioButtonList ID="rblVehiculo" runat="server" AutoPostBack="true" RepeatLayout="Flow"
                CssClass="d-block mb-3" OnSelectedIndexChanged="rblVehiculo_SelectedIndexChanged" />

            <%-- "Otro vehículo": si la patente ya existe en la base, estos campos se ignoran --%>
            <asp:Panel ID="pnlVehiculoNuevo" runat="server" Visible="false">
                <uc:Input ID="inPatenteNueva" runat="server" Label="Patente *" Placeholder="Ej.: AD 118 FK" />
                <uc:Input ID="inMarcaModelo" runat="server" Label="Marca / Modelo *" Placeholder="Ej.: Renault Sandero gris" />
                <uc:DropdownList ID="cbCompania" runat="server" Titulo="Compañía de seguro *" />
                <uc:Input ID="inNroPoliza" runat="server" Label="N° de póliza *" />
                <uc:Input ID="inVigenciaPoliza" runat="server" Label="Vigencia del seguro *" Type="Date" />
            </asp:Panel>

            <%-- Solo si va con vehículo y no tiene una licencia vigente registrada --%>
            <asp:Panel ID="pnlLicencia" runat="server" Visible="false">
                <div class="fw-semibold mb-1">Licencia de conducir</div>
                <div class="small text-muted mb-2">Se pide porque no tiene una licencia vigente registrada.</div>
                <uc:Input ID="inNroLicencia" runat="server" Label="N° de licencia *" />
                <uc:Input ID="inVigenciaLicencia" runat="server" Label="Vigencia licencia *" Type="Date" />
            </asp:Panel>

            <uc:Input ID="inNotaIngreso" runat="server" Label="Nota del guardia"
                Placeholder="Ej.: ingresa con dos acompañantes; deja paquete en garita." />
            <asp:Label ID="lblErrorIngreso" runat="server" CssClass="text-danger d-block" />

            <div class="d-flex justify-content-end gap-2 mt-3">
                <uc:Button ID="btnCancelarIngreso" runat="server" TextButton="Cancelar" TypeButton="Cancel"
                    OnButtonClick="btnCancelar_ButtonClick" />
                <uc:Button ID="btnConfirmarIngreso" runat="server" TextButton="Registrar ingreso" TypeButton="Save"
                    OnButtonClick="btnConfirmarIngreso_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>

    <script>
        // Mismo patrón que NuevaPersona.aspx, con sufijo "Ingreso" porque este bloque
        // vive dentro de un modal (no hay otra instancia en esta página, así que los
        // ids planos no chocan; el sufijo es solo para no confundirlo con el otro archivo).
        function mostrarTabFotoIngreso(cual) {
            document.getElementById('panelArchivoIngreso').style.display = cual === 'archivo' ? 'block' : 'none';
            document.getElementById('panelCamaraIngreso').style.display = cual === 'camara' ? 'block' : 'none';
        }

        function cargarArchivoComoFotoIngreso(input) {
            if (!input.files || !input.files[0]) return;
            var reader = new FileReader();
            reader.onload = function (e) {
                guardarFotoBase64Ingreso(e.target.result);
            };
            reader.readAsDataURL(input.files[0]);
        }

        var streamCamaraIngreso = null;

        function iniciarCamaraIngreso() {
            navigator.mediaDevices.getUserMedia({ video: true })
                .then(function (stream) {
                    streamCamaraIngreso = stream;
                    document.getElementById('videoCamaraIngreso').srcObject = stream;
                })
                .catch(function () {
                    alert('No se pudo acceder a la cámara. Revisá los permisos del navegador.');
                });
        }

        function capturarFotoIngreso() {
            var video = document.getElementById('videoCamaraIngreso');
            var canvas = document.getElementById('canvasCamaraIngreso');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            canvas.getContext('2d').drawImage(video, 0, 0);
            guardarFotoBase64Ingreso(canvas.toDataURL('image/jpeg', 0.85));

            if (streamCamaraIngreso) {
                streamCamaraIngreso.getTracks().forEach(function (t) { t.stop(); });
            }
        }

        // Busca el HiddenField por el wrapper con id plano, no por ClientID (ver comentario arriba).
        function guardarFotoBase64Ingreso(dataUrl) {
            document.querySelector('#wrapperFotoIngreso input[type=hidden]').value = dataUrl;
            document.getElementById('previewFotoIngreso').innerHTML =
                '<img src="' + dataUrl + '" style="max-width:160px; border-radius:8px;" />';
        }
    </script>

</asp:Content>
