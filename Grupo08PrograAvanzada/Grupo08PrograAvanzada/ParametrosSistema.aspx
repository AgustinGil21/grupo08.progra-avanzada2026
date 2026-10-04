<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ParametrosSistema.aspx.cs" Inherits="Grupo08PrograAvanzada.ParametrosSistema" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="page-header">
        <div>
            <h1>Parámetros del sistema</h1>
        </div>
        <button type="button" class="btn btn-save" onclick="mostrarModal('<%= mdlInforme.OverlayClientID %>')">Descargar informe PDF</button>
    </div>

    <div class="form-card">
        <h3>Datos del barrio</h3>

        <div class="form-row">
            <div>
                <label class="busqueda-label">Nombre del barrio *</label>
                <asp:TextBox ID="txtNombreBarrio" runat="server" CssClass="input-generico" MaxLength="150" />
            </div>
            <div>
                <label class="busqueda-label">Domicilio</label>
                <asp:TextBox ID="txtDomicilio" runat="server" CssClass="input-generico" MaxLength="250" />
            </div>
            <uc:DropdownList ID="cbZonaHoraria" runat="server" Titulo="Zona horaria" Layout="OcupaMas" />
        </div>
    </div>

    <div class="form-card">
        <h3>Retención de datos</h3>

        <div class="form-row">
            <uc:DropdownList ID="cbMesesMovimientos" runat="server" Titulo="Conservar movimientos" Layout="OcupaMas" />
            <uc:DropdownList ID="cbMesesReportes" runat="server" Titulo="Conservar reportes" Layout="OcupaMas" />
        </div>
    </div>

    <p class="page-sub"><asp:Literal ID="litUltimoCambio" runat="server" /></p>
    <asp:Label ID="lblMensajeError" runat="server" CssClass="page-sub" ForeColor="#e05252" />
    <asp:Label ID="lblMensajeOk" runat="server" CssClass="page-sub" ForeColor="#2f9e6e" />

    <div class="form-actions">
        <uc:Button ID="btnDescartar" runat="server" TextButton="Descartar" TypeButton="Cancel" OnButtonClick="btnDescartar_ButtonClick" />
        <uc:Button ID="btnGuardar" runat="server" TextButton="Guardar cambios" TypeButton="Save" OnButtonClick="btnGuardar_ButtonClick" />
    </div>

    <uc:Modal ID="mdlInforme" runat="server" Tittle="Descargar informe PDF">
        <Content>
            <p class="page-sub">Por persona, categoría, lote o rango de fechas. Incluye ingresos, egresos o ambos.</p>

            <uc:DropdownList ID="cbTipoInforme" runat="server" Titulo="Tipo de informe" Layout="OcupaMas" />

            <label class="busqueda-label">Movimiento</label>
            <asp:RadioButtonList ID="rblMovimiento" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow">
                <asp:ListItem Value="Ingreso" Text="Ingresos" />
                <asp:ListItem Value="Egreso" Text="Egresos" />
                <asp:ListItem Value="Ambos" Text="Ambos" Selected="True" />
            </asp:RadioButtonList>

            <asp:Panel ID="pnlFiltroPersona" runat="server">
                <div class="form-row">
                    <uc:DropdownList ID="cbTipoDocumento" runat="server" Titulo="Tipo de documento" Layout="OcupaMas" />
                    <div>
                        <label class="busqueda-label">Número de documento</label>
                        <asp:TextBox ID="txtNroDocumento" runat="server" CssClass="input-generico" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlFiltroCategoria" runat="server">
                <uc:DropdownList ID="cbCategoriaInforme" runat="server" Titulo="Categoría" Layout="OcupaMas" />
            </asp:Panel>

            <asp:Panel ID="pnlFiltroLote" runat="server">
                <uc:DropdownList ID="cbLoteInforme" runat="server" Titulo="Lote" Layout="OcupaMas" />
            </asp:Panel>

            <asp:Panel ID="pnlFiltroFechas" runat="server">
                <div class="form-row">
                    <div>
                        <label class="busqueda-label">Desde</label>
                        <asp:TextBox ID="txtDesde" runat="server" CssClass="input-generico" TextMode="Date" />
                    </div>
                    <div>
                        <label class="busqueda-label">Hasta</label>
                        <asp:TextBox ID="txtHasta" runat="server" CssClass="input-generico" TextMode="Date" />
                    </div>
                </div>
            </asp:Panel>

            <uc:DropdownList ID="cbGaritaInforme" runat="server" Titulo="Garita" Layout="OcupaMas" />

            <asp:Label ID="lblErrorInforme" runat="server" CssClass="page-sub" ForeColor="#e05252" />

            <div class="form-actions">
                <uc:Button ID="btnCancelarInforme" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelarInforme_ButtonClick" />
                <uc:Button ID="btnDescargarPdf" runat="server" TextButton="Descargar PDF" TypeButton="Save" OnButtonClick="btnDescargarPdf_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>

</asp:Content>
