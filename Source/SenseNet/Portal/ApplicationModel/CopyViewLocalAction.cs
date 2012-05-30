using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SenseNet.ContentRepository;

namespace SenseNet.ApplicationModel
{
    public class CopyViewLocalAction : ServiceAction
    {
        public override string ServiceName
        {
            get
            {
                return "ContentListViewHelper.mvc";
            }
            set
            {
                base.ServiceName = value;
            }
        }

        public override string MethodName
        {
            get
            {
                return "CopyViewLocal";
            }
            set
            {
                base.MethodName = value;
            }
        }

        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            AddParameter("viewpath", HttpUtility.UrlEncode(context.Path));

            //if the list view is under a content list, it could not be copied to the 
            //local views folder - only global views can be made local
            //var cl = ContentList.GetContentListByParentWalk(context.ContentHandler);
            //if (cl != null)
            //    this.Forbidden = true;
        }
    }
}
