<%@ Page Title="Vehiculos" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Vehiculos.aspx.cs" Inherits="Grupo08PrograAvanzada.Vehiculos" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <uc:Buscar ID="ucBuscar" runat="server" Titulo="Vehículos" Placeholder="Buscar por patente..." OnBuscarClick="ucBuscar_BuscarClick">
        <Filters>
            <uc:DropdownList ID="cbEstado" runat="server" Titulo="Estado del seguro" Layout="EnFila" />
            <uc:Checkbox ID="chkVerSoloVencidos" runat="server" Label="Ver solo vencidos" />
        </Filters>
    </uc:Buscar>

    <div class="table-container">
        <table class="table table-hover">
            <thead>
                <tr>
                    <th>Patente</th>
                    <th>Descripción</th>
                    <th>Persona</th>
                    <th>Compañía</th>
                    <th>Póliza</th>
                    <th>Vigencia</th>
                    <th>Estado</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater ID="rptVehiculos" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("patente") %></td>
                            <td><%# Eval("descripcion") %></td>
                            <td><%# Eval("Titular") %></td>
                            <td><%# Eval("CompaniaSeguro") %></td>
                            <td><%# Eval("poliza_seguro") %></td>
                            <td><%# Eval("vigencia_poliza_seguro", "{0:dd/MM/yyyy}") %></td>
                            <td>
                                <span class='<%# (string)Eval("EstadoPoliza") == "Vigente" ? "aviso-ok" : "aviso-alerta" %>'>
                                    <%# Eval("EstadoPoliza") %>
                                </span>
                            </td>
                            <td>
                                <asp:HiddenField ID="hdnIdVehiculoFila" runat="server" Value='<%# Eval("id_vehiculo") %>' />
                                <uc:Button ID="btnActualizarFila" runat="server" TextButton="Actualizar" TypeButton="Save" ButtonSize="Small"
                                    OnButtonClick="btnActualizarFila_ButtonClick" />
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <asp:Label ID="lblVacio" runat="server" CssClass="table-container-empty" Visible="false" Text="No hay vehículos para mostrar" />
    </div>

    <uc:Modal ID="mdlActualizarPoliza" runat="server" Tittle="Actualizar póliza de seguro">
        <Content>
            <asp:HiddenField ID="hdnIdVehiculo" runat="server" />
            <uc:DropdownList ID="cbCompaniaSeguroModal" runat="server" Titulo="Compañía de seguro" />
            <uc:Input ID="txtNumeroPolizaModal" runat="server" Label="Número de póliza" />
            <uc:Input ID="txtVigenciaSeguroModal" runat="server" Label="Vigencia" Type="Date" />
            <asp:Label ID="lblErrorModal" runat="server" CssClass="text-danger" />
            <uc:Button ID="btnGuardarPoliza" runat="server" TextButton="Guardar" TypeButton="Save" OnButtonClick="btnGuardarPoliza_ButtonClick" />
        </Content>
    </uc:Modal>

</asp:Content>