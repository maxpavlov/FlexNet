using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using SenseNet.ContentRepository.Storage;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Fields;
using SenseNet.Portal.UI.Controls;

namespace SenseNet.ContentRepository.Tests
{
	[TestClass]
    public class EvaluatorTest : TestBase
	{
		#region Test Infrastructure
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

		private static string _testRootName = "_EvaluatorTest";
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
        [ClassCleanup]
        public static void DestroyPlayground()
        {
            if (Node.Exists(__testRootPath))
                Node.ForceDelete(__testRootPath);
            ContentType ct;
            ct = ContentType.GetByName("DefaultValueTesterContentType");
            if (ct != null)
                ct.Delete();
            //ct = ContentType.GetByName("ValidatedContent");
            //if (ct != null)
            //    ct.Delete();
            //ct = ContentType.GetByName("ReferredContent");
            //if (ct != null)
            //    ct.Delete();
        }

		[TestMethod]
		public void DefaultValue_Simple_ShortText()
		{
			ContentType carType = ContentType.GetByName("Car");
			string defaultValue = null;
			string userInput = "-- UserInput --";
			string fieldName = null;
			string fieldValue = null;

			//==== search a testable ShortText field
			foreach (var fieldSetting in carType.FieldSettings)
			{
				ShortTextFieldSetting shortTextSetting = fieldSetting as ShortTextFieldSetting;
				if (shortTextSetting == null)
					continue;
				fieldName = shortTextSetting.Name;
				defaultValue = "== " + fieldName + " ==";
				var baseSettingAccessor = new PrivateObject(shortTextSetting, new PrivateType(typeof(FieldSetting)));
				baseSettingAccessor.SetField("_defaultValue", BindingFlags.NonPublic | BindingFlags.Instance, defaultValue);
				break;
			}
			if (fieldName == null)
				Assert.Inconclusive("Car ContentType do not have any ShortText field.");

			//==== create a new Content
			var newContent = Content.CreateNew("Car", _testRoot, "Car1");
			var editedField = newContent.Fields[fieldName];

			//==== simulating contentview:
			//-- create control RegisterFieldControl, SetData, Post default value
			ShortText shortTextControl = new ShortText();
			ShortTextAccessor shortTextControlAcc = new ShortTextAccessor(shortTextControl);
			//-- RegisterFieldControl, SetData, Post default value
			shortTextControlAcc.ConnectToField(editedField);
			shortTextControlAcc.SetDataInternal();
			//-- Post default value
			editedField.SetData(shortTextControl.GetData());

			//==== Check default value
			fieldValue = (string)newContent[fieldName];
			Assert.IsTrue(fieldValue == defaultValue, "#1");

			//-- simulating userinput: overwrite default value
			shortTextControlAcc.InputTextBox.Text = userInput;

			//-- simulating contentview: PostData
			editedField.SetData(shortTextControl.GetData());

			//==== Check user input
			fieldValue = (string)newContent[fieldName];
			Assert.IsTrue(fieldValue == userInput, "#1");

		}
		[TestMethod]
		public void DefaultValue_Scripted_ShortText()
		{
			ContentType carType = ContentType.GetByName("Car");
			string defaultValue = "The answer to life the universe and everything plus one = [Script:jScript]WhatIsTheAnswerToLifeTheUniverseAndEverything() + 1;[/Script].";
			string evaluatedValue = "The answer to life the universe and everything plus one = 43.";
			string userInput = "-- UserInput --";
			string fieldName = null;
			string fieldValue = null;

			//==== search a testable ShortText field
			foreach (var fieldSetting in carType.FieldSettings)
			{
				ShortTextFieldSetting shortTextSetting = fieldSetting as ShortTextFieldSetting;
				if (shortTextSetting == null)
					continue;
				fieldName = shortTextSetting.Name;
				var baseSettingAccessor = new PrivateObject(shortTextSetting, new PrivateType(typeof(FieldSetting)));
				baseSettingAccessor.SetField("_defaultValue", BindingFlags.NonPublic | BindingFlags.Instance, defaultValue);
				break;
			}
			if (fieldName == null)
				Assert.Inconclusive("Car ContentType do not have any ShortText field.");

			//==== create a new Content
			var newContent = Content.CreateNew("Car", _testRoot, "Car1");
			var editedField = newContent.Fields[fieldName];

			//==== simulating contentview:
			//-- create control RegisterFieldControl, SetData, Post default value
			ShortText shortTextControl = new ShortText();
			ShortTextAccessor shortTextControlAcc = new ShortTextAccessor(shortTextControl);
			//-- RegisterFieldControl, SetData, Post default value
			shortTextControlAcc.ConnectToField(editedField);
			shortTextControlAcc.SetDataInternal();
			//-- Post default value
			editedField.SetData(shortTextControl.GetData());

			//==== Check default value
			fieldValue = (string)newContent[fieldName];
			Assert.IsTrue(fieldValue == evaluatedValue, "#1");

			//-- simulating userinput: overwrite default value
			shortTextControlAcc.InputTextBox.Text = userInput;

			//-- simulating contentview: PostData
			editedField.SetData(shortTextControl.GetData());

			//==== Check user input
			fieldValue = (string)newContent[fieldName];
			Assert.IsTrue(fieldValue == userInput, "#2");

		}
		[TestMethod]
		public void DefaultValue_Scripted_Simple()
		{
			var result = DefaultValueTest(new string[][]
			{
				new string[]{"BooleanField", "[Script:jScript]<![CDATA[12 > 11 && 12 < 13]]>[/Script]"},
				new string[]{"DateTimeField", "[Script:jScript]DateTime.Now[/Script]"},
				new string[]{"IntegerField", "[Script:jScript]123 + 987[/Script]"},
				new string[]{"LongTextField", "[Script:jScript]\"Default\" + \" long text \" + \"value\"[/Script]"},
				new string[]{"NumberField", "[Script:jScript]1.0 / 3.0[/Script]"},
				new string[]{"ReferenceField", "[Script:jScript]User.Administrator.Path[/Script]"},
				new string[]{"ShortTextField", "[Script:jScript]\"Default\" + \" short text \" + \"value\"[/Script]"}
				//new string[]{"BinaryField", "Test content for Binary field.", "Test content for Binary field."},
				//new string[]{"ChoiceField", "Option2;Option4", "Option2;Option4"},
				//new string[]{"ColorField", "Red", "Red"},
				//new string[]{"HyperLinkField", "????", "????"},
				//new string[]{"SiteListField", "????", "????"},
				//new string[]{"UrlListField", "????", "????"}
			});

			Assert.IsTrue((bool)result[0] == true);
			Assert.IsTrue((DateTime)result[1] - DateTime.Now < new TimeSpan(0, 0, 2));
			Assert.IsTrue((int)result[2] == 1110);
			Assert.IsTrue((string)result[3] == "Default long text value");
			Assert.IsTrue((decimal)result[4] == (decimal)0.33);
			Assert.IsTrue(((Node)result[5]).Path == "/Root/IMS/BuiltIn/Portal/Administrator");
			Assert.IsTrue((string)result[6] == "Default short text value");
		}

		#region CTD
		string ctdFormat = @"<?xml version='1.0' encoding='utf-8'?>
<ContentType name='DefaultValueTesterContentType' parentType='GenericContent' handler='SenseNet.ContentRepository.GenericContent' xmlns='http://schemas.sensenet.com/SenseNet/ContentRepository/ContentTypeDefinition'>
	<Fields>
		<Field name='BinaryField' type='Binary'>
			<Configuration>{0}</Configuration>
		</Field>
		<Field name='BooleanField' type='Boolean'>
			<Configuration>{1}</Configuration>
		</Field>
		<Field name='ChoiceField' type='Choice'>
			<Configuration>{2}
				<AllowMultiple>true</AllowMultiple>
				<AllowExtraValue>false</AllowExtraValue>
				<Options>
					<Option selected='true'>Option1</Option>
					<Option>Option2</Option>
					<Option>Option3</Option>
					<Option>Option4</Option>
					<Option>Option5</Option>
				</Options>
			</Configuration>
		</Field>
		<Field name='ColorField' type='Color'>
			<Configuration>{3}</Configuration>
		</Field>
		<Field name='DateTimeField' type='DateTime'>
			<Configuration>{4}</Configuration>
		</Field>
		<Field name='HyperLinkField' type='HyperLink'>
			<Configuration>{5}</Configuration>
		</Field>
		<Field name='IntegerField' type='Integer'>
			<Configuration>{6}</Configuration>
		</Field>
		<Field name='LongTextField' type='LongText'>
			<Configuration>{7}</Configuration>
		</Field>
		<Field name='NumberField' type='Number'>
			<Configuration>{8}</Configuration>
		</Field>
		<Field name='ReferenceField' type='Reference'>
			<Configuration>{9}</Configuration>
		</Field>
		<Field name='ShortTextField' type='ShortText'>
			<Configuration>{10}</Configuration>
		</Field>
		<Field name='SiteListField' type='SiteList'>
			<Configuration>{11}</Configuration>
		</Field>
		<Field name='UrlListField' type='UrlList'>
			<Configuration>{12}</Configuration>
		</Field>
		<!-- Not supported: LockField, NodeTypeField, PasswordField, SecurityField, SiteRelativeUrlField, VersionField, WhoAndWhenField -->
	</Fields>
</ContentType>
";
		#endregion

		List<string> fieldNames = new List<string>(new string[]{
			"BinaryField", "BooleanField", "ChoiceField", "ColorField", "DateTimeField", 
			"HyperLinkField", "IntegerField", "LongTextField", "NumberField", "ReferenceField", "ShortTextField",
			"SiteListField", "UrlListField"});

		private object[] DefaultValueTest(string[][] data)
		{
			var result = new List<object>();
			var defaultStrings = new string[fieldNames.Count];
			//var expectedValues = new string[fieldNames.Count];

			foreach (var record in data)
			{
				//new string[]{"ShortTextField", "Default short text value.", "Default short text value."},
				string fieldName = record[0];
				int fieldIndex = fieldNames.IndexOf(fieldName);
				defaultStrings[fieldIndex] = String.Concat("<DefaultValue>", record[1], "</DefaultValue>");
				//expectedValues[fieldIndex] = record[2];
			}

			//----
			var ctd = String.Format(ctdFormat, defaultStrings);
			ContentTypeInstaller.InstallContentType(ctd);

			//----
			var contentTypeName = "DefaultValueTesterContentType";
			var contentType = ContentType.GetByName(contentTypeName);

			//==== create a new Content
			var newContent = Content.CreateNew(contentTypeName, _testRoot, "Content1");
			object fieldValue;

			for (int i = 0; i < data.Length; i++)
			{
				var record = data[i];
				string fieldName = record[0];
				var field = newContent.Fields[fieldName];
				try
				{
					//==== simulating contentview:
					//-- create control
					FieldControl control = GenericFieldControl.CreateDefaultFieldControl(field);
					var controlAcc = new FieldControlAccessor(control);
					//-- RegisterFieldControl
					controlAcc.ConnectToField(field);
					//-- SetData
					controlAcc.SetDataInternal();
					//-- Post default value
					field.SetData(control.GetData());
					//-- check posted value
					fieldValue = newContent[fieldName];
					result.Add(fieldValue);
				}
				catch (Exception e)
				{
					Exception ee = e;
					StringBuilder sb = new StringBuilder();
					while (ee != null)
					{
						sb.Append(ee.Message);
						ee = ee.InnerException;
					}
					result.Add(String.Concat(fieldName, "{Exception:\"", sb.ToString(), "\"}"));
				}
			}

			return result.ToArray();
		}
	}
}