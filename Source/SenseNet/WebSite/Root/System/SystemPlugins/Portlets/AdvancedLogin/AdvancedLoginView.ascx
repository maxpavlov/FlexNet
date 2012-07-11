<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginView" %>
<%@ Register Src="~/Root/System/SystemPlugins/Portlets/AdvancedLogin/LoginDemo.ascx" TagPrefix="sn" TagName="LoginDemo" %>

<sn:ContextInfo runat="server" Selector="CurrentUser" UsePortletContext="false" ID="myContext" />

<script>
    $(document).ready(function () {
        $(".sn-login-link").click(
        function () {
            if (!$(".sn-login-form").hasClass("show")) {
                $('.sn-login-form').addClass('show');
                $('.sn-login-form').fadeIn('1000');
                $('.sn-login-form-arrow').addClass('show');
                $('.sn-login-form-arrow').fadeIn('1000');
            }
            else {
                $('.sn-login-form').fadeOut('1000');
                $('.sn-login-form').removeClass('show');
                $('.sn-login-form-arrow').fadeOut('1000');
                $('.sn-login-form-arrow').removeClass('show');
            }

        });

        $(".sn-login .sn-error-msg span").each(function () {
            var value = $.trim($(this).text())
            if (value != "") {
                $('.sn-login-form').addClass('show');
                $('.sn-login-form').fadeIn('1000');
                $('.sn-login-form-arrow').addClass('show');
                $('.sn-login-form-arrow').fadeIn('1000')
            }
        })
    });
</script>

<asp:LoginView ID="LoginViewControl" runat="server">
    <AnonymousTemplate>
    <div class="sn-login-link"><%= HttpContext.GetGlobalResourceObject("User","LoginText") %></div>
    <div class="sn-login-form-arrow" style="display: none;"></div>
    <div class="sn-login-form" style="display: none;">
         <asp:Login ID="LoginControl" Width="100%" runat="server" DisplayRememberMe="false" RememberMeSet="false" FailureText='<%$ Resources:LoginPortlet, FailureText %>'>
            <LayoutTemplate>
                <asp:Panel DefaultButton="Login" runat="server">
                    <div class="sn-login">
<%--                        <div class="sn-login-text"><strong><%= HttpContext.GetGlobalResourceObject("LoginPortlet","LoginText") %></strong></div>
--%>                    <div class="sn-login-inner">
                    <div class="sn-login-form-row"> 
                        <asp:Label AssociatedControlID="UserName" CssClass="sn-iu-label" ID="UsernameLabel" runat="server" Text="<%$ Resources:LoginPortlet, UsernameLabel %>"></asp:Label> 
                        <asp:TextBox ID="UserName" CssClass="sn-ctrl sn-login-username" runat="server"></asp:TextBox><br />                
                    </div>    
                    <div class="sn-login-form-row">
                            <asp:Label AssociatedControlID="Password" CssClass="sn-iu-label" ID="PasswordLabel" runat="server" Text="<%$ Resources:LoginPortlet, PasswordLabel %>"></asp:Label> 
                            <asp:TextBox ID="Password" CssClass="sn-ctrl sn-login-password" runat="server" TextMode="Password"></asp:TextBox>
                        <%-- <asp:CheckBox ID="RememberMe" runat="server" Text='<%$ Resources:LoginPortlet,RememberMe %>'></asp:CheckBox> --%>
                       

                    </div>   

                        <div class="sn-login-links">
                        <%--<a class="sn-link sn-link-forgotpass" href="/forgotpassword"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","ForgotPass") %></a>
                        <br />--%>
                        <strong><a class="sn-link sn-link-registration" href="/Publicregistration"><%= HttpContext.GetGlobalResourceObject("LoginPortlet","Registration") %></a></strong>
                       </div>     
                                                                        <asp:Button ID="Login" CssClass="sn-submit" CommandName="Login" runat="server" Text='<%$ Resources:LoginPortlet,LoginButtonTitle %>'></asp:Button>&#160;

                       </div>
                       <div class="sn-error-msg">
                            <asp:Label ID="FailureText" runat="server"></asp:Label>
                        </div>
                        <div class="sn-login-demo">
                            <sn:LoginDemo ID="asdf" runat="server" />
                        </div>
                        
                    </div>
                 </asp:Panel>
            </LayoutTemplate>
        </asp:Login>
        </div>
    </AnonymousTemplate>
    <LoggedInTemplate>
        <div class="sn-loggedin">
            <%--<%= HttpContext.GetGlobalResourceObject("LoginPortlet","LoggedIn") %>--%>
            <div class="sn-panel">
                <div class="sn-avatar sn-floatleft"><img class="sn-icon sn-icon32" src="<%= SenseNet.Portal.UI.UITools.GetAvatarUrl() %>?dynamicThumbnail=1&width=32&height=32" alt="" title="<%= SenseNet.ContentRepository.User.Current.FullName %>" /></div>
                <div class="sn-userName">
                    <sn:ActionMenu id="UserActions" ContextInfoID="myContext" runat="server" Scenario="UserActions">
                        <%= SenseNet.ContentRepository.User.Current.FullName %>
                    </sn:ActionMenu>
                </div>   
                <div style="clear: both;font-size: 1px;">&#160;</div>         
            </div>
            <%--<hr />--%>
            <%--<asp:LoginStatus ID="LoginStatusControl" LogoutText="<%$ Resources:LoginPortlet,Logout %>" LogoutPageUrl="/" LogoutAction="Redirect" runat="server" CssClass="sn-link sn-logout" />--%>
        </div>
    </LoggedInTemplate>
</asp:LoginView>