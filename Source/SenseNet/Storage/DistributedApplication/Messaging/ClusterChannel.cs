using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Configuration;
using SenseNet.Diagnostics;

namespace SenseNet.Communication.Messaging
{
    
    public abstract class ClusterChannel : IClusterChannel
    {
        private IClusterMessageFormatter m_formatter;
        private ClusterMemberInfo m_clusterMemberInfo;

        public delegate void Msg<T>(T t);

        public event MessageReceivedEventHandler MessageReceived;
        public event ReceiveExceptionEventHandler ReceiveException;
        public event SendExceptionEventHandler SendException;

        public static List<Type> ProcessedMessageTypes;

        public Dictionary<Type, MulticastDelegate> delegs = new Dictionary<Type, MulticastDelegate>();
        public void DoIt(ClusterMessage msg)
        {
            delegs[msg.GetType()].DynamicInvoke(msg);
        }
        public Receiver<T> GetReceiver<T>() where T: ClusterMessage
        {
           
            return new Receiver<T>();
        }

        public ClusterChannel(IClusterMessageFormatter formatter, ClusterMemberInfo clusterMemberInfo)
        {
            m_formatter = formatter;
            m_clusterMemberInfo = clusterMemberInfo;
        }

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

        protected virtual void OnMessageRecieved(Stream messageBody)
        {
            ClusterMessage message = m_formatter.Deserialize(messageBody);
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
        
        public ClusterMemberInfo ClusterMemberInfo
        {
            get { return m_clusterMemberInfo; }
        }

        protected virtual void StartMessagePump()
        {
            //good old noop
        }
        protected virtual void StopMessagePump()
        {
            //good old noop
        }

        public virtual void Start()
        {
            StartMessagePump();
        }
        public virtual void ShutDown()
        {
            StopMessagePump();
        }
        public virtual void Purge()
        {
            //throw new NotImplementedException();
        }

    }

    public delegate void MessageReceived<T>(T message);

    public class Receiver<T> where T : ClusterMessage
    {
        public void MsgRece(T msg)
        {
        }
        public event MessageReceived<T> Received
        {
            add
            {
                //((ClusterChannel)DistributedApplication.ClusterChannel).delegs[typeof(T)]
                //    = MulticastDelegate.CreateDelegate(
                //    this.GetType(), this.GetType().GetMethod("MsgRece"));
                ////Receiver<T>.Received(
                //((ClusterChannel)DistributedApplication.ClusterChannel).delegs.Add(
                //    typeof(T), value);
            }

            remove
            {

            }
        }
    }

}