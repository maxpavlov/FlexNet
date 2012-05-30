using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Versioning;
using SNP = SenseNet.Portal;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:SearchView runat=server />")]
	public class SearchView : WebControl
	{

		//TODO:
//		<div class="sn-search-result">
//			<h2><a><strong></strong></a></h2>
//			<p class="snSearchExtract"><strong></strong></p>
//			<p class="snSearchUrl"></p>
//		</div>

		private string _cssResult = "sn-search-result";
		public string CssResult
		{
			get { return _cssResult; }
			set { _cssResult = value; }
		}

		private string _cssExtract = "sn-search-extract";
		public string CssExtract
		{
			get { return _cssExtract; }
			set { _cssExtract = value; }
		}

		private string _cssUrl = "sn-search-url";
		public string CssUrl
		{
			get { return _cssUrl; }
			set { _cssUrl = value; }
		}

		private string QueryString
		{
			get { return (string)HttpContext.Current.Session["quickSearchString"]; }
		}

		private GenericContent _currentGenericContent;
		private GenericContent CurrentGenericContent
		{
			get
			{
				if (_currentGenericContent != null)
				{
					return _currentGenericContent;
				}
				else
				{
					if (Parent is ContentView)
					{
						_currentGenericContent = (Parent as ContentView).Content.ContentHandler as GenericContent;
						return _currentGenericContent;

					}
					return null;
				}
			}
			set
			{
				_currentGenericContent = value;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			
			foreach (string url in SNP.Page.Current.Site.UrlList.Keys)
			{
				if (PortalContext.Current.OriginalUri.ToString().IndexOf(string.Concat(PortalContext.Current.OriginalUri.Scheme, "://", url, "/")) == 0)
				{
					Controls.Add(CreateLiteral(string.Concat("<div class=\"", CssResult, "\">")));

					Controls.Add(CreateLiteral("<h2>"));
					//TODO: <strong></strong>
					Controls.Add(CreateLiteral(CreateLink(GenerateUrl(url, CurrentGenericContent.Path), GetContentName())));
					Controls.Add(CreateLiteral("</h2>"));

					//Controls.Add(CreateLiteral(string.Concat("<p class=\"", CssExtract, "\">")));
					//TODO: Extract <strong></strong>
					//Controls.Add(CreateLiteral(GetExtract()));
					//Controls.Add(CreateLiteral("</p>"));

					var pageContent = CurrentGenericContent as SNP.Page;
					if (pageContent != null && pageContent.MetaDescription.Length > 0)
                    {
                        Controls.Add(CreateLiteral(string.Concat("<p class=\"", CssUrl, "\">")));
						Controls.Add(CreateLiteral(pageContent.MetaDescription.Length > 150 ? string.Concat(pageContent.MetaDescription.Substring(0, 150), "...") : pageContent.MetaDescription));
                        Controls.Add(CreateLiteral("</p>"));
                    }

					Controls.Add(CreateLiteral(string.Concat("<p class=\"", CssUrl, "\">")));
					Controls.Add(CreateLiteral(GenerateUrl(url, CurrentGenericContent.Path.Length > 100 ? string.Concat(CurrentGenericContent.Path.Substring(0, 100), "...") : CurrentGenericContent.Path)));
					Controls.Add(CreateLiteral("</p>"));

					Controls.Add(CreateLiteral("</div>"));
				}
			}
		}

		private string GetContentName()
		{
            //if(CurrentGenericContent is SNP.Page)
            //    return (CurrentGenericContent as SNP.Page).PageNameInMenu;
            //return CurrentGenericContent.Name;

            return CurrentGenericContent.DisplayName;
		}

		private string GetExtract()
		{
			if (CurrentGenericContent is SNP.Page)
			{
				string extract = (CurrentGenericContent as SNP.Page).TextExtract;
				if (!string.IsNullOrEmpty(extract))
				{
					if (!string.IsNullOrEmpty(QueryString))
					{
						int idx = extract.IndexOf(QueryString);
						int start = idx < 75 ? 0 : idx - 75;
						return extract.Substring(start, extract.Length > 150 ? 150 : extract.Length);
					}
					return extract.Length > 149 ? extract.Substring(0, 150) : extract;
				}
			}
			return string.Empty;
		}

		private string GenerateUrl(string url, string nodePath)
		{
            if (!nodePath.StartsWith(SNP.Page.Current.Site.Path + "/"))
                return String.Concat(PortalContext.Current.OriginalUri.Scheme, "://", url, nodePath);

			return string.Concat(PortalContext.Current.OriginalUri.Scheme, "://", url,
				nodePath.Substring(SNP.Page.Current.Site.Path.Length));
		}

		private string CreateLink(string url, string urlText)
		{
			return string.Concat("<a href=\"", url, "\">", urlText, "</a>");
		}

		private Literal CreateDiv(string cssClass)
		{
			return CreateLiteral(string.Concat("<div class=\"", cssClass, "\">"));
		}

		private static Literal CreateLiteral(string text)
		{
			Literal literal = new Literal();
			literal.Text = text;
			return literal;
		}
	}
}