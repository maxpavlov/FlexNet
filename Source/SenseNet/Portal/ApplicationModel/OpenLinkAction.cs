using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.ApplicationModel
{
    public class OpenLinkAction : UrlAction
    {
        public override string Uri
        {
            get
            {
                if (this.Content == null)
                    return base.Uri;

                var link = this.Content["Url"] as string;
                return string.IsNullOrEmpty(link) ? base.Uri : link;
            }
        }
    }
}
