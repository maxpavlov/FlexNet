using System;
using System.Collections.Generic;
using System.Text;
using System.EnterpriseServices;
using Msmq = System.Messaging;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Messaging;
using SenseNet.Communication.Messaging;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Communication.Messaging
{
    /// <summary>
    /// Provides MSMQ based cluster messaging channel.
    /// </summary>
    /// 
    public class MsmqChannelProvider : ClusterChannel
    {
        private Msmq.MessageQueue m_messageQueue;
        private Msmq.BinaryMessageFormatter m_formatter = new System.Messaging.BinaryMessageFormatter();
        
        public MsmqChannelProvider(IClusterMessageFormatter formatter,
            ClusterMemberInfo memberInfo) : base(formatter, memberInfo)
        {
        }

        private Msmq.Message CreateMessage(Stream messageBody)
        {
            Msmq.Message m = new System.Messaging.Message(messageBody);
            m.TimeToBeReceived = TimeSpan.FromSeconds(RepositoryConfiguration.MessageRetentionTime);
            return m;
        }

        protected override void InternalSend(System.IO.Stream messageBody)
        {
            Msmq.Message message = CreateMessage(messageBody);
            message.Formatter = m_formatter;
            m_messageQueue.Send(message);
        }

        protected override void StartMessagePump()
        {
            m_messageQueue = PrepareMessageQueue();
            BeginPeekMessages();

            if (_firstStart)
                _firstStart = false;
        }

        private Msmq.MessageQueue PrepareMessageQueue()
        {
            //Msmq.MessageQueue q = Msmq.MessageQueue.Exists(qname) ?
            //    new Msmq.MessageQueue(qname) : q = Msmq.MessageQueue.Create(qname);
            Msmq.MessageQueue q = new Msmq.MessageQueue(RepositoryConfiguration.MessageQueueName);
            q.Formatter = new Msmq.BinaryMessageFormatter();
            return q;
        }

        protected override void StopMessagePump()
        {

        }


        Msmq.Cursor cursor = null;
        Msmq.PeekAction action = PeekAction.Current;
        IAsyncResult m_asyncResult;
        private bool _firstStart = true;



        internal void BeginPeekMessages()
        {
            try
            {
                if (cursor == null)
                {
                    //we have to check if the queue is available before we open the cursor
                    //http://connect.microsoft.com/VisualStudio/feedback/details/264116/system-messaging-cursor-finalize-throws-nullreferenceexception-when-gc-starts-collecting-the-garbage
                    var readHandle = m_messageQueue.ReadHandle;
                    cursor = m_messageQueue.CreateCursor();
                }
                
                m_asyncResult = m_messageQueue.BeginPeek(Msmq.MessageQueue.InfiniteTimeout, cursor, action, null, PeekReceived);
            }
            catch (MessageQueueException mex) //logged
            {
                Logger.WriteCritical(String.Format("Message queue is unavailable. Queue name: {0}",RepositoryConfiguration.MessageQueueName));
                if (_firstStart)
                    throw;

                Logger.WriteException(mex);
                HandleReceiveException(mex);
                
                RecoverConnection();
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                HandleReceiveException(e);
            }
        }

        internal void HandleReceiveException(Exception e)
        {
            OnReceiveException(e);
        }


        void PeekReceived(IAsyncResult r)
        {
            try
            {
                Msmq.Message message = m_messageQueue.EndPeek(r);
                action = PeekAction.Next;
                OnMessageRecieved(message.Body as Stream);
                
                BeginPeekMessages();
            }
            catch (MessageQueueException mex) //logged
            {
                Logger.WriteCritical(String.Format("Message queue is unavailable. Queue name: {0}", RepositoryConfiguration.MessageQueueName));
                Logger.WriteException(mex);
                HandleReceiveException(mex);
                
                RecoverConnection();
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                HandleReceiveException(e);
                
                BeginPeekMessages();
            }
        }

        private void RecoverConnection()
        {
            //the queue must be closed and the connection cache cleared before we try to reconnect
            m_messageQueue.Close();
            MessageQueue.ClearConnectionCache();
            
            //restore initial values for: cursor, action
            cursor = null;
            action = PeekAction.Current;
            
            Thread.Sleep(RepositoryConfiguration.MsmqReconnectDelay);

            //reconnect
            StartMessagePump();
        }

        public override void Purge()
        {
            var count = 0;
            var iterator = m_messageQueue.GetMessageEnumerator2();
            while (iterator.MoveNext())
                count++;

            Logger.WriteInformation("MsmqChannel Purge", Logger.EmptyCategoryList, new Dictionary<string,object>
            {
                {"MachineName", m_messageQueue.MachineName},
                {"DeletedMessages", count}
            });

            m_messageQueue.Purge();
        }
    }
}