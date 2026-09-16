<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuevaPersona.aspx.cs" Inherits="Grupo08PrograAvanzada.NuevaPersona" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>Nueva Persona Categoría</h1>

    <p class="page-sub">La persona tiene que existir en el padrón para vincularla a un lote y registrarle movimientos.</p>

    <div class="form-card">

        <h3>Identificación de la persona</h3>
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
                <label class="busqueda-label">Fecha de nacimiento</label>
                <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="input-generico" TextMode="Date" />
            </div>
            <div>
                <label class="busqueda-label">Teléfono</label>
                <asp:TextBox ID="txtTelefono" runat="server" CssClass="input-generico" />
            </div>
        </div>

        <div>
            <label class="busqueda-label">E-mail</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="input-generico" />
        </div>

        <h3>Categoría de la persona</h3>
        <asp:CheckBoxList ID="chkCategorias" runat="server" CssClass="categorias-list" RepeatDirection="Horizontal">
            <asp:ListItem Text="Propietario" Value="PROPIETARIO" />
            <asp:ListItem Text="Familiar" Value="FAMILIAR" />
            <asp:ListItem Text="Inquilino" Value="INQUILINO" />
            <asp:ListItem Text="Visita" Value="VISITA" />
            <asp:ListItem Text="Proveedor" Value="PROVEEDOR" />
            <asp:ListItem Text="Servicio doméstico" Value="SERVICIO_DOMESTICO" />
        </asp:CheckBoxList>

        <h3>Lote de la persona</h3>
        <div class="form-row">
            <uc:DropdownList ID="cbLote" runat="server" Titulo="Lote *" Layout="OcupaMas" />
            <div>
                <label class="busqueda-label">Piso / Depto.</label>
                <asp:TextBox ID="txtPisoDepto" runat="server" CssClass="input-generico" />
            </div>
        </div>

        <h3>Vehículo de la persona</h3>
        <uc:Checkbox ID="chkTieneVehiculo" runat="server" Label="La persona tiene vehículo" OnCheckedChanged="chkTieneVehiculo_CheckedChanged" />

        <asp:Panel ID="pnlVehiculo" runat="server" Visible="false">
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Patente *</label>
                    <asp:TextBox ID="txtPatente" runat="server" CssClass="input-generico" />
                </div>
                <div>
                    <label class="busqueda-label">Marca / Modelo *</label>
                    <asp:TextBox ID="txtMarcaModelo" runat="server" CssClass="input-generico" />
                </div>
            </div>
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Compañía de seguro</label>
                    <asp:TextBox ID="txtCompaniaSeguro" runat="server" CssClass="input-generico" />
                </div>
                <div>
                    <label class="busqueda-label">N° de póliza</label>
                    <asp:TextBox ID="txtNumeroPoliza" runat="server" CssClass="input-generico" />
                </div>
            </div>
            <div class="form-row">
                <div>
                    <label class="busqueda-label">Vigencia del seguro</label>
                    <asp:TextBox ID="txtVigenciaSeguro" runat="server" CssClass="input-generico" TextMode="Date" />
                </div>
            </div>
        </asp:Panel>

        <div class="form-actions">
            <uc:Button ID="btnCancelar" runat="server" TextButton="Cancelar" TypeButton="Cancel" OnButtonClick="btnCancelar_ButtonClick" />
            <uc:Button ID="btnGuardar" runat="server" TextButton="Guardar persona" TypeButton="Save" OnButtonClick="btnGuardar_ButtonClick" />
        </div>

    </div>



</asp:Content>
