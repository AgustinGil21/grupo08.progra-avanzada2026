<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PadronPersonas.aspx.cs" Inherits="Grupo08PrograAvanzada.PadronPersonas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">

        <div>
            <h1>Padron de personas</h1>
        </div>

        <uc:Button ID="btnNuevaPersona" runat="server" TextButton="Nueva persona"
            TypeButton="Save" OnButtonClick="btnNuevaPersona_ButtonClick" />
    </div>

    <uc:Buscar ID="ucBuscar" runat="server" Titulo="Buscar persona"
        Placeholder="Escribí nombre o apellido…"
        OnBuscarClick="ucBuscar_BuscarClick">
        
        <Filters>
            <uc:DropdownList ID="ddlCategoria" runat="server" Titulo="Categoría" Layout="EnFila" />
        </Filters>
    
    </uc:Buscar>

    <uc:Table ID="ucTablaPersonasG" runat="server" />
</asp:Content>
