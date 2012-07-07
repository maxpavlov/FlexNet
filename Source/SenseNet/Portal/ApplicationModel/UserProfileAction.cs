using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class UserProfileAction : ServiceAction
    {
        public override string ServiceName
        {
            get
            {
                return "Workspace.mvc";
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
                return "BrowseProfile";
            }
            set
            {
                base.MethodName = value;
            }
        }

        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            AddParameter("path", HttpUtility.UrlEncode(context.Path));
        }
    }
}
