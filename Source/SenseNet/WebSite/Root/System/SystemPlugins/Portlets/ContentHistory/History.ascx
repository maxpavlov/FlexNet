<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>
<%@ Import Namespace="SenseNet.Portal.UI.PortletFramework" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>

<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />
<sn:SenseNetDataSource ID="SNDSVersions" ContextInfoID="myContext" MemberName="Versions" FieldNames="Name DisplayName Version CheckInComments" runat="server"  />

<% 
    var contextNode = ContextBoundPortlet.GetContainingContextBoundPortlet(this).ContextNode as GenericContent;
    var versioningMode = UITools.GetVersioningModeText(contextNode);
    var lockedByName = contextNode == null ? string.Empty : (contextNode.Lock.LockedBy == null ? string.Empty : contextNode.Lock.LockedBy.Name);
%>

<div class="sn-pt-body-border ui-widget-content">
     <div class="sn-pt-body">
					
        <div class="sn-content">
            <div class="sn-floatleft"><%= SenseNet.Portal.UI.IconHelper.RenderIconTag(contextNode.Icon, null, 32)%></div>
            <div id="sn-version-info" style="padding-left: 40px; margin-bottom:1em;">
                <h1 class="sn-content-title"><%= contextNode["DisplayName"] %></h1>
                <div>
                    <strong>Path:</strong> <%= contextNode["Path"] %><br />
                    <strong><%= HttpContext.GetGlobalResourceObject("Portal", "VersioningMode") as string %>:</strong> <%= versioningMode %>
                    <% if (!string.IsNullOrEmpty(lockedByName)) { %>
                    <br /><strong>Content is locked by</strong> <%= lockedByName %>
                    <% } %>
                </div>
            </div>
        </div>

<asp:ListView ID="HistoryListView" runat="server" EnableViewState="false" DataSourceID="SNDSVersions"  >
    <LayoutTemplate>

        <table id="sn-version-history" class="sn-listgrid ui-widget-content">
          <thead>
              <tr class="ui-widget-content">
                  <th width="160" class="sn-lg-col-1 ui-state-default">Version</th>    
                  <th class="sn-lg-col-2 ui-state-default">Modified</th>
                  <th class="sn-lg-col-3 ui-state-default">Comments</th>
                  <th width="70" nowrap="nowrap" class="sn-lg-col-3 ui-state-default">&nbsp;</th>
              </tr>
          </thead>
          <tbody>
              <tr runat="server" id="itemPlaceHolder" />
          </tbody>
        </table>
    </LayoutTemplate>
    <ItemTemplate>
        <tr class='sn-lg-row<%# Container.DisplayIndex % 2 %> ui-widget-content'>      		
          <td class="sn-lg-col-1">
              <sn:ActionLinkButton runat="server" ToolTip="View this version" ActionName="Browse" IncludeBackUrl="True"
                ContextInfoID="myContext" Text='<%# ((SenseNet.ContentRepository.Content)Container.DataItem).ContentHandler.Version.ToDisplayText() %>' ParameterString='<%# String.Concat("version=" + Eval("Version")) %>' />
          </td>
          <td class="sn-lg-col-2"><%# ((SenseNet.ContentRepository.Content)Container.DataItem)["ModificationDate"].ToString()%> (<%# ((SenseNet.ContentRepository.Content)Container.DataItem)["ModifiedBy"].ToString()%>)</td>
          <td class="sn-lg-col-3"><%# Eval("CheckInComments")%></td>
          <td class="sn-lg-col-4"><sn:ActionLinkButton runat="server" ID="RestoreButton" ActionName="RestoreVersion" IconName="restoreversion" ContextInfoID="myContext" Text="Restore" ParameterString='<%# String.Concat("version=" + Eval("Version")) %>' /></td>
        </tr>
    </ItemTemplate>
    <EmptyDataTemplate>
    </EmptyDataTemplate>
</asp:ListView>   

    </div>
</div> 

<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <sn:BackButton Text="Done" ID="BackButton1" runat="server" CssClass="sn-submit" />
    </div>
</div>
