using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;
using SNP = SenseNet.Portal;

namespace SenseNet.Portal.Portlets
{
	public class QuickSearchPortlet : CacheablePortlet
	{
		//-- Variables ---------------------------------------------------

		string _searchResultPageUrl = string.Empty;
		string _enterKeyword = string.Empty;
		
		TextBox _tbSearch = null;
		Button _btnSearch = null;
		Panel _panelSearch = null;

		//-- Properties ---------------------------------------------------

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Search result page URL")]
        [WebDescription("Url of the page where results are listed with a Search portlet")]
        [WebCategory(EditorCategory.QuickSearch, EditorCategory.QuickSearch_Order)]
        public string SearchResultPageUrl
		{
			get { return _searchResultPageUrl; }
			set { _searchResultPageUrl = value; }
		}

		[WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Default search keyword")]
        [WebDescription("Quick search input shows this value by default")]
        [WebCategory(EditorCategory.QuickSearch, EditorCategory.QuickSearch_Order)]
        public string EnterKeyword
		{
			get { return _enterKeyword; }
			set { _enterKeyword = value; }
		}

        //-- Constructor -------------------------------------------------
        public QuickSearchPortlet()
        {
            this.Name = "Quick search";
            this.Description = "A simple search field and button for executing quick searches from each page.";
            this.Category = new PortletCategory(PortletCategoryType.Search);

            this.HiddenProperties.Add("Renderer");
        }
        //-- Initialize --------------------------------------------------
        protected override void CreateChildControls()
        {
            if (ShowExecutionTime)
                Timer.Start();

            this.Controls.Clear();
            CreateSearchModeControls();
            AddSearchModeControls();

            ChildControlsCreated = true;

            if (ShowExecutionTime)
                Timer.Stop();
        }


		//-- Search mode -------------------------------------------------

		private void AddSearchModeControls()
		{
			//this.Controls.Add(CreateLiteral("<div class=\"sn-quicksearch\">"));

			_panelSearch.Controls.Add(_tbSearch);
			_panelSearch.Controls.Add(_btnSearch);
			this.Controls.Add(_panelSearch);

			//this.Controls.Add(CreateLiteral("</div>"));
		}

		private void CreateSearchModeControls()
		{
			_tbSearch = new TextBox();
			_tbSearch.ID = "tbSearch1";
			_tbSearch.Text = EnterKeyword;
			_tbSearch.CssClass = "sn-quicksearch-text";

			_btnSearch = new Button();
			_btnSearch.ID = "btnSearch";
			_btnSearch.CssClass = "sn-quicksearch-button";
			_btnSearch.Text = HttpContext.GetGlobalResourceObject("SearchPortlet", "Search") as string;
			_btnSearch.UseSubmitBehavior = true;
			_btnSearch.Click += new EventHandler(_btnSearch_Click);

			_panelSearch = new Panel();
			_panelSearch.ID = "sn-quicksearch";
			_panelSearch.CssClass = "sn-quicksearch";
			_panelSearch.DefaultButton = "btnSearch";
		}

		//-- Events ------------------------------------------------------

		void _btnSearch_Click(object sender, EventArgs e)
		{
			Uri origUri = PortalContext.Current.OriginalUri;

            StringBuilder redirectUrlBuilder = new StringBuilder();

            redirectUrlBuilder.Append(origUri.Scheme);
            redirectUrlBuilder.Append("://");
            if (!string.IsNullOrEmpty(SearchResultPageUrl))
            {
                redirectUrlBuilder.Append(SNP.Site.GetUrlByRepositoryPath(origUri.AbsoluteUri.Substring(origUri.Scheme.Length + 3), SearchResultPageUrl));
            }
            else
            {
                redirectUrlBuilder.Append(origUri.Authority);
                redirectUrlBuilder.Append(origUri.AbsolutePath);
            }
            redirectUrlBuilder.Append("?Search=");
            redirectUrlBuilder.Append(HttpUtility.UrlEncode(_tbSearch.Text));
            //redirectUrlBuilder.Append(HttpUtility.UrlEncodeUnicode(_tbSearch.Text));

			HttpContext.Current.Response.Redirect(redirectUrlBuilder.ToString());
		}

		//-- Helper ------------------------------------------------------

		private static Literal CreateLiteral(string text)
		{
			Literal literal = new Literal();
			literal.Text = text;
			return literal;
		}
	}
}