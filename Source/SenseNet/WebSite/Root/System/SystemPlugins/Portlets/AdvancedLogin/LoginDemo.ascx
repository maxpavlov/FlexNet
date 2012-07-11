<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.Controls.LoginDemo" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<h1>Login as</h1>
<ul>
    <li>
        <asp:LinkButton ID="link1" runat="server" CommandArgument="BuiltIn\\prikazkina" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Builtin", "prikazkina");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
                <img src=<%=avatar1 %> width="36" height="36" />
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1 == null ? string.Empty : user1.FullName%> (Кассир)</div>
            <div class="sn-logindemo-userdata">
                 <small>Может создавать приказы и отправлять их на согласование.</small>
                 <small class="sn-link">имя: <strong>prikazkina</strong> / пароль: <strong>priksog</strong></small>
            </div>
        </div>
        <div style="clear:both;"></div>
        </asp:LinkButton>
    </li>
    <li>
        <asp:LinkButton ID="link2" runat="server" CommandArgument="BuiltIn\\vizirov" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Builtin", "vizirov");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
                <img src=<%=avatar1 %> width="36" height="36" />
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1 == null ? string.Empty : user1.FullName%> (Менеджер)</div>
            <div class="sn-logindemo-userdata">
                <small>Менеджер по согласованию приказов.</small>
                <small class="sn-link">имя: <strong>vizirov</strong> / пароль: <strong>vizbiz</strong></small>
            </div>
        </div>
        <div style="clear:both;"></div>
        </asp:LinkButton>
    </li>
    <li class="last">
        <asp:LinkButton ID="link3" runat="server" CommandArgument="BuiltIn\\maxpavlov" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Builtin", "maxpavlov");
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
