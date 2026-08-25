<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Buscar.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Buscar" %>

<div class="busqueda-wrapper">
    <asp:Label ID="lblTitulo" runat="server" CssClass="busqueda-label" />
    <div class="busqueda-fila">
        <asp:TextBox ID="txtBuscar" runat="server" CssClass="busqueda-input" />
        <asp:Button ID="btnBuscar" runat="server" TextButton="Buscar" TypeButton="Save" OnButtonClick="btnBuscar_ButtonClick" />
    </div>
</div>