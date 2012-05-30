<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>

<div class="sn-content-inlineview-header ui-helper-clearfix">
    <%= SenseNet.Portal.UI.IconHelper.RenderIconTag(this.Content.Icon, null, 32)%>
    <div class="sn-content-info">
        <h2 class="sn-view-title"><sn:ShortText ID="DisplayName" runat="server" FieldName="DisplayName" ControlMode="Browse" FrameMode="NoFrame" /></h2>
        <strong>Path:</strong> <%= this.Content.Path %>
    </div>
</div>

<sn:LongText ID="CheckInComments" runat="server" FieldName="CheckInComments" ControlMode="Edit">
    <EditTemplate>
         <asp:TextBox CssClass="sn-ctrl sn-ctrl-text sn-ctrl-textarea" Width="98.5%" ID="InnerControl" runat="server" TextMode="MultiLine"></asp:TextBox>
    </EditTemplate>
</sn:LongText>

<div class="sn-panel sn-buttons">
    <sn:CommandButtons ID="CommandButtons1" runat="server" LayoutControlPath="/Root/System/SystemPlugins/Controls/CheckInButtons.ascx" />
</div>
