using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Compilation;
using System.Web.UI.WebControls.WebParts;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Search.Internal;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal.Virtualization;
using SenseNet.Diagnostics;
using System.Xml;

namespace SenseNet.Portal
{
    [ContentHandler]
    public class Page : Webform, IFolder
    {
        [Obsolete("Use typeof(Page).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Page).Name;

        internal const string ContextKey = "Portal.CurrentPage";

        public Page(Node parent) : this(parent, null) { }
        public Page(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Page(NodeToken nt) : base(nt) { }

        public override void Save()
        {
            SetPageData();

            base.Save();

            //TODO: SetContentPageRelation();

            HandleSmartUrlListChanging();
        }

        public override void Save(SavingMode mode)
		{
            SetPageData();

            base.Save(mode);

			//TODO: SetContentPageRelation();
            
            HandleSmartUrlListChanging();
		}

		public override void SaveSameVersion()
		{
			SetPageData();

			base.SaveSameVersion();

			//TODO: SetContentPageRelation();

            HandleSmartUrlListChanging();
		}

        private void SetPageData()
        {
            if (this.SmartUrl == null || this.CopyInProgress)
            {
                var savedFlag = SmartUrlChanged;
                this.SmartUrl = string.Empty;
                SmartUrlChanged = savedFlag;
            }

            if (this.SmartUrl != String.Empty)
            {
                NodeQueryResult result;
                using (new SystemAccount())
                {
                    if (StorageContext.Search.IsOuterEngineEnabled && StorageContext.Search.SearchEngine != InternalSearchEngine.Instance)
                    {
                        //this NodeQuery will be compiled to LucQuery because the outer engine is enabled
                        var pageQuery = new NodeQuery();
                        pageQuery.Add(new TypeExpression(ActiveSchema.NodeTypes[typeof (Page).Name]));
                        pageQuery.Add(new StringExpression(ActiveSchema.PropertyTypes["SmartUrl"], StringOperator.Equal, this.SmartUrl));
                        pageQuery.Add(new NotExpression(new StringExpression(StringAttribute.Path, StringOperator.Equal, this.Path)));

                        result = pageQuery.Execute();
                    }
                    else
                    {
                        //we need to execute a direct database query because the outer engine is disabled
                        result = NodeQuery.QueryNodesByTypeAndPathAndProperty(
                            ActiveSchema.NodeTypes[typeof (Page).Name], false, null, false,
                            new List<QueryPropertyData>(new[] { 
                                new QueryPropertyData {PropertyName = "SmartUrl", Value = this.SmartUrl},
                                new QueryPropertyData {PropertyName = "Path", Value = this.Path, QueryOperator = Operator.NotEqual}}));
                    }
                }

                if (result.Count > 0)
                {
                    var page = result.Nodes.First() as Page;
                    if (page != null)
                        throw new Exception(String.Concat("'", this.SmartUrl, "' smartUrl is already mapped to page with path '", page.Path, "'"));
                }
            }

            SetBinary();
        }

        private void HandleSmartUrlListChanging()
        {
            if (SmartUrlChanged)
            {
                var action = new PortalContext.ReloadSmartUrlListDistributedAction();
                action.Execute();
            }
        }

		public override void Publish()
		{
            SetBinary(); //FIXME
			base.Publish();
			//TODO: SetContentPageRelation();
		}

		public override void CheckIn()
		{
			SetBinary(); //FIXME
			base.CheckIn();
			//TODO: SetContentPageRelation();
		}

		private void SetBinary()
		{
            var x = PersonalizationSettings;
            if (x != null)
                x.GetStream();

			if (this.PageTemplateNode != null)
			{
				this.Binary = PageTemplateManager.GetPageBinaryData(this, this.PageTemplateNode);
			}
		}

        internal bool SmartUrlChanged { get; private set; }

        #region Page properties

		[RepositoryProperty("MetaTitle")]
		public string MetaTitle
		{
			get { return this.GetProperty<string>("MetaTitle"); }
			set { this["MetaTitle"] = value; }
		}

        [RepositoryProperty("Keywords", RepositoryDataType.Text)]
        public string Keywords
        {
			get { return this.GetProperty<string>("Keywords"); }
			set { this["Keywords"] = value; }
		}
		[RepositoryProperty("MetaDescription", RepositoryDataType.Text)]
        public string MetaDescription
        {
			get { return this.GetProperty<string>("MetaDescription"); }
			set { this["MetaDescription"] = value; }
		}

        [RepositoryProperty("MetaAuthors")]
        public string Authors
        {
			get { return this.GetProperty<string>("MetaAuthors"); }
			set { this["MetaAuthors"] = value; }
		}

		[RepositoryProperty("CustomMeta", RepositoryDataType.Text)]
        public string CustomMeta
        {
			get { return this.GetProperty<string>("CustomMeta"); }
			set { this["CustomMeta"] = value; }
		}

		[RepositoryProperty("PageTemplateNode", RepositoryDataType.Reference)]
		public PageTemplate PageTemplateNode
		{
			get { return this.GetReference<PageTemplate>("PageTemplateNode"); }
			set { this.SetReference("PageTemplateNode", value); }
		}

        [RepositoryProperty("Comment")]
        public string Comment
        {
			get { return this.GetProperty<string>("Comment"); }
			set { this["Comment"] = value; }
		}

		[RepositoryProperty("PersonalizationSettings", RepositoryDataType.Binary)]
		public BinaryData PersonalizationSettings
		{
			get { return this.GetBinary("PersonalizationSettings"); }
			set { this.SetBinary("PersonalizationSettings", value); }
		}

        [RepositoryProperty("TemporaryPortletInfo", RepositoryDataType.Text)]
        public string TemporaryPortletInfo
        {
			get { return this.GetProperty<string>("TemporaryPortletInfo"); }
            set
            {
                this["TemporaryPortletInfo"] = value;
                this["HasTemporaryPortletInfo"] = String.IsNullOrEmpty(value) ? 0 : 1;
            }
        }

        [RepositoryProperty("HasTemporaryPortletInfo")]
        private bool HasTemporaryPortletInfo
        {
			get { return this.GetProperty<int>("HasTemporaryPortletInfo") != 0; }
        }

		[RepositoryProperty("TextExtract", RepositoryDataType.Text)]
		public string TextExtract
		{
			get { return this.GetProperty<string>("TextExtract"); }
		}

        [RepositoryProperty("SmartUrl", RepositoryDataType.String)]
        public string SmartUrl
        {
			get { return this.GetProperty<string>("SmartUrl"); }
			set
            {
                if (SmartUrl != value)
                {
                    SmartUrlChanged = true;
                    this["SmartUrl"] = value;
                }
            }
        }

        [RepositoryProperty("PageSkin", RepositoryDataType.Reference)]
        public Node PageSkin
        {
            get { return this.GetReference<Node>("PageSkin"); }
            set { this.SetReference("PageSkin", value); }
        }

        [Obsolete("Use DisplayName instead")]
        public string PageNameInMenu
        {
            get { return this.DisplayName; }
            set { this.DisplayName = value; }
        }

		public Site Site
		{
			get { return Site.GetSiteByNode(this); }
		}

        #endregion

        //#region IFile Members
        //[RepositoryProperty("Binary", RepositoryDataType.Binary)]
        //public virtual BinaryData Binary
        //{
        //    get { return this.GetBinary("Binary"); }
        //    set { this.SetBinary("Binary", value); }
        //}
        //public int Downloads
        //{
        //    //TODO: Download counter is a statistical data rather than a node specific one
        //    get { return 0; }
        //}
        //public long Size
        //{
        //    get { return this.GetBinary("Binary").Size; }
        //}
        //public long FullSize
        //{
        //    get { return this.GetBinary("Binary").Size; }
        //}
        //public void IncreaseDownloads()
        //{
        //    //TODO: Download counter is a statistical data rather than a node specific one
        //}
        //public void RestoreVersion(VersionNumber versionNumber)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}
        //public event EventHandler FileDownloaded;
        //#endregion

        //-------------------------------------------- IFolder Members

        public virtual IEnumerable<Node> Children
        {
            get { return this.GetChildren(); }
        }
        public virtual int ChildCount
        {
            get { return this.GetChildCount(); }
        }

        public override IEnumerable<ContentType> AllowedChildTypes
        {
            get { return base.AllowedChildTypes; }
            set
            {
                if (this.NodeType.Name == "Page")
                    return;
                base.AllowedChildTypes = value;
            }
        }

        //////////////////////////////////////// Static Access ////////////////////////////////////////

        public static Page Current
        {
            get
            {
                return PortalContext.Current.Page;
            }
        }

        #region OfflineExecution

        internal static NodeQueryResult GetAllPage()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["Page"], true));
            return query.Execute();
        }

        public static string[] RunPagesBackground(HttpContext context, out Exception[] exceptions)
        {
            var pages = GetAllPage();
            string[] result = new string[pages.Count];
            List<string> pageList = new List<string>();
            List<Exception> exceptionList = new List<Exception>();

            foreach (Node pageItem in pages.Nodes)
            {
                Exception exc = null;
                Page p = (Page)pageItem;
                if (p != null && p.HasTemporaryPortletInfo)
                {
                    Site site = p.Site;

                    if (site != null)
                    {
                        Page.RunPage(HttpContext.Current, p.Path, p, out exc);
                        pageList.Add(p.Path);
                        
                        if (exc != null)
                            exceptionList.Add(exc);

                    }
                }
            }
            pageList.CopyTo(result, 0);
            exceptions = exceptionList.ToArray();
            return result;
        }

        internal static void RunPage(HttpContext context, string path, Page pageNode, out Exception exception)
        {
            PageBase page = null;
            string virtualPath = string.Empty;

            // prepare repository path
            try
            {
                virtualPath = CreateVirtualPath(path);
                page = InstantiatePage(context, virtualPath, pageNode);
                ExecutePage(context, virtualPath, page, pageNode, false);
                exception = null;
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
                exception = exc;
            }
        }

        static string CreateVirtualPath(string path)
        {
            string virtualPath = VirtualPathUtility.AppendTrailingSlash(path);
            virtualPath = string.Concat(virtualPath, PortalContext.InRepositoryPageSuffix.Substring(1, PortalContext.InRepositoryPageSuffix.Length - 1));  // remove first "/"
            return virtualPath;
        }

        static PageBase InstantiatePage(HttpContext context, string virtualPath, Page pageNode)
        {
            PageBase page = null;

            Page originalCurrentPage = PortalContext.Current.Page;
            string originalPath = context.Request.Path;

            try
            {
                PortalContext.Current.Page = pageNode;
                context.RewritePath(virtualPath);
                page = (PageBase)BuildManager.CreateInstanceFromVirtualPath(virtualPath, typeof(PageBase));
            }
            finally
            {
                context.RewritePath(originalPath);
                PortalContext.Current.Page = originalCurrentPage;
            }

            return page;
        }

        static void ExecutePage(HttpContext context, string virtualPath, PageBase page, Page pageNode, bool silent)
        {
            Page originalCurrentPage = PortalContext.Current.Page;
            string originalPath = context.Request.Path;

            if (originalCurrentPage.Path == pageNode.Path)
                throw new InvalidOperationException("Executing a Page within itself is forbidden!");

            try
            {
                PortalContext.Current.Page = pageNode;
                context.RewritePath(virtualPath);
                context.Server.Execute(page, TextWriter.Null, false);
            }
            catch (Exception e)
            {
                if (!silent)
                    throw e;
                else
                    Logger.WriteException(e);
            }
            finally
            {
                context.RewritePath(originalPath);
                PortalContext.Current.Page = originalCurrentPage;
            }
        }

        #endregion

        #region PortletManagement

        public static WebPartCollection GetPortlets(HttpContext context, Page pageNode)
        {
            WebPartCollection webParts = null;

            string virtualPath = CreateVirtualPath(pageNode.Path);
            PageBase page = InstantiatePage(context, virtualPath, pageNode);

            page.PreLoad += delegate
            {
                webParts = WebPartManager.GetCurrentWebPartManager(page).WebParts;
            };
            
            ExecutePage(context, virtualPath, page, pageNode, true);

            return webParts;
        }

        public WebPartCollection GetPortlets(HttpContext context)
        {
            return GetPortlets(context, this);
        }

        public static XmlDocument GetPersonalizationXml(HttpContext context, Page pageNode)
        {
            XmlDocument xml = new XmlDocument();
            xml.AppendChild(xml.CreateElement("webPartZones"));

            string virtualPath = CreateVirtualPath(pageNode.Path);
            PageBase page = InstantiatePage(context, virtualPath, pageNode);

            page.PreLoad += delegate
            {
                WebPartManager wpm = WebPartManager.GetCurrentWebPartManager(page);
                WebPartZoneCollection webPartZones = wpm.Zones;

                foreach (WebPartZone zone in webPartZones)
                {
                    XmlElement zoneElement = xml.CreateElement("webPartZone");
                    XmlAttribute zoneId = xml.CreateAttribute("id");
                    zoneId.Value = zone.ID;
                    zoneElement.SetAttributeNode(zoneId);
                    xml.DocumentElement.AppendChild(zoneElement);

                    WebPartCollection webParts = zone.WebParts;

                    foreach (WebPart webPart in webParts)
                    {
                        if (!webPart.IsStatic)
                        {
                            XmlDocument xmlFragment = new XmlDocument();
                            using (StringWriter sw = new StringWriter())
                            {
                                using (XmlWriter writer = new XmlTextWriter(sw))
                                {
                                    webPart.ExportMode = WebPartExportMode.All; // Force exporting of all information
                                    wpm.ExportWebPart(webPart, writer);
                                    writer.Flush();
                                }
                                xmlFragment.LoadXml(sw.ToString());
                            }
                            zoneElement.AppendChild(xml.ImportNode(xmlFragment.FirstChild.FirstChild, true));
                        }
                    }
                }
            };

            ExecutePage(context, virtualPath, page, pageNode, true);

            return xml;
        }

        public XmlDocument GetPersonalizationXml(HttpContext context)
        {
            return GetPersonalizationXml(context, this);
        }

        public static void SetPersonalizationFromXml(HttpContext context, Page pageNode, XmlDocument xml, out string errorMessage)
        {
            string error = String.Empty;

            string virtualPath = CreateVirtualPath(pageNode.Path);
            PageBase page = InstantiatePage(context, virtualPath, pageNode);


            page.PreLoad += delegate
            {
                WebPartManager wpm = WebPartManager.GetCurrentWebPartManager(page);
                WebPartZoneCollection webPartZones = wpm.Zones;

                foreach (WebPartZone zone in webPartZones)
                {
                    foreach (WebPart part in zone.WebParts)
                    {
                        if (!part.IsStatic)
                            wpm.DeleteWebPart(part);
                    }

                    XmlNode zoneDescriptionNode = xml.SelectNodes(@"//*[@id='" + zone.ID + "']")[0];

                    XmlElement zoneDescription = zoneDescriptionNode as XmlElement;

                    if (zoneDescription != null)
                    {
                        foreach (XmlElement webPartDescription in zoneDescription.GetElementsByTagName("webPart"))
                        {
                            string webPartXml = String.Empty;

                            using (StringWriter sw = new StringWriter())
                            {
                                using (XmlWriter writer = new XmlTextWriter(sw))
                                {
                                    writer.WriteStartElement("webParts");
                                    webPartDescription.WriteTo(writer);
                                    writer.WriteEndElement();
                                    writer.Flush();
                                }
                                webPartXml = sw.ToString();
                            }

                            using (StringReader sr = new StringReader(webPartXml))
                            {
                                using (XmlReader reader = new XmlTextReader(sr))
                                {
                                    WebPart part = wpm.ImportWebPart(reader, out error);
                                    wpm.AddWebPart(part, zone, zone.WebParts.Count);
                                }
                            }
                        }
                    }
                }
            };

            //TODO: Clever exception handling
            ExecutePage(context, virtualPath, page, pageNode, true);
            errorMessage = error;
        }

        public void SetPersonalizationFromXml(HttpContext context, XmlDocument xml, out string errorMessage)
        {
            SetPersonalizationFromXml(context, this, xml, out errorMessage);
        }

        #endregion


        //================================================================================= Generic Property handling

        public override object GetProperty(string name)
        {
            switch (name)
            {
                case GenericContent.ALLOWEDCHILDTYPES:
                    return this.AllowedChildTypes;
                case "MetaAuthors":
                    return this.Authors;
                case "CustomMeta":
                    return this.CustomMeta;
                case "Comment":
                    return this.Comment;
                case "PageNameInMenu":
                    return this.PageNameInMenu;
                case "Hidden":
                    return this.Hidden;
                case "Keywords":
                    return this.Keywords;
                case "MetaDescription":
                    return this.MetaDescription;
                case "MetaTitle":
                    return this.MetaTitle;
                case "PageTemplateNode":
                    return this.PageTemplateNode;
                case "PersonalizationSettings":
                    return this.PersonalizationSettings;
                case "TemporaryPortletInfo":
                    return this.TemporaryPortletInfo;
                case "HasTemporaryPortletInfo":
                    return this.HasTemporaryPortletInfo;
                case "Site":
                    return this.Site;
                case "PageSkin":
                    return this.PageSkin;
                case "DisplayName":
                    return this.DisplayName;
                default:
                    return base.GetProperty(name);
            }
        }
        public override void SetProperty(string name, object value)
        {
            switch (name)
            {
                case GenericContent.ALLOWEDCHILDTYPES:
                    this.AllowedChildTypes = (IEnumerable<ContentType>)value;
                    break;
                case "MetaAuthors":
                    this.Authors = (string)value;
                    break;
                case "CustomMeta":
                    this.CustomMeta = (string)value;
                    break;
                case "Comment":
                    this.Comment = (string)value;
                    break;
                case "PageNameInMenu":
                    this.PageNameInMenu = (string)value;
                    break;
                case "Hidden":
                    this.Hidden = (bool)value;
                    break;
                case "Keywords":
                    this.Keywords = (string)value;
                    break;
                case "MetaDescription":
                    this.MetaDescription = (string)value;
                    break;
                case "MetaTitle":
                    this.MetaTitle = (string)value;
                    break;
                case "PageTemplateNode":
                    this.PageTemplateNode = (PageTemplate)value;
                    break;
                case "PersonalizationSettings":
                    this.PersonalizationSettings = (BinaryData)value;
                    break;
                case "TemporaryPortletInfo":
                    this.TemporaryPortletInfo = (string)value;
                    break;
                case "SmartUrl":
                    this.SmartUrl = (string)value;
                    break;
                case "PageSkin":
                    this.PageSkin = (Node)value;
                    break;
                case "HasTemporaryPortletInfo":
                    bool? boolValue = value as bool?;
                    if (boolValue != null)
                        base.SetProperty(name, boolValue.Value ? 1 : 0);
                    else
                        base.SetProperty(name, value);
                    break;
                default:
                    base.SetProperty(name, value);
                    break;
            }
        }

    }
}
