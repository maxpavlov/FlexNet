<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<string>" %>

<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Schema" %>
<%@ Import Namespace="SenseNet.Portal.HcExplore" %>
<%@ Import Namespace="System.Collections.Generic" %>
<asp:content id="Content1" contentplaceholderid="menu" runat="server">
<div class="treeview">
<ul>
    <li class="folder open"><a href="HcExplore.mvc?Root=%2FRoot">Root</a>
    
    <% Html.RenderPartial(@"~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreMenuItem.aspx", new Dictionary<string, string> { { "CurrentPath", "/Root" }, { "SelectedPath", this.Model } });%>
    </li>
</ul>
</div>
</asp:content>
<asp:content id="Content3" contentplaceholderid="wideContent" runat="server">
    <div class="hcexp-panel">
      <h2 style="float: right; padding: 3px 10px;"><span>Logged in user: <%=HttpContext.Current.User.Identity.Name%></span></h2>
      <h2>Selected content: <% =this.Model%></h2>
    </div>
</asp:content>
<asp:content id="Content2" contentplaceholderid="content" runat="server">



<%if (this.GetNode(this.Model).Security.HasPermission(PermissionType.AddNew) && this.GetNode(this.Model) is IFolder)
  {%>
  <div class="hcexp-panel">
    <% Html.RenderPartial(@"~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreUpload.aspx", this.Model);%>
</div>
<%
    }%>

<div class="hcexp-panel">
    <% Html.RenderPartial(@"~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreBrowse.aspx", this.Model);%>
</div>
<%if (this.GetNode(this.Model).Security.HasPermission(PermissionType.Save))
  {%>
<div class="hcexp-panel">
    <% Html.RenderPartial(@"~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreEdit.aspx", this.Model);%>
</div>
<%
    }%>
</asp:content>
