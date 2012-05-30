using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;
using System.Web;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Portal
{
    public class PortalQueryTemplateReplacer : RepositoryQueryTemplateReplacer
    {
        private static string[] objectNames = new string[] { "currentsite", "currentworkspace", "currentpage", "currentcontent" };

        public override IEnumerable<string> ObjectNames
        {
            get { return objectNames; }
        }

        public override string EvaluateObjectProperty(string objectName, string propertyName)
        {
            if(HttpContext.Current == null || PortalContext.Current == null)
                return base.EvaluateObjectProperty(objectName, propertyName);

            switch (objectName)
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
