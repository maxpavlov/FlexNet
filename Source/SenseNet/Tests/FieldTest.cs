using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Tests.ContentHandlers;
using SenseNet.Portal.UI.Controls;
using SnControls = SenseNet.Portal.UI.Controls;
using System.Text;
using System.Reflection;

namespace SenseNet.ContentRepository.Tests
{
    [TestClass]
    public class FieldTest : TestBase
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

        #region Accessors
        private class ContentAccessor : Accessor
        {
            public Content TargetContent
            {
                get { return (Content)_target; }
            }
            public ContentAccessor(Content target) : base(target) { }
            public void SaveFields()
            {
                base.CallPrivateMethod("SaveFields");
            }
        }
        #endregion

        #region TestRoot - ClassInitialize - ClassCleanup
        private static string _testRootName = "_RepositoryTest_FieldTest";
        private static string __testRootPath = String.Concat("/Root/", _testRootName);
        private Folder __testRoot;
        private Folder _testRoot
        {
            get
            {
                if (__testRoot == null)
                {
                    __testRoot = (Folder)Node.LoadNode(__testRootPath);
                    if (__testRoot == null)
                    {
                        Folder folder = new Folder(Repository.Root);
                        folder.Name = _testRootName;
                        folder.Save();
                        __testRoot = (Folder)Node.LoadNode(__testRootPath);
                    }
                }
                return __testRoot;
            }
        }

        [ClassInitialize]
        public static void InstallContentTypes(TestContext testContext)
        {
            ContentTypeInstaller.InstallContentType(_genericCtd, _handlerCtd, _defaultValueCtd);
        }
        [ClassCleanup]
        public static void RemoveContentTypes()
        {
            if (Node.Exists(__testRootPath))
                Node.ForceDelete(__testRootPath);
            ContentType ct;
            ct = ContentType.GetByName("FieldOnGenericTest");
            if (ct != null)
                ct.Delete();
            ct = ContentType.GetByName("FieldOnHandlerTest");
            if (ct != null)
                ct.Delete();
            ct = ContentType.GetByName("OuterFieldTestContentType");
            if (ct != null)
                ct.Delete();
        }
        #endregion

        #region Source codes
        private static string _genericCtd = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='FieldOnGenericTest' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<DisplayName>FieldTest</DisplayName>
	<Description>Test type for ContentHandler and Field connection tests</Description>
	<Fields>
		<Field name='ShortText' type='ShortText'></Field>
		<Field name='LongText' type='LongText'></Field>
		<Field name='Boolean' type='Boolean'></Field>
		<Field name='Number' type='Number'></Field>
		<Field name='WhoAndWhen' type='WhoAndWhen'>
			<Bind property='Who' />
			<Bind property='When' />
		</Field>
		<Field name='DateTime' type='DateTime'></Field>

        <Field name='Id' type='Integer'>
          <DisplayName>Id</DisplayName>
          <Description>A unique ID for the Content</Description>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='ParentId' type='Integer'>
          <DisplayName>Id</DisplayName>
          <Description>A unique ID for the Content</Description>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='VersionId' type='Integer'>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='Type' type='NodeType'>
          <DisplayName>NodeType</DisplayName>
          <Description>The type of the Node in the Repository</Description>
          <Bind property='NodeType' />
          <Indexing>
            <IndexHandler>SenseNet.Search.Indexing.ExclusiveTypeIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='TypeIs' type='NodeType'>
          <DisplayName>NodeType</DisplayName>
          <Description>The type tree of the Node in the Repository</Description>
          <Bind property='NodeType' />
          <Indexing>
            <IndexHandler>SenseNet.Search.Indexing.TypeTreeIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='Name' type='ShortText'>
          <DisplayName>Uri name</DisplayName>
          <Indexing>
            <Store>Yes</Store>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
          </Indexing>
          <Configuration>
            <Compulsory>true</Compulsory>
          </Configuration>
        </Field>
        <Field name='CreatedById' type='Integer'>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='ModifiedById' type='Integer'>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        
        <Field name='Path' type='ShortText'>
          <DisplayName>Path</DisplayName>
          <Description>Path in repository</Description>
          <Indexing>
            <Store>Yes</Store>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='InTree' type='ShortText'>
          <Bind property='Path'/>
          <Indexing>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
            <IndexHandler>SenseNet.Search.Indexing.InTreeIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='InFolder' type='ShortText'>
          <Bind property='Path'/>
          <Indexing>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
            <IndexHandler>SenseNet.Search.Indexing.InFolderIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>

	</Fields>
</ContentType>";

        private static string _handlerCtd = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='FieldOnHandlerTest' parentType='GenericContent' handler='SenseNet.ContentRepository.Tests.ContentHandlers.FieldTestHandler' 
		xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'
		xmlns:q='http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression'>
	<DisplayName>FieldTest</DisplayName>
	<Description>Test type for ContentHandler and Field connection tests</Description>
	<Fields>
		<Field name='ShortText'  type='ShortText'></Field>
		<Field name='LongText'   type='LongText'></Field>
		<Field name='Boolean'    type='Boolean'></Field>
		<Field name='WhoAndWhen' type='WhoAndWhen'>
			<Bind property='Who' />
			<Bind property='When' />
		</Field>
		<Field name='Number'     type='Number'></Field>
		<Field name='Byte'       type='Integer'></Field>
		<Field name='Int16'      type='Integer'></Field>
		<Field name='Int32'      type='Integer'></Field>
		<Field name='Int64'      type='Number'></Field>
		<Field name='Single'     type='Number'></Field>
		<Field name='Double'     type='Number'></Field>
		<Field name='Decimal'    type='Number'></Field>
		<Field name='SByte'      type='Integer'></Field>
		<Field name='UInt16'     type='Integer'></Field>
		<Field name='UInt32'     type='Number'></Field>
		<Field name='UInt64'     type='Number'></Field>

		<Field name='UserReference' type='Reference'>
			<Configuration>
				<AllowedTypes>
					<Type>User</Type>
				</AllowedTypes>
			</Configuration>
		</Field>
		<Field name='UsersReference' type='Reference'>
			<Configuration>
				<AllowMultiple>true</AllowMultiple>
				<AllowedTypes>
					<Type>User</Type>
				</AllowedTypes>
			</Configuration>
		</Field>
		<Field name='GeneralReference' type='Reference'>
			<Configuration>
				<AllowMultiple>true</AllowMultiple>
				<Compulsory>false</Compulsory>
			</Configuration>
		</Field>

		<Field name='SingleNotNull' type='Reference'>
			<Bind property='GeneralReference' /><Configuration><AllowMultiple>false</AllowMultiple><Compulsory>true</Compulsory></Configuration>
		</Field>
		<Field name='SingleNull' type='Reference'>
			<Bind property='GeneralReference' /><Configuration><AllowMultiple>false</AllowMultiple><Compulsory>false</Compulsory></Configuration>
		</Field>
		<Field name='MultipleNotNull' type='Reference'>
			<Bind property='GeneralReference' /><Configuration><AllowMultiple>true</AllowMultiple><Compulsory>true</Compulsory></Configuration>
		</Field>
		<Field name='MultipleNull' type='Reference'>
			<Bind property='GeneralReference' /><Configuration><AllowMultiple>true</AllowMultiple><Compulsory>false</Compulsory></Configuration>
		</Field>
		<Field name='AllowedTypes' type='Reference'>
			<Bind property='GeneralReference' />
			<Configuration>
				<AllowedTypes>
					<Type>User</Type>
					<Type>Folder</Type>
				</AllowedTypes>
			</Configuration>
		</Field>
		<Field name='SelectionRoot' type='Reference'>
			<Bind property='GeneralReference' />
			<Configuration>
				<SelectionRoot>
					<Path>.</Path>
					<Path>/Root/System/Schema/ContentTypes/GenericContent</Path>
					<Path>/Root/System/Schema/ContentViews</Path>
				</SelectionRoot>
			</Configuration>
		</Field>
		<Field name='Query' type='Reference'>
			<Bind property='GeneralReference' />
			<Configuration>
				<AllowMultiple>true</AllowMultiple>
				<Query>
					<q:And>
						<q:String op='StartsWith' property='Path'>/Root/System/Schema/ContentTypes/GenericContent</q:String>
					</q:And>
				</Query>
			</Configuration>
		</Field>
		<Field name='HyperLink' type='HyperLink'></Field>
		<Field name='VersionNumber' type='Version'></Field>
		<Field name='DateTime' type='DateTime'></Field>

        <Field name='Id' type='Integer'>
          <DisplayName>Id</DisplayName>
          <Description>A unique ID for the Content</Description>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='ParentId' type='Integer'>
          <DisplayName>Id</DisplayName>
          <Description>A unique ID for the Content</Description>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='VersionId' type='Integer'>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='Type' type='NodeType'>
          <DisplayName>NodeType</DisplayName>
          <Description>The type of the Node in the Repository</Description>
          <Bind property='NodeType' />
          <Indexing>
            <IndexHandler>SenseNet.Search.Indexing.ExclusiveTypeIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='TypeIs' type='NodeType'>
          <DisplayName>NodeType</DisplayName>
          <Description>The type tree of the Node in the Repository</Description>
          <Bind property='NodeType' />
          <Indexing>
            <IndexHandler>SenseNet.Search.Indexing.TypeTreeIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='Name' type='ShortText'>
          <DisplayName>Uri name</DisplayName>
          <Indexing>
            <Store>Yes</Store>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
          </Indexing>
          <Configuration>
            <Compulsory>true</Compulsory>
          </Configuration>
        </Field>
        <Field name='CreatedById' type='Integer'>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='ModifiedById' type='Integer'>
          <Indexing>
            <Store>Yes</Store>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        
        <Field name='Path' type='ShortText'>
          <DisplayName>Path</DisplayName>
          <Description>Path in repository</Description>
          <Indexing>
            <Store>Yes</Store>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='InTree' type='ShortText'>
          <Bind property='Path'/>
          <Indexing>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
            <IndexHandler>SenseNet.Search.Indexing.InTreeIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
        <Field name='InFolder' type='ShortText'>
          <Bind property='Path'/>
          <Indexing>
            <Analyzer>Lucene.Net.Analysis.KeywordAnalyzer</Analyzer>
            <IndexHandler>SenseNet.Search.Indexing.InFolderIndexHandler</IndexHandler>
          </Indexing>
          <Configuration>
            <Visible>false</Visible>
            <ReadOnly>true</ReadOnly>
          </Configuration>
        </Field>
	</Fields>
</ContentType>";


        private static string _defaultValueCtd = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='DefaultValueTest' parentType='FieldOnHandlerTest' handler='SenseNet.ContentRepository.Tests.ContentHandlers.DefaultValueTest' 
        xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'
        xmlns:q='http://schemas.sensenet.com/SenseNet/ContentRepository/SearchExpression'>
    <DisplayName>FieldTest</DisplayName>
    <Description>Test type for ContentHandler and Field connection tests</Description>
    <Fields>
        <Field name='ShortText' type='ShortText'>
            <Configuration>
                <Compulsory>true</Compulsory>
                <DefaultValue>Forty four</DefaultValue>
            </Configuration>
        </Field>
		<Field name='Int32' type='Integer'>
            <Configuration>
                <Compulsory>true</Compulsory>
                <DefaultValue>42</DefaultValue>
            </Configuration>
        </Field>
		<Field name='GeneralReference' type='Reference'>
			<Configuration>
                <Compulsory>true</Compulsory>
				<AllowMultiple>true</AllowMultiple>
				<Compulsory>false</Compulsory>
                <DefaultValue>/Root,/Root/IMS</DefaultValue>
			</Configuration>
		</Field>
    </Fields>
</ContentType>";
        #endregion


        [TestMethod]
        public void GenericControlByField()
        {
            PrivateType t = new PrivateType(typeof(GenericFieldControl));
            object obj = t.GetStaticProperty("ControlTable");
            Dictionary<string, Type> qwer = (Dictionary<string, Type>)obj;

            string message;
            Assert.IsTrue(CheckTypes(typeof(ApprovingModeField), typeof(SnControls.DropDown), out message), message);
            Assert.IsTrue(CheckTypes(typeof(BinaryField), typeof(SnControls.Binary), out message), message);
            Assert.IsTrue(CheckTypes(typeof(BooleanField), typeof(SnControls.Boolean), out message), message);
            Assert.IsTrue(CheckTypes(typeof(ChoiceField), typeof(SnControls.DropDown), out message), message);
            Assert.IsTrue(CheckTypes(typeof(ColorField), typeof(SnControls.ShortText), out message), message);
            Assert.IsTrue(CheckTypes(typeof(DateTimeField), typeof(SnControls.DatePicker), out message), message);
            Assert.IsTrue(CheckTypes(typeof(HyperLinkField), typeof(SnControls.HyperLink), out message), message);
            Assert.IsTrue(CheckTypes(typeof(InheritableApprovingModeField), typeof(SnControls.DropDown), out message), message);
            Assert.IsTrue(CheckTypes(typeof(InheritableVersioningModeField), typeof(SnControls.DropDown), out message), message);
            Assert.IsTrue(CheckTypes(typeof(IntegerField), typeof(SnControls.WholeNumber), out message), message);
            Assert.IsTrue(CheckTypes(typeof(LockField), typeof(SnControls.ShortText), out message), message);
            Assert.IsTrue(CheckTypes(typeof(LongTextField), typeof(SnControls.LongText), out message), message);
            Assert.IsTrue(CheckTypes(typeof(NodeTypeField), typeof(SnControls.ShortText), out message), message);
            Assert.IsTrue(CheckTypes(typeof(NumberField), typeof(SnControls.Number), out message), message);
            Assert.IsTrue(CheckTypes(typeof(PasswordField), typeof(SnControls.Password), out message), message);
            Assert.IsTrue(CheckTypes(typeof(ReferenceField), typeof(SnControls.ReferenceGrid), out message), message);
            Assert.IsTrue(CheckTypes(typeof(SecurityField), typeof(SnControls.ShortText), out message), message);
            Assert.IsTrue(CheckTypes(typeof(ShortTextField), typeof(SnControls.ShortText), out message), message);
            //Assert.IsTrue(CheckTypes(typeof(SiteListField), typeof(SnControls.SiteList), out message), message);
            Assert.IsTrue(CheckTypes(typeof(SiteRelativeUrlField), typeof(SnControls.SiteRelativeUrl), out message), message);
            Assert.IsTrue(CheckTypes(typeof(UrlListField), typeof(SnControls.UrlList), out message), message);
            Assert.IsTrue(CheckTypes(typeof(VersioningModeField), typeof(SnControls.DropDown), out message), message);
            Assert.IsTrue(CheckTypes(typeof(WhoAndWhenField), typeof(SnControls.WhoAndWhen), out message), message);
        }
        private bool CheckTypes(Type fieldType, Type expectedType, out string message)
        {
            message = "";
            Type resultType = GetDefaultControl(fieldType).GetType();
            if (resultType == expectedType)
                return true;
            message = String.Concat(fieldType.Name, ": expected ", expectedType.Name, " but ", resultType.Name);
            return false;
        }
        private FieldControl GetDefaultControl(Type fieldType)
        {
            //-- ...hogy ez mekkora hack :)
            object[] attrs1 = fieldType.GetCustomAttributes(typeof(ShortNameAttribute), false);
            string ftname = fieldType.Name;
            string shortName = attrs1.Length == 0 ?
                ftname.ToLower().EndsWith("Field") && ftname.Length > 5 ? ftname.Substring(0, ftname.Length - 5) : ftname
                : ((ShortNameAttribute)attrs1[0]).ShortName;

            XmlDocument xd = new XmlDocument();
            if (fieldType == typeof(WhoAndWhenField))
                xd.LoadXml(String.Concat("<Field name='TestField' type='", shortName, "'><Bind property='a' /><Bind property='b' /></Field>"));
            else
                xd.LoadXml(String.Concat("<Field name='TestField' type='", shortName, "' />"));
            xd.DocumentElement.CreateNavigator();

            FieldDescriptor fieldDesc = FieldDescriptor.Parse(xd.DocumentElement.CreateNavigator(), null, ContentType.GetByName("GenericContent"));
            FieldSetting setting = FieldSetting.Create(fieldDesc);
            Content content = Content.CreateNew("GenericContent", Repository.Root, "sdf");
            Field field = Field.Create(content, setting);

            FieldControl fieldControl;
            fieldControl = GenericFieldControl.CreateDefaultFieldControl(field);

            return fieldControl;
        }

        [TestMethod]
        public void XssProtection()
        {
            string data, dataDefault, dataRaw, dataText, dataHtml;

            var hackValue = "aa <b>bb</b><script>alert('XSS alert')</script>";

            var content = Content.CreateNew("Car", _testRoot, null);
            content["Make"] = hackValue;
            content["Description"] = hackValue;

            var makeControl = new ShortText();
            makeControl.SetData(hackValue);
            makeControl.Field = content.Fields["Make"];
            //data = makeControl.GetData().ToString();
            data = makeControl.Data.ToString();
            dataDefault = makeControl.GetOutputData(OutputMethod.Default);
            dataRaw = makeControl.GetOutputData(OutputMethod.Raw);
            dataText = makeControl.GetOutputData(OutputMethod.Text);
            dataHtml = makeControl.GetOutputData(OutputMethod.Html);
            Assert.IsTrue(data == dataDefault, "Shorttext's raw output is not raw");
            Assert.IsTrue(dataDefault == dataText, "Shorttext's default output is not TEXT");

            var descControl = new LongText();
            descControl.Field = content.Fields["Description"];
            descControl.SetData(hackValue);
            //data = descControl.GetData().ToString();
            data = descControl.Data.ToString();
            dataDefault = descControl.GetOutputData(OutputMethod.Default);
            dataRaw = descControl.GetOutputData(OutputMethod.Raw);
            dataText = descControl.GetOutputData(OutputMethod.Text);
            dataHtml = descControl.GetOutputData(OutputMethod.Html);
            Assert.IsTrue(data == dataDefault, "LongText's raw output is not raw");
            Assert.IsTrue(dataDefault == dataText, "LongText's default output is not TEXT");

            var richControl = new RichText();
            richControl.Field = content.Fields["Description"];
            richControl.SetData(hackValue);
            //data = richControl.GetData().ToString();
            data = richControl.Data.ToString();
            dataDefault = richControl.GetOutputData(OutputMethod.Default);
            dataRaw = richControl.GetOutputData(OutputMethod.Raw);
            dataText = richControl.GetOutputData(OutputMethod.Text);
            dataHtml = richControl.GetOutputData(OutputMethod.Html);
            Assert.IsTrue(data == dataDefault, "RichText's raw output is not raw");
            Assert.IsTrue(dataDefault == dataHtml, "RichText's default output is not HTML");
        }


        ////ne torold ki, amig nem tudod, hogy hova tesszuk
        //[TestMethod]
        //public void FieldFactory()
        //{
        //    XmlDocument xd = new XmlDocument();
        //    xd.LoadXml("<Field name='TestShortText' type='ShortText' />");
        //    xd.DocumentElement.CreateNavigator();
        //    FieldDescriptor fieldDesc = FieldDescriptor.Parse(xd.DocumentElement.CreateNavigator(), null, ContentType.GetByName("GenericContent"));
        //    FieldSetting setting = FieldSetting.Create(fieldDesc);
        //    Content content = Content.CreateNew("GenericContent", Portal1.Root, "sdf");

        //    FieldManager1 fieldManager1 = new FieldManager1();
        //    FieldManager2 fieldManager2 = new FieldManager2();
        //    int count = 1000000;
        //    Field field;
        //    Stopwatch stopper;

        //    //----

        //    stopper = Stopwatch.StartNew();
        //    for (int i = 0; i < count; i++)
        //        field = Field.Create(content, setting);
        //    //field = fieldPrototypes["SenseNet.ContentRepository.Fields.ShortTextField"].CreateInstance();
        //    stopper.Stop();
        //    TimeSpan time1 = TimeSpan.FromTicks(stopper.ElapsedTicks);

        //    //----

        //    stopper = Stopwatch.StartNew();
        //    for (int i = 0; i < count; i++)
        //        field = fieldManager1.CreateField("SenseNet.ContentRepository.Fields.ShortTextField");
        //    stopper.Stop();
        //    TimeSpan time2 = TimeSpan.FromTicks(stopper.ElapsedTicks);

        //    //----

        //    stopper = Stopwatch.StartNew();
        //    for (int i = 0; i < count; i++)
        //        field = fieldManager2.CreateField("SenseNet.ContentRepository.Fields.ShortTextField");
        //    stopper.Stop();
        //    TimeSpan time3 = TimeSpan.FromTicks(stopper.ElapsedTicks);

        //    Trace.WriteLine("Field.Create average milliseconds: " + time1.TotalMilliseconds);
        //    Trace.WriteLine("FieldManager1.Create average milliseconds: " + time2.TotalMilliseconds);
        //    Trace.WriteLine("FieldManager2.Create average milliseconds: " + time3.TotalMilliseconds);

        //    Assert.Inconclusive();
        //}

        [TestMethod]
        public void Field_DefaultValue()
        {
            var ch = new DefaultValueTest(_testRoot);
            ch.Save();
            var id = ch.Id;
            var paths = new[]{Repository.Root.Path, Repository.ImsFolder.Path};

            ch = Node.Load<DefaultValueTest>(id);

            Assert.IsTrue(ch.ShortText == "Forty four", "#1");
            Assert.IsTrue(ch.Int32 == 42, "#2");
            Assert.IsTrue(ch.GeneralReference.Count() == 2, "#3");
            Assert.IsTrue(paths.Contains(ch.GeneralReference.First().Path), "#4");
            Assert.IsTrue(paths.Contains(ch.GeneralReference.Last().Path), "#5");
        }

        [TestMethod]
        public void Field_NamingConvention_Correct()
        {
            //-- Sikeres, ha nem dob hibat
            string ctd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='FieldNamingTest' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields><Field name='ShortText' type='ShortText' /></Fields></ContentType>";

            string ltd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
					<Fields><ContentListField name='#ContentListField1' type='ShortText' /></Fields></ContentListDefinition>";

            if (ContentTypeManager.Current.GetContentTypeByName("FieldNamingTest") != null)
                ContentTypeInstaller.RemoveContentType("FieldNamingTest");
            if (Node.Exists("/Root/ContentList1"))
                Node.ForceDelete("/Root/ContentList1");

            ContentTypeInstaller.InstallContentType(ctd);

            ContentList list = new ContentList(Repository.Root);
            list.Name = "ContentList1";
            list.ContentListDefinition = ltd;
            list.Save();

            if (ContentTypeManager.Current.GetContentTypeByName("FieldNamingTest") != null)
                ContentTypeInstaller.RemoveContentType("FieldNamingTest");
            if (Node.Exists("/Root/ContentList1"))
                Node.ForceDelete("/Root/ContentList1");
        }
        [TestMethod]
        [ExpectedException(typeof(ContentRegistrationException))]
        public void Field_NamingConvention_BadContentField()
        {
            //-- Sikeres, ha hibat dob
            string ctd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentType name='FieldNamingTest' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields><Field name='#ShortText' type='ShortText' /></Fields></ContentType>";

            if (ContentTypeManager.Current.GetContentTypeByName("FieldNamingTest") != null)
                ContentTypeInstaller.RemoveContentType("FieldNamingTest");

            ContentTypeInstaller.InstallContentType(ctd);

            if (ContentTypeManager.Current.GetContentTypeByName("FieldNamingTest") != null)
                ContentTypeInstaller.RemoveContentType("FieldNamingTest");
        }
        [TestMethod]
        [ExpectedException(typeof(ContentRegistrationException))]
        public void Field_NamingConvention_BadContentListField()
        {
            //-- Sikeres, ha hibat dob

            string ltd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentListDefinition name='FieldNamingTest' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
					<Fields><ContentListField name='ContentListField1' type='ShortText' /></Fields></ContentListDefinition>";

            if (Node.Exists("/Root/ContentList1"))
                Node.ForceDelete("/Root/ContentList1");

            ContentList list = new ContentList(Repository.Root);
            list.Name = "ContentList1";
            list.ContentListDefinition = ltd;
            list.Save();

            if (Node.Exists("/Root/ContentList1"))
                Node.ForceDelete("/Root/ContentList1");

        }

        [TestMethod]
        public void FieldOnGeneric_ShortText()
        {
            ShortTextTest("FieldOnGenericTest");
        }
        [TestMethod]
        public void FieldOnGeneric_LongText()
        {
            LongTextTest("FieldOnGenericTest");
        }
        [TestMethod]
        public void FieldOnGeneric_Boolean()
        {
            BooleanTest("FieldOnGenericTest");
        }
        [TestMethod]
        public void FieldOnGeneric_WhoAndWhen()
        {
            WhoAndWhenTest("FieldOnGenericTest");
        }
        [TestMethod]
        public void FieldOnGeneric_Number()
        {
            NumberTest("FieldOnGenericTest");
        }
        [TestMethod]
        public void FieldOnGeneric_Version()
        {
            VersionTest("FieldOnHandlerTest");
        }
        [TestMethod]
        public void FieldOnGeneric_DateTime()
        {
            DatetimeTest("FieldOnGenericTest");
        }

        public void FieldOnGeneric_Reference() { }
        public void FieldOnGeneric_HyperLink() { }
        public void FieldOnGeneric_Binary() { }
        public void FieldOnGeneric_Choice() { }
        public void FieldOnGeneric_VersioningMode() { }
        public void FieldOnGeneric_UrlList() { }

        public void FieldOnGeneric_Integer() { }
        public void FieldOnGeneric_Color() { }
        public void FieldOnGeneric_NodeType() { }
        public void FieldOnGeneric_Security() { }
        public void FieldOnGeneric_Lock() { }
        public void FieldOnGeneric_SiteRelativeUrl() { }

        [TestMethod]
        public void FieldOnHandler_ShortText()
        {
            ContentAccessor contentAcc = ShortTextTest("FieldOnHandlerTest");
            Content content = contentAcc.TargetContent;

            string fieldName = "ShortText";
            string testValue = "ChangedOnHandler";

            ((FieldTestHandler)content.ContentHandler).ShortText = testValue;
            Assert.IsTrue((string)content.Fields[fieldName].OriginalValue == testValue, "#14");
        }
        [TestMethod]
        public void FieldOnHandler_LongText()
        {
            ContentAccessor contentAcc = LongTextTest("FieldOnHandlerTest");
            Content content = contentAcc.TargetContent;

            string fieldName = "LongText";
            string testValue = "ChangedOnHandler";

            ((FieldTestHandler)content.ContentHandler).LongText = testValue;
            Assert.IsTrue((string)content.Fields[fieldName].OriginalValue == testValue, "#14");
        }
        [TestMethod]
        public void FieldOnHandler_Boolean()
        {
            ContentAccessor contentAcc = BooleanTest("FieldOnHandlerTest");
            Content content = contentAcc.TargetContent;

            string fieldName = "Boolean";
            bool testValue = true;

            ((FieldTestHandler)content.ContentHandler).Boolean = testValue;
            Assert.IsTrue((bool)content.Fields[fieldName].OriginalValue == testValue, "#14");

            ((FieldTestHandler)content.ContentHandler).Boolean = !testValue;
            Assert.IsTrue((bool)content.Fields[fieldName].OriginalValue == !testValue, "#16");
        }
        [TestMethod]
        public void FieldOnHandler_WhoAndWhen()
        {
            WhoAndWhenTest("FieldOnHandlerTest");
        }
        [TestMethod]
        public void FieldOnHandler_Number()
        {
            NumberTest("FieldOnHandlerTest");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnByte()
        {
            int originalValue;
            int currentValue;
            byte handlerValue;
            string fieldName = "Byte";
            int defaultValue = 0;
            int testValue = 123;
            int testIntValue = 123;
            byte defaultHandlerValue = 0;
            byte testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.Byte;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.Byte;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.Byte;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnInt16()
        {
            int originalValue;
            int currentValue;
            Int16 handlerValue;
            string fieldName = "Int16";
            int defaultValue = 0;
            int testValue = 123;
            int testIntValue = 123;
            Int16 defaultHandlerValue = 0;
            Int16 testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.Int16;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.Int16;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.Int16;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnInt32()
        {
            int originalValue;
            int currentValue;
            Int32 handlerValue;
            string fieldName = "Int32";
            int defaultValue = 0;
            int testValue = 123;
            int testIntValue = 123;
            Int32 defaultHandlerValue = 0;
            Int32 testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.Int32;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.Int32;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.Int32;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnInt64()
        {
            decimal originalValue;
            decimal currentValue;
            Int64 handlerValue;
            string fieldName = "Int64";
            decimal defaultValue = 0;
            decimal testValue = 123.456m;
            decimal testIntValue = 123m;
            Int64 defaultHandlerValue = 0;
            Int64 testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.Int64;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.Int64;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.Int64;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnSingle()
        {
            decimal originalValue;
            decimal currentValue;
            Single handlerValue;
            string fieldName = "Single";
            decimal defaultValue = 0;
            decimal testValue = 123.456m;
            Single defaultHandlerValue = 0;
            Single testHandlerValue = 123.456f;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.Single;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, "#6");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.Single;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, "#9");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.Single;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnDouble()
        {
            decimal originalValue;
            decimal currentValue;
            Double handlerValue;
            string fieldName = "Double";
            decimal defaultValue = 0;
            decimal testValue = 123.456m;
            Double defaultHandlerValue = 0;
            Double testHandlerValue = 123.456d;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.Double;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, "#6");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.Double;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, "#9");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.Double;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnSByte()
        {
            int originalValue;
            int currentValue;
            SByte handlerValue;
            string fieldName = "SByte";
            int defaultValue = 0;
            int testValue = 123;
            int testIntValue = 123;
            SByte defaultHandlerValue = 0;
            SByte testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.SByte;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.SByte;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.SByte;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnUInt16()
        {
            int originalValue;
            int currentValue;
            UInt16 handlerValue;
            string fieldName = "UInt16";
            int defaultValue = 0;
            int testValue = 123;
            int testIntValue = 123;
            UInt16 defaultHandlerValue = 0;
            UInt16 testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.UInt16;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.UInt16;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (int)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (int)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.UInt16;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnUInt32()
        {
            decimal originalValue;
            decimal currentValue;
            UInt32 handlerValue;
            string fieldName = "UInt32";
            decimal defaultValue = 0;
            decimal testValue = 123.456m;
            decimal testIntValue = 123m;
            Int32 defaultHandlerValue = 0;
            Int32 testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.UInt32;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.UInt32;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.UInt32;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }
        [TestMethod]
        public void FieldOnHandler_NumberOnUInt64()
        {
            decimal originalValue;
            decimal currentValue;
            UInt64 handlerValue;
            string fieldName = "UInt64";
            decimal defaultValue = 0;
            decimal testValue = 123.456m;
            decimal testIntValue = 123m;
            UInt64 defaultHandlerValue = 0;
            UInt64 testHandlerValue = 123;

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;

            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#1");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#2");
            handlerValue = handler.UInt64;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#3");

            content[fieldName] = testValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#4");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#5");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#6");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, "#7");
            handlerValue = handler.UInt64;
            Assert.IsTrue(handlerValue == testHandlerValue, "#8");

            content[fieldName] = defaultValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testIntValue, "#9");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#10");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, "#11");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, "#12");
            handlerValue = handler.UInt64;
            Assert.IsTrue(handlerValue == defaultHandlerValue, "#13");
        }

        [TestMethod]
        public void FieldOnHandler_UserReference()
        {
            string fieldName = "UserReference";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            Assert.IsNull(handler.UserReference, "#1");
            content[fieldName] = User.Administrator;
            contentAcc.SaveFields();
            Assert.IsTrue(handler.UserReference.Id == User.Administrator.Id, "#2");
        }
        [TestMethod]
        public void FieldOnHandler_UsersReference()
        {
            string fieldName = "UsersReference";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];
            var userList = handler.UsersReference.ToList();

            Assert.IsTrue(userList.Count == 0, "#1");
            var users = new User[] { User.Administrator, User.Visitor };
            content[fieldName] = users;
            contentAcc.SaveFields();
            userList = handler.UsersReference.ToList();

            Assert.IsTrue(userList.Count == 2, "#2");
            Assert.IsTrue(userList[0].Id == User.Administrator.Id, "#3");
            Assert.IsTrue(userList[1].Id == User.Visitor.Id, "#4");
        }
        [TestMethod]
        public void FieldOnHandler_GeneralReference()
        {
            string fieldName = "GeneralReference";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];
            var nodeList = handler.GeneralReference.ToList();

            Assert.IsTrue(nodeList.Count == 0, "#1");

            var nodes = new List<Node>(new Node[] { User.Administrator });
            content[fieldName] = nodes;
            nodes.Add(Repository.Root);
            content[fieldName] = nodes;
            contentAcc.SaveFields();
            nodes.Add(ContentTypeManager.Current.GetContentTypeByName("GenericContent"));

            var values = handler.GeneralReference.ToList();

            Assert.IsTrue(values.Count == 2, "#2");
            Assert.IsTrue(values[0].Id == User.Administrator.Id, "#3");
            Assert.IsTrue(values[1].Id == Repository.Root.Id, "#4");
        }
        [TestMethod]
        public void FieldOnHandler_Ref_SingleNotNull()
        {
            //Assert.Inconclusive("Missing validation message");

            string fieldName = "SingleNotNull";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            Node node0 = User.Administrator;
            Node node1 = Repository.Root;
            Node node2 = ContentTypeManager.Current.GetContentTypeByName("GenericContent");
            bool[] flags = new bool[12];

            var refs = new List<Node>();
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: empty
            flags[0] = !field.IsValid;
            flags[1] = !content.IsValid;
            flags[2] = handler.GeneralReference.ToList().Count == 0;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0
            flags[3] = field.IsValid;
            flags[4] = content.IsValid;
            flags[5] = handler.GeneralReference.ToList().Count == 1;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1
            flags[6] = !field.IsValid;
            flags[7] = !content.IsValid;
            flags[8] = handler.GeneralReference.ToList().Count == 0;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            refs.Add(node2);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1, node2
            flags[9] = !field.IsValid;
            flags[10] = !content.IsValid;
            flags[11] = handler.GeneralReference.ToList().Count == 0;

            string error = "";
            for (int i = 0; i < flags.Length; i++)
                if (!flags[i])
                    error = String.Concat(error, "#", i, " ");

            Assert.IsTrue(error.Length == 0, "False flags: " + error);

            //SingleNull
            //MultipleNotNull
            //MultipleNull

        }
        [TestMethod]
        public void FieldOnHandler_Ref_SingleNull()
        {
            //Assert.Inconclusive("Missing validation message");

            string fieldName = "SingleNull";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            Node node0 = User.Administrator;
            Node node1 = Repository.Root;
            Node node2 = ContentTypeManager.Current.GetContentTypeByName("GenericContent");
            bool[] flags = new bool[12];

            var refs = new List<Node>();
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: empty
            flags[0] = field.IsValid;
            flags[1] = content.IsValid;
            flags[2] = handler.GeneralReference.ToList().Count == 0;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0
            flags[3] = field.IsValid;
            flags[4] = content.IsValid;
            flags[5] = handler.GeneralReference.ToList().Count == 1;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1
            flags[6] = !field.IsValid;
            flags[7] = !content.IsValid;
            flags[8] = handler.GeneralReference.ToList().Count == 0;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            refs.Add(node2);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1, node2
            flags[9] = !field.IsValid;
            flags[10] = !content.IsValid;
            flags[11] = handler.GeneralReference.ToList().Count == 0;

            string error = "";
            for (int i = 0; i < flags.Length; i++)
                if (!flags[i])
                    error = String.Concat(error, "#", i, " ");

            Assert.IsTrue(error.Length == 0, "False flags: " + error);

        }
        [TestMethod]
        public void FieldOnHandler_Ref_MultipleNotNull()
        {
            //Assert.Inconclusive("Missing validation message");

            string fieldName = "MultipleNotNull";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            Node node0 = User.Administrator;
            Node node1 = Repository.Root;
            Node node2 = ContentTypeManager.Current.GetContentTypeByName("GenericContent");
            bool[] flags = new bool[12];

            var refs = new List<Node>();
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: empty
            flags[0] = !field.IsValid;
            flags[1] = !content.IsValid;
            flags[2] = handler.GeneralReference.ToList().Count == 0;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0
            flags[3] = field.IsValid;
            flags[4] = content.IsValid;
            flags[5] = handler.GeneralReference.ToList().Count == 1;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1
            flags[6] = field.IsValid;
            flags[7] = content.IsValid;
            flags[8] = handler.GeneralReference.ToList().Count == 2;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            refs.Add(node2);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1, node2
            flags[9] = field.IsValid;
            flags[10] = content.IsValid;
            flags[11] = handler.GeneralReference.ToList().Count == 3;

            string error = "";
            for (int i = 0; i < flags.Length; i++)
                if (!flags[i])
                    error = String.Concat(error, "#", i, " ");

            Assert.IsTrue(error.Length == 0, "False flags: " + error);
        }
        [TestMethod]
        public void FieldOnHandler_Ref_MultipleNull()
        {
            string fieldName = "MultipleNull";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            Node node0 = User.Administrator;
            Node node1 = Repository.Root;
            Node node2 = ContentTypeManager.Current.GetContentTypeByName("GenericContent");
            bool[] flags = new bool[12];

            var refs = new List<Node>();
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: empty
            flags[0] = field.IsValid;
            flags[1] = content.IsValid;
            flags[2] = handler.GeneralReference.ToList().Count == 0;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0
            flags[3] = field.IsValid;
            flags[4] = content.IsValid;
            flags[5] = handler.GeneralReference.ToList().Count == 1;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1
            flags[6] = field.IsValid;
            flags[7] = content.IsValid;
            flags[8] = handler.GeneralReference.ToList().Count == 2;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(node0);
            refs.Add(node1);
            refs.Add(node2);
            field.SetData(refs);
            contentAcc.SaveFields();	//-- content: node0, node1, node2
            flags[9] = field.IsValid;
            flags[10] = content.IsValid;
            flags[11] = handler.GeneralReference.ToList().Count == 3;

            string error = "";
            for (int i = 0; i < flags.Length; i++)
                if (!flags[i])
                    error = String.Concat(error, "#", i, " ");

            Assert.IsTrue(error.Length == 0, "False flags: " + error);
        }
        [TestMethod]
        public void FieldOnHandler_Ref_AllowedTypes()
        {
            string fieldName = "AllowedTypes";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            List<Node> refs;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(User.Administrator);
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsTrue(field.IsValid, "#1");

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(Repository.Root);
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsTrue(field.IsValid, "#2");

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(ContentTypeManager.Current.GetContentTypeByName("GenericContent"));
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsFalse(field.IsValid, "#3");
        }
        [TestMethod]
        public void FieldOnHandler_Ref_SelectionRoot()
        {
            string fieldName = "SelectionRoot";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            List<Node> refs;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(User.Administrator);
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsFalse(field.IsValid, "#1");

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(ContentTypeManager.Current.GetContentTypeByName("Car"));
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsTrue(field.IsValid, "#3");
        }
        [TestMethod]
        public void FieldOnHandler_Ref_Query()
        {
            string fieldName = "Query";

            Content content = Content.CreateNew("FieldOnHandlerTest", Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);
            FieldTestHandler handler = (FieldTestHandler)content.ContentHandler;
            ReferenceField field = (ReferenceField)content.Fields[fieldName];

            List<Node> refs;

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(User.Administrator);
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsFalse(field.IsValid, "#1");

            handler.GeneralReference = new Node[0];
            refs = new List<Node>();
            refs.Add(ContentTypeManager.Current.GetContentTypeByName("Car"));
            field.SetData(refs);
            contentAcc.SaveFields();
            Assert.IsTrue(field.IsValid, "#2");
        }

        [TestMethod]
        public void FieldOnHandler_HyperLink()
        {
            ContentAccessor contentAcc = HyperLinkTest("FieldOnHandlerTest");
            Content content = contentAcc.TargetContent;

            string fieldName = "HyperLink";
            string testHandlerValue = "<a href=\"testHref\" target=\"testTarget\" title=\"testTitle\">testText</a>";
            string testText = "testText";
            string testHref = "testHref";
            string testTarget = "testTarget";
            string testTitle = "testTitle";

            ((FieldTestHandler)content.ContentHandler).HyperLink = testHandlerValue;
            HyperLinkField.HyperlinkData data = content.Fields[fieldName].OriginalValue as HyperLinkField.HyperlinkData;
            Assert.IsNotNull(data, "#14a");
            Assert.IsTrue(data.Href == testHref, "#14b");
            Assert.IsTrue(data.Text == testText, "#14c");
            Assert.IsTrue(data.Target == testTarget, "#14d");
            Assert.IsTrue(data.Title == testTitle, "#14e");

        }
        [TestMethod]
        public void FieldOnHandler_DateTime()
        {
            ContentAccessor contentAcc = DatetimeTest("FieldOnHandlerTest");
            Content content = contentAcc.TargetContent;

            string fieldName = "DateTime";
            DateTime testHandlerValue = DateTime.Parse("2008-06-04 12:34:56");

            ((FieldTestHandler)content.ContentHandler).DateTime = testHandlerValue;
            DateTime data = (DateTime)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(data == testHandlerValue);
        }

        //[TestMethod]
        //public void FieldOnHandler_NullValue_DateTime()
        //{
        //    var dateTimeValue = DateTime.Parse("2008-06-04 12:34:56");

        //    FieldOnHandler_NullValueTest("DateTime", dateTimeValue, dateTimeValue.AddDays(1));
        //}

        [TestMethod]
        public void FieldOnHandler_NullValue_Int()
        {
            FieldOnHandler_NullValueTest("Int32", 10, 20);
        }

        [TestMethod]
        public void FieldOnHandler_NullValue_Decimal()
        {
            FieldOnHandler_NullValueTest("Decimal", 10.0m, 20.0m);
        }

        public void FieldOnHandler_NullValueTest(string fieldName, object realValue1, object realValue2)
        {
            var node = new FieldTestHandler(Repository.Root, "FieldOnHandlerTest");

            //save and reload the node to have shared data
            node.Save();
            node = Node.Load<FieldTestHandler>(node.Id);

            //set null value
            node[fieldName] = null;

            var ndType = typeof(NodeData);

            var changed = node.Data.IsDynamicPropertyChanged(node.PropertyTypes[fieldName], null);

            Assert.IsTrue(changed, "Value not changed");

            //set real value
            node[fieldName] = realValue1;

            changed = node.Data.IsDynamicPropertyChanged(node.PropertyTypes[fieldName], realValue1);
            Assert.IsTrue(changed, "Value not changed");

            //save and reload the node to have shared data
            node.Save();
            node = Node.Load<FieldTestHandler>(node.Id);

            changed = node.Data.IsDynamicPropertyChanged(node.PropertyTypes[fieldName], realValue1);
            Assert.IsFalse(changed, "Value changed");

            //check with different real value
            node[fieldName] = realValue2;

            changed = node.Data.IsDynamicPropertyChanged(node.PropertyTypes[fieldName], realValue2);
            Assert.IsTrue(changed, "Value not changed");

            node.Delete();
        }

        private ContentAccessor ShortTextTest(string contentTypeName)
        {
            string originalValue;
            string currentValue;
            string handlerValue;
            string fieldName = "ShortText";
            string defaultValue = (string)PropertyType.GetDefaultValue(DataType.String);
            string testValue = "TestValue";
            string defaultHandlerValue = defaultValue;
            string testHandlerValue = testValue;

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#1");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#2");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#4");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#6");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#7");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#9");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#11");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#12");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#13");

            return contentAcc;
        }
        private ContentAccessor LongTextTest(string contentTypeName)
        {
            string originalValue;
            string currentValue;
            string handlerValue;
            string fieldName = "LongText";
            string defaultValue = (string)PropertyType.GetDefaultValue(DataType.Text);
            string testValue = "TestValue";
            string defaultHandlerValue = defaultValue;
            string testHandlerValue = testValue;

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#1");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#2");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#4");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#6");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#7");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#9");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (string)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#11");
            currentValue = (string)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#12");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#13");

            return contentAcc;
        }
        private ContentAccessor BooleanTest(string contentTypeName)
        {
            bool originalValue;
            bool currentValue;
            int handlerValue;
            string fieldName = "Boolean";
            bool defaultValue = false;
            bool testValue = true;
            int defaultHandlerValue = 0;
            int testHandlerValue = 1;

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (bool)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#1");
            currentValue = (bool)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#2");
            handlerValue = (int)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (bool)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#4");
            currentValue = (bool)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (bool)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#6");
            currentValue = (bool)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#7");
            handlerValue = (int)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (bool)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#9");
            currentValue = (bool)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (bool)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#11");
            currentValue = (bool)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#12");
            handlerValue = (int)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#13");

            return contentAcc;
        }
        private ContentAccessor WhoAndWhenTest(string contentTypeName)
        {
            WhoAndWhenField.WhoAndWhenData originalValue;
            WhoAndWhenField.WhoAndWhenData currrenValue;
            //WhoAndWhenField.WhoAndWhenData defultValue;
            //WhoAndWhenField.WhoAndWhenData testValue;
            User originalUserValue;
            User currentUserValue;
            NodeList<Node> handlerUserValue;
            DateTime originalDateValue;
            DateTime currentDateValue;
            DateTime handlerDateValue;
            string fieldName = "WhoAndWhen";
            string userPropertyName = "Who";
            string datePropertyName = "When";
            User defaultUserValue = null;
            User testUserValue = User.Administrator;
            DateTime defaultDateValue = DateTime.MinValue;
            DateTime testDateValue = new DateTime(2001, 4, 17);
            DateTime defaultHandlerDateValue = defaultDateValue;
            DateTime testHandlerDateValue = testDateValue;

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (WhoAndWhenField.WhoAndWhenData)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue.Who == defaultUserValue && ItIsDateTimeDefault(originalValue.When), contentTypeName + "####1 Storage2: DateTime default value");
            //Assert.IsTrue(originalValue.Who == defaultUserValue && ((TimeSpan)(DateTime.Now - originalValue.When)).TotalMinutes < 1, contentTypeName + "####1 Storage2: DateTime default value");
            currrenValue = (WhoAndWhenField.WhoAndWhenData)content[fieldName];
            Assert.IsTrue(currrenValue.Who == defaultUserValue && ItIsDateTimeDefault(currrenValue.When), contentTypeName + "#2");
            handlerUserValue = (NodeList<Node>)content.ContentHandler[userPropertyName];
            handlerDateValue = (DateTime)content.ContentHandler[datePropertyName];
            Assert.IsTrue(handlerUserValue.Count == 0 && ItIsDateTimeDefault(handlerDateValue), contentTypeName + "#3");

            content[fieldName] = new WhoAndWhenField.WhoAndWhenData(testUserValue, testDateValue);
            originalValue = (WhoAndWhenField.WhoAndWhenData)content.Fields[fieldName].OriginalValue;
            originalUserValue = originalValue.Who;
            originalDateValue = originalValue.When;
            Assert.IsTrue(originalUserValue == defaultUserValue && ItIsDateTimeDefault(originalDateValue), contentTypeName + "#1");
            currrenValue = (WhoAndWhenField.WhoAndWhenData)content[fieldName];
            currentUserValue = currrenValue.Who;
            currentDateValue = currrenValue.When;
            Assert.IsTrue(currentUserValue == testUserValue && currentDateValue == testDateValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (WhoAndWhenField.WhoAndWhenData)content.Fields[fieldName].OriginalValue;
            originalUserValue = originalValue.Who;
            originalDateValue = originalValue.When;
            Assert.IsTrue(originalUserValue.Id == testUserValue.Id && originalDateValue == testDateValue, contentTypeName + "#6");
            currrenValue = (WhoAndWhenField.WhoAndWhenData)content[fieldName];
            currentUserValue = currrenValue.Who;
            currentDateValue = currrenValue.When;
            Assert.IsTrue(currentUserValue == testUserValue && currentDateValue == testDateValue, contentTypeName + "#7");
            handlerUserValue = (NodeList<Node>)content.ContentHandler[userPropertyName];
            handlerDateValue = (DateTime)content.ContentHandler[datePropertyName];
            Assert.IsTrue(handlerUserValue[0].Id == User.Administrator.Id && handlerDateValue == testDateValue, contentTypeName + "#8");

            WhoAndWhenField.WhoAndWhenData defaultValue = new WhoAndWhenField.WhoAndWhenData();
            defaultValue.Who = defaultUserValue;
            defaultValue.When = DateTime.Now;
            content[fieldName] = defaultValue;
            originalValue = (WhoAndWhenField.WhoAndWhenData)content.Fields[fieldName].OriginalValue;
            originalUserValue = originalValue.Who;
            originalDateValue = originalValue.When;
            Assert.IsTrue(originalUserValue.Id == testUserValue.Id && originalDateValue == testDateValue, contentTypeName + "#9");
            currrenValue = (WhoAndWhenField.WhoAndWhenData)content[fieldName];
            currentUserValue = currrenValue.Who;
            currentDateValue = currrenValue.When;
            Assert.IsTrue(currentUserValue == defaultUserValue && ItIsDateTimeDefault(currentDateValue), contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (WhoAndWhenField.WhoAndWhenData)content.Fields[fieldName].OriginalValue;
            originalUserValue = originalValue.Who;
            originalDateValue = originalValue.When;
            Assert.IsTrue(originalUserValue == defaultUserValue && ItIsDateTimeDefault(originalDateValue), contentTypeName + "#11");
            currrenValue = (WhoAndWhenField.WhoAndWhenData)content[fieldName];
            currentUserValue = currrenValue.Who;
            currentDateValue = currrenValue.When;
            Assert.IsTrue(currentUserValue == defaultUserValue && ItIsDateTimeDefault(currentDateValue), contentTypeName + "#12");
            handlerUserValue = (NodeList<Node>)content.ContentHandler[userPropertyName];
            handlerDateValue = (DateTime)content.ContentHandler[datePropertyName];
            Assert.IsTrue(handlerUserValue.Count == 0, contentTypeName + "#13");
            Assert.IsTrue(ItIsDateTimeDefault(handlerDateValue), contentTypeName + "#14");

            return contentAcc;
        }
        private ContentAccessor NumberTest(string contentTypeName)
        {
            decimal originalValue;
            decimal currentValue;
            decimal handlerValue;
            string fieldName = "Number";
            decimal defaultValue = 0m;
            decimal testValue = 123m;
            decimal defaultHandlerValue = defaultValue;
            decimal testHandlerValue = testValue;

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#1");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#2");
            handlerValue = (decimal)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#4");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#6");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#7");
            handlerValue = (decimal)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#9");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (decimal)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#11");
            currentValue = (decimal)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#12");
            handlerValue = (decimal)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#13");

            return contentAcc;
        }
        private ContentAccessor HyperLinkTest(string contentTypeName)
        {
            string fieldName = "HyperLink";
            HyperLinkField.HyperlinkData originalValue;
            HyperLinkField.HyperlinkData currentValue;
            HyperLinkField.HyperlinkData defaultValue = new HyperLinkField.HyperlinkData();
            HyperLinkField.HyperlinkData testValue = new HyperLinkField.HyperlinkData("testHref", "testText", "testTitle", "testTarget");
            string handlerValue;
            string defaultHandlerValue = (string)PropertyType.GetDefaultValue(DataType.String);
            string testHandlerValue = "<a href=\"testHref\" target=\"testTarget\" title=\"testTitle\">testText</a>";

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (HyperLinkField.HyperlinkData)content.Fields[fieldName].OriginalValue;
            currentValue = (HyperLinkField.HyperlinkData)content[fieldName];
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(originalValue.Href == defaultValue.Href, contentTypeName + "#1a");
            Assert.IsTrue(originalValue.Text == defaultValue.Text, contentTypeName + "#1b");
            Assert.IsTrue(originalValue.Title == defaultValue.Title, contentTypeName + "#1c");
            Assert.IsTrue(originalValue.Target == defaultValue.Target, contentTypeName + "#1d");
            Assert.IsTrue(currentValue.Href == defaultValue.Href, contentTypeName + "#2a");
            Assert.IsTrue(currentValue.Text == defaultValue.Text, contentTypeName + "#2b");
            Assert.IsTrue(currentValue.Title == defaultValue.Title, contentTypeName + "#2c");
            Assert.IsTrue(currentValue.Target == defaultValue.Target, contentTypeName + "#2d");
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (HyperLinkField.HyperlinkData)content.Fields[fieldName].OriginalValue;
            currentValue = (HyperLinkField.HyperlinkData)content[fieldName];
            Assert.IsTrue(originalValue.Href == defaultValue.Href, contentTypeName + "#4a");
            Assert.IsTrue(originalValue.Text == defaultValue.Text, contentTypeName + "#4b");
            Assert.IsTrue(originalValue.Title == defaultValue.Title, contentTypeName + "#4c");
            Assert.IsTrue(originalValue.Target == defaultValue.Target, contentTypeName + "#4d");
            Assert.IsTrue(currentValue.Href == testValue.Href, contentTypeName + "#5a");
            Assert.IsTrue(currentValue.Text == testValue.Text, contentTypeName + "#5b");
            Assert.IsTrue(currentValue.Title == testValue.Title, contentTypeName + "#5c");
            Assert.IsTrue(currentValue.Target == testValue.Target, contentTypeName + "#5d");

            contentAcc.SaveFields();
            originalValue = (HyperLinkField.HyperlinkData)content.Fields[fieldName].OriginalValue;
            currentValue = (HyperLinkField.HyperlinkData)content[fieldName];
            Assert.IsTrue(originalValue.Href == testValue.Href, contentTypeName + "#6a");
            Assert.IsTrue(originalValue.Text == testValue.Text, contentTypeName + "#6b");
            Assert.IsTrue(originalValue.Title == testValue.Title, contentTypeName + "#6c");
            Assert.IsTrue(originalValue.Target == testValue.Target, contentTypeName + "#6d");
            Assert.IsTrue(currentValue.Href == testValue.Href, contentTypeName + "#7a");
            Assert.IsTrue(currentValue.Text == testValue.Text, contentTypeName + "#7b");
            Assert.IsTrue(currentValue.Title == testValue.Title, contentTypeName + "#7c");
            Assert.IsTrue(currentValue.Target == testValue.Target, contentTypeName + "#7d");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (HyperLinkField.HyperlinkData)content.Fields[fieldName].OriginalValue;
            currentValue = (HyperLinkField.HyperlinkData)content[fieldName];
            Assert.IsTrue(originalValue.Href == testValue.Href, contentTypeName + "#9a");
            Assert.IsTrue(originalValue.Text == testValue.Text, contentTypeName + "#9b");
            Assert.IsTrue(originalValue.Title == testValue.Title, contentTypeName + "#9c");//renameok
            Assert.IsTrue(originalValue.Target == testValue.Target, contentTypeName + "#9d");
            Assert.IsTrue(currentValue.Href == defaultValue.Href, contentTypeName + "#10a");
            Assert.IsTrue(currentValue.Text == defaultValue.Text, contentTypeName + "#10b");
            Assert.IsTrue(currentValue.Title == defaultValue.Title, contentTypeName + "#10c");//renameok
            Assert.IsTrue(currentValue.Target == defaultValue.Target, contentTypeName + "#10d");

            contentAcc.SaveFields();
            originalValue = (HyperLinkField.HyperlinkData)content.Fields[fieldName].OriginalValue;
            currentValue = (HyperLinkField.HyperlinkData)content[fieldName];
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(originalValue.Href == defaultValue.Href, contentTypeName + "#11a");
            Assert.IsTrue(originalValue.Text == defaultValue.Text, contentTypeName + "#11b");
            Assert.IsTrue(originalValue.Title == defaultValue.Title, contentTypeName + "#11c"); //renameok
            Assert.IsTrue(originalValue.Target == defaultValue.Target, contentTypeName + "#11d");
            Assert.IsTrue(currentValue.Href == defaultValue.Href, contentTypeName + "#12a");
            Assert.IsTrue(currentValue.Text == defaultValue.Text, contentTypeName + "#12b");
            Assert.IsTrue(currentValue.Title == defaultValue.Title, contentTypeName + "#12c");//renameok
            Assert.IsTrue(currentValue.Target == defaultValue.Target, contentTypeName + "#12d");
            Assert.IsTrue(handlerValue == "<a></a>", contentTypeName + "#13");

            return contentAcc;

        }
        private ContentAccessor VersionTest(string contentTypeName)
        {
            VersionNumber originalValue;
            VersionNumber currentValue;
            string handlerValue;
            string fieldName = "VersionNumber";
            VersionNumber defaultValue = null;
            VersionNumber testValue = new VersionNumber(5, 6, VersionStatus.Draft);
            string defaultHandlerValue = (string)PropertyType.GetDefaultValue(DataType.String);
            string testHandlerValue = testValue.ToString();

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (VersionNumber)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#1");
            currentValue = (VersionNumber)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#2");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (VersionNumber)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#4");
            currentValue = (VersionNumber)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (VersionNumber)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#6");
            currentValue = (VersionNumber)content[fieldName];
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#7");
            handlerValue = (string)content.ContentHandler[fieldName];
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (VersionNumber)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#9");
            currentValue = (VersionNumber)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (VersionNumber)content.Fields[fieldName].OriginalValue;
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#11");
            currentValue = (VersionNumber)content[fieldName];
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#12");
            handlerValue = (string)content.ContentHandler[fieldName];
            //Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#13");
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "####13: Storage2: Expected: [null], current: [empty]");

            return contentAcc;
        }
        private ContentAccessor DatetimeTest(string contentTypeName)
        {
            DateTime originalValue;
            DateTime currentValue;
            DateTime handlerValue;
            string fieldName = "DateTime";
            DateTime defaultValue = DateTime.MinValue;
            DateTime testValue = DateTime.Parse("2008-06-04 12:34:56");
            DateTime defaultHandlerValue = defaultValue;
            DateTime testHandlerValue = testValue;

            Content content = Content.CreateNew(contentTypeName, Repository.Root, "FieldTest");
            ContentAccessor contentAcc = new ContentAccessor(content);

            originalValue = (DateTime)content.Fields[fieldName].OriginalValue;
            currentValue = (DateTime)content[fieldName];
            handlerValue = (DateTime)content.ContentHandler[fieldName];
            Assert.IsTrue(ItIsDateTimeDefault(originalValue), contentTypeName + "####1: Storage2: DateTime default value");
            Assert.IsTrue(ItIsDateTimeDefault(currentValue), contentTypeName + "#2");
            Assert.IsTrue(ItIsDateTimeDefault(handlerValue), contentTypeName + "#3");

            content[fieldName] = testValue;
            originalValue = (DateTime)content.Fields[fieldName].OriginalValue;
            currentValue = (DateTime)content[fieldName];
            Assert.IsTrue(ItIsDateTimeDefault(originalValue), contentTypeName + "#4");
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#5");
            contentAcc.SaveFields();
            originalValue = (DateTime)content.Fields[fieldName].OriginalValue;
            currentValue = (DateTime)content[fieldName];
            handlerValue = (DateTime)content.ContentHandler[fieldName];
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#6");
            Assert.IsTrue(currentValue == testValue, contentTypeName + "#7");
            Assert.IsTrue(handlerValue == testHandlerValue, contentTypeName + "#8");

            content[fieldName] = defaultValue;
            originalValue = (DateTime)content.Fields[fieldName].OriginalValue;
            currentValue = (DateTime)content[fieldName];
            Assert.IsTrue(originalValue == testValue, contentTypeName + "#9");
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#10");
            contentAcc.SaveFields();
            originalValue = (DateTime)content.Fields[fieldName].OriginalValue;
            currentValue = (DateTime)content[fieldName];
            handlerValue = (DateTime)content.ContentHandler[fieldName];
            Assert.IsTrue(originalValue == defaultValue, contentTypeName + "#11");
            Assert.IsTrue(currentValue == defaultValue, contentTypeName + "#12");
            Assert.IsTrue(handlerValue == defaultHandlerValue, contentTypeName + "#13");

            return contentAcc;
        }

        private bool ItIsDateTimeDefault(DateTime value)
        {
            var d = (DateTime)PropertyType.GetDefaultValue(DataType.DateTime);
            var b = ((TimeSpan)(d - value)).TotalMinutes < 1;
            return b;
        }

        //======================================================================= Field Parse method tests
        [TestMethod]
        public void Field_Parse()
        {
            var content = Content.CreateNew("FieldOnHandlerTest", _testRoot, "asdf");

            //-- strings
            //TODO: ShortText maxlength?
            Assert.IsTrue(ParseField(content, "ShortText", "short testvalue"), "#Parse error: ShortText");
            Assert.IsTrue((string)content["ShortText"] == "short testvalue", "#Value error: ShortText");

            Assert.IsTrue(ParseField(content, "LongText", "long testvalue"), "#Parse error: LongText");
            Assert.IsTrue((string)content["LongText"] == "long testvalue", "#Value error: LongText");

            //--bool true
            Assert.IsTrue(ParseField(content, "Boolean", "true"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "yes"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "tRUE"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "yES"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "1"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "-1"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "-1234"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "-0.01"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == true, "#Value error: Boolean");

            //-- bool false
            Assert.IsTrue(ParseField(content, "Boolean", "false"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "no"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "fALSe"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "0"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", ""), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", null), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");
            Assert.IsTrue(ParseField(content, "Boolean", "-0.00"), "#Parse error: Boolean");
            Assert.IsTrue((bool)content["Boolean"] == false, "#Value error: Boolean");

            Assert.IsTrue(ParseField(content, "Byte", "123"), "#Parse error: Byte");
            Assert.IsTrue((int)content["Byte"] == (int)123, "#Value error: Byte");

            Assert.IsTrue(ParseField(content, "Int16", "-1234"), "#Parse error: Int16");
            Assert.IsTrue((int)content["Int16"] == (int)(-1234), "#Value error: Int16");

            Assert.IsTrue(ParseField(content, "Int32", "-12345"), "#Parse error: Int32");
            Assert.IsTrue((int)content["Int32"] == (int)(-12345), "#Value error: Int32");

            Assert.IsTrue(ParseField(content, "Int64", "-123456"), "#Parse error: Int64");
            Assert.IsTrue((decimal)content["Int64"] == (decimal)(-123456), "#Value error: Int64");

            Assert.IsTrue(ParseField(content, "Single", "-3.14"), "#Parse error: Single");
            Assert.IsTrue((decimal)content["Single"] == (decimal)(-3.14), "#Value error: Single");

            Assert.IsTrue(ParseField(content, "Double", "-3.1415"), "#Parse error: Double");
            Assert.IsTrue((decimal)content["Double"] == (decimal)(-3.1415), "#Value error: Double");

            Assert.IsTrue(ParseField(content, "Decimal", "-6.66666"), "#Parse error: Decimal");
            Assert.IsTrue((decimal)content["Decimal"] == (decimal)(-6.66666), "#Value error: Decimal");

            Assert.IsTrue(ParseField(content, "Number", "-123.456789"), "#Parse error: Number");
            Assert.IsTrue((decimal)content["Number"] == (decimal)(-123.456789), "#Value error: Number");

            Assert.IsTrue(ParseField(content, "SByte", "123"), "#Parse error: SByte");
            Assert.IsTrue((int)content["SByte"] == 123, "#Value error: SByte");

            Assert.IsTrue(ParseField(content, "UInt16", "1234"), "#Parse error: UInt16");
            Assert.IsTrue((int)content["UInt16"] == 1234, "#Value error: UInt16");

            Assert.IsTrue(ParseField(content, "UInt32", "12345"), "#Parse error: UInt32");
            Assert.IsTrue((decimal)content["UInt32"] == (decimal)12345, "#Value error: UInt32");

            Assert.IsTrue(ParseField(content, "UInt64", "123456"), "#Parse error: UInt64");
            Assert.IsTrue((decimal)content["UInt64"] == (decimal)123456, "#Value error: UInt64");

            //-- references
            Assert.IsTrue(ParseField(content, "UserReference", User.Administrator.Path), "#Parse error: UserReference");
            Assert.IsTrue(((User)content["UserReference"]).Id == User.Administrator.Id, "#Value error: UserReference");

            Assert.IsTrue(ParseField(content, "UsersReference", String.Concat(User.Administrator.Path, ",", User.Visitor.Path)), "#Parse error: UsersReference");
            foreach (var user in (IEnumerable<Node>)content["UsersReference"])
                Assert.IsTrue(user.Id == User.Administrator.Id || user.Id == User.Visitor.Id, "#Value error: UsersReference");

            Assert.IsTrue(ParseField(content, "GeneralReference", String.Concat(User.Administrator.Path, ",", Repository.Root.Path)), "#Parse error: GeneralReference");
            foreach (var node in (IEnumerable<Node>)content["GeneralReference"])
                Assert.IsTrue(node.Id == User.Administrator.Id || node.Id == Repository.Root.Id, "#Value error: GeneralReference");

            //-- datetime
            var datetime = DateTime.Now;
            var dstring = datetime.ToString("s");
            Assert.IsTrue(ParseField(content, "DateTime", dstring), "#Parse error: DateTime");
            Assert.IsTrue(((DateTime)content["DateTime"]).ToString("s") == dstring, "#Value error: DateTime");

            //-- color

            //Assert.IsTrue(ParseField(content, "HyperLink", ""), "#Parse error: HyperLink");
            //Assert.IsTrue((___)content["___"] == ___, "#Value error: HyperLink");

            //Assert.IsTrue(ParseField(content, "VersionNumber", ""), "#Parse error: VersionNumber");
            //Assert.IsTrue((___)content["___"] == ___, "#Value error: VersionNumber");


        }
        private bool ParseField(Content content, string fieldName, string value)
        {
            var field = content.Fields[fieldName];
            var success = field.Parse(value);
            return success;
        }

        [TestMethod]
        public void Field_ParseContent()
        {
            var fieldData = new Dictionary<string, string>();
            fieldData.Add("Byte", "256");
            fieldData.Add("Single", Double.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture));
            var content = Content.CreateNewAndParse("FieldOnHandlerTest", _testRoot, "asdf", fieldData);
            Assert.IsFalse(content.IsValid);

            var sb = new StringBuilder();
            foreach (var fieldName in content.Fields.Keys)
                //if(content.Fields[fieldName].ValidationResult != null)
                sb.Append(fieldName).Append(": ").AppendLine(content.Fields[fieldName].GetValidationMessage());

            Assert.IsTrue(content.Fields["Byte"].GetValidationMessage() != "Successful");
            Assert.IsTrue(content.Fields["Single"].GetValidationMessage() != "Successful");

            var contentAcc = new ContentAccessor(content);
            contentAcc.SaveFields();
        }

        //======================================================================= Outer Field tests

        [TestMethod]
        public void Field_OuterField_OnTheContent()
        {
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
                <ContentType name='OuterFieldTestContentType' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                    <Fields>
                        <Field name='OuterField1' type='OuterField' />
                    </Fields>
                </ContentType>
                ");

            var content = Content.CreateNew("OuterFieldTestContentType", _testRoot, new Guid().ToString());

            content["OuterField1"] = "asdf";
            content.ContentHandler.Index = 123;
            content.Save();
            var id = content.Id;

            content = Content.Load(id);
            var fieldValue = content["OuterField1"];
            var indexValue = content.ContentHandler.Index;

            Assert.IsTrue(content.ContentHandler.PropertyTypes["OuterField1"] == null, "#1");
            Assert.IsTrue(content.ContentHandler.Index == 124, "#2");
        }
        [TestMethod]
        public void Field_OuterField_OnTheContentList()
        {
            ContentTypeInstaller.InstallContentType(@"<?xml version='1.0' encoding='utf-8'?>
                <ContentType name='OuterFieldTestContentType' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
                </ContentType>
                ");
            string ltd = @"<?xml version='1.0' encoding='utf-8'?>
				<ContentListDefinition xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentListDefinition'>
					<Fields><ContentListField name='#OuterField1' type='OuterField' /></Fields></ContentListDefinition>";

            ContentList list = new ContentList(_testRoot);
            list.ContentListDefinition = ltd;
            list.Save();

            var content = Content.CreateNew("OuterFieldTestContentType", list, new Guid().ToString());

            content["#OuterField1"] = "asdf";
            content.ContentHandler.Index = 123;
            content.Save();
            var id = content.Id;

            content = Content.Load(id);
            var fieldValue = content["#OuterField1"];
            var indexValue = content.ContentHandler.Index;

            Assert.IsTrue(content.ContentHandler.PropertyTypes["#OuterField1"] == null, "#1");
            Assert.IsTrue(content.ContentHandler.Index == 124, "#2");
        }
    }

    [ShortName("OuterField")]
    [DefaultFieldSetting(typeof(NullFieldSetting))]
    [DefaultFieldControl("SenseNet.Portal.UI.Controls.ShortText")]
    public class OuterField : Field
    {
        protected override object ReadProperties()
        {
            return this.Content.ContentHandler.Index;
        }
        protected override void WriteProperties(object value)
        {
            this.Content.ContentHandler.Index++;
        }
        protected override void ImportData(XmlNode fieldNode, ImportContext context)
        {
            throw new NotImplementedException();
        }
    }

}