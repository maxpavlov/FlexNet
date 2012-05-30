using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using  SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using System.Collections;

namespace SenseNet.ContentRepository.Tests.Schema
{
	[TestClass()]
    public class AttributeTest : TestBase
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

		[TestMethod]
		public void Attr_ContentHandlerMustBeANode()
		{
			Type type = typeof(SenseNet.ContentRepository.Tests.ContentHandlers.BadHandler1);
			ContentTypeManagerAccessor ctmAcc = new ContentTypeManagerAccessor(ContentTypeManager.Current);
			try
			{
				NodeTypeRegistrationAccessor ntrAcc = ctmAcc.ParseAttributes(type);
			}
			catch (Exception e)
			{
				Exception ee = e.InnerException;
				Assert.IsTrue(ee is ContentRegistrationException);
			}
		}
		[TestMethod]
		public void Attr_ContentHandlerMustHaveAttribute()
		{
			Type type = typeof(SenseNet.ContentRepository.Tests.ContentHandlers.BadHandler2);
			ContentTypeManagerAccessor ctmAcc = new ContentTypeManagerAccessor(ContentTypeManager.Current);
			NodeTypeRegistrationAccessor ntrAcc = ctmAcc.ParseAttributes(type);
			Assert.IsNull(ntrAcc);
		}
		[TestMethod]
		public void Attr_Prop_DefaultNameDefaultDataType()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode3, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode3
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode3.TestInt, DataType=Int, IsDeclared=True
";
			Assert.IsTrue(AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode3)) == expectedLog);
		}
		[TestMethod]
		public void Attr_Prop_DefinedNameDefaultDataType()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode4, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode4
	PTR: name=TestInt1, DataType=Int, IsDeclared=True
";

			Assert.IsTrue(AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode4)) == expectedLog);
		}
		[TestMethod]
		public void Attr_Prop_DefaultNameDefinedDataType()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode5, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode5
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode5.TestInt, DataType=Int, IsDeclared=True
";

			Assert.IsTrue(AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode5)) == expectedLog);
		}
		[TestMethod]
		public void Attr_Prop_DefinedNameDefinedDataType()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode6, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode6
	PTR: name=TestInt1, DataType=Int, IsDeclared=True
";

			Assert.IsTrue(AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode6)) == expectedLog);
		}
		[TestMethod]
		public void Attr_Prop_WithNonPublicProperty()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7.TestInt1, DataType=Int, IsDeclared=True
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7.TestInt, DataType=Int, IsDeclared=True
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7.TestInt2, DataType=Int, IsDeclared=True
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7.TestInt3, DataType=Int, IsDeclared=True
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7.TestInt4, DataType=Int, IsDeclared=True
	PTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7.TestInt5, DataType=Int, IsDeclared=True
";

			Assert.IsTrue(AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode7)) == expectedLog);
		}

		[TestMethod]
		public void Attr_DefaultPropertyDataTypes()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode8, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode8
	PTR: name=bool, DataType=Int, IsDeclared=True
	PTR: name=byte, DataType=Int, IsDeclared=True
	PTR: name=sbyte, DataType=Int, IsDeclared=True
	PTR: name=int, DataType=Int, IsDeclared=True
	PTR: name=Int16, DataType=Int, IsDeclared=True
	PTR: name=Int32, DataType=Int, IsDeclared=True
	PTR: name=Int64, DataType=Currency, IsDeclared=True
	PTR: name=UInt16, DataType=Int, IsDeclared=True
	PTR: name=UInt32, DataType=Int, IsDeclared=True
	PTR: name=UInt64, DataType=Currency, IsDeclared=True
	PTR: name=DateTime, DataType=DateTime, IsDeclared=True
	PTR: name=Single, DataType=Currency, IsDeclared=True
	PTR: name=Double, DataType=Currency, IsDeclared=True
	PTR: name=Decimal, DataType=Currency, IsDeclared=True
	PTR: name=BinaryData, DataType=Binary, IsDeclared=True
";
			string log = AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode8));
			Assert.IsTrue(log == expectedLog);
		}
		[TestMethod]
		public void Attr_PropertyDataTypes()
		{
			string expectedLog = @"NTR: name=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode9, ParentName=[null], Type=SenseNet.ContentRepository.Tests.ContentHandlers.TestNode9
	PTR: name=TestProp1, DataType=Int, IsDeclared=True
	PTR: name=TestProp2, DataType=String, IsDeclared=True
	PTR: name=TestProp3, DataType=Text, IsDeclared=True
";

			Assert.IsTrue(AttributeParsingHelper(typeof(SenseNet.ContentRepository.Tests.ContentHandlers.TestNode9)) == expectedLog);
		}

		private string AttributeParsingHelper(Type type)
		{
			ContentTypeManagerAccessor ctmAcc = new ContentTypeManagerAccessor(ContentTypeManager.Current);
			NodeTypeRegistrationAccessor ntrAcc = ctmAcc.ParseAttributes(type);
			return ntrAcc.ToString();
		}

	}
}