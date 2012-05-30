using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Schema;
using System.Diagnostics;
using System.Configuration;
using System.Globalization;
using System.Data.Linq;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.AppModel;

namespace SenseNet.Messaging
{
    /// <summary>
    /// Implements a service that can start when the Repository is starting.
    /// </summary>
    public class NotificationService : ISnService
    {
        /// <summary>
        /// Starts the service if it is enabled in the configuration.
        /// </summary>
        /// <returns>True if the service has started.</returns>
        public bool Start()
        {
            return NotificationHandler.StartNotificationSystem();
        }
    }

    internal class NotificationHandler
    {
        private const double MINPOLLINTERVAL = 2000.0;
        private const string NOTIFICATIONLOCKNAME = "ProcessNotificationLock";
        private const double TimeoutMultiplier = 1.6;
        private static double LockingTimeframe;
        private static string AppDomainId;
        private const string insertSql = @"
UPDATE [dbo].[Messaging.Synchronization]
   SET [Locked] = 1
      ,[LockedUntil] = @lockedUntil
      ,[ComputerName] = @computerName
      ,[LockId] = @lockId
 WHERE [LockName] = @lockName AND [LockedUntil] = @oldLockedUntil";

        private static bool _started;
        internal static bool StartNotificationSystem()
        {
            if (Configuration.Enabled && !_started)
            {
                var pollInterval = Configuration.TimerInterval * 60.0 * 1000.0;
                LockingTimeframe = TimeoutMultiplier * Configuration.TimerInterval;
                AppDomainId = Guid.NewGuid().ToString();

                _notifTimer = new System.Timers.Timer(pollInterval);
                _notifTimer.Elapsed += new System.Timers.ElapsedEventHandler(NotifTimerElapsed);
                _notifTimer.Disposed += new EventHandler(NotifTimerDisposed);
                _notifTimer.Enabled = true;

                _started = true;
            }
            return _started;
        }

        //=========================================================================================================== Polling

        private static System.Timers.Timer _notifTimer;
        private static bool _processing;

        private static void NotifTimerDisposed(object sender, EventArgs e)
        {
            _notifTimer.Elapsed -= new System.Timers.ElapsedEventHandler(NotifTimerElapsed);
            _notifTimer.Disposed -= new EventHandler(NotifTimerDisposed);
        }
        private static void NotifTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Configuration.Enabled)
                return;
            if (_processing)
                RefreshLock(LockingTimeframe);
            else
                TimerTick(DateTime.Now);
        }
        internal static void TimerTick(DateTime now)
        {
            if (AcquireLock(LockingTimeframe))
            {
                try
                {
                    Debug.WriteLine("#Notification> ** lock recieved");
                    _processing = true;
                    TimerTick(now, NotificationFrequency.Immediately);
                    TimerTick(now, NotificationFrequency.Daily);
                    TimerTick(now, NotificationFrequency.Weekly);
                    TimerTick(now, NotificationFrequency.Monthly);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }
                finally
                {
                    _processing = false;
                    ReleaseLock();
                    NotificationSender.StartMessageProcessing();
                }
            }
            else
            {
                Debug.WriteLine("#Notification> ** couldn't get lock");
            }
        }
        internal static void TimerTick(DateTime now, NotificationFrequency freq)
        {
            switch (freq)
            {
                case NotificationFrequency.Immediately: if (!Configuration.ImmediatelyEnabled) return; break;
                case NotificationFrequency.Daily: if (!Configuration.DailyEnabled) return; break;
                case NotificationFrequency.Weekly: if (!Configuration.WeeklyEnabled) return; break;
                case NotificationFrequency.Monthly: if (!Configuration.MonthlyEnabled) return; break;
                default: throw GetUnknownFrequencyException(freq);
            }
            
            if (now >= LastProcessTime.GetNextTime(freq, now))
                GenerateMessages(freq, now);
        }
        
        //-------------------------------------------------

        //TODO: AcquireLock, RefreshLock, Release lock csak a teszteles miatt internal, private jobb lenne
        internal static bool AcquireLock(double timeframe)
        {
            Synchronization notificationLock;
            
            using (var context = new DataHandler())
            {
                notificationLock = context.Synchronizations.SingleOrDefault(lockItem => lockItem.LockName == NOTIFICATIONLOCKNAME);
                if (notificationLock == null)
                    return InsertLock(timeframe);
            }
            
            if (notificationLock.Locked && (notificationLock.LockId != AppDomainId) && notificationLock.LockedUntil > DateTime.Now)
                return false;

            return UpdateLock(notificationLock, timeframe);
        }
        internal static void RefreshLock(double timeframe)
        {
            using (var context = new DataHandler())
            {
                var notificationLock = context.Synchronizations.SingleOrDefault(lockItem => lockItem.LockName == NOTIFICATIONLOCKNAME);
                if (notificationLock == null)
                    throw new InvalidOperationException("Could not update the Notification lock, because it doesn't exist. (the lock must be present when RefreshLock is called.)");

                notificationLock.LockedUntil = DateTime.Now.AddMinutes(timeframe);
                context.SubmitChanges();
            }
            Debug.WriteLine("#Notification> ** lock refreshed");
        }
        internal static void ReleaseLock()
        {
            using (var context = new DataHandler())
            {
                var notificationLock = context.Synchronizations.SingleOrDefault(lockItem => lockItem.LockName == NOTIFICATIONLOCKNAME);
                if (notificationLock == null)
                    throw new Exception(); //TODO: #notifB: ?? what is this?

                notificationLock.Locked = false;
                context.SubmitChanges();
            }
            Debug.WriteLine("#Notification> ** lock released");
        }
        
        private static bool InsertLock(double timeframe)
        {
            using (var context = new DataHandler())
            {
                var success = false;
                var notifLock = new Synchronization
                                    {
                                        LockName = NOTIFICATIONLOCKNAME,
                                        Locked = true,
                                        ComputerName = Environment.MachineName,
                                        LockedUntil =
                                            DateTime.Now.AddMinutes(timeframe),
                                        LockId = AppDomainId
                                    };
                try
                {
                    context.Synchronizations.InsertOnSubmit(notifLock);
                    context.SubmitChanges();
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }

                return success;
            }
        }
        private static bool UpdateLock(Synchronization oldLock, double timeframe)
        {
            var proc = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateDataProcedure(insertSql);
            proc.CommandType = System.Data.CommandType.Text;

            //lockname - nem biztos, hogy kell
            var lockNamePrm = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateParameter();
            lockNamePrm.ParameterName = "@lockName";
            lockNamePrm.DbType = System.Data.DbType.String;
            lockNamePrm.Value = NOTIFICATIONLOCKNAME;
            proc.Parameters.Add(lockNamePrm);
            
            //lockedUntil
            var lockedUntilPrm = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateParameter();
            lockedUntilPrm.ParameterName = "@lockedUntil";
            lockedUntilPrm.DbType = System.Data.DbType.DateTime;
            lockedUntilPrm.Value = DateTime.Now.AddMinutes(timeframe);
            proc.Parameters.Add(lockedUntilPrm);

            //computername
            var computerNamePrm = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateParameter();
            computerNamePrm.ParameterName = "@computerName";
            computerNamePrm.DbType = System.Data.DbType.String;
            computerNamePrm.Value = Environment.MachineName;
            proc.Parameters.Add(computerNamePrm);

            //lockId
            var lockIdPrm = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateParameter();
            lockIdPrm.ParameterName = "@lockId";
            lockIdPrm.DbType = System.Data.DbType.String;
            lockIdPrm.Value = Guid.NewGuid().ToString();
            proc.Parameters.Add(lockIdPrm);

            //oldLockedUntil
            var oldLockedUntilPrm = SenseNet.ContentRepository.Storage.Data.DataProvider.CreateParameter();
            oldLockedUntilPrm.ParameterName = "@oldLockedUntil";
            oldLockedUntilPrm.DbType = System.Data.DbType.DateTime2;
            oldLockedUntilPrm.Value = oldLock.LockedUntil;
            proc.Parameters.Add(oldLockedUntilPrm);
            
            try
            {
                var rows = proc.ExecuteNonQuery();
                return rows == 1;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                return false;
            }

            
        }

        //===========================================================================================================

        public static void GenerateMessages(NotificationFrequency freq, DateTime now)
        {
            var subscriptions = CollectEventsPerSubscription(freq, now);
            if (subscriptions != null)
                GenerateMessages(subscriptions);

            Cleanup(freq, now);
        }
        private static IEnumerable<Subscription> CollectEventsPerSubscription(NotificationFrequency freq, DateTime now)
        {
            var lastTime = now;
            var subscriptions = Subscription.GetActiveSubscriptionsByFrequency(freq);
            if (subscriptions.Count() == 0)
                return null;
            //var time = GetTimeLimit(freq);
            var time = LastProcessTime.GetLastProcessTime(freq);

            using (var context = new DataHandler())
            {
                var events = (time == DateTime.MinValue)?
                    context.Events.Where(x => x.When <= lastTime) :
                    context.Events.Where(x => x.When > time && x.When <= lastTime);
                foreach (var @event in events.OrderBy(x => x.When))
                    foreach (var subscription in subscriptions)
                        if (IsRelatedPath(@event.ContentPath, subscription.ContentPath))
                            if (HasPermission(subscription, @event))
                                subscription.AddRelatedEvent(@event);
            }
            LastProcessTime.SetLastProcessTime(freq, lastTime);
            return subscriptions;
        }

        private static bool IsRelatedPath(string eventPath, string subscriptionPath)
        {
            if (eventPath == subscriptionPath)
                return true;
            if (!eventPath.StartsWith(subscriptionPath))
                return false;
            return eventPath[subscriptionPath.Length] == '/';
        }
        private static bool HasPermission(Subscription subscription, Event @event)
        {
            if (@event.NotificationType == NotificationType.MinorVersionModified)
                return SecurityHandler.HasPermission(subscription.User, @event.ContentPath, @event.CreatorId, @event.LastModifierId, PermissionType.OpenMinor);
            else
                return SecurityHandler.HasPermission(subscription.User, @event.ContentPath, @event.CreatorId, @event.LastModifierId, PermissionType.Open);
        }

        private static void GenerateMessages(IEnumerable<Subscription> subscriptions)
        {
            var configs = new Dictionary<string, NotificationConfig>();

            using (var context = new DataHandler())
            {
                foreach (var subscription in subscriptions)
                {
                    // gather notification configs (handler: NotificationConfig.cs) for all related content of subscriptions
                    GatherConfigsForSubscription(subscription, configs);

                    // generate messges for content whose notification are controlled by a notification config
                    var msgs = GenerateMessagesForConfig(subscription, configs);
                    foreach (var message in msgs)
                    {
                        context.Messages.InsertOnSubmit(message);
                    }

                    // generate messages for content whose notification are NOT controlled by a notification config (send a generic notification message)
                    var msg = GenerateMessage(subscription, configs);
                    if (msg != null)
                        context.Messages.InsertOnSubmit(msg);
                }
                context.SubmitChanges();
            }
        }
        private static void GatherConfigsForSubscription(Subscription subscription, Dictionary<string, NotificationConfig> configs)
        {
            // go through related events, and gather available configs
            foreach (var @event in subscription.RelatedEvents)
            {
                // config already resolved for this contentpath
                if (configs.ContainsKey(@event.ContentPath))
                    continue;

                // check custom config
                var query = new ApplicationQuery(NotificationConfig.CONFIGFOLDERNAME, false, false, HierarchyOption.Path);
                var configHeads = query.ResolveApplications(NotificationConfig.NOTIFICATIONCONFIGCONTENTNAME, @event.ContentPath, NotificationConfig.NOTIFICATIONCONFIGTYPENAME);
                var targetConfigHead = configHeads.FirstOrDefault();

                NotificationConfig configNode = null;
                if (targetConfigHead != null)
                {
                    configNode = Node.LoadNode(targetConfigHead.Id) as NotificationConfig;
                }
                configs.Add(@event.ContentPath, configNode);
            }
        }
        private static IEnumerable<Message> GenerateMessagesForConfig(Subscription subscription, Dictionary<string, NotificationConfig> configs)
        {
            if (subscription.RelatedEvents.Count == 0)
                yield break;

            foreach (var @event in subscription.RelatedEvents)
            {
                // we will process those content only for which config is available.
                var config = configs[@event.ContentPath];
                if (config == null)
                    continue;

                var contextNode = Node.LoadNode(@event.ContentPath);
                if (contextNode == null)
                    continue;

                var allowed = config.IsNotificationAllowedForContent(contextNode);
                if (!allowed)
                    continue;

                var subject = config.GetSubject(contextNode);
                var body = config.GetBody(contextNode);

                yield return new Message
                {
                    Address = subscription.UserEmail,
                    Subject = subject,
                    Body = body.ToString()
                };
            }
        }
        private static Message GenerateMessage(Subscription subscription, Dictionary<string, NotificationConfig> configs)
        {
            if (subscription.RelatedEvents.Count == 0)
                return null;

            // we will process those content only for which no config is available. if there is no such content, we are done.
            if (!subscription.RelatedEvents.Any(e => configs[e.ContentPath] == null))
                return null;

            var template = new MessageTemplate(subscription.Language);
            var cultureInfo = CultureInfo.CreateSpecificCulture(subscription.Language);

            string subjectTemplate = null;
            switch (subscription.Frequency)
            {
                case NotificationFrequency.Immediately: subjectTemplate = template.ImmediatelySubject; break;
                case NotificationFrequency.Daily: subjectTemplate = template.DailySubject; break;
                case NotificationFrequency.Weekly: subjectTemplate = template.WeeklySubject; break;
                case NotificationFrequency.Monthly: subjectTemplate = template.MonthlySubject; break;
                default: throw GetUnknownFrequencyException(subscription.Frequency);
            }
            var subject = ReplaceParameters(subjectTemplate, subscription);

            var body = new StringBuilder();
            string headTemplate = null;
            switch (subscription.Frequency)
            {
                case NotificationFrequency.Immediately: headTemplate = template.ImmediatelyHeader; break;
                case NotificationFrequency.Daily: headTemplate = template.DailyHeader; break;
                case NotificationFrequency.Weekly: headTemplate = template.WeeklyHeader; break;
                case NotificationFrequency.Monthly: headTemplate = template.MonthlyHeader; break;
                default: throw GetUnknownFrequencyException(subscription.Frequency);
            }
            body.Append(ReplaceParameters(headTemplate, subscription));

            foreach (var @event in subscription.RelatedEvents)
            {
                // a custom config exists for this contentpath, email for that has already been processed previously
                if (configs[@event.ContentPath] != null)
                    continue;

                body.Append(ReplaceParameters(GetEventTemplate(template, @event.NotificationType), subscription.SitePath, subscription.SiteUrl, @event, cultureInfo));
            }

            string footTemplate = null;
            switch (subscription.Frequency)
            {
                case NotificationFrequency.Immediately: footTemplate = template.ImmediatelyFooter; break;
                case NotificationFrequency.Daily: footTemplate = template.DailyFooter; break;
                case NotificationFrequency.Weekly: footTemplate = template.WeeklyFooter; break;
                case NotificationFrequency.Monthly: footTemplate = template.MonthlyFooter; break;
                default: throw GetUnknownFrequencyException(subscription.Frequency);
            }
            body.Append(ReplaceParameters(footTemplate, subscription));

            return new Message
            {
                Address = subscription.UserEmail,
                Subject = subject,
                Body = body.ToString()
            };

        }

        private static string GetEventTemplate(MessageTemplate template, NotificationType notificationType)
        {
            switch (notificationType)
            {
                case NotificationType.Created: return template.CreatedTemplate;
                case NotificationType.MajorVersionModified: return template.MajorVersionModifiedTemplate;
                case NotificationType.MinorVersionModified: return template.MinorVersionModifiedTemplate;
                case NotificationType.CopiedFrom: return template.CopiedFromTemplate;
                case NotificationType.MovedFrom: return template.MovedFromTemplate;
                case NotificationType.MovedTo: return template.MovedToTemplate;
                case NotificationType.RenamedFrom: return template.RenamedFromTemplate;
                case NotificationType.RenamedTo: return template.RenamedToTemplate;
                case NotificationType.Deleted: return template.DeletedTemplate;
                case NotificationType.Restored: return template.RestoredTemplate;
                default: throw new NotImplementedException("Unknown NotificationType: " + notificationType);
            }
        }
        private static string ReplaceParameters(string template, Subscription subscription)
        {
            return template
                .Replace("{Address}", subscription.UserEmail)
                .Replace("{Addressee}", subscription.UserName);
        }
        private static string ReplaceParameters(string template, string sitePath, string siteUrl, Event @event, CultureInfo cultureInfo)
        {
            return template
                .Replace("{ContentPath}", @event.ContentPath)
                .Replace("{ContentUrl}", String.Concat("http://", GetContentUrl(sitePath, siteUrl, @event.ContentPath)))
                //.Replace("{NotificationType}", @event.NotificationType.ToString())
                .Replace("{When}", @event.When.ToString(cultureInfo))
                .Replace("{Who}", @event.Who);
        }

        private static string GetContentUrl(string sitePath, string siteUrl, string contentPath)
        {
            if (contentPath == sitePath)
                return siteUrl;
            if (!contentPath.StartsWith(sitePath))
                return contentPath;
            if (contentPath[sitePath.Length] != '/')
                return contentPath;
            return contentPath.Replace(sitePath, siteUrl);
        }

        private static void Cleanup(NotificationFrequency freq, DateTime now)
        {
            switch (freq)
            {
                case NotificationFrequency.Immediately:
                    if (Configuration.DailyEnabled || Configuration.WeeklyEnabled || Configuration.MonthlyEnabled)
                        return;
                    break;
                case NotificationFrequency.Daily:
                    if (Configuration.WeeklyEnabled || Configuration.MonthlyEnabled)
                        return;
                    break;
                case NotificationFrequency.Weekly:
                    if (Configuration.MonthlyEnabled)
                        return;
                    break;
                case NotificationFrequency.Monthly:
                    break;
                default:
                    throw GetUnknownFrequencyException(freq);
            }
            Event.DeleteOldEvents(now);
        }

        internal static Exception GetUnknownFrequencyException(NotificationFrequency freq)
        {
            return new NotImplementedException("Unknown NotificationFrequency: " + freq);
        }

    }
}
