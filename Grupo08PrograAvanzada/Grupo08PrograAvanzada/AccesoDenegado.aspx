<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="Grupo08PrograAvanzada.AccesoDenegado" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <uc:Card ID="cardAcceso" runat="server">
        <Contenido>
            <h2>Acceso Denegado</h2>
            <p>No tenés permiso para ver esta página con tu rol actual.</p>

            <uc:Button ID="btnVolver" runat="server" TextButton="Volver al inicio" 
                TypeButton="Save" OnButtonClick="btnVolver_Click" />
        </Contenido>
    </uc:Card>
</asp:Content>
