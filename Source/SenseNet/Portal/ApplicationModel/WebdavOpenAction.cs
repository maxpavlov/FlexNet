using System;
using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class WebdavOpenAction : ClientAction
    {
        public override string MethodName
        {
            get
            {
                return "SN.WebDav.OpenDocument";
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

            if (!Repository.WebdavEditExtensions.Any(extension => context.Name.EndsWith(extension)))
                this.Visible = false;
        }
    }
}
