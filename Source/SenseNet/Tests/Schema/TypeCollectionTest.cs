using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests.Schema
{
	/// <summary>
	///This is a test class for SenseNet.ContentRepository.Storage.Schema.TypeCollection&lt;T&gt; and is intended
	///to contain all SenseNet.ContentRepository.Storage.Schema.TypeCollection&lt;T&gt; Unit Tests
	///</summary>
	[TestClass()]
    public class TypeCollectionTest : TestBase
	{
		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public override TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}
		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion

		[TestMethod()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TypeCollection_Constructor_WithNull()
		{
			TypeCollection<NodeType> tc1 = TypeCollectionAccessor<NodeType>.Create((SchemaEditor)null).Target;
		}
		[TestMethod()]
		public void TypeCollection_Constructor_WithAllAllowedTypes()
		{
			SchemaEditor ed = new SchemaEditor();
			TypeCollection<NodeType> tc1 = TypeCollectionAccessor<NodeType>.Create(ed).Target;
			TypeCollection<PermissionType> tc4 = TypeCollectionAccessor<PermissionType>.Create(ed).Target;
			TypeCollection<PropertyType> tc5 = TypeCollectionAccessor<PropertyType>.Create(ed).Target;
		}
		[TestMethod()]
		public void TypeCollection_Add_NodeTypeToSchema()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);

			Assert.IsTrue(nodeTypes.Count == 2 && nodeTypes[0] == ed.NodeTypes[0] && nodeTypes[1] == ed.NodeTypes[1]);
		}
		[TestMethod()]
		[ExpectedException(typeof(InvalidSchemaException))]
		public void TypeCollection_Add_AlienNodeTypeToSchema()
		{
			SchemaEditor ed1 = new SchemaEditor();
			SchemaEditor ed2 = new SchemaEditor();
			ed1.Load();
			ed2.Load();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed1);
			try
			{
				nodeTypes.Add(ed2.NodeTypes[0]);
			}
			catch (Exception e)
			{
				//  :)
				throw e.InnerException;
			}
		}
		[TestMethod()]
		public void TypeCollection_Clear()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);
			nodeTypes.Clear();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);
			nodeTypes.Clear();

			Assert.IsTrue(nodeTypes.Count == 0);
		}
		[TestMethod()]
		public void TypeCollection_Contains_ANodeType()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);

			Assert.IsTrue(nodeTypes.Contains(ed.NodeTypes[0]) && !nodeTypes.Contains(ed.NodeTypes[2]));
		}
		[TestMethod()]
		public void TypeCollection_CopyTo()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);
			nodeTypes.Add(ed.NodeTypes[2]);
			NodeType[] copy = nodeTypes.CopyTo(1);
			Assert.IsTrue(copy.Length == 4 && copy[1] == ed.NodeTypes[0] && copy[2] == ed.NodeTypes[1] && copy[3] == ed.NodeTypes[2]);
		}
		[TestMethod()]
		public void TypeCollection_CopyTo_1()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);
			nodeTypes.Add(ed.NodeTypes[2]);
			NodeType[] copy = new NodeType[4];
			nodeTypes.CopyTo(copy, 1);
			Assert.IsTrue(copy[1] == ed.NodeTypes[0] && copy[2] == ed.NodeTypes[1] && copy[3] == ed.NodeTypes[2]);
		}
		[TestMethod()]
		public void TypeCollection_Count()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);
			nodeTypes.Add(ed.NodeTypes[2]);
			Assert.IsTrue(nodeTypes.Count == 3);
		}
		[TestMethod()]
		public void TypeCollection_GetEnumerator()
		{
			SchemaEditor ed = new SchemaEditor();
			var nodeTypes = TypeCollectionAccessor<NodeType>.Create(ed);
			ed.Load();
			nodeTypes.Add(ed.NodeTypes[0]);
			nodeTypes.Add(ed.NodeTypes[1]);
			nodeTypes.Add(ed.NodeTypes[2]);
			IEnumerator<NodeType> enumerator = nodeTypes.GetEnumerator();
			int index = 0;
			while (enumerator.MoveNext())
				if (ed.NodeTypes[index++] != enumerator.Current)
					Assert.Fail();
			Assert.IsTrue(true);
		}
		[TestMethod()]
		public void TypeCollection_GetItemById()
		{
			SchemaEditor ed = new SchemaEditor();
			ed.Load();
			int id = ed.NodeTypes[1].Id;
			var tc = TypeCollectionAccessor<NodeType>.Create(ed.NodeTypes);
			NodeType nt = tc.GetItemById(id);

			Assert.IsTrue(nt.Id == id);
		}
		[TestMethod()]
		public void TypeCollection_IndexOf()
		{
			SchemaEditor ed = new SchemaEditor();
			ed.Load();
			int id = ed.NodeTypes[1].Id;
			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);
			tc.RemoveAt(0);
			for (int i = 1; i < tc.Count; i++)
				if (tc.IndexOf(ed.NodeTypes[i]) != i - 1)
					Assert.Fail();
			if (tc.IndexOf(ed.NodeTypes[0]) != -1)
				Assert.Fail();
			Assert.IsTrue(true);
		}

		[TestMethod()]
		public void TypeCollection_Insert()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			NodeType nt1 = ed.NodeTypes[0];
			tc.RemoveAt(0);
			tc.Insert(1, nt1);

			Assert.IsTrue(tc[0].Name == "NT1");
			Assert.IsTrue(tc[1].Name == "NT0");
			Assert.IsTrue(tc[2].Name == "NT2");
			Assert.IsTrue(tc[3].Name == "NT3");
			Assert.IsTrue(tc[4].Name == "NT4");
		}
		[TestMethod()]
		public void TypeCollection_Remove()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			NodeType nt1 = ed.NodeTypes[1];
			bool removed = tc.Remove(nt1);
			bool removedAgain = tc.Remove(nt1);

			Assert.IsTrue(tc[0].Name == "NT0");
			Assert.IsTrue(tc[1].Name == "NT2");
			Assert.IsTrue(tc[2].Name == "NT3");
			Assert.IsTrue(tc[3].Name == "NT4");
			Assert.IsTrue(tc.Count == 4);
			Assert.IsTrue(removed && !removedAgain);
		}
		[TestMethod()]
		public void TypeCollection_RemoveAt()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			tc.RemoveAt(1);

			Assert.IsTrue(tc[0].Name == "NT0");
			Assert.IsTrue(tc[1].Name == "NT2");
			Assert.IsTrue(tc[2].Name == "NT3");
			Assert.IsTrue(tc[3].Name == "NT4");
			Assert.IsTrue(tc.Count == 4);
		}

		[TestMethod()]
		public void TypeCollection_GetItem()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			for (int i = 0; i < 5; i++)
				Assert.IsTrue(tc[i].Name == String.Concat("NT", i));
		}
		[TestMethod()]
		public void TypeCollection_SetItem()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			//-- Content: NT0, NT1, NT2, NT3, NT4
			for (int i = 0; i < 5; i++)
				Assert.IsTrue(tc[i].Name == String.Concat("NT", i));

			tc.RemoveAt(0);

			//-- Content: NT1, NT2, NT3, NT4
			for (int i = 0; i < 4; i++)
				Assert.IsTrue(tc[i].Name == String.Concat("NT", i + 1));

			for (int i = 0; i < 4; i++)
				tc[i] = ed.NodeTypes[i];

			//-- Content: NT0, NT1, NT2, NT3
			for (int i = 0; i < 4; i++)
				Assert.IsTrue(tc[i].Name == String.Concat("NT", i));
		}

		[TestMethod()]
		public void TypeCollection_GetItemWithName()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			//-- Content: NT0, NT1, NT2, NT3, NT4
			tc.RemoveAt(0);
			//-- Content: NT1, NT2, NT3, NT4

			Assert.IsTrue(tc["NT2"] == ed.NodeTypes["NT2"]);

		}
		[TestMethod()]
		[ExpectedException(typeof(InvalidSchemaException))]
		public void TypeCollection_SetItemWithName()
		{
			SchemaEditor ed = new SchemaEditor();
			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			tc.Add(ed.CreateNodeType(null, String.Concat("NT0")));
			tc.Add(ed.CreateNodeType(null, String.Concat("NT1")));
			try
			{
				tc["NT1"] = ed.NodeTypes["NT0"];
			}
			catch (Exception e)
			{
				throw e.InnerException;
			}
		}
		[TestMethod()]
		public void TypeCollection_ToArray()
		{
			SchemaEditor ed = new SchemaEditor();
			for (int i = 0; i < 5; i++)
				ed.CreateNodeType(null, String.Concat("NT", i));

			var tc = TypeCollectionAccessor<NodeType>.Create(ed);
			foreach (NodeType nt in ed.NodeTypes)
				tc.Add(nt);

			NodeType[] ntArray = tc.ToArray();

			for (int i = 0; i < 5; i++)
				Assert.IsTrue(ntArray[i] == tc[i] && ntArray[i] == ed.NodeTypes[i]);
		}


	}


}