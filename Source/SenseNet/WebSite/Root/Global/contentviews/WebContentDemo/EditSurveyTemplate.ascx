<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<div>
    <sn:ShortText runat="server" ID="ShortTextSubtitle" FieldName="Subtitle" />
    <sn:RichText runat="server" ID="RichTextBody" FieldName="Body" />
    <sn:CommandButtons runat="server" ID="CommandButtons1" />
</div>