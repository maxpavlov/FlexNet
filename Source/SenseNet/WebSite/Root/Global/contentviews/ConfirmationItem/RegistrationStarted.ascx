<%@  Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>

Thank you, we sent you a confirmation email. <br />
Please check your mailbox and click on the confirmation link in the email to continue the process.

<div class="sn-panel sn-buttons">
    <asp:Button CssClass="sn-submit" Text="Ok" ID="Confirm" runat="server" OnClientClick="location.href='/';return false;" />
</div>
