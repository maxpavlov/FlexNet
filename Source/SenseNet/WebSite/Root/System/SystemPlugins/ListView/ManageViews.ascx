
    <%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.ContentListViews.ListView" %>
    <%@ Import Namespace="SNCR=SenseNet.ContentRepository" %>
    <%@ Import Namespace="SenseNet.Portal.UI.ContentListViews" %>
    <%@ Import Namespace="SenseNet.Portal.Helpers" %>
    
    <%
        var list = SNCR.ContentList.GetContentListByParentWalk(this.ContextNode);
        var defaultView = list == null ? null : ViewManager.LoadDefaultView(list);
            
         %>

         <div class="sn-pt-body-border ui-widget-content ui-corner-all sn-content-inlineview-header ui-helper-clearfix">
            <%= SenseNet.Portal.UI.IconHelper.RenderIconTag("content", null, 32) %>
	        <div class="sn-content-info">
                <%--h2 class="sn-view-title"><% = DisplayName %> (<%= ContentType.DisplayName %>)</h2>--%>
                <span><%= HttpContext.GetGlobalResourceObject("Portal", "ContentList_ManageViewsDefaultLabel") as string %>: </span> 
                <span title="<%= defaultView == null ? "" : defaultView.Path %>"><strong><%= defaultView == null ? HttpContext.GetGlobalResourceObject("Portal", "ContentList_ManageViewsNoDefault") as string : defaultView.DisplayName%></strong></span> <br/>            
                <% if (list != null && defaultView != null && !defaultView.Path.StartsWith(list.Path))
                   { %>
                   <span><%= HttpContext.GetGlobalResourceObject("Portal", "ContentList_ManageViewsGlobal") as string %></span>
                   <a href='<%= SenseNet.Portal.Helpers.Actions.ActionUrl(SNCR.Content.Create(defaultView), "CopyViewLocal", true, new { listPath = HttpUtility.UrlEncode(list.Path) }) %>'>Create local copy</a>
                <% } %>
            </div>
          </div>

    <sn:ListGrid ID="ViewBody"
                  DataSourceID="ViewDatasource"
                  runat="server">
      <LayoutTemplate>
        <table class="sn-listgrid  ui-widget-content">
          <thead>
            <asp:TableRow runat="server" class="ui-widget-content">
            
    <sn:ListHeaderCell  runat="server" ID="checkboxHeader" class="sn-lg-cbcol ui-state-default"><input type='checkbox' /></sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-1 sn-nowrap ui-state-default" FieldName="GenericContent.Locked" Width="62">      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.Locked" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Locked</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-2 sn-nowrap ui-state-default" FieldName="GenericContent.DisplayName" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.DisplayName" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Name</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-3 sn-nowrap ui-state-default" FieldName="ViewBase.IsDefault" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="ViewBase.IsDefault" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Default</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-4 sn-nowrap ui-state-default" FieldName="GenericContent.ModifiedBy" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.ModifiedBy" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Modified by</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-5 sn-nowrap ui-state-default" FieldName="GenericContent.ModificationDate" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.ModificationDate" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Last modified</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
            </asp:TableRow>
          </thead>
          <tbody>
            <asp:TableRow runat="server" id="itemPlaceHolder" />
          </tbody>
        </table>
      </LayoutTemplate>
      <ItemTemplate>
        <asp:TableRow runat="server" class="sn-lg-row0 ui-widget-content">
    <asp:TableCell class="sn-lg-cbcol" runat="server" Visible="<%# (this.ShowCheckboxes.HasValue && this.ShowCheckboxes.Value) ? true : false %>">
        <input type='checkbox' value='<%# Eval("Id") %>' />
      </asp:TableCell>
    
          <asp:TableCell runat="server" class="sn-lg-col-1" HorizontalAlign="Center" >
          <sn:ActionLinkButton ID="ActionLinkButton1" runat='server' NodePath='<%# Eval("Path") %>' ActionName='checkin' Tooltip='<%$ Resources: Portal, CheckIn %>' />    
<sn:ActionLinkButton ID="ActionLinkButton2" runat='server' NodePath='<%# Eval("Path") %>' ActionName='checkout' Tooltip='<%$ Resources: Portal, CheckOut %>' />    
<sn:ActionLinkButton ID="ActionLinkButton3" runat='server' NodePath='<%# Eval("Path") %>' ActionName='undocheckout' Tooltip='<%$ Resources: Portal, UndoCheckOut %>' />
<sn:ActionLinkButton ID="ActionLinkButton4" runat='server' NodePath='<%# Eval("Path") %>' ActionName='forceundocheckout' Tooltip='<%# ((SNCR.Content)Container.DataItem).ContentHandler.Locked ? "Force undo changes. Locked by " + ((SNCR.Content)Container.DataItem).ContentHandler.LockedBy.Name : string.Empty %>' />
<asp:PlaceHolder runat="server" id="plcLocked" Visible="<%# ((SNCR.Content)Container.DataItem).ContentHandler.Locked && !SNCR.SavingAction.HasUndoCheckOut(((SNCR.Content)Container.DataItem).ContentHandler as SNCR.GenericContent) && !SNCR.SavingAction.HasForceUndoCheckOutRight(((SNCR.Content)Container.DataItem).ContentHandler as SNCR.GenericContent) %>">
<a disabled="disabled" class="sn-actionlinkbutton sn-disabled" href="#"><img class="sn-icon sn-icon16" title="<%# ((SNCR.Content)Container.DataItem).ContentHandler.Locked ? "Locked by " + ((SNCR.Content)Container.DataItem).ContentHandler.LockedBy.Name : string.Empty %>" alt="[locked]" src="/Root/Global/images/icons/16/checkin.png"></a>
</asp:PlaceHolder>
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-2"  >
           <sn:ActionMenu NodePath='<%# Eval("Path") %>' runat="server" Scenario="ManageViewsListItem" IconName="<%# ((SenseNet.ContentRepository.Content)Container.DataItem).Icon %>" >
            <sn:ActionLinkButton runat='server' NodePath='<%# Eval("Path") %>' ActionName='<%# ((SenseNet.ContentRepository.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("ViewBase") ? "Edit" : "Browse" %>' IconVisible='false' >
          <%# Eval("GenericContent_DisplayName") %>
            </sn:ActionLinkButton>
              <asp:Placeholder runat="server" Visible="<%# !((SNCR.Content)Container.DataItem).Security.HasPermission(SNCR.Storage.Schema.PermissionType.Open) %>">
          
          <%# Eval("GenericContent_DisplayName") %>
          
              </asp:Placeholder>
            </sn:ActionMenu>
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-3">            
            <asp:Image ID="IsDefaultIcon" runat='server' Visible='<%# Eval("ViewBase_IsDefault") %>' ToolTip="Default content list view" ImageUrl="/Root/Global/images/icons/16/approve.png" /> 
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-4"  >
<%# Eval("GenericContent_ModifiedBy") %>
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-5"  >
          <%# ((Container.DataItem as SNCR.Content).Fields["ModificationDate"].FieldSetting as SNCR.Fields.DateTimeFieldSetting).DateTimeMode == SNCR.Fields.DateTimeMode.Date ? ((DateTime)Eval("GenericContent_ModificationDate")).ToShortDateString() : Eval("GenericContent_ModificationDate") %>
          </asp:TableCell>
        </asp:TableRow>
      </ItemTemplate>
      <AlternatingItemTemplate>
        <asp:TableRow runat="server" class="sn-lg-row1 ui-widget-content">
    <asp:TableCell class="sn-lg-cbcol" runat="server" Visible="<%# (this.ShowCheckboxes.HasValue && this.ShowCheckboxes.Value) ? true : false %>">
        <input type='checkbox' value='<%# Eval("Id") %>' />
      </asp:TableCell>
    
          <asp:TableCell runat="server" class="sn-lg-col-1" HorizontalAlign="Center" >
          <sn:ActionLinkButton ID="ActionLinkButton1" runat='server' NodePath='<%# Eval("Path") %>' ActionName='checkin' Tooltip='<%$ Resources: Portal, CheckIn %>' />    
<sn:ActionLinkButton ID="ActionLinkButton2" runat='server' NodePath='<%# Eval("Path") %>' ActionName='checkout' Tooltip='<%$ Resources: Portal, CheckOut %>' />    
<sn:ActionLinkButton ID="ActionLinkButton3" runat='server' NodePath='<%# Eval("Path") %>' ActionName='undocheckout' Tooltip='<%$ Resources: Portal, UndoCheckOut %>' />
<sn:ActionLinkButton ID="ActionLinkButton4" runat='server' NodePath='<%# Eval("Path") %>' ActionName='forceundocheckout' Tooltip='<%# ((SNCR.Content)Container.DataItem).ContentHandler.Locked ? "Force undo changes. Locked by " + ((SNCR.Content)Container.DataItem).ContentHandler.LockedBy.Name : string.Empty %>' />
<asp:PlaceHolder runat="server" id="plcLocked" Visible="<%# ((SNCR.Content)Container.DataItem).ContentHandler.Locked && !SNCR.SavingAction.HasUndoCheckOut(((SNCR.Content)Container.DataItem).ContentHandler as SNCR.GenericContent) && !SNCR.SavingAction.HasForceUndoCheckOutRight(((SNCR.Content)Container.DataItem).ContentHandler as SNCR.GenericContent) %>">
<a disabled="disabled" class="sn-actionlinkbutton sn-disabled" href="#"><img class="sn-icon sn-icon16" title="<%# ((SNCR.Content)Container.DataItem).ContentHandler.Locked ? "Locked by " + ((SNCR.Content)Container.DataItem).ContentHandler.LockedBy.Name : string.Empty %>" alt="[locked]" src="/Root/Global/images/icons/16/checkin.png"></a>
</asp:PlaceHolder>
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-2"  >
           <sn:ActionMenu NodePath='<%# Eval("Path") %>' runat="server" Scenario="ManageViewsListItem" IconName="<%# ((SenseNet.ContentRepository.Content)Container.DataItem).Icon %>" >
            <sn:ActionLinkButton runat='server' NodePath='<%# Eval("Path") %>' ActionName='<%# ((SenseNet.ContentRepository.Content)Container.DataItem).ContentHandler.NodeType.IsInstaceOfOrDerivedFrom("ViewBase") ? "Edit" : "Browse" %>' IconVisible='false' >
          <%# Eval("GenericContent_DisplayName") %>
            </sn:ActionLinkButton>
              <asp:Placeholder runat="server" Visible="<%# !((SNCR.Content)Container.DataItem).Security.HasPermission(SNCR.Storage.Schema.PermissionType.Open) %>">
          
          <%# Eval("GenericContent_DisplayName") %>
          
              </asp:Placeholder>
            </sn:ActionMenu>
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-3"  >
            <asp:Image ID="IsDefaultIcon" runat='server' Visible='<%# Eval("ViewBase_IsDefault") %>' ToolTip="Default content list view" ImageUrl="/Root/Global/images/icons/16/approve.png" /> 
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-4"  >
              <%# Eval("GenericContent_ModifiedBy") %>
          </asp:TableCell>
        
          <asp:TableCell runat="server" class="sn-lg-col-5"  >
          <%# ((Container.DataItem as SNCR.Content).Fields["ModificationDate"].FieldSetting as SNCR.Fields.DateTimeFieldSetting).DateTimeMode == SNCR.Fields.DateTimeMode.Date ? ((DateTime)Eval("GenericContent_ModificationDate")).ToShortDateString() : Eval("GenericContent_ModificationDate") %>
          </asp:TableCell>
        </asp:TableRow>
      </AlternatingItemTemplate>
      <EmptyDataTemplate>
        <table class="sn-listgrid ui-widget-content">
          <thead>
          <asp:TableRow runat="server">
    <sn:ListHeaderCell  runat="server" ID="checkboxHeader" class="sn-lg-cbcol ui-state-default"><input type='checkbox' /></sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-1 sn-nowrap ui-state-default" FieldName="GenericContent.Locked" Width="62">      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.Locked" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Locked</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-2 sn-nowrap ui-state-default" FieldName="GenericContent.DisplayName" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.DisplayName" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Name</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-3 sn-nowrap ui-state-default" FieldName="ViewBase.IsDefault" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="ViewBase.IsDefault" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Default</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-4 sn-nowrap ui-state-default" FieldName="GenericContent.ModifiedBy" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.ModifiedBy" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Modified by</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    
      <sn:ListHeaderCell runat="server" class="sn-lg-col-5 sn-nowrap ui-state-default" FieldName="GenericContent.ModificationDate" >      
        <asp:LinkButton runat="server" CommandName="Sort" CommandArgument="GenericContent.ModificationDate" >
          <span class="sn-sort">
            <span class="sn-sort-asc ui-icon ui-icon-carat-1-n"></span>
            <span class="sn-sort-desc ui-icon ui-icon-carat-1-s"></span>
          </span>
          <span>Last modified</span>            
        </asp:LinkButton>
      </sn:ListHeaderCell>
    </asp:TableRow>
          </thead>
        </table>
        <div class="sn-warning-msg ui-widget-content ui-state-default">The list is empty&hellip;</div>
      </EmptyDataTemplate>
    </sn:ListGrid>
    <asp:Literal runat="server" id="ViewScript" />
    <sn:SenseNetDataSource ID="ViewDatasource" runat="server" />
  