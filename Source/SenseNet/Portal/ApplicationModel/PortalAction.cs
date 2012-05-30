using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public abstract class PortalAction : ActionBase
    {
        public virtual string IconTag
        {
            get
            {
                return IconHelper.RenderIconTag(Icon, null);
            }
        }

        public virtual string SiteRelativePath
        {
            get
            {
                return PortalContext.GetSiteRelativePath(Content.Path);
            }
        }
    }
}
