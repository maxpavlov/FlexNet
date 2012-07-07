using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository;
using SenseNet.Search;

namespace SenseNet.Portal.Wall
{
    public class WallController : Controller
    {
        //===================================================================== Public methods
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetPosts(string contextPath, int skip, int pageSize, string rnd)
        {
            var posts = DataLayer.GetPostsForWorkspace(contextPath).Skip(skip).Take(pageSize).ToList();
            var postsMarkup = WallHelper.GetWallPostsMarkup(contextPath, posts);
            return Json(postsMarkup, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Creates a manually written Post in the Content Repository and returns Post markup.
        /// </summary>
        /// <param name="contextPath"></param>
        /// <param name="text"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CreatePost(string contextPath, string text, string rnd)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            AssertPermission();

            var post = DataLayer.CreateManualPost(contextPath, text);
            var postInfo = new PostInfo(post);
            var postMarkup = WallHelper.GetPostMarkup(postInfo, contextPath);
            return Json(postMarkup, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Creates a comment for a Post and returns Comment markup.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="text"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult CreateComment(string postId, string contextPath, string text, string rnd)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            AssertPermission();

            var comment = DataLayer.CreateComment(postId, contextPath, text);

            var commentMarkup = WallHelper.GetCommentMarkup(comment.CreationDate, SenseNet.ContentRepository.User.Current as User, text, comment.Id, new LikeInfo(), comment);
            return Json(commentMarkup, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Creates a like for a Post/Comment and returns Like markup.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Like(string itemId, string contextPath, bool fullMarkup, string rnd)
        {
            AssertPermission();

            var parentId = 0;
            DataLayer.CreateLike(itemId, contextPath, out parentId);

            var likeInfo = new LikeInfo(parentId);
            return Json(fullMarkup ? likeInfo.GetLongMarkup() : likeInfo.GetShortMarkup(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Deletes a like for a Post/Comment and returns Like markup.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Unlike(string itemId, bool fullMarkup, string rnd)
        {
            AssertPermission();

            var parentId = 0;
            DataLayer.DeleteLike(itemId, out parentId);

            var likeInfo = new LikeInfo(parentId);
            return Json(fullMarkup ? likeInfo.GetLongMarkup() : likeInfo.GetShortMarkup(), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Returns full markup of like list corresponding to itemId
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="rnd"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetLikeList(string itemId, string rnd)
        {
            if (!HasPermission())
                return Json("Please log in to see who liked this item!", JsonRequestBehavior.AllowGet);

            var id = PostInfo.GetIdFromClientId(itemId);

            // create like markup
            var likeInfo = new LikeInfo(id);
            var likelist = new StringBuilder();
            foreach (var likeitem in likeInfo.LikeUsers)
            {
                var likeuser = likeitem as User;
                likelist.Append(WallHelper.GetLikeListItemMarkup(likeuser));
            }

            return Json(likelist.ToString(), JsonRequestBehavior.AllowGet);
        }
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Share(int itemId, string contextPath, string text, string rnd)
        {
            if (!WallHelper.HasWallPermission(contextPath))
                return Json("403", JsonRequestBehavior.AllowGet);

            AssertPermission();

            try
            {
                DataLayer.CreateSharePost(contextPath, text, itemId);
            }
            catch (SenseNetSecurityException)
            {
                return Json("403", JsonRequestBehavior.AllowGet);
            }

            return null;
        }


        //===================================================================== Helper methods
        private static readonly string PlaceholderPath = "/Root/System/PermissionPlaceholders/Wall-mvc";

        private static void AssertPermission()
        {
            if (!HasPermission())
                throw new SenseNetSecurityException("Access denied for " + PlaceholderPath);
        }

        public static bool HasPermission()
        {
            var permissionContent = Node.LoadNode(PlaceholderPath);
            var nopermission = (permissionContent == null || !permissionContent.Security.HasPermission(PermissionType.RunApplication));
            return !nopermission;
        }
    }
}
