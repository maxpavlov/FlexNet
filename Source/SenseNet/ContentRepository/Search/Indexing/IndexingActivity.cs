using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Lucene.Net.Documents;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Search.Indexing
{
    public enum IndexingActivityType
    {
        AddDocument = 1,
        AddTree = 2,
        UpdateDocument = 3,
        RemoveDocument = 4,
        RemoveTree = 5
    }

    public partial class IndexingActivity : INotifyPropertyChanging, INotifyPropertyChanged
    {

    }
    
}
