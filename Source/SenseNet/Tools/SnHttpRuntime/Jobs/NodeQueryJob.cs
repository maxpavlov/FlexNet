using System;
using System.Linq;
using SenseNet.ContentRepository.Storage.Search;
using System.IO;
using SenseNet.Utilities.ExecutionTesting;
using SenseNet.ContentRepository.Storage;

namespace ConcurrencyTester
{
    [Serializable]
    public class NodeQueryJob : NodeJob
    {
        private readonly string _nodeType;
        
        public NodeQueryJob(string name, string path, TextWriter output, string nodeType) : base(name, path, output)
        {
            _nodeType = nodeType;
        }

        public override Action<JobExecutionContext> Action
        {
            get
            {
                return context =>
                {
                    var id = Guid.NewGuid();
                    Console.WriteLine("{0}\t{1} starts\t{2}", id, Name, DateTime.Now.Ticks);

                    var query = new NodeQuery();

                    if (_path != null)
                    {
                        var expression1 = new StringExpression(StringAttribute.Path, StringOperator.StartsWith, _path);
                        query.Add(expression1);
                    }

                    var expression2 = new TypeExpression(ActiveSchema.NodeTypes[_nodeType], true);
                    query.Add(expression2);

                    var nodeList = query.Execute().Nodes.ToList();

                    Console.WriteLine("{0}\t{1} ends\t{2} Queried: {3} {4}", id, Name, DateTime.Now.Ticks,nodeList.Count,_nodeType);
                };
            }
        }
    }
}
