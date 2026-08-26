<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Modal.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Modal" %>

<div class="modal-overlay" id="overlay" runat="server">
    <div class="modal-box">
        <asp:Literal ID="lit_tittle" runat="server" />
        <button 
            type="button" 
            class="modal-cerrar" 
            onclick="cerrarModal('<%= overlay.ClientID %>')">
            <i class="bi bi-x-lg"></i>
        </button>
    </div>

    <div class="modal-body">
        <asp:PlaceHolder ID="ph_body" runat="server" />
    </div>
</div>