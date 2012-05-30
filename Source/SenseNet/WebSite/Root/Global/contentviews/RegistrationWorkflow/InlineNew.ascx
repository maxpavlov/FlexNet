<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>

<% 
var smtp = System.Configuration.ConfigurationManager.AppSettings["SMTP"];
if(String.IsNullOrEmpty(smtp)){%>
  <div class="sn-infobox"> 
    <h1 class="sn-content-title" style="color: Red"><img src="/Root/Global/images/icons/32/error.png" class="sn-icon sn-icon_32" alt="">SMTP settings are not present in Web.config.</h1>
  </div>
<%}
else{
    if(smtp.ToLower().Equals("mail.sn.hu")){
%>
 <div class="sn-infobox" style="margin-bottom: 10px"> 
   <h1 class="sn-content-title" style="color: Black"><img src="/Root/Global/images/icons/32/warning.png" class="sn-icon sn-icon_32" alt=""><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("RegistrationWorkflow", "AttentionDefaultSettings") %></h1>
   <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("RegistrationWorkflow", "AttentionDetails")%>
 </div>
<%}%>
<div id="InlineViewContent" runat="server" class="sn-content sn-content-inlineview">
    <sn:ShortText runat="server" ID="FullName" FieldName="FullName" RenderMode="Edit" />
    <sn:ShortText runat="server" ID="UserName" FieldName="UserName" RenderMode="Edit">
      <EditTemplate>
        <asp:TextBox ID="InnerShortText" Class="sn-ctrl sn-ctrl-text sn-ctrl-username" runat="server"></asp:TextBox>
      </EditTemplate>
    </sn:ShortText>
    <sn:ShortText runat="server" ID="Email" FieldName="Email" RenderMode="Edit" />
    <sn:ShortText ID="InitialPassword" runat="server" FieldName="InitialPassword">
      <EditTemplate>
        <asp:TextBox ID="InnerShortText" Class="sn-ctrl sn-ctrl-text sn-ctrl-password" runat="server" TextMode="Password"></asp:TextBox>
      </EditTemplate>
   </sn:ShortText>
    <sn:DropDown runat="server" ID="RegistrationType" FieldName="RegistrationType" RenderMode="Edit" />
</div>
<div class="sn-panel sn-buttons">
  <asp:Button class="sn-submit" ID="StartWorkflow" runat="server" Text="Register" />
</div>
<%}%>

