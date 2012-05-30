using System;
using System.Collections.Specialized;
using System.Configuration;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Web.Configuration;

namespace SenseNet.ContentRepository
{
    public static class Repository
    {
        /// <summary>
        /// Executes the default boot sequence of the Repository.
        /// </summary>
        /// <example>
        /// Use the following code in your tool or other outer application:
        /// <code>
        /// using (Repository.Start())
        /// {
        ///     // your code
        /// }
        /// </code>
        /// </example>
        /// <remarks>
        /// Repository will be stopped if the returned <see cref="RepositoryStartSettings"/> instance is disposed.
        /// </remarks>
        /// <returns>A new IDisposable <see cref="RepositoryInstance"/> instance.</returns>
        public static RepositoryInstance Start()
        {
            return Start(RepositoryStartSettings.Default);
        }
        /// <summary>
        /// Executes the boot sequence of the Repository by the passed <see cref="RepositoryStartSettings"/>.
        /// </summary>
        /// <example>
        /// Use the following code in your tool or other outer application:
        /// <code>
        /// var startSettings = new RepositoryStartSettings
        /// {
        ///     PluginsPath = pluginsPath, // Local directory path of plugins if it is different from your tool's path.
        ///     Console = Console.Out      // Startup sequence will be traced to given writer.
        /// };
        /// using (SenseNet.ContentRepository.Repository.Start(startSettings))
        /// {
        ///     // your code
        /// }
        /// </code>
        /// </example>
        /// <remarks>
        /// Repository will be stopped if the returned <see cref="RepositoryStartSettings"/> instance is disposed.
        /// </remarks>
        /// <returns>A new IDisposable <see cref="RepositoryInstance"/> instance.</returns>
        /// <returns></returns>
        public static RepositoryInstance Start(RepositoryStartSettings settings)
        {
            var instance = RepositoryInstance.Start(settings);
            AccessProvider.ChangeToSystemAccount();
            Repository._root = (PortalRoot)Node.LoadNode(RootPath);
            AccessProvider.RestoreOriginalUser();
            return instance;
        }
        /// <summary>
        /// Returns the running state of the Repository.
        /// </summary>
        /// <returns>True if the Repository has started yet otherwise false.</returns>
        public static bool Started()
        {
            return RepositoryInstance.Started();
        }
        /// <summary>
        /// Stops all internal services of the Repository.
        /// </summary>
        public static void Shutdown()
        {
            RepositoryInstance.Shutdown();
        }

        //========================================================================= Constants

        public static readonly string RootName = "Root";
        public static readonly string SystemFolderName = "System";
        public static readonly string SchemaFolderName = "Schema";
        public static readonly string ContentTypesFolderName = "ContentTypes";
        public static readonly string ContentTemplatesFolderName = "ContentTemplates";

        public static readonly string RootPath = String.Concat("/", RootName);
        public static readonly string SystemFolderPath = RepositoryPath.Combine(RootPath, SystemFolderName);
        public static readonly string SchemaFolderPath = RepositoryPath.Combine(SystemFolderPath, SchemaFolderName);
        public static readonly string ContentTypesFolderPath = RepositoryPath.Combine(SchemaFolderPath, ContentTypesFolderName);

        public static readonly string PORTALSECTIONKEY = "sensenet/portalSettings";

        //public static readonly string BinFolderName = "Bin";
        //public static readonly string ContentViewsFolderName = "ContentViews";
        //public static readonly string ImsFolderName = "IMS";
        //public static readonly string PageTemplatesFolderName = "PageTemplates";
        //public static readonly string ResourceFolderName = "Resources";
        //public static readonly string fieldControlTemplatesFolderName = "FieldControlTemplates";

        //public static readonly string BinFolderPath = RepositoryPath.Combine(SystemFolderPath, BinFolderName);
        //public static readonly string ContentViewsFolderPath = RepositoryPath.Combine(SystemFolderPath, ContentViewsFolderName);
        //public static readonly string ImsFolderPath = RepositoryPath.Combine(RootPath, ImsFolderName);
        //public static readonly string PageTemplatesFolderPath = RepositoryPath.Combine(SystemFolderPath, PageTemplatesFolderName);
        //public static readonly string ResourceFolderPath = RepositoryPath.Combine(SystemFolderPath, ResourceFolderName);
        //public static readonly string FieldControlTemplatesPath = RepositoryPath.Combine(SystemFolderPath,fieldControlTemplatesFolderName);

        public static string FieldControlTemplatesPath
        {
            get { return WebConfigurationManager.AppSettings["FieldControlTemplatesPath"]; }
        }

        public static string CellTemplatesPath
        {
            get { return ConfigurationManager.AppSettings["CellTemplatesPath"]; }
        }

        public static string ContentViewGlobalFolderPath
        {
            get { return WebConfigurationManager.AppSettings["ContentViewGlobalFolderPath"]; }
        }

        public static string ContentViewFolderName
        {
            get { return WebConfigurationManager.AppSettings["ContentViewFolderName"]; }
        }

        public static string ContentTemplateFolderPath
        {
            get { return WebConfigurationManager.AppSettings["ContentTemplateFolderPath"]; }
        }

        public static string ImsFolderPath
        {
            get { return WebConfigurationManager.AppSettings["IMSFolderPath"]; }
        }

        public static string PageTemplatesFolderPath
        {
            get { return WebConfigurationManager.AppSettings["PageTemplateFolderPath"]; }
        }

        public static string ResourceFolderPath
        {
            get { return WebConfigurationManager.AppSettings["ResourceFolderPath"]; }
        }

        public static string SkinRootFolderPath
        {
            get { return WebConfigurationManager.AppSettings["SkinRootFolderPath"]; }
        }

        public static string SkinGlobalFolderPath
        {
            get { return WebConfigurationManager.AppSettings["SkinGlobalFolderPath"]; }
        }

        public static string WorkflowDefinitionPath
        {
            get { return "/Root/System/Workflows/"; }
            //get { return WebConfigurationManager.AppSettings["WorkflowDefinitionPath"]; }
        }

        public static string UserProfilePath
        {
            get { return "/Root/Profiles"; }
            //get { return WebConfigurationManager.AppSettings["UserProfilePath"]; }
        }

        public static string LocalGroupsFolderName
        {
            get { return "Groups"; }
        }

        //========================================================================= Properties

        public static Folder SkinRootFolder
        {
            get { return (Folder)Node.LoadNode(SkinRootFolderPath); }
        }

        public static Folder SkinGlobalFolder
        {
            get { return (Folder)Node.LoadNode(SkinGlobalFolderPath); }
        }

        /// <summary>
        /// Gets the root Node.
        /// </summary>
        /// <value>The root Node.</value>
        public static PortalRoot Root
        {
            get
            {
                return _root;
            }
        }
        public static Folder SystemFolder
        {
            get { return (Folder)Node.LoadNode(SystemFolderPath); }
        }
        public static Folder SchemaFolder
        {
            get { return (Folder)Node.LoadNode(SchemaFolderPath); }
        }
        public static Folder ContentTypesFolder
        {
            get { return (Folder)Node.LoadNode(ContentTypesFolderPath); }
        }
        public static Folder ImsFolder
        {
            get { return (Folder)Node.LoadNode(ImsFolderPath); }
        }
        public static Folder PageTemplatesFolder
        {
            get { return (Folder)Node.LoadNode(PageTemplatesFolderPath); }
        }
        private static bool? _crawlerStart = null;
        private static bool CrawlerStart
        {
            get
            {
                if (_crawlerStart == null)
                {
                    string crawlerStart = System.Web.Configuration.WebConfigurationManager.AppSettings["CrawlerStart"];
                    if (!string.IsNullOrEmpty(crawlerStart))
                    {
                        bool start = false;
                        bool.TryParse(crawlerStart, out start);
                        _crawlerStart = start;
                        return _crawlerStart == true;
                    }
                }
                return _crawlerStart == true;
            }
        }

        private static bool? _userProfilesEnabled;
        public static bool UserProfilesEnabled
        {
            get
            {
                if (!_userProfilesEnabled.HasValue)
                    _userProfilesEnabled = GetBooleanConfigValue("UserProfilesEnabled", false);

                return _userProfilesEnabled.Value;
            }
        }

        private static bool? _downloadCounterEnabled;
        public static bool DownloadCounterEnabled
        {
            get
            {
                if (!_downloadCounterEnabled.HasValue)
                    _downloadCounterEnabled = GetBooleanConfigValue("DownloadCounterEnabled", false);

                return _downloadCounterEnabled.Value;
            }
        }

        public static CheckInCommentsMode CheckInCommentsMode
        {
            get
            {
                var settings = ConfigurationManager.GetSection("sensenet/portalSettings") as NameValueCollection;
                if (settings != null)
                {
                    var cic = settings["CheckInComments"];
                    if (!string.IsNullOrEmpty(cic))
                        return (CheckInCommentsMode)Enum.Parse(typeof(CheckInCommentsMode), cic);
                }

                return CheckInCommentsMode.Recommended;
            }
        }

        private static string IsGlobalTemplateEnabledKey = "GlobaFieldControlTemplateEnabled";
        private static bool? _isGlobalTemplateEnabled;
        public static bool IsGlobalTemplateEnabled
        {
            get
            {
                if (_isGlobalTemplateEnabled == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[IsGlobalTemplateEnabledKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _isGlobalTemplateEnabled = value;
                }
                return _isGlobalTemplateEnabled.Value;
            }
        }

        private static string SkipBinaryImportKey = "SkipBinaryImportIfFileDoesNotExist";
        private static bool? _skipBinaryImport;
        public static bool SkipBinaryImport
        {
            get
            {
                if (_skipBinaryImport == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[SkipBinaryImportKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _skipBinaryImport = value;
                }
                return _skipBinaryImport.Value;
            }
        }

        private static string SkipImportingMissingReferencesKey = "SkipImportingMissingReferences";
        private static bool? _skipImportingMissingReferences;
        public static bool SkipImportingMissingReferences
        {
            get
            {
                if (_skipImportingMissingReferences == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[SkipImportingMissingReferencesKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _skipImportingMissingReferences = value;
                }
                return _skipImportingMissingReferences.Value;
            }
        }

        private static string[] _skipReferenceNames = new [] { "CreatedBy", "ModifiedBy" };
        public static string[] SkipReferenceNames
        {
            get { return _skipReferenceNames; }
        }

        private static string[] _editSourceExtensions;
        private static object _editSourceLocker = new object();
        public static string[] EditSourceExtensions
        {
            get
            {
                if (_editSourceExtensions == null)
                {
                    lock (_editSourceLocker)
                    {
                        if (_editSourceExtensions == null)
                        {
                            var settings = ConfigurationManager.GetSection("sensenet/portalSettings") as NameValueCollection;
                            if (settings != null)
                            {
                                var extensions = settings["EditSourceExtensions"];
                                if (!string.IsNullOrEmpty(extensions))
                                {
                                    _editSourceExtensions = extensions.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                }
                            }

                            //not found in config
                            if (_editSourceExtensions == null)
                                _editSourceExtensions = new string[0];
                        }
                    }
                }

                return _editSourceExtensions;
            }
        }

        private static string[] _webdavExtensions;
        private static object _webdavExtensionsLocker = new object();
        public static string[] WebdavEditExtensions
        {
            get
            {
                if (_webdavExtensions == null)
                {
                    lock (_webdavExtensionsLocker)
                    {
                        if (_webdavExtensions == null)
                        {
                            var settings = ConfigurationManager.GetSection("sensenet/portalSettings") as NameValueCollection;
                            if (settings != null)
                            {
                                var extensions = settings["WebdavEditExtensions"];
                                if (!string.IsNullOrEmpty(extensions))
                                {
                                    _webdavExtensions = extensions.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                                }
                            }

                            //not found in config
                            if (_webdavExtensions == null)
                                _webdavExtensions = new[] { ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".csv" }; 
                        }
                    }
                }

                return _webdavExtensions;
            }
        }

        private static PortalRoot _root;

        private static bool GetBooleanConfigValue(string key, bool defaultValue)
        {
            var result = defaultValue;
            var settings = ConfigurationManager.GetSection(PORTALSECTIONKEY) as NameValueCollection;
            if (settings != null)
            {
                var configString = settings[key];
                if (!string.IsNullOrEmpty(configString))
                {
                    bool configVal;
                    if (bool.TryParse(configString, out configVal))
                        result = configVal;
                }
            }

            return result;
        }
    }
}
