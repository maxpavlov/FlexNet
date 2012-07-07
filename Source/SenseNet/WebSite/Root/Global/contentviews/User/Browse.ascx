<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<div class="sn-user">
    <sn:Image ID="Image" CssClass="sn-pic sn-pic-left" runat="server" FieldName="Avatar" RenderMode="Browse">
        <browsetemplate>
            <asp:Image CssClass="sn-pic sn-pic-left" ImageUrl="/Root/Global/images/orgc-missinguser.png" Width="128" Height="128" ID="ImageControl" runat="server" alt="Missing User Image" title="" />
        </browsetemplate>
    </sn:Image>
    <div class="sn-user-actions">
        <sn:ActionLinkButton ID="BtnEdit" CssClass="sn-user-btnedit" runat="server" ActionName="Edit" />
        <sn:ActionLinkButton ID="BtnDelete" CssClass="sn-user-btndelete" runat="server" ActionName="Delete" />
        <sn:ActionLinkButton ID="BtnExplore" CssClass="sn-user-btnexplore" runat="server"
            ActionName="Explore" />
        <sn:ActionLinkButton ID="BtnVersion" CssClass="sn-user-btnversion" runat="server"
            ActionName="Versions" />
    </div>
    <div class="sn-user-properties">
        <table>
            <tr>
                <td class="sn-user-td-left sn-content-subtitle sn-user-main">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "FullNameLabel") %>:
                </td>
                <td class="sn-user-td-right sn-content-title">
                    <%= GetValue("FullName") %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left sn-content-subtitle sn-user-main">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "DomainLabel") %>:
                </td>
                <td class="sn-user-td-right sn-content-title">
                    <%= GetValue("Domain") %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "NameLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <%= GetValue("Name") %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "EmailLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <a href='mailto:<%= GetValue("Email")%>'>
                        <%= GetValue("Email")%></a>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "ManagerLabel") %>:
                </td>
                <td class="sn-user-td-right"">
                    <%
                        if (!GetValue("Manager.Name").Equals(string.Empty))
                        {
                            Response.Write(string.Format("<a href='{0}'>{1}</a>", GetValue("Manager.Path"), GetValue("Manager.Name")));
                        }
                    %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "TitleLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <%= GetValue("DisplayName") %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "PhoneLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <%= GetValue("Phone") %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "DepartmentLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <%= GetValue("Department") %>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "LanguagesLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <%= GetValue("Languages")%>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    <%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "DescriptionLabel") %>:
                </td>
                <td class="sn-user-td-right">
                    <%= GetValue("Description")%>
                </td>
            </tr>
            <tr>
                <td class="sn-user-td-left">
                    Education:
                </td>
                <td class="sn-user-td-right">
                    <sn:EducationEditor ID="Edutor" FieldName="Education" runat="server" FrameMode="NoFrame" />
                </td>
            </tr>
        </table>
    </div>
</div>
