using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;

namespace SenseNet.Workflow
{
    public class WfContentCollection : ICollection<WfContent>
    {
        private Node[] EmptyNodeArray = new Node[0];
        private string _path;
        private string _fieldname;

        public WfContentCollection(string path, string fieldname)
        {
            _path = path;
            _fieldname = fieldname;
        }

        public void Add(WfContent item)
        {
            var cNode = GetContentNode();
            var node = Node.LoadNode(item.Path);
            cNode.AddReference(_fieldname, node);
        }
        public void Clear()
        {
            var cNode = GetContentNode();
            cNode.ClearReference(_fieldname);
        }
        public bool Contains(WfContent item)
        {
            var list = GetReferences();
            var nList = list as NodeList<Node>;
            if (nList != null)
                return nList.Contains(item.Id);
            var node = Node.LoadNode(item.Path);
            return list.Contains(node);
        }
        public void CopyTo(WfContent[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        public int Count
        {
            get
            {
                var refs = GetContentNode().GetReferences(_fieldname);
                return GetCount(refs);
            }
        }
        private int GetCount(IEnumerable<Node> list)
        {
            if (list == null)
                return 0;
            var nList = list as NodeList<Node>;
            if (nList != null)
                return nList.IdCount;
            var enumerable = list as IEnumerable<Node>;
            if (enumerable != null)
                return enumerable.Count();
            return 0;
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
        public bool Remove(WfContent item)
        {
            var node = GetContentNode();
            var refNode = Node.LoadNode(item.Path);
            var count0 = GetCount(node.GetReferences(_fieldname));
            node.RemoveReference(_fieldname, node);
            var count1 = GetCount(node.GetReferences(_fieldname));
            return count0 != count1;
        }

        public IEnumerator<WfContent> GetEnumerator()
        {
            foreach (var node in GetReferences())
                yield return new WfContent(node.Path);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private Node GetContentNode()
        {
            return Node.LoadNode(_path);
        }
        private IEnumerable<Node> GetReferences()
        {
            return GetContentNode().GetReferences(_fieldname) ?? EmptyNodeArray;
        }

    }
}
