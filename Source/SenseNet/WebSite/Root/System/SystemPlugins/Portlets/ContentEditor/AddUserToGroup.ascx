<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>

<div class="sn-content sn-content-inlineview">
    <sn:ErrorView ID="ErrorView1" runat="server" />
    <sn:ReferenceGrid ID="RefMembers" runat="server" FieldName="Members" RenderMode="Edit" />
</div>

<div class="sn-panel sn-buttons">
  <sn:DefaultButtons ButtonCssClass="sn-submit" ID="DefaultButtons1" visibleCheckOut="false" runat="server" />
</div>