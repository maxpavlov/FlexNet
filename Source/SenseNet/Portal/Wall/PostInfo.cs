using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Workspaces;

namespace SenseNet.Portal.Wall
{
    public enum PostType
    {
        BigPost = 0,
        JournalCreated,
        JournalModified,
        JournalDeletedPhysically,
        JournalMoved,
        JournalCopied
    }

    public class PostInfo
    {
        // =============================================================================== Properties
        public int Id { get; private set; }
        public string ClientId { get; private set; }
        public int JournalId { get; private set; }
        public DateTime CreationDate { get; private set; }
        public User CreatedBy { get; private set; }
        public string Text { get; private set; }
        public PostType Type { get; private set; }
        public Node SharedContent { get; private set; }
        public string Action { get; private set; }
        public string Details { get; private set; }
        public string LastPath { get; private set; }


        // =============================================================================== Static methods
        public static int GetIdFromClientId(string clientId)
        {
            var idStr = clientId;
            if (clientId.StartsWith("J"))
                idStr = clientId.Substring(1);
            return Convert.ToInt32(idStr);
        }
        public static bool IsJournalId(string clientId)
        {
            return clientId.StartsWith("J");
        }


        // =============================================================================== Constructors
        public static PostInfo CreateFromCommentOrLike(Node commentOrLike)
        {
            var targetContent = commentOrLike.Parent.Parent;

            // comment likes should not appear, only content likes
            if (targetContent.NodeType.Name == "Comment")
                return null;

            var postInfo = new PostInfo();
            postInfo.CreationDate = commentOrLike.CreationDate;
            postInfo.CreatedBy = commentOrLike.CreatedBy as User;
            postInfo.Action = commentOrLike.NodeType.Name == "Comment" ? "commented on a Content" : "likes a Content";
            postInfo.Id = targetContent.Id;
            postInfo.ClientId = postInfo.Id.ToString();
            postInfo.Type = PostType.BigPost;
            postInfo.SharedContent = targetContent;
            return postInfo;
        }
        private PostInfo()
        { 
        }
        public PostInfo(Node node)
        {
            CreationDate = node.CreationDate;
            CreatedBy = node.CreatedBy as User;
            Text = node.GetProperty<string>("Description");
            Details = node.GetProperty<string>("PostDetails"); 
            JournalId = node.GetProperty<int>("JournalId"); // journal's id to leave out item from wall if post already exists
            Id = node.Id;
            ClientId = Id.ToString();
            Type = (PostType)node.GetProperty<int>("PostType");
            SharedContent = node.GetReference<Node>("SharedContent");
            if (SharedContent != null) {
                LastPath = SharedContent.Path;
                var ws = SenseNet.ContentRepository.Workspaces.Workspace.GetWorkspaceWithWallForNode(node);
                if (ws != null)
                    Action = string.Format("to <a href='{0}'>{1}</a>", ws.Path, ws.DisplayName);
            }
        }
        public PostInfo(JournalItem journalItem, string lastPath)
        {
            LastPath = lastPath;
            CreationDate = journalItem.When;
            var backspIndex = journalItem.Who.IndexOf('\\');
            if (backspIndex != -1)
            {
                var domain = journalItem.Who.Substring(0, backspIndex);
                var name = journalItem.Who.Substring(backspIndex + 1);
                CreatedBy = User.Load(domain, name);
            }

            //var contentName = string.Empty;
            //if (journalItem.Wherewith.StartsWith(contextPath))
            //{
            //    contentName = journalItem.Wherewith.Substring(contextPath.Length).TrimStart('/');
            //    // if workspace relative path is empty, the context path is the workspace itself
            //    if (string.IsNullOrEmpty(contentName))
            //        contentName = RepositoryPath.GetFileName(contextPath);
            //}
            //else
            //{
            //    contentName = RepositoryPath.GetFileName(journalItem.Wherewith);
            //}

            var what = journalItem.What.ToLower();
            if (what == "deletedphysically")
                what = "deleted";

            // type
            if (what == "created")
                Type = PostType.JournalCreated;
            if (what == "modified")
                Type = PostType.JournalModified;
            if (what == "deleted")
                Type = PostType.JournalDeletedPhysically;
            if (what == "moved")
                Type = PostType.JournalMoved;
            if (what == "copied")
                Type = PostType.JournalCopied;

            Text = Type == PostType.JournalCopied || Type == PostType.JournalMoved ?
                string.Format("{0} <a href='{1}'>{2}</a> to <a href='{3}'>{4}</a>", what, "{{path}}", journalItem.DisplayName, journalItem.TargetPath, journalItem.TargetDisplayName) :
                string.Format("{0} <a href='{1}'>{2}</a>", what, "{{path}}", journalItem.DisplayName);

            JournalId = journalItem.Id; // journal's id to leave out item from wall if post already exists
            Id = journalItem.Id;   // negative id differentiates journal post from Post Contents on client side
            ClientId = "J" + Id.ToString();

            // details
            switch (Type)
            {
                case PostType.JournalModified:
                    Details = journalItem.Details;
                    break;
                case PostType.JournalMoved:
                    Details = string.Format("Source: <a href='{0}'>{0}</a><br/>Target: <a href='{1}'>{1}</a>", RepositoryPath.GetParentPathSafe(journalItem.SourcePath), journalItem.TargetPath);
                    break;
                case PostType.JournalCopied:
                    Details = string.Format("Source: <a href='{0}'>{0}</a><br/>Target: <a href='{1}'>{1}</a>", RepositoryPath.GetParentPathSafe(journalItem.Wherewith), journalItem.TargetPath);
                    break;
                default:
                    break;
            }

            //SharedContent = Node.LoadNode(journalItem.Wherewith);
        }
    }
}
