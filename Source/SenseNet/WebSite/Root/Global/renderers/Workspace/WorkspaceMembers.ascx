<%@ Control Language="C#" AutoEventWireup="true" Inherits="SenseNet.Portal.Portlets.ContentCollectionView" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.UI" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.ContentRepository.Schema" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>

<sn:ScriptRequest Path="$skin/scripts/sn/SN.Workspace.js" runat="server" />

<div id="sn-workspacemembers-removeconfirm">
    <div class="sn-workspacemembers-removeconfirm-text">
        Are you sure you want to remove <a id="sn-workspacemembers-removeconfirm-name"></a> from <label id="sn-workspacemembers-removeconfirm-groupname"></label>?
        <input type="hidden" id="sn-workspacemembers-removeconfirm-groupid" />
        <input type="hidden" id="sn-workspacemembers-removeconfirm-userid" />
    </div>
    <div class="sn-workspacemembers-buttoncontainer">
        <div class="sn-workspacemembers-buttondiv">
            <input type="button" class="sn-submit sn-button sn-notdisabled" value="Remove" onclick="SN.WorkspaceMembers.okRemove();return false;" />
            <input type="button" class="sn-submit sn-button sn-notdisabled" value="Cancel" onclick="SN.WorkspaceMembers.cancelRemove();return false;" />
        </div>
    </div>
</div>

<%
    var settings = new SenseNet.Search.QuerySettings { EnableAutofilters = false };
    var workspaceGroups = SenseNet.Search.ContentQuery.Query("+TypeIs:Group +InTree:\"" + PortalContext.Current.ContextWorkspace.Path + "\"", settings).Nodes;
%>

<div id="sn-workspacemembers-adduser">
    <div class="sn-workspacemembers-removeconfirm-text">
        <small>Pick users into the following groups: </small><br /><br />
        <% foreach (var group in workspaceGroups.OrderBy(n => n.DisplayName))
            { %>
        <div class="sn-workspacemembers-adduser-groupdiv">
            <div class="sn-workspacemembers-adduser-leftcol">
                <img class="sn-wall-smallicon" src="/Root/Global/images/icons/16/group.png" /><%= Actions.BrowseAction(group.Path, true)%>
            </div>
            <div class="sn-workspacemembers-adduser-rightcol">
                <input class="sn-workspacemembers-groupid" type="hidden" value="<%= group.Id %>" />
                <div class="sn-workspacemembers-userpicker"></div>
                <input type="button" class="sn-submit sn-button sn-notdisabled sn-workspacemembers-adduser-pickuser" value="..." onclick="SN.WorkspaceMembers.addUserPick($(this));return false;" />
                <div style="clear:both;">
                </div>
            </div>
            <div style="clear:both;">
            </div>
        </div>
        <% } %>
    </div>
    <div class="sn-workspacemembers-buttoncontainer">
        <div class="sn-workspacemembers-buttondiv">
            <input type="button" class="sn-submit sn-button sn-notdisabled" value="Add" onclick="SN.WorkspaceMembers.okAdd();return false;" />
            <input type="button" class="sn-submit sn-button sn-notdisabled" value="Cancel" onclick="SN.WorkspaceMembers.cancelAdd();return false;" />
        </div>
    </div>
</div>

<%
    var members = new[] { new { Group = null as SenseNet.ContentRepository.Group, Member = null as Node } }.ToList();
    members.Clear();
    var groups = (this.Parent as ContextBoundPortlet).ContextNode;
    var groupsFolder = groups as IFolder;
    foreach (var groupNode in groupsFolder.Children)
    {
        var group = groupNode as SenseNet.ContentRepository.Group;
        if (group == null)
            continue;
        members.AddRange(group.Members.Select(m => new { Group = group, Member = m }));
    }

    var memberGroupLists = from m in members
                       orderby m.Member.NodeType.IsInstaceOfOrDerivedFrom("User") ? m.Member["FullName"] : m.Member.DisplayName
                       group m by m.Member.Id into w
                       select new { Member = w.Key, Groups = w };
    
     %>

<div class="sn-workspacemembers">
    <% var editable = groups.Security.HasPermission(SenseNet.ContentRepository.Storage.Schema.PermissionType.Save); %>

    <% if (editable) { %>
    <div class="sn-workspacemembers-addmembers">
        <img src="/Root/Global/images/icons/16/add.png" class="sn-workspacemembers-addmembersimg" />
        <a href="javascript:" onclick="SN.WorkspaceMembers.add(); return false;">Add new members</a>
    </div>
    <% } %>
    <div class="sn-workspacemembers-items">

        <%  var index = 0;
            var ids = string.Empty;
            foreach (var memberGroupList in memberGroupLists)
          {
              var groupInfos = memberGroupList.Groups.OrderBy(g => g.Group.DisplayName);
              var firstGroup = groupInfos.First();
              var node = firstGroup.Member;
                
              var content = SenseNet.ContentRepository.Content.Create(node);
              ids += node.Id.ToString() + ',';
              var name = node.NodeType.IsInstaceOfOrDerivedFrom("User") ? node["FullName"] : node.DisplayName;
              var link = node.NodeType.IsInstaceOfOrDerivedFrom("User") ? Actions.ActionUrl(content, "Profile") : Actions.BrowseUrl(content);
              index++;
                %>
            <div class="sn-workspacemembers-item <%= index > 5 ? "sn-workspacemembers-item-hidden" : "" %>">
			    <div class="sn-workspacemembers-itemavatardiv">
				    <img src=<%= UITools.GetAvatarUrl(node) %> style="width:32px; height:32px;" />
			    </div>
			    <div class="sn-workspacemembers-itemnamediv">
				    <a class="sn-workspacemembers-itemnamelink" href="<%= link %>"><%=name %></a><br />
                    <div>
                    <%
              foreach (var groupInfo in groupInfos)
              {
                         %>
                    <span class="sn-workspacemembers-itemremovelink" style="display:inline-block;"><%= Actions.BrowseAction(groupInfo.Group.Path, true) %>
                    <% if (editable)
                       { %>
                        <a href="javascript:" onclick="SN.WorkspaceMembers.remove($(this), <%= node.Id %>, '<%= link %>', '<%= name %>', '<%= groupInfo.Group.Id %>', '<%= groupInfo.Group.DisplayName %>'); return false;" >
                            <img src="/Root/Global/images/icons/16/delete.png" class="sn-workspacemembers-itemremoveimg" />
                        </a>
                    <% } %>
                    </span>
                    <% } %>
                    </div>
			    </div>
			    <div style="clear:both;">
			    </div>
            </div>
        <%}
            ids = ids.TrimEnd(',');
           %>
    </div>
    <input id="sn-workspacemembers-selecteditems" type="hidden" value="<%= ids %>" />

    
    <% if (index > 5) { %>
        <div class="sn-workspacemembers-showall">
            <a href="javascript:" onclick="SN.WorkspaceMembers.showAll($(this)); return false;">Show all <%= index %> members</a>
        </div>
    <% } %>
</div>
