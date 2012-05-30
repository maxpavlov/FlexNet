using System.Collections.Generic;

namespace SenseNet.Search
{
    public enum FilterStatus
    {
        Default,
        Enabled,
        Disabled
    }

    public class SortInfo
    {
        public string FieldName { get; set; }
        public bool Reverse { get; set; }
    }
    public class QuerySettings
    {
        public int Top { get; set; }
        public int Skip { get; set; }
        public IEnumerable<SortInfo> Sort { get; set; }

        public bool? EnableAutofilters { get; set; }
        public bool? EnableLifespanFilter { get; set; }
    }
}
