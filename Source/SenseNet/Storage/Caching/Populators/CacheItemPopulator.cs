using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace SenseNet.ContentRepository.Storage.Caching.Populators
{
    public abstract class CacheItemPopulator
    {
        public abstract object CreateItem(object initParam);
        public abstract string CreateKey(object initParam);
        public abstract string CreateKeyFromItem(object item);
        public abstract CacheDependency CreateDependencies(object item, object initParam);
        public abstract Type GetItemType();
    }

}
