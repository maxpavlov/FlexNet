using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository;

namespace SenseNet.ApplicationModel
{
    public class AbortWorkflowAction : UrlAction
    {
        public override void Initialize(Content context, string backUri, Application application, object parameters)
        {
            base.Initialize(context, backUri, application, parameters);

            if (context == null)
                return;

            if (!context.ContentType.IsInstaceOfOrDerivedFrom("Workflow"))
                return;

            //hide Abort action if the workflow is not in Running state
            var wfs = context["WorkflowStatus"] as IEnumerable<string>;
            if (wfs == null || wfs.Count() != 1 || wfs.FirstOrDefault() != "1")
                this.Visible = false;
        }
    }
}
