<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Buscar.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Buscar" %>
<%@ Register TagPrefix="uc" TagName="Button" Src="Button.ascx" %>

<div class="busqueda-wrapper">

    <div class="busqueda-fila">

        <div class="busqueda-campo busqueda-campo-input">
            <asp:Label ID="lblTitulo" runat="server" CssClass="busqueda-label" />
            <asp:TextBox ID="txtBuscar" runat="server" CssClass="busqueda-input" />
        </div>

        <asp:PlaceHolder ID="ph_filtros" runat="server" />
        <uc:Button ID="btnBuscar" runat="server" TextButton="Buscar" ButtonSize="Small" TypeButton="Save" OnButtonClick="btnBuscar_ButtonClick" />
    </div>

</div>