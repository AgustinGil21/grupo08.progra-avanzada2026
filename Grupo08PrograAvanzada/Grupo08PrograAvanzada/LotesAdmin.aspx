<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="LotesAdmin.aspx.cs" Inherits="Grupo08PrograAvanzada.LotesAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div>
        <h1>Lotes del barrio</h1>
    </div>

    <uc:Card ID="cardLotes" runat="server" Tittle="Lotes del barrio">
        <Contenido>
            <p class="card-subtitulo">
                <asp:Literal ID="litResumen" runat="server" />
            </p>

            <div class="acciones-header">
                <uc:Button ID="btnNuevoLote" runat="server" TextButton="Nuevo lote" TypeButton="Save" />
                <uc:Button ID="btnVerPlano" runat="server" TextButton="Ver plano completo" />
            </div>

            <uc:Buscar ID="ucBuscar" runat="server" Placeholder="Número de lote, descripción o apellido..." Titulo="Buscar por descripcion de lote"
                       OnBuscarClick="ucBuscar_BuscarClick">
                <Filters>
                    <uc:DropdownList ID="ddlEstadoFiltro" runat="server" Titulo="Estado del lote" />
                    <uc:DropdownList ID="ddlTitularFiltro" runat="server" Titulo="Titular" />
                </Filters>
            </uc:Buscar>

            <table class="table-lotes">
                <thead>
                    <tr>
                        <th>Descripción</th>
                        <th>Titular</th>
                        <th>Estado</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptLotes" runat="server" OnItemDataBound="rptLotes_ItemDataBound"
                        OnItemCommand="rptLotes_ItemCommand">
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("descripcion") %></td>
                                <td><%# Eval("Titular") %></td>
                                <td><asp:Literal ID="litEstado" runat="server" /></td>
                                <td>
                                    <asp:LinkButton ID="btnAccionFila" runat="server"
                                        CommandName="AccionFila"
                                        CommandArgument='<%# Eval("id_lote") %>' />
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>

            <asp:Label ID="lblSinResultados" runat="server" Visible="false" Text="No hay lotes para mostrar" />
        </Contenido>
    </uc:Card>

    <!-- Modal: Nuevo lote -->
    <uc:Modal ID="mdlNuevoLote" runat="server" Tittle="Nuevo lote">
        <Content>
            <uc:Input ID="txtNuevoDescripcion" runat="server" Label="Número de lote / descripción *" Required="true" />
            <uc:DropdownList ID="ddlNuevoEstado" runat="server" Titulo="Estado *" />
            <uc:Input ID="txtNuevoLatitud" runat="server" Label="Latitud (opcional)" Type="Number" />
            <uc:Input ID="txtNuevoLongitud" runat="server" Label="Longitud (opcional)" Type="Number" />

            <uc:Input ID="txtNuevoDni" runat="server" Label="Buscar persona por DNI" />
            <uc:Button ID="btnNuevoBuscarDni" runat="server" TextButton="Buscar"
                       OnButtonClick="btnNuevoBuscarDni_ButtonClick" />
            <asp:Literal ID="litNuevoPersonaEncontrada" runat="server" />

            <div class="form-actions">
                <uc:Button ID="btnCancelarCrearNuevoLote" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelarCrearNuevoLote_ButtonClick" />
                <uc:Button ID="btnCrearLote" runat="server" TextButton="Crear lote" TypeButton="Save" OnButtonClick="btnCrearLote_ButtonClick" />
            </div>
            
            <asp:Literal ID="litErrorNuevoLote" runat="server" />
        </Content>
    </uc:Modal>

    <!-- Modal: Editar lote -->
    <uc:Modal ID="mdlEditarLote" runat="server" Tittle="Editar lote">
        <Content>
            <asp:HiddenField ID="hdnIdLoteEditar" runat="server" />
            <uc:Input ID="txtEditarDescripcion" runat="server" Label="Número de lote *" Required="true" />
            <uc:DropdownList ID="ddlEditarEstado" runat="server" Titulo="Estado *" />
            <uc:Input ID="txtEditarLatitud" runat="server" Label="Latitud (opcional)" Type="Number" />
            <uc:Input ID="txtEditarLongitud" runat="server" Label="Longitud (opcional)" Type="Number" />

            <asp:Literal ID="litEditarTitularActual" runat="server" />
            <uc:Button ID="btnCambiarTitular" runat="server" TextButton="Cambiar titular"
                       OnButtonClick="btnCambiarTitular_ButtonClick" />

            <div class="form-actions">
                <uc:Button ID="btnCancelarGuardarEdicion" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelarGuardarEdicion_ButtonClick" />
                <uc:Button ID="btnGuardarEdicion" runat="server" TextButton="Guardar cambios" TypeButton="Save" OnButtonClick="btnGuardarEdicion_ButtonClick" />
            </div>
            
            <asp:Literal ID="litErrorEditarLote" runat="server" />
        </Content>
    </uc:Modal>

    <!-- Modal: Asignar / cambiar titular -->
    <uc:Modal ID="mdlAsignarTitular" runat="server" Tittle="Asignar titular">
        <Content>
            <asp:HiddenField ID="hdnIdLoteTitular" runat="server" />
            <uc:Input ID="txtTitularDni" runat="server" Label="Buscar persona por DNI" />
            <uc:Button ID="btnBuscarDniTitular" runat="server" TextButton="Buscar"
                       OnButtonClick="btnBuscarDniTitular_ButtonClick" />
            <asp:Literal ID="litPersonaEncontradaTitular" runat="server" />
            <asp:HiddenField ID="hdnIdPersonaTitular" runat="server" />

            <div class="form-actions">
                <uc:Button ID="btnCancelarConfirmarTitular" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelarConfirmarTitular_ButtonClick" />
                <uc:Button ID="btnConfirmarTitular" runat="server" TextButton="Asignar titular" TypeButton="Save" OnButtonClick="btnConfirmarTitular_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>

    <!-- Modal: Plano completo (solo lectura) -->
    <uc:Modal ID="mdlPlano" runat="server" Tittle="Plano del barrio" Tamanio="grande">
        <Content>
            <div class="mapa-wrapper">
                <div id="divMapaGrande" class="mapa-grande"></div>
                <div class="plano-leyenda-flotante">
                    <span class="leyenda-activo">●</span> Activo
                    <span class="leyenda-bloqueado">●</span> Bloqueado
                    <span class="leyenda-inactivo">●</span> Inactivo
                </div>
            </div>
            <div class="form-actions">
                <uc:Button ID="btnCerrarPlano" runat="server" TextButton="Cerrar" TypeButton="Cancel" OnButtonClick="btnCerrarPlano_ButtonClick"/>
            </div>
        </Content>
    </uc:Modal>

    <script>
        var mapaGrande, mapaGrandeIniciado = false;

        function colorPorEstado(estado) {
            if (estado === "Activo") return "green";
            if (estado === "Bloqueado") return "red";
            return "gray"; // Inactivo
        }

        function pintarMarcadores(mapa) {
            lotesParaMapa.forEach(function (l) {
                L.circleMarker([l.lat, l.lng], { radius: 6, color: colorPorEstado(l.estado) })
                    .addTo(mapa)
                    .bindPopup("<b>" + l.descripcion + "</b><br>" + l.estado + "<br>" + l.titular);
            });
        }

        function verPlanoCompleto() {
            mostrarModal('<%= mdlPlano.OverlayClientID %>');
            setTimeout(function () {
                if (!mapaGrandeIniciado && typeof lotesParaMapa !== "undefined" && lotesParaMapa.length > 0) {
                    mapaGrande = L.map('divMapaGrande').setView([lotesParaMapa[0].lat, lotesParaMapa[0].lng], 15);
                    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(mapaGrande);
                    pintarMarcadores(mapaGrande);
                    mapaGrandeIniciado = true;
                }
                if (mapaGrande) mapaGrande.invalidateSize();
            }, 200);
        }
    </script>

</asp:Content>
