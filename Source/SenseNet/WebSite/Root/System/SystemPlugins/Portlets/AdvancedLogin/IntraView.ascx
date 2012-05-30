<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginView" %>
<asp:LoginView ID="LoginViewControl" runat="server">
    <AnonymousTemplate>
         <asp:Login ID="LoginControl" runat="server" DisplayRememberMe="false" RememberMeSet="false" FailureText='<%$ Resources:LoginPortlet, FailureText %>'>
            <LayoutTemplate>
                <asp:Panel DefaultButton="Login" runat="server">
                    <div class="sn-login">
                        <div class="sn-login-text"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","LoginText") %></div>
                        <asp:Label AssociatedControlID="UserName" CssClass="sn-iu-label" ID="UsernameLabel" runat="server" Text="<%$ Resources:LoginPortlet, UsernameLabel %>"></asp:Label> 
                        <asp:TextBox ID="UserName" CssClass="sn-ctrl sn-login-username" runat="server"></asp:TextBox><br />                
                        <asp:Label AssociatedControlID="Password" CssClass="sn-iu-label" ID="PasswordLabel" runat="server" Text="<%$ Resources:LoginPortlet, PasswordLabel %>"></asp:Label> 
                        <asp:TextBox ID="Password" CssClass="sn-ctrl sn-login-password" runat="server" TextMode="Password"></asp:TextBox><br />
                        <%-- <asp:CheckBox ID="RememberMe" runat="server" Text='<%$ Resources:LoginPortlet,RememberMe %>'></asp:CheckBox> --%>
                        <asp:Button ID="Login" CssClass="sn-submit" CommandName="Login" runat="server" Text='<%$ Resources:LoginPortlet,LoginButtonTitle %>'></asp:Button>&#160;
                        
                        <div class="sn-login-links">
                        <a class="sn-link sn-link-forgotpass" href="/"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","ForgotPass") %></a>
                        <br />
                        <strong><a class="sn-link sn-link-registration" href="/"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","Registration") %></a></strong>
                        </div>

                        <div class="sn-error-msg">
                            <asp:Label ID="FailureText" runat="server"></asp:Label>
                        </div>
                    </div>
                 </asp:Panel>
            </LayoutTemplate>
        </asp:Login>
    </AnonymousTemplate>
    <LoggedInTemplate>
      <div class="sn-login-leftpane">
        <span title="<%= SenseNet.ContentRepository.User.Current.Name%>"><%= String.Format(HttpContext.GetGlobalResourceObject("LoginPortlet","Welcome").ToString(), SenseNet.ContentRepository.User.Current.FullName) %></span>
      </div>
      <div class="sn-login-rightpane">
        <a class="sn-link" href='<%= SenseNet.Portal.PortletFramework.PortalActionLinkResolver.Instance.ResolveRelative(
                                                                            SenseNet.ContentRepository.User.Current.Path,
                                                                            "Edit",
                                                                            System.Web.HttpUtility.UrlEncode(SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.ToString()))%>'>
            <%= HttpContext.GetGlobalResourceObject("LoginPortlet","EditProfile") %></a><br />
      </div>
    </LoggedInTemplate>
</asp:LoginView>