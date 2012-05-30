using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.Diagnostics;
using SenseNet.Portal.Portlets.Controls;
using SenseNet.Portal.UI.PortletFramework;

namespace SenseNet.Portal.Portlets
{
    public class BookCalendarPortlet : EventCalendarPortlet
    {

        private string _contentViewPath = "/Root/Skins/book/renderers/BookCalendar.ascx";

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public override string ContentViewPath
        {
            get
            {
                return _contentViewPath;
            }
            set
            {
                _contentViewPath = value;
            }
        }

        // portlet uses custom ascx, hide renderer property
        [WebBrowsable(false), Personalizable(true)]
        public override string Renderer { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Books path")]
        [WebDescription("Path of books to be displayed")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions()]
        [WebOrder(110)]
        public string CalendarPath
        {
            get; set;
        }

        /// <summary>
        /// Initalize the portlet name and description
        /// </summary>
        public BookCalendarPortlet()
        {
            Name = "Book calendar";
            Description = "Portlet for displaying upcoming books in calendar view (context bound)";
            Category = new PortletCategory(PortletCategoryType.Application);
        }

        protected override IEnumerable<Node> GetEvents()
        {
            var query = new NodeQuery(ChainOperator.And);
            query.Add(new StringExpression(StringAttribute.Path, StringOperator.StartsWith, CalendarPath));
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["Book"]));
            var results = query.Execute().Nodes;
            return results;
        }
    }
}
