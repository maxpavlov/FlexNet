using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Diagnostics;
using System.Collections.Concurrent;
using System.Threading;

namespace SenseNet.DirectoryServices
{

    public static class AdLog
    {
        private enum EventType
        {
            Info,
            Error,
            Warning,
            Verbose
        }

        /* ==================================================================================== Subscriptions */
        private static object _subscriberSync = new object();
        private static ConcurrentDictionary<int,StringBuilder> __subscribers;
        private static ConcurrentDictionary<int, StringBuilder> Subscribers
        {
            get
            {
                if (__subscribers == null)
                {
                    lock (_subscriberSync)
                    {
                        if (__subscribers == null)
                        {
                            __subscribers = new ConcurrentDictionary<int, StringBuilder>();
                        }
                    }
                }
                return __subscribers;
            }
        }
        public static int SubscribeToLog()
        {
            // get current thread id, and add id-stringbuilder pair to the subscriber list.
            // subscribers this way will only see adsync events that occurred on their own thread.
            var threadid = Thread.CurrentThread.GetHashCode();
            var sb = new StringBuilder();
            sb.AppendLine(GetMsgWithTimeStamp("AD sync started."));
            Subscribers.TryAdd(threadid, sb);
            return threadid;
        }
        public static string GetLogAndRemoveSubscription(int id)
        {
            StringBuilder sb;
            Subscribers.TryRemove(id, out sb);
            sb.AppendLine(GetMsgWithTimeStamp("AD sync finished."));
            return sb.ToString();
        }


        /* ==================================================================================== Consts */
        public static readonly string[] AdSyncLogCategory = new[] { "AdSync" };
        private const string _logPath = "/Root/System/SystemPlugins/Tools/DirectoryServices/Log";


        /* ==================================================================================== Properties */
        private static object _errorsSync = new object();
        private static int __errors = 0;
        private static int Errors
        {
            get 
            {
                lock (_errorsSync)
                {
                    return __errors;
                }
            }
        }
        private static void IncreaseError()
        {
            lock (_errorsSync)
            {
                __errors++;
            }
        }
        private static object _warningSync = new object();
        private static int __warnings = 0;
        private static int Warnings
        {
            get
            {
                lock (_warningSync)
                {
                    return __warnings;
                }
            }
        }
        private static void IncreaseWarning()
        {
            lock (_warningSync)
            {
                __warnings++;
            }
        }


        /* ==================================================================================== Methods */
        private static string GetMsgWithTimeStamp(string msg) 
        {
            return string.Format("{0}: {1}", DateTime.Now.ToLongTimeString(), msg);
        }
        public static void StartLog()
        {
            LogLine("AD Sync started", EventType.Info);
        }
        public static void EndLog()
        {
            LogLine(string.Format("AD sync finished with {0} warnings, {1} errors", Warnings, Errors), EventType.Info);
        }
        private static void LogLine(string msg, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Error:
                    Logger.WriteError(msg, AdSyncLogCategory);
                    break;
                case EventType.Warning:
                    Logger.WriteWarning(msg, AdSyncLogCategory);
                    break;
                case EventType.Info:
                    Logger.WriteInformation(msg, AdSyncLogCategory);
                    break;
                case EventType.Verbose:
                    Logger.WriteVerbose(string.Format("  {0}", msg), AdSyncLogCategory);
                    break;
            }

            Console.WriteLine(msg);

            // log event for subscriber of the current thread
            StringBuilder sb;
            if (Subscribers.TryGetValue(Thread.CurrentThread.GetHashCode(), out sb))
            {
                if (sb != null)
                    sb.AppendLine(GetMsgWithTimeStamp(msg));
            }
        }
        public static void Log(string msg)
        {
            LogLine(string.Format("       {0}", msg), EventType.Verbose);
        }
        public static void LogOuter(string msg)
        {
            LogLine(string.Format("    {0}", msg), EventType.Verbose);
        }
        public static void LogMain(string msg)
        {
            LogLine(msg, EventType.Info);
        }
        public static void LogMainActivity(string msg, string ADPath, string portalPath)
        {
            LogMain(string.Format("{0} ({1} --> {2})", msg, ADPath, portalPath));
        }
        public static void LogError(string msg)
        {
            LogLine(string.Format("ERROR: {0}", msg), EventType.Error);
            IncreaseError();
        }
        public static void LogWarning(string msg)
        {
            LogLine(string.Format("WARNING: {0}", msg), EventType.Warning);
            IncreaseWarning();
        }
        public static void LogErrorADObject(string msg, string obj)
        {
            LogError(string.Format("{0} (AD object: {1})", msg, obj));
        }
        public static void LogErrorPortalObject(string msg, string obj)
        {
            LogError(string.Format("{0} (Portal object: {1})", msg, obj));
        }
        public static void LogErrorObjects(string msg, string ADobj, string portalObj)
        {
            LogError(string.Format("{0} (AD object: {1}; portal object: {2})", msg, ADobj, portalObj));
        }
        public static void LogADObject(string msg, string obj)
        {
            Log(string.Format("{0} (AD object: {1})", msg, obj));
        }
        public static void LogPortalObject(string msg, string obj)
        {
            Log(string.Format("{0} (Portal object: {1})", msg, obj));
        }
        public static void LogOuterADObject(string msg, string obj)
        {
            LogOuter(string.Format("{0} (AD object: {1})", msg, obj));
        }
        public static void LogObjects(string msg, string ADobj, string portalObj)
        {
            Log(string.Format("{0} (AD object: {1}; portal object: {2})", msg, ADobj, portalObj));
        }
        public static void LogException(Exception ex)
        {
            Logger.WriteException(ex, AdSyncLogCategory);

            Console.WriteLine(string.Format("ERROR - exception: {0}", ex.Message));
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
                Console.WriteLine(ex.InnerException.Message);

            // log event for subscriber of the current thread
            StringBuilder sb;
            if (Subscribers.TryGetValue(Thread.CurrentThread.GetHashCode(), out sb))
            {
                if (sb != null)
                {
                    sb.AppendLine(string.Format("ERROR - exception: {0}", ex.Message));
                    sb.AppendLine(ex.StackTrace);
                    if (ex.InnerException != null)
                        sb.AppendLine(ex.InnerException.Message);
                }
            }

            //Logger.WriteError(string.Format("ERROR - exception: {0}", ex.Message));
            //LogMain(string.Format("        {0}", ex.StackTrace));
            //if (ex.InnerException != null)
            //{
            //    LogMain(string.Format("        {0}", ex.InnerException.Message));
            //}
            IncreaseError();
        }
    }
}
