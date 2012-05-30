<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<string>" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Web.Mvc" %>

<script runat="server">
    protected IEnumerable<SenseNet.ContentRepository.Storage.Schema.NodeType> GetAllNodeTypes()
    {
        return SenseNet.ContentRepository.Storage.Schema.NodeType.GetByName("GenericContent").GetAllTypes().OrderBy(nodeType => nodeType.Name);
    }
</script>

<h3>
    File upload</h3>
<% using (Html.BeginForm("Upload", "HcExplore", FormMethod.Post, new { enctype = "multipart/form-data" }))
   {%>
<input type="hidden" id="root" name="root" value="<%=this.Model %>" />
<p>
    <table>
        <tr>
            <td style="width: 170px;" >
                New content type:
            </td>
            <td style="width: 235px;">
                <select name="nodeType" id="nodeType" style="width: 100%; padding: 2px 4px;">
                    <%foreach (var cntType in GetAllNodeTypes())
                      {%>
                    %>
                    <option value="<%=cntType.Name %>">
                        <%=cntType.Name%></option>
                    <%} %>
                </select>
            </td>
        </tr>
        <tr>
            <td style="width: 170px;" >
                Binary attachment:
            </td>
            <td style="width: 170px;">
                <input type="file" id="fileUpload" name="fileUpload" size="23" style="width: 100%; padding: 2px 4px;" />
            </td>
        </tr>
        <tr>
            <td style="width: 170px;" >
                New content URI name:
            </td>
            <td style="width: 235px;">
                <input type="text" id="fileName" name="fileName" style="width: 100%; padding: 2px 4px;" />
            </td>
        </tr>
    </table>
</p>
<p>
    <input type="submit" value="Create new file" /></p>
<% } %>
