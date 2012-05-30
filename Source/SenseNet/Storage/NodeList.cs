using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using System.Diagnostics;

namespace SenseNet.ContentRepository.Storage
{
    internal abstract class BufferedEnumerator<T> : IEnumerator<T>
    {
        protected int[] IdSet { get; private set; }
        T[] buffer;
        int pointer;
        int nextOffset;
        int offset;

        public BufferedEnumerator(IEnumerable<int> idSet)
        {
            this.IdSet = idSet.ToArray();
            offset = 0;
            pointer = -1;
        }

        //================================================== Interface implementations

        public void Reset()
        {
            if (offset > 0)
                buffer = null;
            offset = 0;
        }
        public void Dispose()
        {
            IdSet = null;
            buffer = null;
        }
        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }
        public T Current
        {
            get
            {
                if (pointer == -1)
                    throw new InvalidOperationException("Enumeration has not started. Call MoveNext.");
                if (pointer >= IdSet.Length)
                    throw new InvalidOperationException("Enumeration already finished");
                return buffer[pointer];
            }
        }
        public bool MoveNext()
        {
            if (++pointer == IdSet.Length)
                return false;
            if (buffer != null)
                if (pointer < buffer.Length)
                    return true;
            while (true)
            {
                if (buffer != null)
                {
                    offset = nextOffset;
                    pointer = 0;
                }
                if (offset > IdSet.Length)
                    return false;
                buffer = ReadPage(offset, out nextOffset);
                if (buffer.Length > 0)
                    return true;
            }
        }

        protected abstract T[] ReadPage(int offset, out int nextOffset);
    }
    internal class BufferedNodeHeadEnumerator : BufferedEnumerator<NodeHead>
    {
        protected static int BufferSize = 1000;
        public BufferedNodeHeadEnumerator(IEnumerable<int> idSet) : base(idSet) { }
        protected override NodeHead[] ReadPage(int offset, out int nextOffset)
        {
            //var p = offset;
            nextOffset = offset + BufferSize;
            var count = IdSet.Length - offset;
            if (count < 1)
                return new NodeHead[0];
            if (count > BufferSize)
                count = BufferSize;
            var idPage = new int[count];
            Array.Copy(IdSet, offset, idPage, 0, count);
            var nodeHeads = NodeHead.Get(idPage).ToArray();
            //Trace.WriteLine("@@@##$$> ReadPage from: " + offset + ": " + String.Join(",", nodeHeads.Select(h => h.Id.ToString()).ToArray()));
            return nodeHeads;
        }
    }
    internal class BufferedNodeEnumerator<T> : BufferedEnumerator<T> where T : Node
    {
        protected static int BufferSize = 100;
        public BufferedNodeEnumerator(IEnumerable<int> idSet) : base(idSet) { }
        protected override T[] ReadPage(int offset, out int nextOffset)
        {
            //var p = offset;
            nextOffset = offset + BufferSize;
            var count = IdSet.Length - offset;
            if (count < 1)
                return new T[0];
            if (count > BufferSize)
                count = BufferSize;
            var idPage = new int[count];
            Array.Copy(IdSet, offset, idPage, 0, count);
            T[] nodes = Node.LoadNodes(idPage).Cast<T>().ToArray();
            //Trace.WriteLine("@@@##$$> ReadPage#" + offset + ": " + String.Join(",", nodes.Select(h => h.Id.ToString()).ToArray()));
            return nodes;
        }
    }

    internal class NodeHeadResolver : IEnumerable<NodeHead>
    {
        IEnumerable<int> idSet;
        public NodeHeadResolver(IEnumerable<int> idSet)
        {
            this.idSet = idSet;
        }
        public IEnumerator<NodeHead> GetEnumerator()
        {
            return new BufferedNodeHeadEnumerator(idSet);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    internal class NodeSet<T> : IEnumerable<T> where T : Node
    {
        IEnumerable<int> idSet;
        public NodeSet(IEnumerable<int> idSet)
        {
            this.idSet = idSet;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return new BufferedNodeEnumerator<T>(idSet);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    internal interface INodeResolver<T> : IEnumerable<T> where T : Node
    {
        int IdCount { get; }
        int GetPermittedCount();
        IEnumerable<int> GetIdentifiers();
        IEnumerable<T> GetPage(int skip, int top);
    }

    /// <summary>
    /// DO NOT USE THIS CLASS IN YOUR CODE.
    /// Represents a strongly typed list of Repository Node objects that can be accessed by index.
    /// Sort, and random access is not supported.
    /// </summary>
    /// <typeparam name="T"><see cref="T"/> can be the Node or a type that is derived from the Node.</typeparam>
    public class NodeList<T> : INodeResolver<T>, IEnumerable<T>, /*IList<T>, System.Collections.IList,*/ IDynamicDataAccessor where T : Node
    {
        private List<int> __privateList;
        //private List<Node> _resolvedList;

        //=============================================== Accessor Interface

        Node IDynamicDataAccessor.OwnerNode
        {
            get { return OwnerNode; }
            set { OwnerNode = value; }
        }
        PropertyType IDynamicDataAccessor.PropertyType
        {
            get { return PropertyType; }
            set { PropertyType = value; }
        }
        object IDynamicDataAccessor.RawData
        {
            get { return RawData; }
            set { RawData = (List<int>)value; }
        }
        object IDynamicDataAccessor.GetDefaultRawData() { return GetDefaultRawData(); }

        //=============================================== Accessor Implementation

        internal Node OwnerNode { get; set; }
        internal PropertyType PropertyType { get; set; }
        internal static object GetDefaultRawData()
        {
            return new List<int>();
        }
        private List<int> RawData
        {
            get
            {
                if (OwnerNode == null)
                    return __privateList;
                var value = (List<int>)OwnerNode.Data.GetDynamicRawData(PropertyType);
                if (value == null)
                {
                    value = new List<int>();
                    OwnerNode.MakePrivateData();
                    OwnerNode.Data.SetDynamicRawData(PropertyType, value);
                }
                return (List<int>)value;
            }
            set
            {
                __privateList = new List<int>(value);
            }
        }

        //=============================================== Data

        public bool IsModified
        {
            get
            {
                if (OwnerNode == null)
                    return true;
                return OwnerNode.Data.IsModified(PropertyType);
            }
        }
        private void Modifying()
        {
            //_resolvedList = null;
            if (IsModified)
                return;
            //-- Clone
            var orig = (List<int>)OwnerNode.Data.GetDynamicRawData(PropertyType);
            var clone = orig == null ? new List<int>() : new List<int>(orig);
            OwnerNode.MakePrivateData();
            OwnerNode.Data.SetDynamicRawData(PropertyType, clone, false);
        }
        private void ChangeData(List<int> newData)
        {
            if (OwnerNode != null)
            {
                OwnerNode.MakePrivateData();
                OwnerNode.Data.SetDynamicRawData(PropertyType, newData);
            }
            else
                __privateList = newData;
        }
        private void Modified()
        {
            if (OwnerNode != null)
                if (OwnerNode.Data.SharedData != null)
                    OwnerNode.Data.CheckChanges(PropertyType);
        }

        //================================================================================== Construction

        /// <summary>
        /// Initializes a new instance of the NodeList&lt;<see cref="T"/>&gt; class that is empty and has the default initial capacity.
        /// </summary>
        public NodeList()
        {
            __privateList = new List<int>();
        }

        /// <summary>
        /// Initializes a new instance of the NodeList&lt;<see cref="T"/>&gt; class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public NodeList(int capacity)
        {
            __privateList = new List<int>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the NodeList&lt;<see cref="T"/>&gt; class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public NodeList(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            var array = collection.ToArray<T>();
            foreach (var node in array)
                CheckId(node);
            __privateList = new List<int>(from node in array select node.Id);
        }

        public NodeList(IEnumerable<int> idList)
        {
            __privateList = new List<int>(idList);
        }

        //================================================================================== INodeResolver implementation

        public IEnumerable<int> GetIdentifiers()
        {
            return RawData.ToArray();
        }
        public int IdCount
        {
            get { return RawData.Count; }
        }
        public int GetPermittedCount()
        {
            var userId = SenseNet.ContentRepository.Storage.Security.AccessProvider.Current.GetCurrentUser().Id;
            if (userId < 0)
                return IdCount;

            int count = 0;
            foreach (var head in new NodeHeadResolver(RawData))
                if (SecurityHandler.HasPermission(head, PermissionType.See))
                    count++;
            return count;
        }
        public IEnumerable<T> GetPage(int skip, int top)
        {
            return new NodeSet<T>(RawData.Skip(skip).Take(top));
        }

        //================================================================================== Linq support

        public NodeList<T> Skip(int count)
        {
            return new NodeList<T>(RawData.Skip(count));
        }
        public NodeList<T> Take(int count)
        {
            return new NodeList<T>(RawData.Take(count));
        }
        public NodeList<Q> Cast<Q>() where Q : Node
        {
            return new NodeList<Q>(RawData);
        }

        //================================================================================== List implementation

        /// <summary>
        /// Adds a Node object to the end of the <see cref="T:System.Collections.Generic.List`1"></see>, and then fires the Change event.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1"></see>. The value can be null for reference types.</param>
        public void Add(T item)
        {
            CheckId(item);
            Modifying();
            RawData.Add(item == null ? 0 : item.Id);
            Modified();
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="T:System.Collections.Generic.List`1"></see>, and then fires the Change event.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="T:System.Collections.Generic.List`1"></see>. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
        /// <exception cref="T:System.ArgumentNullException">collection is null.</exception>
        public void AddRange(IEnumerable<T> collection)
        {
            var array = collection.ToArray<T>();
            foreach (var node in array)
                CheckId(node);
            Modifying();
            RawData.AddRange(from item in array select item.Id);
            Modified();
        }
        public void AddRange<Q>(IEnumerable<Q> collection) where Q : Node
        {
            var array = collection.ToArray<Q>();
            foreach (var node in array)
                CheckId(node);
            Modifying();
            RawData.AddRange(from item in array select item.Id);
            Modified();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.List`1"></see>, and then fires the Change event if the item was found and removed.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.List`1"></see>. The value can be null for reference types.</param>
        /// <returns>
        /// true if item is successfully removed; otherwise, false.  This method also returns false if item was not found in the <see cref="T:System.Collections.Generic.List`1"></see>.
        /// </returns>
        public bool Remove(T item)
        {
            CheckId(item);
            Modifying();
            int index = RawData.IndexOf(item == null ? 0 : item.Id);
            if (index < 0)
                return false;
            RawData.RemoveAt(index);
            Modified();
            return true;
        }

        ///// <summary>
        ///// Removes the all the elements that match the conditions defined by the specified predicate, and then fires the Change event if there were any.
        ///// </summary>
        ///// <param name="match">The <see cref="T:System.Predicate`1"></see> delegate that defines the conditions of the elements to remove.</param>
        ///// <returns>
        ///// The number of elements removed from the <see cref="T:System.Collections.Generic.List`1"></see> .
        ///// </returns>
        ///// <exception cref="T:System.ArgumentNullException">match is null.</exception>
        //public int RemoveAll(Predicate<T> match)
        //{
        //    var nodes = this.Select<T, T>(x => x).ToList<T>();
        //    int count = nodes.RemoveAll(match);
        //    if (count < 1)
        //        return 0;

        //    Modifying();
        //    var newData = nodes.Select<T, int>(x => x.Id).ToList<int>();
        //    ChangeData(newData);

        //    return count;
        //}

        /// <summary>
        /// Removes all elements from the <see cref="T:SenseNet.ContentRepository.Storage.NodeList`1"></see>, and then fires the Change event.
        /// </summary>
        public void Clear()
        {
            Modifying();
            RawData.Clear();
            Modified();
        }

        public bool Contains(T item)
        {
            CheckId(item);
            return RawData.Contains(item == null ? 0 : item.Id);
        }
        public bool Contains(int id)
        {
            return RawData.Contains(id);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int i = arrayIndex;
            foreach (var node in this)
                array[i++] = node;
        }

        public int Count
        {
            get { return RawData.Count; }
        }

        public bool IsReadOnly
        {
            get { return (RawData as ICollection<T>).IsReadOnly; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            ////if (_resolvedList == null)
            ////    _resolvedList = Node.LoadNodes(RawData);
            ////return _resolvedList.Cast<T>().GetEnumerator();

            //return new PermittedNodeEnumerator<T>(RawData);

            return new BufferedNodeEnumerator<T>(RawData);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Exists(Predicate<T> match)
        {
            var nodes = this.Select<T, T>(x => x).ToList<T>();
            return nodes.Exists(match);
        }

        #region Explicite interface implementations

        //int System.Collections.IList.Add(object value)
        //{
        //    Add((T)value);
        //    return Count - 1;
        //}
        //void System.Collections.IList.Clear()
        //{
        //    Clear();
        //}
        //bool System.Collections.IList.Contains(object value)
        //{
        //    return Contains((T)value);
        //}
        //int System.Collections.IList.IndexOf(object value)
        //{
        //    return IndexOf((T)value);
        //}
        //void System.Collections.IList.Insert(int index, object value)
        //{
        //    Insert(index, (T)value);
        //}
        //bool System.Collections.IList.IsFixedSize
        //{
        //    get { return (RawData as System.Collections.IList).IsFixedSize; }
        //}
        //bool System.Collections.IList.IsReadOnly
        //{
        //    get { return (RawData as System.Collections.IList).IsReadOnly; }
        //}
        //void System.Collections.IList.Remove(object value)
        //{
        //    Remove((T)value);
        //}
        //void System.Collections.IList.RemoveAt(int index)
        //{
        //    RemoveAt(index);
        //}
        //object System.Collections.IList.this[int index]
        //{
        //    get
        //    {
        //        return this[index];
        //    }
        //    set
        //    {
        //        this[index] = (T)value;
        //    }
        //}

        //void System.Collections.ICollection.CopyTo(Array array, int index)
        //{
        //    CopyTo((T[])array, index);
        //}
        //int System.Collections.ICollection.Count
        //{
        //    get { return Count; }
        //}
        //bool System.Collections.ICollection.IsSynchronized
        //{
        //    get { return (RawData as System.Collections.ICollection).IsSynchronized; }
        //}
        //object System.Collections.ICollection.SyncRoot
        //{
        //    get { return (RawData as System.Collections.ICollection).SyncRoot; }
        //}

        #endregion

        //---------------------------------------------------------------------------------- partially supported

        /// <summary>
        /// Only first element is allowed
        /// </summary>
        public T this[int index]
        {
            get
            {
                // Enumerable.First and FirlstOrDefault calls this.
                if(index != 0)
                    throw new NotSupportedException("Random access is not supoported");
                return Node.Load<T>(RawData[index]);
            }
            set
            {
                throw new NotSupportedException("Random access is not supoported");
                //CheckId(value);
                //Modifying();
                //RawData[index] = value.Id;
                //Modified();
            }
        }

        //---------------------------------------------------------------------------------- not supported

        /// <summary>
        /// Not supported
        /// </summary>
        public T this[string path]
        {
            get
            {
                throw new NotSupportedException("Random access is not supoported");
                //foreach (T node in this)
                //    if (node.Path == path)
                //        return node;
                //return null;
                ////for (int i = 0; i < DataSource.Count; i++)
                ////    if (DataSource[i].Path == path)
                ////        return DataSource[i];
                ////return null;
            }
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void InsertRange(int index, IEnumerable<T> collection)
        {
            throw new NotSupportedException("Insert is not supoported");
            //var array = collection.ToArray<T>();
            //foreach (var node in array)
            //    CheckId(node);
            //Modifying();
            //RawData.InsertRange(index, from item in array select item == null ? 0 : item.Id);
            //Modified();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Insert is not supoported");
            //CheckId(item);
            //Modifying();
            //RawData.Insert(index, item == null ? 0 : item.Id);
            //Modified();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException("RemoveAt is not supoported");
            //Modifying();
            //RawData.RemoveAt(index);
            //Modified();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            throw new NotSupportedException("RemoveRange is not supoported");
            //Modifying();
            //RawData.RemoveRange(index, count);
            //Modified();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public int IndexOf(T item)
        {
            throw new NotSupportedException("RemoveRange is not supoported");
            //CheckId(item);
            //return RawData.IndexOf(item == null ? 0 : item.Id);
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void Sort()
        {
            throw new NotSupportedException("Sorting is not supported.");
            //Modifying();
            //RawData.Sort();
            //Modified();
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void Sort(Comparison<T> comparison)
        {
            throw new NotSupportedException("Sorting is not supported.");
            //Modifying();
            //var nodes = this.Select<T, T>(x => x).ToList<T>();
            //nodes.Sort(comparison);
            //var newData = nodes.Select<T, int>(x => x.Id).ToList<int>();
            //ChangeData(newData);
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void Sort(IComparer<T> comparer)
        {
            throw new NotSupportedException("Sorting is not supported.");
            //Modifying();
            //var nodes = this.Select<T, T>(x => x).ToList<T>();
            //nodes.Sort(comparer);
            //var newData = nodes.Select<T, int>(x => x.Id).ToList<int>();
            //ChangeData(newData);
        }
        /// <summary>
        /// Not supported
        /// </summary>
        public void Sort(int index, int count, IComparer<T> comparer)
        {
            throw new NotSupportedException("Sorting is not supported.");
            //Modifying();
            //var nodes = this.Select<T, T>(x => x).ToList<T>();
            //nodes.Sort(index, count, comparer);
            //var newData = nodes.Select<T, int>(x => x.Id).ToList<int>();
            //ChangeData(newData);
        }

        //================================================================================== Tools

        private static void CheckId(Node node)
        {
            if (node == null)
                throw new NotSupportedException("Referenced item cannot be null");
            if (node.Id == 0)
                throw new NotSupportedException("Referenced Node must be saved");
        }

        internal Q GetSingleValue<Q>() where Q : Node
        {
            //if (RawData.Count < 1)
            //    return null;
            //if (_resolvedList == null)
            //    _resolvedList = new List<Node>();
            //var singleNode = Node.Load<T>(RawData[0]);
            //if (singleNode == null)
            //    return null;
            //_resolvedList.Add(singleNode);
            //return _resolvedList[0] as Q;

            if (RawData.Count < 1)
                return null;
            var singleNode = Node.Load<T>(RawData[0]);
            return singleNode as Q;
        }
        internal void SetSingleValue<Q>(Q value) where Q : Node
        {
            //-- Clear if value is null.
            if (value == null)
            {
                //-- Clear if there was any items.
                if (RawData.Count > 0)
                {
                    Modifying(); //-- This clears th resolved list too.
                    RawData.Clear();
                    Modified();
                }
            }
            //-- Insert or change if value is notn ull.
            else
            {
                CheckId(value);
                Modifying(); //-- This clears th resolved list too.
                //-- Add if empty.
                if (RawData.Count == 0)
                {
                    RawData.Add(value.Id);
                }
                //-- ha eddig nem volt ures, akkor a nulladik elem csereje
                //-- Change if not empty.
                else
                {
                    RawData[0] = value.Id;
                }
                Modified();
                //_resolvedList = new List<Node>(new Q[] { value });
            }
        }
    }
}