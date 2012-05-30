using System;
using System.Collections.Generic;
using System.Linq;
using SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SenseNet.Portal.HcExplore
{
    public class HcExploreMenuItem : System.Web.Mvc.ViewPage<IDictionary<string,string>>
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="contentPath">The content path.</param>
        /// <returns></returns>
        public IEnumerable<Content> GetChildren(string contentPath)
        {
            try
            {
                var cnt = Content.Load(contentPath);
                var lucQuery = SenseNet.Search.LucQuery.Parse("+TypeIs:folder +InFolder:" + cnt.Path);
                lucQuery.EnableAutofilters = false;
                lucQuery.EnableLifespanFilter = false;
                var lucObjectList = lucQuery.Execute();
                var list = (from lucObject in lucObjectList
                            select Content.Load(lucObject.NodeId)).ToList();

                return list;
            }
            catch (Exception ex)
            {
                Logger.WriteException(ex);
                return new List<Content>();
            }
        }
    }
}