<%@ Control Language="C#" AutoEventWireup="false" Inherits="SenseNet.Portal.DiscussionForum.ForumView" %>

<sn:ContextInfo ID="ViewContext" runat="server" UsePortletContext="true" />
<sn:SenseNetDataSource ID="ViewDatasource" runat="server" ContextInfoID="ViewContext" />

<div class="sn-forum-main">

    <div class="sn-content">
        <%= ContextElement.Description %>
    </div>
    
    <asp:Repeater ID="ForumBody" DataSourceID="ViewDatasource" runat="server">
        <HeaderTemplate>
        <table class="sn-topiclist">
            <thead>
                <tr>
                    <th class="sn-first">Topics</th>
                    <th>Posts</th>
                    <th class="sn-last">Last entry</th>
                </tr>
            </thead>
        </HeaderTemplate>
        <ItemTemplate>
            <tr class="sn-topic sn-topic-row<%#Container.ItemIndex % 2 %>">
                <td class="sn-topics-col-1 sn-first">
                    <sn:SNIcon ID="SNIcon1" Icon="topics" Size="32" runat="server" />
                    <p class="sn-topic-title">
                        <big><sn:ActionLinkButton runat="server" ID="BrowseLink" IconVisible="false" /></big><br />
                        <small><%# Eval("Description") %></small>
                    </p>
                </td>
                <td class="sn-topics-col-2"><asp:Label runat="server" ID="PostNum" /></td>
                <td class="sn-topics-col-3 sn-last"><asp:Label runat="server" ID="PostDate" /></td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
        </table>
        </FooterTemplate>
    </asp:Repeater>

    <p style="text-align:right;"><br /><b><sn:ActionLinkButton runat="server" ID="ReplyLink" IconName="add" Text="New topic" ActionName="Add" ContextInfoID="ViewContext" ParameterString="ContentTypeName=/Root/ContentTemplates/ForumTopic/NewTopic" /></b></p>
    
</div>
