using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public class TimingTests : TestBase
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


        //========================================================================= Benchmark

        private static long STUTicks = 0;

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            RunBenchmark();
        }
        private static void RunBenchmark()
        {
            // (5 create + 5 save + 500 load) / 15
            try
            {
                CreateStructure(0, 0, 0);
                Stopwatch stopper = Stopwatch.StartNew();
                DoBenchmarkSteps();
                stopper.Stop();

                STUTicks = stopper.ElapsedTicks / 15;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                Node.ForceDelete("/Root/Timing_TestRoot");
            }
        }
        private static void DoBenchmarkSteps()
        {
            var root = Node.Load<Folder>("/Root/Timing_TestRoot/STU");
            int id = 0;

            Node n = null;
            for (int i = 0; i < 5; i++)
            {
                n = new GenericContent(root, "Car");
                n.Save();
            }
            id = n.Id;

            Node node;
            for (var i = 0; i < 5; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    node = Node.LoadNode(id);
                    var sb = new StringBuilder();
                    foreach (var proptype in node.PropertyTypes)
                    {
                        sb.Append(node.GetProperty<object>(proptype));
                    }
                }
                node = Node.LoadNode(id);
                node.Index++;
                node.Save();
            }
        }

        //========================================================================= Timing tests

        //Clone: 0,170204868669885 STU per node (1 STU = 53,8833 ms, measuring: 30 nodes, full time: 275,138 ms)
        //Clone: 0,172244461642104 STU per node (1 STU = 53,8833 ms, measuring: 420 nodes, full time: 3898,1016 ms)
        //Copy: 1,15243684035685 STU per node (1 STU = 53,8833 ms, measuring: 30 nodes, full time: 1862,914 ms)
        //Copy: 0,906249246055828 STU per node (1 STU = 53,8833 ms, measuring: 420 nodes, full time: 20509,3228 ms)

        [TestMethod]
        public void Timing_Clone30()
        {
            string msg = "??";
            try
            {
                int folders = 5;
                int files = 5;
                CreateStructure(folders, files, 1024);
                Stopwatch stopper = Stopwatch.StartNew();
                Node.Copy("/Root/Timing_TestRoot/Source", "/Root/Timing_TestRoot/Target");
                stopper.Stop();

                msg = GetMessage("Clone", folders, files, stopper);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                Node.ForceDelete("/Root/Timing_TestRoot");
            }
            Console.WriteLine(msg);
            Assert.IsTrue(true, msg);
        }
        [TestMethod]
        public void Timing_Clone420()
        {
            string msg = "??";
            try
            {
                int folders = 20;
                int files = 20;
                CreateStructure(folders, files, 1024);
                Stopwatch stopper = Stopwatch.StartNew();
                Node.Copy("/Root/Timing_TestRoot/Source", "/Root/Timing_TestRoot/Target");
                stopper.Stop();

                msg = GetMessage("Clone", folders, files, stopper);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                Node.ForceDelete("/Root/Timing_TestRoot");
            }
            Console.WriteLine(msg);
            Assert.IsTrue(true, msg);
        }
        [TestMethod]
        public void Timing_RealCopy25()
        {
            string msg = "??";
            try
            {
                int folders = 5;
                int files = 5;
                CreateStructure(folders, files, 1024);
                Stopwatch stopper = Stopwatch.StartNew();
                Copy("/Root/Timing_TestRoot/Source", "/Root/Timing_TestRoot/Target");
                stopper.Stop();

                msg = GetMessage("Copy", folders, files, stopper);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                Node.ForceDelete("/Root/Timing_TestRoot");
            }
            Console.WriteLine(msg);
            Assert.IsTrue(true, msg);
        }
        [TestMethod]
        public void Timing_RealCopy420()
        {
            string msg = "??";
            try
            {
                int folders = 20;
                int files = 20;
                CreateStructure(folders, files, 1024);
                Stopwatch stopper = Stopwatch.StartNew();
                Copy("/Root/Timing_TestRoot/Source", "/Root/Timing_TestRoot/Target");
                stopper.Stop();

                msg = GetMessage("Copy", folders, files, stopper);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                Node.ForceDelete("/Root/Timing_TestRoot");
            }
            Console.WriteLine(msg);
            Assert.IsTrue(true, msg);
        }

        private void Copy(string sourcePath, string targetPath)
        {
            Copy(Node.LoadNode(sourcePath), Node.LoadNode(targetPath));
        }
        private void Copy(Node sourceNode, Node targetNode)
        {
            var newTarget = CreateCopy(sourceNode, targetNode);
            var folder = sourceNode as IFolder;
            if(folder == null)
                return;
            foreach (var node in folder.Children)
                Copy(node, newTarget);
        }
        private Node CreateCopy(Node sourceNode, Node targetNode)
        {
            var sourceContent = Content.Create(sourceNode);
            var newContent = Content.CreateNew(sourceNode.NodeType.Name, targetNode, sourceNode.Name);

            newContent.ContentHandler.Index = sourceContent.ContentHandler.Index;

            foreach (var fieldName in sourceContent.Fields.Keys)
                newContent[fieldName] = sourceContent[fieldName];
            newContent.Save();
            return newContent.ContentHandler;
        }

        private static string GetMessage(string category, int folders, int files, Stopwatch stopper)
        {
            var nodeCount = files * folders + folders;

            var normalizedTicks = stopper.ElapsedTicks / nodeCount;
            double stu = Convert.ToDouble(normalizedTicks) / Convert.ToDouble(STUTicks);

            TimeSpan time = TimeSpan.FromTicks(stopper.ElapsedTicks);
            return String.Format("{4}: {1} STU per node (1 STU = {3} ms, measuring: {0} nodes, full time: {2} ms)", nodeCount, stu, time.TotalMilliseconds, TimeSpan.FromTicks(STUTicks).TotalMilliseconds, category);
        }

        //========================================================================= Structure
        
        private static void CreateStructure(int folders, int filesPerFolders, int averageSizeInBytes)
        {
            var node = Node.LoadNode("/Root/Timing_TestRoot");
            if (node != null)
                node.Delete();

            var testRoot = new Folder(Repository.Root);
            testRoot.Name = "Timing_TestRoot";
            testRoot.Save();

            var sctuFolder = new Folder(testRoot);
            sctuFolder.Name = "STU";
            sctuFolder.Save();

            var sourceFolder = new Folder(testRoot);
            sourceFolder.Name = "Source";
            sourceFolder.Save();

            var targetFolder = new Folder(testRoot);
            targetFolder.Name = "Target";
            targetFolder.Save();

            for (var i = 0; i < folders; i++)
            {
                var folder = new Folder(sourceFolder);
                folder.Name = "SubFolder" + i;
                folder.Save();
                CreateManyFiles(folder, filesPerFolders, 20);
            }

            // Timing_TestRoot
            //     SCTU
            //     Target
            //     Source
            //         Subfolder1
            //             [files]
        }
        private static void CreateManyFiles(Folder parent, int count, int averageSizeInBytes)
        {
            for (var i = 0; i < count; i++)
            {
                var file = new File(parent);
                file.Binary.SetStream(GetTestStream(averageSizeInBytes));
                file.Save();
            }
        }
        public static System.IO.Stream GetTestStream(int length)
        {
            var stream = new System.IO.MemoryStream(Convert.ToInt32(length));
            for (int i = 0; i < length; i++)
                stream.WriteByte(byte.MaxValue);
            return stream;
        }
    }
}
