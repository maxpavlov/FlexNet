<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.UserChangePassword" %>

Change password<br />
<p>Use this form to change your password.</p>
<p>New password: <asp:TextBox ID="NewPassword" TextMode="Password" runat="server" Columns="30" CssClass=""></asp:TextBox></p>
<p>Re-enter new password:<asp:TextBox ID="ReenteredNewPassword"  TextMode="Password" runat="server" Columns="30" CssClass=""></asp:TextBox></p>
<p><asp:Button ID="ChangePasswordButton" Text="Change password" runat="server" /></p>
<asp:CompareValidator id="PasswordValidator1" runat="server" EnableClientScript="true" ControlToValidate="NewPassword" ControlToCompare="ReenteredNewPassword" Display="Dynamic" ErrorMessage="Passwords must match."></asp:CompareValidator>
<asp:Label id="Message" runat="server"></asp:Label>