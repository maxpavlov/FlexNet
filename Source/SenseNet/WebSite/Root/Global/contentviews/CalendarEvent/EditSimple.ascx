<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<div id="InlineViewContent" class="sn-content sn-content-inlineview">
    <sn:GenericFieldControl runat="server" ID="GenericField1" ExcludedFields="RequiresRegistration RegistrationForm OwnerEmail NotificationMode EmailTemplate EmailTemplateSubmitter EmailFrom EmailFromSubmitter EmailField MaxParticipants NumParticipants Version Index" />
</div>
<div class="sn-panel sn-buttons">
    <sn:CommandButtons ID="CommandButtons1" runat="server" LayoutControlPath="/Root/System/SystemPlugins/Controls/CommandButtons.ascx" />
</div>
