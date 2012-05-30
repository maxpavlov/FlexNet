<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<div>
    <asp:PlaceHolder ID="DisplayNamePlaceHolder" runat="server">
        <div style="padding: 10px;">
            <div style="width:200px; float:left; font-weight:bold;">Display Name:</div>
            <div style="float:left;">
                <sn:DisplayName ID="Name" runat="server" FieldName="DisplayName" ControlMode="Edit" FrameMode="NoFrame" AlwaysUpdateName="true" />
            </div>
            <div style="clear:both;"></div>
        </div>
    </asp:PlaceHolder>
    <asp:PlaceHolder ID="NamePlaceHolder" runat="server">
        <div style="padding: 10px;">
            <div style="width:200px; float:left; font-weight:bold;">Name:</div>
            <div style="float:left;">
                <sn:Name ID="UrlName" runat="server" FieldName="Name" ControlMode="Edit" FrameMode="NoFrame" />
            </div>
            <div style="clear:both;"></div>
        </div>
    </asp:PlaceHolder>
</div>
