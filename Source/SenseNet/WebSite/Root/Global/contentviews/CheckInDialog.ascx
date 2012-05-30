<%@ Control Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.UserControl" %>

<div id="CheckInDialog" class="sn-panel sn-hide">

    <strong><%= HttpContext.GetGlobalResourceObject("Portal","CheckInCommentsTextBoxTitle") %>:</strong> 

    <div id="CheckInErrorPanel" class="sn-error sn-hide">
        <span><%= HttpContext.GetGlobalResourceObject("Portal","CheckInCommentsCompulsory") %></span>
    </div>

    <sn:LongText ID="CheckInComments" runat="server" FieldName="CheckInComments" ControlMode="Edit" FrameMode="NoFrame" >
        <EditTemplate>
            <asp:TextBox ID="InnerControl" runat="server" Width="97%" CssClass="sn-ctrl sn-ctrl-text sn-ctrl-textarea sn-checkincompulsory" TextMode="MultiLine"></asp:TextBox>
        </EditTemplate>
    </sn:LongText>

    <div class="sn-panel sn-buttons">
        <sn:CommandButtons ID="CheckInCommandButtons" runat="server" HideButtons="Cancel;Publish;Save;CheckoutSave" />
        <input type="button" value="Cancel" class="sn-submit sn-notdisabled" onclick="javascript:$('#CheckInDialog').dialog('close');return false;" />
    </div>
</div>
