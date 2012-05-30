<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<string>" %>

<%@ Import Namespace="System.Web.Mvc" %>
<asp:content id="Content2" contentplaceholderid="wideContent" runat="server">
<div class="hcexp-panel">
<h3>Login</h3>

<% using (Html.BeginForm("Login", "HcExplore", FormMethod.Post, new { enctype = "multipart/form-data" }))
   {%>
   <table>
        <tr><td colspan="2">You do not have enough permissions to use Hardcode Explore function.</td></tr>
        <tr><td colspan="2">Please login with another user</td></tr>
       <tr><td style="width: 100px;">User name:</td><td><%=Html.TextBox("username") %></td></tr>
       <tr><td style="width: 100px;">Password:</td><td><%=Html.Password("password") %></td></tr>
   </table>
   <input type="submit" value="Login" />
   <%
       }%>
</div>
</asp:content>
