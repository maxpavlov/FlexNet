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
            var markupStr = GetMarkupString(postInfo.Type == PostType.BigPost ? "/Root/Global/renderers/Wall/Post.html" : "/Root/Global/renderers/Wall/SmallPost.html");
            if (markupStr == null)
                return null;

            var commentSectionStr = GetMarkupString("/Root/Global/renderers/Wall/CommentSection.html");
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
            markupStr = markupStr.Replace("{{friendlydate}}", GetFriendlyDate(postInfo.CreationDate));
            markupStr = markupStr.Replace("{{hiddencomments}}", hiddenCommentsMarkup);
            markupStr = markupStr.Replace("{{comments}}", commentsMarkup);
            markupStr = markupStr.Replace("{{commentboxdisplay}}", (commentCount > 0) && haspermission ? "block" : "none");
            markupStr = markupStr.Replace("{{hiddencommentboxdisplay}}", commentCount > 2 ? "block" : "none");
            markupStr = markupStr.Replace("{{commentcount}}", commentCount.ToString());
            markupStr = markupStr.Replace("{{likeboxdisplay}}", likeInfo.Count > 0 ? "block" : "none");
            markupStr = markupStr.Replace("{{likes}}", likeInfo.GetLongMarkup());
            markupStr = markupStr.Replace("{{ilikedisplay}}", !likeInfo.iLike ? "inline" : "none");
            markupStr = markupStr.Replace("{{iunlikedisplay}}", likeInfo.iLike ? "inline" : "none");
            var currentUser = User.Current as User;
            markupStr = markupStr.Replace("{{currentuserlink}}", Actions.ActionUrl(Content.Create(currentUser), "Profile"));
            markupStr = markupStr.Replace("{{currentuseravatar}}", UITools.GetAvatarUrl(currentUser));

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
                //var path = sharedGc.Path;
                //var action = ActionFramework.GetAction("Profile", Content.Create(user), null);
                //path = action.Uri;
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
            var currentUser = User.Current as User;
            markupStr = markupStr.Replace("{{currentuserlink}}", Actions.ActionUrl(Content.Create(currentUser), "Profile"));
            markupStr = markupStr.Replace("{{currentuseravatar}}", UITools.GetAvatarUrl(currentUser));

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
            var markupStr = WallHelper.GetMarkupString("/Root/Global/renderers/Wall/ContentWall.html");
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
            var currentUser = User.Current as User;
            markupStr = markupStr.Replace("{{currentuserlink}}", Actions.ActionUrl(Content.Create(currentUser), "Profile"));
            markupStr = markupStr.Replace("{{currentuseravatar}}", UITools.GetAvatarUrl(currentUser));

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
            var markupStr = GetMarkupString("/Root/Global/renderers/Wall/Comment.html");
            if (markupStr == null)
                return null;

            markupStr = markupStr.Replace("{{commentid}}", commentId.ToString());
            markupStr = markupStr.Replace("{{avatar}}", UITools.GetAvatarUrl(user));
            markupStr = markupStr.Replace("{{username}}", user.Name);
            markupStr = markupStr.Replace("{{userlink}}", Actions.ActionUrl(Content.Create(user), "Profile"));
            markupStr = markupStr.Replace("{{text}}", text);
            markupStr = markupStr.Replace("{{date}}", creationDate.ToString());
            markupStr = markupStr.Replace("{{friendlydate}}", GetFriendlyDate(creationDate));
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
        /// Gets the user friendly string representation of a date relative to the current time
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetFriendlyDate(DateTime date)
        {
            //- 53 seconds ago
            //- 15 minutes ago
            //- 21 hours ago
            //- Yesterday at 3:43pm
            //- Sunday at 2:12pm
            //- May 25 at 1:23pm
            //- December 27, 2010 at 5:41pm

            var shortTime = date.ToShortTimeString();   // 5:41 PM

            var ago = DateTime.Now - date;
            if (ago < new TimeSpan(0, 1, 0))
                return ago.Seconds == 1 ? 
                    "1 second ago" :
                    string.Format("{0} seconds ago", ago.Seconds);
            if (ago < new TimeSpan(1, 0, 0))
                return ago.Minutes == 1 ? 
                    "1 minute ago" :
                    string.Format("{0} minutes ago", ago.Minutes);
            if (ago < new TimeSpan(1, 0, 0, 0))
                return ago.Hours == 1 ?
                    "1 hour ago" : 
                    string.Format("{0} hours ago", ago.Hours);
            if (ago < new TimeSpan(2, 0, 0, 0))
                return string.Format("Yesterday at {0}", shortTime);
            if (ago < new TimeSpan(7, 0, 0, 0))
                return string.Format("{0} at {1}", date.DayOfWeek.ToString(), shortTime);
            if (date.Year == DateTime.Now.Year)
                return string.Format("{0} {1} at {2}", date.ToString("MMMM"), date.Day, shortTime);

            return string.Format("{0} {1}, {2} at {3}", date.ToString("MMMM"), date.Day, date.Year, shortTime);
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
    }
}
