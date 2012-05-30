<%@ Control Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="SenseNet.ContentRepository" %>
<%@ Import Namespace="SenseNet.ContentRepository.Storage" %>
<%@ Import Namespace="SenseNet.Portal.Virtualization" %>

<div>
    <div>
        <asp:PlaceHolder ID="WorkspaceIsWallContainer" runat="server" Visible="false">
            <div class="sn-wall-workspacewarning">
                <img src="/Root/Global/images/icons/16/warning.png" class="sn-wall-smallicon" />The current context workspace (<asp:HyperLink ID="PortletContextNodeLink" runat="server" />) is not configured to contain a Wall. This might be a problem when using sub-workspaces under this workspace and sharing Content. 
            <br /><br />
            <strong>Would you like to configure the current workspace as a Wall container?</strong>
            <asp:Button ID="ConfigureWorkspaceWall" runat="server" Text="Configure Workspace as Wall Container" CssClass="sn-submit sn-button" />
            </div>
        </asp:PlaceHolder>
    </div>
    <div class="sn-likelist">
        <div class="sn-likelist-items">
        </div>
        <div class="sn-likelist-buttoncontainer">
            <div class="sn-likelist-buttondiv">
                <input type="button" class="sn-submit sn-button sn-notdisabled" value="Close" onclick="SN.Wall.closeLikeList();return false;" />
            </div>
        </div>
    </div>
    
    <% 
        var contentTypeList = PortalContext.Current.ContextWorkspace.GetAllowedChildTypes().ToList();
        var isListEmpty = contentTypeList.Count == 0;
        var allowed = PortalContext.Current.ArbitraryContentTypeCreationAllowed;
        var wsPath = PortalContext.Current.ContextWorkspace.Path;
            
        var postAllowed = SenseNet.Portal.Wall.WallHelper.HasWallPermission(wsPath, null);
    %>
    <% if (postAllowed) { %>
    <div class="sn-dropbox">
        <div class="sn-dropbox-buttons">
            <a class="sn-dp-post sn-dropbox-buttons-selected" href="javascript:" onclick="SN.Wall.dropboxSelect($(this),$('.sn-dropbox-postboxdiv')); return false;">Post</a>
            <a class="sn-dp-post" href="javascript:" onclick="SN.Wall.dropboxSelect($(this),$('.sn-dropbox-createboxdiv')); return false;">Create Content</a>
            <a class="sn-dp-post" href="javascript:" onclick="SN.Wall.dropboxSelect($(this),$('.sn-dropbox-uploadboxdiv')); return false;">Upload Files</a>
        </div>
        <div class="sn-dropbox-postboxdiv sn-dropboxdiv">
            <textarea class="sn-unfocused-postbox sn-postbox" onkeydown="if (SN.Wall.createPost(event, $(this))) return false;" onfocus="SN.Wall.onfocusPostBox($(this));" onblur="SN.Wall.onblurPostBox($(this));">Post something...</textarea>
        </div>
        <div class="sn-dropbox-createboxdiv sn-dropboxdiv ui-helper-clearfix">
            <div class="sn-dropbox-createboxcolumn">
                <% if (Node.Exists(RepositoryPath.Combine(wsPath, "Tasks"))) { %><div><img src="/Root/Global/images/icons/16/FormItem.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('Task','/Tasks');return false;">Task</a></div><% } %>
                <% if (Node.Exists(RepositoryPath.Combine(wsPath, "Memos"))) { %><div><img src="/Root/Global/images/icons/16/Document.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('Memo','/Memos');return false;">Memo</a></div><% } %>
                <% if (Node.Exists(RepositoryPath.Combine(wsPath, "Links"))) { %><div><img src="/Root/Global/images/icons/16/link.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('Link','/Links');return false;">Link</a></div><% } %>
                <% if (Node.Exists(RepositoryPath.Combine(wsPath, "Calendar"))) { %><div><img src="/Root/Global/images/icons/16/CalendarEvent.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('CalendarEvent','/Calendar');return false;">Event</a></div><% } %>
                <% if (Node.Exists(RepositoryPath.Combine(wsPath, "Wiki/Articles"))) { %><div><img src="/Root/Global/images/icons/16/WikiArticle.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('WikiArticle','/Wiki/Articles');return false;">Wiki Article</a></div><% } %>
                <div><img src="/Root/Global/images/icons/16/Folder.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('Folder','');return false;">Folder</a></div>
                <% if (Node.Exists(RepositoryPath.Combine(wsPath, "Document_Library"))) { %>
                <div><img src="/Root/Global/images/icons/16/document.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('/Root/ContentTemplates/File/Empty text document.txt','/Document_Library');return false;">Empty text document.txt</a></div>
                <div><img src="/Root/Global/images/icons/16/word.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('/Root/ContentTemplates/File/Empty document.docx','/Document_Library');return false;">Empty document.docx</a></div>
                <div><img src="/Root/Global/images/icons/16/excel.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('/Root/ContentTemplates/File/Empty workbook.xlsx','/Document_Library');return false;">Empty workbook.xlsx</a></div>
                <div><img src="/Root/Global/images/icons/16/powerpoint.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('/Root/ContentTemplates/File/Empty presentation.pptx','/Document_Library');return false;">Empty presentation.pptx</a></div>
                <% } %>
            </div>
            <div class="sn-dropbox-createboxcolumn">
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "DocumentLibrary")) { %><div><img src="/Root/Global/images/icons/16/ContentList.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("DocumentLibrary") ? ContentTemplate.GetTemplate("DocumentLibrary").Path : "DocumentLibrary" %>','');return false;">Document Library</a></div><% } %>
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "TaskList")) { %><div><img src="/Root/Global/images/icons/16/ContentList.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("TaskList") ? ContentTemplate.GetTemplate("TaskList").Path : "TaskList" %>','');return false;">TaskList</a></div><% } %>
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "MemoList")) { %><div><img src="/Root/Global/images/icons/16/ContentList.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("MemoList") ? ContentTemplate.GetTemplate("MemoList").Path : "MemoList" %>','');return false;">MemoList</a></div><% } %>
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "LinkList")) { %><div><img src="/Root/Global/images/icons/16/ContentList.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("LinkList") ? ContentTemplate.GetTemplate("LinkList").Path : "LinkList" %>','');return false;">LinkList</a></div><% } %>
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "EventList")){ %><div><img src="/Root/Global/images/icons/16/ContentList.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("EventList") ? ContentTemplate.GetTemplate("EventList").Path : "EventList" %>','');return false;">EventList</a></div><% } %>
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "Wiki"))     { %><div><img src="/Root/Global/images/icons/16/Wiki.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("Wiki") ? ContentTemplate.GetTemplate("Wiki").Path : "Wiki" %>','');return false;">Wiki</a></div><% } %>
                <% if ((isListEmpty && allowed) || contentTypeList.Any(ct => ct.Name == "Blog"))     { %><div><img src="/Root/Global/images/icons/16/ContentList.png" class="sn-wall-smallicon" /><a href="javascript:" onclick="SN.Wall.createContent('<%= ContentTemplate.HasTemplate("Blog") ? ContentTemplate.GetTemplate("Blog").Path : "Blog" %>','');return false;">Blog</a></div><% } %>
            </div>
            <div class='sn-dropbox-createothersdiv ui-helper-clearfix'>
                <div class='sn-dropbox-createothers-left'>Create other</div>
                <div class='sn-dropbox-createothers-right'>
                    <input class="sn-dropbox-createother sn-unfocused-postbox" onfocus="SN.Wall.onfocusPostBox($(this), false, true);" onblur="SN.Wall.onblurPostBox($(this), false, true);" onkeydown="if (event.keyCode == 13) return false;" onkeyup="SN.Wall.onchangeCreateOtherBox(event, $(this)); return false;" type="text" value="Start typing..."/>
                    <a class='sn-dropbox-createothers-showalllink' tabindex='-1' href='javascript:' title='Show all types' onclick='SN.Wall.createOtherShowAll();'><img class='sn-dropbox-createothers-showallimg' src='/Root/Global/images/actionmenu_down.png'/></a>
                    <input class="sn-dropbox-createother-value" type="hidden" />
                    <input type="button" class="sn-dropbox-createother-submit sn-submit sn-button sn-notdisabled" value="Create" onclick="SN.Wall.createOther(); return false;" /><input type="button" class="sn-dropbox-createother-submitfake sn-submit sn-button sn-notdisabled sn-disabled" disabled="disabled" value="Create" />
                    <div class="sn-dropbox-createother-autocomplete"></div>
                </div>
            </div>
        </div>
        <div class="sn-dropbox-uploadboxdiv sn-dropboxdiv">
            <input type='button' class='sn-submit sn-button sn-notdisabled' value='Upload files to...' onclick='SN.Wall.dropboxUpload(); return false;' />
        </div>
    </div>    
    <% } %>
    <input type="hidden" class="sn-posts-workspace-path" value=<%= SenseNet.Portal.Virtualization.PortalContext.Current.ContextWorkspace.Path %> />
    <div id="sn-posts">
        <asp:PlaceHolder ID="Posts" runat="server"></asp:PlaceHolder>
    </div>
</div>
