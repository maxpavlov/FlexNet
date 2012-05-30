using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;

namespace SenseNet.Messaging
{
    public partial class Subscription
    {
        public NotificationFrequency Frequency
        {
            get { return (NotificationFrequency)this.FrequencyId; }
            set { this.FrequencyId = (int)value; }
        }
        public bool IsActive
        {
            get { return this.Active != 0; }
            set { this.Active = value ? (byte)1 : (byte)0; }
        }

        private List<Event> _relatedEvents;
        internal List<Event> RelatedEvents
        {
            get
            {
                if (_relatedEvents == null)
                    _relatedEvents = new List<Event>();
                return _relatedEvents;
            }
        }

        private IUser _user;
        internal IUser User
        {
            get
            {
                if (_user == null)
                    _user = Node.Load<User>(this.UserId);
                return _user;
            }
        }

        public void Save()
        {
            using (var context = new DataHandler())
            {
                var existing = context.Subscriptions.Where(x =>
                    x.ContentPath == this.ContentPath &&
                    x.UserPath == this.UserPath);

                var count = existing.Count();
                if (count == 1)
                {
                    // update
                    var item = existing.First();
                    item.Active = this.Active;
                    item.ContentPath = this.ContentPath;
                    item.Frequency = this.Frequency;
                    item.IsActive = this.IsActive;
                    item.UserEmail = this.UserEmail;
                    item.UserId = this.UserId;
                    item.UserName = this.UserName;
                    item.UserPath = this.UserPath;
                    item.Language = this.Language;
                }
                else
                {
                    if (count > 1)
                        context.Subscriptions.DeleteAllOnSubmit(existing);
                    context.Subscriptions.InsertOnSubmit(this);
                }
                context.SubmitChanges();
            }
        }

        //========================================================= Static part

        public static void Subscribe(User subscriber, Node target, NotificationFrequency frequency, string language, string sitePath, string siteUrl)
        {
            Subscribe(subscriber, target, frequency, language, sitePath, siteUrl, true);
        }
        public static void Subscribe(User subscriber, Node target, NotificationFrequency frequency, string language, string sitePath, string siteUrl, bool isActive)
        {
            if (subscriber.Email == null)
                throw new InvalidOperationException("Subscriber's email cannot be null.");
            if (subscriber.Email.Length == 0)
                throw new InvalidOperationException("Subscriber's email cannot be empty.");

            var userName = subscriber.FullName;
            if (String.IsNullOrEmpty(userName))
                userName = subscriber.Username;

            var subscription = new Subscription
            {
                UserEmail = subscriber.Email,
                UserId = subscriber.Id,
                UserPath = subscriber.Path,
                UserName = userName,
                ContentPath = target.Path,
                Frequency = frequency,
                Language = language,
                IsActive = true,
                SitePath = sitePath,
                SiteUrl = siteUrl,
            };
            subscription.Save();
        }
        public static void UnSubscribe(User subscriber, Node target)
        {
            using (var context = new DataHandler())
            {
                var existing = context.Subscriptions.Where(
                    x => x.ContentPath == target.Path && x.UserPath == subscriber.Path);

                if (existing.Count() > 0)
                {
                    context.Subscriptions.DeleteAllOnSubmit(existing);
                    context.SubmitChanges();
                }
            }
        }
        public static void UnSubscribeAll(User subscriber)
        {
            using (var context = new DataHandler())
            {
                var existing = context.Subscriptions.Where(
                    x => x.UserPath == subscriber.Path);

                if (existing.Count() > 0)
                {
                    context.Subscriptions.DeleteAllOnSubmit(existing);
                    context.SubmitChanges();
                }
            }
        }
        public static void UnSubscribeFrom(Node target)
        {
            using (var context = new DataHandler())
            {
                var existing = context.Subscriptions.Where(
                    x => x.ContentPath == target.Path);

                if (existing.Count() > 0)
                {
                    context.Subscriptions.DeleteAllOnSubmit(existing);
                    context.SubmitChanges();
                }
            }
        }
        public static void DeleteAllSubscriptions()
        {
            using (var context = new DataHandler())
            {
                context.ExecuteCommand("DELETE FROM [Messaging.Subscriptions]");
            }
        }

        public static void ActivateSubscription(User subscriber, Node target)
        {
            SetActivation(subscriber, target, true);
        }
        public static void InactivateSubscription(User subscriber, Node target)
        {
            SetActivation(subscriber, target, false);
        }
        private static void SetActivation(User subscriber, Node target, bool value)
        {
            using (var context = new DataHandler())
            {
                var subscription = context.Subscriptions.Where(x =>
                    x.UserPath == subscriber.Path &&
                    x.ContentPath == target.Path &&
                    x.Active == (byte)(value ? 0 : 1)).FirstOrDefault();
                if (subscription == null)
                    return;
                subscription.IsActive = value;
                context.SubmitChanges();
            }
        }

        public static IEnumerable<Subscription> GetAllSubscriptions()
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.ToArray();
            }
        }
        public static int GetCountOfSubscriptions()
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Count();
            }
        }
        public static IEnumerable<Subscription> GetSubscriptionsByUser(User subscriber)
        {
            return GetSubscriptionsByUser(subscriber.Path);
        }
        public static IEnumerable<Subscription> GetSubscriptionsByUser(string userPath)
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Where(x => x.UserPath == userPath).ToArray();
            }
        }
        public static IEnumerable<Subscription> GetActiveSubscriptionsByUser(User subscriber)
        {
            return GetActiveSubscriptionsByUser(subscriber.Path);
        }
        public static IEnumerable<Subscription> GetActiveSubscriptionsByUser(string userPath)
        {
            return GetSubscriptionsByUser(userPath, true);
        }
        public static IEnumerable<Subscription> GetInactiveSubscriptionsByUser(User subscriber)
        {
            return GetInactiveSubscriptionsByUser(subscriber.Path);
        }
        public static IEnumerable<Subscription> GetInactiveSubscriptionsByUser(string userPath)
        {
            return GetSubscriptionsByUser(userPath, false);
        }
        internal static IEnumerable<Subscription> GetSubscriptionsByUser(string subscriberPath, bool isActive)
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Where(
                    x => x.UserPath == subscriberPath && x.Active == (byte)(isActive ? 1 : 0))
                    .ToArray();
            }
        }

        public static Subscription GetSubscriptionByUser(User subscriber, Node target)
        {
            return GetSubscriptionByUser(subscriber.Path, target.Path);
        }
        public static Subscription GetSubscriptionByUser(User subscriber, Node target, bool isActive)
        {
            return GetSubscriptionByUser(subscriber.Path, target.Path, isActive);
        }
        public static Subscription GetSubscriptionByUser(string subscriberPath, string contentPath)
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Where(x =>
                    x.UserPath == subscriberPath &&
                    x.ContentPath == contentPath).FirstOrDefault();
            }
        }
        public static Subscription GetSubscriptionByUser(string subscriberPath, string contentPath, bool isActive)
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Where(x =>
                    x.UserPath == subscriberPath &&
                    x.ContentPath == contentPath &&
                    x.Active == (byte)(isActive ? 1 : 0)).FirstOrDefault();
            }
        }

        public static IEnumerable<Subscription> GetSubscriptionsByContent(Node target)
        {
            return GetSubscriptionsByContent(target.Path);
        }
        public static IEnumerable<Subscription> GetSubscriptionsByContent(string contentPath)
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Where(x => x.ContentPath == contentPath).ToArray();
            }
        }


        internal static IEnumerable<Subscription> GetActiveSubscriptionsByFrequency(NotificationFrequency frequency)
        {
            using (var context = new DataHandler())
            {
                return context.Subscriptions.Where(x => x.FrequencyId == (int)frequency && x.Active != 0).ToArray();
            }
        }

        internal void AddRelatedEvent(Event @event)
        {
            var toRemove = RelatedEvents.Where(x => x.NotificationType == @event.NotificationType
                && x.ContentPath == @event.ContentPath && x.When < @event.When).FirstOrDefault();
            if (toRemove != null)
                _relatedEvents.Remove(toRemove);
            _relatedEvents.Add(@event);
        }
    }
}
