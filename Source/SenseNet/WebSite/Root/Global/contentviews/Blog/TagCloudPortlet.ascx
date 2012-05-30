<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="~/Controls/TagCloudPortlet.ascx.cs" Inherits="SenseNet.Portal.Portlets.Controls.TagCloudControl" %>

<script runat="server" type="text/C#">
protected string GetCurrentUrl()
{
    if (SenseNet.Portal.Virtualization.PortalContext.Current != null && SenseNet.Portal.Virtualization.PortalContext.Current.ContextWorkspace != null)
    {
        return SenseNet.Portal.Helpers.Actions.ActionUrl(SenseNet.ContentRepository.Content.Load(SenseNet.Portal.Virtualization.PortalContext.Current.ContextWorkspace.Path), "Search", false);
    }
    else return String.Empty;
}    
</script>
<% if((this.FindControl("TagCloudRepeater") as Repeater).Items.Count == 0){%>
    <div class="sn-tags-notags"><span><%= SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "SnBlog_TagCloudPortlet_NoTags") %></span></div>
<%}
else{%>
    <asp:Repeater ID="TagCloudRepeater" runat="server">
        <HeaderTemplate>
            <div class="sn-tags">
                <ul>
        </HeaderTemplate>
        <ItemTemplate>
            <li class="sn-tag<%# Eval("Value") %>">           
                <a title='<%# Eval("Key") %>' href='<%# GetCurrentUrl() + "&text=" + HttpUtility.UrlEncode(Eval("Key").ToString()) %>'><%# Eval("Key")%></a>
            </li>
        </ItemTemplate>
        <FooterTemplate>
            </ul> </div>
        </FooterTemplate>
    </asp:Repeater>
<%} %>
