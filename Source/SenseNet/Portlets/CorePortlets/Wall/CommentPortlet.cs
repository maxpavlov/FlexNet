using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Search;
using SenseNet.ContentRepository;
using SenseNet.Portal.Wall;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.Portlets.Wall
{
    public class CommentPortlet : ContextBoundPortlet
    {
        // ================================================================================================ Constructor
        public CommentPortlet()
        {
            this.Name = "Comment";
            this.Description = "This portlet displays a comment section and a simple wall for a specific content (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Enterprise20);
        }


        // ================================================================================================ Methods
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            UITools.AddScript("$skin/scripts/sn/SN.Wall.js");
        }
        protected override void CreateChildControls()
        {
            if (this.ContextNode == null)
                return;

            // gather posts for this content
            PostInfo prevPost = null;
            var postsMarkup = new StringBuilder();
            foreach (var postInfo in DataLayer.GetPostsForContent(this.ContextNode))
            {
                // get comments for current post
                var commentInfo = new CommentInfo(postInfo.Id);

                // get likes for this post
                var likeInfo = new LikeInfo(postInfo.Id);

                var drawBoundary = (prevPost != null) && (prevPost.Type != PostType.BigPost) && (postInfo.Type == PostType.BigPost);

                var markup = WallHelper.GetPostMarkup(
                    postInfo,
                    this.ContextNode.Path,
                    commentInfo.HiddenCommentsMarkup,
                    commentInfo.CommentsMarkup,
                    commentInfo.CommentCount,
                    likeInfo, drawBoundary);

                prevPost = postInfo;
                postsMarkup.Append(markup);
            }



            // get comments for this content
            var contentCommentInfo = new CommentInfo(this.ContextNode.Id);

            // get likes for this content
            var contentLikeInfo = new LikeInfo(this.ContextNode.Id);

            var markupStr = WallHelper.GetContentWallMarkup(
                this.ContextNode, 
                contentCommentInfo.HiddenCommentsMarkup,
                contentCommentInfo.CommentsMarkup,
                contentCommentInfo.CommentCount,
                contentLikeInfo,
                postsMarkup.ToString());

            this.Controls.Add(new Literal { Text = markupStr });

            base.CreateChildControls();
            this.ChildControlsCreated = true;
        }
    }
}
