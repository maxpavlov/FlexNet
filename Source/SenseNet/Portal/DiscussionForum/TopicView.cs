using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using SenseNet.Portal.UI.Controls;
using ASP=System.Web.UI.WebControls;

namespace SenseNet.Portal.DiscussionForum
{
    public class TopicView : UserControl
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

        private ASP.Repeater _topicBody;
        public ASP.Repeater TopicBody
        {
            get
            {
                if (_topicBody == null)
                    _topicBody = FindControl("TopicBody") as ASP.Repeater;

                return _topicBody;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            TopicBody.ItemDataBound += new ASP.RepeaterItemEventHandler(TopicBody_ItemDataBound);
        }

        void TopicBody_ItemDataBound(object sender, ASP.RepeaterItemEventArgs e)
        {
            var replyLink = e.Item.FindControl("ReplyLink") as ActionLinkButton;

            if (replyLink == null)
                return;

            replyLink.ActionName = "Add";
            replyLink.NodePath = ContextElement.Path;
            replyLink.Parameters = new { ReplyTo = ((Content)e.Item.DataItem).Path };
        }

    }
}
