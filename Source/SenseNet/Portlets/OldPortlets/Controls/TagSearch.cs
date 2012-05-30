using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.UI;

namespace SenseNet.Portal.Portlets.Controls
{
	public class TagSearch : UserControl, ITagSearchPortlet
	{

        protected TextBox tbTagSearch;
        protected ListView TagSearchListView;
        public IEnumerable Results { get; set; }
        public string LastSearchedTag { get; set; }

        public event TagSearchPortlet.TagSearchHandler TagSearching;

        /// <summary>
        /// Overrided method for initalize the content view, get the searched tag's name and the search result.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            tbTagSearch.Text = LastSearchedTag;
            TagSearchListView.DataSource = Results;
            TagSearchListView.DataBind();

            SenseNet.Portal.UI.UITools.AddScript(SenseNet.Portal.UI.UITools.ClientScriptConfigurations.jQueryPath);
            UITools.AddStyleSheetToHeader(UITools.GetHeader(), "$skin/styles/SN.Tagging.css");
        }

        /// <summary>
        /// Handle the button click on the content view, and triger the Tagsearching event with the textbox value.
        /// </summary>
        /// <param name="sender">The sender object</param>
        /// <param name="e">Event arguments</param>
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (TagSearching != null)
                Response.Redirect(TagSearching(e, tbTagSearch.Text));
        }
	}
}