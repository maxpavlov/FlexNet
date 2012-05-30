using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class PurgeFromProxyAction : UrlAction
    {
        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            if (context == null)
                return;

            //if there are no proxy servers defined, make this action forbidden
            if (PortalContext.ProxyIPs.Count == 0)
                this.Forbidden = true;
        }
    }
}
