using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Messaging;
using config = SenseNet.Messaging.Configuration;
using System.Reflection;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public class NotificationTests : TestBase
    {
        #region Test infrastructure
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public override TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
        #endregion
        #region Playground
        private static string _testRootName = "_NotificationTests";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);
        /// <summary>
        /// Do not use. Instead of TestRoot property
        /// </summary>
        private Node _testRoot;
        public Node TestRoot
        {
            get
            {
                if (_testRoot == null)
                {
                    _testRoot = Node.LoadNode(_testRootPath);
                    if (_testRoot == null)
                    {
                        Node node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
                        node.Name = _testRootName;
                        node.Save();
                        _testRoot = Node.LoadNode(_testRootPath);
                    }
                }
                return _testRoot;
            }
        }

        static List<string> _pathsToDelete = new List<string>();
        static void AddPathToDelete(string path)
        {
            lock (_pathsToDelete)
            {
                if (_pathsToDelete.Contains(path))
                    return;
                _pathsToDelete.Add(path);
            }
        }

        User __subscriber1;
        User Subscriber1
        {
            get
            {
                if (__subscriber1 == null)
                    __subscriber1 = LoadOrCreateUser("subscriber1", "Subscriber One", UserFolder);
                return __subscriber1;
            }
        }
        User __subscriber2;
        User Subscriber2
        {
            get
            {
                if (__subscriber2 == null)
                    __subscriber2 = LoadOrCreateUser("subscriber2", "Subscriber Two", UserFolder);
                return __subscriber2;
            }
        }

        Node __userFolder;
        Node UserFolder
        {
            get
            {
                if (__userFolder == null)
                {
                    //__userFolder = Folder.Load(Path.GetParentPath(User.Administrator.Path));
                    var userFolder = Node.LoadNode(RepositoryPath.GetParentPath(User.Administrator.Path));
                    if (userFolder == null)
                        throw new ApplicationException("UserFolder cannot be found.");
                    __userFolder = userFolder as Node;
                }
                return __userFolder;
            }
        }

        private User LoadOrCreateUser(string name, string fullName, Node parentFolder)
        {
            string path = RepositoryPath.Combine(parentFolder.Path, name);
            AddPathToDelete(path);

            User user = User.LoadNode(path) as User;
            if (user == null)
            {
                user = new User(parentFolder);
                user.Name = name;
            }
            user.Email = name + "@email.com";
            user.Enabled = true;
            user.FullName = fullName;
            user.Save();
            return user;
        }

        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);
            foreach (string path in _pathsToDelete)
            {
                Node n = Node.LoadNode(path);
                if (n != null)
                    Node.ForceDelete(path);
            }

        }
        [TestInitialize()]
        public void ResetDatabaseAndConfig()
        {
            config.ImmediatelyEnabled = true;
            config.DailyEnabled = true;
            config.WeeklyEnabled = true;
            config.MonthlyEnabled = true;

            Subscription.DeleteAllSubscriptions();
            Event.DeleteAllEvents();
            Message.DeleteAllMessages();
            LastProcessTime.Reset();
            Configuration.Enabled = false;
        }

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            if (!Node.Exists("/Root/Localization/MessageTemplateResources.xml"))
                CreateResourceFile();
            Configuration.Enabled = true;
            NotificationHandler.StartNotificationSystem();
        }
        #endregion
        #region Accessor
        private class NotificationHandlerAccessor
        {
            static Type type;
            static NotificationHandlerAccessor()
            {
                type = TypeHandler.GetType("SenseNet.Messaging.NotificationHandler");
            }
            public static IEnumerable<Subscription> CollectEventsPerSubscription(NotificationFrequency freq, DateTime now)
            {
                var m = type.GetMethod("CollectEventsPerSubscription", BindingFlags.Static | BindingFlags.NonPublic);
                var result = m.Invoke(null, new object[] { freq, now });
                return (IEnumerable<Subscription>)result;
            }
        }
        #endregion

        private const string TESTSITEPATH = "/Root";
        private const string TESTSITEURL = "http://site1.com";

        //======================================================================== Subscribing

        [TestMethod]
        public void Notification_SubscriptionSave()
        {
            var user = Subscriber1;

            var subscription = new Subscription
            {
                UserEmail = user.Email,
                UserId = user.Id,
                UserPath = user.Path,
                UserName = user.FullName,
                ContentPath = "/Root/IMS",
                Frequency = NotificationFrequency.Weekly,
                Language = "en",
                IsActive = true
            };
            subscription.Save();

            subscription = new Subscription
            {
                UserEmail = user.Email,
                UserId = user.Id,
                UserPath = user.Path,
                UserName = user.FullName,
                ContentPath = "/Root/IMS/BuiltIn",
                Frequency = NotificationFrequency.Monthly,
                Language = "en",
                IsActive = true
            };
            subscription.Save();

            subscription = new Subscription
            {
                UserEmail = user.Email,
                UserId = user.Id,
                UserPath = user.Path,
                UserName = user.FullName,
                ContentPath = "/Root/IMS",
                Frequency = NotificationFrequency.Daily,
                Language = "en",
                IsActive = true
            };
            subscription.Save();

            var result = Subscription.GetAllSubscriptions();

            Assert.IsTrue(2 == result.Count(), "#1");
        }
        [TestMethod]
        public void Notification_Subscribe_Unsubscribe()
        {
            var user = Subscriber1;
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");

            Subscription.Subscribe(user, node1, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(1 == Subscription.GetAllSubscriptions().Count(), "#1");

            Subscription.Subscribe(user, node2, NotificationFrequency.Monthly, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(2 == Subscription.GetAllSubscriptions().Count(), "#2");

            Subscription.Subscribe(user, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(2 == Subscription.GetAllSubscriptions().Count(), "#3");

            Subscription.UnSubscribe(user, node1);
            Assert.IsTrue(1 == Subscription.GetAllSubscriptions().Count(), "#4");

            Subscription.UnSubscribe(user, node2);
            Assert.IsTrue(0 == Subscription.GetAllSubscriptions().Count(), "#5");
        }
        [TestMethod]
        public void Notification_Subscribe_GetSubscriptionsByUser()
        {
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");

            Subscription.Subscribe(Subscriber1, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.Subscribe(Subscriber1, node2, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.Subscribe(Subscriber2, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);

            var subscriptions = Subscription.GetSubscriptionsByUser(Subscriber1).ToArray();
            Assert.IsTrue(2 == subscriptions.Count(), "#1");
            Assert.IsTrue(subscriptions[0].UserPath == Subscriber1.Path, "#2");
            Assert.IsTrue(subscriptions[0].ContentPath == node1.Path, "#3");
            Assert.IsTrue(subscriptions[1].UserPath == Subscriber1.Path, "#4");
            Assert.IsTrue(subscriptions[1].ContentPath == node2.Path, "#5");

            subscriptions = Subscription.GetSubscriptionsByUser(Subscriber2).ToArray();
            Assert.IsTrue(1 == subscriptions.Count(), "#1");
            Assert.IsTrue(subscriptions[0].UserPath == Subscriber2.Path, "#2");
            Assert.IsTrue(subscriptions[0].ContentPath == node1.Path, "#3");
        }
        [TestMethod]
        public void Notification_Subscribe_UnsubscribeAll()
        {
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");

            Subscription.Subscribe(Subscriber1, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(1 == Subscription.GetAllSubscriptions().Count(), "#1");

            Subscription.Subscribe(Subscriber1, node2, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(2 == Subscription.GetAllSubscriptions().Count(), "#2");

            Subscription.Subscribe(Subscriber2, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(3 == Subscription.GetAllSubscriptions().Count(), "#3");

            Subscription.Subscribe(Subscriber2, node2, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(4 == Subscription.GetAllSubscriptions().Count(), "#4");

            Subscription.UnSubscribeAll(Subscriber2);

            var subscriptions = Subscription.GetAllSubscriptions().ToArray();
            Assert.IsTrue(2 == subscriptions.Count(), "#5");
            Assert.IsTrue(subscriptions[0].UserPath == Subscriber1.Path, "#6");
            Assert.IsTrue(subscriptions[1].UserPath == Subscriber1.Path, "#7");
        }
        [TestMethod]
        public void Notification_Subscribe_UnsubscribeFromContent()
        {
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");

            Subscription.Subscribe(Subscriber1, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(1 == Subscription.GetAllSubscriptions().Count(), "#1");

            Subscription.Subscribe(Subscriber1, node2, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(2 == Subscription.GetAllSubscriptions().Count(), "#2");

            Subscription.Subscribe(Subscriber2, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(3 == Subscription.GetAllSubscriptions().Count(), "#3");

            Subscription.Subscribe(Subscriber2, node2, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(4 == Subscription.GetAllSubscriptions().Count(), "#4");

            Subscription.UnSubscribeFrom(node1);

            var subscriptions = Subscription.GetAllSubscriptions().ToArray();
            Assert.IsTrue(2 == subscriptions.Count(), "#5");
            Assert.IsTrue(subscriptions[0].ContentPath == node2.Path, "#6");
            Assert.IsTrue(subscriptions[1].ContentPath == node2.Path, "#7");
        }
        [TestMethod]
        public void Notification_SubscriptionActivation()
        {
            var user = Subscriber1;
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");

            Subscription.Subscribe(user, node1, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(1 == Subscription.GetAllSubscriptions().Count(), "#1");

            Subscription.Subscribe(user, node2, NotificationFrequency.Monthly, "en", TESTSITEPATH, TESTSITEURL, true);
            var subscriptions = Subscription.GetAllSubscriptions().ToArray();
            Assert.IsTrue(2 == subscriptions.Count(), "#");
            Assert.IsTrue(2 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#2");
            Assert.IsTrue(0 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#3");

            var subscription = subscriptions[0];
            subscription.IsActive = false;
            subscription.Save();

            Assert.IsTrue(1 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#4");
            Assert.IsTrue(1 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#5");

            subscription = subscriptions[1];
            subscription.IsActive = false;
            subscription.Save();

            Assert.IsTrue(0 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#6");
            Assert.IsTrue(2 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#7");

        }
        [TestMethod]
        public void Notification_SubscriptionActivation_Static()
        {
            var user = Subscriber1;
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");

            Subscription.Subscribe(user, node1, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL, true);
            Assert.IsTrue(1 == Subscription.GetAllSubscriptions().Count(), "#1");

            Subscription.Subscribe(user, node2, NotificationFrequency.Monthly, "en", TESTSITEPATH, TESTSITEURL, true);
            var subscriptions = Subscription.GetAllSubscriptions().ToArray();
            Assert.IsTrue(2 == subscriptions.Count(), "#");
            Assert.IsTrue(2 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#2");
            Assert.IsTrue(0 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#3");

            Subscription.InactivateSubscription(user, node1);

            Assert.IsTrue(1 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#4");
            Assert.IsTrue(1 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#5");

            Subscription.InactivateSubscription(user, node2);

            Assert.IsTrue(0 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#6");
            Assert.IsTrue(2 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#7");

            Subscription.ActivateSubscription(user, node1);

            Assert.IsTrue(1 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#8");
            Assert.IsTrue(1 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#9");

            Subscription.ActivateSubscription(user, node2);

            Assert.IsTrue(2 == Subscription.GetActiveSubscriptionsByUser(user).Count(), "#10");
            Assert.IsTrue(0 == Subscription.GetInactiveSubscriptionsByUser(user).Count(), "#11");
        }
        [TestMethod]
        public void Notification_SubscriptionQueryMethods()
        {
            var user1 = Subscriber1;
            var user2 = Subscriber2;
            var node1 = Node.LoadNode("/Root/IMS");
            var node2 = Node.LoadNode("/Root/IMS/BuiltIn");
            var node3 = Node.LoadNode("/Root/System");

            Subscription.Subscribe(user1, node1, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.Subscribe(user1, node2, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.Subscribe(user1, node3, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.InactivateSubscription(user1, node1);

            Subscription.Subscribe(user2, node1, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.Subscribe(user2, node2, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.Subscribe(user2, node3, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL, true);
            Subscription.InactivateSubscription(user2, node2);
            Subscription.InactivateSubscription(user2, node3);

            Subscription subscription;
            Subscription[] subscriptions;
            var user1path = user1.Path;
            var user2path = user2.Path;

            subscriptions = Subscription.GetActiveSubscriptionsByUser(user1).ToArray();
            Assert.IsTrue(subscriptions.Length == 2, "#1");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#2");
            Assert.IsTrue(subscriptions[0].ContentPath == node2.Path, "#3");
            Assert.IsTrue(subscriptions[1].UserPath == user1path, "#4");
            Assert.IsTrue(subscriptions[1].ContentPath == node3.Path, "#5");

            subscriptions = Subscription.GetInactiveSubscriptionsByUser(user1).ToArray();
            Assert.IsTrue(subscriptions.Length == 1, "#10");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#11");
            Assert.IsTrue(subscriptions[0].ContentPath == node1.Path, "#12");

            subscriptions = Subscription.GetSubscriptionsByUser(user1).ToArray();
            Assert.IsTrue(subscriptions.Length == 3, "#20");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#21");
            Assert.IsTrue(subscriptions[0].ContentPath == node1.Path, "#22");
            Assert.IsTrue(subscriptions[1].UserPath == user1path, "#23");
            Assert.IsTrue(subscriptions[1].ContentPath == node2.Path, "#24");
            Assert.IsTrue(subscriptions[2].UserPath == user1path, "#25");
            Assert.IsTrue(subscriptions[2].ContentPath == node3.Path, "#26");

            subscription = Subscription.GetSubscriptionByUser(user1, node1);
            Assert.IsTrue(subscription.UserPath == user1path, "#31");
            Assert.IsTrue(subscription.ContentPath == node1.Path, "#32");

            subscription = Subscription.GetSubscriptionByUser(user1, node2);
            Assert.IsTrue(subscription.UserPath == user1path, "#41");
            Assert.IsTrue(subscription.ContentPath == node2.Path, "#42");

            subscription = Subscription.GetSubscriptionByUser(user1, node3);
            Assert.IsTrue(subscription.UserPath == user1path, "#51");
            Assert.IsTrue(subscription.ContentPath == node3.Path, "#52");

            subscription = Subscription.GetSubscriptionByUser(user1, node3, true);
            Assert.IsTrue(subscription.UserPath == user1path, "#61");
            Assert.IsTrue(subscription.ContentPath == node3.Path, "#62");

            //---

            subscriptions = Subscription.GetActiveSubscriptionsByUser(user1.Path).ToArray();
            Assert.IsTrue(subscriptions.Length == 2, "#101");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#102");
            Assert.IsTrue(subscriptions[0].ContentPath == node2.Path, "#103");
            Assert.IsTrue(subscriptions[1].UserPath == user1path, "#104");
            Assert.IsTrue(subscriptions[1].ContentPath == node3.Path, "#105");

            subscriptions = Subscription.GetInactiveSubscriptionsByUser(user1.Path).ToArray();
            Assert.IsTrue(subscriptions.Length == 1, "#110");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#111");
            Assert.IsTrue(subscriptions[0].ContentPath == node1.Path, "#112");

            subscriptions = Subscription.GetSubscriptionsByUser(user1.Path).ToArray();
            Assert.IsTrue(subscriptions.Length == 3, "#120");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#121");
            Assert.IsTrue(subscriptions[0].ContentPath == node1.Path, "#122");
            Assert.IsTrue(subscriptions[1].UserPath == user1path, "#123");
            Assert.IsTrue(subscriptions[1].ContentPath == node2.Path, "#124");
            Assert.IsTrue(subscriptions[2].UserPath == user1path, "#125");
            Assert.IsTrue(subscriptions[2].ContentPath == node3.Path, "#126");

            subscription = Subscription.GetSubscriptionByUser(user1.Path, node1.Path);
            Assert.IsTrue(subscription.UserPath == user1path, "#131");
            Assert.IsTrue(subscription.ContentPath == node1.Path, "#132");

            subscription = Subscription.GetSubscriptionByUser(user1.Path, node2.Path);
            Assert.IsTrue(subscription.UserPath == user1path, "#141");
            Assert.IsTrue(subscription.ContentPath == node2.Path, "#142");

            subscription = Subscription.GetSubscriptionByUser(user1.Path, node3.Path);
            Assert.IsTrue(subscription.UserPath == user1path, "#151");
            Assert.IsTrue(subscription.ContentPath == node3.Path, "#152");

            subscription = Subscription.GetSubscriptionByUser(user1.Path, node3.Path, true);
            Assert.IsTrue(subscription.UserPath == user1path, "#161");
            Assert.IsTrue(subscription.ContentPath == node3.Path, "#162");

            //----

            subscriptions = Subscription.GetSubscriptionsByContent(node3).ToArray();
            Assert.IsTrue(subscriptions.Length == 2, "#220");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#221");
            Assert.IsTrue(subscriptions[0].ContentPath == node3.Path, "#222");
            Assert.IsTrue(subscriptions[1].UserPath == user2path, "#223");
            Assert.IsTrue(subscriptions[1].ContentPath == node3.Path, "#224");

            subscriptions = Subscription.GetSubscriptionsByContent(node3.Path).ToArray();
            Assert.IsTrue(subscriptions.Length == 2, "#320");
            Assert.IsTrue(subscriptions[0].UserPath == user1path, "#321");
            Assert.IsTrue(subscriptions[0].ContentPath == node3.Path, "#322");
            Assert.IsTrue(subscriptions[1].UserPath == user2path, "#323");
            Assert.IsTrue(subscriptions[1].ContentPath == node3.Path, "#324");
        }

        //======================================================================== Event creating and collecting

        [TestMethod]
        public void Notification_Event_WithoutSubscription()
        {
            Configuration.Enabled = true;

            var content = Content.CreateNew("Car", TestRoot, "car_WithoutSubscription");
            content.Save();

            content.ContentHandler.Index++;
            content.Save();

            Configuration.Enabled = false;

            Assert.IsTrue(Event.GetCountOfEvents() == 0);
        }

        [TestMethod]
        public void Notification_Event_CreationAndDeletion()
        {
            Configuration.Enabled = true;
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            var content = Content.CreateNew("Car", TestRoot, "car_CreationAndDeletion");
            content.Save();
            content.ForceDelete();

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 2, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.Created, "#2");
            Assert.IsTrue(events[1].NotificationType == NotificationType.Deleted, "#3");
        }
        [TestMethod]
        public void Notification_Event_Modification_None()
        {
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            var content = Content.CreateNew("Car", TestRoot, "car_Modification_None");
            content.Save();

            Configuration.Enabled = true;
            content.ContentHandler.Index++;
            content.Save();
            Configuration.Enabled = false;

            var events = Event.GetAllEvents();
            Assert.IsTrue(events.Count() > 0, "#1");
            Assert.IsTrue(events.First().NotificationType == NotificationType.MajorVersionModified, "#2");
        }
        [TestMethod]
        public void Notification_Event_Modification_MajorOnly()
        {
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            var content = Content.CreateNew("Car", TestRoot, "car_Modification_MajorOnly");
            ((GenericContent)content.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorOnly;
            content.Save();

            Configuration.Enabled = true;

            content.CheckOut();
            content.ContentHandler.Index++;
            content.Save();
            content.CheckIn();

            content.CheckOut();
            content.ContentHandler.Index++;
            content.Save();
            content.UndoCheckOut();

            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 6, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.MinorVersionModified, "#2");
            Assert.IsTrue(events[1].NotificationType == NotificationType.MinorVersionModified, "#3");
            Assert.IsTrue(events[2].NotificationType == NotificationType.MajorVersionModified, "#4");
            Assert.IsTrue(events[0].NotificationType == NotificationType.MinorVersionModified, "#5");
            Assert.IsTrue(events[1].NotificationType == NotificationType.MinorVersionModified, "#6");
            Assert.IsTrue(events[2].NotificationType == NotificationType.MajorVersionModified, "#7");
        }
        [TestMethod]
        public void Notification_Event_Modification_MajorAndMinor()
        {
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            var content = Content.CreateNew("Car", TestRoot, "car_Modification_MajorAndMinor");
            ((GenericContent)content.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            content.Save();

            Configuration.Enabled = true;

            content.CheckOut();              // {V0.2.L}
            content.ContentHandler.Index++;
            content.Save();                  // {V0.2.L}
            content.CheckIn();               // {V0.2.D}

            content.CheckOut();              // {V0.3.L}
            content.ContentHandler.Index++;
            content.Save();                  // {V0.3.L}
            content.UndoCheckOut();          // {V0.2.D}

            content.CheckOut();              // {V0.3.L}
            content.ContentHandler.Index++;
            content.Save();                  // {V0.3.L}
            content.Publish();               // {V1.0.A}

            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 9, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.MinorVersionModified, "#2");
            Assert.IsTrue(events[1].NotificationType == NotificationType.MinorVersionModified, "#3");
            Assert.IsTrue(events[2].NotificationType == NotificationType.MinorVersionModified, "#4");
            Assert.IsTrue(events[3].NotificationType == NotificationType.MinorVersionModified, "#5");
            Assert.IsTrue(events[4].NotificationType == NotificationType.MinorVersionModified, "#6");
            Assert.IsTrue(events[5].NotificationType == NotificationType.MinorVersionModified, "#7");
            Assert.IsTrue(events[6].NotificationType == NotificationType.MinorVersionModified, "#8");
            Assert.IsTrue(events[7].NotificationType == NotificationType.MinorVersionModified, "#9");
            Assert.IsTrue(events[8].NotificationType == NotificationType.MajorVersionModified, "#10");
        }

        [TestMethod]
        public void Notification_Event_Copy()
        {
            var source = Content.CreateNew("Folder", TestRoot, "source1");
            source.Save();
            var subFolder = Content.CreateNew("Folder", source.ContentHandler, "folder1");
            subFolder.Save();
            var doc = Content.CreateNew("Car", subFolder.ContentHandler, "car_Copy");
            doc.Save();
            var target = Content.CreateNew("Folder", TestRoot, "target1");
            target.Save();

            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);

            Configuration.Enabled = true;
            source.ContentHandler.CopyTo(target.ContentHandler);
            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 3, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.CopiedFrom, "#2");
            Assert.IsTrue(events[0].ContentPath == String.Format("{0}/target1/source1", TestRoot.Path), "#3");
            Assert.IsTrue(events[1].NotificationType == NotificationType.CopiedFrom, "#4");
            Assert.IsTrue(events[1].ContentPath == String.Format("{0}/target1/source1/folder1", TestRoot.Path), "5");
            Assert.IsTrue(events[2].NotificationType == NotificationType.CopiedFrom, "#6");
            Assert.IsTrue(events[2].ContentPath == String.Format("{0}/target1/source1/folder1/car_Copy", TestRoot.Path), "#7");
        }
        [TestMethod]
        public void Notification_Event_Move()
        {
            var source = Content.CreateNew("Folder", TestRoot, "source2");
            source.Save();
            var subFolder = Content.CreateNew("Folder", source.ContentHandler, "folder1");
            subFolder.Save();
            var doc = Content.CreateNew("Car", subFolder.ContentHandler, "car_Move");
            doc.Save();
            var target = Content.CreateNew("Folder", TestRoot, "target2");
            target.Save();

            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);

            Configuration.Enabled = true;
            source.ContentHandler.MoveTo(target.ContentHandler);
            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 2, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.MovedTo, "#2");
            Assert.IsTrue(events[0].ContentPath == String.Format("{0}/source2", TestRoot.Path), "#3");
            Assert.IsTrue(events[1].NotificationType == NotificationType.MovedFrom, "#4");
            Assert.IsTrue(events[1].ContentPath == String.Format("{0}/target2/source2", TestRoot.Path), "#5");
        }
        [TestMethod]
        public void Notification_Event_MoveWithDeepSubscription()
        {
            var source = Content.CreateNew("Folder", TestRoot, "source");
            source.Save();
            var subFolder = Content.CreateNew("Folder", source.ContentHandler, "folder1");
            subFolder.Save();
            var doc = Content.CreateNew("Car", subFolder.ContentHandler, "car_MoveWithDeepSubscription");
            doc.Save();
            var target = Content.CreateNew("Folder", TestRoot, "target");
            target.Save();

            Subscription.Subscribe(Subscriber1, doc.ContentHandler, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);

            Configuration.Enabled = true;
            source.ContentHandler.MoveTo(target.ContentHandler);
            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 2, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.MovedTo, "#2");
            Assert.IsTrue(events[0].ContentPath == String.Format("{0}/source", TestRoot.Path), "#3");
            Assert.IsTrue(events[1].NotificationType == NotificationType.MovedFrom, "#4");
            Assert.IsTrue(events[1].ContentPath == String.Format("{0}/target/source", TestRoot.Path), "5");
        }

        [TestMethod]
        public void Notification_Event_DeleteRestore()
        {
            var trash = Node.Load<TrashBin>("/Root/Trash");
            if (trash == null)
            {
                trash = new TrashBin(Repository.Root);
                trash.Name = "Trash";
                trash.IsActive = true;
                trash.Save();
            }
            else
            {
                if (!trash.IsActive)
                {
                    trash.IsActive = true;
                    trash.Save();
                }
            }
            TrashBin.Purge();

            var source = Content.CreateNew("Folder", TestRoot, "DeleteRestore");
            source.Save();
            var doc = Content.CreateNew("Car", source.ContentHandler, "car_DeleteRestore");
            doc.Save();

            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);

            Configuration.Enabled = true;
            source.Delete();
            var bags = TrashBin.Instance.Children.OfType<TrashBag>().ToArray();
            var bag = bags.FirstOrDefault(x => ((TrashBag)x).DeletedContent.Path.EndsWith("/DeleteRestore"));
            if (bag == null)
                Assert.Inconclusive();
            TrashBin.Restore(bag);
            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 2, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.Deleted, "#2");
            Assert.IsTrue(events[0].ContentPath == source.Path, String.Concat("#3: Path is: ", events[0].ContentPath, ", expected: ", source.Path));
            Assert.IsTrue(events[1].NotificationType == NotificationType.Restored, "#4");
            Assert.IsTrue(events[1].ContentPath == source.Path, String.Concat("#5: Path is: ", events[1].ContentPath, ", expected: ", source.Path));
        }
        [TestMethod]
        public void Notification_Event_Rename()
        {
            var source = Content.CreateNew("Folder", TestRoot, "Rename");
            source.Save();
            var originalPath = source.Path;

            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);

            Configuration.Enabled = true;
            source.ContentHandler.Name = "Renamed";
            source.Save();
            Configuration.Enabled = false;

            var events = (Event[])Event.GetAllEvents();
            Assert.IsTrue(events.Length == 2, "#1");
            Assert.IsTrue(events[0].NotificationType == NotificationType.RenamedTo, "#2");
            Assert.IsTrue(events[0].ContentPath == originalPath, "#3");
            Assert.IsTrue(events[1].NotificationType == NotificationType.RenamedFrom, "#4");
            Assert.IsTrue(events[1].ContentPath == source.Path, "#5");
        }

        [TestMethod]
        public void Notification_Message1()
        {
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber2, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            TestRoot.Security.SetPermission(Subscriber1, true, PermissionType.OpenMinor, ContentRepository.Storage.Security.PermissionValue.Allow);
            TestRoot.Security.SetPermission(Subscriber2, true, PermissionType.Open, ContentRepository.Storage.Security.PermissionValue.Allow);

            var content1 = Content.CreateNew("Car", TestRoot, "car_msg1");
            var content2 = Content.CreateNew("Car", TestRoot, "car_msg2");
            ((GenericContent)content1.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            ((GenericContent)content2.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            content1.Save();
            content2.Save();

            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg2", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg2", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg2", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");

            var subscriptions = NotificationHandlerAccessor.CollectEventsPerSubscription(NotificationFrequency.Immediately, DateTime.Now).ToArray();

            Assert.IsTrue(subscriptions.Length == 2, "#1");
            if (subscriptions[0].UserId != Subscriber1.Id)
                Assert.Inconclusive();
            if (subscriptions[1].UserId != Subscriber2.Id)
                Assert.Inconclusive();

            var events0 = subscriptions[0].RelatedEvents.ToArray();
            Assert.IsTrue(events0.Length == 4, String.Format("#11: events1.Length: {0}, expected: 4", events0.Length));
            Assert.IsTrue(events0[0].NotificationType == NotificationType.MinorVersionModified, "#3");
            Assert.IsTrue(events0[0].ContentPath == "/Root/_NotificationTests/car_msg2", "#4");
            Assert.IsTrue(events0[1].NotificationType == NotificationType.MinorVersionModified, "#5");
            Assert.IsTrue(events0[1].ContentPath == "/Root/_NotificationTests/car_msg1", "#6");
            Assert.IsTrue(events0[2].NotificationType == NotificationType.MajorVersionModified, "#7");
            Assert.IsTrue(events0[2].ContentPath == "/Root/_NotificationTests/car_msg1", "#8");
            Assert.IsTrue(events0[3].NotificationType == NotificationType.MajorVersionModified, "#9");
            Assert.IsTrue(events0[3].ContentPath == "/Root/_NotificationTests/car_msg2", "#10");

            var events1 = subscriptions[1].RelatedEvents.ToArray();
            Assert.IsTrue(events1.Length == 2, String.Format("#11: events1.Length: {0}, expected: 2", events1.Length));
            Assert.IsTrue(events1[0].NotificationType == NotificationType.MajorVersionModified, "#12");
            Assert.IsTrue(events1[0].ContentPath == "/Root/_NotificationTests/car_msg1", "#13");
            Assert.IsTrue(events1[1].NotificationType == NotificationType.MajorVersionModified, "#14");
            Assert.IsTrue(events1[1].ContentPath == "/Root/_NotificationTests/car_msg2", "#15");
        }
        [TestMethod]
        public void Notification_Message2()
        {
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber2, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            TestRoot.Security.SetPermission(Subscriber1, true, PermissionType.OpenMinor, ContentRepository.Storage.Security.PermissionValue.Allow);
            TestRoot.Security.SetPermission(Subscriber2, true, PermissionType.Open, ContentRepository.Storage.Security.PermissionValue.Allow);

            var content1 = Content.CreateNew("Car", TestRoot, "car_msg1");
            var content2 = Content.CreateNew("Car", TestRoot, "car_msg2");
            ((GenericContent)content1.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            ((GenericContent)content2.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            content1.Save();
            content2.Save();

            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg2", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg2", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg1", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");
            Event.CreateAndSave("/Root/_NotificationTests/car_msg2", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");

            NotificationHandler.GenerateMessages(NotificationFrequency.Immediately, DateTime.Now);
            var messages = Message.GetAllMessages().ToArray();

            Assert.Inconclusive();
        }

        [TestMethod]
        public void Notification_Message_LastExecutionTime()
        {
            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            TestRoot.Security.SetPermission(Subscriber1, true, PermissionType.OpenMinor, ContentRepository.Storage.Security.PermissionValue.Allow);

            var content1 = Content.CreateNew("Car", TestRoot, "car_LastExecutionTime");
            content1.Save();

            IEnumerable<Subscription> subscriptions;

            // The time of the operation always will be written into the database after collecting subscriptions.
            // The next collecting process uses this time if the NotificationFrequency is Immediately
            // This ensures that a message will be generated once one

            Event.CreateAndSave("/Root/_NotificationTests", 1, 1, NotificationType.MajorVersionModified, "\\Administrator");
            subscriptions = NotificationHandlerAccessor.CollectEventsPerSubscription(NotificationFrequency.Immediately, DateTime.Now).ToArray();
            Assert.IsTrue(subscriptions.Count() == 1, "#1");

            Event.CreateAndSave("/Root/_NotificationTests", 1, 1, NotificationType.MinorVersionModified, "\\Administrator");
            subscriptions = NotificationHandlerAccessor.CollectEventsPerSubscription(NotificationFrequency.Immediately, DateTime.Now).ToArray();
            Assert.IsTrue(subscriptions.Count() == 1, "#2");

        }

        //======================================================================== Next time computing

        [TestMethod]
        public void Notification_NextTime_Daily()
        {
            string msg;
            msg = TestNextDailyTime(1, 23, 0, "2011-01-01 10:10:10", "2011-01-01 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextDailyTime(2, 23, 0, "2011-01-01 23:10:10", "2011-01-02 23:00:00"); Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Notification_NextTime_Weekly()
        {
            string msg;
            msg = TestNextWeeklyTime(1, 23, 0, DayOfWeek.Sunday, "2011-03-14 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(2, 23, 0, DayOfWeek.Sunday, "2011-03-14 23:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(3, 23, 0, DayOfWeek.Sunday, "2011-03-15 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(4, 23, 0, DayOfWeek.Sunday, "2011-03-15 23:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(5, 23, 0, DayOfWeek.Sunday, "2011-03-16 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(6, 23, 0, DayOfWeek.Sunday, "2011-03-16 23:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(7, 23, 0, DayOfWeek.Sunday, "2011-03-17 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(8, 23, 0, DayOfWeek.Sunday, "2011-03-17 23:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(9, 23, 0, DayOfWeek.Sunday, "2011-03-18 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(10, 23, 0, DayOfWeek.Sunday, "2011-03-18 23:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(11, 23, 0, DayOfWeek.Sunday, "2011-03-19 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(12, 23, 0, DayOfWeek.Sunday, "2011-03-19 23:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(13, 23, 0, DayOfWeek.Sunday, "2011-03-20 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg); //Sunday
            msg = TestNextWeeklyTime(14, 23, 0, DayOfWeek.Sunday, "2011-03-20 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Sunday
            msg = TestNextWeeklyTime(15, 23, 0, DayOfWeek.Sunday, "2011-03-21 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(16, 23, 0, DayOfWeek.Sunday, "2011-03-21 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(17, 23, 0, DayOfWeek.Sunday, "2011-03-22 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(18, 23, 0, DayOfWeek.Sunday, "2011-03-22 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(19, 23, 0, DayOfWeek.Sunday, "2011-03-23 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(20, 23, 0, DayOfWeek.Sunday, "2011-03-23 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(21, 23, 0, DayOfWeek.Sunday, "2011-03-24 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(22, 23, 0, DayOfWeek.Sunday, "2011-03-24 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(23, 23, 0, DayOfWeek.Sunday, "2011-03-25 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(24, 23, 0, DayOfWeek.Sunday, "2011-03-25 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(25, 23, 0, DayOfWeek.Sunday, "2011-03-26 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(26, 23, 0, DayOfWeek.Sunday, "2011-03-26 23:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(27, 23, 0, DayOfWeek.Sunday, "2011-03-27 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg); //Sunday
            msg = TestNextWeeklyTime(28, 23, 0, DayOfWeek.Sunday, "2011-03-27 23:10:10", "2011-04-03 23:00:00"); Assert.IsNull(msg, msg); //Sunday

            msg = TestNextWeeklyTime(301, 23, 0, DayOfWeek.Wednesday, "2011-03-14 10:10:10", "2011-03-16 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(302, 23, 0, DayOfWeek.Wednesday, "2011-03-14 23:10:10", "2011-03-16 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(303, 23, 0, DayOfWeek.Wednesday, "2011-03-15 10:10:10", "2011-03-16 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(304, 23, 0, DayOfWeek.Wednesday, "2011-03-15 23:10:10", "2011-03-16 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(305, 23, 0, DayOfWeek.Wednesday, "2011-03-16 10:10:10", "2011-03-16 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(306, 23, 0, DayOfWeek.Wednesday, "2011-03-16 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(307, 23, 0, DayOfWeek.Wednesday, "2011-03-17 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(308, 23, 0, DayOfWeek.Wednesday, "2011-03-17 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(309, 23, 0, DayOfWeek.Wednesday, "2011-03-18 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(310, 23, 0, DayOfWeek.Wednesday, "2011-03-18 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(311, 23, 0, DayOfWeek.Wednesday, "2011-03-19 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(312, 23, 0, DayOfWeek.Wednesday, "2011-03-19 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(313, 23, 0, DayOfWeek.Wednesday, "2011-03-20 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Sunday
            msg = TestNextWeeklyTime(314, 23, 0, DayOfWeek.Wednesday, "2011-03-20 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Sunday
            msg = TestNextWeeklyTime(315, 23, 0, DayOfWeek.Wednesday, "2011-03-21 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(316, 23, 0, DayOfWeek.Wednesday, "2011-03-21 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Monday
            msg = TestNextWeeklyTime(317, 23, 0, DayOfWeek.Wednesday, "2011-03-22 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(318, 23, 0, DayOfWeek.Wednesday, "2011-03-22 23:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Tuesday
            msg = TestNextWeeklyTime(319, 23, 0, DayOfWeek.Wednesday, "2011-03-23 10:10:10", "2011-03-23 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(320, 23, 0, DayOfWeek.Wednesday, "2011-03-23 23:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Wednesday
            msg = TestNextWeeklyTime(321, 23, 0, DayOfWeek.Wednesday, "2011-03-24 10:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(322, 23, 0, DayOfWeek.Wednesday, "2011-03-24 23:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Thursday
            msg = TestNextWeeklyTime(323, 23, 0, DayOfWeek.Wednesday, "2011-03-25 10:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(324, 23, 0, DayOfWeek.Wednesday, "2011-03-25 23:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Friday
            msg = TestNextWeeklyTime(325, 23, 0, DayOfWeek.Wednesday, "2011-03-26 10:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(326, 23, 0, DayOfWeek.Wednesday, "2011-03-26 23:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Saturday
            msg = TestNextWeeklyTime(327, 23, 0, DayOfWeek.Wednesday, "2011-03-27 10:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Sunday
            msg = TestNextWeeklyTime(328, 23, 0, DayOfWeek.Wednesday, "2011-03-27 23:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg); //Sunday
        }
        [TestMethod]
        public void Notification_NextTime_Monthly_EveryDayNr()
        {
            string msg;
            msg = TestNextMonthlyTime_EveryDayNr(1, 23, 0, 20, "2011-01-01 10:10:10", "2011-01-20 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(2, 23, 0, 20, "2011-01-19 23:10:10", "2011-01-20 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(3, 23, 0, 20, "2011-01-20 10:10:10", "2011-01-20 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(4, 23, 0, 20, "2011-01-20 23:10:10", "2011-02-20 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(5, 23, 0, 20, "2011-01-21 10:10:10", "2011-02-20 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(6, 23, 0, 20, "2011-01-22 10:10:10", "2011-02-20 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_EveryDayNr(10, 23, 0, 31, "2011-01-30 10:10:10", "2011-01-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(11, 23, 0, 31, "2011-01-30 23:10:10", "2011-01-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(12, 23, 0, 31, "2011-01-31 10:10:10", "2011-01-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(13, 23, 0, 31, "2011-01-31 23:10:10", "2011-02-28 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_EveryDayNr(20, 23, 0, 31, "2011-02-27 10:10:10", "2011-02-28 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(21, 23, 0, 31, "2011-02-27 23:10:10", "2011-02-28 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(22, 23, 0, 31, "2011-02-28 10:10:10", "2011-02-28 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(23, 23, 0, 31, "2011-02-28 23:10:10", "2011-03-31 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_EveryDayNr(30, 23, 0, 31, "2011-03-30 10:10:10", "2011-03-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(31, 23, 0, 31, "2011-03-30 23:10:10", "2011-03-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(32, 23, 0, 31, "2011-03-31 10:10:10", "2011-03-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(33, 23, 0, 31, "2011-03-31 23:10:10", "2011-04-30 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_EveryDayNr(40, 23, 0, 31, "2011-12-31 10:10:10", "2011-12-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_EveryDayNr(41, 23, 0, 31, "2011-12-31 23:10:10", "2012-01-31 23:00:00"); Assert.IsNull(msg, msg);

            var d1 = new DateTime(2011, 1, 31);
            var d2 = d1.AddMonths(1);
            var d3 = d1.AddMonths(2);
            var d4 = d1.AddMonths(3);
        }
        [TestMethod]
        public void Notification_NextTime_Monthly_WeekNrWeekDay()
        {
            string msg;
            msg = TestNextMonthlyTime_WeekNrWeekday(1, 23, 0, 1, DayOfWeek.Sunday, "2011-03-01 10:10:10", "2011-03-06 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_WeekNrWeekday(2, 23, 0, 2, DayOfWeek.Sunday, "2011-03-01 10:10:10", "2011-03-13 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_WeekNrWeekday(3, 23, 0, 3, DayOfWeek.Sunday, "2011-03-01 10:10:10", "2011-03-20 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_WeekNrWeekday(4, 23, 0, 4, DayOfWeek.Sunday, "2011-03-01 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_WeekNrWeekday(5, 23, 0, 1, DayOfWeek.Sunday, "2011-03-19 10:10:10", "2011-04-03 23:00:00"); Assert.IsNull(msg, msg);
        }
        [TestMethod]
        public void Notification_NextTime_Monthly_LastWeekDay()
        {
            string msg;
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Sunday, "2011-03-01 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Sunday, "2011-03-27 10:10:10", "2011-03-27 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Sunday, "2011-03-27 23:10:10", "2011-04-24 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Sunday, "2011-03-30 10:10:10", "2011-04-24 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Monday, "2011-03-01 10:10:10", "2011-03-28 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Monday, "2011-03-28 10:10:10", "2011-03-28 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Monday, "2011-03-28 23:10:10", "2011-04-25 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Monday, "2011-03-30 10:10:10", "2011-04-25 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Tuesday, "2011-03-01 10:10:10", "2011-03-29 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Tuesday, "2011-03-29 10:10:10", "2011-03-29 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Tuesday, "2011-03-29 23:10:10", "2011-04-26 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Wednesday, "2011-03-01 10:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Wednesday, "2011-03-30 10:10:10", "2011-03-30 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Wednesday, "2011-03-30 23:10:10", "2011-04-27 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Thursday, "2011-03-01 10:10:10", "2011-03-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Thursday, "2011-03-31 10:10:10", "2011-03-31 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Thursday, "2011-03-31 23:10:10", "2011-04-28 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Friday, "2011-03-01 10:10:10", "2011-03-25 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Friday, "2011-03-25 10:10:10", "2011-03-25 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Friday, "2011-03-25 23:10:10", "2011-04-29 23:00:00"); Assert.IsNull(msg, msg);

            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Saturday, "2011-03-01 10:10:10", "2011-03-26 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Saturday, "2011-03-26 10:10:10", "2011-03-26 23:00:00"); Assert.IsNull(msg, msg);
            msg = TestNextMonthlyTime_LastWeekday(1, 23, 0, DayOfWeek.Saturday, "2011-03-26 23:10:10", "2011-04-30 23:00:00"); Assert.IsNull(msg, msg);
        }

        //         Mo   Tu   We   Th   Fr   Sa   Su
        //2011.03        1    2    3    4    5    6
        //          7    8    9   10   11   12   13
        //         14   15   16   17   18   19   20
        //         21   22   23   24   25   26   27
        //         28   29   30   31
        //2011.04                       1    2    3
        //          4    5    6    7    8    9   10
        //         11   12   13   14   15   16   17
        //         18   19   20   21   22   23   24
        //         25   26   27   28   29   30
        //2011.05                                 1
        //          2    3    4    5    6    7    8
        //          9   10   11   12   13   14   15
        //         16   17   18   19   20   21   22
        //         23   24   25   26   27   28   29
        //         30

        private string TestNextDailyTime(int testNr, int configHour, int configMinute, string nowstr, string expectedstr)
        {
            config.DailyHour = configHour;
            config.DailyMinute = configMinute;

            var nextTimeStr = LastProcessTime.GetNextDailyTime(DateTime.Parse(nowstr)).ToString("yyyy-MM-dd HH:mm:ss");
            return nextTimeStr == expectedstr ? null :
                String.Format("Daily#{0}: Time is {1}, expected: {2}", testNr, nextTimeStr, expectedstr);
        }
        private string TestNextWeeklyTime(int testNr, int configHour, int configMinute, DayOfWeek weekDay, string nowstr, string expectedstr)
        {
            config.WeeklyHour = configHour;
            config.WeeklyMinute = configMinute;
            config.WeeklyWeekDay = weekDay;

            var nextTimeStr = LastProcessTime.GetNextWeeklyTime(DateTime.Parse(nowstr)).ToString("yyyy-MM-dd HH:mm:ss");
            return nextTimeStr == expectedstr ? null :
                String.Format("Weekly#{0}: Time is {1}, expected: {2}", testNr, nextTimeStr, expectedstr);
        }
        private string TestNextMonthlyTime_WeekNrWeekday(int testNr, int configHour, int configMinute, int weekNr, DayOfWeek weekDay, string nowstr, string expectedstr)
        {
            config.MonthlyEvery = false;
            config.MonthlyLast = false;
            config.MonthlyHour = configHour;
            config.MonthlyMinute = configMinute;
            config.MonthlyWeek = weekNr;
            config.MonthlyWeekDay = weekDay;

            var nextTimeStr = LastProcessTime.GetNextMonthlyTime(DateTime.Parse(nowstr)).ToString("yyyy-MM-dd HH:mm:ss");
            return nextTimeStr == expectedstr ? null :
                String.Format("Weekly#{0}: Time is {1}, expected: {2}", testNr, nextTimeStr, expectedstr);
        }
        private string TestNextMonthlyTime_LastWeekday(int testNr, int configHour, int configMinute, DayOfWeek weekDay, string nowstr, string expectedstr)
        {
            config.MonthlyEvery = false;
            config.MonthlyLast = true;
            config.MonthlyHour = configHour;
            config.MonthlyMinute = configMinute;
            config.MonthlyWeekDay = weekDay;

            var nextTimeStr = LastProcessTime.GetNextMonthlyTime(DateTime.Parse(nowstr)).ToString("yyyy-MM-dd HH:mm:ss");
            return nextTimeStr == expectedstr ? null :
                String.Format("Weekly#{0}: Time is {1}, expected: {2}", testNr, nextTimeStr, expectedstr);
        }
        private string TestNextMonthlyTime_EveryDayNr(int testNr, int configHour, int configMinute, int day, string nowstr, string expectedstr)
        {
            config.MonthlyEvery = true;
            config.MonthlyHour = configHour;
            config.MonthlyMinute = configMinute;
            config.MonthlyDay = day;

            var nextTimeStr = LastProcessTime.GetNextMonthlyTime(DateTime.Parse(nowstr)).ToString("yyyy-MM-dd HH:mm:ss");
            return nextTimeStr == expectedstr ? null :
                String.Format("Weekly#{0}: Time is {1}, expected: {2}", testNr, nextTimeStr, expectedstr);
        }

        //======================================================================== Configuration parsing

        [TestMethod]
        public void Notification_ConfigParsing_Daily()
        {
            config.ParseDaily("10:20");
            Assert.IsTrue(config.DailyEnabled, "config.DailyEnabled is false but expected is true.");
            Assert.AreEqual(config.DailyHour, 10);
            Assert.AreEqual(config.DailyMinute, 20);

            config.ParseDaily("Never");
            Assert.IsFalse(config.DailyEnabled, "config.DailyEnabled is true but expected is false.");

            try
            {
                config.ParseDaily("24:20");
                Assert.Fail("Exception was not thrown: 24:20");
            }
            catch (System.Configuration.ConfigurationException) { }

            try
            {
                config.ParseDaily("23:61");
                Assert.Fail("Exception was not thrown: 23:61");
            }
            catch (System.Configuration.ConfigurationException) { }

        }
        [TestMethod]
        public void Notification_ConfigParsing_Weekly()
        {
            config.ParseWeekly("Sunday 22:12");
            Assert.IsTrue(config.WeeklyEnabled, "config.WeeklyEnabled is false but expected is true.");
            Assert.AreEqual(config.WeeklyWeekDay, DayOfWeek.Sunday);
            Assert.AreEqual(config.WeeklyHour, 22);
            Assert.AreEqual(config.WeeklyMinute, 12);

            config.ParseWeekly("Never");
            Assert.IsFalse(config.WeeklyEnabled, "config.WeeklyEnabled is true but expected is false.");

            try
            {
                config.ParseDaily("Hetfo 22:12");
                Assert.Fail("Exception was not thrown: Hetfo 22:12");
            }
            catch (System.Configuration.ConfigurationException) { }
        }
        [TestMethod]
        public void Notification_ConfigParsing_Monthly()
        {
            config.ParseMonthly("1st Sunday 23:10");
            Assert.IsTrue(config.MonthlyEnabled, "config.MonthlyEnabled is false but expected is true.");
            Assert.IsFalse(config.MonthlyEvery, "MonthlyEvery = true, expected: false");
            Assert.IsFalse(config.MonthlyLast, "MonthlyLast = true, expected: false");
            Assert.AreEqual(config.MonthlyWeek, 1);
            Assert.AreEqual(config.MonthlyWeekDay, DayOfWeek.Sunday);
            Assert.AreEqual(config.MonthlyHour, 23);
            Assert.AreEqual(config.MonthlyMinute, 10);

            config.ParseMonthly("2nd Sunday 23:10");
            Assert.IsTrue(config.MonthlyEnabled, "config.MonthlyEnabled is false but expected is true.");
            Assert.IsFalse(config.MonthlyEvery, "MonthlyEvery = true, expected: false");
            Assert.IsFalse(config.MonthlyLast, "MonthlyLast = true, expected: false");
            Assert.AreEqual(config.MonthlyWeek, 2);
            Assert.AreEqual(config.MonthlyWeekDay, DayOfWeek.Sunday);
            Assert.AreEqual(config.MonthlyHour, 23);
            Assert.AreEqual(config.MonthlyMinute, 10);

            config.ParseMonthly("3rd Sunday 23:10");
            Assert.IsTrue(config.MonthlyEnabled, "config.MonthlyEnabled is false but expected is true.");
            Assert.IsFalse(config.MonthlyEvery, "MonthlyEvery = true, expected: false");
            Assert.IsFalse(config.MonthlyLast, "MonthlyLast = true, expected: false");
            Assert.AreEqual(config.MonthlyWeek, 3);
            Assert.AreEqual(config.MonthlyWeekDay, DayOfWeek.Sunday);
            Assert.AreEqual(config.MonthlyHour, 23);
            Assert.AreEqual(config.MonthlyMinute, 10);

            config.ParseMonthly("4th Sunday 23:10");
            Assert.IsTrue(config.MonthlyEnabled, "config.MonthlyEnabled is false but expected is true.");
            Assert.IsFalse(config.MonthlyEvery, "MonthlyEvery = true, expected: false");
            Assert.IsFalse(config.MonthlyLast, "MonthlyLast = true, expected: false");
            Assert.AreEqual(config.MonthlyWeek, 4);
            Assert.AreEqual(config.MonthlyWeekDay, DayOfWeek.Sunday);
            Assert.AreEqual(config.MonthlyHour, 23);
            Assert.AreEqual(config.MonthlyMinute, 10);

            config.ParseMonthly("Last Sunday 23:10");
            Assert.IsTrue(config.MonthlyEnabled, "config.MonthlyEnabled is false but expected is true.");
            Assert.IsFalse(config.MonthlyEvery, "MonthlyEvery = true, expected: false");
            Assert.IsTrue(config.MonthlyLast, "MonthlyLast = false, expected: true");
            Assert.AreEqual(config.MonthlyWeekDay, DayOfWeek.Sunday);
            Assert.AreEqual(config.MonthlyHour, 23);
            Assert.AreEqual(config.MonthlyMinute, 10);

            config.ParseMonthly("Every 5, 23:10");
            Assert.IsTrue(config.MonthlyEnabled, "config.MonthlyEnabled is false but expected is true.");
            Assert.IsTrue(config.MonthlyEvery, "MonthlyEvery = false, expected: true");
            Assert.IsFalse(config.MonthlyLast, "MonthlyLast = true, expected: false");
            Assert.AreEqual(config.MonthlyDay, 5);
            Assert.AreEqual(config.MonthlyHour, 23);
            Assert.AreEqual(config.MonthlyMinute, 10);

            config.ParseMonthly("Never");
            Assert.IsFalse(config.MonthlyEnabled, "config.MonthlyEnabled is true but expected is false.");

            try
            {
                config.ParseMonthly("Every 5,");
                Assert.Fail("Exception was not thrown: Every 5,");
            }
            catch (System.Configuration.ConfigurationException) { }

            try
            {
                config.ParseMonthly("Every 5 23:10");
                Assert.Fail("Exception was not thrown: Every 5 23:10");
            }
            catch (System.Configuration.ConfigurationException) { }

            try
            {
                config.ParseMonthly("0. Sunday 23:10");
                Assert.Fail("Exception was not thrown: 0. Sunday 23:10");
            }
            catch (System.Configuration.ConfigurationException) { }
            try
            {
                config.ParseMonthly("1. Sunday 23:10");
                Assert.Fail("Exception was not thrown: 1. Sunday 23:10");
            }
            catch (System.Configuration.ConfigurationException) { }

            try
            {
                Configuration.ParseMonthly("Hetfo 23:10");
                Assert.Fail("Exception was not thrown: Hetfo 23:10");
            }
            catch (System.Configuration.ConfigurationException) { }
        }

        //======================================================================== Timer tick

        [TestMethod]
        public void Notification_Timer_1()
        {
            TestRoot.Security.SetPermission(Subscriber1, true, PermissionType.OpenMinor, ContentRepository.Storage.Security.PermissionValue.Allow);
            TestRoot.Security.SetPermission(Subscriber2, true, PermissionType.Open, ContentRepository.Storage.Security.PermissionValue.Allow);

            var folder1 = Content.CreateNew("Folder", TestRoot, "Folder_timer1");
            folder1.Save();
            var folder2 = Content.CreateNew("Folder", folder1.ContentHandler, "Folder_timer2");
            folder2.Save();
            var folder3 = Content.CreateNew("Folder", folder2.ContentHandler, "Folder_timer3");
            folder3.Save();

            Subscription.Subscribe(Subscriber1, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber2, TestRoot, NotificationFrequency.Immediately, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber1, folder1.ContentHandler, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber2, folder1.ContentHandler, NotificationFrequency.Daily, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber1, folder2.ContentHandler, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber2, folder2.ContentHandler, NotificationFrequency.Weekly, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber1, folder3.ContentHandler, NotificationFrequency.Monthly, "en", TESTSITEPATH, TESTSITEURL);
            Subscription.Subscribe(Subscriber2, folder3.ContentHandler, NotificationFrequency.Monthly, "en", TESTSITEPATH, TESTSITEURL);

            var content1 = Content.CreateNew("Car", folder3.ContentHandler, "car_timer1");
            var content2 = Content.CreateNew("Car", folder3.ContentHandler, "car_timer2");
            ((GenericContent)content1.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            ((GenericContent)content2.ContentHandler).VersioningMode = ContentRepository.Versioning.VersioningType.MajorAndMinor;
            content1.Save();
            content2.Save();
            var content1path = content1.Path;
            var content2path = content2.Path;

            Event.CreateAndSave(content1path, 1, 1, NotificationType.Created, "\\Administrator", DateTime.Parse("2011-03-22 08:00:01"));
            Event.CreateAndSave(content1path, 1, 1, NotificationType.MinorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:05:02"));
            Event.CreateAndSave(content2path, 1, 1, NotificationType.MinorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:10:03"));
            Event.CreateAndSave(content1path, 1, 1, NotificationType.MinorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:15:04"));
            Event.CreateAndSave(content2path, 1, 1, NotificationType.MinorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:20:05"));
            Event.CreateAndSave(content1path, 1, 1, NotificationType.MinorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:25:06"));
            Event.CreateAndSave(content1path, 1, 1, NotificationType.MajorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:30:07"));
            Event.CreateAndSave(content1path, 1, 1, NotificationType.MinorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:35:08"));
            Event.CreateAndSave(content1path, 1, 1, NotificationType.MajorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:40:09"));
            Event.CreateAndSave(content2path, 1, 1, NotificationType.MajorVersionModified, "\\Administrator", DateTime.Parse("2011-03-22 08:45:10"));

            config.ImmediatelyEnabled = true;
            config.DailyEnabled = true;
            config.WeeklyEnabled = true;
            config.MonthlyEnabled = true;

            config.DailyHour = 1;
            config.DailyMinute = 0;
            config.WeeklyHour = 1;
            config.WeeklyMinute = 0;
            config.WeeklyWeekDay = DayOfWeek.Tuesday;
            config.MonthlyEvery = true;
            config.MonthlyHour = 1;
            config.MonthlyMinute = 0;
            config.MonthlyLast = false;
            config.MonthlyDay = 22;

            int count = 0;

            ////---- immediately
            //Message.DeleteAllMessages();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-22 08:05:59"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 2, String.Format("#1: count is {0}, expected {1}", count, 2));
            //Message.DeleteAllMessages();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-22 08:15:59"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 1, String.Format("#2: count is {0}, expected {1}", count, 1));
            //Message.DeleteAllMessages();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-22 08:25:59"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 1, String.Format("#3: count is {0}, expected {1}", count, 1));
            //Message.DeleteAllMessages();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-22 08:35:59"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 2, String.Format("#4: count is {0}, expected {1}", count, 2));
            //Message.DeleteAllMessages();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-22 08:45:59"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 2, String.Format("#5: count is {0}, expected {1}", count, 2));

            ////---- daily
            //Message.DeleteAllMessages(); LastProcessTime.Reset();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-23 00:59:59"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 2, String.Format("#6: count is {0}, expected {1}", count, 2));
            //Message.DeleteAllMessages(); LastProcessTime.Reset();
            //NotificationHandler.TimerTick(DateTime.Parse("2011-03-23 01:00:00"));
            //Assert.IsTrue((count = Message.GetAllMessages().Count()) == 4, String.Format("#7: count is {0}, expected {1}", count, 4));

            //---- weekly
            Message.DeleteAllMessages(); LastProcessTime.Reset();
            Debug.WriteLine("@#$Test> **** weekly 1");
            NotificationHandler.TimerTick(DateTime.Parse("2011-03-29 00:59:59"));
            Assert.IsTrue((count = Message.GetAllMessages().Count()) == 2, String.Format("#8: count is {0}, expected {1}", count, 2));
            Message.DeleteAllMessages(); LastProcessTime.Reset();
            Debug.WriteLine("@#$Test> **** weekly 2");
            NotificationHandler.TimerTick(DateTime.Parse("2011-03-29 01:00:00"));
            Assert.IsTrue((count = Message.GetAllMessages().Count()) == 6, String.Format("#9: count is {0}, expected {1}", count, 6));

            //---- monthly
            Message.DeleteAllMessages(); LastProcessTime.Reset();
            Debug.WriteLine("@#$Test> **** monthly 1");
            NotificationHandler.TimerTick(DateTime.Parse("2011-04-22 00:59:59"));
            Assert.IsTrue((count = Message.GetAllMessages().Count()) == 2, String.Format("#10: count is {0}, expected {1}", count, 2));
            Message.DeleteAllMessages(); LastProcessTime.Reset();
            Debug.WriteLine("@#$Test> **** monthly 2");
            NotificationHandler.TimerTick(DateTime.Parse("2011-04-22 01:00:00"));
            Assert.IsTrue((count = Message.GetAllMessages().Count()) == 6, String.Format("#11: count is {0}, expected {1}", count, 6));
        }

        //======================================================================== tools

        private static void CreateResourceFile()
        {
            //  /Root/Localization/MessageTemplateResources.xml
            var locFolder = Node.LoadNode("/Root/Localization");
            if (locFolder == null)
            {
                locFolder = new SystemFolder(Repository.Root);
                locFolder.Name = "Localization";
                locFolder.Save();
            }
            var resFile = new SenseNet.ContentRepository.i18n.Resource(locFolder);
            resFile.Name = "MessageTemplateResources.xml";
            var binData = new BinaryData();
            binData.SetStream(Tools.GetStreamFromString(_resFileSrc));
            resFile.SetBinary("Binary", binData);
            resFile.Save();
        }

        #region _resFileSrc
        private static readonly string _resFileSrc = @"<?xml version='1.0' encoding='utf-8'?>
<Resources>
  <ResourceClass name='MessageTemplate'>
    <Languages>
      <Language cultureName='en'>
        <data name='ImmediatelySubject' xml:space='preserve'>
				  <value>Immediately report about document changes</value>
				</data>
        <data name='DailySubject' xml:space='preserve'>
				  <value>Daily report about document changes</value>
				</data>
        <data name='WeeklySubject' xml:space='preserve'>
				  <value>Weekly report about document changes</value>
				</data>
        <data name='MonthlySubject' xml:space='preserve'>
				  <value>Monthly report about document changes</value>
				</data>
        <!-- -->
        <data name='ImmediatelyHeader' xml:space='preserve'>
          <value>Dear {Addressee},\r\n\r\nSince the last notice, the following changes were made:\r\n\r\n</value>
				</data>
        <data name='DailyHeader' xml:space='preserve'>
          <value>Dear {Addressee},\r\n\r\nSince the last notice, the following changes were made:\r\n\r\n</value>
				</data>
        <data name='WeeklyHeader' xml:space='preserve'>
          <value>Dear {Addressee},\r\n\r\nSince the last notice, the following changes were made:\r\n\r\n</value>
				</data>
        <data name='MonthlyHeader' xml:space='preserve'>
          <value>Dear {Addressee},\r\n\r\nSince the last notice, the following changes were made:\r\n\r\n</value>
				</data>
        <!-- -->
        <data name='ImmediatelyFooter' xml:space='preserve'>
          <value>\r\nRegards:\r\nSense/Net 6 System</value>
				</data>
        <data name='DailyFooter' xml:space='preserve'>
          <value>\r\nRegards:\r\nSense/Net 6 System</value>
				</data>
        <data name='WeeklyFooter' xml:space='preserve'>
          <value>\r\nRegards:\r\nSense/Net 6 System</value>
				</data>
        <data name='MonthlyFooter' xml:space='preserve'>
          <value>\r\nRegards:\r\nSense/Net 6 System</value>
				</data>
        <!-- -->
        <data name='DocumentCreated' xml:space='preserve'>
          <value>{Who} created the following content at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentMajorVersionModified' xml:space='preserve'>
          <value>{Who} modified a major version of the following content at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentMinorVersionModified' xml:space='preserve'>
          <value>{Who} modified a minor version of the following content at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentCopiedFrom' xml:space='preserve'>
          <value>{Who} copied the following content from another place at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentMovedFrom' xml:space='preserve'>
          <value>{Who} moved the following content from another place {When}: {ContentPath}\r\n</value>
				</data>
        <data name='DocumentMovedTo' xml:space='preserve'>
          <value>{Who} moved the following content to another place at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentRenamedFrom' xml:space='preserve'>
          <value>{Who} renamed the following content at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentRenamedTo' xml:space='preserve'>
          <value>{Who} renamed the following content to another name at {When}: {ContentPath}\r\n</value>
				</data>
        <data name='DocumentDeleted' xml:space='preserve'>
          <value>{Who} deleted the following content at {When}: {ContentUrl}\r\n</value>
				</data>
        <data name='DocumentRestored' xml:space='preserve'>
          <value>{Who} restored the following content at {When}: {ContentUrl}\r\n</value>
				</data>
      </Language>
    </Languages>
  </ResourceClass>
</Resources>";
        #endregion
    }
}
