using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.Utilities.ExecutionTesting;
using System.IO;

namespace ConcurrencyTester
{
    [Serializable]
    public class NodeSaveJob : NodeJob
    {
        public NodeSaveJob(string name, string path, TextWriter output) : base(name, path, output)
        {
            
        }

        public override Action<JobExecutionContext> Action
        {
            get
            {
                return context =>
                {
                    var id = Guid.NewGuid();
                    Console.WriteLine("{0}\t{1} starts\t{2}", id, Name, DateTime.Now.Ticks);
                    
                    var node = Node.LoadNode(_path);
                    
                    var index = (int)node["Index"];

                    int referenceValue = index + 1;

                    node["Index"] = referenceValue;
                    
                    node.Save();

                    node = Node.LoadNode(_path);
                    string state = (referenceValue == node.Index) ? "Done" : "Failed";
                    
                    Console.WriteLine("{0}\t{1} ends\t{2} --- Modified: {3}", id, Name, DateTime.Now.Ticks, state);
                };
            }
        }
    }
}
