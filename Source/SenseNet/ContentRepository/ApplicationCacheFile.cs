using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using System.Diagnostics;
using SenseNet.ContentRepository.Storage.Events;

namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class ApplicationCacheFile : File
    {
        public ApplicationCacheFile(Node parent) : this(parent, null) { }
        public ApplicationCacheFile(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected ApplicationCacheFile(NodeToken nt) : base(nt) { }

        //========================================================== Cached data

        private IEnumerable<string> cachedData;
        public IEnumerable<string> CachedData { get { return cachedData; } }

        private const string CACHEDAPPLICATIONDATAKEY = "CachedApplicationData";

        protected override void OnLoaded(object sender, NodeEventArgs e)
        {
            base.OnLoaded(sender, e);

            cachedData = (IEnumerable<string>)base.GetCachedData(CACHEDAPPLICATIONDATAKEY);
            if (cachedData != null)
                return;

            var stringData = Tools.GetStreamString(this.Binary.GetStream());
            cachedData = stringData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            base.SetCachedData(CACHEDAPPLICATIONDATAKEY, cachedData);
        }
    }
}
