using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

namespace SenseNet.ContentRepository.Storage.Caching.Dependency
{
    public static class CacheDependencyFactory
    {


        internal static CacheDependency CreateNodeHeadDependency(NodeHead nodeHead)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead", "NodeHead cannot be null.");

            var aggregateCacheDependency = new System.Web.Caching.AggregateCacheDependency();

            aggregateCacheDependency.Add(
                new NodeIdDependency(nodeHead.Id),
                new PathDependency(nodeHead.Path)
                );

            return aggregateCacheDependency;
        }

        private static CacheDependency CreateNodeDependency(int id, string path, int nodeTypeId)
        {
            var aggregateCacheDependency = new System.Web.Caching.AggregateCacheDependency();

            aggregateCacheDependency.Add(
                new NodeIdDependency(id),
                new PathDependency(path),
                new NodeTypeDependency(nodeTypeId)
                );

            return aggregateCacheDependency;
        }

        public static CacheDependency CreateNodeDependency(NodeHead nodeHead)
        {
            if (nodeHead == null)
                throw new ArgumentNullException("nodeHead", "NodeHead cannot be null.");

            return CreateNodeDependency(nodeHead.Id, nodeHead.Path, nodeHead.NodeTypeId);
        }

        public static CacheDependency CreateNodeDependency(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node", "Node cannot be null.");

            return CreateNodeDependency(node.Id, node.Path, node.NodeTypeId);
        }

        //ContentListTypeId 
        //BinaryPropertyTypeIds 
        internal static CacheDependency CreateNodeDataDependency(NodeData nodeData)
        {
            if (nodeData == null)
                throw new ArgumentNullException("nodeData");

            var aggregateCacheDependency = new System.Web.Caching.AggregateCacheDependency();

            aggregateCacheDependency.Add(
                new NodeIdDependency(nodeData.Id),
                new PathDependency(nodeData.Path),
                //new VersionIdDependency(nodeData.VersionId),
                new NodeTypeDependency(nodeData.NodeTypeId)
                );

            return aggregateCacheDependency;
        }
    }

}
