<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Calendario.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.Calendario" %>

<div class="calendario-wrapper">
    <asp:Calendar ID="calendarControl" runat="server" 
        CssClass="calendario-control"
        OnSelectionChanged="calendarControl_SelectionChanged">
    </asp:Calendar>
</div>