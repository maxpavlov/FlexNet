using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenseNet.ContentRepository.Tests
{
	public static class TestTools
	{
		public const long TestStreamLength = 1024;

		public static Stream GetTestStream()
		{
			MemoryStream stream = new MemoryStream(Convert.ToInt32(TestStreamLength));
			for (int i = 0; i < TestStreamLength; i++)
				stream.WriteByte(byte.MaxValue);
			return stream;
		}

		internal static void RemoveNodesAndType(string contentTypeName)
		{
            var query = new NodeQuery();
            var nt = ActiveSchema.NodeTypes[contentTypeName];
            if (nt != null)
            {
                query.Add(new TypeExpression(nt));
                foreach (var nodeId in query.Execute().Identifiers)
                {
                    try
                    {
                        Node.ForceDelete(nodeId);
                    }
                    catch
                    {
                        //suppress the exception that occurs
                        //when node doesn't exist with the given id
                    }
                }
            }
            var ct = ContentType.GetByName(contentTypeName);
            if (ct != null)
            {
                ct.Delete();
                ContentTypeManager.Reset();
            }
		}
		//public static void DeleteContentTypeWithNodes(string contentTypeName)
		//{
		//    NodeType nt = ActiveSchema.NodeTypes[contentTypeName];
		//    if (nt != null)
		//    {
		//        NodeQuery q = new NodeQuery();
		//        q.Add(new TypeExpression(nt));
		//        NodeToken[] tokens = q.ExecuteToTokens();
		//        foreach (NodeToken token in tokens)
		//        {
		//            Node node = Node.LoadNode(token.NodeId);
		//            if (node != null)
		//                node.Delete();
		//        }
		//        ContentType ct = ContentType.GetByName(contentTypeName);
		//        if (ct != null)
		//            ContentTypeInstaller.RemoveContentType(ct);
		//    }
		//}

		public static TypeCollection<PropertyType> GetPropertyTypes(Node node)
		{
			PrivateObject po = new PrivateObject(node);
			object o = po.GetFieldOrProperty("PropertyTypes");
			return (TypeCollection<PropertyType>)o;
		}

		public static bool DataProvider_Current_HasChild(int nodeId)
		{
			return (bool)new PrivateObject(new PrivateType("SenseNet.Storage", "SenseNet.ContentRepository.Storage.Data.DataProvider").InvokeStatic("get_Current")).Invoke("HasChild", nodeId);
		}

        public static BinaryData CreateTestBinary()
        {
            var data = new BinaryData();
            data.SetStream(TestTools.GetTestStream());
            return data;
        }

        public static Folder LoadOrCreateFolder(string path)
        {
            var folder = (Folder)Node.LoadNode(path);
            if (folder != null)
                return folder;

            var parentPath = RepositoryPath.GetParentPath(path);
            var parentFolder = (Folder)Node.LoadNode(parentPath) ?? LoadOrCreateFolder(parentPath);

            folder = new Folder(parentFolder) { Name = RepositoryPath.GetFileName(path) };
            folder.Save();

            return folder;
        }
	}
}