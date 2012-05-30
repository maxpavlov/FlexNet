<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.UI.Controls.SurveyEditTemplatesUserControl" CodeBehind="SurveyEditTemplatesUserControl.cs" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>

<ul>
    <li>
        <%=GetGlobalResourceObject("Survey", "LandingPage")%>
        <%--<sn:ActionButton ID="albEditAfterSubmit" runat="server" ActionName="EditSurveyTemplate" Text="Edit" CommandName="Copy" command OnCommand="AlbEditAfterSubmit_Click"></sn:ActionButton>--%>
        <asp:Button ID="btnEditLanding" runat="server" OnClick="BtnEdit_Click" CommandArgument="Landing" Visible="false" Text="Edit" />
        <div style="background-color: White; margin:20px"><asp:PlaceHolder runat="server" ID="phLanding"></asp:PlaceHolder></div>
    </li>
    
    <li>
        <%=GetGlobalResourceObject("Survey", "InvalidSurveyPage")%>
        <asp:Button ID="btnEditInvalidSurvey" runat="server" OnClick="BtnEdit_Click" CommandArgument="InvalidSurvey" Visible="false" Text="Edit" />
        <div style="background-color: White; margin:20px"><asp:PlaceHolder runat="server" ID="phInvalidSurvey"></asp:PlaceHolder></div>
    </li>
    
    <li>
        <%=GetGlobalResourceObject("Survey", "MailtTemplatePage")%>
        <asp:Button ID="btnEditMailTemplate" runat="server" OnClick="BtnEdit_Click" CommandArgument="MailTemplate" Visible="false" Text="Edit" />
        <div style="background-color: White; margin:20px"><asp:PlaceHolder runat="server" ID="phMailTemplate"></asp:PlaceHolder></div>
    </li>
</ul>


<%--<div>
    <span><%=GetGlobalResourceObject("Survey", "AfterSubmitPage")%></span>
    <span><a href='<%= GetValue("LandingPage.Path") %>?<%= PortalContext.Current.GeneratedBackUrl %>'>Browse</a></span>
    <span><a href='<%= GetValue("LandingPage.Path") %>?action=EditSurveyTemplate&<%= PortalContext.Current.GeneratedBackUrl %>'>Edit</a></span>
</div>

<div>
    <span><%=GetGlobalResourceObject("Survey", "InvalidSurveyPage")%></span>
    <span><a href='<%= GetValue("InvalidSurveyPage.Path") %>?<%= PortalContext.Current.GeneratedBackUrl %>'>Browse</a></span>
    <span><a href='<%= GetValue("InvalidSurveyPage.Path") %>?action=EditSurveyTemplate&<%= PortalContext.Current.GeneratedBackUrl %>'>Edit</a></span>
</div>

<div>
    <span><%=GetGlobalResourceObject("Survey", "MailtTemplatePage")%></span>
    <span><a href='<%= GetValue("MailTemplate.Path") %>?<%= PortalContext.Current.GeneratedBackUrl %>'>Browse</a></span>
    <span><a href='<%= GetValue("MailTemplate.Path") %>?action=EditSurveyTemplate&<%= PortalContext.Current.GeneratedBackUrl %>'>Edit</a></span>
</div>--%>