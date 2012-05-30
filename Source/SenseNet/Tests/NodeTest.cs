using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using System.Reflection;
using System.Data.SqlClient;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using  SenseNet.ContentRepository.Schema;
using System.Linq;
using System.Diagnostics;
using SenseNet.Portal;
using SenseNet.ContentRepository.Versioning;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass]
    public class NodeTest2 : TestBase
	{
		#region Test infrastructure
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
		//Use ClassInitialize to run code before running the first test in the class
		#endregion

		private static string __testRootName = "_Storage2_branch_tests";
		private static string _testRootPath = String.Concat("/Root/", __testRootName);
		private Node _testRoot;
		public Node TestRoot
		{
			get
			{
				if (_testRoot == null)
				{
					_testRoot = Node.LoadNode(_testRootPath);
					if (_testRoot == null)
					{
						Node node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
						node.Name = __testRootName;
						node.Save();
						_testRoot = Node.LoadNode(_testRootPath);
					}
				}
				return _testRoot;
			}
		}
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);
            TestTools.RemoveNodesAndType("File2");
            TestTools.RemoveNodesAndType("BoolTest");
        }

		//===================================================================================== Storage2_pre Tests

		[TestMethod]
		public void Storage2_Milestone1_CreateNodeWithoutSave()
		{
			var fileName = "Storage2_Milestone1_TestFile.txt";
			var file = new File(TestRoot);
			file.Name = fileName;
			var getFileName = file.Name;

			Assert.IsTrue(getFileName == fileName);
		}
		[TestMethod]
		public void Storage2_Milestone1_LoadNode()
		{
			Node node, origNode;

			origNode = Node.LoadNode("/Root");
			node = Node.LoadNode("/Root");
			node = Node.LoadNode("/Root", VersionNumber.LastAccessible);
			node = Node.LoadNode("/Root", VersionNumber.LastMajor);
			node = Node.LoadNode("/Root", VersionNumber.LastMinor);
			// crash: node = Node.LoadNode("/Root", VersionNumber.Header);
			//node = Node.LoadNode("/Root", VersionNumber.Parse("1.0.P"));
			//node = Node.LoadNode("/Root", VersionNumber.Parse("123.456.P"));
			node = Node.LoadNode(2);
			node = Node.LoadNode(2, VersionNumber.LastAccessible);

			User user = Node.Load<User>("/Root/IMS/BuiltIn/Portal/Administrator");
		}
		[TestMethod]
		public void Storage2_Milestone2_SetBinaryWithoutSave()
		{
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ContentType name=""Folder"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.Folder"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
	<DisplayName>Folder</DisplayName>
	<Description>Use folders to group information to one place</Description>
	<Icon>Folder</Icon>
	<Fields>
		<Field name=""Name"" type=""ShortText"">
			<Description>Name of the folder</Description>
		</Field>
		<Field name=""Path"" type=""ShortText"">
			<Description>Path of the folder</Description>
		</Field>
	</Fields>
</ContentType>";

			var node = Node.LoadNode("/Root/System/Schema/ContentTypes/GenericContent/Folder");
			var stream = Tools.GetStreamFromString(xml);
			node.GetBinary("Binary").SetStream(stream);
		}
		[TestMethod]
		public void Storage2_Milestone3_SaveNode()
		{
			var fileName = "Storage2_Milestone2_TestFile.txt";
			var filePath = RepositoryPath.Combine(TestRoot.Path, fileName);
			var text = "Lorem ipsum dolor sit amet...";

			if(Node.Exists(filePath))
                Node.ForceDelete(filePath);

			var stream = Tools.GetStreamFromString(text);

			var file = new File(TestRoot);
			file.Name = fileName;
			file.GetBinary("Binary").FileName = fileName;
			file.GetBinary("Binary").SetStream(stream);

			file.Save();
            
			var loaded = Node.Load<File>(filePath);
			var loadedStream = loaded.GetBinary("Binary").GetStream();
			var loadedText = Tools.GetStreamString(loadedStream);

			Assert.IsTrue(loadedText == text);

			loaded.GetBinary("Binary").FileName = "abc.txt";
			loaded.Save();

			Assert.IsTrue(true);
		}
		[TestMethod]
		public void Storage2_Milestone4_UseReferences()
		{
			var fileName = "Storage2_Milestone2_TestFile.txt";
			var filePath = RepositoryPath.Combine(TestRoot.Path, fileName);
			var text = "Lorem ipsum dolor sit amet...";

			if(ContentType.GetByName("File2") == null)
				ContentTypeInstaller.InstallContentType(@"<?xml version=""1.0"" encoding=""utf-8""?>
					<ContentType name=""File2"" parentType=""File"" handler=""SenseNet.ContentRepository.File"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
						<DisplayName>Folder</DisplayName>
						<Description>Use folders to group information to one place</Description>
						<Icon>Folder</Icon>
						<Fields>
							<Field name=""References"" type=""Reference"" />
						</Fields>
					</ContentType>");

			int rootId = TestRoot.Id;
			int file2ctId = ContentType.GetByName("File2").Id;
			int systemId = Node.LoadNode("/Root/System").Id;

			if (Node.Exists(filePath))
                Node.ForceDelete(filePath);

			//----------------------------- Create

			var file = new File(TestRoot, "File2");
			file.Name = fileName;
			file.GetBinary("Binary").FileName = fileName;
			file.GetBinary("Binary").SetStream(Tools.GetStreamFromString(text));
			file.AddReferences("References", new Node[] { TestRoot, ContentType.GetByName("File2") });

			file.Save();

			//----------------------------- Load

			var loadedNode = Node.Load<File>(filePath);
			var loadedStream = loadedNode.GetBinary("Binary").GetStream();
			var loadedText = Tools.GetStreamString(loadedStream);
			var loadedReferences = loadedNode.GetReferences("References").ToList<Node>();

			Assert.IsTrue(loadedText == text, "#1");
			Assert.IsTrue(loadedReferences.Count == 2, "#2");
			var id0 = loadedReferences[0].Id;
			var id1 = loadedReferences[1].Id;
			Assert.IsTrue(id0 == rootId || id0 == file2ctId, "#3");
			Assert.IsTrue(id1 == rootId || id1 == file2ctId, "#4");

			//----------------------------- Change references

			var refs = ((IEnumerable<Node>)loadedNode["References"]).ToList();
			refs.Remove(TestRoot);

			Assert.IsTrue(refs.Count == 2, "#5");

            loadedNode.RemoveReference("References", TestRoot);
            refs = ((IEnumerable<Node>)loadedNode["References"]).ToList();

			Assert.IsTrue(refs.Count == 1, "#6");
			id0 = refs[0].Id;
			Assert.IsTrue(id0 == file2ctId, "#7");

			refs.Add(Node.LoadNode("/Root/System"));

            loadedNode.SetReferences("References", refs);
            refs = ((IEnumerable<Node>)loadedNode["References"]).ToList();

			Assert.IsTrue(refs.Count == 2, "#8");
			id0 = refs[0].Id;
			id1 = refs[1].Id;
			Assert.IsTrue(id0 == file2ctId, "#9");
			Assert.IsTrue(id1 == systemId, "#10");

			loadedNode.Save();

			//----------------------------- Reload

			var reloadedNode = Node.Load<File>(filePath);
			var reloadedStream = reloadedNode.GetBinary("Binary").GetStream();
			var reloadedText = Tools.GetStreamString(reloadedStream);
			var reloadedReferences = reloadedNode.GetReferences("References").ToList<Node>();

			Assert.IsTrue(reloadedText == text, "#11");
			Assert.IsTrue(reloadedReferences.Count == 2, "#12");
			id0 = reloadedReferences[0].Id;
			id1 = reloadedReferences[1].Id;
            Assert.IsTrue(id0 != id1, "#13");
            Assert.IsTrue(id0 == file2ctId || id0 == systemId, "#14");
            Assert.IsTrue(id1 == file2ctId || id1 == systemId, "#15");

			//----------------------------- Reload

            var identityCheck = reloadedNode.GetReferences("References");
            var countBefore = identityCheck.ToArray().Length;
            reloadedNode.AddReference("References", Repository.Root);
            var countAfter = identityCheck.ToArray().Length;
            Assert.IsTrue(countAfter == countBefore + 1, "#15");
		}
		[TestMethod]
		public void Storage2_Bug_BoolQuery()
		{
			if (ContentType.GetByName("BoolTest") == null)
				ContentTypeInstaller.InstallContentType(@"<?xml version=""1.0"" encoding=""utf-8""?>
					<ContentType name=""BoolTest"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
						<DisplayName>BoolTest</DisplayName>
						<Fields>
							<Field name=""TrueFalse"" type=""Boolean"" />
						</Fields>
					</ContentType>");

			Content c;
			for (int i = 0; i < 10; i++)
			{
				c = Content.CreateNew("BoolTest", TestRoot, "Bool" + i);
				c["TrueFalse"] = (i % 2) != 0;
				c.Save();
			}

			var query = new NodeQuery();
			query.Add(new IntExpression(PropertyType.GetByName("TrueFalse"), ValueOperator.Equal, (int?)0));
			var result = query.Execute();

			var names = (from node in query.Execute().Nodes select node.Name).ToList<string>();
			var count = names.Count;

			for (int i = 0; i < 10; i++)
			{
				//c = Content.Load(TestRoot.Path +"/Bool" + i);
				//c.Delete();
			    var path = TestRoot.Path + "/Bool" + i;
                
                if(Node.Exists(path))
                    Node.ForceDelete(path);
			}
			ContentTypeInstaller.RemoveContentType("BoolTest");

			Assert.IsTrue(count == 5, "#1");
		}
		[TestMethod]
		public void Storage2_Bug_PageCheckin()
		{
            //Assert.Inconclusive("Approving off, None: CheckedOut ==> Publish");

            var binData1 = "PageBinaryData_original";
			var persData1 = "PersonalizationSettingsBinaryData_original";
			var binData2 = "PageBinaryData_edited";
			var persData2 = "PersonalizationSettingsBinaryData_edited";

			var page = new Page(TestRoot);
			page.Name = "TestPage";

			page.Binary = new BinaryData() { ContentType = "text/plain", FileName = "a.aspx" };
			page.PersonalizationSettings = new BinaryData() { ContentType = "text/plain", FileName = "a.PersonalizationSettings" };
			page.Binary.SetStream(Tools.GetStreamFromString(binData1));
			page.PersonalizationSettings.SetStream(Tools.GetStreamFromString(persData1));

			page.Save();
			var pageId = page.Id;

			page.CheckOut();

			page = Node.Load<Page>(pageId);
			page.Binary.SetStream(Tools.GetStreamFromString(binData2 + "bad"));
			page.PersonalizationSettings.SetStream(Tools.GetStreamFromString(persData2 + "bad"));
			page.Save();

			page = Node.Load<Page>(pageId);
			page.Binary.SetStream(Tools.GetStreamFromString(binData2));
			page.PersonalizationSettings.SetStream(Tools.GetStreamFromString(persData2));
			page.Save();

			page.CheckIn();

			page = Node.Load<Page>(pageId);
			var bin = Tools.GetStreamString(page.Binary.GetStream());
			var pers = Tools.GetStreamString(page.PersonalizationSettings.GetStream());

			Assert.IsTrue(bin == binData2, "#1");
			Assert.IsTrue(pers == persData2, "#2");
		}

		[TestMethod]
		public void Storage2_BatchLoad()
		{
            //Trace.WriteLine("========== SELECT Nodes WHERE Id >= 20 AND Id < 30 ==========");

			var query1 = new NodeQuery();
			query1.Add(new IntExpression(IntAttribute.Id, ValueOperator.GreaterThanOrEqual, 20));
			query1.Add(new IntExpression(IntAttribute.Id, ValueOperator.LessThan, 30));
			query1.Orders.Add(new SearchOrder(IntAttribute.Id));

			CheckQuery(query1, "#1");

            //Trace.WriteLine("========== SELECT Nodes WHERE ContentType IS 'ContentType' ORDER BY Path ==========");

			var query2 = new NodeQuery();
			query2.Add(new TypeExpression(ActiveSchema.NodeTypes["ContentType"], false));
			query2.Orders.Add(new SearchOrder(StringAttribute.Path));

			CheckQuery(query2, "#2");

            //Trace.WriteLine("=====================================================================");
		}
		private static void CheckQuery(NodeQuery query, string assertPrefix)
		{
			var result = query.Execute().Nodes;

            var idListBefore = GetInnerIdList(result);
			var nodes = new List<Node>(result);
			var idListAfter = (from node in nodes select node.Id).ToList<int>();

			Assert.IsTrue(idListBefore.Count == idListAfter.Count, assertPrefix + ": counts are not equal");

			for (int i = 0; i < idListBefore.Count; i++)
				Assert.IsTrue(idListBefore[i] == idListAfter[i], assertPrefix + ": Order error: " + i);
		}
		private static List<int> GetInnerIdList(IEnumerable<Node> nodeList)
		{
            //var pager = nodeList as NodePager<Node>;
            //var pt = new PrivateType(typeof(NodePager<Node>));
            var pager = nodeList as NodeList<Node>;
            var pt = new PrivateType(typeof(NodeList<Node>));
            var x = new PrivateObject(pager, pt);
			var y = x.GetField("__privateList", BindingFlags.NonPublic | BindingFlags.Instance);
			var z = new List<int>((List<int>)y);
			return z;
		}

		[TestMethod]
		public void Storage2_NodeEnumeratorCache()
		{
			var query = new NodeQuery();
			query.Add(new IntExpression(IntAttribute.Id, ValueOperator.GreaterThanOrEqual, 20));
			query.Add(new IntExpression(IntAttribute.Id, ValueOperator.LessThan, 30));
			query.Orders.Add(new SearchOrder(IntAttribute.Id));
			var result = query.Execute().Nodes;

			//----

			var nodes1 = new List<Node>(result);
			var nodes2 = new List<Node>(result);

			//----

			var nodes3 = new List<Node>();
			foreach (var node in result)
				nodes3.Add(node);
			var nodes4 = new List<Node>();
			foreach (var node in result)
				nodes4.Add(node);

			//----

			var nodes5 = result.ToArray<Node>();
			var nodes6 = result.ToArray<Node>();

			//----

			for (int i = 0; i < nodes1.Count; i++)
			{
				Assert.IsTrue(Object.ReferenceEquals(nodes1[i], nodes2[i]), "#1");
				Assert.IsTrue(Object.ReferenceEquals(nodes1[i], nodes3[i]), "#2");
				Assert.IsTrue(Object.ReferenceEquals(nodes1[i], nodes4[i]), "#3");
				Assert.IsTrue(Object.ReferenceEquals(nodes1[i], nodes5[i]), "#4");
				Assert.IsTrue(Object.ReferenceEquals(nodes1[i], nodes6[i]), "#5");
			}
		}

		[TestMethod]
		public void Storage2_Compatibility_BinaryCrossReference()
		{
			var file1 = new File(TestRoot);
            file1.Name = "Storage2_Compatibility_BinaryCrossReference-1";
			file1.GetBinary("Binary").FileName = "1.txt";
			file1.GetBinary("Binary").SetStream(Tools.GetStreamFromString("1111"));
			file1.Save();
			var file1id = file1.Id;

			var file2 = new File(TestRoot);
            file1.Name = "Storage2_Compatibility_BinaryCrossReference-2";
            file2.GetBinary("Binary").FileName = "2.txt";
			file2.GetBinary("Binary").SetStream(Tools.GetStreamFromString("2222"));
			file2.Save();
			var file2id = file2.Id;

			file1 = Node.Load<File>(file1id);
			file2 = Node.Load<File>(file2id);
			file2.Binary = file1.Binary;
			file2.Save();
			file1.GetBinary("Binary").FileName = "3.txt";
			file1.GetBinary("Binary").SetStream(Tools.GetStreamFromString("3333"));
			file1.Save();

		}

        [TestMethod]
        public void Storage2_CreatePrivateDataOnlyOnDemand()
        {
            var node = new GenericContent(TestRoot, "Car");
            var isShared = node.Data.IsShared;
            var hasShared = node.Data.SharedData != null;
            Assert.IsFalse(isShared, "#1");
            Assert.IsFalse(hasShared, "#2");

            node.Name = Guid.NewGuid().ToString();
            node.Save();
            var id = node.Id;

            //----------------------------------------------

            node = Node.Load<GenericContent>(id);
            isShared = node.Data.IsShared;
            hasShared = node.Data.SharedData != null;
            Assert.IsTrue(isShared, "#3");
            Assert.IsFalse(hasShared, "#4");

            node.Index += 1;

            isShared = node.Data.IsShared;
            hasShared = node.Data.SharedData != null;
            Assert.IsFalse(isShared, "#5");
            Assert.IsTrue(hasShared, "#6");
        }

        //chars:4517
        private const string LONG_TEXT1 = "Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text1111";
        private const string LONG_TEXT2 = "Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text2222";

        [TestMethod]
        public void Storage2_LoadLongText()
        {
            const string longPropertyName = "HTMLFragment";
            var webContent = new GenericContent(TestRoot, "HTMLContent");
            webContent[longPropertyName] = LONG_TEXT1;
            webContent.Save();

            var reloadedText = webContent[longPropertyName] as string;
            
            Assert.IsTrue(!string.IsNullOrEmpty(reloadedText), "New node: Reloaded longtext property is null after save.");
            Assert.AreEqual(LONG_TEXT1, reloadedText, "New node: Reloaded longtext is not the same as the original.");

            webContent = Node.Load<GenericContent>(webContent.Id);

            reloadedText = webContent[longPropertyName] as string;
            Assert.AreEqual(LONG_TEXT1, reloadedText, "Existing node: Reloaded longtext is not the same as the original #1");

            webContent[longPropertyName] = LONG_TEXT2;
            reloadedText = webContent[longPropertyName] as string;
            Assert.AreEqual(LONG_TEXT2, reloadedText, "Existing node: Reloaded longtext is not the same as the original #2");

            webContent.Save();
            webContent = Node.Load<GenericContent>(webContent.Id);
            reloadedText = webContent[longPropertyName] as string;

            Assert.AreEqual(LONG_TEXT2, reloadedText, "Existing node: Reloaded longtext is not the same as the original #3");
        }

        [TestMethod]
        public void Storage2_LoadBinary()
        {
            //chars:4514
            var longText = "Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text Long Test Text";
            var longFile = new File(TestRoot);
            longFile.Binary.SetStream(Tools.GetStreamFromString(longText));
            longFile.Save();

            var bd = longFile["Binary"] as BinaryData;
            var reloadedText = Tools.GetStreamString(bd.GetStream());

            Assert.IsTrue(!string.IsNullOrEmpty(reloadedText), "Reloaded binary property is null after save.");
            Assert.AreEqual(longText, reloadedText, "Reloaded binary is not the same as the original.");
        }

        [TestMethod]
        public void Node_HasProperty()
        {
            var car = Content.CreateNew("Car", TestRoot, null).ContentHandler;
            foreach (var propName in new[]{"Id","NodeType","ContentListId","ContentListType","Parent","ParentId","Name","DisplayName","Path",
                    "Index","IsModified","IsDeleted","IsInherited","NodeCreationDate","NodeCreatedBy","Version","VersionId","CreationDate","ModificationDate","CreatedBy",
                    "CreatedById","ModifiedBy","ModifiedById","Locked","Lock","LockedById","LockedBy","ETag","LockType","LockTimeout","LockDate","LockToken","LastLockUpdate","Security"})
                Assert.IsTrue(car.HasProperty(propName), "#1: HasProperty falied: " + propName);
            foreach(var prop in car.PropertyTypes)
                Assert.IsTrue(car.HasProperty(prop.Name), "#2: HasProperty falied: " + prop.Name);
        }

        [TestMethod]
        public void Node_SetDates()
        {
            var userFolder = Node.LoadNode(RepositoryPath.GetParentPath(User.Administrator.Path));
            var users = new User[8];
            for (int i = 0; i < users.Length; i++)
                users[i] = SenseNet.ContentRepository.Tests.Security.PermissionTest.LoadOrCreateUser(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), userFolder);

            var times = new DateTime[8];
            for (int i = 0; i < times.Length; i++)
                times[i] = new DateTime(2222, 2, 10 + i);

            var t0 = new DateTime[8];

            var content = Content.CreateNew("Car", TestRoot, Guid.NewGuid().ToString());
            var gcontent = (GenericContent)content.ContentHandler;
            System.Threading.Thread.Sleep(1000);
            gcontent.Save(); // 05:28:49.683 | 05:29:45.590 | 05:28:49.683 | 05:29:45.590
            content = Content.Load(content.Id);
            gcontent = (GenericContent)content.ContentHandler;
            t0[0] = gcontent.NodeCreationDate;
            t0[1] = gcontent.NodeModificationDate;
            t0[2] = gcontent.CreationDate;
            t0[3] = gcontent.ModificationDate;

            System.Threading.Thread.Sleep(1000);
            gcontent.CheckOut(); // 05:28:49.683 | 05:31:33.810 | 05:28:49.683 | 05:29:45.590 | 05:28:49.683 | 05:31:33.810
            content = Content.Load(content.Id);
            gcontent = (GenericContent)content.ContentHandler;
            t0[4] = gcontent.NodeCreationDate;
            t0[5] = gcontent.NodeModificationDate;
            t0[6] = gcontent.CreationDate;
            t0[7] = gcontent.ModificationDate;

            // 0  NodeCreationDate:     {5:28:49}
            // 1  NodeModificationDate:          {5:29:45}
            // 2  CreationDate:         {5:28:49}
            // 3  ModificationDate:              {5:29:45}
            // 4  NodeCreationDate:     {5:28:49}
            // 5  NodeModificationDate:                   {5:31:33}
            // 6  CreationDate:         {5:28:49}
            // 7  ModificationDate:                       {5:31:33}

            // 0  NodeCreationDate:     {5:28:49}
            // 1  NodeModificationDate:          {5:29:45}
            // 2  CreationDate:         {5:28:49}
            // 3  ModificationDate:              {5:29:45}
            // 4  NodeCreationDate:     {5:28:49}
            // 5  NodeModificationDate:                            {5:31:33}
            // 6  CreationDate:                           {5:28:49}
            // 7  ModificationDate:                                {5:31:33}

            Assert.IsTrue(t0[2] == t0[0], "#1: NodeCreationDate and First version's CreationDate are not equal ");
            Assert.IsTrue(t0[4] == t0[0], "#2: NodeCreationDates are not equal after version 2 created");
            Assert.IsTrue(t0[1] > t0[0], "#3: NodeModificationDate is not greater than NodeCreationDate");
            Assert.IsTrue(t0[5] > t0[4], "#4: NodeModificationDate is not greater than NodeCreationDate");
            Assert.IsTrue(t0[3] == t0[1], "#5: ModificationDate and NodeModificationDate are not equal");
            Assert.IsTrue(t0[7] == t0[5], "#6: ModificationDate and NodeModificationDate are not equal");
            Assert.IsTrue(t0[7] > t0[3], "#4: ModificationDate2 is not greater than ModificationDate1");
            //Assert.IsTrue(t0[6] > t0[2], "#4: CreationDate2 is not greater than CreationDate1");


            gcontent.UndoCheckOut();
            gcontent.ForceDelete();

            //--

            content = Content.CreateNew("Car", TestRoot, Guid.NewGuid().ToString());
            gcontent = (GenericContent)content.ContentHandler;

            gcontent.NodeCreationDate = times[0];
            gcontent.NodeModificationDate = times[1];
            gcontent.CreationDate = times[2];
            gcontent.ModificationDate = times[3];
            gcontent.NodeCreatedBy = users[0];
            gcontent.NodeModifiedBy = users[1];
            gcontent.CreatedBy = users[2];
            gcontent.ModifiedBy = users[3];

            gcontent.Save();
            content = Content.Load(content.Id);
            gcontent = (GenericContent)content.ContentHandler;

            var nodeCreationDate1 = gcontent.NodeCreationDate;
            var nodeModificationDate1 = gcontent.NodeModificationDate;
            var creationDate1 = gcontent.CreationDate;
            var modificationDate1 = gcontent.ModificationDate;
            var nodeCreatedById1 = gcontent.NodeCreatedById;
            var nodeModifiedById1 = gcontent.NodeModifiedById;
            var createdById1 = gcontent.CreatedById;
            var modifiedById1 = gcontent.ModifiedById;

            gcontent.CheckOut();
            content = Content.Load(content.Id);
            gcontent = (GenericContent)content.ContentHandler;

            gcontent.NodeCreationDate = times[4];
            gcontent.NodeModificationDate = times[5];
            gcontent.CreationDate = times[6];
            gcontent.ModificationDate = times[7];
            gcontent.NodeCreatedBy = users[4];
            gcontent.NodeModifiedBy = users[5];
            gcontent.CreatedBy = users[6];
            gcontent.ModifiedBy = users[7];

            gcontent.Save();
            content = Content.Load(content.Id);
            gcontent = (GenericContent)content.ContentHandler;

            var nodeCreationDate2 = gcontent.NodeCreationDate;
            var nodeModificationDate2 = gcontent.NodeModificationDate;
            var creationDate2 = gcontent.CreationDate;
            var modificationDate2 = gcontent.ModificationDate;
            var nodeCreatedById2 = gcontent.NodeCreatedById;
            var nodeModifiedById2 = gcontent.NodeModifiedById;
            var createdById2 = gcontent.CreatedById;
            var modifiedById2 = gcontent.ModifiedById;

            //-- cleanup

            gcontent.UndoCheckOut();
            gcontent.ForceDelete();
            foreach (var user in users)
                user.ForceDelete();

            //-- Assert

            Assert.IsTrue(nodeCreationDate1 == times[0], String.Format("nodeCreationDate1 is {0}, expected: {1}", nodeCreationDate1, times[0]));
            Assert.IsTrue(nodeModificationDate1 == times[1], String.Format("nodeModificationDate1 is {0}, expected: {1}", nodeModificationDate1, times[1]));
            Assert.IsTrue(creationDate1 == times[2], String.Format("creationDate1 is {0}, expected: {1}", creationDate1, times[2]));
            Assert.IsTrue(modificationDate1 == times[3], String.Format("modificationDate1 is {0}, expected: {1}", modificationDate1, times[3]));
            //Assert.IsTrue(nodeCreatedById1 == users[2].Id, String.Format("nodeCreatedById1 is {0}, expected: {1}", nodeCreatedById1, users[2].Id)); //!!! if brand new --> NodeCreatedById = CreatedById
            Assert.IsTrue(nodeCreatedById1 == users[0].Id, String.Format("nodeCreatedById1 is {0}, expected: {1}", nodeCreatedById1, users[0].Id));
            Assert.IsTrue(nodeModifiedById1 == users[1].Id, String.Format("nodeModifiedById1 is {0}, expected: {1}", nodeModifiedById1, users[2].Id));
            Assert.IsTrue(createdById1 == users[2].Id, String.Format("createdById1 is {0}, expected: {1}", createdById1, users[3].Id));
            Assert.IsTrue(modifiedById1 == users[3].Id, String.Format("modifiedById1 is {0}, expected: {1}", modifiedById1, users[4].Id));

            Assert.IsTrue(nodeCreationDate2 == times[4], String.Format("nodeCreationDate2 is {0}, expected: {1}", nodeCreationDate2, times[4]));
            Assert.IsTrue(nodeModificationDate2 == times[5], String.Format("nodeModificationDate2 is {0}, expected: {1}", nodeModificationDate2, times[5]));
            Assert.IsTrue(creationDate2 == times[6], String.Format("creationDate2 is {0}, expected: {1}", creationDate2, times[6]));
            Assert.IsTrue(modificationDate2 == times[7], String.Format("modificationDate2 is {0}, expected: {1}", modificationDate2, times[7]));
            Assert.IsTrue(nodeCreatedById2 == users[4].Id, String.Format("nodeCreatedById2 is {0}, expected: {1}", nodeCreatedById2, users[4].Id));
            Assert.IsTrue(nodeModifiedById2 == users[5].Id, String.Format("nodeModifiedById2 is {0}, expected: {1}", nodeModifiedById2, users[5].Id));
            Assert.IsTrue(createdById2 == users[6].Id, String.Format("createdById2 is {0}, expected: {1}", createdById2, users[6].Id));
            Assert.IsTrue(modifiedById2 == users[7].Id, String.Format("modifiedById2 is {0}, expected: {1}", modifiedById2, users[7].Id));

            //-- Check script
            //SELECT     N.NodeId, 
            //    N.CreationDate NodeCreationDate, N.CreatedById NodeCreatedById, N.ModificationDate NodeModificationDate, N.ModifiedById NodeModifiedById, 
            //    [Public].VersionId, [Public].CreationDate AS CreationDate1, [Public].CreatedById AS CreatedById1, [Public].ModificationDate AS ModificationDate1, [Public].ModifiedById AS ModifiedById1,
            //    [Draft].VersionId, [Draft].CreationDate AS CreationDate2, [Draft].CreatedById AS CreatedById2, [Draft].ModificationDate AS ModificationDate2, [Draft].ModifiedById AS ModifiedById2
            //FROM         Nodes AS N INNER JOIN
            //                      Versions [Draft] ON N.LastMinorVersionId = [Draft].VersionId INNER JOIN
            //                      Versions [Public] ON N.LastMajorVersionId = [Public].VersionId
            //WHERE N.NodeId = 1132
        }

        [TestMethod]
        public void Node_CreatedBy()
        {
            var content = Content.CreateNew("Car", TestRoot, null);
            content["CreatedBy"] = User.Visitor;
            content.Save();
            var id = content.Id;

            var content1 = Content.Load(id);
            var head = NodeHead.Get(id);

            Assert.IsTrue(content.ContentHandler.CreatedById == User.Visitor.Id, String.Format("content.CreatedById is {0}, expected: {1}", content.ContentHandler.CreatedById, User.Visitor.Id));
            //Assert.IsTrue(head.CreatorId == User.Visitor.Id, String.Format("head.CreatorId is {0}, expected: {1}", head.CreatorId, User.Visitor.Id));
        }
	}

	[TestClass()]
    public class NodeTest : TestBase
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
		//Use ClassInitialize to run code before running the first test in the class

		[ClassInitialize]
		public static void InitializePlayground(TestContext testContext)
		{
			InstallRefTestNodeType();
		}


		private static string _testRootName = "_NodeTests";
		private static string _testRootPath = String.Concat("/Root/", _testRootName);
		/// <summary>
		/// Do not use. Instead of TestRoot property
		/// </summary>
		private Node _testRoot;
		public Node TestRoot
		{
			get
			{
				if (_testRoot == null)
				{
					_testRoot = Node.LoadNode(_testRootPath);
					if (_testRoot == null)
					{
						Node node = NodeType.CreateInstance("SystemFolder", Node.LoadNode("/Root"));
						node.Name = _testRootName;
						node.Save();
						_testRoot = Node.LoadNode(_testRootPath);
					}
				}
				return _testRoot;
			}
		}
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);

            TestTools.RemoveNodesAndType("RepositoryTest_TestNode");
            TestTools.RemoveNodesAndType("RepositoryTest_TestNode2");
            TestTools.RemoveNodesAndType("RepositoryTest_RefTestNode");

            ContentType ct;
            ct = ContentType.GetByName("RepositoryTest_TestNode2");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);
            ct = ContentType.GetByName("RepositoryTest_TestNode");
            if (ct != null)
                ContentTypeInstaller.RemoveContentType(ct);

            #region builserver error

            // RemoveContentType("File2") is commented out due to build server error. See details below:
            
            //Class Cleanup method NodeTest.DestroyPlayground failed. Error Message: System.ApplicationException: Cannot delete ContentType 'File2' because one or more Content use this type or any descendant type.. Stack Trace:     at SenseNet.ContentRepository.Schema.ContentType.Delete() in c:\Builds\8\SenseNet\bpcheckin\Sources\Source\SenseNet\ContentRepository\Schema\ContentType.cs:line 574
            //at SenseNet.ContentRepository.Schema.ContentTypeInstaller.RemoveContentType(ContentType contentType) in c:\Builds\8\SenseNet\bpcheckin\Sources\Source\SenseNet\ContentRepository\Schema\ContentTypeInstaller.cs:line 166
            //at SenseNet.ContentRepository.Tests.NodeTest.DestroyPlayground() in c:\Builds\8\SenseNet\bpcheckin\Sources\Source\SenseNet\Tests\NodeTest.cs:line 580
            
            //ct = ContentType.GetByName("File2");
            //if (ct != null)
            //    ContentTypeInstaller.RemoveContentType(ct);

            #endregion

        }

		//===================================================================================== Tests

		#region Load tests
		[TestMethod()]
		public void Node_LoadRoot()
		{
			Node node = Repository.Root;
			CheckRootNode(node);
		}
		[TestMethod()]
		public void Node_LoadById()
		{
			int nodeID = 8;
			Node node = Node.LoadNode(nodeID);
			CheckEveryoneGroupNode(node);
		}
        [TestMethod]
        public void Node_LoadByVersionId()
        {
            int versionId = 33;
            Node node = Node.LoadNodeByVersionId(versionId);
            Assert.IsTrue(node.VersionId == versionId);
        }


		[TestMethod()]
		public void Node_LoadById_Invalid()
		{
			int nodeID = -1;
			Node node = Node.LoadNode(nodeID);
			Assert.IsNull(node, "Node was not null.");
		}

		[TestMethod()]
		public void Node_Invalid_LoadByPath()
		{
			string path = "/Root/IMS/BuiltIn/Portal/Everyone";
			Node node = Node.LoadNode(path);
			CheckEveryoneGroupNode(node);
		}

		[TestMethod()]
		public void Node_InvalidLoadByPath_Invalid()
		{
			string path = "[invalidpath]";
			Node node = Node.LoadNode(path);
			Assert.IsNull(node);
		}

		[TestMethod()]
		public void Node_InvalidLoadByPath_NotExistent()
		{
			string path = "/MyPortal/A/B/C.D";
			Node node = Node.LoadNode(path);
			Assert.IsNull(node, "Expected: node is null");
		}

		//------------------------------------------------------- Generic

		[TestMethod]
		public void Node_LoadGeneric_ById()
		{
			int nodeID = 8;
			Group node = Node.Load<Group>(nodeID);
			CheckEveryoneGroupNode(node);
		}
		[TestMethod]
		public void Node_LoadGeneric_ByPath()
		{
			string path = "/Root/IMS/BuiltIn/Portal/Everyone";
			Group node = Node.Load<Group>(path);
			CheckEveryoneGroupNode(node);
		}

		//[TestMethod()]
		//public void Node_LoadByIdAndVersion()
		//{
		//    Assert.Inconclusive("Not implemented feature.");

		//    int nodeID = 8;

		//    //-- test start
		//    Node node = Node.Load(nodeID, new VersionNumber(2, 0));
		//    //-- test end

		//    Assert.IsTrue(node.Id == nodeID, "ID was not equal to the expected");
		//    Assert.IsTrue(node.Path == "/MyPortal/Portal Pages/Internet Portal/MyPage.aspx", "Path was not equal to the expected");
		//    Assert.IsTrue(node.Version.Major == 2 && node.Version.Minor == 0, "Version was not equal to the expected");
		//}

		//[TestMethod()]
		//public void Node_LoadByPathAndVersion()
		//{
		//    Assert.Inconclusive("Not implemented feature.");

		//    string path = "/MyPortal/Portal Pages/Internet Portal/MyPage.aspx";

		//    //-- test start
		//    Node node = Node.Load(path, new VersionNumber(2, 0));
		//    //-- test end

		//    Assert.IsTrue(node.Id == 8, "ID was not equal to the expected");
		//    Assert.IsTrue(node.Path == path, "Path was not equal to the expected");
		//    Assert.IsTrue(node.Version.Major == 2 && node.Version.Minor == 0, "Version was not equal to the expected");
		//}

		////[DeploymentItem("SenseNet.Storage.dll")]
		//[TestMethod()]
		//public void Node_LoadAllVersions()
		//{
		//    Assert.Inconclusive("Not implemented feature.");

		//    //int nodeID = 8;

		//    ////-- test start
		//    //Node node = Node.Load(nodeID);
		//    //var list = node.LoadAllVersions();
		//    ////-- test end

		//    //Assert.IsTrue(list.Count == 3, String.Format("Node.LoadAllVersions was not load {0} items. Expected: 3", list.Count));
		//    //Assert.IsTrue(list[0].Version.ToString() == "V1.0", String.Format("Bad version: {0} (expected: V1.0)", list[0].Version.ToString()));
		//    //Assert.IsTrue(list[1].Version.ToString() == "V2.0", String.Format("Bad version: {0} (expected: V2.0)", list[1].Version.ToString()));
		//    //Assert.IsTrue(list[2].Version.ToString() == "V2.1", String.Format("Bad version: {0} (expected: V2.1)", list[2].Version.ToString()));
		//}

		#endregion

		#region Save tests

		[TestMethod()]
		public void Node_CanFolderSave()
		{
			Folder folder = new Folder(this.TestRoot);
			//folder.Name = "TestFolder";
			//folder.UrlName = "TestFolder";
			folder.Save();
			Assert.IsTrue(folder.Id > 0);
		}

		[TestMethod()]
        [ExpectedException(typeof(NodeAlreadyExistsException))]
		public void Node_FolderSave_WithSameName()
		{
			string guid = Guid.NewGuid().ToString();
			Folder folder = new Folder(this.TestRoot);
			folder.Name = guid;
			folder.Save();
			Folder folder2 = new Folder(this.TestRoot);
			folder2.Name = guid;
			folder2.Save();
		}

		[TestMethod()]
        [ExpectedException(typeof(NodeAlreadyExistsException))]
		public void Node_FileSave_WithSameName()
		{
			string guid = Guid.NewGuid().ToString();
			File file = new File(this.TestRoot);
			file.Name = guid;
			file.Save();
			File file2 = new File(this.TestRoot);
			file2.Name = guid;
			file2.Save();
		}

		#endregion

		#region Delete tests

		private Node CreateTestNodeForDelete()
		{
			InstallTestNodeType();
			string path = RepositoryPath.Combine(this.TestRoot.Path, "Delete_TestNode");
			TestNode node = (TestNode)Node.LoadNode(path);
			if (node == null)
			{
				node = new TestNode(this.TestRoot);
				node.Name = "Delete_TestNode";
				node.TestInt = 1234;
				node.Save();
			}

			return (Node)node;
		}

		[TestMethod]
		public void Node_DeletePhysical()
		{
			TestNode source = CreateTestNodeForDelete() as TestNode;
			source.ForceDelete();

			Node target = Node.LoadNode(source.Path);
			Assert.IsNull(target, "Node has NOT been deleted from database.");
		}
		[TestMethod]
		public void Node_DeletePhysical_WithChildren()
		{
			InstallTestNodeType();

			string path = RepositoryPath.Combine(this.TestRoot.Path, "Delete_TestNode_Parent");
			TestNode parentNode = (TestNode)Node.LoadNode(path);
			if (parentNode == null)
			{
				parentNode = new TestNode(this.TestRoot);
				parentNode.Name = "Delete_TestNode_Parent";
				parentNode.Save();
			}

			Assert.IsNotNull(parentNode, "Parent TestNode has not been created.");

			string childpath = RepositoryPath.Combine(parentNode.Path, "Delete_TestNode_Child");

			TestNode childNode = (TestNode)Node.LoadNode(childpath);
			if (childNode == null)
			{
				childNode = new TestNode(parentNode);
				childNode.Name = "Delete_TestNode_Child";
				childNode.Save();
			}

			Assert.IsNotNull(childNode, "Child TestNode has not been created.");
			Assert.IsTrue(TestTools.DataProvider_Current_HasChild(parentNode.Id), "ParentNode has not children.");

			parentNode.ForceDelete();

			Node target = Node.LoadNode(parentNode.Path);
			Assert.IsNull(target, "Node has NOT been deleted from database.");
		}

		[TestMethod]
		public void Node_DeletePhysical_LockedNode_SameUser()
		{
			TestNode source = CreateTestNodeForDelete() as TestNode;
			source.Lock.Lock();
			Assert.IsTrue(source.Lock.Locked, "Eventough Lock() was called the node is not locked.");
			source.ForceDelete();

			Node target = Node.LoadNode(source.Path);
			Assert.IsNull(target, "Node has NOT been deleted from database.");
		}

        [TestMethod]
        public void Node_DeletePhysical_Bug1967_SecurityEntries()
        {
            var folderName = "Folder_Bug1967";
            var folderPath = RepositoryPath.Combine(TestRoot.Path, folderName);
            Folder folder = Node.Load<Folder>(folderPath);
            if (folder == null)
            {
                folder = new Folder(TestRoot);
                folder.Name = folderName;
                folder.Save();
            }

            folder.Security.BreakInheritance();
            folder.Security.SetPermission(User.Administrator, true, PermissionType.RunApplication, PermissionValue.Deny);
            folder.ForceDelete();
        }

		#endregion

		#region DeleteVersion tests

		[TestMethod]
		public void Node_DeleteVersion()
		{
			RefTestNode delVer = CreateRefTestNode("NodeDeleteVersionTest");
			delVer.VersioningMode = VersioningType.MajorAndMinor;
			delVer.Save();

			delVer.NickName = "1.2";
			delVer.Save();
			int major = delVer.Version.Major;
			int minor = delVer.Version.Minor;

			delVer.CheckOut();

			delVer.NickName = "1.3";
			delVer.Save();

			delVer.UndoCheckOut();

			Assert.IsTrue(delVer.NickName == "1.2", "#1");
			Assert.IsTrue(delVer.Version.Major == major && delVer.Version.Minor == minor, "#2");
		}

		#endregion

		#region Property tests

		[TestMethod()]
		public void Node_Properties_AttributeDefaults()
		{
			object o; // Dummy place
			Folder folder = new Folder(this.TestRoot);
			Assert.IsNotNull(folder.Children);
			Assert.AreEqual(0, folder.ChildCount);
			o = folder.CreationDate;
			Assert.IsNotNull(folder.CreatedBy);
			Assert.AreEqual(0, folder.Id);
			Assert.IsFalse(folder.IsDeleted);
			Assert.IsTrue(folder.IsModified);
			Assert.IsNotNull(folder.Lock);
			o = folder.ModificationDate;
			Assert.IsNotNull(folder.ModifiedBy);
			string name = folder.Name;
			Assert.IsNotNull(folder.NodeType);
			Assert.AreEqual(this.TestRoot.Id, folder.ParentId);
			string path = this.TestRoot.Path + RepositoryPath.PathSeparator + name;
			Assert.AreEqual(path, folder.Path);
			Assert.IsNotNull(TestTools.GetPropertyTypes(folder));
			Assert.IsNotNull(folder.Security);
			Assert.AreEqual(name, folder.Name);
			Assert.AreEqual(path, folder.Path);
			Assert.AreEqual(1, folder.Version.Major);
			Assert.AreEqual(0, folder.Version.Minor);
		}

		[TestMethod()]
		public void File_Save_NullBinary()
		{
			File file = new File(this.TestRoot);

			// Save binary
			file.Binary = null;
			file.Save();
			int id = file.Id;

			// Load binary back
			file = (File)Node.LoadNode(id);

			Assert.IsTrue(file.Binary.IsEmpty);
		}

		[TestMethod()]
		public void Node_Properties_BinaryPropertyDelete()
		{
			File file = new File(this.TestRoot);

			// Save binary
			BinaryData data = new BinaryData();
			data.SetStream(TestTools.GetTestStream());
			data.FileName = ".bin";

			file.Binary = data;
			file.Save();
			int id = file.Id;

			// Load binary back, empty it and save again
			file = (File)Node.LoadNode(id);
			Assert.AreNotEqual(null, file.Binary, "#1");
			file.Binary = null;
			file.Save();

			// Load binary back
			file = (File)Node.LoadNode(id);

			Assert.IsTrue(file.Binary.IsEmpty, "#2");
		}

        [TestMethod]
        public void Node_Properties_BrokenReferenceCache_Bug3816()
        {
            var referencedMemos = new List<Node>();
            for (int i = 0; i < 3; i++)
            {
                var refMemo = Content.CreateNew("Memo", TestRoot, "RefMemo");
                refMemo.Save();
                referencedMemos.Add(refMemo.ContentHandler);
            }
            var referencedMemoPath = referencedMemos[1].Path;
            var memo = Content.CreateNew("Memo", TestRoot, "Memo");
            memo.ContentHandler.SetReferences("SeeAlso", referencedMemos);
            memo.Save();
            var memoPath = memo.Path;
            referencedMemos = null;
            memo = null;

            //----
            var memoNode = Node.LoadNode(memoPath);
            var loadedReferencedMemos1 = memoNode.GetReferences("SeeAlso");
            var paths1 = new List<string>();
            var thrown1 = false;
            try { foreach (var node in loadedReferencedMemos1) paths1.Add(node.Path); }
            catch (Exception e) { thrown1 = true; }

            //----
            Node.ForceDelete(referencedMemoPath);
            memoNode = Node.LoadNode(memoPath);
            var loadedReferencedMemos2 = memoNode.GetReferences("SeeAlso");
            var paths2 = new List<string>();
            var thrown2 = false;
            try { foreach (var node in loadedReferencedMemos2) paths2.Add(node.Path); }
            catch (Exception e) { thrown2 = true; }

            //----
            memoNode.Index++;
            memoNode.Save();
            memoNode = Node.LoadNode(memoPath);
            var loadedReferencedMemos3 = memoNode.GetReferences("SeeAlso");
            var paths3 = new List<string>();
            var thrown3 = false;
            try { foreach (var node in loadedReferencedMemos3) paths3.Add(node.Path); }
            catch (Exception e) { thrown3 = true; }

            Assert.IsFalse(thrown1, "#01");
            Assert.IsFalse(thrown2, "#02");
            Assert.IsFalse(thrown3, "#03");
            Assert.IsTrue(paths1.Count == 3, "#11");
            Assert.IsTrue(paths2.Count == 2, "#12");
            Assert.IsTrue(paths3.Count == 2, "#13");
            Assert.IsTrue(paths1.Contains(referencedMemoPath), "#21");
            Assert.IsFalse(paths2.Contains(referencedMemoPath), "#22");
            Assert.IsFalse(paths3.Contains(referencedMemoPath), "#23");
        }

		#endregion

		#region Version tests

		[TestMethod()]
		public void Node_Version_NextMinor_Test()
		{
			File file = CreateFile("TestNodeVersion");
			File fileOld = CreateFile("TestNodeVersion");

			file.Save(VersionRaising.NextMinor, VersionStatus.Draft);

			File fileNew = Node.LoadNode(file.Id) as File;

			Assert.AreEqual(file.Version, fileNew.Version);
			Assert.IsTrue(fileOld.Version < file.Version);
			Assert.IsTrue(fileOld.Version.Minor < fileNew.Version.Minor);
			Assert.AreEqual(fileOld.Version.Major, file.Version.Major);
		}

		[TestMethod()]
		public void Node_Version_NextMajor_Test()
		{
			File file = CreateFile("TestNodeVersion");
			File fileOld = CreateFile("TestNodeVersion");

			file.Save(VersionRaising.NextMajor, VersionStatus.Approved);

			File fileNew = Node.LoadNode(file.Id) as File;

			Assert.AreEqual(file.Version, fileNew.Version);
			Assert.IsTrue(fileOld.Version < file.Version);
			Assert.IsTrue(fileOld.Version.Major < file.Version.Major);
			Assert.AreEqual(file.Version.Minor, 0);
		}

		[TestMethod()]
		public void Node_Version_Binary_Test()
		{
			File file = CreateFile("TestNodeVersion");
			File fileOld = CreateFile("TestNodeVersion");

			file.Binary = CreateBinary("TestNodeBinary2.bnr", 2);

			file.Save(VersionRaising.NextMinor, VersionStatus.Draft);

			File fileNew = Node.LoadNode(file.Id) as File;

			Assert.IsNotNull(file.Binary);
			Assert.IsNotNull(fileOld.Binary);
			Assert.IsNotNull(fileNew.Binary);
			Assert.AreEqual(file.Binary.Id, fileNew.Binary.Id);
			Assert.IsTrue(fileNew.Binary.Id > fileOld.Binary.Id);
			Assert.AreNotEqual(file.Binary, fileOld.Binary);
		}

		[TestMethod()]
		public void Node_Version_Reference_Test()
		{
			Page page = CreatePage("TestNodeReference7");
			page.PageTemplateNode = CreatePageTemplate("TestPageTemplateReference7");
			page.Save();

			Page pageOld = Node.LoadNode(page.Id) as Page;

			page.Save(VersionRaising.NextMinor, VersionStatus.Draft);

			Page pageNew = Node.LoadNode(page.Id) as Page;

			Assert.AreEqual(pageOld.PageTemplateNode.Id, pageNew.PageTemplateNode.Id);
			Assert.IsTrue(pageOld.Version < pageNew.Version);
		}

		[TestMethod()]
		public void Node_Version_ReferenceChangeNull_Test()
		{
			Page page = CreatePage("TestNodeReference9");
			page.PageTemplateNode = CreatePageTemplate("TestPageTemplateReference9");
			page.Save();

			Page pageOld = Node.LoadNode(page.Id) as Page;

			page.PageTemplateNode = null;

			page.Save(VersionRaising.NextMinor, VersionStatus.Draft);

			Page pageNew = Node.LoadNode(page.Id) as Page;

			Assert.IsNull(pageNew.PageTemplateNode);
			Assert.IsNotNull(pageOld.PageTemplateNode);
		}

		[TestMethod()]
		public void Node_Version_FlatProperties_Test()
		{
			Page page = CreatePage("TestNodeFlatProperty");
			page.PageTemplateNode = CreatePageTemplate("TestNodeFlatPropertyTemplate");
			page.PageNameInMenu = "TNFP";
			page.Save();

			Page pageOld = Node.LoadNode(page.Id) as Page;

			page.PageNameInMenu = "TestNodeFlatProperty";
			page.Save(VersionRaising.NextMinor, VersionStatus.Draft);

			Page pageNew = Node.LoadNode(page.Id) as Page;

			Assert.IsTrue(pageOld.Version < pageNew.Version);
			Assert.AreNotEqual(pageNew.PageNameInMenu, pageOld.PageNameInMenu);
		}

		[TestMethod()]
		public void Node_Version_LoadOldVersion_Test()
		{
			Page page = CreatePage("TestPageVersion");

			page.Save(VersionRaising.NextMinor, VersionStatus.Draft);

			Page pageOld = Node.LoadNode(page.Id, new VersionNumber(page.Version.Major, page.Version.Minor - 1)) as Page;

			Assert.IsNotNull(pageOld);
		}

        [TestMethod()]
        public void Node_Version_LoadOldVersionSaveNextMinor_Test()
        {
            Page page1 = CreatePage("TestPageVersion1");
            page1.Save(VersionRaising.NextMinor, VersionStatus.Draft);
            var page1Version = page1.Version;
            Page page2 = Node.LoadNode(page1.Id, new VersionNumber(page1.Version.Major, page1.Version.Minor - 1)) as Page;
            page2.Save(VersionRaising.NextMinor, VersionStatus.Draft);
            var page2Version = page2.Version;
            //Node.DeletePhysical(page1.Id);
            Assert.IsTrue(page1Version.ToString() == "V1.1.D", String.Concat("page1Version is ", page1Version, ", expected: V1.1.D"));
            Assert.IsTrue(page2Version.ToString() == "V1.2.D", String.Concat("page2Version is ", page2Version, ", expected: V1.2.D"));
        }
        [TestMethod()]
        public void Node_Version_LoadOldVersionSaveNextMajor_Test()
        {
            Page page1 = CreatePage("TestPageVersion2");
            page1.Save(VersionRaising.NextMajor, VersionStatus.Draft);
            var page1Version = page1.Version;
            Page page2 = Node.LoadNode(page1.Id, new VersionNumber(page1.Version.Major - 1, page1.Version.Minor)) as Page;
            page2.Save(VersionRaising.NextMajor, VersionStatus.Draft);
            var page2Version = page2.Version;
            //Node.DeletePhysical(page1.Id);
            Assert.IsTrue(page1Version.ToString() == "V2.0.D", String.Concat("page1Version is ", page1Version, ", expected: V2.0.D"));
            Assert.IsTrue(page2Version.ToString() == "V3.0.D", String.Concat("page2Version is ", page2Version, ", expected: V3.0.D"));
        }

		#region Helper methods

		private File CreateFile(string name)
		{
			return CreateFile(name, 1);
		}

		private File CreateFile(string name, byte byteValue)
		{
			File file = Node.LoadNode(string.Concat(this.TestRoot.Path, "/", name)) as File;
			if (file == null)
			{
				file = new File(this.TestRoot);
				file.Name = name;

				BinaryData binaryData = CreateBinary("TestNodeVersion.bnr", byteValue);

				file.Binary = binaryData;
				file.Save();
			}
			return file;
		}

		private Page CreatePage(string name)
		{
			Page page = Node.LoadNode(string.Concat(this.TestRoot.Path, "/", name)) as Page;
			if (page == null)
			{
				page = new Page(this.TestRoot);
				page.Name = name;
				page.Save();
			}
			return page;
		}

		private PageTemplate CreatePageTemplate(string name)
		{
			PageTemplate pageTemplate = Node.LoadNode(string.Concat(this.TestRoot.Path, "/", name)) as PageTemplate;
			if (pageTemplate == null)
			{
				pageTemplate = new PageTemplate(this.TestRoot);
				pageTemplate.Name = name;
				pageTemplate.Binary = CreateBinaryFromString("TestPageTemplateVersion.bnr", "<html><body></body></html>");
				pageTemplate.Save();
			}
			return pageTemplate;
		}

		private static BinaryData CreateBinary(string name, byte byteValue)
		{
			BinaryData binaryData = new BinaryData();
			binaryData.FileName = name;
			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(100);
			memoryStream.WriteByte(byteValue);
			binaryData.SetStream(memoryStream);
			return binaryData;
		}

		private static BinaryData CreateBinaryFromString(string name, string textData)
		{
			BinaryData binaryData = new BinaryData();
			binaryData.FileName = name;

			System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
			System.IO.StreamWriter writer = new System.IO.StreamWriter(memoryStream, Encoding.Unicode);
			writer.Write(textData);
			writer.Flush();

			binaryData.SetStream(memoryStream);
			return binaryData;
		}

		#endregion

		#endregion

        #region DataModified tests
        [TestMethod]
        public void NodeIsModified_NotModifiedAfterSave()
        {
            var folder = new Folder(TestRoot);
            folder.DisplayName = "OriginalDisplayName";
            folder.Save();
            var changesAfter = folder.Data.GetChangedValues().Count();
            Assert.IsTrue(changesAfter == 0, "#1");
            folder.DisplayName = "NewDisplayName";
            folder.Save();
            changesAfter = folder.Data.GetChangedValues().Count();
            Assert.IsTrue(changesAfter == 0, "#2");
        }
        [TestMethod]
        public void NodeIsModified_NotModifiedAfterRestoreValue()
        {
            var folder = new Folder(TestRoot);
            folder.DisplayName = "OriginalDisplayName";
            folder.Save();
            var id = folder.Id;

            folder = Node.Load<Folder>(id);
            folder.Index += 1;
            var changes0 = folder.Data.GetChangedValues().Count();
            folder.Index -= 1;
            var changes1 = folder.Data.GetChangedValues().Count();
            folder.Save();
            var changes2 = folder.Data.GetChangedValues().Count();

            var origDisplayName = folder.DisplayName;
            folder.DisplayName += "_suffix";
            var changes3 = folder.Data.GetChangedValues().Count();
            folder.DisplayName = origDisplayName;
            var changes4 = folder.Data.GetChangedValues().Count();
            folder.Save();
            var changes5 = folder.Data.GetChangedValues().Count();

            folder.ForceDelete();

            Assert.IsTrue(changes0 > 0, "#0");
            Assert.IsTrue(changes1 == 0, "#1");
            Assert.IsTrue(changes2 == 0, "#2");
            Assert.IsTrue(changes3 > 0, "#3");
            Assert.IsTrue(changes4 == 0, "#4");
            Assert.IsTrue(changes5 == 0, "#5");
        }
        [TestMethod]
        public void NodeIsModified_BinaryData()
        {
            File file = new File(this.TestRoot);
            file.Binary = new BinaryData();
            file.Binary.SetStream(TestTools.GetTestStream());
            file.Save();
            int id = file.Id;

            file = Node.Load<File>(id);
            var origStream = file.Binary.GetStream();
            var binaryData = file.GetBinary("Binary");
            binaryData.SetStream(null);
            var nullBinary = file.GetBinary("Binary");

            var equals = binaryData == nullBinary;
            var changed = file.IsModified;

            Assert.IsTrue(equals, "#1");
            Assert.IsTrue(changed, "#2");

            file.Binary.SetStream(origStream);
            equals = file.GetBinary("Binary") == nullBinary;
            changed = file.IsModified;

            Assert.IsTrue(equals, "#3");
            Assert.IsFalse(changed, "#4");
        }
        [TestMethod]
        public void NodeIsModified_Reference()
        {
            if (ContentType.GetByName("File2") == null)
                ContentTypeInstaller.InstallContentType(@"<?xml version=""1.0"" encoding=""utf-8""?>
					<ContentType name=""File2"" parentType=""File"" handler=""SenseNet.ContentRepository.File"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
						<DisplayName>Folder</DisplayName>
						<Description>Use folders to group information to one place</Description>
						<Icon>Folder</Icon>
						<Fields>
							<Field name=""References"" type=""Reference"" />
						</Fields>
					</ContentType>");

            //int rootId = TestRoot.Id;
            //int file2ctId = ContentType.GetByName("File2").Id;
            var REFS = "References";
            var node1 = Node.LoadNode(1);
            var node2 = Node.LoadNode(2);
            var node3 = Node.LoadNode(3);
            var node4 = Node.LoadNode(4);
            var node5 = Node.LoadNode(5);
            var node6 = Node.LoadNode(6);

            var node = new File(TestRoot, "File2");
            node.SetReferences(REFS, new Node[] { node1, node2 }); // 1, 2
            node.Save();
            node = Node.Load<File>(node.Id);
            bool changed0 = node.IsModified;                       // false

            node.AddReference(REFS, node3);                        // 1, 2, 3
            bool changed1 = node.IsModified;                       // true
            node.RemoveReference(REFS, node3);                     // 1, 2
            bool changed2 = node.IsModified;                       // false
            node.RemoveReference(REFS, node1);                     // 2
            bool changed3 = node.IsModified;                       // true
            node.AddReference(REFS, node1);                        // 2, 1
            bool changed4 = node.IsModified;                       // true
            node.RemoveReference(REFS, node2);                     // 1
            bool changed5 = node.IsModified;                       // true
            node.AddReference(REFS, node2);                        // 1, 2
            bool changed6 = node.IsModified;                       // false
            node.SetReferences(REFS, new Node[] { node1, node2 }); // 1, 2
            bool changed7 = node.IsModified;                       // false

            node.ForceDelete();

            Assert.IsFalse(changed0, "#0");
            Assert.IsTrue(changed1, "#1");
            Assert.IsFalse(changed2, "#2");
            Assert.IsTrue(changed3, "#3");
            Assert.IsTrue(changed4, "#4");
            Assert.IsTrue(changed5, "#5");
            Assert.IsFalse(changed6, "#6");
            Assert.IsFalse(changed7, "#7");
        }
        [TestMethod]
        public void NodeData_NewAndLoadedSharedData()
        {
            var newFile = new File(TestRoot);
            var newFileData = newFile.Data;
            var newFileInnerData = newFile.Data.SharedData;
            Assert.IsFalse(newFileData.IsShared, "New file data is shared");
            Assert.IsNull(newFileInnerData, "New file inner data is not null");

            //----

            newFile.Name = Guid.NewGuid().ToString();
            newFile.Save();
            var id = newFile.Id;

            var savedFileData = newFile.Data;
            var savedFileInnerData = newFile.Data.SharedData;
            Assert.IsTrue(savedFileData.IsShared, "Saved file data is not shared");
            Assert.IsNull(savedFileInnerData, "Saved file inner data is not null");

            //----

            var loadedFile = Node.Load<File>(newFile.Id);

            var loadedFileData = loadedFile.Data;
            var loadedFileInnerData = loadedFile.Data.SharedData;
            Assert.IsTrue(loadedFileData.IsShared, "Loaded file data is not shared");
            Assert.IsNull(loadedFileInnerData, "Loaded file inner data is not null");
            Assert.IsTrue(Object.ReferenceEquals(loadedFileData, savedFileData), "Saved shared and loaded shared are not the same");

            //----

            var reloadedFile = Node.Load<File>(newFile.Id);

            var reloadedFileData = reloadedFile.Data;
            var reloadedFileInnerData = reloadedFile.Data.SharedData;
            Assert.IsTrue(reloadedFileData.IsShared, "Loaded file data is not shared");
            Assert.IsNull(reloadedFileInnerData, "Loaded file inner data is not null");
            Assert.IsTrue(Object.ReferenceEquals(reloadedFileData, loadedFileData), "Reloaded shared and loaded shared are not the same");
            Assert.IsTrue(Object.ReferenceEquals(reloadedFileData, savedFileData), "Reloaded shared and saved shared are not the same");

            //----

            reloadedFile.Description = Guid.NewGuid().ToString();

            var editedFileData = reloadedFile.Data;
            var editedFileInnerData = reloadedFile.Data.SharedData;
            Assert.IsFalse(editedFileData.IsShared, "Edited file data is shared");
            Assert.IsNotNull(editedFileInnerData, "Edited file inner data is null");
            Assert.IsTrue(editedFileInnerData.IsShared, "Edited file inner data is not shared");

            Assert.IsTrue(Object.ReferenceEquals(editedFileInnerData, loadedFileData), "Edited shared and loaded shared are not the same");
            Assert.IsTrue(Object.ReferenceEquals(editedFileInnerData, savedFileData), "Edited shared and saved shared are not the same");

            //----

            reloadedFile.Save();
            var resavedFileData = reloadedFile.Data;
            var resavedFileInnerData = reloadedFile.Data.SharedData;
            Assert.IsTrue(resavedFileData.IsShared, "Resaved file data is not shared");
            Assert.IsNull(resavedFileInnerData, "Resaved file inner data is not null");
            Assert.IsFalse(Object.ReferenceEquals(resavedFileData, loadedFileData), "Resaved shared and loaded shared are the same");

            newFile.ForceDelete();
        }
        #endregion

        [TestMethod]
        public void Node_Load_Bug5527()
        {
            var countForAdmin1 = 0;
            var countForVisitor = 0;
            var countForAdmin2 = 0;
            var nodeId = 0;
            var refAId = 0;
            var refBId = 0;
            try
            {
                ContentTypeInstaller.InstallContentType(
                    @"<ContentType name='Car_Bug5527' parentType='Car' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                    <Fields>
                        <Field name='MyReferenceField' type='Reference'>
                            <DisplayName>My reference field</DisplayName>
                            <Description>Referenced content</Description>
                            <Configuration>
                                <AllowMultiple>true</AllowMultiple>
                            </Configuration>
                        </Field>
                    </Fields>
				</ContentType>"
                );

                var refContentA = Content.CreateNew("Car", TestRoot, "refCarA");
                refContentA.Save();
                refAId = refContentA.Id;
                var refContentB = Content.CreateNew("Car", TestRoot, "refCarB");
                refContentB.Save();
                refBId = refContentB.Id;

                var content = Content.CreateNew("Car_Bug5527", TestRoot, "MyCar");
                var node = content.ContentHandler;
                node.AddReferences("MyReferenceField", new[] { refContentA.ContentHandler, refContentB.ContentHandler });
                node.Save();
                nodeId = node.Id;

                refContentA.Security.SetPermission(User.Visitor, true, PermissionType.See, PermissionValue.Deny);

                //-------------------------------- #1
                node = Node.LoadNode(nodeId);
                countForAdmin1 = node.GetReferences("MyReferenceField").Count();

                //-------------------------------- #2
                User.Current = User.Visitor;
                node = Node.LoadNode(nodeId);
                countForVisitor = 0;
                foreach (var n in node.GetReferences("MyReferenceField"))
                {
                    countForVisitor++;
                }

                //-------------------------------- #3
                User.Current = User.Administrator;
                node = Node.LoadNode(nodeId);
                countForAdmin2 = node.GetReferences("MyReferenceField").Count();
            }
            finally
            {
                User.Current = User.Administrator;
                //ContentTypeInstaller.RemoveContentType("Car_Bug5527");
                TestTools.RemoveNodesAndType("Car_Bug5527");
            }

            //-------------------------------- check
            Assert.IsTrue(countForAdmin1 == 2, String.Concat("countForAdmin1 is", countForAdmin1, ". Expected: 2."));
            Assert.IsTrue(countForVisitor == 1, String.Concat("countForVisitor is", countForVisitor, ". Expected: 1."));
            Assert.IsTrue(countForAdmin2 == 2, String.Concat("countForAdmin2 is", countForAdmin2, ". Expected: 2."));
        }

        //===================================================================================== Tools

		private void CheckRootNode(Node node)
		{
			Assert.IsNotNull(node, "#1: node is null.");
			Assert.AreEqual(node.Id, 2, "#2: node.Id was not the expected value.");
			Assert.IsNull(node.Parent, "#3: node.Parent was not null.");
			Assert.AreEqual(node.NodeType.Id, 4, "#4: node.NodeType.Id was not the expected value.");
			Assert.AreEqual(node.Path, "/Root", "#5: node.Path was not the expected value.");
			Assert.AreEqual(node.Version.Major, 1, "#6: node.Version.Major was not the expected value.");
			Assert.AreEqual(node.Version.Minor, 0, "#7: node.Version.Minor was not the expected value.");
		}
		private static void CheckEveryoneGroupNode(Node node)
		{
			Assert.IsNotNull(node, "#1: node is null.");
			Assert.AreEqual(node.Id, 8, "#2: node.Id was not the expected value.");
			Assert.AreEqual(node.ParentId, 5, "#3: node.ParentId was not the expected value.");
			Assert.AreEqual(node.NodeType.Id, 2, "#4: node.NodeType.Id was not the expected value.");
			Assert.AreEqual(node.Path, "/Root/IMS/BuiltIn/Portal/Everyone", "#5: node.Path was not the expected value.");
			Assert.AreEqual(node.Version.Major, 1, "#6: node.Version.Major was not the expected value.");
			Assert.AreEqual(node.Version.Minor, 0, "#7: node.Version.Minor was not the expected value.");
		}

		private static void InstallTestNodeType()
		{
			if (ActiveSchema.NodeTypes["RepositoryTest_TestNode"] == null)
			{
				ContentTypeInstaller installer =
					ContentTypeInstaller.CreateBatchContentTypeInstaller();
				installer.AddContentType(TestNode.ContentTypeDefinition);
				installer.AddContentType(TestNode2.ContentTypeDefinition);
				installer.ExecuteBatch();
			}
		}

		public static void InstallRefTestNodeType()
		{
			if (ActiveSchema.NodeTypes["RepositoryTest_RefTestNode"] == null)
			{
				ContentTypeInstaller.InstallContentType(RefTestNode.ContentTypeDefinition);
			}
		}

		public RefTestNode CreateRefTestNode(string name)
		{
			RefTestNode rtn = Node.LoadNode(string.Concat(this.TestRoot.Path, "/", name)) as RefTestNode;
            if (rtn != null)
                rtn.ForceDelete();
			rtn = new RefTestNode(this.TestRoot);
			rtn.Name = name;
			rtn.Save();

			return rtn;
		}
	}
}
