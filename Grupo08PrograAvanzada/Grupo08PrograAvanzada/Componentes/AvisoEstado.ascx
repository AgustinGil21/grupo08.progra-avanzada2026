<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AvisoEstado.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.AvisoEstado" %>


<div class="aviso-estado aviso-<%= TipoAviso.ToString().ToLower() %>">
    <div class="aviso-texto">
        <div class="aviso-titulo"><%= Titulo %></div>
        <div class="aviso-detalle"><%= Detalle %></div>
    </div>
    <div class="aviso-accion">
        <asp:Button ID="btnAccion" runat="server" 
            ButtonSize="Chico" 
            OnButtonClick="btnAccion_Click" />
    </div>
</div>

