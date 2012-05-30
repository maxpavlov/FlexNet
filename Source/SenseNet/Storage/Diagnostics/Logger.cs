using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Security;
using System.Configuration;

namespace SenseNet.Diagnostics
{
    public class Logger
    {
        private static volatile Logger _loggerInstance;

        private static object _loggerInstanceLock = new object();

        public static ILoggerAdapter DefaultLoggerAdapter;


        static Logger()
        {
            var logSection = System.Configuration.ConfigurationManager.GetSection("loggingConfiguration");
            if (logSection != null)
                DefaultLoggerAdapter = new EntLibLoggerAdapter();
            else
                DefaultLoggerAdapter = new DebugWriteLoggerAdapter();
        }

        public static Logger Current
        {
            get
            {
                if (_loggerInstance == null)
                {
                    lock (_loggerInstanceLock)
                    {
                        if (_loggerInstance == null)
                        {
                            _loggerInstance = new Logger(DefaultLoggerAdapter);
                        }
                    }
                }
                return _loggerInstance;
            }
        }

        public Logger()
            : this(Logger.DefaultLoggerAdapter)
        {
        }

        public Logger(ILoggerAdapter loggerAdapter)
        {
            LoggerAdapter = loggerAdapter;
        }

        public ILoggerAdapter LoggerAdapter { get; set; }

        public static int DefaultPriority = -1;
        public static TraceEventType DefaultSeverity = TraceEventType.Information;
        public static int DefaultEventId = 1;
        public static string DefaultTitle = "";
        public static readonly ICollection<string> EmptyCategoryList = new string[0];

        public static void Write(object message)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, DefaultSeverity,
                DefaultTitle, null);
        }
        public static void Write(object message, string category)
        {
            ICollection<string> categories = new string[] { category };

            Write(message, categories, DefaultPriority, DefaultEventId, DefaultSeverity,
                DefaultTitle, null);
        }
        public static void Write(object message, ICollection<string> categories)
        {
            if (categories == null)
                categories = Logger.EmptyCategoryList;
            Write(message, categories, DefaultPriority, DefaultEventId, DefaultSeverity,
                DefaultTitle, null);
        }
        public static void Write(object message, ICollection<string> categories, TraceEventType severity)
        {
            if (categories == null)
                categories = Logger.EmptyCategoryList;
            Write(message, categories, DefaultPriority, DefaultEventId, severity,
                DefaultTitle, null);
        }
        public static void Write(object message, ICollection<string> categories, int priority, int eventId, 
            TraceEventType severity, string title, IDictionary<string, object> properties)
        {
            Logger.Current.WriteLog(message, categories, priority, eventId, severity, title, properties);
        }

        private void WriteLog(object message, ICollection<string> categories, int priority, int eventId, TraceEventType severity,
            string title, IDictionary<string, object> properties)
        {
            LoggerAdapter.Write(message, categories, priority, eventId, severity, title, properties);
        }

        public static OperationTrace TraceOperation(string name)
        {
            return new OperationTrace(name);
        }
        public static OperationTrace TraceOperation(string name, string title)
        {
            return new OperationTrace(name, title);
        }
        public static OperationTrace TraceOperation(string name, string title, IEnumerable<string> categories)
        {
            return new OperationTrace(name, title, categories);
        }

        //=================================================================================================================

        public static void WriteCritical(object message)
        {
            Write(message, Logger.EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Critical, null, null);
        }
        public static void WriteCritical(object message, IEnumerable<string> categories)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Critical, null, null);
        }
        public static void WriteCritical(object message, IDictionary<string, object> properties)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Critical, null, properties);
        }
        public static void WriteCritical(object message, IEnumerable<string> categories, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Critical, null, properties);
        }
        public static void WriteCritical(object message, IEnumerable<string> categories, int priority, int eventId,
            string title, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), priority, eventId, TraceEventType.Critical, title, properties);
        }
        public static void WriteCritical<T>(object message, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId,
                TraceEventType.Critical, null, getPropertiesCallback, callbackArg);
        }
        public static void WriteCritical<T>(object message, IEnumerable<string> categories, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, categories.ToList(), DefaultPriority, DefaultEventId,
                TraceEventType.Critical, null, getPropertiesCallback, callbackArg);
        }
        //----
        public static void WriteError(object message)
        {
            Write(message, Logger.EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Error, null, null);
        }
        public static void WriteError(object message, IEnumerable<string> categories)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Error, null, null);
        }
        public static void WriteError(object message, IDictionary<string, object> properties)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Error, null, properties);
        }
        public static void WriteError(object message, IEnumerable<string> categories, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Error, null, properties);
        }
        public static void WriteError(object message, IEnumerable<string> categories, int priority, int eventId,
            string title, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), priority, eventId, TraceEventType.Error, title, properties);
        }
        public static void WriteError<T>(object message, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId,
                TraceEventType.Error, null, getPropertiesCallback, callbackArg);
        }
        public static void WriteError<T>(object message, IEnumerable<string> categories, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, categories.ToList(), DefaultPriority, DefaultEventId,
                TraceEventType.Error, null, getPropertiesCallback, callbackArg);
        }
        //----
        public static void WriteInformation(object message)
        {
            Write(message, Logger.EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Information, null, null);
        }
        public static void WriteInformation(object message, IEnumerable<string> categories)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Information, null, null);
        }
        public static void WriteInformation(object message, IDictionary<string, object> properties)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Information, null, properties);
        }
        public static void WriteInformation(object message, IEnumerable<string> categories, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Information, null, properties);
        }
        public static void WriteInformation(object message, IEnumerable<string> categories, int priority, int eventId,
            string title, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), priority, eventId, TraceEventType.Information, title, properties);
        }
        public static void WriteInformation<T>(object message, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId,
                TraceEventType.Information, null, getPropertiesCallback, callbackArg);
        }
        public static void WriteInformation<T>(object message, IEnumerable<string> categories, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, categories.ToList(), DefaultPriority, DefaultEventId,
                TraceEventType.Information, null, getPropertiesCallback, callbackArg);
        }
        //----
        public static void WriteWarning(object message)
        {
            Write(message, Logger.EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Warning, null, null);
        }
        public static void WriteWarning(object message, IEnumerable<string> categories)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Warning, null, null);
        }
        public static void WriteWarning(object message, IDictionary<string, object> properties)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Warning, null, properties);
        }
        public static void WriteWarning(object message, IEnumerable<string> categories, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Warning, null, properties);
        }
        public static void WriteWarning(object message, IEnumerable<string> categories, int priority, int eventId,
            string title, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), priority, eventId, TraceEventType.Warning, title, properties);
        }
        public static void WriteWarning<T>(object message, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId,
                TraceEventType.Warning, null, getPropertiesCallback, callbackArg);
        }
        public static void WriteWarning<T>(object message, IEnumerable<string> categories, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, categories.ToList(), DefaultPriority, DefaultEventId,
                TraceEventType.Warning, null, getPropertiesCallback, callbackArg);
        }
        //----
        public static void WriteVerbose(object message)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Verbose, null, null);
        }
        public static void WriteVerbose(object message, IEnumerable<string> categories)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Verbose, null, null);
        }
        public static void WriteVerbose(object message, IDictionary<string, object> properties)
        {
            Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId, TraceEventType.Verbose, null, properties);
        }
        public static void WriteVerbose(object message, IEnumerable<string> categories, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Verbose, null, properties);
        }
        public static void WriteVerbose(object message, IEnumerable<string> categories, int priority, int eventId,
            string title, IDictionary<string, object> properties)
        {
            Write(message, categories.ToList(), priority, eventId, TraceEventType.Verbose, title, properties);
        }
        public static void WriteVerbose<T>(object message, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, EmptyCategoryList, DefaultPriority, DefaultEventId,
                TraceEventType.Verbose, null, getPropertiesCallback, callbackArg);
        }
        public static void WriteVerbose<T>(object message, IEnumerable<string> categories, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            Logger.Current.LoggerAdapter.Write(message, categories.ToList(), DefaultPriority, DefaultEventId,
                TraceEventType.Verbose, null, getPropertiesCallback, callbackArg);
        }

        //----
        public static void WriteException(Exception ex)
        {
            WriteException(ex, EmptyCategoryList);
        }
        public static void WriteException(Exception ex, IEnumerable<string> categories)
        {
            Logger.Current.LoggerAdapter.Write(ex.Message, categories.ToList(), DefaultPriority, DefaultEventId, TraceEventType.Error, null,
                Utility.GetDefaultProperties, ex);
        }

        //----
        public static void WriteAudit(AuditEvent auditEvent, IDictionary<string, object> properties)
        {
            Logger.Current.LoggerAdapter.Write(auditEvent, Categories("Audit").ToArray(), DefaultPriority, auditEvent.EventId,
               TraceEventType.Verbose, auditEvent.AuditCategory, properties);
        }
        public static void WriteAudit<T>(AuditEvent auditEvent, Func<T, IDictionary<string, object>> getPropertiesCallback, T callbackArg)
        {
            //WriteAudit(auditEvent, getPropertiesCallback(callbackArg));
            Logger.Current.LoggerAdapter.Write(auditEvent, Categories("Audit").ToArray(), DefaultPriority, auditEvent.EventId,
                TraceEventType.Verbose, auditEvent.AuditCategory, getPropertiesCallback, callbackArg);
        }

        //==========================================================================================

        public static IEnumerable<string> Categories(params string[] categories)
        {
            return categories; 
        }
        public static IDictionary<string, object> GetDefaultProperties(object target)
        {
            return Utility.GetDefaultProperties(target);
        }

        static bool? _auditEnabled;
        public static bool AuditEnabled
        {
            get
            {
                bool result;

                if (!_auditEnabled.HasValue)
                    _auditEnabled = Boolean.TryParse(ConfigurationManager.AppSettings["AuditEnabled"], out result) ? result : true;
                return _auditEnabled.Value;
            }
            set
            {
                _auditEnabled = value;
            }
        }
    }
}
