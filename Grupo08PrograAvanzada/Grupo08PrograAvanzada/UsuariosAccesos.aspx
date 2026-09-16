<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UsuariosAccesos.aspx.cs" Inherits="Grupo08PrograAvanzada.Administracion.UsuarioAccesos" %>
<%@ Register TagPrefix="uc" TagName="Buscar" Src="Componentes/Buscar.ascx" %>
<%@ Register TagPrefix="uc" TagName="DropdownList" Src="Componentes/DropdownList.ascx" %>
<%@ Register TagPrefix="uc" TagName="Button" Src="Componentes/Button.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    
    <h1>Usuarios y niveles de acceso</h1>
    <p class="page-sub">Quién entra al sistema, con qué nivel de acceso y sobre qué garita.</p>

    <div class="filtros-toolbar">
        <uc:Buscar ID="buscarUsuario" runat="server" Titulo="Buscar usuario o persona"
            Placeholder="Escribí usuario, nombre o documento…"
            OnBuscarClick="buscarUsuario_BuscarClick" />
        <uc:DropdownList ID="cbFiltroNivel" runat="server" Titulo="Nivel de acceso" Layout="EnFila" />
        <uc:DropdownList ID="cbFiltroGarita" runat="server" Titulo="Garita" Layout="EnFila" />
        <uc:DropdownList ID="cbFiltroEstado" runat="server" Titulo="Estado" Layout="EnFila" />
    </div>

    <div class="form-card">
        <table class="tabla-usuarios">
            <thead>
                <tr>
                    <th>Usuario</th>
                    <th>Nivel de acceso</th>
                    <th>Garita</th>
                    <th>Último acceso</th>
                    <th>Estado</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptUsuarios" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("Usuario") %></td>
                            <td><%# Eval("NivelAcceso") %></td>
                            <td><%# Eval("Garita") %></td>
                            <td><%# Eval("UltimoAcceso") %></td>
                            <td><%# Eval("Estado") %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>

    <div class="form-card">
        <h3>Nuevo usuario</h3>

        <div class="form-row">
            <div style="flex:2;">
                <label class="busqueda-label">Persona (CUENTA) *</label>
                <asp:TextBox ID="txtDniPersona" runat="server" CssClass="input-generico" placeholder="Buscar por DNI…" />
            </div>
            <uc:Button ID="btnVerificarDni" runat="server" TextButton="Verificar" TypeButton="Cancel" OnButtonClick="btnVerificarDni_ButtonClick" />
        </div>
        <asp:Label ID="lblMensajeVerificacion" runat="server" CssClass="page-sub" />

        <div class="form-row">
            <div>
                <label class="busqueda-label">Correo *</label>
                <asp:TextBox ID="txtCorreo" runat="server" CssClass="input-generico" />
            </div>
            <uc:DropdownList ID="cbNivelAcceso" runat="server" Titulo="Nivel de acceso *" Layout="OcupaMas" />
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

        <div class="form-row">
            <uc:DropdownList ID="cbGaritaAsignada" runat="server" Titulo="Garita asignada" Layout="OcupaMas" />
            <uc:DropdownList ID="cbTurno" runat="server" Titulo="Turno" Layout="OcupaMas" />
        </div>

        <uc:DropdownList ID="cbEstadoUsuario" runat="server" Titulo="Estado del usuario *" Layout="OcupaMas" />

        <asp:Label ID="lblError" runat="server" CssClass="page-sub" ForeColor="#e05252" />

        <div class="form-actions">
            <uc:Button ID="btnCancelar" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelar_ButtonClick" />
            <uc:Button ID="btnCrearUsuario" runat="server" TextButton="Crear usuario" TypeButton="Save" OnButtonClick="btnCrearUsuario_ButtonClick" />
        </div>
    </div>

</asp:Content>