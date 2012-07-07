using System;
using SenseNet.ApplicationModel;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class WebdavBrowseAction : ClientAction
    {
        public override string MethodName
        {
            get
            {
                return "SN.WebDav.BrowseFolder";
            }
            set
            {
                base.MethodName = value;
            }
        }

        public override string ParameterList
        {
            get
            {
                return this.Content == null ? string.Empty : string.Format(@"'{0}'", this.Content.Path);
            }
            set
            {
                base.ParameterList = value;
            }
        }

        public override void Initialize(ContentRepository.Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            //if (string.Compare(PortalContext.Current.AuthenticationMode, "windows", StringComparison.CurrentCultureIgnoreCase) != 0)
            //    this.Forbidden = true;
        }
    }
}
