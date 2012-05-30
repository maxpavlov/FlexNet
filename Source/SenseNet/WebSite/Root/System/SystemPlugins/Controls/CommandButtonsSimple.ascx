<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>

  <!-- visible in states: A -->
<asp:Button CssClass="sn-button sn-submit" ID="CheckoutSaveCheckin" runat="server"
    Text="Ok" 
    ToolTip="Checkout & Save & Checkin" />

<div style="display:none;">
  <!-- visible in states: A,D,P,R -->
  <asp:Button CssClass="sn-button sn-submit" ID="CheckoutSave" runat="server"
  Text="Save Draft" 
  ToolTip="Checkout & Save" />
  
  <!-- visible in states: L -->
  <asp:Button CssClass="sn-button sn-submit" ID="Save" runat="server"
  Text="Save Draft" 
  ToolTip="Save" />
  
  
  <!-- visible in states: L -->
  <asp:Button CssClass="sn-button sn-submit" ID="SaveCheckin" runat="server"
  Text="Done Editing" 
  ToolTip="Save & Checkin" />
  
  <!-- visible in states: D -->
  <asp:Button CssClass="sn-button sn-submit" ID="Publish" runat="server"
  Text="Publish" 
  ToolTip="Save & Publish" />
  
  <!-- visible in states: A,L,D,P,R -->
  <asp:Button CssClass="sn-button sn-submit" ID="Cancel" runat="server"
  Text="Cancel"  />
</div>