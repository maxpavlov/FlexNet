<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<sn:ContextInfo runat="server" Selector="CurrentContext" UsePortletContext="true" ID="myContext" />

<div class="sn-dialog-header sn-dialog-trashbag">
    <h1 class="sn-dialog-title">Trash Bag Information</h1>
    <p class="sn-lead sn-dialog-lead">
        <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(((TrashBag)this.Content.ContentHandler).DeletedContent.Icon, null, 32) %>
        <%= this.Content.ContentHandler.DisplayName %>
    </p>
    <ul class="sn-dialog-properties">
        <li>
            Keep until: <%= GetValue("KeepUntil")%> <br/>
        </li>
        <li>
            Original path: <%= GetValue("OriginalPath")%> <br/> 
        </li>
    </ul>
   
</div>

<sn:Toolbar ID="Toolbar1" runat="server">
    <sn:ToolbarItemGroup Align="left" runat="server">
        <% if ((this.Content.ContentHandler as TrashBag).DeletedContent is SenseNet.ContentRepository.File) { %>
        <a href="<%= (this.Content.ContentHandler as TrashBag).DeletedContent.Path %>" class="sn-actionlinkbutton"><sn:SNIcon ID="SNIcon1" runat="server" Icon="download" /> Download content</a>
        <% } %>        
    </sn:ToolbarItemGroup>
    <sn:ToolbarItemGroup Align="right" runat="server">
        <sn:ActionLinkButton ID="ActionLinkButton1" runat="server" ActionName="Restore" ContextInfoID="myContext" Text="Restore" />
        <sn:ActionLinkButton ID="ActionLinkButton2" runat="server" ActionName="Delete" ContextInfoID="myContext" Text="Purge" />
    </sn:ToolbarItemGroup>
</sn:Toolbar>

<%--<div class="sn-pt-body-border ui-widget-content">
    <div class="sn-pt-body">
    <sn:GenericFieldControl runat=server ID="GenericFieldControl1"  ExcludedFields="KeepUntil OriginalPath Link DisplayName Index TrashDisabled"/>
    </div>
</div>--%>
        
<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <sn:BackButton Text="Cancel" runat="server" CssClass="sn-submit" />
    </div>
</div>

<div class="sn-pt-footer"></div>    
