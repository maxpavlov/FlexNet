using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;

namespace ConcurrencyTester.Jobs
{
    [Serializable]
    public class NodeUndoCheckoutJob : NodeJob
    {
        public NodeUndoCheckoutJob(string name, string path, TextWriter output)
            : base(name, path, output)
        {

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

                    var checkValue = node["ExtensionData"].ToString();
                    node.Hidden = !node.Hidden;
                    node["ExtensionData"] = referenceValue;
                    node.Save();
                    node.UndoCheckOut();

                    //check the value
                    node = Node.Load<GenericContent>(_path);
                    
                    string actualValue = node["ExtensionData"].ToString();
                    string state;
                    
                    if (actualValue == checkValue)
                        state = "Done";
                    else if (actualValue == referenceValue)
                        state = "Failed: OldValue";
                    else
                        state = "Failed: Other";
                    
                    Console.WriteLine("{0}\t{1} ends\t{2} Modified: {3} ----------------------------", id, Name, DateTime.Now.Ticks, state);

                };
            }
        }

    }
}
