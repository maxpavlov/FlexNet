using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;

namespace SenseNet.Portal.Workspaces
{
    //[ContentHandler]
    //public class Journal : GenericContent, IFolder
    //{
    //    //================================================================================= Required construction

    //    public Journal(Node parent) : this(parent, "Journal") { }
    //    public Journal(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
    //    protected Journal(NodeToken nt) : base(nt) { }

    //    //================================================================================= Mapped Properties

    //    [RepositoryProperty("TopItems", RepositoryDataType.Int)]
    //    public int TopItems
    //    {
    //        get { return this.GetProperty<int>("TopItems"); }
    //        set { this["TopItems"] = value; }
    //    }

    //    //================================================================================= Required generic property handling

    //    public override object GetProperty(string name)
    //    {
    //        switch (name)
    //        {
    //            case "TopItems":
    //                return this.TopItems;
    //            default:
    //                return base.GetProperty(name);
    //        }
    //    }
    //    public override void SetProperty(string name, object value)
    //    {
    //        switch (name)
    //        {
    //            case "TopItems":
    //                this.TopItems = (int)value;
    //                break;
    //            default:
    //                base.SetProperty(name, value);
    //                break;
    //        }
    //    }

    //    //================================================================================= IFolder Members

    //    private List<Node> _items;
    //    public IEnumerable<Node> Children
    //    {
    //        get
    //        {
    //            if (_items == null)
    //                LoadItems();
    //            return _items;
    //        }
    //    }
    //    public int ChildCount
    //    {
    //        get
    //        {
    //            if (_items == null)
    //                LoadItems();
    //            return _items.Count;
    //        }
    //    }
    //    private void LoadItems()
    //    {
    //        var top = this.TopItems < 1 ? 10 : this.TopItems;
    //        var items = Journals.Get(this.Path, top);
    //        var nodeList = new List<Node>();
    //        foreach (var item in items)
    //            nodeList.Add(new JournalNode(this, item));
    //        _items = nodeList;
    //    }

    //}

    [ContentHandler]
    public class JournalNode : Node
    {
        private JournalItem _journalItem;

        public JournalNode(Node parent, JournalItem journalItem) : base(parent, null) { _journalItem = journalItem; }
        protected JournalNode(NodeToken nt) : base(nt) { }

        public override bool IsContentType { get { return false; } }

        public string What { get { return _journalItem.What; } }
        public string Wherewith { get { return _journalItem.Wherewith; } }
        public string Who { get { return _journalItem.Who; } }
        public DateTime When { get { return _journalItem.When; } }
    }
}
