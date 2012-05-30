<%@ Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.GenericContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>

<sn:GenericFieldControl runat="server" ID="gfc" ExcludedFields="Compulsory DefaultValue VisibleBrowse VisibleEdit VisibleNew DefaultOrder FieldIndex Version ModifiedBy ModificationDate Index" />

<div class="sn-panel sn-buttons">
  <sn:CommandButtons ID="CommandButtons1" runat="server" layoutControlPath="/Root/System/SystemPlugins/Controls/CommandButtons.ascx" />
</div>