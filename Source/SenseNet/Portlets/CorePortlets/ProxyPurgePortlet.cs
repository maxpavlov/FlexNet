using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SenseNet.ContentRepository.Storage;
using SenseNet.Diagnostics;
using SenseNet.Portal.UI.Controls;
using SenseNet.Portal.UI.PortletFramework;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Portlets
{
    public class ProxyPurgePortlet : ContextBoundPortlet
    {
        public class ProxyPurgeResult
        {
            public string Url { get; set; }
            public string[] Result { get; set; }
        }

        public class ProxyResultMessage
        {
            public string ProxyIP { get; set; }
            public string Message { get; set; }
        }

        //========================================================================================= Constructor

        public ProxyPurgePortlet()
        {
            this.Name = "Proxy purge";
            this.Description = "This portlet clears items from the proxy cache that are related to a content";
            this.Category = new PortletCategory(PortletCategoryType.System);

            this.HiddenPropertyCategories = new List<string> { EditorCategory.Cache };
        }

        //========================================================================================= Properties

        private ListView _urlListView;
        protected ListView UrlListView
        {
            get { return _urlListView ?? (_urlListView = this.FindControlRecursive("UrlListView") as ListView); }
        }

        private PlaceHolder _plcError;
        protected PlaceHolder ErrorPlaceholder
        {
            get { return _plcError ?? (_plcError = this.FindControlRecursive("ErrorPlaceholder") as PlaceHolder); }
        }

        //========================================================================================= Overrides

        protected override void CreateChildControls()
        {
            Controls.Clear();

            var node = GetContextNode();
            if (node == null)
            {
                if (this.RenderException != null)
                {
                    this.Controls.Clear();
                    this.Controls.Add(new LiteralControl(String.Concat("Portlet Error: ", this.RenderException.Message)));
                }

                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(this.Renderer))
                {
                    var viewControl = Page.LoadControl(this.Renderer);
                    if (viewControl != null)
                        this.Controls.Add(viewControl);

                    if (UrlListView == null)
                        throw new Exception("UrlListView is missing from the view: " + this.Renderer);

                    if (PortalContext.ProxyIPs.Count == 0)
                    {
                        UrlListView.Visible = true;

                        if (this.ErrorPlaceholder != null)
                        {
                            var errorLabel = new Label();
                            errorLabel.Text = HttpContext.GetGlobalResourceObject("Portal", "NoProxyServers") as string;
                            errorLabel.CssClass = "sn-purge-error";

                            this.ErrorPlaceholder.Visible = true; 
                            this.ErrorPlaceholder.Controls.Add(errorLabel);
                        }

                        return;
                    }

                    var urlCollector = TypeHandler.ResolveProvider<PurgeUrlCollector>();
                    if (urlCollector == null)
                        throw new Exception("UrlCollector type is missing");

                    var urlList = urlCollector.GetUrls(node);
                    var purgeResult = HttpHeaderTools.PurgeUrlsFromProxy(urlList);
                    var resultList = from url in urlList
                                     where !string.IsNullOrEmpty(url) && purgeResult.ContainsKey(url)
                                     select new ProxyPurgeResult {Url = url, Result = purgeResult[url]};

                    UrlListView.ItemDataBound += UrlListView_ItemDataBound;
                    UrlListView.DataSource = resultList;
                    UrlListView.DataBind();
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                Controls.Add(new LiteralControl(ex.Message));
            } 

            ChildControlsCreated = true;   
        }
        
        protected override void RenderWithAscx(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }

        //========================================================================================= Event handlers

        protected void UrlListView_ItemDataBound(object sender, ListViewItemEventArgs e)
        {
            var dataItem = e.Item as ListViewDataItem;
            if (dataItem == null)
                return;

            var ppResult = dataItem.DataItem as ProxyPurgeResult;
            if (ppResult == null)
                return;

            var proxyLv = GetInnerListView(dataItem);
            if (proxyLv == null)
                return;

            var displayedMessages = new [] {"OK", "PURGED", "MISS"};

            var proxyList = new List<ProxyResultMessage>();
            var proxyIndex = 0;
            foreach (var proxyIP in PortalContext.ProxyIPs)
            {
                proxyList.Add(displayedMessages.Contains(ppResult.Result[proxyIndex]) ?
                    new ProxyResultMessage { ProxyIP = proxyIP, Message = ppResult.Result[proxyIndex] } :
                    new ProxyResultMessage { ProxyIP = proxyIP + " - " + ppResult.Result[proxyIndex], Message = "ERROR" }
                    );

                proxyIndex++;
            }

            proxyLv.DataSource = proxyList;
            proxyLv.DataBind();
        }

        //========================================================================================= Helper methods

        private ListView GetInnerListView(ListViewDataItem dataItem)
        {
            return dataItem.FindControlRecursive("ProxyResultList") as ListView;
        }
    }
}
