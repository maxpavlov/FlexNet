using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI.PortletFramework;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.Portlets.Controls;
using SenseNet.Diagnostics;
using System.Web.UI;
using System.Collections.Generic;
using SenseNet.Search;

namespace SenseNet.Portal.Portlets
{
    public class RatingSearchPortlet : ContextBoundPortlet
    {
        private string _contentViewPath = "/Root/System/SystemPlugins/Portlets/RatingSearch/RatingSearch.ascx";

        public delegate string RatingSearchHandler(EventArgs e, string from, string to);

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("View path")]
        [WebDescription("Path of the .ascx user control which provides the elements of the portlet")]
        [WebCategory(EditorCategory.UI, EditorCategory.UI_Order)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.Ascx)]
        [WebOrder(100)]
        public string ContentViewPath
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
        public RatingSearchPortlet()
        {
            Name = "Rating search";
            Description = "Portlet for searching by rating (context bound)";
            this.Category = new PortletCategory(PortletCategoryType.Application);
        }

        /// <summary>
        /// Load the content view if possible, and get the attributes for it.
        /// </summary>
        private void CreateControls()
        {
            try
            {
                var viewControl = Page.LoadControl(ContentViewPath) as RatingSearch;
                if (viewControl != null)
                {
                    viewControl.LastSearchedRatingFrom = GetSearchedRatingFrom();
                    viewControl.LastSearchedRatingTo = GetSearchedRatingTo();
                    viewControl.Results = GetResult(viewControl.LastSearchedRatingFrom, viewControl.LastSearchedRatingTo);
                    viewControl.RatingSearching += viewControl_TagSearch;
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

        /// <summary>
        /// This method handled the Searvhing event, and return an url which has new parameter.
        /// </summary>
        /// <param name="e">Event arguments</param>
        /// <param name="from">Search Rating from this value</param>
        /// <param name="to">Search Rating to this value</param>
        /// <returns>Return the new url</returns>
        private string viewControl_TagSearch(EventArgs e, string from, string to)
        {
            string newUrl;

            if (Context.Request.Params.Get("RatingFilterFrom") == null)
            {
                newUrl = Context.Request.Url.ToString().Contains('?') ?
                    (string.Format("{0}&RatingFilterFrom={1}&RatingFilterTo={2}", Context.Request.Url, from, to)) : (string.Format("{0}?RatingFilterFrom={1}&RatingFilterTo={2}", Context.Request.Url, from, to));
            }
            else
            {
                var tempUrl = Context.Request.Url.ToString();
                var tempFilterFrom = Context.Request.Params.Get("RatingFilterFrom");
                var tempFilterTo = Context.Request.Params.Get("RatingFilterTo");
                var tempFiltered = tempUrl.Replace("RatingFilterFrom=" + tempFilterFrom, "RatingFilterFrom=" + from);
                newUrl = tempFiltered.Replace("RatingFilterTo=" + tempFilterTo, "RatingFilterTo=" + to);
            }


            return newUrl;
        }

        /// <summary>
        /// Get the last value which searching to.
        /// </summary>
        /// <returns>Return searching parameter: to</returns>
        private string GetSearchedRatingTo()
        {
            var ratingTo = string.Empty;

            if (Context.Request.Params.Get("RatingFilterTo") != null)
                ratingTo = Context.Request.Params.Get("RatingFilterTo");


            return ratingTo;
        }

        /// <summary>
        /// Get the value which searching from.
        /// </summary>
        /// <returns>Raturn searching parameter: from/returns>
        private string GetSearchedRatingFrom()
        {
            var ratingFrom = string.Empty;

            if (Context.Request.Params.Get("RatingFilterFrom") != null)
                ratingFrom = Context.Request.Params.Get("RatingFilterFrom");

            return ratingFrom;
        }

        /// <summary>
        /// Get the search result.
        /// </summary>
        /// <param name="from">value of searching from</param>
        /// <param name="to">value of searching to</param>
        /// <returns>List of nodes which matches the search.</returns>
        private IEnumerable GetResult(string from, string to)
        {
            if (from == string.Empty || to == string.Empty) return null;
            decimal fromD = 0;
            decimal toD = 5;

            Decimal.TryParse(from,out fromD);
            Decimal.TryParse(to, out toD);
            fromD -= (decimal) 0.01;
            toD += (decimal) 0.01;

            var contentsId = new List<int>();
            var query =
                LucQuery.Parse("Rate:{" + string.Format("{0} TO {1}", fromD.ToString("F2"), toD.ToString("F2")) + "}");
            var results = query.Execute();
            foreach (var o in results)
                contentsId.Add(o.NodeId);

            var result = new NodeList<Node>(contentsId);

            return result;

            //Demo query!!!
            //var queryString = "civic";

            //var contentsId = TagManager.GetNodeIds(queryString);
            //var result = new NodeList<Node>(contentsId);

            //return result;
        }

        /// <summary>
        /// Owerrided method, initalize the portlet
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();
            CreateControls();

        }
    }
}
