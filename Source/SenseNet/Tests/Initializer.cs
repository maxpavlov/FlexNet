using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using SenseNet.Search.Indexing;
using System.Linq;
using SenseNet.ContentRepository.Schema;
using System.Xml;
using System.Collections.Generic;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass]
	public class Initializer
	{
        [AssemblyInitialize]
        public static void InitializeAllTests(TestContext context)
        {
            var pluginsPath = context.TestDeploymentDir;
            pluginsPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(pluginsPath, @"..\..\..\WebSite\bin"));
            if(!System.IO.Directory.Exists(pluginsPath))
                pluginsPath = System.IO.Directory.GetCurrentDirectory();
            var settings = new RepositoryStartSettings
            {
                PluginsPath = pluginsPath,
                StartLuceneManager = false,
                RestoreIndex = false
            };
            var repo = Repository.Start(settings);

            //try
            //{
            //    pluginsPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(pluginsPath, @"..\..\..\ContentExplorer\bin"));
            //    SenseNet.ContentRepository.Storage.TypeHandler.LoadAssembliesFrom(pluginsPath);
            //}
            //catch (System.IO.DirectoryNotFoundException)
            //{
            //    pluginsPath = System.IO.Directory.GetCurrentDirectory();
            //    SenseNet.ContentRepository.Storage.TypeHandler.LoadAssembliesFrom(pluginsPath);
            //}

            var populator = new SenseNet.Search.Indexing.DocumentPopulator();
            populator.ClearAndPopulateAll();
            repo.StartLucene();

            //PatchSystemFolder_All(typeof(SystemFolder).Name, "TestSurvey", "BoolTest", "Car_Bug5527", "Car1_AutoNamingTests", "Car2_AutoNamingTests", "File2", "ForMultiPagingSearch", "RepositoryTest_RefTestNode");
            PatchSystemFolder_All("PortalRoot", "RepositoryTest_RefTestNode", "Automobile", "Automobile5", "Automobile6", "DefaultValueTest"
                , "FieldSetting_Analyzer", "OuterFieldTestContentType", "ReferredContent", "TypeForIndexingTest", "ValidatedContent"
                , "XmlNamespaceCompatibility", "ContentList_for_AllowChildTypes");

            PatchSystemFolder_Add("Site", "WebContent", "File");
            PatchSystemFolder_Add("Car", "Car");
            PatchSystemFolder_Add("ContentLink", "Car", "Page");
            PatchSystemFolder_Add("ContentList", "Car", "Folder", "OuterFieldTestContentType", "Task");
            PatchSystemFolder_Add("Page", "Page", "WebContent", "File");
            PatchSystemFolder_Add("Site", "ContentList");
            PatchSystemFolder_Add("TrashBag", "Folder");
        }
        private static void PatchSystemFolder_All(string typeName, params string[] additionalContentTypeNames)
        {
            var ct = ContentType.GetByName(typeName);
            var xml = new XmlDocument();
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("x", ContentType.ContentDefinitionXmlNamespace);
            xml.Load(ct.Binary.GetStream());
            var element = (XmlElement)xml.DocumentElement.SelectSingleNode("x:AllowedChildTypes", nsmgr);
            if (element == null)
            {
                var fieldsElement = (XmlElement)xml.DocumentElement.SelectSingleNode("x:Fields", nsmgr);
                element = xml.CreateElement("", "AllowedChildTypes", ContentType.ContentDefinitionXmlNamespace);
                xml.DocumentElement.InsertBefore(element, fieldsElement);
            }

            var list = ContentTypeManager.Current.ContentTypes.Values.Select(x => x.Name).Except(new[] { "PortalRoot", "JournalNode" }).ToList();
            if( additionalContentTypeNames != null && additionalContentTypeNames.Length > 0)
                list.AddRange(additionalContentTypeNames);
            element.InnerText = string.Join(" ", list);

            ContentTypeInstaller.InstallContentType(xml.OuterXml);
        }
        private static void PatchSystemFolder_Add(string typeName, params string[] additionalContentTypeNames)
        {
            var ct = ContentType.GetByName(typeName);
            var xml = new XmlDocument();
            var nsmgr = new XmlNamespaceManager(xml.NameTable);
            nsmgr.AddNamespace("x", ContentType.ContentDefinitionXmlNamespace);
            xml.Load(ct.Binary.GetStream());
            var list = new List<string>();
            var element = (XmlElement)xml.DocumentElement.SelectSingleNode("x:AllowedChildTypes", nsmgr);
            if (element == null)
            {
                var fieldsElement = (XmlElement)xml.DocumentElement.SelectSingleNode("x:Fields", nsmgr);
                //if (fieldsElement == null)
                //{
                //}
                element = xml.CreateElement("", "AllowedChildTypes", ContentType.ContentDefinitionXmlNamespace);
                xml.DocumentElement.InsertBefore(element, fieldsElement);
            }
            else
            {
                list.AddRange(element.InnerXml.Split(" \t\r\n,;".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries).Select(x=>x.Trim()));
            }

            if (additionalContentTypeNames != null && additionalContentTypeNames.Length > 0)
                list.AddRange(additionalContentTypeNames);
            element.InnerText = string.Join(" ", list);

            ContentTypeInstaller.InstallContentType(xml.OuterXml);
        }

        [AssemblyCleanup]
        public static void FinishAllTests()
        {
            var diff = IntegrityChecker.Check();
            Trace.WriteLine("&> Index integrity check after all tests. Count of differences: " + diff.Count());
            foreach (var d in diff)
                Trace.WriteLine("&> " + d);

            Repository.Shutdown();
        }
	}
}