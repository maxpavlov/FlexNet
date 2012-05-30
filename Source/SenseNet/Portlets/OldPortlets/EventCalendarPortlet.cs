using System;
using System.Collections.Generic;
using System.ComponentModel;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.Portlets.Controls;
using SenseNet.Diagnostics;
using System.Web.UI;
using SenseNet.ContentRepository.Storage;
using SenseNet.Search;

namespace SenseNet.Portal.Portlets
{
    public class EventCalendarPortlet : ContextBoundPortlet
    {
        private string _contentViewPath = "/Root/System/SystemPlugins/Portlets/EventCalendar/EventCalendar.ascx";

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public virtual string ContentViewPath
        {
            get { return _contentViewPath; }
            set { _contentViewPath = value; }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        /// <summary>
        /// Initalize the portlet name and description
        /// </summary>
        public EventCalendarPortlet()
        {
            Name = "Event calendar";
            Description = "Portlet for handling events in calendar view (context bound)";
            Category = new PortletCategory(PortletCategoryType.Application);
        }

        private void CreateControls()
        {
            try
            {
                var viewControl = Page.LoadControl(ContentViewPath) as EventCalendar;
                if (viewControl != null)
                {
                    viewControl.CalendarEvents = GetEvents();
                    Controls.Add(viewControl);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                Controls.Clear();
                Controls.Add(new LiteralControl("ContentView error: " + ex.Message));
            }
        }

        protected virtual IEnumerable<Node> GetEvents()
        {
            return ContextNode == null ? new List<Node>() : ContentQuery.Query(string.Format("+Type:calendarevent +InTree:\"{0}\"", ContextNode.Path)).Nodes;
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            CreateControls();
        }
    }
}
