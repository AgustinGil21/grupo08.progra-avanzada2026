<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Table.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Table" %>

<div class="table-container">

    <table class="table table-hover">
    
        <thead>
            <asp:Repeater ID="rpt_header" runat="server">
                <HeaderTemplate><tr></HeaderTemplate>
                    <ItemTemplate>
                        <th><%# Eval("Titulo") %></th>
                    </ItemTemplate>
                <FooterTemplate></tr></FooterTemplate>
            </asp:Repeater>
        </thead>
    
        <tbody>
            <asp:Repeater ID="rpt_filas" runat="server" OnItemDataBound="rpt_filas_ItemDataBound">
                <ItemTemplate>
                    <tr>
                        <asp:PlaceHolder ID="ph_celdas" runat="server" />
                    </tr>
                </ItemTemplate>
            </asp:Repeater>
        </tbody>

    </table>

    <asp:Label ID="lb_vacio" runat="server" CssClass="table-container-empty" Visible="false"
        Text="No hay registros para mostrar" />

</div>