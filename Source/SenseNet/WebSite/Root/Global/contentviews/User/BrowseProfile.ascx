<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.UI.SingleContentView" EnableViewState="false" %>
<%@ Register Assembly="SenseNet.Portal" Namespace="SenseNet.Portal.UI.Controls" TagPrefix="sn" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>

<% 
    var email = this.Content["Email"] as string;
    var phone = this.Content["Phone"] as string;
    var managerRefList = this.Content["Manager"] as IEnumerable<Node>;
    var manager = managerRefList == null ? null : managerRefList.FirstOrDefault() as User;
    var department = this.Content["Department"] as string;
    var languages = this.Content["Languages"] as string;
    var education = this.Content["Education"] as string;
    
    var username = User.Current.Name;
    var profileUsername = String.Concat(GetValue("Domain"), @"\", GetValue("Name"));
%>

<div class="sn-user-properties">
    <sn:Image ID="Image" CssClass="sn-avatar" runat="server" FieldName="Avatar" RenderMode="Browse">
        <browsetemplate>
            <asp:Image CssClass="sn-avatar" ImageUrl="/Root/Global/images/default_avatar.png" ID="ImageControl" runat="server" alt="Missing User Image" title="" />
        </browsetemplate>
    </sn:Image>
    
    <h1 class="sn-content-title"><%= GetValue("FullName") %></h1>

    <% if((username == profileUsername)) { %>
    <sn:ActionLinkButton ID="BtnEdit"   runat="server" ActionName="EditProfile" Text="Edit profile" /><br />
    <sn:ActionLinkButton ID="alButton1" runat="server" ActionName="Notifications" Text="Notifications"/><br />
    <sn:ActionLinkButton ID="alButton2" runat="server" ActionName="SetPermissions" Text="Permissions" />
    <% } %>

    <dl>
        <dt>User name:</dt><dd><%= profileUsername %></dd>
    <% if (!string.IsNullOrEmpty(email)) { %><dt>E-mail:</dt><dd><a href='mailto:<%= GetValue("Email")%>'><%= email %></a></dd>  <% } %>
    <% if (!string.IsNullOrEmpty(phone)) { %><dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "PhoneLabel") %>:</dt><dd><%= phone %></dd>  <% } %>
    <% if (manager != null) { %>
        <dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "ManagerLabel")%>:</dt>
        <dd><a href='<%= Actions.ActionUrl(SenseNet.ContentRepository.Content.Create(manager), "Profile") %>' title="[manager]"><%= manager.FullName %></a></dd>
    <% } %>
    <% if (!string.IsNullOrEmpty(department)) { %>
        <dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "DepartmentLabel") %>:</dt><dd><%= department%></dd>
    <% } %>
    <% if (!string.IsNullOrEmpty(languages)) { %>
        <dt><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("UserBrowse", "LanguagesLabel")%>:</dt><dd><%= languages%></dd>
    <% } %>
    <% if (!UserProfiles.IsEducationEmpty(education)) { %>
        <dt>Education:</dt>
        <dd>
            <sn:EducationEditor ID="Edutor" FieldName="Education" runat="server" FrameMode="NoFrame">
                <browsetemplate>
                <asp:ListView ID="InnerListView" runat="server" EnableViewState="false">
                    <LayoutTemplate>
                            <asp:placeholder runat="server" id="itemPlaceHolder" />
                    </LayoutTemplate>
                    <ItemTemplate>
                        <span><%# DataBinder.Eval(Container.DataItem, "SchoolName")%></span>
                    </ItemTemplate>
                    <ItemSeparatorTemplate>,</ItemSeparatorTemplate>
                    <EmptyDataTemplate>
                    </EmptyDataTemplate>
                </asp:ListView>
                </browsetemplate>
            </sn:EducationEditor>
        </dd>
    <% } %>

    </dl>

</div>
