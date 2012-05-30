<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>

<div>
    <sn:GenericFieldControl runat="server" ID="GenericFieldControl" ExcludedFields="Compulsory VisibleBrowse VisibleEdit VisibleNew DefaultOrder AddToDefaultView Version Index" />
    <sn:Boolean runat="server" ID="AddToDefaultView" FieldName="AddToDefaultView" Visible="true">
        <InlineEditTemplate>
            <asp:CheckBox ID="CheckBox1" runat="server" Checked="false"></asp:CheckBox>
        </InlineEditTemplate>
    </sn:Boolean>
</div>
<div class="sn-panel sn-buttons">
    <sn:CommandButtons ID="CommandButtons1" runat="server" />
</div>
