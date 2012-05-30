<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<sn:ShortText runat="server" ID="ListEmail" FieldName="ListEmail" />
<sn:RadioButtonGroup runat="server" ID="GroupAttachments" FieldName="GroupAttachments" />
<sn:Boolean runat="server" ID="SaveOriginalEmail" FieldName="SaveOriginalEmail" />
<sn:Boolean runat="server" ID="OverwriteFiles" FieldName="OverwriteFiles" />
<sn:ReferenceGrid ID="IncomingEmailWorkflow" runat="server" FieldName="IncomingEmailWorkflow" />

<div class="sn-panel sn-buttons">
    <asp:Button ID="Save" CssClass="sn-button sn-submit" runat="server" CommandName="Save" Text="Save" OnClick="Click" />
    <asp:Button ID="Cancel" CssClass="sn-button sn-submit" runat="server" CommandName="Cancel" Text="Cancel" OnClick="Click" />
</div>