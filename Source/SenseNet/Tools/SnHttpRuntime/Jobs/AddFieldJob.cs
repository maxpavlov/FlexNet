using System;
using System.IO;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Fields;

namespace ConcurrencyTester
{
    [Serializable]
    public class AddFieldJob : NodeJob
    {
        private readonly string CLDefinition;

        public AddFieldJob(string name, string path, string contentListDefinition, TextWriter output)
            : base(name, path, output)
        {
            CLDefinition = contentListDefinition;
        }

        public override Action<JobExecutionContext> Action
        {
            get
            {
                return context =>
                {

                    var id = Guid.NewGuid();
                    
                    Console.WriteLine("{0} starts \n \t Id:{1} \t Ticks: {2}", Name, id, DateTime.Now.Ticks);

                    var contentList = Node.Load<ContentList>(_path);

                    if(contentList == null)
                        throw new Exception("ConcurrencyTesterError: There is no ContentList in the given path.");

                    contentList.ContentListDefinition = CLDefinition;
                    contentList.Save();

                    Console.WriteLine("{0} ended \n \t Id:{1} \t Ticks: {2}", Name, id, DateTime.Now.Ticks);

                };
            }
        }
    }
}

