using System.Web.UI.WebControls.WebParts;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class SNWebPartPersonalization : WebPartPersonalization
    {
        public SNWebPartPersonalization(WebPartManager parent) : base(parent) { }

        // force PersonalizatinScope to Shared
        public override PersonalizationScope InitialScope
        {
            get
            {
                return PersonalizationScope.Shared;
            }
        }
    }
}
