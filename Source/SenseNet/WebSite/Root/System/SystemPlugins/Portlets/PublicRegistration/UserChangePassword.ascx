<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.UserChangePassword" %>

<p>Use this form to change your password.</p>

<div class="sn-inputunit ui-helper-clearfix">
	<div class="sn-iu-label">
		<asp:Label AssociatedControlID="NewPassword" CssClass="sn-iu-title">New password:</asp:Label>
	</div>
	<div class="sn-iu-control">
		<asp:TextBox ID="NewPassword" TextMode="Password" Columns="30" runat="server" CssClass="sn-ctrl sn-ctrl-text"></asp:TextBox>
	</div>
</div>
<div class="sn-inputunit ui-helper-clearfix">
	<div class="sn-iu-label">
		<asp:Label AssociatedControlID="ReenteredNewPassword" CssClass="sn-iu-title">Re-enter new password:</asp:Label>
	</div>
	<div class="sn-iu-control">
		<asp:TextBox ID="ReenteredNewPassword" TextMode="Password" Columns="30" runat="server" CssClass="sn-ctrl sn-ctrl-text"></asp:TextBox>
	</div>
</div>
<div class="sn-inputunit ui-helper-clearfix">
	<div class="sn-iu-label">
	</div>
	<div class="sn-iu-control">
        <asp:Button ID="ChangePasswordButton" Text="Change password" runat="server" CssClass="sn-submit" />
	</div>
</div>
<asp:CompareValidator id="PasswordValidator1" runat="server" EnableClientScript="true" ControlToValidate="NewPassword" ControlToCompare="ReenteredNewPassword" Display="Dynamic" ErrorMessage="Passwords must match."></asp:CompareValidator>
<asp:Label ID="Message" runat="server"></asp:Label>