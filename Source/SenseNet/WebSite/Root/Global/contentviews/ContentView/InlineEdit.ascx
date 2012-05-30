<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<sn:ShortText runat="server" ID="Name" FieldName="Name" />
<div class="sn-highlighteditor-container">
  <sn:Binary ID="Binary1" runat="server" FieldName="Binary" FullScreenText="true">
    <EditTemplate>
       <asp:TextBox ID="BinaryTextBox" runat="server" TextMode="MultiLine" CssClass="sn-highlighteditor" Rows="40" Columns="100" />
       <asp:FileUpload CssClass="sn-ctrl sn-ctrl-upload" ID="FileUploader" runat="server" Visible="false" />
    </EditTemplate>
  </sn:Binary>
</div>
  
<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server"/>
</div>
