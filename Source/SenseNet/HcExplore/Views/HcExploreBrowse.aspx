<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<string>" %>

<%@ Import Namespace="System.Linq" %>

<%@ Import Namespace="SenseNet.Search" %>

<script runat="server">
    protected IEnumerable<SenseNet.ContentRepository.Content> GetAllChildren(string currentPath)
    {
        var node = SenseNet.ContentRepository.Content.Load(currentPath);
        node.ChildrenQuerySettings = new QuerySettings {EnableAutofilters = false, EnableLifespanFilter = false};
        return node.Children.ToList().OrderBy(cnt => cnt.Fields["DisplayName"].ToString());
    }

</script>

<h3>
    Browse</h3>
<div>
    <table cellpadding="0" cellspacing="0" style="width: 100%; border: 1px solid #e4e4e4;">
        <thead>
            <tr>
                <th align="left" style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;" >
                    Name
                </th>
                <th align="left" style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;">
                    Modified by
                </th>
                <th align="left" style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;">
                    Last modified
                </th>
                <th align="center" style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;">
                    Delete
                </th>
            </tr>
        </thead>
        <%foreach (SenseNet.ContentRepository.Content cnt in GetAllChildren(this.Model))
          { %>
        <tr>
            <td  style="border-bottom: 1px dotted #e4e4e4; padding: 4px;">
                <a href="HcExplore.mvc?root=<%=cnt.Path.ToString()%>">
                    <%=cnt.Fields["DisplayName"].GetData().ToString()%>
                </a>
            </td>
            <td  style="border-bottom: 1px dotted #e4e4e4; padding: 4px;">
                <%=cnt.Fields["ModifiedBy"].GetData().ToString()%>
            </td>
            <td  style="border-bottom: 1px dotted #e4e4e4; padding: 4px;">
                <%=cnt.Fields["ModificationDate"].GetData().ToString()%>
            </td>
            <td align="center"  style="border-bottom: 1px dotted #e4e4e4; padding: 4px;">
                <a href="/HcExplore.mvc/Delete?path=<%=cnt.Path %>">
                    <img src="/Root/!Assembly/HcExplore/SenseNet.Portal.HcExplore.Images.delete.png" /></a>
            </td>
        </tr>
        <%} %>
    </table>
</div>
