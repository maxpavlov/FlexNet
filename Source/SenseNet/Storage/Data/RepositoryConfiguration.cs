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
        public static int SqlCommandTimeout
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

        private static int? _luceneMergeFactor;
        private static object _luceneMergeFactorSync = new object();
        private static readonly string LuceneMergeFactorKey = "LuceneMergeFactor";        
        public static int LuceneMergeFactor
        {
            get
            {
                if (!_luceneMergeFactor.HasValue)
                {
                    lock (_luceneMergeFactorSync)
                    {
                        if (!_luceneMergeFactor.HasValue)
                        {
                            int value;
                            var setting = ConfigurationManager.AppSettings[LuceneMergeFactorKey];
                            if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                                value = 10;
                            _luceneMergeFactor = value;
                        }
                    }
                }
                return _luceneMergeFactor.Value;
            }
        }

        private static double? _luceneRAMBufferSizeMB;
        private static object _luceneRAMBufferSizeMBSync = new object();
        private static readonly string LuceneRAMBufferSizeMBKey = "LuceneRAMBufferSizeMB";
        public static double LuceneRAMBufferSizeMB
        {
            get
            {
                if (!_luceneRAMBufferSizeMB.HasValue)
                {
                    lock (_luceneRAMBufferSizeMBSync)
                    {
                        if (!_luceneRAMBufferSizeMB.HasValue)
                        {
                            double value;
                            var setting = ConfigurationManager.AppSettings[LuceneRAMBufferSizeMBKey];
                            if (String.IsNullOrEmpty(setting) || !double.TryParse(setting, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
                                value = 16.0;
                            _luceneRAMBufferSizeMB = value;
                        }
                    }
                }
                return _luceneRAMBufferSizeMB.Value;
            }
        }

        private static int? _luceneMaxMergeDocs;
        private static object _luceneMaxMergeDocsSync = new object();
        private static readonly string LuceneMaxMergeDocsKey = "LuceneMaxMergeDocs";
        public static int LuceneMaxMergeDocs
        {
            get
            {
                if (!_luceneMaxMergeDocs.HasValue)
                {
                    lock (_luceneMaxMergeDocsSync)
                    {
                        if (!_luceneMaxMergeDocs.HasValue)
                        {
                            int value;
                            var setting = ConfigurationManager.AppSettings[LuceneMaxMergeDocsKey];
                            if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                                value = Int32.MaxValue;
                            _luceneMaxMergeDocs = value;
                        }
                    }
                }
                return _luceneMaxMergeDocs.Value;
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

        //this was a dynamic property before, now it is 
        //hardcoded for performance reasons
        public static readonly int LastModifiersGroupId = 10;

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
        /// Periodicity of executing the lost indexing tasks in seconds. Default: 60 (1 minutes), minimum: 1
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
                        value = 60;
                    if (value < 1)
                        value = 1;
                    _indexHealthMonitorRunningPeriod = value;
                }
                return _indexHealthMonitorRunningPeriod.Value;
            }
        }

        public static readonly string IndexHealthMonitorGapPartitionsKey = "IndexHealthMonitorGapPartitions";
        private static int? _indexHealthMonitorGapPartitions;
        /// <summary>
        /// Rotating partition count of the lost indexing task container. Default: 10, minimum 2
        /// </summary>
        public static int IndexHealthMonitorGapPartitions
        {
            get
            {
                if (_indexHealthMonitorGapPartitions == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[IndexHealthMonitorGapPartitionsKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 10;
                    if (value < 2)
                        value = 2;
                    _indexHealthMonitorGapPartitions = value;
                }
                return _indexHealthMonitorGapPartitions.Value;
            }
        }

        public static readonly string IndexHistoryItemLimitKey = "IndexHistoryItemLimit";
        private static int? _indexHistoryItemLimit;
        /// <summary>
        /// Max number of cached items in indexing history. Default is 1000000.
        /// </summary>
        public static int IndexHistoryItemLimit
        {
            get
            {
                if (_indexHistoryItemLimit == null)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[IndexHistoryItemLimitKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 1000000;
                    _indexHistoryItemLimit = value;
                }
                return _indexHistoryItemLimit.Value;
            }
        }

        public static readonly string CommitDelayInSecondsKey = "CommitDelayInSeconds";
        private static double? _commitDelayInSeconds;
        public static double CommitDelayInSeconds
        {
            get
            {
                if (!_commitDelayInSeconds.HasValue)
                {
                    double value;
                    var setting = ConfigurationManager.AppSettings[CommitDelayInSecondsKey];
                    if (String.IsNullOrEmpty(setting) || !double.TryParse(setting, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out value))
                        value = 2;
                    _commitDelayInSeconds = value;
                }
                return _commitDelayInSeconds.Value;
            }
        }

        public static readonly string DelayedCommitCycleMaxCountKey = "DelayedCommitCycleMaxCount";
        private static int? _delayedCommitCycleMaxCount;
        public static int DelayedCommitCycleMaxCount
        {
            get
            {
                if (!_delayedCommitCycleMaxCount.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[DelayedCommitCycleMaxCountKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 10;
                    _delayedCommitCycleMaxCount = value;
                }
                return _delayedCommitCycleMaxCount.Value;
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

        public enum CacheContentAfterSaveOption
        {
            None = 0,
            Containers,
            All
        }
        private static readonly string CacheContentAfterSaveModeKey = "CacheContentAfterSaveMode";
        private static CacheContentAfterSaveOption? _cacheContentAfterSaveMode;
        public static CacheContentAfterSaveOption CacheContentAfterSaveMode
        {
            get
            {
                if (!_cacheContentAfterSaveMode.HasValue)
                {
                    CacheContentAfterSaveOption value;
                    var setting = ConfigurationManager.AppSettings[CacheContentAfterSaveModeKey];
                    if (String.IsNullOrEmpty(setting) || !Enum.TryParse<CacheContentAfterSaveOption>(setting, out value))
                        value = CacheContentAfterSaveOption.All;
                    _cacheContentAfterSaveMode = value;
                }
                return _cacheContentAfterSaveMode.Value;
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

        private static readonly string MessageProcessorThreadCountKey = "MessageProcessorThreadCount";
        private static int? _messageProcessorThreadCount;
        /// <summary>
        /// Number of clusterchannel message processor threads. Default is 5.
        /// </summary>
        public static int MessageProcessorThreadCount
        {
            get
            {
                if (!_messageProcessorThreadCount.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[MessageProcessorThreadCountKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 5;
                    _messageProcessorThreadCount = value;
                }
                return _messageProcessorThreadCount.Value;
            }
        }

        private static readonly string MessageProcessorThreadMaxMessagesKey = "MessageProcessorThreadMaxMessages";
        private static int? _messageProcessorThreadMaxMessages;
        /// <summary>
        /// Max number of messages processed by a single clusterchannel message processor thread. Default is 100.
        /// </summary>
        public static int MessageProcessorThreadMaxMessages
        {
            get
            {
                if (!_messageProcessorThreadMaxMessages.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[MessageProcessorThreadMaxMessagesKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 100;
                    _messageProcessorThreadMaxMessages = value;
                }
                return _messageProcessorThreadMaxMessages.Value;
            }
        }

        private static readonly string DelayRequestsOnHighMessageCountUpperLimitKey = "DelayRequestsOnHighMessageCountUpperLimit";
        private static int? _delayRequestsOnHighMessageCountUpperLimit;
        /// <summary>
        /// Number of messages in process queue to trigger delaying of incoming requests. Default is 1000.
        /// </summary>
        public static int DelayRequestsOnHighMessageCountUpperLimit
        {
            get
            {
                if (!_delayRequestsOnHighMessageCountUpperLimit.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[DelayRequestsOnHighMessageCountUpperLimitKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 1000;
                    _delayRequestsOnHighMessageCountUpperLimit = value;
                }
                return _delayRequestsOnHighMessageCountUpperLimit.Value;
            }
        }

        private static readonly string DelayRequestsOnHighMessageCountLowerLimitKey = "DelayRequestsOnHighMessageCountLowerLimit";
        private static int? _delayRequestsOnHighMessageCountLowerLimit;
        /// <summary>
        /// Number of messages in process queue to switch off delaying of incoming requests. Default is 500.
        /// </summary>
        public static int DelayRequestsOnHighMessageCountLowerLimit
        {
            get
            {
                if (!_delayRequestsOnHighMessageCountLowerLimit.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[DelayRequestsOnHighMessageCountLowerLimitKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 500;
                    _delayRequestsOnHighMessageCountLowerLimit = value;
                }
                return _delayRequestsOnHighMessageCountLowerLimit.Value;
            }
        }

        private static readonly string MsmqIndexDocumentSizeLimitKey = "MsmqIndexDocumentSizeLimit";
        private static int? _msmqIndexDocumentSizeLimit;
        /// <summary>
        /// Max size (in bytes) of indexdocument that can be sent over MSMQ. Default is 2000000. Larger indexdocuments will be retrieved from db. 
        /// </summary>
        public static int MsmqIndexDocumentSizeLimit
        {
            get
            {
                if (!_msmqIndexDocumentSizeLimit.HasValue)
                {
                    int value;
                    var setting = ConfigurationManager.AppSettings[MsmqIndexDocumentSizeLimitKey];
                    if (String.IsNullOrEmpty(setting) || !Int32.TryParse(setting, out value))
                        value = 2000000;
                    _msmqIndexDocumentSizeLimit = value;
                }
                return _msmqIndexDocumentSizeLimit.Value;
            }
        }

        //============================================================ PasswordField

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

        //============================================================ Diagnostics

        private static bool? _countersEnabled;
        public static bool PerformanceCountersEnabled
        {
            get
            {
                if (!_countersEnabled.HasValue)
                    _countersEnabled = GetBooleanConfigValue("PerformanceCountersEnabled", true);

                return _countersEnabled.Value;
            }
        }

        private static CounterCreationDataCollection _customPerformanceCounters;
        public static CounterCreationDataCollection CustomPerformanceCounters
        {
            get
            {
                if (_customPerformanceCounters == null)
                {
                    var counterNames = GetStringArrayConfigValue("CustomPerformanceCounters");

                    _customPerformanceCounters = new CounterCreationDataCollection(counterNames.Distinct().Select(cn => new CounterCreationData
                    {
                        CounterType = PerformanceCounterType.NumberOfItems32,
                        CounterName = cn
                    }).ToArray());
                }

                return _customPerformanceCounters;
            }
        }

        //============================================================ Security

        public static readonly string BuiltInDomainName = "BuiltIn";
        private static readonly string DefaultDomainKey = "DefaultDomain";
        private static string _defaultDomain;
        public static string DefaultDomain
        {
            get
            {
                if (_defaultDomain == null)
                {
                    _defaultDomain = ConfigurationManager.AppSettings[DefaultDomainKey];
                    if (string.IsNullOrEmpty(_defaultDomain))
                        _defaultDomain = BuiltInDomainName;
                }
                return _defaultDomain;
            }
        }

        //============================================================ Helper methods

        private static bool GetBooleanConfigValue(string key, bool defaultValue)
        {
            var result = defaultValue;
            //var settings = ConfigurationManager.GetSection(SECTIONKEY) as NameValueCollection;
            //if (settings != null)
            //{
            var configString = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configString))
            {
                bool configVal;
                if (bool.TryParse(configString, out configVal))
                    result = configVal;
            }
            //}

            return result;
        }

        private static string[] GetStringArrayConfigValue(string key)
        {
            //var settings = ConfigurationManager.GetSection(SECTIONKEY) as NameValueCollection;
            //if (settings != null)
            //{
            var configString = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configString))
            {
                return configString.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            //}

            return new string[0];
        }

        public static string[] GetStringArrayConfigValues(string sectionKey, string key)
        {
            var settings = ConfigurationManager.GetSection(sectionKey) as NameValueCollection;
            if (settings != null && settings.AllKeys.Contains(key))
            {
                var configString = settings[key];
                if (!string.IsNullOrEmpty(configString))
                {
                    return configString.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            return new string[0];
        }
    }
}