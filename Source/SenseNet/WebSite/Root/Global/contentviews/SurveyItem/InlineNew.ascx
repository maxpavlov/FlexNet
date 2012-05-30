<%@  Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SurveyGenericContentView" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>

<div class="sn-survey sn-content">

    <h3 class="sn-content-title"><%= this.Content.ContentHandler.Parent["DisplayName"]%></h3>
    <div class="sn-content-lead"><%= this.Content.ContentHandler.Parent["Description"] %></div>

    <sn:GenericFieldControl ID="GenericFieldControl1" EnablePaging="true" runat="server" ContentListFieldsOnly="True" />

    <div class="sn-panel sn-buttons">
        <sn:CommandButtons ID="CommandButtons1" runat="server" LayoutControlPath="/Root/System/SystemPlugins/Controls/CommandButtons.ascx" />
        <asp:PlaceHolder ID="PlcEmptySurvey" runat="server" Visible="false">
            <asp:Label ID="LblNoQuestion" CssClass="sn-floatleft" runat="server" Visible="false"></asp:Label>
            <sn:BackButton ID="BackButton" Text="Cancel" runat="server" CssClass="sn-button sn-submit" />
        </asp:PlaceHolder>
    </div>
</div>