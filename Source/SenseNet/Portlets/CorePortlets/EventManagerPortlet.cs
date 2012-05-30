using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.i18n;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Portal.Portlets
{
    public class EventManagerPortlet : ContentCollectionPortlet
    {
        private Node regFormNode;

        private bool regFormLoaded;

        private Node _contextNode;
        public new Node ContextNode
        {
            get { return _contextNode; }
            set { _contextNode = value; }
        }

        public EventManagerPortlet()
        {
            this.Name = SenseNetResourceManager.Current.GetString("EventManager", "PortletTitle");
            this.Description = SenseNetResourceManager.Current.GetString("EventManager", "PortletDescription");
            this.Category = new PortletCategory(PortletCategoryType.Portal);

            Cacheable = false;   // by default, caching is switched off


            regFormLoaded = false;

            BindTarget = BindTarget.CurrentContent;

            _contextNode = GetContextNode();

            try
            {
                regFormNode = ContextNode.GetReference<Node>("RegistrationForm");
            }
            catch
            {
                regFormNode = null;
            }

            if(regFormNode != null) regFormLoaded = true;
        }

        protected override object GetModel()
        {
            return base.GetModel();
        }

        protected override Node GetContextNode()
        {
            return !regFormLoaded ? base.GetContextNode() : regFormNode;
        }
    }
}
