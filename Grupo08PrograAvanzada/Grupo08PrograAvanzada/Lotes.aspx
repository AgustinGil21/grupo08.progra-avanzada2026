<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Lotes.aspx.cs" Inherits="Grupo08PrograAvanzada.Lotes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <uc:Card ID="cardLotes" runat="server" Tittle="Lotes del barrio">
        <Contenido>
            <p class="card-subtitulo">
                Sólo consulta
            </p>

            <uc:Buscar ID="ucBuscar" runat="server" Placeholder="Número de lote, descripción o apellido..."
                       OnBuscarClick="ucBuscar_BuscarClick">
                <Filters>
                    <uc:DropdownList ID="ddlEstadoFiltro" runat="server" Titulo="Estado del lote" />
                </Filters>
            </uc:Buscar>

            <table class="table-lotes">
                <thead>
                    <tr>
                        <th>Descripción</th>
                        <th>Titular</th>
                        <th>Estado</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptLotes" runat="server" OnItemDataBound="rptLotes_ItemDataBound">
                        <ItemTemplate>
                            <tr>
                                <td><%# Eval("descripcion") %></td>
                                <td><%# Eval("Titular") %></td>
                                <td><asp:Literal ID="litEstado" runat="server" /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>

            <asp:Label ID="lblSinResultados" runat="server" Visible="false" Text="No hay lotes para mostrar" />
        </Contenido>
    </uc:Card>

    <!-- Vista chica del plano -->
    <div class="plano-preview">
        <div>
            <p class="plano-titulo">Plano del barrio</p>
            <p class="plano-subtitulo">
                Los 412 lotes tienen latitud y longitud tomadas del mapa: alcanza para dibujar los marcadores sobre el plano.
            </p>
        </div>
        <uc:Button ID="btnVerPlano" runat="server" TextButton="Ver plano completo"
                   OnClientClick="verPlanoCompleto(); return false;" />
    </div>
    <div id="divMapaChico" class="mapa-chico"></div>

    <!-- Modal: Plano completo (solo lectura, sin acciones) -->
    <uc:Modal ID="mdlPlano" runat="server" Tittle="Plano del barrio" Tamanio="grande">
        <Content>
            <div id="divMapaGrande" class="mapa-grande"></div>
            <div class="plano-leyenda">
                <span class="leyenda-activo">●</span> Activo
                <span class="leyenda-bloqueado">●</span> Bloqueado
                <span class="leyenda-inactivo">●</span> Inactivo
            </div>
        </Content>
    </uc:Modal>

    <script>
        var mapaChico, mapaGrande, mapaGrandeIniciado = false;

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

        document.addEventListener("DOMContentLoaded", function () {
            if (typeof lotesParaMapa === "undefined" || lotesParaMapa.length === 0) return;
            mapaChico = L.map('divMapaChico').setView([lotesParaMapa[0].lat, lotesParaMapa[0].lng], 15);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(mapaChico);
            pintarMarcadores(mapaChico);
        });

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
