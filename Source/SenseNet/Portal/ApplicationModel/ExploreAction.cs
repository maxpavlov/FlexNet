using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository;

namespace SenseNet.ApplicationModel
{
    /// <summary>
    /// Generates a URL for the given content in Content Explorer
    /// </summary>
    public class ExploreAction : PortalAction
    {
        public ExploreAction(Content context)
        {
            //this.context = context;
            this.Content = context;
            this.Icon = "Browse";
            this.Text = SenseNet.ContentRepository.i18n.SenseNetResourceManager.Current.GetString("Portal", "OpenInContentExplorerActionText");
            this.IncludeBackUrl = false;
        }
        public override string Uri
        {
            get { return String.Concat("/Explore.html#" + this.Content.Path); }
        }

        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            if (!SecurityHandler.HasPermission(NodeHead.Get("/Root/System/WebRoot/Explore.html"), PermissionType.Open))
                this.Forbidden = true;
        }
    }
}
