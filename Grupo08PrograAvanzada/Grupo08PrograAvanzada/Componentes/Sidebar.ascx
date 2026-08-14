<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Sidebar.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Sidebar" %>

<div class="sidebar">
    <div class="sidebar-header">
        <div class="sidebar-logo"><i class="bi bi-shield-check"></i></div>
        <div>
            <div class="sidebar-title">Barrio Cerrado I</div>
            <div class="sidebar-subtitle"><asp:Literal ID="litSubtitulo" runat="server" /></div>
        </div>
    </div>

    <asp:Panel ID="panelGuardia" runat="server" CssClass="sidebar-section" Visible="false">
        <div class="sidebar-label">OPERACIÓN</div>
        <a runat="server" href="~/Padron.aspx" class="sidebar-item">
            <i class="bi bi-people"></i> Padrón de personas
        </a>
        <a runat="server" href="~/Movimiento.aspx" class="sidebar-item">
            <i class="bi bi-arrow-right-square"></i> Registrar movimiento
        </a>
    </asp:Panel>

    <asp:Panel ID="pnlAdmin" runat="server" CssClass="sidebar-section" Visible="false">
        <div class="sidebar-label">ADMINISTRACIÓN</div>
        <a runat="server" href="~/Tablero.aspx" class="sidebar-item">
            <i class="bi bi-grid"></i> Tablero general
        </a>
        <a runat="server" href="~/Usuarios.aspx" class="sidebar-item">
            <i class="bi bi-person-lines-fill"></i> Usuarios y accesos
        </a>
    </asp:Panel>

    <div class="sidebar-footer">
        <div class="sidebar-avatar"><asp:Literal ID="litIniciales" runat="server" /></div>
        <div class="sidebar-user">
            <div class="sidebar-user-name"><asp:Literal ID="litNombre" runat="server" /></div>
            <div class="sidebar-user-role"><asp:Literal ID="litDetalle" runat="server" /></div>
        </div>
    </div>
    <asp:LinkButton ID="btnCerrar" runat="server" CssClass="sidebar-logout" OnClick="btnCerrar_Click">
        <i class="bi bi-box-arrow-right"></i> Cerrar sesión
    </asp:LinkButton>
</div>