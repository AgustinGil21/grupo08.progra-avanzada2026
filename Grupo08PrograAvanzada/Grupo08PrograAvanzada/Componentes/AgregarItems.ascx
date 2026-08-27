<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AgregarItems.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.AgregarItems" %>

<div class="items-wrapper">

    <asp:Repeater ID="rptItems" runat="server">
        <ItemTemplate>
            <div class="items-fila">
                <span class="items-icono"><%# Eval("Icono") %></span>

                <div class="items-textos">
                    <span class="items-titulo"><%# Eval("Titulo") %></span>
                    <span class="items-subtitulo"><%# Eval("Subtitulo") %></span>
                </div>

                <asp:HiddenField ID="hidId" runat="server" Value='<%# Eval("Id") %>' />
                <uc:Button ID="btnQuitar" runat="server" TextButton="×" TypeButton="Cancel" ButtonSize="Small" OnButtonClick="BtnQuitar_ButtonClick" />
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <uc:Button ID="btnAgregar" runat="server" TypeButton="Save" OnButtonClick="BtnAgregar_ButtonClick" />

</div>