using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SenseNet.Communication.Messaging
{
    /// <summary>
    /// Allows an activity to be executed first in the local AppDomain
    /// then via cluster messaging on the cluster members.
    /// </summary>
    [Serializable]
    public abstract class DistributedAction : ClusterMessage
    {
        public void Execute()
        {
            try
            {
                DoAction(false, true);
                Logger.WriteVerbose("Execute DistributedAction: ", Logger.GetDefaultProperties, this);
            }
            finally
            {
                try
                {
                    Distribute();
                }
                catch (Exception exc2) //logged
                {
                    Logger.WriteException(exc2);
                }
            }
            
        }

        public abstract void DoAction(bool onRemote, bool isFromMe);

        public virtual void Distribute()
        {
            try
            {
                DistributedApplication.ClusterChannel.Send(this);
            }
            catch (Exception exc) //logged
            {
                Logger.WriteException(exc);
            }
        }
    }

    [Serializable]
    public class DebugMessage : ClusterMessage
    {
        public string Message { get; set; }
        public override string ToString()
        {
            return "DebugMessage: " + Message;
        }

        public static void Send(string message)
        {
            new DebugMessage() { Message = message }.Send();
        }
    }

    [Serializable]
    public sealed class PingMessage : DebugMessage
    {
        public PingMessage()
        {
            Message = "PING";
        }
    }

    [Serializable]
    public sealed class PongMessage : DebugMessage
    {
        public PongMessage()
        {
            Message = "PONG";
        }
    }

    
}