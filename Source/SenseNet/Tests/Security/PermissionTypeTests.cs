using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using System.Threading;
using SenseNet.ContentRepository.Storage.Schema;

namespace SenseNet.ContentRepository.Tests.Security
{
	[TestClass]
    public class PermissionTypeTests : TestBase
	{
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

		[TestMethod()]
		public void PermissionType_CheckSystemStructure()
		{
			Assert.IsTrue(PermissionType.DefaultPermissionNames.Length == 15, "Count of default permissions is not 15");
			int id = 0;
			foreach (string name in PermissionType.DefaultPermissionNames)
			{
				PermissionType pt = ActiveSchema.PermissionTypes[name];
				Assert.IsTrue(pt.Id == ++id, "Id is not " + id);
				Assert.IsTrue(pt.IsDefaultPermission, name + " is not default permission");
			}

		}

	}
}