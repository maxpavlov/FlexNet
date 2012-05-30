<%@ Control Language="C#" AutoEventWireup="false" Inherits="System.Web.UI.UserControl" %>
<sn:scriptrequest id="request1" runat="server" path="$skin/scripts/sn/SN.WorkspaceCreateOther.js" />
<%@ Import Namespace="SenseNet.Portal.UI" %>
<%@ Import Namespace="SenseNet.Portal.Helpers" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage.Schema" %>
<%@ Import Namespace="SenseNet.ContentRepository.i18n" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<% 
    var contentTypeList = PortalContext.Current.ContextWorkspace.GetAllowedChildTypes().ToList();
    var isListEmpty = contentTypeList.Count == 0;
    var allowed = PortalContext.Current.ArbitraryContentTypeCreationAllowed;
    var wspContent = SenseNet.ContentRepository.Content.Create(PortalContext.Current.ContextWorkspace);
    var currCntDisplName = PortalContext.Current.ContextNode.DisplayName;
%>
<div id="sn-page-createitem" class="sn-content">
    <% if(isListEmpty && !allowed){ %>
        <h1 class="sn-content-title"><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_YouDontHavePermissionText")%></h1>
        <div class="sn-panel sn-buttons">
        <sn:backbutton cssclass="sn-submit" text="Ok" runat="server" id="BackButton1" />
    </div>
    <% } else { %>
    <h1 class="sn-content-title"><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_SelectAnItemText")%></h1>
    <ul id="sn-createitem-types">
        <li class="sn-pos-1 sn-active">
            <h2><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_WorkspacesTitle") %></h2>
            <p><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_WorkspacesDescription") %></p>
            
                <% 
                    var excludedWorkspaceNodeTypes = new List<string> { "TrashBin", "Site", "Workspace" };
                    IEnumerable<NodeType> wsList = null;
                    if (!isListEmpty)
                    {
                        wsList = contentTypeList.Where(ct => SenseNet.ContentRepository.Schema.ContentType.GetByName(ct.Name).IsInstaceOfOrDerivedFrom("Workspace")).Select(x => NodeType.GetByName(x.Name));
                    }
                    else
                    {
                        wsList = NodeType.GetByName("Workspace").GetAllTypes();
                    }
                    var wsNodeTypesList = wsList.Where(wsnt => !excludedWorkspaceNodeTypes.Contains(wsnt.Name));
                    if (wsNodeTypesList.Count() == 0)
                    {
                        %>
                        <div style="padding: 5px;"><%= String.Format(SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_CantCreateWorkspaceContent"), currCntDisplName) %></div>
                        <%
                    }
                    else
                    {
                        %>
                        <dl>
                        <%
                        foreach (NodeType nt in wsNodeTypesList)
                        {
                            var ct = SenseNet.ContentRepository.Schema.ContentType.GetByName(nt.Name);
                            var template = ContentTemplate.GetTemplate(nt.Name);
                            var ctName = template == null ? ct.Name : template.Path;
                            var actionUrl = Actions.ActionUrl(wspContent, "Add", null, new { ContentTypeName = ctName, backtarget = "newcontent" });
                %>
                <dt><a href='<%= actionUrl %>'>
                    <%= IconHelper.RenderIconTag(ct.Icon) + ct.DisplayName%></a> </dt>
                <dd>
                    <%= ct.Description%></dd>
                <% } %> </dl> <%
                    } %>
            
        </li>
        <li class="sn-pos-2">
            <h2><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_ListsTitle") %></h2>
            <p> <%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_ListsDescription") %></p>
            
                <% 
                    var excludedListNodeTypes = new List<string> { "ItemList" };

                    IEnumerable<NodeType> listList = null;
                    if (!isListEmpty)
                    {
                        listList = contentTypeList.Where(ct => SenseNet.ContentRepository.Schema.ContentType.GetByName(ct.Name).IsInstaceOfOrDerivedFrom("ItemList")).Select(x => NodeType.GetByName(x.Name));
                    }
                    else
                    {
                        listList = NodeType.GetByName("ItemList").GetAllTypes();
                    }
                    var listNodeTypesList = listList.Where(listnt => !excludedListNodeTypes.Contains(listnt.Name));
                    if (listNodeTypesList.Count() == 0)
                    {
                        %>
                        <div style="padding: 5px;"><%= String.Format(SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_CantCreateListContent"), currCntDisplName)%></div>
                        <%
                    }
                    else
                    {
                        %><dl><%
                        foreach (var nt in listNodeTypesList)
                        {
                            var ct = SenseNet.ContentRepository.Schema.ContentType.GetByName(nt.Name);
                            var template = ContentTemplate.GetTemplate(nt.Name);
                            var ctName = template == null ? ct.Name : template.Path;
                            var actionUrl = Actions.ActionUrl(wspContent, "Add", null, new { ContentTypeName = ctName, backtarget = "newcontent" });
                %>
                <dt><a href='<%= actionUrl %>'>
                    <%= IconHelper.RenderIconTag(ct.Icon) + ct.DisplayName%></a></dt>
                <dd>
                    <%= ct.Description%></dd>
                <% } %></dl><%
                    } %>
            
        </li>
        <li class="sn-pos-3">
            <h2><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_LibrariesTitle") %></h2>
            <p><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_LibrariesDescription") %></p>
            
                <% 
                    var excludedLibraryNodeTypes = new List<string> { "Library" };
                    IEnumerable<NodeType> libList = null;
                    if (!isListEmpty)
                    {
                        libList = contentTypeList.Where(ct => SenseNet.ContentRepository.Schema.ContentType.GetByName(ct.Name).IsInstaceOfOrDerivedFrom("Library")).Select(x => NodeType.GetByName(x.Name));
                    }
                    else
                    {
                        libList = NodeType.GetByName("Library").GetAllTypes();
                    }
                    var libNodeTypesList = libList.Where(libnt => !excludedLibraryNodeTypes.Contains(libnt.Name));
                    if (libNodeTypesList.Count() == 0)
                    {
                        %>
                        <div style="padding: 5px;"><%= String.Format(SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_CantCreateLibraryContent"), currCntDisplName)%></div>
                        <%
                    }
                    else
                    {
                        %><dl><%
                        foreach (var nt in libNodeTypesList)
                        {
                            var ct = SenseNet.ContentRepository.Schema.ContentType.GetByName(nt.Name);
                            var template = ContentTemplate.GetTemplate(nt.Name);
                            var ctName = template == null ? ct.Name : template.Path;
                            var actionUrl = Actions.ActionUrl(wspContent, "Add", null, new { ContentTypeName = ctName, backtarget = "newcontent" });
                %>
                <dt><a href='<%= actionUrl %>'>
                    <%= IconHelper.RenderIconTag(ct.Icon) + ct.DisplayName%></a></dt>
                <dd>
                    <%= ct.Description%></dd>
                <% } %> </dl> <%
                    } %>
            
        </li>
        <li class="sn-pos-4 sn-last">
            <h2><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_PagesTitle") %></h2>
            <p><%= SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_PagesDescription") %></p>
            
                <% 
                    var excludedPageNodeTypes = new List<string> { };
                    IEnumerable<NodeType> pageList = null;
                    if (!isListEmpty)
                    {
                        pageList = contentTypeList.Where(ct => SenseNet.ContentRepository.Schema.ContentType.GetByName(ct.Name).IsInstaceOfOrDerivedFrom("Page")).Select(x => NodeType.GetByName(x.Name));
                    }
                    else
                    {
                        pageList = NodeType.GetByName("Page").GetAllTypes();
                    }
                    var pageNodeTypesList = pageList.Where(pagent => !excludedPageNodeTypes.Contains(pagent.Name));
                    if (pageNodeTypesList.Count() == 0)
                    {
                        %>
                        <div style="padding: 5px;"><%= String.Format(SenseNetResourceManager.Current.GetString("Portal", "WorkspaceCreateOther_CantCreatePageContent"), currCntDisplName)%></div>
                        <%
                    }
                    else
                    {
                        %><dl><%
                        foreach (var nt in pageNodeTypesList)
                        {
                            var ct = SenseNet.ContentRepository.Schema.ContentType.GetByName(nt.Name);
                            var template = ContentTemplate.GetTemplate(nt.Name);
                            var ctName = template == null ? ct.Name : template.Path;
                            var actionUrl = Actions.ActionUrl(wspContent, "Add", null, new { ContentTypeName = ctName, backtarget = "newcontent" });
                %>
                <dt><a href='<%= actionUrl %>'>
                    <%= IconHelper.RenderIconTag(ct.Icon) + ct.DisplayName%></a></dt>
                <dd>
                    <%= ct.Description%></dd>
                <% } %></dl><%
                    } %>
            
        </li>
    </ul>    
    <div class="sn-panel sn-buttons">
        <sn:backbutton cssclass="sn-submit" text="Done" runat="server" id="BackButton2" />
    </div>
    <% } %>
</div>
