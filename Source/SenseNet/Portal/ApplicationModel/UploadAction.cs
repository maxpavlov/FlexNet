using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class UploadAction : UrlAction
    {
        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            if(context == null)
                return;

            if(!(context.ContentHandler is Folder))
                return;
            
            var gc = context.ContentHandler as GenericContent;
            var contentTypes = gc.GetAllowedChildTypes().ToArray();
            
            //if there is no contenttype restriction on this content, we will show and enable the upload action only for administrators
            if (contentTypes.Length == 0)
            {
                if (!PortalContext.Current.ArbitraryContentTypeCreationAllowed)
                    this.Forbidden = true;

                return;
            }

            //if there is a contenttype restriction on this content, we will enable the upload action only if the file contenttype or one of its derived type can be found in the contenttypes list
            if (contentTypes.Count(ct => ct.IsInstaceOfOrDerivedFrom("File")) == 0)
                this.Forbidden = true;
        }
    }
}
