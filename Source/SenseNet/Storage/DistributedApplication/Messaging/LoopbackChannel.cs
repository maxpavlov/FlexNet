using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace SenseNet.Communication.Messaging
{
    public class LoopbackChannel : ClusterChannel
    {
        public LoopbackChannel(IClusterMessageFormatter formatter, 
            ClusterMemberInfo clusterMemberInfo) : base(formatter, clusterMemberInfo)
        {
        }
        protected override void InternalSend(System.IO.Stream messageBody)
        {
            this.OnMessageRecieved(messageBody);
        }
    }

    /// <summary>
    /// Provides a dummy "Null" cluster channel. The sent messages will be ignored.
    /// </summary>
    public class VoidChannel : ClusterChannel
    {
        public VoidChannel(IClusterMessageFormatter formatter, 
            ClusterMemberInfo clusterMemberInfo) : base(formatter, clusterMemberInfo)
        {
        }
        protected override void InternalSend(System.IO.Stream messageBody)
        {
            //do nothing
        }
    }

    /// <summary>
    /// Provides a dummy cluster channel. The sent messages will be written to the trace console.
    /// </summary>
    public class TraceChannel : ClusterChannel
    {
        public TraceChannel(IClusterMessageFormatter formatter, ClusterMemberInfo clusterMemberInfo)
            : base(formatter, clusterMemberInfo)
        {
        }

        protected override void InternalSend(System.IO.Stream messageBody)
        {
            string message;
            
            try
            {
                
                System.IO.StreamReader sr = new System.IO.StreamReader(messageBody);
                messageBody.Seek(0, System.IO.SeekOrigin.Begin);
                message = string.Format(CultureInfo.InvariantCulture, "ClusterId={0}, ClusterMemberId={1}, Message={2}", ClusterMemberInfo.ClusterID, ClusterMemberInfo.ClusterMemberID, sr.ReadToEnd());
            }
            catch (Exception ex) //TODO: catch block
            {
                message = "[an exception has been thrown: " + ex.Message + "]";
            }

            System.Diagnostics.Trace.WriteLine(string.Concat("ClusterMessage in TraceChannel: ", message));
        }
    }

}