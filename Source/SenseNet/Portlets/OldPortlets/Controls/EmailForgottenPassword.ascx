<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.EmailForgottenPassword" %>

Reset Password<br />
<p>Please enter the email address you used when you created your account, and we will send you an email with further instructions.</p>
<p>Email address: <asp:TextBox ID="ResetEmailAddress" runat="server" Columns="30" CssClass=""></asp:TextBox></p>
<asp:RequiredFieldValidator ID="ResetEmailAddressValidator" runat="server" ControlToValidate="ResetEmailAddress" ErrorMessage="*"></asp:RequiredFieldValidator>
<p><asp:Button ID="ResetPasswordButton" Text="Reset password" runat="server" /></p>
<asp:Label id="Message" runat="server"></asp:Label>