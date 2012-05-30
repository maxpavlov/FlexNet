<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<div id="InlineViewContent" class="sn-content sn-content-inlineview">

    <sn:LongText ID="HTMLFragment" runat="server" FieldName="HTMLFragment" FullScreenText="true" FrameMode="NoFrame"/>

</div>

<div id="InlineViewProperties" class="sn-content-meta">
        <sn:ShortText ID="Name" runat="server" FieldName="Name" />
        <sn:ShortText ID="Path" runat="server" FieldName="Path" ReadOnly="true" />
        <sn:ShortText ID="Version" runat="server" fieldname="Version" ReadOnly="true"/>
        <sn:WholeNumber ID="Index" runat="server" FieldName="Index" />
        <sn:DatePicker ID="ValidFrom1" runat="server" FieldName="ValidFrom" />
        <sn:DatePicker ID="ReviewDate1" runat="server" FieldName="ReviewDate" />
        <sn:DatePicker ID="ArchiveDate1" runat="server" FieldName="ArchiveDate" />
        <sn:DatePicker ID="ValidTill1" runat="server" FieldName="ValidTill" />        
</div>

<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server" HideButtons="Save CheckoutSave" />
</div>
