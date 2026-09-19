<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UsuariosAccesos.aspx.cs" Inherits="Grupo08PrograAvanzada.Administracion.UsuarioAccesos" %>
<%@ Register TagPrefix="uc" TagName="Buscar" Src="Componentes/Buscar.ascx" %>
<%@ Register TagPrefix="uc" TagName="DropdownList" Src="Componentes/DropdownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="Button" Src="Componentes/Button.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>Usuarios y niveles de acceso</h1>
    <p class="page-sub">Quién entra al sistema, con qué nivel de acceso y sobre qué garita. Las pantallas habilitadas salen de NIVEL_ACCESO_URL.</p>

    <div class="form-actions" style="justify-content:flex-end; margin-bottom:16px;">
        <uc:Button ID="btnMostrarNuevoUsuario" runat="server" TextButton="Nuevo usuario" TypeButton="Save" OnButtonClick="btnMostrarNuevoUsuario_Click" />
    </div>

    <div class="filtros-toolbar">
        <uc:Buscar ID="buscarUsuario" runat="server" Titulo="Buscar por DNI"
            Placeholder="Escribí el número de documento..."
            OnBuscarClick="buscarUsuario_BuscarClick" />
        <uc:DropdownList ID="cbFiltroNivel" runat="server" Titulo="Nivel de acceso" Layout="EnFila" />
        <uc:DropdownList ID="cbFiltroEstado" runat="server" Titulo="Estado" Layout="EnFila" />
    </div>

    <div class="form-card">
        <table class="tabla-usuarios">
            <thead>
                <tr>
                    <th>Usuario</th>
                    <th>Persona</th>
                    <th>Nivel de acceso</th>
                    <th>Garita</th>
                    <th>Último acceso</th>
                    <th>Estado</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptUsuarios" runat="server" OnItemDataBound="rptUsuarios_ItemDataBound" OnItemCommand="rptUsuarios_ItemCommand">
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("Usuario") %></td>
                            <td><%# Eval("Persona") %></td>
                            <td>
                                <asp:DropDownList ID="cbNivelAccesoFila" runat="server" CssClass="input-generico" />
                                <asp:Button ID="btnGuardarNivel" runat="server" Text="Guardar" CssClass="btn btn-save"
                                    CommandName="CambiarNivel" CommandArgument='<%# Eval("id_usuario") %>' CausesValidation="false" />
                            </td>
                            <td><%# Eval("Garita") %></td>
                            <td><%# Eval("UltimoAcceso") %></td>
                            <td><%# Eval("Estado") %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>

    <asp:Panel ID="pnlNuevoUsuario" runat="server" Visible="false" CssClass="form-card">
        <h3>Nuevo usuario</h3>
        <p class="page-sub">Sobre una CUENTA ya existente en el padrón, se crea USUARIO + CONTRASENIA.</p>

        <div class="form-row">
            <div style="flex:2;">
                <label class="busqueda-label">DNI de la persona *</label>
                <asp:TextBox ID="txtDniPersona" runat="server" CssClass="input-generico" placeholder="Número de documento..." />
            </div>
            <uc:Button ID="btnVerificarDni" runat="server" TextButton="Verificar" TypeButton="Cancel" OnButtonClick="btnVerificarDni_ButtonClick" />
        </div>
        <asp:Label ID="lblMensajeVerificacion" runat="server" CssClass="page-sub" />
        <asp:HiddenField ID="hdnIdCuenta" runat="server" />

        <asp:Panel ID="pnlDatosUsuario" runat="server" Visible="false">
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Correo *</label>
                    <asp:TextBox ID="txtCorreo" runat="server" CssClass="input-generico" />
                </div>
            </div>
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Contraseña *</label>
                    <asp:TextBox ID="txtContrasena" runat="server" CssClass="input-generico" TextMode="Password" />
                </div>
                <div>
                    <label class="busqueda-label">Repetir contraseña *</label>
                    <asp:TextBox ID="txtRepetirContrasena" runat="server" CssClass="input-generico" TextMode="Password" />
                </div>
            </div>
            <p class="page-sub">El usuario se crea Activo y con nivel de acceso Sin asignar. Asignale el nivel real desde la tabla de arriba una vez creado.</p>
        </asp:Panel>

        <asp:Label ID="lblError" runat="server" CssClass="page-sub" ForeColor="#e05252" />

        <div class="form-actions">
            <uc:Button ID="btnCancelar" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelar_ButtonClick" />
            <uc:Button ID="btnCrearUsuario" runat="server" TextButton="Crear usuario" TypeButton="Save" OnButtonClick="btnCrearUsuario_ButtonClick" />
        </div>
    </asp:Panel>

</asp:Content>