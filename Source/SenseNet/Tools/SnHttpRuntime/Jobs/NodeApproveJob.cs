using System;
using System.IO;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;

namespace ConcurrencyTester
{
    [Serializable]
    public class NodeApproveJob : NodeJob
    {
        private readonly bool _usePublish;

        public NodeApproveJob(string name, string path, TextWriter output, bool usePublish)
            : base(name, path, output)
        {
            _usePublish = usePublish;
        }

        public override Action<JobExecutionContext> Action
        {
            get
            {
                return context =>
                {

                    var id = Guid.NewGuid();
                    var referenceValue = id.ToString();
                    
                    Console.WriteLine("{0}\t{1} starts\t{2}", id, Name, DateTime.Now.Ticks);

                    var node = Node.Load<GenericContent>(_path);
                    
                    if (node.Locked)
                    {
                        if ((node.LockedById != User.Current.Id))
                        {
                            Console.WriteLine(
                                "{0}\tContent is locked by another user ({1})\t{2} ----------------------------",
                                id, Name, DateTime.Now.Ticks);
                            return;
                        }

                        Console.WriteLine(
                                "{0}\tContent were locked by this user ({1})\t{2} ----------------------------",
                                id, Name, DateTime.Now.Ticks);

                    }
                    else
                    {
                        node.CheckOut();
                    }
                        

                    node.Hidden = !node.Hidden;
                    node["ExtensionData"] = referenceValue;
                    node.Save();
                    node.CheckIn();
                    if(_usePublish)
                        node.Publish();
                    node.Approve();

                    //check the value
                    node = Node.Load<GenericContent>(_path);
                    var state = (node["ExtensionData"].ToString() == referenceValue) ? "Done" : "Failed";

                    Console.WriteLine("{0}\t{1} ends\t{2} Modified: {3} ----------------------------", id, Name, DateTime.Now.Ticks, state);
                 
                };
            }
        }
    }
}
