<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginView" %>

<sn:ContextInfo runat="server" Selector="CurrentUser" UsePortletContext="false" ID="myContext" />

<asp:LoginView ID="LoginViewControl" runat="server">
    <AnonymousTemplate>
         <asp:Login ID="LoginControl" Width="100%" runat="server" DisplayRememberMe="false" RememberMeSet="false" FailureText='<%$ Resources:LoginPortlet, FailureText %>'>
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
                        <%--<a class="sn-link sn-link-forgotpass" href="/forgotpassword"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","ForgotPass") %></a>
                        <br />--%>
                        <strong><a class="sn-link sn-link-registration" href="/Publicregistration"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","Registration") %></a></strong>
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
        <div class="sn-loggedin">
            <%= HttpContext.GetGlobalResourceObject("LoginPortlet","LoggedIn") %>
            <div class="sn-panel">
                <div class="sn-avatar sn-floatleft"><img class="sn-icon sn-icon32" src="<%= SenseNet.Portal.UI.UITools.GetAvatarUrl() %>?dynamicThumbnail=1&width=32&height=32" alt="" title="<%= SenseNet.ContentRepository.User.Current.FullName %>" /></div>
                <strong><%= SenseNet.ContentRepository.User.Current.FullName %></strong><br />
                <sn:ActionLinkButton ID="ProfileLink" IconVisible="false" runat="server" ActionName="Profile" Text="My profile" ContextInfoID="myContext" /> 
            </div>
            <hr />
            <asp:LoginStatus ID="LoginStatusControl" LogoutText="<%$ Resources:LoginPortlet,Logout %>" LogoutPageUrl="/" LogoutAction="Redirect" runat="server" CssClass="sn-link sn-logout" />
        </div>
    </LoggedInTemplate>
</asp:LoginView>