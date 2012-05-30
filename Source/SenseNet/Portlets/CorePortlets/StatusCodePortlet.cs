using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI.PortletFramework;
using System.Web;

namespace SenseNet.Portal.Portlets
{
    /// <summary>
    /// A portlet for setting status code on a page after rendering.
    /// </summary>
    public class StatusCodePortlet : PortletBase
    {
        private int subStatusCode;

        /// <summary>
        /// Gets or sets the sub status code.
        /// </summary>
        /// <value>The sub status code.</value>
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Sub status code")]
        [WebCategory(EditorCategory.Other, EditorCategory.Other_Order)]
        [Editor(typeof(Int32), typeof(IEditorPartField))]
        public int SubStatusCode
        {
            get { return subStatusCode; }
            set { subStatusCode = value; }
        }

        private int statusCode = 200;

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        /// <value>The status code.</value>
        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Status code")]
        [WebCategory(EditorCategory.Other, EditorCategory.Other_Order)]
        [Editor(typeof(Int32), typeof(IEditorPartField))]
        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusCodePortlet"/> class.
        /// </summary>
        public StatusCodePortlet()
        {
            Name = "Status Code";
            Description = "Portlet for setting status code on page after rendering.";
            Category = new PortletCategory(PortletCategoryType.Application);
        }

        /// <summary>
        /// Overrides Render method and sets status code if page is not viewed on localhost.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            base.Render(writer);

            if (WebPartManager.DisplayMode == WebPartManager.BrowseDisplayMode)
            {
                HttpContext.Current.Response.StatusCode = statusCode;
                HttpContext.Current.Response.SubStatusCode = subStatusCode;
            }
        }
    }
}
