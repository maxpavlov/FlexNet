using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Schema;
using System.Collections.ObjectModel;
using System.Xml.XPath;
using System.IO;
using System.Xml;
using System.Diagnostics;
using SenseNet.ContentRepository.Tests.ContentHandlers;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public class RuntimeContentTest : TestBase
    {
        #region test infrastructure
        private TestContext testContextInstance;

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
        #endregion

        #region Playground
        private static string _testRootName = "_RuntimeContentTest";
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
        }
        #endregion

        [TestMethod]
        public void Node_IsDescendantOf()
        {
            bool isDescendant;
            int distance;
            Assert.IsTrue(Repository.Root.NodeLevel() == 0, "#1");
            Assert.IsTrue(Repository.SystemFolder.NodeLevel() == 1, "#2");
            Assert.IsTrue(Repository.SchemaFolder.NodeLevel() == 2, "#3");

            isDescendant = Repository.Root.IsDescendantOf(Repository.Root);
            Assert.IsFalse(isDescendant, "#4");

            isDescendant = Repository.SystemFolder.IsDescendantOf(Repository.Root);
            Assert.IsTrue(isDescendant, "#5");

            isDescendant = Repository.SystemFolder.IsDescendantOf(Repository.Root, out distance);
            Assert.IsTrue(isDescendant, "#6");
            Assert.IsTrue(distance == 1, "#7");

            isDescendant = Repository.SchemaFolder.IsDescendantOf(Repository.Root, out distance);
            Assert.IsTrue(isDescendant, "#8");
            Assert.IsTrue(distance == 2, "#9");

            Assert.IsFalse(Repository.Root.IsDescendantOf(Repository.SchemaFolder), "#10");
            Assert.IsFalse(Repository.Root.IsDescendantOf(Repository.SchemaFolder, out distance), "#11");

        }

        [TestMethod]
        public void FieldTypeInference()
        {
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(bool)) == typeof(SenseNet.ContentRepository.Fields.BooleanField), "bool -> BooleanField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(int)) == typeof(SenseNet.ContentRepository.Fields.IntegerField), "int -> IntegerField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(string)) == typeof(SenseNet.ContentRepository.Fields.ShortTextField), "string -> ShortTextField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(DateTime)) == typeof(SenseNet.ContentRepository.Fields.DateTimeField), "DateTime -> DateTimeField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(decimal)) == typeof(SenseNet.ContentRepository.Fields.NumberField), "decimal -> NumberField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(double)) == typeof(SenseNet.ContentRepository.Fields.NumberField), "double -> NumberField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(VersionNumber)) == typeof(SenseNet.ContentRepository.Fields.VersionField), "VersionNumber -> VersionField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(BinaryData)) == typeof(SenseNet.ContentRepository.Fields.BinaryField), "BinaryData -> BinaryField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(Node)) == typeof(SenseNet.ContentRepository.Fields.ReferenceField), "Node -> ReferenceField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(User)) == typeof(SenseNet.ContentRepository.Fields.ReferenceField), "User -> ReferenceField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(IEnumerable<Node>)) == typeof(SenseNet.ContentRepository.Fields.ReferenceField), "IEnumerable<Node> -> ReferenceField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(IEnumerable<User>)) == typeof(SenseNet.ContentRepository.Fields.ReferenceField), "IEnumerable<User> -> ReferenceField");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(Field)) == null, "Field -> null");
            Assert.IsTrue(DynamicContentTools.GetSuggestedFieldType(typeof(IEnumerable<int>)) == null, "IEnumerable<int> -> null");
        }

        [TestMethod]
        public void RuntimeContent_1()
        {
            var ctd = @"<ContentType name=""RuntimeNode"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
	<DisplayName>RuntimeNode</DisplayName>
	<Description>Use RuntimeNodes to handle an object.</Description>
	<Icon>Folder</Icon>
    <Fields>
        <Field name=""name"" type=""ShortText"">
            <DisplayName>Object Name</DisplayName>
        </Field>
        <Field name=""counter"" type=""Integer"">
            <DisplayName>Counter</DisplayName>
        </Field>
    </Fields>
</ContentType>
";

            var NAME = "name";
            var COUNTER = "counter";

            var initialName = "MyObjectInstance";
            var initialCounter = 123;
            var newName = "New name";
            var newCounter = 987;

            var objectToEdit = new Class1() { name = initialName, counter = initialCounter };
            var content = Content.Create(objectToEdit, ctd);

            Assert.IsTrue(initialName == (string)content[NAME], "#1");
            Assert.IsTrue(initialCounter == (int)content[COUNTER], "#2");

            content[NAME] = newName;
            content[COUNTER] = newCounter;

            Assert.IsTrue(initialName == (string)content.Fields[NAME].OriginalValue, "#3");
            Assert.IsTrue(initialCounter == (int)content.Fields[COUNTER].OriginalValue, "#4");
            Assert.IsTrue(newName == (string)content[NAME], "#5");
            Assert.IsTrue(newCounter == (int)content[COUNTER], "#6");

            content.Save();

            Assert.IsTrue(newName == (string)content.Fields[NAME].OriginalValue, "#7");
            Assert.IsTrue(newCounter == (int)content.Fields[COUNTER].OriginalValue, "#8");
            Assert.IsTrue(newName == (string)content[NAME], "#9");
            Assert.IsTrue(newCounter == (int)content[COUNTER], "#10");
            Assert.IsTrue(newName == objectToEdit.name, "#11");
            Assert.IsTrue(newCounter == objectToEdit.counter, "#12");
        }
        [TestMethod]
        public void RuntimeContent_2()
        {
            var ctd = @"<ContentType name=""RuntimeNode"" parentType=""GenericContent"" handler=""SenseNet.ContentRepository.GenericContent"" xmlns=""http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition"">
	<DisplayName>RuntimeNode</DisplayName>
	<Description>Use RuntimeNodes to handle an object.</Description>
	<Icon>Folder</Icon>
    <Fields>
        <Field name=""name"" type=""ShortText"">
            <DisplayName>Object Name</DisplayName>
        </Field>
        <Field name=""counter"" type=""Integer"">
            <DisplayName>Counter</DisplayName>
        </Field>
    </Fields>
</ContentType>
";
            var folderContent = Content.CreateNew(typeof(RuntimeContentContainer).Name, TestRoot, null);
            folderContent.Save();
            var folder = (RuntimeContentContainer)folderContent.ContentHandler;

            var NAME = "name";
            var COUNTER = "counter";
            var nodes = new Node[3];
            var objectToEdit = new Class1[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                var initialName = "MyObjectInstance"+i;
                var initialCounter = 10000+i;
                var newName = "New name "+i;
                var newCounter = 20000+i;

                objectToEdit[i] = new Class1() { name = initialName, counter = initialCounter };
                var content = Content.Create(objectToEdit[i], ctd);

                nodes[i] = content.ContentHandler;
            }
            folder.SetChildren(nodes);

            var j = 0;
            foreach (var node in folder.Children)
            {
                var content = Content.Create(node);
                var initialName = "MyObjectInstance" + j;
                var initialCounter = 10000 + j;
                var newName = "New name " + j;
                var newCounter = 20000 + j;

                Assert.IsTrue(initialName == (string)content[NAME], "#1");
                Assert.IsTrue(initialCounter == (int)content[COUNTER], "#2");

                content[NAME] = newName;
                content[COUNTER] = newCounter;

                Assert.IsTrue(initialName == (string)content.Fields[NAME].OriginalValue, "#3");
                Assert.IsTrue(initialCounter == (int)content.Fields[COUNTER].OriginalValue, "#4");
                Assert.IsTrue(newName == (string)content[NAME], "#5");
                Assert.IsTrue(newCounter == (int)content[COUNTER], "#6");

                content.Save();

                Assert.IsTrue(newName == (string)content.Fields[NAME].OriginalValue, "#7");
                Assert.IsTrue(newCounter == (int)content.Fields[COUNTER].OriginalValue, "#8");
                Assert.IsTrue(newName == (string)content[NAME], "#9");
                Assert.IsTrue(newCounter == (int)content[COUNTER], "#10");
                Assert.IsTrue(newName == objectToEdit[j].name, "#11");
                Assert.IsTrue(newCounter == objectToEdit[j].counter, "#12");

                j++;
            }
        }
        public class Class1
        {
            string _name;
            public string name
            {
                get { return _name; }
                set { _name = value; }
            }

            int _counter;
            public int counter
            {
                get { return _counter; }
                set { _counter = value; }
            }
        }

        [TestMethod]
        public void SupportDynamicProperties_1()
        {
            ContentTypeInstaller.InstallContentType(@"<ContentType name='Class2' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'><Fields /></ContentType>");

            //-- Checks the contettype and indexing subsystem usability. Do not remove!
            var x = SenseNet.Search.ContentQuery.Query("Name:*admin* .SORT:Id .TOP:100");

            string contentString1before = null;
            int contentInteger1before = 0;
            string contentString2before = null;
            int contentInteger2before = 0;
            DataType contentEnumBefore = DataType.Binary;
            string handlerString1after = null;
            int handlerInteger1after = 0;
            DataType handlerEnumAfter = DataType.Binary;
            string handlerString2after = null;
            int handlerInteger2after = 0;
            try
            {
                var handler = new Class2(TestRoot);
                handler.DynamicString = "Startup value";
                handler.DynamicInteger = 12;
                handler.SetProperty("VeryDynamicString", "Initial");
                handler.SetProperty("VeryDynamicInteger", 34);
                handler.RepoDataType = DataType.Int;

                var content = Content.Create(handler);

                contentString1before = (string)content["DynamicString"];
                contentInteger1before = (int)content["DynamicInteger"];
                contentEnumBefore = (DataType)Enum.Parse(typeof(DataType), ((List<string>)content["RepoDataType"])[0]);
                contentString2before = (string)content["VeryDynamicString"];
                contentInteger2before = (int)content["VeryDynamicInteger"];

                content["DynamicString"] = "New value";
                content["DynamicInteger"] = 98;
                content["RepoDataType"] = new List<string>(new string[] { ((int)DataType.DateTime).ToString() });
                content["VeryDynamicString"] = "Modified";
                content["VeryDynamicInteger"] = 76;
                content.Save();

                handlerString1after = handler.DynamicString;
                handlerInteger1after = handler.DynamicInteger;
                handlerEnumAfter = handler.RepoDataType;
                handlerString2after = (string)handler.PropertyBag["VeryDynamicString"];
                handlerInteger2after = (int)handler.PropertyBag["VeryDynamicInteger"];
                
            }
            finally
            {
                ContentTypeInstaller.RemoveContentType("Class2");
            }
            Assert.IsTrue(contentString1before == "Startup value", "#1");
            Assert.IsTrue(contentInteger1before == 12, "#2");
            Assert.IsTrue(contentEnumBefore == DataType.Int, "#3");
            Assert.IsTrue(contentString2before == "Initial", "#4");
            Assert.IsTrue(contentInteger2before == 34, "#5");
            Assert.IsTrue(handlerString1after == "New value", "#6");
            Assert.IsTrue(handlerInteger1after == 98, "#7");
            Assert.IsTrue(handlerEnumAfter == DataType.DateTime, "#8");
            Assert.IsTrue(handlerString2after == "Modified", "#9");
            Assert.IsTrue(handlerInteger2after == 76, "#10");
        }
        [TestMethod]
        public void SupportDynamicProperties_2()
        {
            ContentTypeInstaller.InstallContentType(@"<ContentType name='Class2' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'><Fields /></ContentType>");
            try
            {
                var handler = new Class2(TestRoot);
                Content content;

                handler.IsStartupMode = false;
                handler.IsRuntimeMode = false;
                content = Content.Create(handler);
                Assert.IsFalse(content.Fields.ContainsKey("RuntimeProperty"), "#1");
                Assert.IsFalse(content.Fields.ContainsKey("StartupProperty"), "#2");

                handler.IsStartupMode = false;
                handler.IsRuntimeMode = true;
                content = Content.Create(handler);
                Assert.IsFalse(content.Fields.ContainsKey("StartupProperty"), "#3");
                Assert.IsTrue(content.Fields.ContainsKey("RuntimeProperty"), "#4");

                handler.IsStartupMode = true;
                handler.IsRuntimeMode = false;
                content = Content.Create(handler);
                Assert.IsTrue(content.Fields.ContainsKey("StartupProperty"), "#5");
                Assert.IsFalse(content.Fields.ContainsKey("RuntimeProperty"), "#6");

                handler.IsStartupMode = true;
                handler.IsRuntimeMode = true;
                content = Content.Create(handler);
                Assert.IsTrue(content.Fields.ContainsKey("StartupProperty"), "#7");
                Assert.IsTrue(content.Fields.ContainsKey("RuntimeProperty"), "#8");

            }
            finally
            {
                ContentTypeInstaller.RemoveContentType("Class2");
            }
        }
        public class Class2 : GenericContent, ISupportsDynamicFields
        {
            private static IDictionary<string, FieldMetadata> _dynamicFieldsBase;
            private Dictionary<string, object> _propertyBag = new Dictionary<string, object>
            {
                {"VeryDynamicString", null},
                {"VeryDynamicInteger", 0},
            };

            static Class2()
            {
                _dynamicFieldsBase = new Dictionary<string, FieldMetadata>
                {
                    {"DynamicString", new FieldMetadata
                        {
                            FieldName = "DynamicString",
                            PropertyType = typeof(string),
                            FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(string)),
                            DisplayName = "Dynamic string",
                            CanRead = true,
                            CanWrite = true
                        }
                    }, 
                    {"DynamicInteger", new FieldMetadata
                        {
                            FieldName = "DynamicInteger",
                            PropertyType = typeof(int),
                            FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(int)),
                            DisplayName = "Dynamic integer",
                            CanRead = true,
                            CanWrite = true
                        }
                    }, 
                    {"RepoDataType", new FieldMetadata
                        {
                            FieldName = "RepoDataType",
                            PropertyType = typeof(DataType),
                            FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(DataType)),
                            DisplayName = "Repository data type",
                            CanRead = true,
                            CanWrite = true
                        }
                    }, 
                    {"VeryDynamicString", new FieldMetadata
                        {
                            FieldName = "VeryDynamicString",
                            PropertyType = typeof(string),
                            FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(string)),
                            DisplayName = "Very dynamic string",
                            CanRead = true,
                            CanWrite = true
                        }
                    }, 
                    {"VeryDynamicInteger", new FieldMetadata
                        {
                            FieldName = "VeryDynamicInteger",
                            PropertyType = typeof(int),
                            FieldType = DynamicContentTools.GetSuggestedFieldType(typeof(int)),
                            DisplayName = "Very dynamic integer",
                            CanRead = true,
                            CanWrite = true
                        }
                    }
                };
            }

            public IDictionary<string, FieldMetadata> GetDynamicFields()
            {
                var data = new Dictionary<string, FieldMetadata>(_dynamicFieldsBase);
                var meta = DynamicContentTools.GetFieldMetadata(typeof(Class3), IsHiddenProperty);
                foreach (var key in meta.Keys)
                    data.Add(key, meta[key]);
                return data;
            }
            private bool IsHiddenProperty(FieldMetadata fieldMeta)
            {
                if (fieldMeta.PropertyInfo.Name == "StartupProperty")
                    return IsStartupMode;
                if (fieldMeta.PropertyInfo.Name == "RuntimeProperty")
                    return IsRuntimeMode;
                return true;
            }

            public bool IsStartupMode { get; set; }
            public bool IsRuntimeMode { get; set; }

            public Class2(Node parent) : this(parent, "Class2") { }
            public Class2(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
            protected Class2(NodeToken nt) : base(nt) { }

            public string DynamicString { get; set; }
            public int DynamicInteger { get; set; }
            public DataType RepoDataType { get; set; }
            public Dictionary<string, object> PropertyBag { get { return _propertyBag; } }

            public override void Save()
            {
            }
            public override void Save(SavingMode mode)
            {
            }

            public override object GetProperty(string name)
            {
                switch (name)
                {
                    case "DynamicString": return DynamicString;
                    case "DynamicInteger": return DynamicInteger;
                    case "RepoDataType": return RepoDataType;
                    default:
                        if (PropertyBag.ContainsKey(name))
                            return PropertyBag[name];
                        return base.GetProperty(name);
                }
            }
            public override void SetProperty(string name, object value)
            {
                switch (name)
                {
                    case "DynamicString": DynamicString = (string)value; return;
                    case "DynamicInteger": DynamicInteger = (int)value; return;
                    case "RepoDataType": RepoDataType = (DataType)value; return;
                    default:
                        if (PropertyBag.ContainsKey(name))
                            PropertyBag[name] = value;
                        else
                            base.SetProperty(name, value);
                        return;
                }
            }
            IDictionary<string, FieldMetadata> ISupportsDynamicFields.GetDynamicFieldMetadata()
            {
                return GetDynamicFields();
            }
            object ISupportsDynamicFields.GetProperty(string name)
            {
                return this.GetProperty(name);
            }
            void ISupportsDynamicFields.SetProperty(string name, object value)
            {
                this.SetProperty(name, value);
            }
            bool ISupportsDynamicFields.IsNewContent
            {
                get { return true; }
            }
        }
        public class Class3
        {
            internal string PrivateProperty { get; set; }
            public string RuntimeProperty { get; set; }
            public string StartupProperty { get; set; }
            public string VisibleProperty { get; set; }
            public DateTime ReadonlyDateTimeProperty { get { return DateTime.Now; } }
        }
    }
}
