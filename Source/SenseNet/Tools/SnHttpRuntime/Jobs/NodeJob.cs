using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Utilities.ExecutionTesting;
using System.IO;

namespace ConcurrencyTester
{
    [Serializable]
    public class NodeJob : Job
    {
        protected string _path;
        public string Path
        {
            get
            {
                return _path;     
            }
        }

        public NodeJob(string name, string path, TextWriter output)
            : base(name, output)
        {
            _path = path;
        }
    }
}
