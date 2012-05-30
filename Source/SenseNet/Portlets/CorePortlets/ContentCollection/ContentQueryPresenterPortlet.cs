using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls;
using repository = SenseNet.ContentRepository;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.ComponentModel;

namespace SenseNet.Portal.Portlets
{
    public class ContentQueryPresenterPortlet : ContentCollectionPortlet
    {
        public const string ContentListID = "ContentList";

        private string _query;

        [WebDisplayName("Content query")]
        [WebDescription("Query defining the set of contents to be presented using Content Query Language")]
        [WebBrowsable(true), Personalizable(true)]
        [WebCategory(EditorCategory.Query, EditorCategory.Query_Order), WebOrder(10)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string QueryString
        {
            get { return _query; }
            set { _query = value; }
        }


        public static class DataBindingHelper
        {
            //private static Dictionary<Type, Action<object, object>> map;

            //static DataBindingHelper()
            //{
            //    map = new Dictionary<Type, Action<object, object>>();
            //    map.Add(
            //        typeof(DataBoundControl),
            //            (control, data) =>
            //            {
            //                ((DataBoundControl)control).DataSource = data;
            //                ((DataBoundControl)control).DataBind();
            //            });
            //    map.Add(typeof(BaseDataList),
            //            (control, data) =>
            //            {
            //                ((BaseDataList)control).DataSource = data;
            //                ((BaseDataList)control).DataBind();
            //            });

            //}
            public static void SetDataSourceAndBind(object control, object dataSource)
            {
                if (control is DataBoundControl)
                {
                    DataBoundControl bindable = (DataBoundControl)control;
                    bindable.DataSource = dataSource;
                    bindable.DataBind();
                }
                else if (control is BaseDataList)
                {
                    BaseDataList bindable = (BaseDataList)control;
                    bindable.DataSource = dataSource;
                    bindable.DataBind();
                }
                else if (control is Repeater)
                {
                    Repeater bindable = (Repeater)control;
                    bindable.DataSource = dataSource;
                    bindable.DataBind();
                }
                else
                    throw new NotSupportedException(control.GetType().Name + " is not a supported data control");

                //foreach (var type in map.Keys)
                //{
                //    if (type.IsAssignableFrom(control.GetType()))
                //    {
                //        map[type](control, dataSource);
                //        return;
                //    }
                //}
                //throw new NotSupportedException(control.GetType().Name + " is not a supported data control");
            }

        }

        protected override void RenderWithAscx(System.Web.UI.HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }

        public ContentQueryPresenterPortlet()
        {
            this.Name = "Content query presenter";
            this.Description = "Display list of Contents based on flexible queries against the Sense/Net Content Repository (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Collection);
            this.HiddenPropertyCategories = new List<string>() { EditorCategory.Collection };
        }


        private bool GetIntFromRequest(string variable, out int value)
        {
            value = 0;
            if (Page.Request.Params.AllKeys.Contains(variable))
            {
                var paramValue = Page.Request.Params[variable];
                if (string.IsNullOrEmpty(paramValue))
                    return false;
                return int.TryParse(paramValue,  out value);
            }
            return false;
        }

        protected override object GetModel()
        {

            SmartFolder sf = null;
            sf = Node.Load<SmartFolder>("/Root/System/RuntimeQuery");
            if (sf == null)
            {
                using(new SystemAccount())
                {
                    var systemFolder = Node.LoadNode("/root/system");
                    sf = new SmartFolder(systemFolder) { Name = "RuntimeQuery" };
                    sf.Save();
                }
            }

            var c = ContentRepository.Content.Create(sf);
            sf.Query = ReplaceTemplates(this.QueryString);

            var oldc = base.GetModel() as ContentRepository.Content;
            if (oldc != null)
            {
                c.ChildrenQueryFilter = oldc.ChildrenQueryFilter;
                c.ChildrenQuerySettings = oldc.ChildrenQuerySettings;
            }

            return c;
        }
    }
}
