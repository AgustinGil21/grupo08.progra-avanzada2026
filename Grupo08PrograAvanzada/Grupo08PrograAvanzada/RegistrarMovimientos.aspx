<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RegistrarMovimientos.aspx.cs" Inherits="Grupo08PrograAvanzada.RegistrarMovimientos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="page-header">
        <h1>Registrar movimiento</h1>
    </div>

    <!-- Card 1 SIEMPRE visible, afuera de cualquier panel condicional -->
    <uc:Card ID="ucCardIdentificar" runat="server">
        <Contenido>
            <div class="paso-titulo">1. Identificar a la persona</div>
            <uc:Input ID="txtDocumento" runat="server" Label="Número de documento" Placeholder="27845113" />
            <uc:Button ID="btnBuscar" runat="server" TextButton="Buscar" TypeButton="Save" />
            <asp:Image ID="imgFotoReferencia" runat="server" Visible="false"
            AlternateText="Foto de referencia" Style="max-width:160px; border-radius:8px;" />
        </Contenido>
    </uc:Card>

    <!-- Rama EGRESO -->
    <asp:Panel ID="pnlEgreso" runat="server" Visible="false">
        <uc:Card ID="ucCardEgreso" runat="server">
            <Contenido>
                <div class="paso-titulo">Movimiento abierto</div>
                <asp:Literal ID="litInfoIngreso" runat="server" />
                <uc:Input ID="txtPatenteEgreso" runat="server" Label="Patente (vacío si sale a pie)" Placeholder="AB123CD" />                
                <uc:Button ID="btnConfirmarEgreso" runat="server" TextButton="Confirmar egreso" TypeButton="Save" />
                <asp:Literal ID="litResultadoEgreso" runat="server" />
            </Contenido>
        </uc:Card>
    </asp:Panel>

    <!-- Rama INGRESO: pasos 2, 3 en 2 columnas -->
    <asp:Panel ID="pnlIngreso" runat="server" Visible="false" CssClass="movimiento-grid">

        <div class="movimiento-columna">
            <uc:Card ID="ucCardModoIngreso" runat="server">
                <Contenido>
                    <div class="paso-titulo">2. Modo de ingreso</div>
                    <asp:CheckBox ID="chkConVehiculo" runat="server" Text="Ingresa con vehículo" AutoPostBack="true" />
                    <asp:Panel ID="pnlPatente" runat="server" Visible="false">
                        <uc:Input ID="txtPatente" runat="server" Label="Patente" Placeholder="AB123CD" />
                    </asp:Panel>
                    <uc:DropdownList ID="ddlLoteDestino" runat="server" Titulo="Lote destino" />
                    <uc:Button ID="btnVerificar" runat="server" TextButton="Registrar ingreso" TypeButton="Save" />
                </Contenido>
            </uc:Card>
        </div>

        <div class="movimiento-columna">
            <asp:Panel ID="pnlVerificacion" runat="server" Visible="false">
                <uc:Card ID="ucCardVerificacion" runat="server">
                    <Contenido>
                        <div class="paso-titulo">3. Verificación de acceso</div>
                        <asp:Literal ID="litResultadoVerificacion" runat="server" />
                    </Contenido>
                </uc:Card>
            </asp:Panel>
        </div>

    </asp:Panel>

</asp:Content>
