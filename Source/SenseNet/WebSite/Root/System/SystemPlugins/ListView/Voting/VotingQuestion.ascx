<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SurveyContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Register TagPrefix="sn" Assembly="SenseNet.CorePortlets" Namespace="SenseNet.Portal.Portlets" %>

<sn:Toolbar runat="server">
    <asp:LinkButton ID="VotingAndResult" runat="server" Text="Link" Visible="false"/>
</sn:Toolbar>

<asp:Panel runat="server" ID="QuestionPanel">
    <h2 class="sn-content-title"><%= GetValue("DisplayName") %></h2>
    <div class="sn-lead"><%= GetValue("Description") %></div>
    <asp:PlaceHolder runat="server" ID="QuestionPlaceHolder"></asp:PlaceHolder>
</asp:Panel>

