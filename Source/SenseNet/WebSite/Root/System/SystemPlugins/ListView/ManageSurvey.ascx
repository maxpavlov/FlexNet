<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Search" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="System.Linq" %>
<sn:SenseNetDataSource ID="SNDSReadOnlyFields" ContextInfoID="ViewContext" MemberName="AvailableContentTypeFields"
    FieldNames="Name DisplayName ShortName Owner ParentId" runat="server" />
<sn:SenseNetDataSource ID="ViewDatasource" ContextInfoID="ViewContext" MemberName="FieldSettingContents"
    FieldNames="Name DisplayName ShortName ParentId FieldIndex" runat="server" DefaultOrdering="FieldIndex" />
<sn:ContextInfo ID="ViewContext" runat="server" />
<sn:ContextInfo runat="server" Selector="CurrentContext" ID="myContext" />

<script type="text/C#" runat="server">
    bool isEditable()
    {
        SenseNet.Search.LucQuery q = LucQuery.Parse("+Type:surveyitem +ParentId:" + PortalContext.Current.ContextNode.Id);

        var children = q.Execute();

        if (children.Count() > 0)
        {
            return false;
        }
        return true;
    }
</script>

<div class="sn-listview">

    <div class="sn-panel ui-helper-clearfix">
    <% if (isEditable()) { %>
        <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/addfield.png" Scenario="SurveyScenario" ContextInfoID="myContext">Add field</sn:ActionMenu>
    <% } else { %>
        <asp:Label CssClass="sn-error">Cannot modify questions due to existing filled survey(s).</asp:Label>
    <% } %>
    </div>

    <div class="sn-listgrid-container">
        <asp:ListView ID="ViewBody" DataSourceID="ViewDatasource" runat="server">
            <LayoutTemplate>
                <table class="sn-listgrid ui-widget-content">
                    <thead>
                        <tr class="ui-widget-content">
                            <th class="sn-lg-col-1 ui-state-default" style="width: 20px;">#</th>
                            <th class="sn-lg-col-2 ui-state-default">Field DisplayName</th>
                            <th class="sn-lg-col-3 ui-state-default">Field type</th>
                            <th class='sn-lg-col-4 ui-state-default'>&nbsp;</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr runat="server" id="itemPlaceHolder" />
                    </tbody>
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr class='<%# Container.DisplayIndex % 2 == 0 ? "sn-lg-row0" : "sn-lg-row1" %> ui-widget-content'>
                    <td class="sn-lg-col-1">
                        <%# ((ListViewDataItem)Container).DataItemIndex + 1%>
                    </td>
                    <td class="sn-lg-col-2">
                        <%# Eval("DisplayName") %>
                    </td>
                    <td class="sn-lg-col-3">
                        <%# Eval("ShortName") %>
                    </td>
                    <td class="sn-lg-col-4 sn-right sn-nowrap" style="width: 96px;">
                    <% if (isEditable()) { %>
                        <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="EditField" IconName="edit" ToolTip="Edit field" ParameterString='<%# "FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) %>' runat="server" />
                        <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="DeleteField" IconName="delete" ToolTip="Delete field" ParameterString='<%# "FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) %>' runat="server" />
                        <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="MoveField" IconName="up" ToolTip="Move up" ParameterString='<%# "Direction=Up&FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) %>' runat="server" />
                        <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="MoveField" IconName="down" ToolTip="Move down" ParameterString='<%# "Direction=Down&FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) %>' runat="server" />
                    <% } %>
                    </td>
                </tr>
            </ItemTemplate>
            <EmptyDataTemplate>
                <table class="sn-listgrid ui-widget-content">
                    <thead>
                        <tr class="ui-widget-content">
                            <th class="sn-lg-col-1 ui-state-default">
                                Field DisplayName
                            </th>
                            <th class="sn-lg-col-2 ui-state-default">
                                Field type
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="ui-widget-content">
                            <td colspan="2" class="sn-lg-col">
                                No editable fields found.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </EmptyDataTemplate>
        </asp:ListView>
    </div>
    <div class="sn-panel sn-buttons">
        <sn:BackButton CssClass="sn-submit" Text="Done" runat="server" ID="BackButton" />
    </div>
</div>
