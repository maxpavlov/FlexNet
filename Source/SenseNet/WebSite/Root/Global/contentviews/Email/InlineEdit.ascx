<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

  <sn:ShortText ID="DisplayName" runat="server" FieldName="DisplayName" />
  <sn:ShortText ID="From" runat="server" FieldName="From" />
  <sn:DatePicker ID="Sent" runat="server" FieldName="Sent" />
  <sn:LongText ID="Body" runat="server" FieldName="Body" />
  
<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server"/>
</div>

