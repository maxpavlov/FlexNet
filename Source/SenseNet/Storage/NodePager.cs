//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using SenseNet.ContentRepository.Storage.Search;
//using SenseNet.Search;

//namespace SenseNet.ContentRepository.Storage
//{
//    public class NodePager<T> : IQueryPageNavigable, IQueryPageNavigator, 
//        IEnumerable<T>, IEnumerator<T> where T : Node
//    {
//        private static readonly int DefaultPageSize = 10000;
//        private static readonly int DefaultNodePageSize = 500;

//        private IContentQuery _query;

//        private List<int> _privateList;
//        private List<Node> _resolvedList;

//        private Dictionary<int, List<int>> _idPages;
//        private Dictionary<int, List<Node>> _nodePages;

//        private int _currentIndex = -1;
//        private int _pageSize = DefaultPageSize;

//        private bool _allIdsLoaded;

//        //================================================================== Constructors

//        public NodePager()
//        {
//            _privateList = new List<int>();
//        }

//        public NodePager(int capacity)
//        {
//            _privateList = new List<int>(capacity);
//        }

//        public NodePager(IEnumerable<int> idList)
//        {
//            _privateList = new List<int>(idList);
//        }

//        public NodePager(IEnumerable<int> idList, IContentQuery query)
//        {
//            if (query == null)
//                throw new ArgumentNullException("query");

//            this.Query = query;
//            this.Count = query.TotalCount;

//            var pageIndex = query.Settings.Skip/PageSize;

//            //in case of e.g. SKIP:2, TOP:10
//            if (query.Settings.Skip % PageSize > 0)
//                pageIndex++;

//            //insert initial id list
//            InsertIdsToPages(idList, pageIndex);

//            //set start indexes
//            CurrentPageIndex = pageIndex;
//            CurrentIndex = CurrentPageIndex*PageSize;
//        }

//        public NodePager(IEnumerable<T> collection)
//        {
//            if (collection == null)
//                throw new ArgumentNullException("collection");

//            CheckIds(collection);

//            _privateList = new List<int>(from node in collection select node.Id);
//        }

//        //================================================================== Internal properties

//        protected virtual List<int> Identifiers
//        {
//            get { return _privateList ?? (_privateList = new List<int>()); }
//            set { _privateList = value; }
//        }
//        protected List<Node> Nodes
//        {
//            get { return _resolvedList ?? (_resolvedList = new List<Node>()); }
//            set { _resolvedList = value; }
//        }

//        protected Dictionary<int, List<int>> IdentifierPages
//        {
//            get { return _idPages ?? (_idPages = new Dictionary<int, List<int>>()); }
//            set { _idPages = value; }
//        }
//        protected Dictionary<int, List<Node>> NodePages
//        {
//            get { return _nodePages ?? (_nodePages = new Dictionary<int, List<Node>>()); }
//            set { _nodePages = value; }
//        }

//        protected Node CurrentNode { get; private set; }
//        protected int CurrentIndex
//        {
//            get { return _currentIndex; } 
//            private set { _currentIndex = value; }
//        }
//        protected int CurrentPageIndex { get; private set; }
        
//        private bool AllIdsLoaded
//        {
//            get
//            {
//                return Query == null || _allIdsLoaded;
//            }
//            set { _allIdsLoaded = value; }
//        }

//        //================================================================== Public properties

//        private int _count;
//        public virtual int Count
//        {
//            get
//            {
//                return Query == null ? Identifiers.Count : _count;
//            }
//            private set
//            {
//                _count = value;
//            }
//        }

//        public int PageCount
//        {
//            get
//            {
//                var count = this.Count;
//                var mod = count % PageSize;
//                var pc = count / PageSize;

//                if (mod > 0)
//                    pc++;

//                return pc;
//            }
//        }

//        public int PageSize
//        {
//            get
//            {
//                if (_pageSize == 0)
//                    _pageSize = DefaultPageSize;

//                return _pageSize;
//            }
//            set
//            {
//                _pageSize = Math.Max(0, Math.Min(value, DefaultPageSize));

//                if (Query != null)
//                    Query.Settings.Top = _pageSize;
//            }
//        }

//        public IContentQuery Query
//        {
//            get { return _query; }
//            set
//            {
//                _query = value;

//                Reset(true, true);

//                if (_query == null)
//                    return;

//                PageSize = _query.Settings.Top;

//                if (PageSize == 0)
//                {
//                    PageSize = DefaultPageSize;
//                }
//            }
//        }

//        public virtual T this[int index]
//        {
//            get
//            {
//                if (Query == null)
//                {
//                    return Node.Load<T>(Identifiers[index]);
//                }
//                else
//                {
//                    var pageIndex = index/PageSize;
//                    var pageRelativeIndex =  index % PageSize;

//                    if (ValidIdPage(pageIndex) && ValidNodePage(pageIndex))
//                        return NodePages[pageIndex][pageRelativeIndex] as T;
                        
//                    throw new IndexOutOfRangeException();
//                }
//            }
//            set
//            {
//                throw new InvalidOperationException("Cannot set item of a NodePager");
//            }
//        }

//        //================================================================== Virtual methods

//        public virtual IEnumerator<T> GetEnumerator()
//        {
//            Reset(false, false);
//            return this;
//        }

//        //================================================================== Public helper methods

//        public IEnumerable<int> GetIdentifiers()
//        {
//            if (Query == null)
//                return Identifiers.ToArray();

//            LoadAllIds();

//            var allIds = new List<int>();

//            foreach (var page in IdentifierPages.Values)
//            {
//                allIds.AddRange(page);
//            }

//            return allIds;
//        }

//        public int IndexOf(T item)
//        {
//            CheckId(item);
//            return GetIdentifiers().ToList().IndexOf(item.Id);
//        }

//        public bool Contains(T item)
//        {
//            CheckId(item);
//            return GetIdentifiers().Contains(item.Id);
//        }

//        public bool Contains(int id)
//        {
//            return GetIdentifiers().Contains(id);
//        }

//        //================================================================== Interfaces: IQueryPageNavigable

//        int IQueryPageNavigable.Count
//        {
//            get
//            {
//                return Count;
//            }
//        }

//        int IQueryPageNavigable.PageCount
//        {
//            get
//            {
//                return PageCount;
//            }
//        }

//        IEnumerable<int> IQueryPageNavigable.Identifiers
//        {
//            get { return Identifiers; }
//        }

//        IEnumerable<Node> IQueryPageNavigable.Nodes
//        {
//            get { return this as IEnumerable<Node>; }
//        }

//        IQueryPageNavigator IQueryPageNavigable.CreatePageNavigator()
//        {
//            return CreatePageNavigator(true);
//        }

//        IQueryPageNavigator IQueryPageNavigable.CreatePageNavigator(bool reset)
//        {
//            return CreatePageNavigator(reset);
//        }

//        IContentQuery IQueryPageNavigable.Query
//        {
//            get { return Query; }
//        }

//        //=========================================== IQueryPageNavigator

//        bool IQueryPageNavigator.Started
//        {
//            get { return CurrentPageIndex >= 0; }
//        }

//        int IQueryPageNavigator.CurrentPageIndex
//        {
//            get { return this.CurrentPageIndex; }
//        }

//        bool IQueryPageNavigator.MoveToNextPage()
//        {
//            return MoveToPage(CurrentPageIndex + 1);
//        }

//        bool IQueryPageNavigator.MoveToPreviousPage()
//        {
//            return MoveToPage(CurrentPageIndex - 1);
//        }

//        bool IQueryPageNavigator.MoveToFirstPage()
//        {
//            return MoveToPage(0);
//        }

//        bool IQueryPageNavigator.MoveToLastPage()
//        {
//            return MoveToLastPage();
//        }

//        bool IQueryPageNavigator.MoveToPage(int pageIndex)
//        {
//            return MoveToPage(pageIndex);
//        }

//        IEnumerable<Node> IQueryPageNavigator.CurrentPage
//        {
//            get
//            {
//                return GetCurrentPage();
//            }
//        }

//        void IQueryPageNavigator.Reset()
//        {
//            Reset(false, false);
//        }

//        //=========================================== IEnumerator<T>

//        T IEnumerator<T>.Current
//        {
//            get { return CurrentNode as T; }
//        }

//        //=========================================== IEnumerator

//        object IEnumerator.Current
//        {
//            get { return CurrentNode; }
//        }

//        bool IEnumerator.MoveNext()
//        {
//            return MoveNext();
//        }

//        void IEnumerator.Reset()
//        {
//            Reset(false, false);
//        }

//        //=========================================== IEnumerable<T>

//        IEnumerator<T> IEnumerable<T>.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        //=========================================== IEnumerable

//        IEnumerator IEnumerable.GetEnumerator()
//        {
//            return GetEnumerator();
//        }

//        //=========================================== IDisposable

//        void IDisposable.Dispose()
//        {
//        }

//        //================================================================== Paging methods

//        private IQueryPageNavigator CreatePageNavigator(bool reset)
//        {
//            if (reset)
//                Reset(false, false);

//            return this;
//        }

//        private IEnumerable<Node> GetCurrentPage()
//        {
//            if (CurrentPageIndex < 0)
//                throw new InvalidOperationException("It is not possible to get the current page before calling MoveToNextPage");

//            if (Query == null)
//            {
//                var pageStartIndex = CurrentPageIndex * PageSize;

//                //return nodes from the given window
//                if (this.Nodes.Count < pageStartIndex)
//                    throw new InvalidOperationException("Missing nodes in current page");

//                return this.Nodes.Skip(pageStartIndex).Take(PageSize);
//            }

//            if (!ValidNodePage(CurrentPageIndex))
//                return new List<Node>();

//            return NodePages[CurrentPageIndex];
//        }

//        private bool MoveNext()
//        {
//            var nextPageIndex = (CurrentIndex + 1) / PageSize;

//            if (!ValidIdPage(nextPageIndex))
//                return false;

//            if (!ValidNodePage(nextPageIndex))
//                return false;

//            if (this.Query != null)
//            {
//                var pageRelativeIndex = (CurrentIndex + 1) % PageSize;

//                //if we are on the last page and tried to move next
//                if (pageRelativeIndex >= NodePages[nextPageIndex].Count)
//                    return false;

//                CurrentIndex = CurrentIndex + 1;

//                //keep the other current values up-to-date
//                CurrentPageIndex = nextPageIndex;
//                LoadCurrentNode();

//                return true;
//            }
//            else
//            {
//                //we don't have a query, so use the 
//                //id list as the whole result set
//                //var allIds = Identifiers;

//                if (CurrentIndex < Identifiers.Count - 1)
//                {
//                    CurrentIndex = CurrentIndex + 1;

//                    //keep the other current values up-to-date
//                    CurrentPageIndex = CurrentIndex / PageSize;
//                    LoadCurrentNode();

//                    return true;
//                }
//            }

//            return false;
//        }

//        private bool MoveToPage(int pageIndex)
//        {
//            if (pageIndex < 0)
//                return false;

//            if (!ValidIdPage(pageIndex))
//                return false;

//            if (!ValidNodePage(pageIndex))
//                return false;

//            CurrentPageIndex = pageIndex;
//            CurrentIndex = CurrentPageIndex * PageSize;

//            LoadCurrentNode();

//            return true;
//        }

//        private bool MoveToLastPage()
//        {
//            return MoveToPage(this.PageCount - 1);
//        }

//        private bool LoadIdsForPage(int pageIndex)
//        {
//            if (this.Query == null)
//                return true;

//            //use the query to get the next few ids
//            this.Query.Settings.Skip = pageIndex * PageSize;
            
//            //we set the big default here to load 
//            //as many ids as we can in one round
//            this.Query.Settings.Top = DefaultPageSize;

//            var ids = this.Query.ExecuteToIds(ExecutionHint.None).ToList();

//            //if the query returned 0 nodes
//            if (ids.Count == 0)
//                return false;

//            //we need to round the results to whole pages,
//            //but only if we found more items here than PageSize
//            var usableCount = ids.Count;
//            if (usableCount > PageSize)
//                usableCount = usableCount - (usableCount % PageSize);

//            InsertIdsToPages(ids.Take(usableCount), pageIndex);

//            return true;
//        }

//        private bool LoadNodesForPage(int pageIndex)
//        {
//            if (Query == null)
//            {
//                var pageFirstIndex = pageIndex*PageSize;
//                var emptyCount = pageFirstIndex - this.Nodes.Count;
//                var idsToLoad = Identifiers.Skip(pageFirstIndex).Take(PageSize).ToList();
//                List<Node> nodes;

//                if (emptyCount < 0)
//                {
//                    //if nodes are already loaded
//                    if (this.Nodes[pageFirstIndex] != null)
//                        return true;

//                    //gaps are empty, so load the nodes   
//                    nodes = LoadNodes(idsToLoad);

//                    if (nodes.Count < idsToLoad.Count)
//                    {
//                        //missing nodes: remove ids and reload nodes
//                        var wrongIds = idsToLoad.Where(idToLoad =>
//                            nodes.Count(n => n.Id == idToLoad) == 0).ToList();

//                        RemoveIds(wrongIds, pageIndex);

//                        return LoadNodesForPage(pageIndex);
//                    }

//                    //normal workflow: fill gaps
//                    for (var i = 0; i < nodes.Count; i++)
//                    {
//                        this.Nodes[i + pageFirstIndex] = nodes[i];
//                    }

//                    return true;
//                }

//                if (emptyCount > 0)
//                {
//                    //load nodes somewhere in the middle,
//                    //fill gaps with null nodes, like:
//                    //xxxx.................[xxxx]
//                    this.Nodes.AddRange(Enumerable.Repeat(null as Node, emptyCount));
//                }

//                nodes = LoadNodes(idsToLoad);

//                if (nodes.Count < idsToLoad.Count)
//                {
//                    //missing nodes: remove ids and reload nodes
//                    var wrongIds = idsToLoad.Where(idToLoad =>
//                        nodes.Count(n => n.Id == idToLoad) == 0).ToList();

//                    RemoveIds(wrongIds, pageIndex);

//                    return LoadNodesForPage(pageIndex);
//                }

//                //add new nodes at the end of the collection
//                this.Nodes.AddRange(nodes);
//            }
//            else
//            {
//                if (!IdentifierPages.ContainsKey(pageIndex))
//                    return false;

//                var idsToLoad = IdentifierPages[pageIndex];

//                //empty result set
//                if (idsToLoad.Count == 0)
//                {
//                    NodePages.Add(pageIndex, new List<Node>());
//                    return true;
//                }

//                var nodes = LoadNodes(idsToLoad);
//                if (nodes.Count == 0)
//                    return false;

//                if (nodes.Count < IdentifierPages[pageIndex].Count)
//                {
//                    DropPages(pageIndex);

//                    //reload ids and nodes for current page
//                    return LoadIdsForPage(pageIndex) && LoadNodesForPage(pageIndex);
//                }

//                NodePages.Add(pageIndex, nodes);
//            }

//            return true;
//        }

//        private void LoadAllIds()
//        {
//            if (AllIdsLoaded)
//                return;

//            var idPageIndex = 0;

//            do
//            {
//                if (!ValidIdPage(idPageIndex))
//                    break;

//                //we just loaded the last page
//                if (IdentifierPages[idPageIndex].Count < PageSize)
//                    break;

//                idPageIndex++;

//            } while (true);

//            AllIdsLoaded = true;
//        }

//        private void LoadCurrentNode()
//        {
//            if (Query != null)
//            {
//                var pageRelativeIndex = CurrentIndex % PageSize;

//                CurrentNode = NodePages[CurrentPageIndex][pageRelativeIndex];
//            }
//            else
//            {
//                CurrentNode = Nodes[CurrentIndex];
//            }
//        }

//        private bool ValidIdPage(int pageIndex)
//        {
//            if (Query == null)
//            {
//                return pageIndex * PageSize < this.Count;
//            }

//            return IdentifierPages.ContainsKey(pageIndex) || LoadIdsForPage(pageIndex);
//        }

//        private bool ValidNodePage(int pageIndex)
//        {
//            if (Query == null)
//                return LoadNodesForPage(pageIndex);

//            return NodePages.ContainsKey(pageIndex) || LoadNodesForPage(pageIndex);
//        }

//        protected void Reset(bool resetNodes, bool resetIds)
//        {
//            CurrentIndex = -1;
//            CurrentPageIndex = -1;
//            CurrentNode = null;

//            if (resetNodes)
//            {
//                Nodes = null;
//                NodePages = null;
//            }

//            if (resetIds)
//            {
//                Identifiers = null;
//                IdentifierPages = null;
//            }
//        }

//        //================================================================== Helper methods

//        private List<Node> LoadNodes(ICollection<int> idsToLoad)
//        {
//            var remainingCount = idsToLoad.Count;
//            var realPageSize = Math.Min(PageSize, DefaultNodePageSize);
//            var nodes = new List<Node>();

//            while (remainingCount > 0)
//            {
//                nodes.AddRange(Node.LoadNodes(idsToLoad.Skip(nodes.Count).Take(realPageSize)));
//                remainingCount -= realPageSize;
//            }

//            return nodes;
//        }

//        private void InsertIdsToPages(IEnumerable<int> identifiers, int startPageIndex)
//        {
//            if (identifiers == null || startPageIndex < 0)
//                throw new InvalidOperationException("Missing identifiers or wrong page index");

//            var tempPageIndex = 0;
//            while (true)
//            {
//                var currentPageIndex = startPageIndex + tempPageIndex;
//                var pageIds = identifiers.Skip(tempPageIndex * PageSize).Take(PageSize).ToList();

//                //stop if no ids were found - except if this is the first page,
//                //because we need to store the empty result
//                if (pageIds.Count == 0 && currentPageIndex > 0)
//                    break;

//                //if we have this page already, refresh it
//                if (IdentifierPages.ContainsKey(currentPageIndex))
//                {
//                    IdentifierPages.Remove(currentPageIndex);
//                }

//                IdentifierPages.Add(currentPageIndex, pageIds);

//                //ok, now we can get out if no more ids to store
//                if (pageIds.Count < PageSize)
//                    break;

//                tempPageIndex++;
//            }
//        }

//        private void DropPages(int startPageIndex)
//        {
//            if (Query == null)
//            {
//                throw new InvalidOperationException("No need to drop flat values in case of a missing node, just shift.");
//            }
//            else
//            {
//                var keysToRemove = IdentifierPages.Keys.Where(k => k >= startPageIndex).ToList();
//                foreach (var key in keysToRemove)
//                {
//                    IdentifierPages.Remove(key);
//                }

//                keysToRemove = NodePages.Keys.Where(k => k >= startPageIndex).ToList();
//                foreach (var key in keysToRemove)
//                {
//                    NodePages.Remove(key);
//                }
//            }
//        }

//        private void RemoveIds(IEnumerable<int> wrongIds, int pageIndex)
//        {
//            //remove wrong ids
//            foreach (var wrongId in wrongIds)
//            {
//                Identifiers.Remove(wrongId);
//            }

//            //remove every loaded node following this, and reload them
//            this.Nodes = this.Nodes.Take(pageIndex * PageSize).ToList();
//        }

//        protected static void CheckId(Node node)
//        {
//            if (node == null)
//                throw new NotSupportedException("Item cannot be null");
//            if (node.Id == 0)
//                throw new NotSupportedException("Node must be saved");
//        }

//        protected static void CheckIds(IEnumerable<T> nodes)
//        {
//            foreach (var node in nodes)
//                CheckId(node);
//        }
//    }
//}
