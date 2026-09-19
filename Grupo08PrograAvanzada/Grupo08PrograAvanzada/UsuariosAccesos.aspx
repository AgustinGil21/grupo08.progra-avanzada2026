<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UsuariosAccesos.aspx.cs" Inherits="Grupo08PrograAvanzada.Administracion.UsuarioAccesos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="page-header">
        <div>
            <h1>Usuarios y niveles de acceso</h1>
        </div>
        <button type="button" class="btn btn-save" onclick="mostrarModal('<%= mdlNuevoUsuario.OverlayClientID %>')">Nuevo usuario</button>
    </div>

    <uc:Buscar ID="buscarUsuario" runat="server" Titulo="Buscar por DNI"
        Placeholder="Escribí el número de documento..."
        OnBuscarClick="buscarUsuario_BuscarClick">
        <Filters>
            <uc:DropdownList ID="cbFiltroNivel" runat="server" Titulo="Nivel de acceso" Layout="EnFila" />
            <uc:DropdownList ID="cbFiltroEstado" runat="server" Titulo="Estado" Layout="EnFila" />
        </Filters>
    </uc:Buscar>

    <div class="form-card">
        <table class="tabla-usuarios">
            <thead>
                <tr>
                    <th>Usuario</th>
                    <th>Persona</th>
                    <th>DNI</th>
                    <th>Nivel de acceso</th>
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
                            <td><%# Eval("nro_documento") %></td>
                            <td>
                                <asp:DropDownList ID="cbNivelAccesoFila" runat="server" CssClass="input-generico" />
                                <asp:Button ID="btnGuardarNivel" runat="server" Text="Guardar" CssClass="btn btn-save"
                                    CommandName="CambiarNivel" CommandArgument='<%# Eval("id_usuario") %>' CausesValidation="false" />
                            </td>
                            <td><%# Eval("UltimoAcceso") %></td>
                            <td><%# Eval("Estado") %></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>

    <uc:Modal ID="mdlNuevoUsuario" runat="server" Tittle="Nuevo usuario">
        <Content>
            <p class="page-sub">Nuevo usuario.</p>

            <div class="form-row">
                <uc:DropdownList ID="cbTipoDocumento" runat="server" Titulo="Tipo de documento *" Layout="OcupaMas" />
                <div>
                    <label class="busqueda-label">Número de documento *</label>
                    <asp:TextBox ID="txtNumeroDocumento" runat="server" CssClass="input-generico" />
                </div>
            </div>

            <uc:DropdownList ID="cbNacionalidad" runat="server" Titulo="Nacionalidad *" Layout="OcupaMas" />

            <div class="form-row">
                <div>
                    <label class="busqueda-label">Apellido *</label>
                    <asp:TextBox ID="txtApellido" runat="server" CssClass="input-generico" />
                </div>
                <div>
                    <label class="busqueda-label">Nombres *</label>
                    <asp:TextBox ID="txtNombres" runat="server" CssClass="input-generico" />
                </div>
            </div>

            <div class="form-row">
                <div>
                    <label class="busqueda-label">Fecha de nacimiento *</label>
                    <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="input-generico" TextMode="Date" />
                </div>
                <div>
                    <label class="busqueda-label">Teléfono</label>
                    <asp:TextBox ID="txtTelefono" runat="server" CssClass="input-generico" />
                </div>
            </div>

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

            <asp:Label ID="lblError" runat="server" CssClass="page-sub" ForeColor="#e05252" />

            <div class="form-actions">
                <uc:Button ID="btnCancelar" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelar_ButtonClick" />
                <uc:Button ID="btnCrearUsuario" runat="server" TextButton="Crear usuario" TypeButton="Save" OnButtonClick="btnCrearUsuario_ButtonClick" />
            </div>
        </Content>
    </uc:Modal>

</asp:Content>