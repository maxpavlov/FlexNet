<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.Controls.AdvancedPanelButton" %>
<sn:ScriptRequest id="ScriptRequest1" runat="server" Path="$skin/scripts/sn/SN.Util.js" />
<div class="sn-advancedfieldstoggle ui-helper-reset ui-state-default ui-corner-all" runat="server" id="AdvancedButtonOuter">
    <div runat="server" id="HidePanel" style="display: none"><img src="/Root/Global/images/Minimize.gif" alt="Hide advanced fields"  />&nbsp;Hide advanced fields</div>
    <div runat="server" id="ShowPanel"><img src="/Root/Global/images/Expand.gif" alt="Show advanced fields"  />&nbsp;Show  advanced fields</div>
</div>