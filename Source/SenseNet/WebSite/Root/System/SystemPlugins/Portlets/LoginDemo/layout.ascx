<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>

<style type="text/css">
.sn-logindemo-link { color:black !important; cursor:pointer; }
.sn-logindemo-linkcontainer { padding: 5px; cursor:pointer; }
.sn-logindemo-linkcontainer:hover { background-color:#EEE; }
.sn-logindemo-avatar { border:1px solid #EEE;}
.sn-logindemo-avatardiv {border:1px solid #DDD;padding:3px;padding-bottom:0px; width:52px; background-color:#FFF; }
.sn-logindemo-userdata { color: #444; }
.sn-logindemo-username { color: #444; font-weight:bold;}
.sn-logindemo-userdatadiv {float:left; margin-left:10px;width:95px;}
.sn-logindemoportlet .sn-pt-body { padding:0px;  }
.sn-logindemo-divider { margin-top:2px; margin-bottom: 2px; border-bottom:1px solid #DFDFDF; }
.sn-logindemo-loginlink { font-size: 10px; color: #007DC2; text-decoration: none; cursor: pointer; margin-top:2px; text-align:left;}
.sn-logindemo-loginlink:hover {text-decoration:underline;}
.sn-logindemo-leftdiv {float:left; width:63px;}
</style>

<div class="sn-logindemo-linkcontainer">
    <asp:LinkButton ID="link1" runat="server" CommandArgument="Demo\\albamonday" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Demo", "albamonday");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
            <div class="sn-logindemo-avatardiv">
                <img src=<%=avatar1 %> width="50" height="50" class="sn-logindemo-avatar" />
            </div>
            <div class="sn-logindemo-loginlink">Login as Alba Monday</div>
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1.FullName %></div>
            <div class="sn-logindemo-userdata">
                Manager<br />
                <small>She can create and edit workspaces, add and modify documents.</small>
            </div>
        </div>
        <div style="clear:both;"></div>
    </asp:LinkButton>
</div>
<div class="sn-logindemo-divider"></div>
<div class="sn-logindemo-linkcontainer">
    <asp:LinkButton ID="link2" runat="server" CommandArgument="Demo\\mikescroll" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Demo", "mikescroll");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
            <div class="sn-logindemo-avatardiv">
                <img src=<%=avatar1 %> width="50" height="50" class="sn-logindemo-avatar" />
            </div>
            <div class="sn-logindemo-loginlink">Login as Mike Scroll</div>
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1.FullName %></div>
            <div class="sn-logindemo-userdata">
                Developer<br />
                <small>He can create new pagetemplates, views, content types.</small>
            </div>
        </div>
        <div style="clear:both;"></div>
    </asp:LinkButton>
</div>
<div class="sn-logindemo-divider"></div>
<div class="sn-logindemo-linkcontainer">
    <asp:LinkButton ID="link3" runat="server" CommandArgument="BuiltIn\\admin" CssClass="sn-logindemo-link">
        <% var user1 = User.Load("Builtin", "admin");
           var avatar1 = UITools.GetAvatarUrl(user1) + "?dynamicThumbnail=1&width=128&height=128";
        %>
        <div class="sn-logindemo-leftdiv">
            <div class="sn-logindemo-avatardiv">
                <img src=<%=avatar1 %> width="50" height="50" class="sn-logindemo-avatar" />
            </div>
            <div class="sn-logindemo-loginlink">Login as Admin</div>
        </div>
        <div class="sn-logindemo-userdatadiv">
            <div class="sn-logindemo-username"><%= user1.FullName %></div>
            <div class="sn-logindemo-userdata">
                Administrator<br />
                <small>She has full control in the ECM system, she can also create sites and users.</small>
            </div>
        </div>
        <div style="clear:both;"></div>
    </asp:LinkButton>
</div>