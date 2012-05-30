<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SurveyContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>

<div class="sn-survey sn-content">
    <h3 class="sn-content-title"><%= GetValue("DisplayName")%></h3>
    <div class="sn-content-lead"><%= GetValue("Description")%></div>

    <div>
        <sn:ActionLinkButton NodePath='<%# Eval("Path") %>' ActionName="Add" IconName="edit" Text="<%$ Resources: Survey, FillOut %>" ParameterString='<%# "ContentTypeName=SurveyItem" %>' runat="server" />
    </div>
    <div><asp:Label CssClass="sn-error" runat="server" ID="LiteralMessage" Visible="false"></asp:Label></div>
    <div><asp:PlaceHolder runat="server" ID="phInvalidPage" /></div>
</div>
