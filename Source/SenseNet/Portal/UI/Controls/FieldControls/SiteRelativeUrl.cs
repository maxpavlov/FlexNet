using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.Portal.Virtualization;
using System.Web;
using System.Linq;

namespace SenseNet.Portal.UI.Controls
{
	[ToolboxData("<{0}:SiteRelativeUrl ID=\"SiteRelativeUrl1\" runat=server></{0}:SiteRelativeUrl>")]
	public class SiteRelativeUrl : FieldControl, INamingContainer, ITemplateFieldControl
	{
	    public SiteRelativeUrl() { InnerControlID = "RelativeUrlList"; }

		public override void SetData(object data)
		{
            if ((!UseBrowseTemplate && !UseEditTemplate) && !UseInlineEditTemplate)
                return;
            var title = GetLabelForTitleControl() as Label;
            var desc = GetLabelForDescription() as Label;
            if (title != null) title.Text = Field.DisplayName;
            if (desc != null) desc.Text = Field.Description;            
		}
		public override object GetData() { return null; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            var contentPath = string.Empty;
            var currentUrlList = GetSiteUrlList(ref contentPath);
            var protocol = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Scheme);
            
            #region template

            if (UseBrowseTemplate)
            {
                Try2AddUrlLinks(contentPath, currentUrlList, protocol);
                base.RenderContents(writer);
                return;
            }
            if (UseEditTemplate)
            {
                Try2AddUrlLinks(contentPath, currentUrlList, protocol);
                base.RenderContents(writer);
                return;
            }
            if (UseInlineEditTemplate)
            {
                Try2AddUrlLinks(contentPath, currentUrlList, protocol);
                base.RenderContents(writer);
                return;
            }

            #endregion

            foreach (var siteUrl in currentUrlList)
            {
                var url = string.Concat(protocol, siteUrl, contentPath);
                var link = new System.Web.UI.WebControls.HyperLink();
                link.NavigateUrl = url;
                link.Text = url;
                link.Target = "new";
                link.CssClass = this.CssClass;
                link.RenderControl(writer);
                writer.WriteBreak();
            }
        }

	    

        // Internals ////////////////////////////////////////////////////////////////////
	    private void AddLinkTo(Control list, string url)
	    {
	        if (list == null) 
                throw new ArgumentNullException("list");
	        if (url == null) 
                throw new ArgumentNullException("url");
	        var link = new System.Web.UI.WebControls.HyperLink
                           {
                               NavigateUrl = url,
                               Text = url,
                               Target = "new",
                               CssClass = CssClass
                           };
	        list.Controls.Add(link);
            list.Controls.Add(new LiteralControl("<br />"));
	    }
        private IEnumerable<string> GetSiteUrlList(ref string contentPath)
        {
            if (contentPath == null) 
                throw new ArgumentNullException("contentPath");

            var site = SenseNet.Portal.Site.GetSiteByNode(ContentHandler);
            if (site == null)
            {
                site = PortalContext.Current.Site;
                contentPath = this.ContentHandler.Path;
            }
            else
                contentPath = this.ContentHandler.Path.Substring(site.Path.Length);

            return site.UrlList.Keys.ToList().AsReadOnly();
        }
        private void Try2AddUrlLinks(string contentPath, IEnumerable<string> currentUrlList, string protocol)
        {
            if (contentPath == null)
                throw new ArgumentNullException("contentPath");
            if (currentUrlList == null)
                throw new ArgumentNullException("currentUrlList");
            if (protocol == null)
                throw new ArgumentNullException("protocol");

            var urlList = GetInnerControl() as PlaceHolder;
            if (urlList == null)
                return;
            foreach (var siteUrl in currentUrlList)
            {
                var url = string.Concat(protocol, siteUrl, contentPath);
                AddLinkTo(urlList, url);
            }
        }
	    
        #region ITemplateFieldControl Members

        public Control GetInnerControl()
        {
            return this.FindControlRecursive(InnerControlID);
        }

        public Control GetLabelForDescription()
        {
            return this.FindControlRecursive(DescriptionControlID);
        }

        public Control GetLabelForTitleControl()
        {
            return this.FindControlRecursive(TitleControlID);
        }

        #endregion
    }
}