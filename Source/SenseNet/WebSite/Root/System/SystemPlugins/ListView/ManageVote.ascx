<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Search" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="System.Linq" %>
<sn:sensenetdatasource id="SNDSReadOnlyFields" contextinfoid="ViewContext" membername="AvailableContentTypeFields"
    fieldnames="Name DisplayName ShortName Owner ParentId" runat="server" />
<sn:sensenetdatasource id="ViewDatasource" contextinfoid="ViewContext" membername="FieldSettingContents"
    fieldnames="Name DisplayName ShortName ParentId" runat="server" />
<sn:contextinfo id="ViewContext" runat="server" />
<sn:contextinfo runat="server" selector="CurrentContext" id="myContext" />

<script type="text/C#" runat="server">
    bool CanAddQuestion()
    {
        var currentContent = SenseNet.ContentRepository.Content.Create(PortalContext.Current.ContextNode);

        var refField = currentContent.Fields["FieldSettingContents"] as SenseNet.ContentRepository.Fields.ReferenceField;

        var originalValue = refField.OriginalValue as List<SenseNet.ContentRepository.Storage.Node>;

        if (currentContent != null)
        {
            var addedFields = from fs in originalValue where fs.Name.StartsWith("#") select fs;

            if (addedFields.Count() == 0)
            {
                return true;
            }
        }
        return false;
    }

</script>

<div class="sn-listview">
    <% if (CanAddQuestion())
       { %>
    <div class="sn-toolbar">
        <div class="sn-toolbar-inner">
            <sn:actionmenu runat="server" iconurl="/Root/Global/images/icons/16/addfield.png"
                scenario="VotingAddFieldScenario" contextinfoid="myContext">Add</sn:actionmenu>
        </div>
    </div>
    <% } %>
    <br />
    <h2 class="sn-content-title">
        Voting question</h2>
    <div class="sn-listgrid-container">
        <asp:ListView ID="ViewBody" DataSourceID="ViewDatasource" runat="server">
            <layouttemplate>
                <table class="sn-listgrid ui-widget-content">
                    <thead>
                        <tr id="Tr1" runat="server" class="ui-widget-content">
                            <th class="sn-lg-col-1 ui-state-default">
                                Edit/Delete
                            </th>
                            <th class="sn-lg-col-2 ui-state-default">
                                Question Name
                            </th>
                            <th class="sn-lg-col-3 ui-state-default">
                                Field type
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr runat="server" id="itemPlaceHolder" />
                    </tbody>
                </table>
            </layouttemplate>
            <itemtemplate>
                <tr id="Tr2" runat="server" class='<%# Container.DisplayIndex % 2 == 0 ? "sn-lg-row0" : "sn-lg-row1" %> ui-widget-content'>
                    <td class="sn-lg-col-1">
                        <a title="Edit field" class="sn-icon-button sn-icon-small snIconSmall_Edit" href="?action=EditField&FieldName=<%# HttpUtility.UrlEncode(Eval("Name").ToString()) %>&back=<%# HttpUtility.UrlEncode(SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.PathAndQuery.ToString())  %>">
                            Edit</a> <a title="Delete field" class="sn-icon-button sn-icon-small snIconSmall_Delete"
                                href="?action=DeleteField&FieldName=<%# HttpUtility.UrlEncode(Eval("Name").ToString()) %>&back=<%# HttpUtility.UrlEncode(SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.PathAndQuery.ToString())  %>">
                                Delete</a>
                    </td>
                    <td class="sn-lg-col-2">
                        <%# Eval("DisplayName") %>
                    </td>
                    <td class="sn-lg-col-3">
                        <%# Eval("ShortName") %>
                    </td>
                </tr>
            </itemtemplate>
            <emptydatatemplate>
                <table class="sn-listgrid ui-widget-content">
                    <thead>
                        <tr id="Tr4" runat="server" class="ui-widget-content">
                            <th class="sn-lg-col-1 ui-state-default">
                                Field title
                            </th>
                            <th class="sn-lg-col-2 ui-state-default">
                                Field type
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr class="ui-widget-content">
                            <td colspan="2" class="sn-lg-col">
                                There is no question for this voting.
                            </td>
                        </tr>
                    </tbody>
                </table>
            </emptydatatemplate>
        </asp:ListView>
    </div>
    <div class="sn-panel sn-buttons">
        <sn:backbutton cssclass="sn-submit" text="Done" runat="server" id="BackButton" />
    </div>
</div>
