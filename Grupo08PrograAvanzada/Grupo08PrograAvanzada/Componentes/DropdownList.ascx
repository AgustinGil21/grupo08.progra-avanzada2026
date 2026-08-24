<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DropdownList.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.DropdownList" %>

<div class="combo-wrapper combo-<%= Layout.ToString().ToLower() %>">
    <asp:Label ID="lblTitulo" runat="server" CssClass="combo-label" />
    <asp:DropDownList ID="ddl" runat="server" CssClass="combo-select" />
</div>
