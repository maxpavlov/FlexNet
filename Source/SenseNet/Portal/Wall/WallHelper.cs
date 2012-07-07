using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.IO;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using SenseNet.Portal.UI;
using SenseNet.ContentRepository.Workspaces;
using SenseNet.ApplicationModel;
using SenseNet.Portal.Helpers;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.Portal.Wall
{
    public class WallHelper
    {
        // ================================================================================================ Public methods
        /// <summary>
        /// Gets Binary of a Content Repository File in a string.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMarkupString(string filePath)
        {
            //string markupStr = null;
            //using (var stream = SenseNet.Portal.Virtualization.RepositoryPathProvider.OpenFile(filePath))
            //{
            //    using (var reader = new StreamReader(stream))
            //    {
            //        markupStr = reader.ReadToEnd();
            //    }
            //}
            //return markupStr;

            // load from repository to use nodecache
            var file = Node.Load<SenseNet.ContentRepository.File>(filePath);
            if (file == null)
                return null;

            string markupStr = null;
            using (var stream = file.Binary.GetStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    markupStr = reader.ReadToEnd();
                }
            }
            return markupStr;
        }
        public static string GetBigPostMarkupStr()
        {
            return GetMarkupString("/Root/Global/renderers/Wall/Post.html");
        }
        public static string GetSmallPostMarkupStr()
        {
            return GetMarkupString("/Root/Global/renderers/Wall/SmallPost.html");
        }
        public static string GetContentWallMarkupStr()
        {
            return GetMarkupString("/Root/Global/renderers/Wall/ContentWall.html");
        }
        public static string GetCommentMarkupStr()
        {
            return GetMarkupString("/Root/Global/renderers/Wall/Comment.html");
        }
        public static string GetCommentSectionMarkupStr()
        {
            return GetMarkupString("/Root/Global/renderers/Wall/CommentSection.html");
        }

        /// <summary>
        /// Gets markup for a new post, when there are no comments and likes yet.
        /// </summary>
        /// <returns></returns>
        public static string GetPostMarkup(PostInfo postInfo, string contextPath)
        {
            return WallHelper.GetPostMarkup(postInfo, contextPath, string.Empty, string.Empty, 0, new LikeInfo(), false);
        }
        /// <summary>
        /// Gets markup for a Post.
        /// </summary>
        /// <returns></returns>
        public static string GetPostMarkup(PostInfo postInfo, string contextPath, string hiddenCommentsMarkup, string commentsMarkup, int commentCount, LikeInfo likeInfo, bool drawBoundary)
        {
            var markupStr = postInfo.Type == PostType.BigPost ? GetBigPostMarkupStr() : GetSmallPostMarkupStr();
            var commentSectionStr = GetCommentSectionMarkupStr();
            return GetPostMarkup(markupStr, commentSectionStr, postInfo, contextPath, hiddenCommentsMarkup, commentsMarkup, commentCount, likeInfo, drawBoundary);
        }
        public static string GetPostMarkup(string markupStr, string commentSectionStr, PostInfo postInfo, string contextPath, string hiddenCommentsMarkup, string commentsMarkup, int commentCount, LikeInfo likeInfo, bool drawBoundary)
        {
            if (markupStr == null)
                return null;

            if (commentSectionStr == null)
                return null;

            markupStr = markupStr.Replace("{{commentsection}}", commentSectionStr);
            markupStr = markupStr.Replace("{{postid}}", postInfo.ClientId.ToString());
            markupStr = markupStr.Replace("{{avatar}}", UITools.GetAvatarUrl(postInfo.CreatedBy));
            markupStr = markupStr.Replace("{{username}}", postInfo.CreatedBy.Name);
            markupStr = markupStr.Replace("{{userlink}}", Actions.ActionUrl(Content.Create(postInfo.CreatedBy), "Profile"));

            var text = postInfo.Text;
            if (text != null)
                text = text.Replace("{{path}}", postInfo.LastPath ?? string.Empty);

            var haspermission = WallHelper.HasWallPermission(contextPath);

            markupStr = markupStr.Replace("{{text}}", text);
            markupStr = markupStr.Replace("{{date}}", postInfo.CreationDate.ToString());
            markupStr = markupStr.Replace("{{friendlydate}}", UITools.GetFriendlyDate(postInfo.CreationDate));
            markupStr = markupStr.Replace("{{hiddencomments}}", hiddenCommentsMarkup);
            markupStr = markupStr.Replace("{{comments}}", commentsMarkup);
            markupStr = markupStr.Replace("{{commentboxdisplay}}", (commentCount > 0) && haspermission ? "block" : "none");
            markupStr = markupStr.Replace("{{hiddencommentboxdisplay}}", commentCount > 2 ? "block" : "none");
            markupStr = markupStr.Replace("{{commentcount}}", commentCount.ToString());
            markupStr = markupStr.Replace("{{likeboxdisplay}}", likeInfo.Count > 0 ? "block" : "none");
            markupStr = markupStr.Replace("{{likes}}", likeInfo.GetLongMarkup());
            markupStr = markupStr.Replace("{{ilikedisplay}}", !likeInfo.iLike ? "inline" : "none");
            markupStr = markupStr.Replace("{{iunlikedisplay}}", likeInfo.iLike ? "inline" : "none");

            // content card - only manualposts count here, journals don't have this markup
            if (postInfo.Type == PostType.BigPost && postInfo.SharedContent != null)
                markupStr = markupStr.Replace("{{contentcard}}", WallHelper.GetContentCardMarkup(postInfo.SharedContent, contextPath));
            else
                markupStr = markupStr.Replace("{{contentcard}}", string.Empty);

            // small post icon
            var smallposticon = "/Root/Global/images/icons/16/add.png";
            if (postInfo.Type == PostType.JournalModified)
                smallposticon = "/Root/Global/images/icons/16/edit.png";
            if (postInfo.Type == PostType.JournalDeletedPhysically)
                smallposticon = "/Root/Global/images/icons/16/delete.png";
            if (postInfo.Type == PostType.JournalMoved)
                smallposticon = "/Root/Global/images/icons/16/move.png";
            if (postInfo.Type == PostType.JournalCopied)
                smallposticon = "/Root/Global/images/icons/16/copy.png";
            markupStr = markupStr.Replace("{{smallposticon}}", smallposticon);

            markupStr = markupStr.Replace("{{postboundaryclass}}", drawBoundary ? "sn-post-boundary" : string.Empty);

            markupStr = markupStr.Replace("{{action}}", postInfo.Action);

            // small post details
            markupStr = markupStr.Replace("{{detailsdisplay}}", string.IsNullOrEmpty(postInfo.Details) ? "none" : "inline");
            markupStr = markupStr.Replace("{{detailssection}}", postInfo.Details);

            // user interaction allowed
            markupStr = markupStr.Replace("{{interactdisplay}}", haspermission ? "inline" : "none");

            return markupStr;
        }
        public static string GetWallPostsMarkup(string contextPath, List<PostInfo> posts)
        {
            if (posts.Count == 0)
                return string.Empty;

            // create query for comments and likes
            var csb = new StringBuilder();
            foreach (var postInfo in posts)
            {
                if (postInfo.IsJournal)
                    continue;

                csb.Append("\"" + postInfo.Path + "\" ");
            }

            var paths = csb.ToString().Trim();

            List<Node> allComments;
            List<Node> allLikes;

            if (string.IsNullOrEmpty(paths))    // only non-persisted journal posts are there to show (no comments or likes)
            {
                allComments = new List<Node>();
                allLikes = new List<Node>();
            } 
            else 
            {
                var commentsAndLikesQuery = "+TypeIs:(Comment Like) +InTree:(" + paths + ")";
                var settings = new QuerySettings() { EnableAutofilters = false };
                var allCommentsAndLikes = ContentQuery.Query(commentsAndLikesQuery, settings).Nodes.ToList();

                var commentNodeTypeId = NodeType.GetByName("Comment").Id;
                var likeTypeId = NodeType.GetByName("Like").Id;

                allComments = allCommentsAndLikes.Where(c => c.NodeTypeId == commentNodeTypeId).ToList();
                allLikes = allCommentsAndLikes.Where(l => l.NodeTypeId == likeTypeId).ToList();
            }

            var bigPostMarkupStr = GetBigPostMarkupStr();
            var smallPostMarkupStr = GetSmallPostMarkupStr();
            var commentMarkupStr = GetCommentMarkupStr();
            var commentSectionStr = GetCommentSectionMarkupStr();

            PostInfo prevPost = null;
            var sb = new StringBuilder();
            foreach (var postInfo in posts)
            {
                // get comments and likes for post
                CommentInfo commentInfo;
                LikeInfo likeInfo;

                if (postInfo.IsJournal)
                {
                    commentInfo = new CommentInfo();
                    likeInfo = new LikeInfo();
                }
                else
                {
                    var commentsForPost = allComments.Where(c => RepositoryPath.GetParentPath(RepositoryPath.GetParentPath(c.Path)) == postInfo.Path).ToList();
                    var likesForPostAndComments = allLikes.Where(l => l.Path.StartsWith(postInfo.Path)).ToList();
                    var likesForPost = likesForPostAndComments.Where(l => RepositoryPath.GetParentPath(RepositoryPath.GetParentPath(l.Path)) == postInfo.Path).ToList();

                    commentInfo = new CommentInfo(commentsForPost, likesForPostAndComments, commentMarkupStr);
                    likeInfo = new LikeInfo(likesForPost, postInfo.Id);
                }

                var drawBoundary = (prevPost != null) && (prevPost.Type != PostType.BigPost) && (postInfo.Type == PostType.BigPost);

                var markup = WallHelper.GetPostMarkup(
                    postInfo.Type == PostType.BigPost ? bigPostMarkupStr : smallPostMarkupStr,
                    commentSectionStr,
                    postInfo,
                    contextPath,
                    commentInfo.HiddenCommentsMarkup,
                    commentInfo.CommentsMarkup,
                    commentInfo.CommentCount,
                    likeInfo, drawBoundary);

                prevPost = postInfo;

                sb.Append(markup);
            }
            return sb.ToString();
        }
        public static string GetContentCardMarkup(Node sharedContent, string contextPath)
        {
            var markupStr = WallHelper.GetMarkupString("/Root/Global/renderers/Wall/ContentCard.html");
            if (markupStr == null)
                return null;

            var sharedGc = sharedContent as GenericContent;
            markupStr = markupStr.Replace("{{shareicon}}", IconHelper.ResolveIconPath(sharedGc.Icon, 32));
            markupStr = markupStr.Replace("{{sharedisplayname}}", sharedGc.DisplayName);
            markupStr = markupStr.Replace("{{sharecontenttype}}", sharedGc.NodeType.Name);

            var user = sharedContent as User;
            if (user == null)
            {
                markupStr = markupStr.Replace("{{sharepath}}", sharedGc.Path);

                var wsRelPath = sharedGc.Path;
                if (sharedGc.Path.StartsWith(contextPath) && sharedGc.Path != contextPath)
                    wsRelPath = sharedGc.Path.Substring(contextPath.Length);

                markupStr = markupStr.Replace("{{shareworkspacerelativepath}}", wsRelPath);
            }
            else
            {
                var path = Actions.ActionUrl(Content.Create(user), "Profile");

                markupStr = markupStr.Replace("{{sharepath}}", path);
                markupStr = markupStr.Replace("{{shareworkspacerelativepath}}", path);
            }

            return markupStr;
        }
        /// <summary>
        /// Gets markup for a Comment control
        /// </summary>
        /// <returns></returns>
        public static string GetCommentControlMarkup(Node contextNode, string hiddenCommentsMarkup, string commentsMarkup, int commentCount, LikeInfo likeInfo)
        {
            var markupStr = WallHelper.GetMarkupString("/Root/Global/renderers/Wall/CommentControl.html");
            if (markupStr == null)
                return null;

            markupStr = markupStr.Replace("{{postid}}", contextNode.Id.ToString());
            markupStr = markupStr.Replace("{{hiddencomments}}", hiddenCommentsMarkup);
            markupStr = markupStr.Replace("{{comments}}", commentsMarkup);
            markupStr = markupStr.Replace("{{hiddencommentboxdisplay}}", commentCount > 2 ? "block" : "none");
            markupStr = markupStr.Replace("{{commentcount}}", commentCount.ToString());
            markupStr = markupStr.Replace("{{likeboxdisplay}}", likeInfo.Count > 0 ? "block" : "none");
            markupStr = markupStr.Replace("{{likes}}", likeInfo.GetLongMarkup());
            markupStr = markupStr.Replace("{{ilikedisplay}}", !likeInfo.iLike ? "inline" : "none");
            markupStr = markupStr.Replace("{{iunlikedisplay}}", likeInfo.iLike ? "inline" : "none");

            // user interaction allowed
            markupStr = markupStr.Replace("{{interactdisplay}}", WallHelper.HasWallPermission(contextNode.Path, contextNode) ? "block" : "none");

            return markupStr;
        }
        /// <summary>
        /// Gets markup for a Comment control
        /// </summary>
        /// <returns></returns>
        public static string GetCommentControlMarkup(Node contextNode, out int commentCount, out int likeCount)
        {
            // get comments for this content
            var contentCommentInfo = new CommentInfo(contextNode.Id);

            // get likes for this content
            var contentLikeInfo = new LikeInfo(contextNode.Id);

            var markupStr = WallHelper.GetCommentControlMarkup(
                contextNode,
                contentCommentInfo.HiddenCommentsMarkup,
                contentCommentInfo.CommentsMarkup,
                contentCommentInfo.CommentCount,
                contentLikeInfo);

            commentCount = contentCommentInfo.CommentCount;
            likeCount = contentLikeInfo.Count;

            return markupStr;
        }
        /// <summary>
        /// Gets markup for a Content Wall
        /// </summary>
        /// <param name="contextId"></param>
        /// <param name="hiddenCommentsMarkup"></param>
        /// <param name="commentsMarkup"></param>
        /// <param name="commentCount"></param>
        /// <returns></returns>
        public static string GetContentWallMarkup(Node contextNode, string hiddenCommentsMarkup, string commentsMarkup, int commentCount, LikeInfo likeInfo, string postsMarkup)
        {
            var markupStr = GetContentWallMarkupStr();
            return GetContentWallMarkup(markupStr, contextNode, hiddenCommentsMarkup, commentsMarkup, commentCount, likeInfo, postsMarkup);
        }
        public static string GetContentWallMarkup(string markupStr, Node contextNode, string hiddenCommentsMarkup, string commentsMarkup, int commentCount, LikeInfo likeInfo, string postsMarkup)
        {
            if (markupStr == null)
                return null;

            markupStr = markupStr.Replace("{{postid}}", contextNode.Id.ToString());
            markupStr = markupStr.Replace("{{hiddencomments}}", hiddenCommentsMarkup);
            markupStr = markupStr.Replace("{{comments}}", commentsMarkup);
            markupStr = markupStr.Replace("{{hiddencommentboxdisplay}}", commentCount > 2 ? "block" : "none");
            markupStr = markupStr.Replace("{{commentcount}}", commentCount.ToString());
            markupStr = markupStr.Replace("{{likeboxdisplay}}", likeInfo.Count > 0 ? "block" : "none");
            markupStr = markupStr.Replace("{{likes}}", likeInfo.GetLongMarkup());
            markupStr = markupStr.Replace("{{ilikedisplay}}", !likeInfo.iLike ? "inline" : "none");
            markupStr = markupStr.Replace("{{iunlikedisplay}}", likeInfo.iLike ? "inline" : "none");

            var contextGc = contextNode as GenericContent;
            markupStr = markupStr.Replace("{{shareicon}}", IconHelper.ResolveIconPath(contextGc.Icon, 32));
            markupStr = markupStr.Replace("{{sharedisplayname}}", contextGc.DisplayName);
            markupStr = markupStr.Replace("{{sharecontenttype}}", contextGc.NodeType.Name);
            markupStr = markupStr.Replace("{{sharepath}}", contextGc.Path);

            var ws = Workspace.GetWorkspaceWithWallForNode(contextNode);
            if (ws == null)
                ws = Workspace.GetWorkspaceForNode(contextNode);

            if (ws != null)
            {
                markupStr = markupStr.Replace("{{sharetargetdefaultpath}}", ws.Path);
                markupStr = markupStr.Replace("{{sharetargetdefaultname}}", ws.DisplayName);
                markupStr = markupStr.Replace("{{workspacepath}}", ws.Path);
                markupStr = markupStr.Replace("{{workspacename}}", ws.DisplayName);
            }
            else
            {
                markupStr = markupStr.Replace("{{sharetargetdefaultpath}}", string.Empty);
                markupStr = markupStr.Replace("{{sharetargetdefaultname}}", string.Empty);
            }

            // always include profile link - it will be created if not yet exists
            var currentUser = User.Current as User;
            markupStr = markupStr.Replace("{{mywallpath}}", Actions.ActionUrl(Content.Create(currentUser), "Profile"));
            markupStr = markupStr.Replace("{{mywallname}}", "My wall");
            markupStr = markupStr.Replace("{{mywalldisplay}}", "inline");

            markupStr = markupStr.Replace("{{workspacedisplay}}", ws != null ? "inline" : "none");

            markupStr = markupStr.Replace("{{posts}}", postsMarkup);

            // user interaction allowed
            markupStr = markupStr.Replace("{{interactdisplay}}", WallHelper.HasWallPermission(contextNode.Path, contextNode) ? "block" : "none");

            return markupStr;
        }
        /// <summary>
        /// Gets markup for a comment.
        /// </summary>
        /// <param name="creationDate">Date when the comment was created</param>
        /// <param name="user">User who created the comment.</param>
        /// <param name="text">Text of the comment.</param>
        /// <returns></returns>
        public static string GetCommentMarkup(DateTime creationDate, User user, string text, int commentId, LikeInfo likeInfo, Node commentNode)
        {
            var markupStr = GetCommentMarkupStr();
            return GetCommentMarkup(markupStr, creationDate, user, text, commentId, likeInfo, commentNode);
        }
        public static string GetCommentMarkup(string markupStr, DateTime creationDate, User user, string text, int commentId, LikeInfo likeInfo, Node commentNode)
        {
            if (markupStr == null)
                return null;

            markupStr = markupStr.Replace("{{commentid}}", commentId.ToString());
            markupStr = markupStr.Replace("{{avatar}}", UITools.GetAvatarUrl(user));
            markupStr = markupStr.Replace("{{username}}", user.Name);
            markupStr = markupStr.Replace("{{userlink}}", Actions.ActionUrl(Content.Create(user), "Profile"));
            markupStr = markupStr.Replace("{{text}}", text);
            markupStr = markupStr.Replace("{{date}}", creationDate.ToString());
            markupStr = markupStr.Replace("{{friendlydate}}", UITools.GetFriendlyDate(creationDate));
            markupStr = markupStr.Replace("{{likeboxdisplay}}", likeInfo.Count > 0 ? "inline" : "none");
            markupStr = markupStr.Replace("{{likes}}", likeInfo.GetShortMarkup());
            markupStr = markupStr.Replace("{{ilikedisplay}}", !likeInfo.iLike ? "inline" : "none");
            markupStr = markupStr.Replace("{{iunlikedisplay}}", likeInfo.iLike ? "inline" : "none");

            // user interaction allowed
            var haspermission = WallHelper.HasLikePermission(commentNode);
            markupStr = markupStr.Replace("{{interactdisplay}}", haspermission ? "inline" : "none");
            // show 'like' icon for comment likes if user does not have permission -> in this case like icon would not appear since like link is hidden
            markupStr = markupStr.Replace("{{interactclass}}", haspermission ? string.Empty : "sn-commentlike");
            return markupStr;
        }
        /// <summary>
        /// Gets markup for an item in the list of people who liked an item
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string GetLikeListItemMarkup(User user)
        {
            var markupStr = GetMarkupString("/Root/Global/renderers/Wall/LikeListItem.html");
            if (markupStr == null)
                return null;

            markupStr = markupStr.Replace("{{avatar}}", UITools.GetAvatarUrl(user));
            markupStr = markupStr.Replace("{{username}}", user.Name);
            markupStr = markupStr.Replace("{{userlink}}", Actions.ActionUrl(Content.Create(user), "Profile"));

            return markupStr;
        }

        /// <summary>
        /// Checks if current user can interact with wall. Parameter workspace is optional - if omitted, it will be loaded if Posts folder does not exist.
        /// </summary>
        /// <param name="workspacePath"></param>
        /// <param name="workspace"></param>
        /// <returns></returns>
        public static bool HasWallPermission(string workspacePath, Node workspace)
        {
            // if posts folder exists, we should have addnew rights. if not, it will automatically be created, so let's see if we have add new right to the parent.
            var checkedFolder = Node.LoadNode(RepositoryPath.Combine(workspacePath, "Posts"));
            if (checkedFolder == null)
                checkedFolder = workspace == null ? Node.LoadNode(workspacePath) : workspace;

            return checkedFolder.Security.HasPermission(SenseNet.ContentRepository.Storage.Schema.PermissionType.AddNew) && SenseNet.Portal.Wall.WallController.HasPermission();
        }
        public static bool HasWallPermission(string workspacePath)
        {
            return HasWallPermission(workspacePath, null);
        }
        public static bool HasLikePermission(Node commentNode)
        {
            return commentNode.Security.HasPermission(SenseNet.ContentRepository.Storage.Schema.PermissionType.AddNew) && SenseNet.Portal.Wall.WallController.HasPermission();
        }
        [Obsolete("Use UITools.GetFriendlyDate instead")]
        public static string GetFriendlyDate(DateTime date)
        {
            return UITools.GetFriendlyDate(date);
        }
    }
}
