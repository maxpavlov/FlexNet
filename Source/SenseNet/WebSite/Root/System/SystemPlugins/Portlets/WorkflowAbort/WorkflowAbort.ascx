<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.ApplicationModel" %>
<%@ Import Namespace="SenseNet.Portal.Portlets" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Fields" %>
<%@ Import Namespace="SenseNet.Workflow" %>

 <div class="sn-pt-body-border ui-widget-content">
     <div class="sn-pt-body">
         <div class="sn-dialog-lead">
             <sn:SNIcon ID="SNIcon1" Icon="warning" Size="32" runat="server" />You are about to abort: <strong><asp:Label ID="ContentName" runat="server" /></strong>
         </div>
         <div style="padding-left: 45px;">
            <% var context = ContextBoundPortlet.GetContextNodeForControl(this) as WorkflowHandlerBase;
               var relatedContent = context.GetReference<SenseNet.ContentRepository.Storage.Node>("RelatedContent"); 
               
               if (relatedContent != null) {
            %>
                Related content: <strong><%= relatedContent.DisplayName%></strong>
            <% } %>
         </div>
         <asp:PlaceHolder runat="server" ID="ErrorPanel">
             <div class="sn-error-msg">
                <asp:Label runat="server" ID="ErrorLabel" />
             </div>
         </asp:PlaceHolder>
     </div>
</div>   
        
<div class="sn-pt-body-border ui-widget-content sn-dialog-buttons">
    <div class="sn-pt-body">
        <asp:Button ID="Abort" runat="server" Text="Abort" CommandName="Abort" CssClass="sn-submit" />
        <sn:BackButton Text="Cancel" ID="BackButton1" runat="server" CssClass="sn-submit" />
    </div>
</div>