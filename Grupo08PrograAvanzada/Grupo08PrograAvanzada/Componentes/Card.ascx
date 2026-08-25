<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Card.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Card" %>

<div class="card-caja">
    <div class="card-titulo" runat="server" id="div_titulo" visible="false">
        <asp:Literal ID="lit_titulo" runat="server"></asp:Literal>
    </div>

    <div class="card-body">
        <asp:PlaceHolder ID="ph_body" runat="server"/>
    </div>
</div>