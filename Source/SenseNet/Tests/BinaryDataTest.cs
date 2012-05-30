using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using System.IO;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using SenseNet.ContentRepository.Schema;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass()]
    public class BinaryDataTest : TestBase
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

		private static string _testRootName = "_BinaryDataTests";
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
		[ClassInitialize]
		public static void InitializePlayground(TestContext testContext)
		{
			if (ActiveSchema.NodeTypes["RepositoryTest_TestNodeWithBinaryProperty"] == null)
			{
				ContentTypeInstaller installer =
					ContentTypeInstaller.CreateBatchContentTypeInstaller();
				installer.AddContentType(TestNodeWithBinaryProperty.ContentTypeDefinition);
				installer.ExecuteBatch();
			}
		}
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(_testRootPath))
                Node.ForceDelete(_testRootPath);

            if (ActiveSchema.NodeTypes["RepositoryTest_TestNodeWithBinaryProperty"] != null)
                ContentTypeInstaller.RemoveContentType(ContentType.GetByName("RepositoryTest_TestNodeWithBinaryProperty"));
        }

        [TestMethod()]
        public void BinaryData_Constructor_NewUnbound()
        {
            BinaryData target = new BinaryData();
            Assert.IsNotNull(target);
        }

        [TestMethod()]
        public void BinaryData_DefaultValues()
        {
            BinaryData target = new BinaryData();
            Assert.AreEqual(0, target.Id, "SenseNet.ContentRepository.Storage.BinaryData.Id default value is incorrect.");
            Assert.AreEqual(string.Empty, target.ContentType, "SenseNet.ContentRepository.Storage.BinaryData.ContentType default value is incorrect.");
            Assert.AreEqual(string.Empty, target.FileName.Extension, "SenseNet.ContentRepository.Storage.BinaryData.FileName.Extension default value is incorrect.");
            Assert.AreEqual(string.Empty, target.FileName.FileNameWithoutExtension, "SenseNet.ContentRepository.Storage.BinaryData.FileName.FileNameWithoutExtension default value is incorrect.");
            Assert.AreEqual(Convert.ToInt64(-1), target.Size, "SenseNet.ContentRepository.Storage.BinaryData.Size default value is incorrect.");
            Assert.AreEqual(null, target.GetStream(), "SenseNet.ContentRepository.Storage.BinaryData.Stream default value is incorrect.");
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod()]
        public void BinaryData_ContentType_SetNull()
        {
            BinaryData target = new BinaryData();
            target.ContentType = null;
        }

        //[ExpectedException(typeof(ArgumentNullException))]
        //[TestMethod()]
        //public void BinaryData_FileName_SetNull()
        //{
        //    BinaryData target = new BinaryData();
        //    target.FileName = null;
        //}

        [TestMethod()]
        public void BinaryData_EmptyWrite()
        {
            // Save binary
            File file = new File(this.TestRoot);

            BinaryData target = new BinaryData();
            file.Binary = target;

            file.Save();
            int id = file.Id;

            // Load binary back
            file = (File)Node.LoadNode(id);

			Assert.IsTrue(file.GetBinary("Binary").IsEmpty);

			//Assert.AreNotEqual(0, file.Binary.Id);
			//Assert.AreEqual(string.Empty, file.Binary.ContentType);

			//Assert.AreEqual(string.Empty, file.Binary.FileName.FullFileName);
			//Assert.AreEqual((long)-1, file.Binary.Size);
			//Assert.AreEqual(null, file.Binary.GetStream());
        }

        [TestMethod()]
        public void BinaryData_SampleStream()
        {
            File file = new File(this.TestRoot);

            // Save binary
            BinaryData data = new BinaryData();
			data.SetStream(TestTools.GetTestStream());
            data.FileName = "....bin";

            file.Binary = data;
            file.Save();
            int id = file.Id;

            // Load binary back
            file = (File)Node.LoadNode(id);

            Assert.AreNotEqual(0, file.Binary.Id);
            Assert.AreEqual("application/octet-stream", file.Binary.ContentType);
            Assert.AreEqual("....bin", file.Binary.FileName.FullFileName);
			Assert.AreEqual(TestTools.TestStreamLength, file.Binary.Size);
            Assert.AreNotEqual(null, file.Binary.GetStream());
        }

        [TestMethod()]
        public void BinaryData_Rollback()
        {
			string text = "Lorem ipsum...";
			File file = new File(this.TestRoot);
			var bin = new BinaryData();
			bin.FileName = "1.txt";
			bin.SetStream(Tools.GetStreamFromString(text));
			file.Binary = bin;
            file.Save();
			int id = file.Id;

			file = Node.Load<File>(id);

            TransactionScope.Begin();
			try
			{
				// Save binary
				file.Binary = null;
				file.Save();
			}
			finally
			{
				// Rollback
				TransactionScope.Rollback();
			}
			file = Node.Load<File>(id);

			Assert.IsNotNull(file.Binary, "#1");
			Assert.IsTrue(file.Binary.Size > 0, "#2");
			Assert.IsTrue(Tools.GetStreamString(file.Binary.GetStream()) == text, "#3");
        }

        [TestMethod()]
        public void BinaryData_InsertUpdateDelete()
        {
			File file = new File(this.TestRoot);
            file.Save();
            file = Node.Load<File>(file.Id);
            var x = file.Binary.Id;

            // Insert
            file.Binary = new BinaryData();
			file.Binary.SetStream(TestTools.GetTestStream());
            file.Save();
            int id = file.Id;
            int binaryId = file.Binary.Id;
            Assert.AreNotEqual(0, file.Binary.Id);

            // Load back
            file = (File)Node.LoadNode(id);
            Assert.AreEqual(binaryId, file.Binary.Id);
			Assert.AreEqual(TestTools.TestStreamLength, file.Binary.GetStream().Length);
			Assert.AreEqual(TestTools.TestStreamLength, file.Binary.Size);

            // Update
            file.Binary.SetStream (null);
            file.Save();

            // Load back
            file = (File)Node.LoadNode(id);
            Assert.AreEqual(binaryId, file.Binary.Id);
            Assert.AreEqual((long)-1, file.Binary.Size);

            // Delete
            file.Binary = null;
            file.Save();

            // Load back
            file = (File)Node.LoadNode(id);
            Assert.IsTrue(file.Binary.IsEmpty);
        }

        [TestMethod]
        public void BinaryData_RenameFileNode()
        {
			string rootName = "BinaryTestFolder";
			string rootPath = RepositoryPath.Combine(this.TestRoot.Path, rootName);

			if (Node.Exists(rootPath))
                Node.ForceDelete(rootPath);

			Folder folder = new Folder(this.TestRoot);
			folder.Name = rootName;
			folder.Save();

            Stream stream;

            const int TESTBINARY_SIZE = 512 * 1024; // 512k
            byte[] testBinaryArray = new byte[TESTBINARY_SIZE];
            int i;
            for (i=0; i < TESTBINARY_SIZE; i++) testBinaryArray[i] = Convert.ToByte(i % 256);

            BinaryData testBinary = new BinaryData();
            testBinary.FileName = "test.txt";
            testBinary.SetStream(new MemoryStream(testBinaryArray));

            File file = new File(folder);
            file.Name = "OriginalName";
            file.Binary = testBinary;

            file.Save();

            stream = file.Binary.GetStream();

            file = null;

            int readByte;
            i = 0;
            while ((readByte = stream.ReadByte()) != -1)
            {
                Assert.IsTrue(readByte == i % 256);
                i++;
            }
            Assert.IsTrue(i == TESTBINARY_SIZE);

            file = (File)File.LoadNode(RepositoryPath.Combine(rootPath, "OriginalName"));
            file.Name = "NewName";
            file.Save();

            file = (File)File.LoadNode(RepositoryPath.Combine(rootPath, "NewName"));
            stream = file.Binary.GetStream();

            file = null;

            i = 0;
            while ((readByte = stream.ReadByte()) != -1)
            {
                Assert.IsTrue(readByte == i % 256);
                i++;
            }

			if (Node.Exists(rootPath))
                Node.ForceDelete(rootPath);

            Assert.IsTrue(i == TESTBINARY_SIZE);

        }

        [TestMethod]
        public void BinaryData_RenameNodeWithBinaryData()
        {
			string rootName = "BinaryTestFolder";
			string rootPath = RepositoryPath.Combine(this.TestRoot.Path, rootName);

			if (Node.Exists(rootPath))
                Node.ForceDelete(rootPath);

			Folder binaryTestFolder = new Folder(this.TestRoot);
			binaryTestFolder.Name = rootName;
            binaryTestFolder.Save();

            TestNodeWithBinaryProperty tn1 = new TestNodeWithBinaryProperty(binaryTestFolder);

            tn1.Name = "OriginalName";
            tn1.Note = "This is the first test node. Nice, isn't it?";

            BinaryData firstBinary = new BinaryData();
            firstBinary.SetStream(new MemoryStream(new byte[] { 65, 66, 67, 68, 69 }));
            tn1.FirstBinary = firstBinary;

            BinaryData secondBinary = new BinaryData();
            secondBinary.SetStream(new MemoryStream(new byte[] { 97, 98, 99 }));
            tn1.SecondBinary = secondBinary;

            tn1.Save();

			// Drop the in-memory node and load from the Repository
            int testNodeId = tn1.Id;
            tn1 = null;
            TestNodeWithBinaryProperty loadedNode = (TestNodeWithBinaryProperty)Node.LoadNode(testNodeId);

            Stream firstStream = loadedNode.FirstBinary.GetStream();
            Stream secondStream = loadedNode.SecondBinary.GetStream();

            byte[] firstBuffer = new byte[firstStream.Length];
            byte[] secondBuffer = new byte[secondStream.Length];

            firstStream.Read(firstBuffer, 0, (int)firstStream.Length);
            secondStream.Read(secondBuffer, 0, (int)secondStream.Length);

            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(firstBuffer) == "ABCDE", "The first binary had became corrupt after save.");
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(secondBuffer) == "abc", "The second binary had became corrupt after save.");
            
            // Close and dispost streams
            firstStream.Close();
            firstStream.Dispose();
            firstStream = null;
            secondStream.Close();
            secondStream.Dispose();
            secondStream = null;

            // The buffers should still hold  the data
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(firstBuffer) == "ABCDE", "The first binary had became corrupt after save.");
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(secondBuffer) == "abc", "The second binary had became corrupt after save.");

            firstBuffer = null;
            secondBuffer = null;

            // Get the stream again
            firstStream = loadedNode.FirstBinary.GetStream();
            secondStream = loadedNode.SecondBinary.GetStream();

            firstBuffer = new byte[firstStream.Length];
            secondBuffer = new byte[secondStream.Length];

            firstStream.Read(firstBuffer, 0, (int)firstStream.Length);
            secondStream.Read(secondBuffer, 0, (int)secondStream.Length);

            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(firstBuffer) == "ABCDE", "The first binary had became corrupt after save.");
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(secondBuffer) == "abc", "The second binary had became corrupt after save.");

            // Close and dispost streams
            firstStream.Close();
            firstStream.Dispose();
            firstStream = null;
            secondStream.Close();
            secondStream.Dispose();
            secondStream = null;

            // The buffers should still hold  the data
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(firstBuffer) == "ABCDE", "The first binary had became corrupt after save.");
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(secondBuffer) == "abc", "The second binary had became corrupt after save.");

            // Change the name
            loadedNode.Name = "ModifiedName";
            loadedNode.Save();
            loadedNode = null;

            // Load the node again
            loadedNode = (TestNodeWithBinaryProperty)Node.LoadNode(testNodeId);

            Assert.IsTrue(loadedNode.Name == "ModifiedName");

            // Get the stream again
            firstStream = loadedNode.FirstBinary.GetStream();
            secondStream = loadedNode.SecondBinary.GetStream();

            firstBuffer = new byte[firstStream.Length];
            secondBuffer = new byte[secondStream.Length];

            firstStream.Read(firstBuffer, 0, (int)firstStream.Length);
            secondStream.Read(secondBuffer, 0, (int)secondStream.Length);

            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(firstBuffer) == "ABCDE", "The first binary had became corrupt after save.");
            Assert.IsTrue(System.Text.Encoding.ASCII.GetString(secondBuffer) == "abc", "The second binary had became corrupt after save.");


            // Cleanup test nodes
            binaryTestFolder.ForceDelete();
        }

        [TestMethod]
        public void BinaryData_ChecksumAlgorithm()
        {
            var s0 = "qwer";
            var s1 = "qwert";
            var s2 = "Qwert";
            var s3 = "QWERT";
            var h0 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s0));
            var h1 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s1));
            var h2 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s2));
            var h3 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s3));
            Assert.IsTrue(h0 != h1, s0 + " == " + s1);
            Assert.IsTrue(h0 != h2, s0 + " == " + s2);
            Assert.IsTrue(h0 != h3, s0 + " == " + s3);
            Assert.IsTrue(h1 != h2, s1 + " == " + s2);
            Assert.IsTrue(h1 != h3, s1 + " == " + s3);
            Assert.IsTrue(h2 != h3, s2 + " == " + s3);
        }
        [TestMethod]
        public void BinaryData_FileStreamChanged()
        {
            var s0 = "qwer";
            var s1 = "qwert";
            var s2 = "Qwert";
            var s3 = "QWERT";
            var ch0 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s0));
            var ch1 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s1));
            var ch2 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s2));
            var ch3 = BinaryData.CalculateChecksum(Tools.GetStreamFromString(s3));

            var file = new File(TestRoot);
            file.Binary.SetStream(Tools.GetStreamFromString(s0));
            file.Save();
            var fileId = file.Id;
            var ch01 = file.Binary.Checksum;
            var ss01 = Tools.GetStreamString(file.Binary.GetStream());

            file = Node.Load<File>(fileId);
            var ch10 = file.Binary.Checksum;
            var ss10 = Tools.GetStreamString(file.Binary.GetStream());
            file.Binary.SetStream(Tools.GetStreamFromString(s1));
            file.Save();
            var ch11 = file.Binary.Checksum;
            var ss11 = Tools.GetStreamString(file.Binary.GetStream());

            file = Node.Load<File>(fileId);
            var ch20 = file.Binary.Checksum;
            var ss20 = Tools.GetStreamString(file.Binary.GetStream());
            file.Binary.SetStream(Tools.GetStreamFromString(s2));
            file.Save();
            var ch21 = file.Binary.Checksum;
            var ss21 = Tools.GetStreamString(file.Binary.GetStream());

            file = Node.Load<File>(fileId);
            var ch30 = file.Binary.Checksum;
            var ss30 = Tools.GetStreamString(file.Binary.GetStream());
            file.Binary.SetStream(Tools.GetStreamFromString(s3));
            file.Save();
            var ch31 = file.Binary.Checksum;
            var ss31 = Tools.GetStreamString(file.Binary.GetStream());

            Assert.IsTrue(ss01 == s0, "#1");
            Assert.IsTrue(ss10 == s0, "#2");
            Assert.IsTrue(ss11 == s1, "#3");
            Assert.IsTrue(ss20 == s1, "#4");
            Assert.IsTrue(ss21 == s2, "#5");
            Assert.IsTrue(ss30 == s2, "#6");
            Assert.IsTrue(ss31 == s3, "#7");

            Assert.IsTrue(ch01 == ch0, "#11");
            Assert.IsTrue(ch10 == ch0, "#12");
            Assert.IsTrue(ch11 == ch1, "#13");
            Assert.IsTrue(ch20 == ch1, "#14");
            Assert.IsTrue(ch21 == ch2, "#15");
            Assert.IsTrue(ch30 == ch2, "#16");
            Assert.IsTrue(ch31 == ch3, "#17");

        }
    }

}