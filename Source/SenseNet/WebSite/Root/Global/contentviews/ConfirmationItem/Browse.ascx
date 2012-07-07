<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>

<%= this.Content["Description"] %>

<div class="sn-panel sn-buttons">
    <asp:Button CssClass="sn-submit" Text="Ok" ID="Ok" runat="server" OnClientClick="location.href='/';return false;" />
</div>
