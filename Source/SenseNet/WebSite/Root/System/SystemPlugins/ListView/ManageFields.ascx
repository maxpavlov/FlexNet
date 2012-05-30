<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<sn:SenseNetDataSource ID="SNDSReadOnlyFields" ContextInfoID="ViewContext" MemberName="AvailableContentTypeFields" FieldNames="Name DisplayName ShortName Owner" runat="server"  />
<sn:SenseNetDataSource ID="ViewDatasource" ContextInfoID="ViewContext" MemberName="FieldSettingContents" FieldNames="Name DisplayName ShortName" runat="server" DefaultOrdering="FieldIndex" />
<sn:ContextInfo ID="ViewContext" runat="server" />    
<sn:ContextInfo runat="server" Selector="CurrentContext" ID="myContext" />

<div class="sn-listview">
    
    <h2 class="sn-content-title">Readonly fields</h2>

    <div class="sn-listgrid-container">
    <asp:ListView ID="ViewReadOnlyFields" DataSourceID="SNDSReadOnlyFields" runat="server" >
      <LayoutTemplate>
        <table class="sn-listgrid ui-widget-content">
            <thead>
                <tr id="Tr3" runat="server" class="ui-widget-content">
                    <th class="sn-lg-col-1 ui-state-default">Field title</th>        
                    <th class="sn-lg-col-2 ui-state-default">Field type</th>
                    <th class="sn-lg-col-3 ui-state-default">Used in</th>
                </tr>
            </thead>
            <tbody>
                <tr runat="server" id="itemPlaceHolder" />
            </tbody>
        </table>
      </LayoutTemplate>
      <ItemTemplate>
                <tr id="Tr5" runat="server" class='<%# Container.DisplayIndex % 2 == 0 ? "sn-lg-row0" : "sn-lg-row1" %> ui-widget-content'>      
                  <td class="sn-lg-col-1"><%# Eval("DisplayName") %></td>    
                  <td class="sn-lg-col-2"><%# Eval("ShortName") %></td>    
                  <td class="sn-lg-col-3"><%# Eval("Owner") %></td>     
                </tr>
      </ItemTemplate>
    </asp:ListView>
    </div>

    <sn:Toolbar runat="server">
        <sn:ActionMenu runat="server" IconUrl="/Root/Global/images/icons/16/addfield.png" Scenario="AddField" ContextInfoID="myContext" >Add</sn:ActionMenu>
    </sn:Toolbar>

    <br />
    <h2 class="sn-content-title">Editable fields</h2>

    <div class="sn-listgrid-container">
    <asp:ListView ID="ViewBody" DataSourceID="ViewDatasource" runat="server" >
      <LayoutTemplate>
        <table class="sn-listgrid ui-widget-content">
          <thead>
              <tr id="Tr1" runat="server" class="ui-widget-content">                
                <th class="sn-lg-col-2 ui-state-default">Field title</th>        
                <th class="sn-lg-col-3 ui-state-default">Field type</th>
                <th class="sn-lg-col-1 ui-state-default" width="110px">&nbsp;</th>
              </tr>
          </thead>  
          <tbody>
              <tr runat="server" id="itemPlaceHolder" />
          </tbody>
        </table>
      </LayoutTemplate>
      <ItemTemplate>
        <tr id="Tr2" runat="server" class='<%# Container.DisplayIndex % 2 == 0 ? "sn-lg-row0" : "sn-lg-row1" %> ui-widget-content'>                   
          <td class="sn-lg-col-2"><%# Eval("DisplayName") %></td>
          <td class="sn-lg-col-3"><%# Eval("ShortName") %></td>    
          <td class="sn-lg-col-1"  width="110px">
              <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="EditField" IconName="edit" ToolTip="Edit" IncludeBackUrl="false" ParameterString='<%# "FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) + "&back=" + HttpUtility.UrlEncode(SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.PathAndQuery.ToString()) %>' runat="server" />              
              <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="MoveField" IconName="up" ToolTip="Move up" ParameterString='<%# "Direction=Up&FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) %>' runat="server" />
              <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="MoveField" IconName="down" ToolTip="Move down" ParameterString='<%# "Direction=Down&FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) %>' runat="server" />
              <sn:ActionLinkButton CssClass="sn-icononly" ContextInfoID="myContext" ActionName="DeleteField" IconName="delete" ToolTip="Delete" IncludeBackUrl="false" ParameterString='<%# "FieldName=" + HttpUtility.UrlEncode(Eval("Name").ToString()) + "&back=" + HttpUtility.UrlEncode(SenseNet.Portal.Virtualization.PortalContext.Current.RequestedUri.PathAndQuery.ToString()) %>' runat="server" />
          </td> 
        </tr>
      </ItemTemplate>
      <EmptyDataTemplate>
        <table class="sn-listgrid ui-widget-content">
          <thead>
          <tr id="Tr4" runat="server" class="ui-widget-content">          	  
              <th class="sn-lg-col-1 ui-state-default">Field title</th>
              <th class="sn-lg-col-2 ui-state-default">Field type</th>
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
