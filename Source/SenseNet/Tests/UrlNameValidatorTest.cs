using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;

namespace SenseNet.ContentRepository.Tests
{
	/// <summary>
	///This is a test class for SenseNet.Portal.UrlNameValidator and is intended
	///to contain all SenseNet.Portal.UrlNameValidator Unit Tests
	///</summary>
	[TestClass()]
    public class UrlNameValidatorTest : TestBase
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


		/// <summary>
		///A test for ValidUrlName (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void ValidUrlNameTest()
		{
			string urlName = "aábcü0-9.aspx";
			string expected = "aabcu0-9.aspx";

			string actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_UrlNameValidatorAccessor.ValidUrlName(urlName);

			Assert.AreEqual(expected, actual, "UrlNameValidator.ValidUrlName did not return the expected value.");
		}


		/// <summary>
		///A test for Validate (string)
		///</summary>
		[DeploymentItem("SenseNet.ContentRepository.dll")]
		[TestMethod()]
		public void ValidateTest()
		{
			string url = "http://www.aábcű.com";

			bool expected = false;
			bool actual = SenseNet.ContentRepository.Tests.SenseNet_Portal_UrlNameValidatorAccessor.Validate(url);

			Assert.AreEqual(expected, actual, "UrlNameValidator.Validate did not return the expected value.");
		}
	}


}