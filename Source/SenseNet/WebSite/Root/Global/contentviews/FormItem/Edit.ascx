<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<div class="sn-form ui-helper-clearfix">
     
    <h2 class="sn-form-title">
        <%= (this.Content.ContentHandler.Parent as SenseNet.Portal.Portlets.ContentHandlers.Form).DisplayName%>
    </h2>
    <div class="sn-form-description">
        <%= (this.Content.ContentHandler.Parent as SenseNet.Portal.Portlets.ContentHandlers.Form).Description%>
    </div>
    <div class="sn-form-fields">
        <sn:ErrorView ID="ErrorView1" runat="server" />
        <sn:GenericFieldControl runat=server ID="GenericFieldControl1" ContentListFieldsOnly="true" />
    </div>
    <div class="sn-form-comment">
        * compulsory field
    </div>
    <div class="sn-panel sn-buttons sn-form-buttons">
        <asp:Button ID="BtnSend" CssClass="sn-submit" runat="server" CommandName="save" Text="Send" EnableViewState="false" OnClick="Click" />        
    </div>
</div>
