<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>


<!-- visible in states: L -->
<asp:Button CssClass="sn-submit" ID="SaveCheckin" runat="server"
    Text="Checkin" 
    ToolTip="Checkin" />


<!-- visible in states: A,L,D,P,R -->
<asp:Button CssClass="sn-submit" ID="Cancel" runat="server"
    Text="Cancel"  />
