using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Configuration;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;
using System.Threading;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Communication.Messaging
{
    
    public abstract class ClusterChannel : IClusterChannel
    {
        /* ============================================================================== Members */
        private static List<ClusterMessage> _incomingMessages;
        private static volatile int _messagesCount;
        //private static AutoResetEvent _incomingMessageSignal;
        protected static bool _shutdown;
        private static object _messageListSwitchSync = new object();
        protected IClusterMessageFormatter m_formatter;
        protected ClusterMemberInfo m_clusterMemberInfo;
        public static List<Type> ProcessedMessageTypes;

        public bool AllowMessageProcessing { get; set; }

        /* ============================================================================== Properties */
        public ClusterMemberInfo ClusterMemberInfo
        {
            get { return m_clusterMemberInfo; }
        }
        private int _incomingMessageCount;
        public int IncomingMessageCount
        {
            get
            {
                return _incomingMessageCount;
            }
        }

        /* ============================================================================== Events */
        public event MessageReceivedEventHandler MessageReceived;
        public event ReceiveExceptionEventHandler ReceiveException;
        public event SendExceptionEventHandler SendException;

        /* ============================================================================== Init */
        public ClusterChannel(IClusterMessageFormatter formatter, ClusterMemberInfo clusterMemberInfo)
        {
            _incomingMessages = new List<ClusterMessage>();
            CounterManager.Reset("IncomingMessages");
            CounterManager.Reset("TotalMessagesToProcess");

            //_incomingMessageSignal = new AutoResetEvent(false);
            m_formatter = formatter;
            m_clusterMemberInfo = clusterMemberInfo;

            // initiate processing threads
            for (var i = 0; i < RepositoryConfiguration.MessageProcessorThreadCount; i++)
            {
                var thstart = new ParameterizedThreadStart(CheckProcessableMessages);
                var thread = new Thread(thstart);
                //thread.Priority = ThreadPriority.Highest;
                thread.Name = i.ToString();
                thread.Start();
            }
        }
        protected virtual void StartMessagePump()
        {

        }
        protected virtual void StopMessagePump()
        {
            _shutdown = true;
        }
        public virtual void Start()
        {
            StartMessagePump();
        }
        public virtual void ShutDown()
        {
            StopMessagePump();
        }

        /* ============================================================================== Send */
        public virtual void Send(ClusterMessage message)
        {
            try
            {
                message.SenderInfo = m_clusterMemberInfo;
                Stream messageStream = m_formatter.Serialize(message);
                InternalSend(messageStream);
            }
            catch (Exception e) //logged
            {
                Logger.WriteException(e);
                OnSendException(message, e);
            }
        }
        protected abstract void InternalSend(Stream messageBody);

        /* ============================================================================== Receive */
        private void CheckProcessableMessages(object parameter)
        {
            List<ClusterMessage> messagesToProcess;
            while (true)
            {
                try
                {
                    if (AllowMessageProcessing)
                    {
                        while ((messagesToProcess = GetProcessableMessages()) != null)
                        {
                            var count = messagesToProcess.Count;
                            Interlocked.Add(ref _messagesCount, count);

                            // process all messages in the queue
                            for (var i = 0; i < count; i++)
                            {
                                ProcessSingleMessage(messagesToProcess[i]);
                                messagesToProcess[i] = null;
                            }

                            Interlocked.Add(ref _messagesCount, -count);

                            if (_shutdown)
                                return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteException(ex);
                }

                // no messages to process, wait some time and continue checking incoming messages
                Thread.Sleep(100);
                //_incomingMessageSignal.WaitOne(1000);

                if (_shutdown)
                    return;
            }
        }
        private List<ClusterMessage> GetProcessableMessages()
        {
            List<ClusterMessage> messagesToProcess = null;
            lock (_messageListSwitchSync)
            {
                _incomingMessageCount = _incomingMessages.Count;

                if (_incomingMessageCount == 0)
                    return null;

                if (_incomingMessageCount <= RepositoryConfiguration.MessageProcessorThreadMaxMessages)
                {
                    // if total message count is smaller than the maximum allowed, process all of them and empty incoming queue
                    messagesToProcess = _incomingMessages;
                    _incomingMessages = new List<ClusterMessage>();
                }
                else
                {
                    // process the maximum allowed number of messages, leave the rest in the incoming queue
                    messagesToProcess = _incomingMessages.Take(RepositoryConfiguration.MessageProcessorThreadMaxMessages).ToList();
                    _incomingMessages = _incomingMessages.Skip(RepositoryConfiguration.MessageProcessorThreadMaxMessages).ToList();
                }
            }

            return messagesToProcess;
        }
        private void ProcessSingleMessage(object parameter)
        {
            var message = parameter as ClusterMessage;
            var msg = message as DistributedAction;
            if (msg != null)
            {
                if (ProcessedMessageTypes == null || ProcessedMessageTypes.Contains(msg.GetType()))
                {
                    msg.DoAction(true, msg.SenderInfo.IsMe);
                }
            }
            else if (message is PingMessage)
            {
                new PongMessage().Send();
            }
            if (MessageReceived != null)
                MessageReceived(this, new MessageReceivedEventArgs(message));
        }
        internal virtual void OnMessageReceived(Stream messageBody)
        {
            ClusterMessage message = m_formatter.Deserialize(messageBody);

            lock (_messageListSwitchSync)
            {
                _incomingMessages.Add(message);
                CounterManager.SetRawValue("IncomingMessages", Convert.ToInt64(_incomingMessages.Count));
                var totalMessages = _incomingMessages.Count + _messagesCount;
                CounterManager.SetRawValue("TotalMessagesToProcess", Convert.ToInt64(totalMessages));
                //_incomingMessageSignal.Set();
            }
        }

        /* ============================================================================== Purge */
        public virtual void Purge()
        {
            //throw new NotImplementedException();
        }

        /* ============================================================================== Error handling */
        protected virtual void OnSendException(ClusterMessage message, Exception exception)
        {
            if (SendException != null)
                SendException(this, new ExceptionEventArgs(exception, message));
        }
        protected virtual void OnReceiveException(Exception exception)
        {
            if (ReceiveException != null)
                ReceiveException(this, new ExceptionEventArgs(exception, null));
        }
    }
}