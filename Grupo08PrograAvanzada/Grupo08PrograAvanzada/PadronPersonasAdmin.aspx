<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PadronPersonasAdmin.aspx.cs" Inherits="Grupo08PrograAvanzada.PadronPersonasAdmin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">

        <div>
            <h2>Padrón de personas y lotes</h2>
        </div>

        <uc:Button ID="btnNuevaPersona" runat="server" TextButton="Nueva persona"
            TypeButton="Save" OnButtonClick="btnNuevaPersona_ButtonClick" />
    </div>

    <uc:Buscar ID="ucBuscar" runat="server" Titulo="Buscar persona"
        Placeholder="Escribí DNI.."
        OnBuscarClick="ucBuscar_BuscarClick">
        <Filters>
            <uc:DropdownList ID="ddlCategoria" runat="server" Titulo="Categoría" Layout="EnFila" />
        </Filters>
    </uc:Buscar>

    <uc:Table ID="ucTablaPersonas" runat="server" />
</asp:Content>
