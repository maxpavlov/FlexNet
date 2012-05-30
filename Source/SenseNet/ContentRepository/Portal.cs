//using System;
//using System.Collections.Specialized;
//using System.Configuration;
//using SenseNet.ContentRepository.Storage;
//using SenseNet.ContentRepository.Storage.Security;
//using ConfigurationException=SenseNet.ContentRepository.Storage.Data.ConfigurationException;
//using System.Web.Configuration;

//namespace SenseNet.ContentRepository
//{
//    /// <summary>
//    /// This Class represents the portal itself.
//    /// </summary>
//    public static class Repository
//    {
//        //========================================================================= Constants

//        public static readonly string RootName = "Root";
//        public static readonly string SystemFolderName = "System";
//        public static readonly string SchemaFolderName = "Schema";
//        public static readonly string ContentTypesFolderName = "ContentTypes";

//        public static readonly string RootPath = String.Concat("/", RootName);
//        public static readonly string SystemFolderPath = RepositoryPath.Combine(RootPath, SystemFolderName);
//        public static readonly string SchemaFolderPath = RepositoryPath.Combine(SystemFolderPath, SchemaFolderName);
//        public static readonly string ContentTypesFolderPath = RepositoryPath.Combine(SchemaFolderPath, ContentTypesFolderName);

//        //public static readonly string BinFolderName = "Bin";
//        //public static readonly string ContentViewsFolderName = "ContentViews";
//        //public static readonly string ImsFolderName = "IMS";
//        //public static readonly string PageTemplatesFolderName = "PageTemplates";
//        //public static readonly string ResourceFolderName = "Resources";
//        //public static readonly string fieldControlTemplatesFolderName = "FieldControlTemplates";
		
//        //public static readonly string BinFolderPath = RepositoryPath.Combine(SystemFolderPath, BinFolderName);
//        //public static readonly string ContentViewsFolderPath = RepositoryPath.Combine(SystemFolderPath, ContentViewsFolderName);
//        //public static readonly string ImsFolderPath = RepositoryPath.Combine(RootPath, ImsFolderName);
//        //public static readonly string PageTemplatesFolderPath = RepositoryPath.Combine(SystemFolderPath, PageTemplatesFolderName);
//        //public static readonly string ResourceFolderPath = RepositoryPath.Combine(SystemFolderPath, ResourceFolderName);
//        //public static readonly string FieldControlTemplatesPath = RepositoryPath.Combine(SystemFolderPath,fieldControlTemplatesFolderName);

//        public static string FieldControlTemplatesPath
//        {
//            get { return WebConfigurationManager.AppSettings["FieldControlTemplatesPath"]; }
//        }

//        public static string ContentViewGlobalFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["ContentViewGlobalFolderPath"]; }
//        }

//        public static string ContentViewFolderName
//        {
//            get { return WebConfigurationManager.AppSettings["ContentViewFolderName"]; }
//        }

//        public static string ContentTemplateFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["ContentTemplateFolderPath"]; }
//        }

//        public static string ImsFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["IMSFolderPath"]; }
//        }

//        public static string PageTemplatesFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["PageTemplateFolderPath"]; }
//        }

//        public static string ResourceFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["ResourceFolderPath"]; }
//        }

//        public static string SkinRootFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["SkinRootFolderPath"]; }
//        }

//        public static string SkinGlobalFolderPath
//        {
//            get { return WebConfigurationManager.AppSettings["SkinGlobalFolderPath"]; }
//        }

//        //========================================================================= Properties

//        public static Folder SkinRootFolder
//        {
//            get { return (Folder)Node.LoadNode(SkinRootFolderPath); }
//        }

//        public static Folder SkinGlobalFolder
//        {
//            get { return (Folder)Node.LoadNode(SkinGlobalFolderPath); }
//        }

//        /// <summary>
//        /// Gets the root Node.
//        /// </summary>
//        /// <value>The root Node.</value>
//        public static PortalRoot Root
//        {
//            get
//            {
//                return _root;
//            }
//        }
//        public static Folder SystemFolder
//        {
//            get { return (Folder)Node.LoadNode(SystemFolderPath); }
//        }
//        //public static Folder BinFolder
//        //{
//        //    get { return (Folder)Node.LoadNode(BinFolderPath); }
//        //}
//        public static Folder SchemaFolder
//        {
//            get { return (Folder)Node.LoadNode(SchemaFolderPath); }
//        }
//        public static Folder ContentTypesFolder
//        {
//            get { return (Folder)Node.LoadNode(ContentTypesFolderPath); }
//        }
//        public static Folder ImsFolder
//        {
//            get { return (Folder)Node.LoadNode(ImsFolderPath); }
//        }
//        public static Folder PageTemplatesFolder
//        {
//            get { return (Folder)Node.LoadNode(PageTemplatesFolderPath); }
//        }
//        private static bool? _crawlerStart = null;
//        private static bool CrawlerStart
//        {
//            get
//            {
//                if (_crawlerStart == null)
//                {
//                    string crawlerStart = System.Web.Configuration.WebConfigurationManager.AppSettings["CrawlerStart"];
//                    if (!string.IsNullOrEmpty(crawlerStart))
//                    {
//                        bool start = false;
//                        bool.TryParse(crawlerStart, out start);
//                        _crawlerStart = start;
//                        return _crawlerStart == true;
//                    }
//                }
//                return _crawlerStart == true;
//            }
//        }
        
        

//        private static string IsGlobalTemplateEnabledKey = "GlobaFieldControlTemplateEnabled";
//        private static bool? _isGlobalTemplateEnabled;
//        public static bool IsGlobalTemplateEnabled
//        {
//            get
//            {
//                if (_isGlobalTemplateEnabled == null)
//                {
//                    bool value;
//                    var setting = ConfigurationManager.AppSettings[IsGlobalTemplateEnabledKey];
//                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
//                        value = false;
//                    _isGlobalTemplateEnabled = value;
//                }
//                return _isGlobalTemplateEnabled.Value;
//            }
//        }

//        private static string[] _editSourceExtensions;
//        private static object _editSourceLocker = new object();
//        public static string[] EditSourceExtensions
//        {
//            get
//            {
//                if (_editSourceExtensions == null)
//                {
//                    lock (_editSourceLocker)
//                    {
//                        if (_editSourceExtensions == null)
//                        {
//                            var settings = ConfigurationManager.GetSection("sensenet/portalSettings") as NameValueCollection;
//                            if (settings != null)
//                            {
//                                var extensions = settings["EditSourceExtensions"];
//                                if (!string.IsNullOrEmpty(extensions))
//                                {
//                                    _editSourceExtensions = extensions.Split(new [] {';', ','}, StringSplitOptions.RemoveEmptyEntries);
//                                }
//                            }

//                            //not found in config
//                            if (_editSourceExtensions == null)
//                                _editSourceExtensions = new string[0];
//                        }
//                    }
//                }

//                return _editSourceExtensions;
//            }
//        }

//        private static readonly PortalRoot _root;
//        static Repository()
//        {
//            AccessProvider.ChangeToSystemAccount();
//            _root = (PortalRoot)Node.LoadNode(RootPath);
//            AccessProvider.RestoreOriginalUser();

//            //TODO: RENAMEPROJECT: CrawlerStart (NodeObserver?)
//            //if (CrawlerStart)
//            //    Crawler.CrawlerManager.StartCrawler();
//        }

//        //========================================================================= Methods

//        /// <summary>
//        /// Initializes this instance. If you call this method the assembly will be loaded.
//        /// </summary>
//        public static void Initialize()
//        {
//        }

//        //========================================================================= 

//        /// <summary>
//        /// Portals made easy.
//        /// </summary>
//        public static void Make(bool easy)
//        {
//            if (!easy)
//                throw new ApplicationException("Portals can only be made easy.");
//        }


//    }
//}