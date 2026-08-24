<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Button.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Button" %>

<asp:LinkButton ID="btn_interno" runat="server" OnClick="btn_interno_Click">
    <asp:Literal ID="lit_texto_btn" runat="server"></asp:Literal>
</asp:LinkButton>