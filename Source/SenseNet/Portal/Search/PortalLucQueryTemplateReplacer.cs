using System.Collections.Generic;
using System.Web;
using SenseNet.Portal.Virtualization;
using SenseNet.Search;
using SenseNet.Search.Parser;

namespace SenseNet.Portal.Search
{
    public class PortalLucQueryTemplateReplacer : RepositoryLucQueryTemplateReplacer
    {
        private static readonly string[] objectNames = new[] { "currentsite", "currentworkspace", 
            "currentpage", "currentcontent" };

        public override IEnumerable<string> ObjectNames
        {
            get { return objectNames; }
        }

        public override string EvaluateObjectProperty(string objectName, string propertyName)
        {
            if (HttpContext.Current == null || PortalContext.Current == null)
                return base.EvaluateObjectProperty(objectName, propertyName);

            switch (objectName.ToLower())
            {
                case "currentsite":
                    return GetProperty(PortalContext.Current.Site, propertyName);
                case "currentworkspace":
                    return GetProperty(PortalContext.Current.ContextWorkspace, propertyName);
                case "currentpage":
                    return GetProperty(PortalContext.Current.Page, propertyName);
                case "currentcontent":
                    return GetProperty(PortalContext.Current.ContextNode, propertyName);
                default:
                    return base.EvaluateObjectProperty(objectName, propertyName);
            }
        }
    }
}
