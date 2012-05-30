using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.UI.Controls;
using System.Web.UI;
using SenseNet.Search;

namespace SenseNet.ContentRepository.Tests.Search
{
    [TestClass()]
    public class SenseNetDataSourceTest : TestBase
    {
        private TestContext testContextInstance;
        public override TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Playground

        private static string _testRootName = "Folder_SNDataSourceTest";
        private static string _testRootPath = String.Concat("/Root/", _testRootName);
        private static Node _testRoot;
        public static Node TestRoot
        {
            get
            {
                if (_testRoot == null)
                {
                    _testRoot = Node.LoadNode(_testRootPath);
                    if (_testRoot == null)
                    {
                        var node = NodeType.CreateInstance("Folder", Node.LoadNode("/Root"));
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
            var testRoot = Node.LoadNode(_testRootPath);
            if (testRoot != null)
                testRoot.ForceDelete();
        }

        #endregion

        [TestMethod]
        public void SnDataSource_Execute_WithSystemFiles()
        {
            DestroyPlayground();

            TestEquipment.EnsureNode(_testRootPath + "/SystemFolder1");
            TestEquipment.EnsureNode(_testRootPath + "/Folder1");

            //create a datasource to query simple content
            var snds = new SenseNetDataSource { Query = "InTree:" + _testRootPath };
            var results = snds.Select(DataSourceSelectArguments.Empty);

            //expected: 2 (test root and the folder)
            Assert.AreEqual(2, results.Count());

            //include system files and folders too (switch off AutoFilters)
            snds = new SenseNetDataSource
                       {
                           Query = "InTree:" + _testRootPath,
                           Settings = new QuerySettings { EnableAutofilters = false }
                       };

            results = snds.Select(DataSourceSelectArguments.Empty);

            //expected: 3 (test root and both normal and system folder)
            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public void SnDataSource_Execute_Top1()
        {
            DestroyPlayground();

            TestEquipment.EnsureNode(_testRootPath + "/Folder1");
            TestEquipment.EnsureNode(_testRootPath + "/Folder2");
            TestEquipment.EnsureNode(_testRootPath + "/Folder3");
            TestEquipment.EnsureNode(_testRootPath + "/Folder4");

            //create a datasource to query simple content
            var snds = new SenseNetDataSource {ContentPath = _testRootPath, Top = 2};
            var results = snds.Select(DataSourceSelectArguments.Empty);

            //expected: Top 2 folders
            Assert.AreEqual(2, results.Count());
        }

        [TestMethod]
        public void SnDataSource_Execute_Flatten1()
        {
            DestroyPlayground();

            TestEquipment.EnsureNode(_testRootPath + "/Folder1");
            TestEquipment.EnsureNode(_testRootPath + "/Folder1/Folder2");

            //create a datasource to query simple content
            var snds = new SenseNetDataSource { ContentPath = _testRootPath, FlattenResults = true };
            var results = snds.Select(DataSourceSelectArguments.Empty);

            //expected: Top 2 folders
            Assert.AreEqual(2, results.Count());
        }
    }
}
