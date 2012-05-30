<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginView" %>
<sn:ContextInfo runat="server" Selector="CurrentUser" UsePortletContext="false" ID="myContext" />
<div id="snm-container" class="bg-semitransparent">
     <div id="page1" class="snm-page">
        <div id="snm-loginpage" class="snm-col snm-col-wide4">

<asp:LoginView ID="LoginViewControl" runat="server">
    <AnonymousTemplate>
         <asp:Login ID="LoginControl" runat="server" DisplayRememberMe="false" RememberMeSet="false" FailureText='<%$ Resources:LoginPortlet, FailureText %>' RenderOuterTable="false">
            <LayoutTemplate>
                <div class="snm-tile snm-tile-wide2 snm-tile-tall2">
                    <span class="snm-background"><img src="/Root/Skins/snmobile/images/logotile.png" width="176" height="176" alt="Sense/Net" /></span>
                </div>
                <asp:Panel CssClass="snm-tile snm-tile-wide2 snm-tile-tall2" DefaultButton="Login" runat="server">
                        <span id="snm-loginfields" class="snm-tile snm-tile-wide2">
                            <asp:TextBox ID="UserName" Text="username" onfocus="if(this.value=='username') this.value=''" onblur="if(this.value=='') this.value='username'" CssClass="snm-ctrl snm-username" runat="server" />               
                            <asp:TextBox ID="Password" Text="password" onfocus="if(this.value=='password') { this.value=''; this.type='password'; }" onblur="if(this.value=='') { this.value='password'; this.type='text'; }" CssClass="snm-ctrl snm-password" runat="server" />
                        </span>
                        <asp:Button ID="Login" CssClass="snm-tile snm-tile-wide2 snm-submit" CommandName="Login" runat="server" Text='<%$ Resources:LoginPortlet,LoginButtonTitle %>'></asp:Button>
                 </asp:Panel>
                 <div class="snm-tile snm-tile-wide2 snm-floatright snm-error"><asp:Label ID="FailureText" CssClass="snm-fontsize4" runat="server"></asp:Label></div>
            </LayoutTemplate>
        </asp:Login>
    </AnonymousTemplate>
    <LoggedInTemplate>
        <div id="snm-loggedin" class="snm-tile snm-tile-wide5">
            <div class="snm-floatleft snm-avatar"><img src="<%= SenseNet.Portal.UI.UITools.GetAvatarUrl() %>?dynamicThumbnail=1&width=80&height=80" alt="" title="<%= SenseNet.ContentRepository.User.Current.FullName %>" /></div>
            <h2>Welcome <%= SenseNet.ContentRepository.User.Current.FullName %>!</h2>
            <span class="snm-fontsize3"><asp:LoginStatus ID="LoginStatusControl" LogoutText="<%$ Resources:LoginPortlet,Logout %>" LogoutPageUrl="/" LogoutAction="Redirect" runat="server" CssClass="snm-logout" /> &nbsp;/&nbsp; <a href="/">Back to Home</a></span>
        </div>
    </LoggedInTemplate>
</asp:LoginView>
        </div>
    </div>
</div>
