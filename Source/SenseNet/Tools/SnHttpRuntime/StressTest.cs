using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Utilities.ExecutionTesting;

namespace ConcurrencyTester
{
    public class StressTest
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public static int ActiveManagerCount = 0;

        private List<JobManager> _jobManagers;

        public StressTest(string name, string description, IEnumerable<JobManager> managers)
        {
            Name = name;
            Description = description;
            _jobManagers = new List<JobManager>(managers);
        }

        public int GetManagerCount()
        {
            return _jobManagers.Count;
        }

        public void Run()
        {
            foreach (var manager in _jobManagers)
            {
                manager.Start();
            }
        }
    }
}
