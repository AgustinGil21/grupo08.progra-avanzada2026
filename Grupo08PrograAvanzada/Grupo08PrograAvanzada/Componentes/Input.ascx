<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Input.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Input" %>

<div class="input-wrapper input-<%= Size.ToString().ToLower() %>">
    <asp:Label ID="lblLabel" runat="server" CssClass="input-label" />
    <asp:TextBox ID="txt_interno" runat="server" CssClass="input-text" 
                 OnTextChanged="txt_interno_TextChanged" AutoPostBack="true" />
    <div class="input-error">
        <asp:Literal ID="litMensajeError" runat="server" Visible="false" />
    </div>
</div>
