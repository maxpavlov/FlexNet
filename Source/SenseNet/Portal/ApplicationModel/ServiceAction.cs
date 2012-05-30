using System.Web;
using SenseNet.Portal.Virtualization;

namespace SenseNet.ApplicationModel
{
    public class ServiceAction : PortalAction
    {
        public virtual string ServiceName { get; set; }

        public virtual string MethodName { get; set; }

        public override string Uri
        {
            get
            {
                if (this.Forbidden)
                    return string.Empty;

                var s = SerializeParameters(GetParameteres());

                return string.Format("/{0}/{1}?{2}={3}{4}",
                    ServiceName,
                    MethodName,
                    PortalContext.BackUrlParamName,
                    HttpUtility.UrlEncode(this.BackUri),
                    s);
            }
        }
    }
}
