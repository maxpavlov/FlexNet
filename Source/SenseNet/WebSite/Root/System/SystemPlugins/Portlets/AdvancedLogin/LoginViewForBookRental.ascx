<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginView" %>

<sn:ContextInfo runat="server" Selector="CurrentUser" UsePortletContext="false" ID="myContext" />

<asp:LoginView ID="LoginViewControl" runat="server">
    <AnonymousTemplate>
         <asp:Login ID="LoginControl" runat="server" DisplayRememberMe="false" RememberMeSet="false" FailureText='<%$ Resources:LoginPortlet, FailureText %>'>
            <LayoutTemplate>
                <asp:Panel ID="Panel1" DefaultButton="Login" runat="server">
                    <div class="sn-login">
                        <div class="sn-login-text"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","LoginText") %></div>
                        <asp:Label AssociatedControlID="UserName" CssClass="sn-iu-label" ID="UsernameLabel" runat="server" Text="<%$ Resources:LoginPortlet, UsernameLabel %>"></asp:Label> 
                        <asp:TextBox ID="UserName" CssClass="sn-ctrl sn-login-username" runat="server"></asp:TextBox><br />                
                        <asp:Label AssociatedControlID="Password" CssClass="sn-iu-label" ID="PasswordLabel" runat="server" Text="<%$ Resources:LoginPortlet, PasswordLabel %>"></asp:Label> 
                        <asp:TextBox ID="Password" CssClass="sn-ctrl sn-login-password" runat="server" TextMode="Password"></asp:TextBox><br />
                        <%-- <asp:CheckBox ID="RememberMe" runat="server" Text='<%$ Resources:LoginPortlet,RememberMe %>'></asp:CheckBox> --%>
                        <asp:Button ID="Login" CssClass="sn-submit" CommandName="Login" runat="server" Text='<%$ Resources:LoginPortlet,LoginButtonTitle %>'></asp:Button>&#160;

                        <div class="sn-error-msg">
                            <asp:Label ID="FailureText" runat="server"></asp:Label>
                        </div>
                    </div>
                 </asp:Panel>
            </LayoutTemplate>
        </asp:Login>
    </AnonymousTemplate>
    <LoggedInTemplate>
        <%= HttpContext.GetGlobalResourceObject("LoginPortlet","LoggedIn") %> <strong><%= SenseNet.ContentRepository.User.Current.Name%></strong>
        <p style="margin: 1em 0;">
            <sn:ActionLinkButton ID="EditProfileLink" runat="server" ActionName="EditProfile" Text="Edit profile" ContextInfoID="myContext" />
            /
            <a class="snTextLink" href="/Book_Rental_Demo_Site/MyBooks">My Books</a>
            <br />
            <asp:LoginStatus ID="LoginStatusControl" LogoutText="<%$ Resources:LoginPortlet,Logout %>" LogoutPageUrl="/Book_Rental_Demo_Site" LogoutAction="Redirect" runat="server" CssClass="sn-link sn-logout" />
        </p>
    </LoggedInTemplate>
</asp:LoginView>