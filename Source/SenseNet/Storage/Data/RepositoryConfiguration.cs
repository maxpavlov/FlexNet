using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using SenseNet.ContentRepository.Storage.Search;
using System.Linq;

namespace SenseNet.ContentRepository.Storage.Data
{
    public static class RepositoryConfiguration
    {
        private static readonly string DataProviderClassNameKey = "DataProvider";
        private static readonly string AccessProviderClassNameKey = "AccessProvider";
        private static readonly string SnCrMsSqlConnectrionStringKey = "SnCrMsSql";
        private static readonly string SqlCommandTimeoutKey = "SqlCommandTimeout";
        private static readonly string BackwardCompatibilityDefaultValuesKey = "BackwardCompatibilityDefaultValues";
        private static readonly string BackwardCompatibilityXmlNamespacesKey = "BackwardCompatibilityXmlNamespaces";
        private static readonly string IndexDirectoryPathKey = "IndexDirectoryPath";
        private static readonly string IndexDirectoryBackupPathKey = "IndexDirectoryBackupPath";
        private static readonly string IsOuterSearchEngineEnabledKey = "EnableOuterSearchEngine";
        private static readonly string TraceReportEnabledKey = "EnableTraceReportOnPage";
        private static readonly string DataProviderSectionKey = "sensenet/dataHandling";
        

        private static string _connectionString;
        static int? _sqlCommandTimeout;
        private static bool? _backwardCompatibilityDefaultValues;
        private static bool? _backwardCompatibilityXmlNamespaces;
        private static string _indexDirectoryPath;
        private static string _indexDirectoryBackupPath;
        private static bool? _isOuterSearchEngineEnabled;
        private static bool? _traceReportEnabled;
        

        private static string DefaultConnectionString = "Persist Security Info=False;Initial Catalog=SenseNetContentRepository;Data Source=MySenseNetContentRepositoryDatasource;User ID=SenseNetContentRepository;password=SenseNetContentRepository";
        private static string DefaultDataProviderClassName = "SenseNet.ContentRepository.Storage.Data.SqlClient.SqlProvider";
        private static string DefaultAccessProviderClassName = "SenseNet.ContentRepository.Security.DesktopAccessProvider";
        private static string DefaultIndexDirectoryPath = "..\\LuceneIndex";
        private static string DefaultIndexDirectoryBackupPath = "..\\LuceneIndex_backup";
        private static int DefaultSqlCommandTimeout = 30;
        private static bool DefaultTraceReportEnabledValue = false;
        

        public static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    var setting = ConfigurationManager.ConnectionStrings[SnCrMsSqlConnectrionStringKey];
                    _connectionString = setting == null ? DefaultConnectionString : setting.ConnectionString;
                }
                return _connectionString;
            }
        }
        public static string DataProviderClassName
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[DataProviderClassNameKey];
                return (String.IsNullOrEmpty(setting)) ? DefaultDataProviderClassName : setting;
            }
        }
        public static string AccessProviderClassName
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[AccessProviderClassNameKey];
                return (String.IsNullOrEmpty(setting)) ? DefaultAccessProviderClassName : setting;
            }
        }
        public static bool IsWebEnvironment
        {
            get { return (System.Web.HttpContext.Current != null); }
        }
        internal static int SqlCommandTimeout
        {
            get
            {
                if (!_sqlCommandTimeout.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[SqlCommandTimeoutKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = DefaultSqlCommandTimeout;
                    _sqlCommandTimeout = value;
                }
                return _sqlCommandTimeout.Value;
            }
        }
        public static bool BackwardCompatibilityDefaultValues
        {
            get
            {
                if (_backwardCompatibilityDefaultValues == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[BackwardCompatibilityDefaultValuesKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _backwardCompatibilityDefaultValues = value;
                }
                return _backwardCompatibilityDefaultValues.Value;
            }
        }
        public static bool BackwardCompatibilityXmlNamespaces
        {
            get
            {
                if (_backwardCompatibilityXmlNamespaces == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[BackwardCompatibilityXmlNamespacesKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _backwardCompatibilityXmlNamespaces = value;
                }
                return _backwardCompatibilityXmlNamespaces.Value;
            }
        }
        /// <summary>
        /// Do not use. Use StorageContext.Search.IndexDirectoryPath instead.
        /// </summary>
        internal static string IndexDirectoryPath
        {
            get
            {
                if (_indexDirectoryPath == null)
                {
                    var setting = ConfigurationManager.AppSettings[IndexDirectoryPathKey];
                    var path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").Replace("/", "\\");
                    _indexDirectoryPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path),
                        String.IsNullOrEmpty(setting) ? DefaultIndexDirectoryPath : setting));
                }
                return _indexDirectoryPath;
            }
        }
        /// <summary>
        /// Do not use. Use StorageContext.Search.IndexDirectoryBackupPath instead.
        /// </summary>
        internal static string IndexDirectoryBackupPath
        {
            get
            {
                if (_indexDirectoryBackupPath == null)
                {
                    var setting = ConfigurationManager.AppSettings[IndexDirectoryBackupPathKey];
                    var path = System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Replace("file:///", "").Replace("/", "\\");
                    _indexDirectoryBackupPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path),
                        String.IsNullOrEmpty(setting) ? DefaultIndexDirectoryBackupPath : setting));
                }
                return _indexDirectoryBackupPath;
            }
        }
        /// <summary>
        /// Do not use. Use StorageContext.Search.IsOuterEngineEnabled instead.
        /// </summary>
        internal static bool IsOuterSearchEngineEnabled
        {
            get
            {
                if (_isOuterSearchEngineEnabled == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[IsOuterSearchEngineEnabledKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _isOuterSearchEngineEnabled = value;
                }
                return _isOuterSearchEngineEnabled.Value;
            }
        }

        private static bool? _fullCacheInvalidationOnSave = null;
        public static bool FullCacheInvalidationOnSave
        {
            get
            {
                if (!_fullCacheInvalidationOnSave.HasValue)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings["FullCacheInvalidationOnSave"];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = false;
                    _fullCacheInvalidationOnSave = value;
                }
                return _fullCacheInvalidationOnSave.Value;
            }
        }

        private static int? _permissionCacheTTL;
        public static int PermissionCacheTTL
        {
            get
            {
                if (!_permissionCacheTTL.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings["PermissionCacheTTL"];
                    if (!int.TryParse(setting, out value))
                        value = 0;

                    _permissionCacheTTL = value;
                }

                return _permissionCacheTTL.Value;
            }
        }
        public static bool TraceReportEnabled
        {
            get
            {
                if (_traceReportEnabled == null)
                {
                    bool value;
                    var setting = ConfigurationManager.AppSettings[TraceReportEnabledKey];
                    if (String.IsNullOrEmpty(setting) || !Boolean.TryParse(setting, out value))
                        value = DefaultTraceReportEnabledValue;
                    _traceReportEnabled = value;
                }
                return _traceReportEnabled.Value;
            }
        }

        private static bool? _tracePermissionCheck;
        public static bool TracePermissionCheck
        {
            get
            {
                if (!_tracePermissionCheck.HasValue)
                {
                    var value = false;
                    var settings = ConfigurationManager.GetSection("sensenet/portalSettings") as NameValueCollection;

                    if (settings != null)
                    {
                        var setting = settings["TracePermissionCheck"];
                        if (string.IsNullOrEmpty(setting) || !bool.TryParse(setting, out value))
                            value = false;
                    }

                    _tracePermissionCheck = value;
                }

                return _tracePermissionCheck.Value;
            }
        }

        private const string CACHEDBINARYSIZEKEY = "CachedBinarySize";
        private static int? _cachedBinarySize;
        public static int CachedBinarySize
        {
            get
            {
                if (!_cachedBinarySize.HasValue)
                {
                    var value = 16000;
                    var settings = ConfigurationManager.GetSection(DataProviderSectionKey) as NameValueCollection;

                    if (settings != null)
                    {
                        var setting = settings[CACHEDBINARYSIZEKEY];
                        if (!string.IsNullOrEmpty(setting))
                        {
                            //do not let this value to be set too low
                            if (int.TryParse(setting, out value) && value < 8000)
                                value = 8000;
                        }
                    }

                    _cachedBinarySize = value;
                }

                return _cachedBinarySize.Value;
            }
        }

        public const int AdministratorUserId = 1;
        public const int StartupUserId = -2;
        public const int PortalRootId = 2;
        public const int VisitorUserId = 6;
        public const int AdministratorsGroupId = 7;
        public const int EveryoneGroupId = 8;
        public const int CreatorsGroupId = 9;

        private static int _lastModifiersGroupId;
        private static object _lastModifiersGroupIdSync = new object();
        private static bool _lastModifiersGroupIdIsLoaded;
        public static int LastModifiersGroupId
        {
            get
            {
                if (!_lastModifiersGroupIdIsLoaded)
                {
                    lock (_lastModifiersGroupIdSync)
                    {
                        if (!_lastModifiersGroupIdIsLoaded)
                        {
                            _lastModifiersGroupId = LoadLastModifiersGroupId();
                            _lastModifiersGroupIdIsLoaded = true;
                        }
                    }
                }
                return _lastModifiersGroupId;
            }
        }

        private static int LoadLastModifiersGroupId()
        {
            return DataProvider.Current.LoadLastModifiersGroupId();
        }

        private static string[] _specialGroupNames = new[] { "Everyone", "Creators", "LastModifiers" };
        public static string[] SpecialGroupNames
        {
            get { return _specialGroupNames; }
        }

        //============================================================

        private static readonly string LuceneLockDeleteRetryIntervalKey = "LuceneLockDeleteRetryInterval";
        private static int? _luceneLockDeleteRetryInterval;
        public static int LuceneLockDeleteRetryInterval
        {
            get
            {
                if (_luceneLockDeleteRetryInterval == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[LuceneLockDeleteRetryIntervalKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 60;
                    _luceneLockDeleteRetryInterval = value;
                }
                return _luceneLockDeleteRetryInterval.Value;
            }
        }
        private static readonly string IndexLockFileWaitForRemovedTimeoutKey = "IndexLockFileWaitForRemovedTimeout";
        private static int? _indexLockFileWaitForRemovedTimeout;
        public static int IndexLockFileWaitForRemovedTimeout
        {
            get
            {
                if (!_indexLockFileWaitForRemovedTimeout.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[IndexLockFileWaitForRemovedTimeoutKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 5;
                    _indexLockFileWaitForRemovedTimeout = value;
                }
                return _indexLockFileWaitForRemovedTimeout.Value;
            }
        }
        public static readonly string IndexLockFileRemovedNotificationEmailKey = "IndexLockFileRemovedNotificationEmail";
        public static string IndexLockFileRemovedNotificationEmail
        {
            get { return ConfigurationManager.AppSettings[IndexLockFileRemovedNotificationEmailKey]; }
        }
        public static readonly string NotificationSectionKey = "sensenet/notification";
        public static readonly string NotificationSenderKey = "NotificationSenderAddress";
        public static string NotificationSender
        {
            get 
            {
                var section = ConfigurationManager.GetSection(NotificationSectionKey) as System.Collections.Specialized.NameValueCollection;
                return section[NotificationSenderKey]; 
            }
        }

        

        public static readonly string IndexHealthMonitorRunningPeriodKey = "IndexHealthMonitorRunningPeriod";
        private static int? _indexHealthMonitorRunningPeriod;
        /// <summary>
        /// Periodicity of checking and executing unprocessed indexing tasks in seconds. Default: 300 (5 minutes), minimum: 5
        /// </summary>
        public static int IndexHealthMonitorRunningPeriod
        {
            get
            {
                if (_indexHealthMonitorRunningPeriod == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[IndexHealthMonitorRunningPeriodKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 300;
                    if (value < 5)
                        value = 5;
                    _indexHealthMonitorRunningPeriod = value;
                }
                return _indexHealthMonitorRunningPeriod.Value;
            }
        }


        //============================================================ Working modes

        private static readonly string SpecialWorkingModeKey = "SpecialWorkingMode";
        private static string _specialWorkingMode;
        public static string SpecialWorkingMode
        {
            get
            {
                if (_specialWorkingMode == null)
                {
                    _specialWorkingMode = ConfigurationManager.AppSettings[SpecialWorkingModeKey];
                    if (String.IsNullOrEmpty(_specialWorkingMode))
                        _specialWorkingMode = String.Empty;
                }
                return _specialWorkingMode;
            }
        }

        private static readonly string PopulatingWorkingModeKeyword = "POPULATING";
        private static bool? _workingModeIsPopulating;
        public static bool WorkingModeIsPopulating
        {
            get
            {
                if (_workingModeIsPopulating == null)
                    _workingModeIsPopulating = SpecialWorkingMode.Contains(PopulatingWorkingModeKeyword);
                return _workingModeIsPopulating.Value;
            }
            set
            {
                _workingModeIsPopulating = value;
            }
        }

        private static readonly string IndexBackupCreatorIdKey = "IndexBackupCreatorId";
        private static string _indexBackupCreatorId;
        public static string IndexBackupCreatorId
        {
            get
            {
                if (_indexBackupCreatorId == null)
                {
                    _indexBackupCreatorId = ConfigurationManager.AppSettings[IndexBackupCreatorIdKey];
                    if (String.IsNullOrEmpty(_indexBackupCreatorId))
                        _indexBackupCreatorId = String.Empty;
                }
                return _indexBackupCreatorId;
            }
        }

        //============================================================ Cache Dependency Event Default Partitions

        private static readonly string NodeIdDependencyEventPartitionsKey = "NodeIdDependencyEventPartitions";
        private static int? _nodeIdDependencyEventPartitions;
        public static int NodeIdDependencyEventPartitions
        {
            get
            {
                if (_nodeIdDependencyEventPartitions == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[NodeIdDependencyEventPartitionsKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 0;
                    _nodeIdDependencyEventPartitions = value;
                }
                return _nodeIdDependencyEventPartitions.Value;
            }
        }

        private static readonly string NodeTypeDependencyEventPartitionsKey = "NodeTypeDependencyEventPartitions";
        private static int? _nodeTypeDependencyEventPartitions;
        public static int NodeTypeDependencyEventPartitions
        {
            get
            {
                if (_nodeTypeDependencyEventPartitions == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[NodeTypeDependencyEventPartitionsKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 0;
                    _nodeTypeDependencyEventPartitions = value;
                }
                return _nodeTypeDependencyEventPartitions.Value;
            }
        }

        private static readonly string PathDependencyEventPartitionsKey = "PathDependencyEventPartitions";
        private static int? _pathDependencyEventPartitions;
        public static int PathDependencyEventPartitions
        {
            get
            {
                if (_pathDependencyEventPartitions == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[PathDependencyEventPartitionsKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 0;
                    _pathDependencyEventPartitions = value;
                }
                return _pathDependencyEventPartitions.Value;
            }
        }

        private static readonly string PortletDependencyEventPartitionsKey = "PortletDependencyEventPartitions";
        private static int? _portletDependencyEventPartitions;
        public static int PortletDependencyEventPartitions
        {
            get
            {
                if (_portletDependencyEventPartitions == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[PortletDependencyEventPartitionsKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 0;
                    _portletDependencyEventPartitions = value;
                }
                return _portletDependencyEventPartitions.Value;
            }
        }
        
        //============================================================ MSMQ

        private static readonly string MessageQueueNameKey = "MsmqChannelQueueName";
        private static string _messageQueueName;
        public static string MessageQueueName
        {
            get
            {
                if (_messageQueueName == null)
                {
                    _messageQueueName = ConfigurationManager.AppSettings[MessageQueueNameKey];
                    if (String.IsNullOrEmpty(_messageQueueName))
                        _messageQueueName = String.Empty;
                }
                return _messageQueueName;
            }
        }
        
        private static readonly string MessageRetentionTimeKey = "MessageRetentionTime";
        private static int? _messageRetentionTime;
        /// <summary>
        /// Retention time of the messages in the message queue in seconds. Default: 10, minimum: 2
        /// </summary>
        public static int MessageRetentionTime
        {
            get
            {
                if (_messageRetentionTime == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[MessageRetentionTimeKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 10;
                    if (value < 2)
                        value = 2;
                    _messageRetentionTime = value;
                }
                return _messageRetentionTime.Value;
            }
        }


        private static readonly string MsmqReconnectDelayKey = "MsmqReconnectDelay";
        private static int DefaultMsmqReconnectDelay = 30;
        private static int? _msmqReconnectDelay;
        /// <summary>
        /// MsmqReconnectDelay defines the time interval between reconnect attempts (in seconds).  Default value: 30 sec.
        /// </summary>
        internal static int MsmqReconnectDelay
        {
            get
            {
                if (!_msmqReconnectDelay.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[MsmqReconnectDelayKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = DefaultMsmqReconnectDelay;
                    _msmqReconnectDelay = value * 1000;
                }
                return _msmqReconnectDelay.Value;
            }
        }

        private static readonly string PasswordHistoryFieldMaxLengthKey = "PasswordHistoryFieldMaxLength";
        private static int? _passwordHistoryFieldMaxLength;
        public static int PasswordHistoryFieldMaxLength
        {
            get
            {
                if (_passwordHistoryFieldMaxLength == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[PasswordHistoryFieldMaxLengthKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 10;
                    _passwordHistoryFieldMaxLength = value;
                }
                return _passwordHistoryFieldMaxLength.Value;
            }
        }

    }
}