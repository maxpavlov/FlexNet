using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;

namespace SenseNet.Portal.Wall
{
    public class CommentInfo
    {
        // =============================================================================== Properties
        public string CommentsMarkup { get; private set; }
        public string HiddenCommentsMarkup { get; private set; }
        public int CommentCount { get; private set; }


        // =============================================================================== Constructors
        public CommentInfo(int itemId)
        {
            var comments = new StringBuilder();
            var hiddenComments = new StringBuilder();
            var commentsResult = DataLayer.GetComments(itemId);
            if (commentsResult == null)
                return;

            var index = 0;
            foreach (var comment in commentsResult.Nodes)
            {
                var commentGc = comment as GenericContent;
                var commentLikeInfo = new LikeInfo(commentGc.Id);
                var commentMarkup = WallHelper.GetCommentMarkup(commentGc.CreationDate, commentGc.CreatedBy as User, commentGc.Description, commentGc.Id, commentLikeInfo, comment);

                // if it is one of the last two comments, add it to visible comments list, otherwise to hidden comments list
                if (index < commentsResult.Count - 2)
                    hiddenComments.Append(commentMarkup);
                else
                    comments.Append(commentMarkup);
                index++;
            }

            CommentsMarkup = comments.ToString();
            HiddenCommentsMarkup = hiddenComments.ToString();
            CommentCount = commentsResult.Count;
        }
        public static int GetCommentCount(int parentId)
        {
            var parent = NodeHead.Get(parentId);
            if (parent == null)
                return -1;

            var commentFolderPath = RepositoryPath.Combine(parent.Path, "Comments");
            var queryText = string.Format("+InFolder:\"{0}\" +Type:Comment .COUNTONLY", commentFolderPath);
            var settings = new QuerySettings { EnableAutofilters = false };
            var result = ContentQuery.Query(queryText, settings);
            return result.Count;
        }
    }
}
