<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Checkbox.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Checkbox" %>

<div class="checkbox-wrapper">
    <asp:CheckBox ID="chkInterno" runat="server" CssClass="checkbox-input" 
                  OnCheckedChanged="chkInterno_CheckedChanged" AutoPostBack="true" />
    <asp:Label ID="lblLabel" runat="server" CssClass="checkbox-label" AssociatedControlID="chkInterno" />
    <div class="checkbox-error">
        <asp:Literal ID="litMensajeError" runat="server" Visible="false" />
    </div>
</div>
