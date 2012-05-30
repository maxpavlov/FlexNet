<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<asp:ListView ID="ContentList" runat="server" EnableViewState="false">
    <LayoutTemplate>
        <div class="sn-surveylist sn-contentlist">
            <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
        </div>
    </LayoutTemplate>
    <ItemTemplate>
        <div class="sn-content sn-survey">
            <h2 class="sn-content-title"><%# Eval("DisplayName") %></h2>
            <p class="sn-content-lead"><%#Eval("Description") %></p>
            <sn:ActionLinkButton NodePath='<%#Eval("Path") %>' ActionName="Add" IconName="edit" Text="<%$ Resources: Survey, FillOut %>" ParameterString='<%# "ContentTypeName=SurveyItem" %>' runat="server" />
        </div>
    </ItemTemplate>
</asp:ListView>