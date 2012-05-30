<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>


  <span style='font-size:10.0pt;font-family:"Tahoma","sans-serif"'><b>From: </b><sn:ShortText ID="From" runat="server" FieldName="From" /></span><br/>
  <span style='font-size:10.0pt;font-family:"Tahoma","sans-serif"'><b>Subject: </b><sn:ShortText ID="DisplayName" runat="server" FieldName="DisplayName" /></span><br/>
  <span style='font-size:10.0pt;font-family:"Tahoma","sans-serif"'><b>Sent: </b><sn:DatePicker ID="Sent" runat="server" FieldName="Sent" /></span>
  <hr/>
  <sn:LongText ID="Body" runat="server" FieldName="Body" />

<div class="sn-panel sn-buttons">
    <sn:BackButton CssClass="sn-submit" Text="Back" ID="BackButton1" runat="server" />
</div>

