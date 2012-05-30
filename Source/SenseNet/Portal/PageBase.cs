using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Web.UI;
using SenseNet.ContentRepository;
using SenseNet.Portal.Personalization;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Caching.DistributedActions;
using System.Web.SessionState;
using SNP = SenseNet.Portal;
using SenseNet.Diagnostics;
using SenseNet.Portal.Virtualization;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.UI;
using System.Configuration;
using SenseNet.ContentRepository.i18n;

namespace SenseNet.Portal
{
    public class PageBase : System.Web.UI.Page, IRequiresSessionState
    {
        #region Timing
        protected Stopwatch _timer;

        public PageBase()
        {
            // start timer if the user has been requested it.
            if (this.ShowExecutionTime)
            {
                _timer = new Stopwatch();
                _timer.Start();
            }
        }

        /// <summary>
        /// Indicates that the portlet can render information about its exection time at the end of html fragment of the portlet.
        /// </summary>
        protected bool ShowExecutionTime
        {
            get
            {
                HttpRequest request = HttpContext.Current.Request;
                if (request != null && request.Params["ShowExecutionTime"] != null)
                {
                    string showExecutionTime = request.Params["ShowExecutionTime"] as string;
                    bool showExecutionTimeValue = false;
                    if (bool.TryParse(showExecutionTime, out showExecutionTimeValue))
                        return showExecutionTimeValue;
                }
                return false;
            }
        }

        #endregion


        private bool CleanupCache
        {
            get
            {
                HttpRequest request = HttpContext.Current.Request;
                if (request != null && request.Params["CleanupCache"] != null)
                {
                    string cleanupCache = request.Params["CleanupCache"] as string;
                    bool cleanCacheValue = false;
                    if (bool.TryParse(cleanupCache, out cleanCacheValue))
                        return cleanCacheValue;
                }
                return false;
            }
        }

        
        
        protected override void OnPreInit(EventArgs e)
        {

            if (this.CleanupCache)
            {
                try
                {
                    new CacheCleanAction().Execute();
                }
                catch (Exception exc) //logged
                {
                    Logger.WriteException(exc);
                }

            }

            // This hack solves the page template change timing issue.
            // If you change a page template from the Edit Page datasheet in PRC,
            // the page reloads with the correct master page. Without this hack, it does not.
            try
            {
                var currentPage = SenseNet.Portal.Virtualization.PortalContext.Current.Page;
                if (currentPage != null)
                {
                    string masterfile = currentPage.PageTemplateNode.Path.Replace(".html", ".Master");
                    if (!string.IsNullOrEmpty(masterfile) &&
                        masterfile.EndsWith(".master", StringComparison.InvariantCultureIgnoreCase) &&
                        masterfile != base.MasterPageFile)
                    {
                        base.MasterPageFile = masterfile;
                    }
                }
            }
            catch (Exception ex) //logged
            {
                Logger.WriteException(ex);
            }
            ////////////////////////////////////////////////////////////////////////////////////////


            base.OnPreInit(e);
        }

        protected override void OnLoadComplete(EventArgs e)
        {
			var currentPage = SNP.Page.Current;
            try
            {
                base.OnLoadComplete(e);
            }
            catch (InvalidOperationException exc) //logged
            {
                Logger.WriteException(exc);
            }
            if (currentPage != null)
            {
                //currentPage.ProcessTemporaryPortletInfo(this);
                SetHeadMetaTagsAndTitle();
            }
        }

        private void SetHeadMetaTagsAndTitle()
        {
            var contextNode = PortalContext.Current.ContextNode ?? SenseNet.Portal.Page.Current;

            string keywords = GetSeoFieldValue(contextNode, "Keywords");
            string metaDesc = GetSeoFieldValue(contextNode, "MetaDescription");
            string author = GetSeoFieldValue(contextNode, "MetaAuthors");
            string customMeta = GetSeoFieldValue(contextNode, "CustomMeta");
            string metaTitle = GetSeoFieldValue(contextNode, "MetaTitle");

            if (!String.IsNullOrEmpty(metaTitle))
            {
                HtmlTitle t = GetTitle();
                if (t != null) t.Text = metaTitle;
            }

            if (!string.IsNullOrEmpty(keywords))
                AddMetaTag(keywords, "Keywords");

            if (!string.IsNullOrEmpty(metaDesc))
                AddMetaTag(metaDesc, "Description");

            if (!string.IsNullOrEmpty(author))
                AddMetaTag(author, "Author");

            if (!string.IsNullOrEmpty(customMeta))
                this.Header.Controls.Add(new LiteralControl(customMeta));

        }

        private static string GetSeoFieldValue(Node contextNode, string propertyName)
        {
            if (contextNode == null)
                return string.Empty;
            if (string.IsNullOrEmpty(propertyName))
                return string.Empty;

            var result = contextNode.GetPropertySafely(propertyName);
            string resultString = result as string;
            return string.IsNullOrEmpty(resultString) ? string.Empty : resultString;
        }

        private void AddMetaTag(string content, string name)
        {
            this.Header.Controls.Add(new HtmlMeta {Content = content, Name = name});
        }
        internal HtmlTitle GetTitle()
        {
            return this.Header.Controls.OfType<HtmlTitle>().FirstOrDefault();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);

            if (this.ShowExecutionTime)
            {
                _timer.Stop();
                RenderTimerValue(writer);

            }

            var s = Tracing.GetOperationTrace();
            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    RenderDiagnosticTree(s, writer);
                }
                finally 
                {
                    Tracing.ClearOperationTrace();    
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (SenseNetResourceManager.IsResourceEditorAllowed)
                ResourceEditorController.InitEditorScript(this);

            base.OnPreRender(e);
        }

        /// <summary>
        /// Renders a(n) html fragment which contains the text will be displayed in the browser of the end user, if user has requested it.
        /// </summary>
        /// <param name="writer">HtmlTextWriter stores the content.</param>
        private void RenderTimerValue(System.Web.UI.HtmlTextWriter writer)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"<div style=""color:#fff;background:#008;font-weight:bold,padding:2px"">");
            //this.Cacheable && this.CanCache
            sb.Append(String.Format("Execution time of the current page was {0:F10} seconds.", _timer.Elapsed.TotalSeconds ));
            sb.Append(@"</div>");
            writer.Write(sb.ToString());
        }


        public void Done()
        {
            Done(true);
        }

        public void Done(bool endResponse)
        {
            Done(endResponse, null);
        }

        public void Done(Node newNode)
        {
            Done(true, newNode);
        }

        public void Done(bool endResponse, Node newNode)
        {
            var back = PortalContext.Current.BackUrl;
            var backTarget = PortalContext.Current.BackTarget;
            if (backTarget == BackTargetType.None && !string.IsNullOrEmpty(back))
            {
                Response.Redirect(back, endResponse);
            }
            else
            {
                var backTargetUrl = PortalContext.GetBackTargetUrl(newNode);
                if (!string.IsNullOrEmpty(backTargetUrl))
                    Response.Redirect(backTargetUrl, endResponse);
            }
        }


        /* ===================================================================== Helper Methods */
        private static void RenderDiagnosticTree(string xmlString, System.Web.UI.HtmlTextWriter writer)
        {
            using (TextReader xmlTextReader = new StringReader(xmlString))
            {
                WriteTransformedXML(xmlTextReader, writer, "/Root/System/Renderers/Default-Page-Diagnostics.xslt");
            }
        }
        private static void WriteTransformedXML(TextReader xmlTextReader, TextWriter writer, string xsltPath)
        {
            if (string.IsNullOrEmpty(xsltPath))
                return;

            var xsltNode = Node.Load<ContentRepository.File>(xsltPath);
            if (xsltNode == null)
                return;

            using (var reader = XmlReader.Create(xsltNode.Binary.GetStream()))
            {
                var transformer = new XslCompiledTransform();
                transformer.Load(reader);

                using (var xmlReader = XmlReader.Create(xmlTextReader))
                {
                    using (var xmlWriter = new XmlTextWriter(writer))
                    {
                        transformer.Transform(xmlReader, xmlWriter);
                    }
                }
            }
        }
    }
}
