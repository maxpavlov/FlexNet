using System;
using System.Web;
using System.Xml.Serialization;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    [Serializable]
    public class PortletState
    {
        //TODO:wrap it
        [XmlIgnore] [NonSerialized] public PortletBase Portlet;

        public PortletState()
        {
            throw new NotImplementedException();
        }

        public PortletState(PortletBase portlet)
        {
            this.Portlet = portlet;
        }

        public virtual void CollectValues()
        {
        }

        public static void Persist(PortletState state)
        {
            var requestNodePath = PortalContext.Current.ContextNodePath;
            if (requestNodePath == null)
            {
                var cbPortlet = state.Portlet as ContextBoundPortlet;
                if (cbPortlet != null)
                {
                    var node = cbPortlet.ContextNode;
                    if (node != null)
                        requestNodePath = node.Path;
                }
            }
            HttpContext.Current.Session[Math.Abs((requestNodePath + state.Portlet.ClientID).GetHashCode()).ToString()] = state;
        }

        public static bool Restore(PortletBase portlet, out PortletState state)
        {
            var requestNodePath = PortalContext.Current.ContextNodePath;
            if (requestNodePath == null)
            {
                var cbPortlet = portlet as ContextBoundPortlet;
                if (cbPortlet != null)
                {
                    var node = cbPortlet.ContextNode;
                    if (node != null)
                        requestNodePath = node.Path;
                }
            }
            state = HttpContext.Current.Session[Math.Abs((requestNodePath + portlet.ClientID).GetHashCode()).ToString()] as PortletState;
            if (state != null)
                state.Portlet = portlet;
            return state != null;
        }
    }
}
