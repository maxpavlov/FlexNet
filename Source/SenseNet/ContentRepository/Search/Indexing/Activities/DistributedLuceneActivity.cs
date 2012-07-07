using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Communication.Messaging;
using System.Diagnostics;
using System.Configuration;
using System.IO;

namespace SenseNet.Search.Indexing.Activities
{
    [Serializable]
    internal abstract class DistributedLuceneActivity : LuceneActivity
    {
        // =========================================================================== TEST MSMQ
        // This is a code for msmq and indexingactivity processing testing. 
        // Put <add key="MsmqLogPath" value="c:\\msmqlog-1-{0}.csv"/> in the webconfig, name should reflect the actual web node (use "1" for 1st node, "2" for 2nd node, etc.)
        public enum MSMQLogStatusType
        {
            SENT = 0,
            RECEIVED,
            REMOVED,
            ADDED
        }
        private static bool? _MSMQLogEnabled;
        private static object MSMQLogEnabledSync = new object();
        private static bool MSMQLogEnabled
        {
            get
            {
                if (!_MSMQLogEnabled.HasValue)
                {
                    lock (MSMQLogEnabledSync)
                    {
                        if (!_MSMQLogEnabled.HasValue)
                        {
                            _MSMQLogEnabled = StartMSMQLog();
                        }
                    }
                }
                return _MSMQLogEnabled.Value;
            }
        }
        private static object MSMQLogSync = new object();
        private static System.Timers.Timer MSMQLogTimer;
        private static string MSMQLogPath;
        private static string MSMQLog;
        private static void WriteMSMQLog(DistributedLuceneActivity activity, MSMQLogStatusType statusType)
        {
            if (!MSMQLogEnabled)
                return;

            var pAct = activity as LuceneIndexingActivity;
            if (pAct == null)
                return;

            WriteMSMQLog(pAct.ActivityId, statusType, null);
        }
        internal static void WriteMSMQLog(int activityid, MSMQLogStatusType statusType, int? addedBy)
        {
            if (!MSMQLogEnabled)
                return;

            var status = statusType.ToString();
            var line = DateTime.Now.Ticks.ToString() + ";" + DateTime.Now.Hour.ToString() + ";" + DateTime.Now.Minute.ToString() + ";" + DateTime.Now.Second.ToString() + ";" + activityid.ToString() + ";" + status + ";";
            line += (addedBy.HasValue ? addedBy.Value.ToString() : string.Empty) + Environment.NewLine;
            lock (MSMQLogSync)
            {
                MSMQLog += line;
            }
        }
        private static bool StartMSMQLog()
        {
            // eg  "c:\\msmqlog-1-{0}.csv"
            var appSettingsMSMQLogPath = ConfigurationManager.AppSettings["MsmqLogPath"];
            if (string.IsNullOrEmpty(appSettingsMSMQLogPath))
                return false;

            MSMQLogPath = string.Format(appSettingsMSMQLogPath, DateTime.Now.ToString("yyyyMMddhhmmss"));
            using (var fs = new FileStream(MSMQLogPath, FileMode.Create))
            {
                using (var wr = new StreamWriter(fs))
                {
                    wr.WriteLine("ticks;hour;minute;second;activityid;sent/received;addedby");
                }
            }

            MSMQLog = string.Empty;
            MSMQLogTimer = new System.Timers.Timer(1000.0);
            MSMQLogTimer.Elapsed += new System.Timers.ElapsedEventHandler(MsmqLogTimer_Elapsed);
            MSMQLogTimer.Start();
            return true;
        }
        private static void MsmqLogTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (MSMQLogSync)
            {
                using (var writer = new StreamWriter(MSMQLogPath, true))
                {
                    writer.Write(MSMQLog);
                }
                MSMQLog = string.Empty;
            }
        }
        // =========================================================================== TEST MSMQ END


        [Serializable]
        public class LuceneActivityDistributor : DistributedAction
        {
            public DistributedLuceneActivity Activity;
            
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (!LuceneManager.Running)
                    return;
                
                //Trace.WriteLine(String.Format("@#$> T:{0} LuceneActivityDistributor.DoAction: {1}. onRemote: {2}, isFromMe:{3}", System.Threading.Thread.CurrentThread.ManagedThreadId, Activity.GetType().Name, onRemote, isFromMe));

                if (onRemote && !isFromMe)
                {
                    DistributedLuceneActivity.WriteMSMQLog(this.Activity, MSMQLogStatusType.RECEIVED);
                    this.Activity.InternalExecute();
                }
            }
        }

        public virtual void Distribute()
        {
            var distributor = new LuceneActivityDistributor() { Activity = this };
            distributor.Execute();
            DistributedLuceneActivity.WriteMSMQLog(this, MSMQLogStatusType.SENT);
        }

    }
}
