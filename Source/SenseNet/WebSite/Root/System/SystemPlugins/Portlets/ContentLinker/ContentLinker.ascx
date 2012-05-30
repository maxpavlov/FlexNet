<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>

<div class="sn-pt-body-border ui-widget-content" >
    <div class="sn-pt-body" >
        You are going to link the contents above under <strong><asp:Label ID="ContentName" runat="server" /></strong>. <br/>
    </div> 
</div>

<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <asp:Button ID="LinkerButton" runat="server" Text="Link contents" CssClass="sn-submit" />
        <sn:BackButton Text="Cancel" ID="CancelButton" runat="server" CssClass="sn-submit" />
    </div>
</div>