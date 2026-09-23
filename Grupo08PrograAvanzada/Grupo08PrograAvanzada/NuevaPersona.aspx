<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuevaPersona.aspx.cs" Inherits="Grupo08PrograAvanzada.NuevaPersona" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>Nueva persona con categoría</h1>
    <div class="form-card">

        <h3>Identificación de la persona</h3>
        <div class="form-row">
            <uc:DropdownList ID="cbTipoDocumento" runat="server" Titulo="Tipo de documento *" Layout="OcupaMas" />
            <div>
                <label class="busqueda-label">Número de documento *</label>
                <asp:TextBox ID="txtNumeroDocumento" runat="server" CssClass="input-generico" />
            </div>
        </div>

        <uc:DropdownList ID="cbNacionalidad" runat="server" Titulo="Nacionalidad *" Layout="OcupaMas" />

        <div class="form-row">
            <div>
                <label class="busqueda-label">Apellido *</label>
                <asp:TextBox ID="txtApellido" runat="server" CssClass="input-generico" />
            </div>
            <div>
                <label class="busqueda-label">Nombres *</label>
                <asp:TextBox ID="txtNombres" runat="server" CssClass="input-generico" />
            </div>
        </div>

        <div class="form-row">
            <div>
                <label class="busqueda-label">Fecha de nacimiento *</label>
                <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="input-generico" TextMode="Date" />
            </div>
            <div>
                <label class="busqueda-label">Teléfono</label>
                <asp:TextBox ID="txtTelefono" runat="server" CssClass="input-generico" />
            </div>
        </div>

        <div>
            <label class="busqueda-label">Foto de referencia</label>

            <div class="foto-tabs">
                <button type="button" id="btnTabArchivo" class="btn btn-cancel" onclick="mostrarTabFoto('archivo')">Subir archivo</button>
                <button type="button" id="btnTabCamara" class="btn btn-cancel" onclick="mostrarTabFoto('camara')">Usar cámara</button>
            </div>

            <div id="panelArchivo">
                <input type="file" id="inputArchivoFoto" accept="image/*" onchange="cargarArchivoComoFoto(this)" />
            </div>

            <div id="panelCamara" style="display:none;">
               <video id="videoCamara" autoplay="autoplay" playsinline="playsinline" muted="muted" style="max-width:320px; border-radius:8px;"></video>
                <div style="margin-top:8px;">
                    <button type="button" class="btn btn-cancel" onclick="iniciarCamara()">Activar cámara</button>
                    <button type="button" class="btn btn-save" onclick="capturarFoto()">Capturar</button>
                </div>
                <canvas id="canvasCamara" style="display:none;"></canvas>
            </div>

            <div id="previewFoto" style="margin-top:10px;"></div>

            <asp:HiddenField ID="hdnFotoBase64" runat="server" />
        </div>

        <h3>Categoría de la persona</h3>
        <asp:CheckBoxList ID="chkCategorias" runat="server" CssClass="categorias-list" RepeatDirection="Horizontal" />

        <h3>Vehículo de la persona</h3>
        <uc:Checkbox ID="chkTieneVehiculo" runat="server" Label="La persona tiene vehículo" OnCheckedChanged="chkTieneVehiculo_CheckedChanged" />

        <asp:Panel ID="pnlVehiculo" runat="server" Visible="false">
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Patente *</label>
                    <asp:TextBox ID="txtPatente" runat="server" CssClass="input-generico" />
                </div>
                <div>
                    <label class="busqueda-label">Marca / Modelo *</label>
                    <asp:TextBox ID="txtMarcaModelo" runat="server" CssClass="input-generico" />
                </div>
            </div>
            <div class="form-row">
                <uc:DropdownList ID="cbCompaniaSeguro" runat="server" Titulo="Compañía de seguro" Layout="OcupaMas" />
                <div>
                    <label class="busqueda-label">N° de póliza</label>
                    <asp:TextBox ID="txtNumeroPoliza" runat="server" CssClass="input-generico" />
                </div>
            </div>
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Vigencia del seguro</label>
                    <asp:TextBox ID="txtVigenciaSeguro" runat="server" CssClass="input-generico" TextMode="Date" />
                </div>
            </div>

            <h3>Licencia de conducir</h3>
            <div class="form-row">
                <div>
                    <label class="busqueda-label">N° de licencia *</label>
                    <asp:TextBox ID="txtNumeroLicencia" runat="server" CssClass="input-generico" />
                </div>
                <div>
                    <label class="busqueda-label">Vigencia *</label>
                    <asp:TextBox ID="txtVigenciaLicencia" runat="server" CssClass="input-generico" TextMode="Date" />
                </div>
            </div>
        </asp:Panel>

        <asp:Label ID="lblError" runat="server" CssClass="page-sub" ForeColor="#e05252" />

        <div class="form-actions">
            <uc:Button ID="btnCancelar" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelar_ButtonClick" />
            <uc:Button ID="btnGuardar" runat="server" TextButton="Guardar persona" TypeButton="Save" OnButtonClick="btnGuardar_ButtonClick" />
        </div>

    </div>

    <script>
        function mostrarTabFoto(cual) {
            document.getElementById('panelArchivo').style.display = cual === 'archivo' ? 'block' : 'none';
            document.getElementById('panelCamara').style.display = cual === 'camara' ? 'block' : 'none';
        }

        function cargarArchivoComoFoto(input) {
            if (!input.files || !input.files[0]) return;
            var reader = new FileReader();
            reader.onload = function (e) {
                guardarFotoBase64(e.target.result);
            };
            reader.readAsDataURL(input.files[0]);
        }

        var streamCamara = null;

        function iniciarCamara() {
            navigator.mediaDevices.getUserMedia({ video: true })
                .then(function (stream) {
                    streamCamara = stream;
                    document.getElementById('videoCamara').srcObject = stream;
                })
                .catch(function () {
                    alert('No se pudo acceder a la cámara. Revisá los permisos del navegador.');
                });
        }

        function capturarFoto() {
            var video = document.getElementById('videoCamara');
            var canvas = document.getElementById('canvasCamara');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            canvas.getContext('2d').drawImage(video, 0, 0);
            guardarFotoBase64(canvas.toDataURL('image/jpeg', 0.85));

            if (streamCamara) {
                streamCamara.getTracks().forEach(function (t) { t.stop(); });
            }
        }

        function guardarFotoBase64(dataUrl) {
            document.getElementById('<%= hdnFotoBase64.ClientID %>').value = dataUrl;
            document.getElementById('previewFoto').innerHTML =
                '<img src="' + dataUrl + '" style="max-width:160px; border-radius:8px;" />';
        }
    </script>

</asp:Content>
