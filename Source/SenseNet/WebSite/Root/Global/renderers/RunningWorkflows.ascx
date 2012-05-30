<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>
<%@ Import Namespace="SenseNet.Workflow" %>


<% if (this.Model.Content == null || this.Model.Content.ChildCount == 0) { %>

<div class="sn-workflow-list">
    There are no running workflows.
</div>

<% } else { %>

<div class="sn-workflow-filters">
    Filter workflows: 
    <input type="radio" name="sn-workflow-filter" class="sn-workflow-filter" id="wfFilterAll" value="-1" checked="checked" /><label for="wfFilterAll">All</label>
    <% 
        var backUrl = PortalContext.Current.BackUrl;

        foreach(var name in Enum.GetNames(typeof(SenseNet.Workflow.WorkflowStatusEnum))) {
        string statusID = ((int)Enum.Parse(typeof(SenseNet.Workflow.WorkflowStatusEnum), name)).ToString();
        if (statusID != "0") {
    %>
        <input type="radio" name="sn-workflow-filter" class="sn-workflow-filter" id="wfFilter<%= statusID %>" value="<%= statusID %>" /><label for="wfFilter<%= statusID %>"><%= name %></label>
    <%
            }
        } 
    %>
</div>

<div id="sn-current-workflows" class="sn-workflow-list">
<% foreach (var content in this.Model.Items) {
    var actionName = ( (content.ContentHandler as WorkflowHandlerBase).WorkflowStatus == WorkflowStatusEnum.Running ) ? "Abort" : "Delete";
    var action = ActionFramework.GetAction(actionName, content, backUrl, null);
    var relatedContent = (Node)content["RelatedContent"];
    var status = (content.ContentHandler as WorkflowHandlerBase).WorkflowStatus.ToString();
%>
    <div class="sn-content sn-workflow ui-helper-clearfix sn-wf-state-<%= ((int)Enum.Parse(typeof(SenseNet.Workflow.WorkflowStatusEnum), status)).ToString() %>">
        <% if (action != null) { %>
        <a class="sn-actionlinkbutton" href="<%=action.Uri %>">
            <%= SenseNet.Portal.UI.IconHelper.RenderIconTag("delete", null, 16)%>
            <%= actionName %>
            workflow </a>
        <% } %>
        <h2 class="sn-content-title">
            <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(content.Icon, null, 32)%>
            <%= Actions.BrowseAction(content) %>    
        </h2>
        <div class="sn-content-lead">
            <% if (relatedContent != null && relatedContent.Id != PortalContext.Current.ContextNode.Id) { %>
            Content: <strong>
                <%= relatedContent.DisplayName %></strong><br />
            <% } %>
            Status: <strong>
                <%= status %></strong>
        </div>
    </div>
<%  } %>
</div>

<sn:InlineScript runat="server">
<script type="text/javascript">
    $(function() {

        //Initialize workflow-list filters
        $(".sn-workflow-filters").each(function () { 
            
            var $workflowFilters = $(".sn-workflow-filter",this);
            var $workflowList = $(this).next(".sn-workflow-list");
            var $workflowItems = $(".sn-workflow", $workflowList);

            $workflowFilters.each(function () {
                var $this = $(this);
                var value = $this.val();

                if (value != -1) var $relatedWorkflows = $(".sn-wf-state-" + value, $workflowList);

                $this.button().click(function () {
                    if (value != -1) {
                        $workflowItems.slideUp(200);
                        $relatedWorkflows.slideDown(200);
                    } else {
                        $workflowItems.slideDown(200);
                    }
                });

            });
        });

    });
</script>
</sn:InlineScript>

<% } %>
