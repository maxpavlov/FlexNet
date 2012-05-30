using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.UI;
using SenseNet.Portal.UI.PortletFramework;
using SNC = SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.Portlets
{
	public class SearchPortlet : CacheablePortlet
	{
		//-- Variables ---------------------------------------------------

		public enum SearchMode
		{
			/*None = 0,*/
			Quick = 1/*,
		    Advanced = 2*/
		}

		SearchMode _searchPaneMode = SearchMode.Quick;
		string _cvFolderPath = string.Empty;
        //ViewMode _vwMode = ViewMode.Query;
		string _extraQueryXML = string.Empty;
        bool _allowEmptySearch = false;

		int _pageSize = 20;

		Panel _panelSearch = null;
		Panel _panelBaseSearch = null;
		TextBox _tbSearch = null;
		Button _btnSearch = null;
		Label _lblError = null;

		//-- Properties ---------------------------------------------------

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Extra query XML")]
        [WebDescription("")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(10)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string ExtraQueryXML
		{
			get { return _extraQueryXML; }
			set { _extraQueryXML = value; }
		}

		[WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Full query XML")]
        [WebDescription("")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(20)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.MultiLine)]
        public string FullQueryXML { get; set; }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Page size")]
        [WebDescription("The given number of contents are listed simultaneously")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(30)]
        [Editor(typeof(TextEditorPartField), typeof(IEditorPartField))]
        [TextEditorPartOptions(TextEditorCommonType.Small)]
        public int PageSize
		{
			get { return _pageSize; }
			set { _pageSize = value; }
		}

        [WebBrowsable(false), Personalizable(true)]
        [WebDisplayName("Search pane mode")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(40)]
        public SearchMode SearchPaneMode
		{
			get { return _searchPaneMode; }
			set { _searchPaneMode = value; }
		}

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Result contentview path")]
        [WebDescription("The contentview used to present result contents")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(50)]
        [Editor(typeof(ContentPickerEditorPartField), typeof(IEditorPartField))]
        [ContentPickerEditorPartOptions(ContentPickerCommonType.ContentView)]
        public string CvFolderPath
		{
			get { return _cvFolderPath; }
			set { _cvFolderPath = value; }
		}

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Allow empty search")]
        [WebDescription("")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(60)]
        public bool AllowEmptySearch
        {
            get { return _allowEmptySearch; }
            set { _allowEmptySearch = value; }
        }

        [WebBrowsable(true), Personalizable(true)]
        [WebDisplayName("Run as system")]
        [WebDescription("Execute the query in an elevated security context")]
        [WebCategory(EditorCategory.Search, EditorCategory.Search_Order)]
        [WebOrder(70)]
		public bool IsSystemAccount { get; set; }

		private string QueryString
		{
			get { return HttpContext.Current.Request.Params["Search"]; }
		}

		private int CurrentPage
		{
			get
			{
				int page;
				int.TryParse(HttpContext.Current.Request.Params["Page"], out page);
				return page > 0 ? page : 1;
			}
		}

        //-- Constructor ------------------------------------------------
        public SearchPortlet()
        {
            this.Name = "Search";
            this.Description = "Run free text searches and display search results in a pageable list.";
            this.Category = new PortletCategory(PortletCategoryType.Search);
        }

		//-- Initialize --------------------------------------------------

        protected override void CreateChildControls()
        {
            this.Controls.Clear();

            _panelBaseSearch = new Panel();
            _panelBaseSearch.ID = "panelBaseSearch";

            switch (SearchPaneMode)
            {
                case SearchMode.Quick: InitQuickPaneControls(); break;
                //case SearchMode.Advanced: InitAdvancedPaneControls(); break;
            }

            if (!Page.IsPostBack)
            {
                CreateResultControls();
            }
            this.Controls.Add(_panelBaseSearch);

            ChildControlsCreated = true;

        }

		//-- Result mode -------------------------------------------------

		private void CreateResultControls()
		{
            if (QueryString != null || (AllowEmptySearch && (!string.IsNullOrEmpty(FullQueryXML) || !string.IsNullOrEmpty(ExtraQueryXML))))
			{
                if ((AllowEmptySearch && (!string.IsNullOrEmpty(FullQueryXML) || !string.IsNullOrEmpty(ExtraQueryXML))) || QueryString.Length > 2)
				{
					var nodeList = RunSearch(QueryString);
					AddResultControls(nodeList);
				}
				else
				{
					CreateError(HttpContext.GetGlobalResourceObject("SearchPortlet", "SearchCriteria3Char") as string);
					AddControl(_lblError);
				}
			}
		}

		private void AddResultControls(List<Node> nodeList)
		{
			if (nodeList != null && nodeList.Count > 0)
			{
				AddResultPaging(nodeList);
			}
			else
			{
				CreateError(HttpContext.GetGlobalResourceObject("SearchPortlet", "NoSearchResultText") as string);
				AddControl(_lblError);
			}
		}

		private void AddResultPaging(List<Node> nodeList)
		{
			int start = CurrentPage * PageSize - PageSize;
			int end = start + PageSize >= nodeList.Count ? nodeList.Count : start + PageSize;

			if (nodeList.Count >= start)
			{
				for (int i = start; i < end; i++)
				{
					ContentView cv = CreateContentView(nodeList[i]);
					AddControl(cv);
				}

				int rem = 0;
				int pageCount = Math.DivRem(nodeList.Count, PageSize, out rem);
				if (rem > 0)
					pageCount++;

				if (pageCount > 1)
				{
					AddControl(new LiteralControl("<div class=\"sn_searchPage\">"));
					for (int j = 1; j <= pageCount; j++)
					{
						if (j != CurrentPage)
							AddControl(new LiteralControl(string.Concat("<a href=\"", GetSearchUrl(QueryString), "&Page=", j, "\">", j, "</a> ")));
						else
							AddControl(new LiteralControl(string.Concat(j, "&nbsp;")));
					}
					AddControl(new LiteralControl("</div>"));
				}
			}
		}

		private ContentView CreateContentView(Node node)
		{
			SNC.Content content = SNC.Content.Create(node);
			ContentView cv = null;

			if (string.IsNullOrEmpty(this.CvFolderPath))
				cv = ContentView.Create(content, this.Page, ViewMode.Query);
			else
				cv = ContentView.Create(content, this.Page, ViewMode.Query, this.CvFolderPath);

			return cv;
		}

		private List<Node> RunSearch(string queryString)
		{
			NodeQuery query = new NodeQuery();

            if (string.IsNullOrEmpty(queryString))
                queryString = string.Empty;

			if (string.IsNullOrEmpty(FullQueryXML))
			{
				StringBuilder queryXML = new StringBuilder();
				queryXML.Append("<SearchExpression xmlns=\"").Append(NodeQuery.XmlNamespace).AppendLine("\">");
                queryXML.AppendLine("  <And>");
                if (!string.IsNullOrEmpty(queryString))
				    queryXML.AppendLine(string.Concat("    <FullText>\"", queryString, "*\"</FullText>"));
                queryXML.AppendLine("    <Not><String op=\"StartsWith\" property=\"Path\">/Root/System</String></Not>");
                //queryXML.AppendLine("    <Type nodeType=\"Page\" />");
                //queryXML.AppendLine(string.Concat("    <String op=\"StartsWith\" property=\"Path\">", SenseNet.Portal.Virtualization.PortalContext.Current.Site.Path, "</String>"));

                queryXML.AppendLine(ExtraQueryXML);

                queryXML.AppendLine("  </And>");
                queryXML.AppendLine("</SearchExpression>");

				query = NodeQuery.Parse(queryXML.ToString());
			}
			else
			{
				if (AllowEmptySearch && string.IsNullOrEmpty(queryString))
				{
					int start = FullQueryXML.IndexOf("<FullText");
					if (start > -1)
					{
						int end = FullQueryXML.IndexOf("</FullText>");
						query = NodeQuery.Parse(string.Concat(FullQueryXML.Substring(0, start), FullQueryXML.Substring(end + 11)));
					}
					else
					{
						query = NodeQuery.Parse(FullQueryXML.Replace("#SEARCH_STRING", queryString));
					}
				}
				else
				{
					query = NodeQuery.Parse(FullQueryXML.Replace("#SEARCH_STRING", queryString));
				}
			}

			var nodeList = new List<Node>();

			try
			{
				if (IsSystemAccount)
					AccessProvider.ChangeToSystemAccount();

				nodeList = query.Execute().Nodes.ToList();
			}
			catch(Exception ex) //logged
			{
                Logger.WriteException(ex);
				Controls.Add(new LiteralControl(string.Concat("!<!--", ex.Message, " ", ex.StackTrace, "-->")));
			}
			finally
			{
				if (IsSystemAccount)
					AccessProvider.RestoreOriginalUser();
			}

			return nodeList;
		}

		//-- Quick pane --------------------------------------------------

		private void InitQuickPaneControls()
		{
			CreateQuickPaneControls();
			AddQuickPaneControls();
		}

		private void AddQuickPaneControls()
		{
			AddControl(CreateLiteral("<div class=\"sn-search\">"));
			AddControl(_panelSearch);
			AddControl(CreateLiteral("</div>"));
		}

		private void CreateQuickPaneControls()
		{
			_tbSearch = new TextBox();
			_tbSearch.ID = "tbSearch1";
			_tbSearch.CssClass = "sn-search-text";
			_tbSearch.Text = QueryString ?? string.Empty;

			_btnSearch = new Button();
			_btnSearch.ID = "btnSearch";
			_btnSearch.CssClass = "sn-search-button";
			_btnSearch.Text = HttpContext.GetGlobalResourceObject("SearchPortlet", "Search") as string;
			_btnSearch.UseSubmitBehavior = true;
			_btnSearch.Click += new EventHandler(_btnSearch_Click);

			_panelSearch = new Panel();
			_panelSearch.ID = "panelSearch";
			_panelSearch.DefaultButton = "btnSearch";

			_panelSearch.Controls.Add(_tbSearch);
			_panelSearch.Controls.Add(_btnSearch);
		}

		//-- Advanced pane -----------------------------------------------

		private void InitAdvancedPaneControls()
		{
			AddAdvancedPaneControls();
			CreateAdvancedPaneControls();
		}

		private void AddAdvancedPaneControls()
		{
			throw new NotImplementedException();
		}

		private void CreateAdvancedPaneControls()
		{
			throw new NotImplementedException();
		}

		//-- Events ------------------------------------------------------

		void _btnSearch_Click(object sender, EventArgs e)
		{
			string url = GetSearchUrl(_tbSearch.Text);

			HttpContext.Current.Response.Redirect(url);
		}

		private static string GetSearchUrl(string searchText)
		{
			string url = SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.AbsoluteUri;
			string query = SenseNet.Portal.Virtualization.PortalContext.Current.OriginalUri.Query;
			string ru = string.IsNullOrEmpty(query) ? url : url.Substring(0, url.Length - query.Length);

            return string.Concat(ru, "?Search=", HttpUtility.UrlEncode(searchText));
            //return string.Concat(ru, "?Search=", HttpUtility.UrlEncodeUnicode(searchText));
        }

		//-- Helper ------------------------------------------------------

		private void AddControl(Control control)
		{
			_panelBaseSearch.Controls.Add(control);
		}

		private static Literal CreateLiteral(string text)
		{
			Literal literal = new Literal();
			literal.Text = text;
			return literal;
		}

		private void CreateError(string errorMessage)
		{
			_lblError = new Label();
			_lblError.CssClass = "sn-error";
			_lblError.Text = errorMessage;
		}
	}

	public class ProgressTemplate : ITemplate
	{
		private string template;

		public ProgressTemplate(string tmp)
		{
			template = tmp;
		}

		public void InstantiateIn(Control container)
		{
			LiteralControl lc = new LiteralControl(this.template);
			container.Controls.Add(lc);
		}
	}
}