<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginView" %>
<asp:LoginView ID="LoginViewControl" runat="server">
    <AnonymousTemplate>
        <asp:Login ID="LoginControl" runat="server" DisplayRememberMe="false" RememberMeSet="false">
            <LayoutTemplate>
                <asp:TextBox ID="UserName" runat="server"></asp:TextBox>
<%--                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" ControlToValidate="UserName"
                    Text="*"></asp:RequiredFieldValidator>
--%>                   
<%--                     <asp:RegularExpressionValidator ID="UserNameReqexp" runat="server"     
                                    ErrorMessage="This expression does not validate." 
                                    ControlToValidate="UserName"     
                                    ValidationExpression="^[a-zA-Z'.\s]{1,40}$" />
--%>
                    
                <asp:TextBox ID="Password" runat="server" TextMode="Password"></asp:TextBox>
<%--                <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" ControlToValidate="Password"
                    Text="*"></asp:RequiredFieldValidator>
               <asp:CheckBox ID="RememberMe" runat="server" Text="Remember my login"></asp:CheckBox>--%> 
                <asp:Button ID="Login" CommandName="Login" runat="server" Text="Login"></asp:Button>
                <%--<asp:Literal ID="FailureText" runat="server"></asp:Literal>--%>
            </LayoutTemplate>
        </asp:Login>
    </AnonymousTemplate>
    <LoggedInTemplate>
        Logged in as
        <asp:LoginName 
                ID="LoginNameControl" runat="server" /><br />
        <asp:LoginStatus ID="LoginStatusControl" LogoutPageUrl="" LogoutAction="Redirect" runat="server" />
    </LoggedInTemplate>
</asp:LoginView>