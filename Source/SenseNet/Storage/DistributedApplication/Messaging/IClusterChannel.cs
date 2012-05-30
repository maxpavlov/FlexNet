using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Timers;

namespace SenseNet.Communication.Messaging
{



    public interface IClusterChannel
    {
        ClusterMemberInfo ClusterMemberInfo { get; }
        void Send(ClusterMessage message);
        Receiver<T> GetReceiver<T>() where T : ClusterMessage;
        event MessageReceivedEventHandler MessageReceived;
        event ReceiveExceptionEventHandler ReceiveException;
        event SendExceptionEventHandler SendException;
        void Start();
        void ShutDown();
        void Purge();
    }


    public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs args);
    public delegate void ReceiveExceptionEventHandler(object sender, ExceptionEventArgs args);
    public delegate void SendExceptionEventHandler(object sender, ExceptionEventArgs args);

    public class MessageReceivedEventArgs : EventArgs
    {
        public ClusterMessage Message { get; set; }

        public MessageReceivedEventArgs() { }
        public MessageReceivedEventArgs(ClusterMessage message) { Message = message; }
    }

    public class ExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; set; }
        public ClusterMessage Message { get; set; }

        public ExceptionEventArgs() { }
        public ExceptionEventArgs(Exception exception, ClusterMessage message)
        {
            Exception = exception;
            Message = message;
        }
    }


    public interface IClusterMessageFormatter
    {
        ClusterMessage Deserialize(Stream data);
        Stream Serialize(ClusterMessage message);
    }



}