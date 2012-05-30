<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.EmailForgottenPassword" %>

<p>Please enter the email address you used when you created your account, and we will send you an email with further instructions.</p>

<div class="sn-inputunit ui-helper-clearfix">
	<div class="sn-iu-label">
		<asp:Label AssociatedControlID="ResetEmailAddress" CssClass="sn-iu-title" ID="NPLabel" runat="server">Email address:</asp:Label>
	</div>
	<div class="sn-iu-control">
		<asp:TextBox ID="ResetEmailAddress" runat="server" CssClass="sn-ctrl sn-ctrl-text"></asp:TextBox>
	</div>
</div>
<div class="sn-inputunit ui-helper-clearfix">
	<div class="sn-iu-label">
	</div>
	<div class="sn-iu-control">
        <asp:Button class="sn-submit" ID="ResetPasswordButton" Text="Reset password" runat="server" />
	</div>
</div>
<asp:Label ID="Message" runat="server"></asp:Label>