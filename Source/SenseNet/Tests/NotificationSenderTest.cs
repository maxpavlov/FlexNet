using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Messaging;
using System.Linq;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public class NotificationSenderTest : TestBase
    {

        private TestContext testContextInstance;
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

        [TestInitialize]
        public void TestInit()
        {
            ClearMessages();
            NotificationSender.TestMode = true;
        }
        
        [TestMethod]
        public void TestMethod2()
        {
            NotificationSender.TestMode = true;
            InsertMessages(0, 50);
            NotificationSender.StartMessageProcessing();
            System.Threading.Thread.Sleep(5 * 1000);
            Assert.IsTrue(Message.GetAllMessages().Count() == 0);
        }

        private static void ClearMessages()
        {
            using (var context = new DataHandler())
            {
                context.Messages.DeleteAllOnSubmit(context.Messages);
                context.SubmitChanges();
            }
        }

        private static void InsertMessages(int from, int to)
        {
            using (var context = new DataHandler())
            {
                for (int i = from; i < to; i++)
                {
                    var message = new Message
                    {
                        Address = "bela.ilovszky@sensenet.com",
                        Body = i.ToString(),
                        Subject = i.ToString()
                    };

                    context.Messages.InsertOnSubmit(message);
                }

                context.SubmitChanges();
            }
        }
    }
}
