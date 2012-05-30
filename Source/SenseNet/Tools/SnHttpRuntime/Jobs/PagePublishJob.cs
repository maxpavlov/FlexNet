using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.Utilities.ExecutionTesting;
using System.IO;
using SenseNet.Portal;
using SenseNet.ContentRepository;

namespace ConcurrencyTester
{
    [Serializable]
    public class PagePublishJob : Job
    {
        private string _path;

        public PagePublishJob(string name, string pagePath, TextWriter output)
            : base(name, output)
        {
            _path = pagePath;
        }

        public override Action<JobExecutionContext> Action
        {
            get
            {
                return context =>
                {

                    var id = Guid.NewGuid();
                    Console.WriteLine("{0}\t{1} starts\t{2}", id, Name, DateTime.Now.Ticks);

                    var page = Page.Load<Page>(_path);

                    if (!page.Locked)
                    {
                        page.CheckOut();
                        page.Hidden = !page.Hidden;
                        page["ExtensionData"] = Guid.NewGuid().ToString();
                        page.Publish();

                        Console.WriteLine("{0}\t{1} ends\t{2}", id, Name, DateTime.Now.Ticks);
                    }
                    else
                    {
                        if (page.LockedById == User.Current.Id)
                        {
                            page.Hidden = !page.Hidden;
                            page["ExtensionData"] = Guid.NewGuid().ToString();
                            page.Publish();

                            Console.WriteLine("{0}\t{1} ends\t{2}", id, Name, DateTime.Now.Ticks);
                        }
                        else
                        {
                            Console.WriteLine("{0}\tPage is locked ({1})\t{2}", id, Name, DateTime.Now.Ticks);
                        }

                    }


                };
            }
        }
    }
   
}
