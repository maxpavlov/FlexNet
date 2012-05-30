using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.Portal.UI;

namespace SenseNet.Search.Indexing
{
    public class WikiReferencedTitlesIndexHandler : LongTextIndexHandler
    {
        public override IEnumerable<IndexFieldInfo> GetIndexFieldInfos(ContentRepository.Field snField, out string textExtract)
        {
            var data = snField.GetData() as string ?? string.Empty;
            var titles = data.Split(new[] {WikiTools.REFERENCEDTITLESFIELDSEPARATOR}, 100000, StringSplitOptions.RemoveEmptyEntries);

            textExtract = string.Empty;

            return CreateFieldInfo(snField.Name, titles);
        }
    }
}
