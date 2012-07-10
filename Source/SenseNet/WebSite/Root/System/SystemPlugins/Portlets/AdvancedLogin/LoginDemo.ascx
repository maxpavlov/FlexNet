<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginDemo" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<h1>Login as</h1>
<ul>
    <li>
        <asp:LinkButton ID="link1" runat="server" CommandArgument="Demo\\alba" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Demo", "alba");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
                <img src=<%=avatar1 %> width="36" height="36" />
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1 == null ? string.Empty : user1.FullName%> (Manager)</div>
            <div class="sn-logindemo-userdata">
                 <small>She can create and edit workspaces, add and modify documents.</small>
                 <small class="sn-link">username: <strong>alba</strong> / password: <strong>alba</strong></small>
            </div>
        </div>
        <div style="clear:both;"></div>
        </asp:LinkButton>
    </li>
    <li>
        <asp:LinkButton ID="link2" runat="server" CommandArgument="Demo\\mike" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Demo", "mike");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
                <img src=<%=avatar1 %> width="36" height="36" />
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1 == null ? string.Empty : user1.FullName%> (Developer)</div>
            <div class="sn-logindemo-userdata">
                <small>He can create new pagetemplates, views, content types.</small>
                <small class="sn-link">username: <strong>mike</strong> / password: <strong>mike</strong></small>
            </div>
        </div>
        <div style="clear:both;"></div>
        </asp:LinkButton>
    </li>
    <li class="last">
        <asp:LinkButton ID="link3" runat="server" CommandArgument="BuiltIn\\MaxPavlov" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Builtin", "MaxPavlov");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=90&height=90";
        %>
        <div class="sn-logindemo-leftdiv">
                <img src=<%=avatar1 %> width="36" height="36" />
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1 == null ? string.Empty : user1.FullName%> (Админ)</div>
            <div class="sn-logindemo-userdata">
                <small>Имеет полный контроль над системой.</small>
                <small class="sn-link">имя: <strong>maxpavlov</strong> / пароль: <strong>maximpavlov</strong></small>
            </div>
        </div>
        <div style="clear:both;"></div>
        </asp:LinkButton>
    </li>
</ul>
