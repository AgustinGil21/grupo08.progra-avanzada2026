<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Card.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Card" %>

<div class="card-box">
    <div class="card-tittle" runat="server" id="div_titulo" visible="false">
        <asp:Literal ID="lit_tittle" runat="server"></asp:Literal>
    </div>

    <div class="card-body">
        <asp:PlaceHolder ID="ph_body" runat="server"/>
    </div>
</div>