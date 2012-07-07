<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<div class="sn-form ui-helper-clearfix">
     
    <h2 class="sn-form-title">
        <%= (this.Content.ContentHandler.Parent as SenseNet.Portal.Portlets.ContentHandlers.Form).DisplayName%>
    </h2>
    <div class="sn-form-description">
        <%= (this.Content.ContentHandler.Parent as SenseNet.Portal.Portlets.ContentHandlers.Form).Description%>
    </div>
    <div class="sn-form-fields">
        <sn:GenericFieldControl runat=server ID="GenericFieldControl1" ContentListFieldsOnly="true" />
    </div>
    <br />
    <div class="sn-panel sn-buttons sn-form-buttons">
        <sn:BackButton Text="Back" ID="BackButton" runat="server" CssClass="sn-submit" />
    </div>
</div>
