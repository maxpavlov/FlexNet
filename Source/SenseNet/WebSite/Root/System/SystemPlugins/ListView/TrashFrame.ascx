<%@ Import Namespace="SenseNet.ApplicationModel"%>
<%@ Import Namespace="SenseNet.ContentRepository"%>
<%@ Import Namespace="SenseNet.ContentRepository.Storage"%>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ViewFrame" %>

<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />

<% 
   var trashbin = SenseNet.ContentRepository.TrashBin.Instance;
   var fullsize = trashbin.GetTreeSize();
   double percent = (fullsize == 0) ? 0 : Math.Round((fullsize * 1.0 / (trashbin.SizeQuota * 1048576.0))*100.0);
%>

<div class="sn-dialog-header">
    <div id="sn-trash-pic">
    <% if (trashbin.SizeQuota != 0) { %>
            <div id="sn-trash-pic-trash" style="top:<%= percent > 100 ? "0": String.Format("{0:0.00}%", 100.0-percent) %>"></div>
            <div id="sn-trash-pic-mask"></div>
    <% } %>
    </div>
    <h1 class="sn-dialog-title">Trash Bin Information</h1>
    <p class="sn-lead sn-dialog-lead">
        This is the place for your deleted items, here you are able to restore your deleted items or empty the trash to save space. (Navigate your mouse over the <img src="/Root/Global/images/icon-info.png" /> to get more information!)
    </p>
    <dl class="sn-dialog-properties">
        <dt>
            Trash Bin state <img src="/Root/Global/images/icon-info.png" alt="info" title="Contents already in the trash are not affected, but new trash can not be added to the Trash Bin. This settings overwrites local settings." />
        </dt>
        <dd>
            <strong>globally <%= trashbin.IsActive ? "enabled" : "disabled" %></strong>
        </dd>
        <dt>
            Minimum retention time <img src="/Root/Global/images/icon-info.png" alt="info" title="Contents deleted today will not be disposed of before <%= DateTime.Now.AddDays(trashbin.MinRetentionTime) %>. Please note that retention time may change for future deletions!" />
        </dt>
        <dd>
            <%= trashbin.MinRetentionTime %> days
        </dd>
        <dt>Space used <img src="/Root/Global/images/icon-info.png" alt="info" title="<%= trashbin.SizeQuota == 0 ? "Size Quota is currently not set (unlimited)" : "Maximum capacity is defined by the Size Quota of the Trash Bin" %>"Trash Bin size is set" /></dt>
        <dd>
        <% if (trashbin.SizeQuota != 0)
           { %>
            <span class="sn-trash-o-meter">
                <span class="sn-trashmeter-container">
                    <span style="width:<%= percent > 100 ? "104": percent.ToString() %>%"></span>
                </span>
                <strong<%= percent > 100 ? " class=\"overflow\"": "" %>><%= percent.ToString()%>% (<%= String.Format("{0:0.00}", (fullsize * 1.0 / 1048576.0))%>Mb used from <%= trashbin.SizeQuota.ToString()%>Mb)</strong>
            </span>       
        <% } else { %>
            <%= String.Format("{0:0.00}", (fullsize * 1.0 / 1048576.0))%>Mb used
        <% } %>
        </dd>
    </dl>
</div>

<div class="sn-listview">

    <sn:Toolbar runat="server">
        <sn:ToolbarItemGroup Align="Left" runat="server">
            <sn:ActionLinkButton runat="server" ActionName="Purge" IconUrl="/Root/Global/images/icons/16/purge.png" ContextInfoID="myContext" Text="Empty Trash Bin" />
        </sn:ToolbarItemGroup>
        <sn:ToolbarItemGroup Align="Right" runat="server">
             <% if (this.ContextNode != null && ScenarioManager.GetScenario("Views").GetActions(SenseNet.ContentRepository.Content.Create(this.ContextNode), null).Count() > 0) 
               { %>
            <span class="sn-actionlabel">View:</span>
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/views.png" Scenario="Views" ContextInfoID="myContext"
                ScenarioParameters="{PortletID}" >
              <%= SenseNet.Portal.UI.ContentListViews.ViewManager.LoadViewInContext(ContextNode, LoadedViewName).DisplayName%>
            </sn:ActionMenu>
             <% } %>
            <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/settings.png" Scenario="Settings" ContextInfoID="myContext">Settings</sn:ActionMenu>
        </sn:ToolbarItemGroup>                
    </sn:Toolbar>

    <asp:Panel ID="ListViewPanel" runat="server"></asp:Panel>
    
</div>

<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <sn:BackButton Text="Done" ID="DoneButton" runat="server" CssClass="sn-submit" Target="currentsite" />
    </div>
</div>