<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CambiarContrasenia.aspx.cs" Inherits="Grupo08PrograAvanzada.CambiarContrasenia" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>Cambiar mi contraseña</h1>
    <p class="page-sub">Para cambiarla necesitás saber la contraseña actual.</p>

    <div class="form-card">

        <asp:Panel ID="pnlFormulario" runat="server">

            <div>
                <label class="busqueda-label">Contraseña actual *</label>
                <asp:TextBox ID="txtActual" runat="server" CssClass="input-generico" TextMode="Password" />
            </div>

            <div class="form-row">
                <div>
                    <label class="busqueda-label">Contraseña nueva *</label>
                    <asp:TextBox ID="txtNueva" runat="server" CssClass="input-generico" TextMode="Password" />
                </div>
                <div>
                    <label class="busqueda-label">Repetir contraseña nueva *</label>
                    <asp:TextBox ID="txtNuevaRepetir" runat="server" CssClass="input-generico" TextMode="Password" />
                </div>
            </div>

            <p class="page-sub">Mínimo 8 caracteres, combinando letras y números.</p>

            <asp:Label ID="lblError" runat="server" CssClass="page-sub" ForeColor="#e05252" />

            <div class="form-actions">
                <uc:Button ID="btnCancelar" runat="server" TextButton="Cancelar"
                    TypeButton="Cancel" OnButtonClick="btnCancelar_ButtonClick" />
                <uc:Button ID="btnGuardar" runat="server" TextButton="Cambiar contraseña"
                    TypeButton="Save" OnButtonClick="btnGuardar_ButtonClick" />
            </div>

        </asp:Panel>

    </div>

</asp:Content>
