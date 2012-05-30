<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<string>" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Web.Mvc" %>

<script runat="server">
    protected System.Linq.IOrderedEnumerable<KeyValuePair<string, SenseNet.ContentRepository.Field>> GetAllFields(string currentPath)
    {
        return SenseNet.ContentRepository.Content.Load(currentPath).Fields.OrderBy(item => item.Value.FieldSetting.ReadOnly).ThenBy(item => item.Key);
    }
    protected string GetProperty(string itemPath, string propertyName)
    {
        return (SenseNet.ContentRepository.Content.Load(itemPath).ContentHandler.GetPropertySafely(propertyName) ?? "").ToString();
    }
    protected SenseNet.ContentRepository.Storage.Schema.DataType GetFieldType(string fieldName)
    {
        if (SenseNet.ContentRepository.Storage.Schema.PropertyType.GetByName(fieldName) != null)
            return SenseNet.ContentRepository.Storage.Schema.PropertyType.GetByName(fieldName).DataType;
        return default(SenseNet.ContentRepository.Storage.Schema.DataType);
    }
</script>

<h3>
    Edit</h3>
<div>
    <% using (Html.BeginForm("Update", "HcExplore", FormMethod.Post))
       {%>
    <input type="hidden" id="root" name="root" value="<%=this.Model %>" />
    <table class="hcexp-edit" cellpadding="0" cellspacing="0" style="width: 100%; border: 1px solid #e4e4e4;">
        <tr>
            <th style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;">
                Property name
            </th>
            <th style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;">
                Original value
            </th>
            <th style="background: #e4e4e4; border-bottom: 1px solid black; padding: 5px;">
                New value
            </th>
        </tr>
        <%foreach (var keyvaluePair in GetAllFields(this.Model))
          { %>
        <tr>
            <td style="width: 30%; border-bottom: 1px dotted #e4e4e4;">
                <span>
                    <%=keyvaluePair.Value.FieldSetting.DisplayName%>
                    <br />
                </span><span style="padding: 10px 0 0 0; color: Gray;">
                    <%=keyvaluePair.Value.FieldSetting.Description%>
                </span>
            </td>
            <td style="width: 40%; text-align: center;  border-bottom: 1px dotted #e4e4e4;">
                <%=(keyvaluePair.Value.HasValue()  &&
                      (keyvaluePair.Value.FieldSetting.ShortName.Equals("ShortText") ||
                      keyvaluePair.Value.FieldSetting.ShortName.Equals("Integer") ||
                      keyvaluePair.Value.FieldSetting.ShortName.Equals("Boolean") ||
                      keyvaluePair.Value.FieldSetting.ShortName.Equals("DateTime") ||
                    keyvaluePair.Value.FieldSetting.ShortName.Equals("LongText"))) ? keyvaluePair.Value.GetData().ToString() : "<i>Data cannot be shown</i>"%>
            </td>
            <td style="border-bottom: 1px dotted #e4e4e4;">
                <%--<%=keyvaluePair.Value.FieldSetting.ShortName%><br />
                 <%=keyvaluePair.Value.FieldSetting.FieldDataType.ToString()%>--%>
                <%if (!keyvaluePair.Value.FieldSetting.ReadOnly &&
                      (keyvaluePair.Value.FieldSetting.ShortName.Equals("ShortText") ||
                      keyvaluePair.Value.FieldSetting.ShortName.Equals("Integer") ||
                      keyvaluePair.Value.FieldSetting.ShortName.Equals("Boolean") ||
                      keyvaluePair.Value.FieldSetting.ShortName.Equals("DateTime")
                      ))
                  {%>
                <%=Html.TextBox(keyvaluePair.Key, keyvaluePair.Value.GetData())%>
                <%}
                  else if (!keyvaluePair.Value.FieldSetting.ReadOnly &&
                      (keyvaluePair.Value.FieldSetting.ShortName.Equals("LongText")))
                  {%>
                <textarea id="<%=keyvaluePair.Key%>" name="<%=keyvaluePair.Key%>">
                    <%=keyvaluePair.Value.GetData()%>
                  </textarea>
                <%
                    }
                  else
                  {
%>                  <span><i>Read only field</i></span>
                  <%
                  }%>
            </td>
        </tr>
        <%} %>
    </table>
    <input type="submit" id="update" name="update" value="Update" />
    <%} %>
</div>
