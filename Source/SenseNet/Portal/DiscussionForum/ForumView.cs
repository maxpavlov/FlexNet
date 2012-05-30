using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository;
using ASP = System.Web.UI.WebControls;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.Portal.DiscussionForum
{
    public class ForumView : UserControl
    {
        private Content _contextElement;
        public Content ContextElement
        {
            get
            {
                if (_contextElement == null)
                {
                    var ci = (SenseNet.Portal.UI.Controls.ContextInfo)FindControl("ViewContext");
                    _contextElement = Content.Load(ci.Path);
                }

                return _contextElement;
            }
        }

        private ASP.Repeater _forumBody;
        public ASP.Repeater ForumBody
        {
            get
            {
                if (_forumBody == null)
                    _forumBody = FindControl("ForumBody") as ASP.Repeater;

                return _forumBody;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ForumBody.ItemDataBound += new System.Web.UI.WebControls.RepeaterItemEventHandler(ForumBody_ItemDataBound);
        }

        void ForumBody_ItemDataBound(object sender, System.Web.UI.WebControls.RepeaterItemEventArgs e)
        {
            var myContent = (Content)e.Item.DataItem;

            var browseLink = e.Item.FindControl("BrowseLink") as ActionLinkButton;
            var numLabel = e.Item.FindControl("PostNum") as ASP.Label;
            var dateLabel = e.Item.FindControl("PostDate") as ASP.Label;

            if (browseLink != null)
            {
                browseLink.ActionName = "Browse";
                browseLink.NodePath = myContent.Path;
                browseLink.Text = myContent.DisplayName;
            }

            if (numLabel != null)
                numLabel.Text = myContent.ContentHandler.PhysicalChildArray.Count().ToString();

            if (dateLabel != null)
            {
                var oldest = myContent.ContentHandler.PhysicalChildArray.OrderBy(n => n.CreationDate).FirstOrDefault();
                if (oldest == null)
                    oldest = myContent.ContentHandler;

                dateLabel.Text = oldest.CreationDate.ToString();
            }
        }
    }
}
