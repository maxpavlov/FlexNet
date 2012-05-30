<%@ Page Language="C#" Inherits="SenseNet.Portal.HcExplore.HcExploreMenuItem" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Web.Mvc.Html" %>
<%@ Import Namespace="System.Collections.Generic" %>
<% if (this.GetChildren(this.Model["CurrentPath"]).Count() > 0)
   {%>
<ul>
    <%
        foreach (SenseNet.ContentRepository.Content item in this.GetChildren(this.Model["CurrentPath"]))
        {%>
    <%if (this.Model["SelectedPath"].StartsWith(item.Path))
      {%>
    <li class="folder open">
        <%}
      else
      {%>
    <li class="folder">
            <%}%>
            <% if (this.Model["SelectedPath"].Equals(item.Path))
               {%>
            <a class="active" style="color: Red; text-decoration: underline;" href="HcExplore.mvc?root=<%=item.Path%>">
            <%
                }
               else
{%>
<a href="HcExplore.mvc?root=<%=item.Path%>">
<%
}%>
                <%=item.Name%></a>
            <%
                Html.RenderPartial(@"~/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Views.HcExploreMenuItem.aspx", new Dictionary<string, string> { { "CurrentPath", item.Path }, { "SelectedPath", this.Model["SelectedPath"] } });%>
        </li>
        <%
            }%>
</ul>
<%
    }%>
