using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Reflection;

[assembly: TagPrefix("SenseNet.Portal.UI.PortletFramework", "sn")]
namespace SenseNet.Portal.UI.PortletFramework
{
    public class SNWebPartManager : WebPartManager
    {
        //========================================================================================== Overrides

        protected override void RegisterClientScript()
        {
            // We render our own javascript for drag and drop instead of the old WebPart.js
            // Render the JS only if not in Browse mode
            if (this.DisplayMode != WebPartManager.BrowseDisplayMode)
                UITools.AddScript("/Root/Global/scripts/sn/SN.WebPart.js");
        }

        protected override bool CheckRenderClientScript()
        {
            // The CheckRenderClientScript() method decides whether javascripts are rendered to the client. 
            // In the base version of the WebPartManager it returns true only in IE5.5 and above. 
            // Since we support drag and drop in any modern browser, we have to override this method.  

            var flag = false;
            if (this.EnableClientScript && (this.Page != null))
            {
                var browser = this.Page.Request.Browser;
                if (browser.EcmaScriptVersion.Major > 1)
                {
                    flag = true;
                }
            }

            return flag;
        }
        
        protected override void OnPreRender(EventArgs e)
        {
            //We do not need the OnPreRender base method in our implementation because it would render some js text messages
            //(ExportSensitiveDataWarningDeclaration, CloseProviderWarningDeclaration, DeleteWarningDeclaration).
            //This reduces the size of the rendered html.

            if (!this.CheckRenderClientScript()) 
                return;

            //helper call for a postback event to force the clientscriptmanager to call internal Page.RegisterPostBackScript
            this.Page.ClientScript.GetPostBackEventReference(this, string.Empty);

            this.RegisterClientScript();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            //We override the Render method to get rid of the old Drag div of the WebPartManager, since it is not needed any more.
        }

        /// <summary>
        /// Create custom personalization implementation
        /// </summary>
        /// <returns>SNWebPartPersonalization object</returns>
        protected override WebPartPersonalization CreatePersonalization()
        {
            return new SNWebPartPersonalization(this);
        }

        protected override string CreateDynamicWebPartID(Type webPartType)
        {
            return CreateDynamicPortletID(webPartType.Name);
        }

        //========================================================================================== Public methods

        public void SetDirty()
        {
            SetPersonalizationDirty();
        }

        //========================================================================================== Static methods

        protected static string CreateDynamicPortletID(string portletTypeName)
        {
            var newWebPartIDSecondPart = Math.Abs(Guid.NewGuid().GetHashCode()).ToString(CultureInfo.InvariantCulture);
            return String.Concat(portletTypeName, newWebPartIDSecondPart);
        }
    }
}
