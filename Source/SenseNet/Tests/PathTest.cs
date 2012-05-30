using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Collections.Generic;
using SenseNet.ContentRepository.Storage;

using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.ContentRepository.Tests
{
    /// <summary>
    ///This is a test class for SenseNet.ContentRepository.Storage.Path and is intended
    ///to contain all SenseNet.ContentRepository.Storage.Path Unit Tests
    ///</summary>
    [TestClass()]
    public class PathTest : TestBase
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


        [TestMethod()]
        public void Path_IsPathExists()
        {
			Assert.IsTrue(Node.Exists(Repository.RootPath));
        }

        [TestMethod()]
        public void Path_IsPathExists_Invalid()
        {
			Assert.IsFalse(Node.Exists(Repository.RootPath + "/A/B/C.D"));
        }


        //[TestMethod()]
        //public void Node_IsPathAvailable()
        //{
        //    //Folder folder = Portal1.Root;
        //    //Assert.IsTrue(folder.IsPathAvailable);
        //    //folder.Name = "TestRoot";
        //    //Assert.IsTrue(folder.IsPathAvailable);
        //}

        //[TestMethod()]
        //public void Node_IsPathAvailable_Invalid()
        //{
        //    Folder folder = new Folder(Repository.Root);
        //    folder.Name = Repository.SystemFolderName;
        //    Assert.IsTrue(Node.Exists(Repository.RootPath + "/" + Repository.SystemFolderName));
        //    Assert.IsFalse(folder.IsPathAvailable);
        //}

        [TestMethod()]
        public void Path_InvalidPathChars()
        {
            Assert.IsNotNull(RepositoryPath.InvalidPathCharsPattern);
        }

        [TestMethod()]
        public void Path_InvalidPathChars_Modify1()
        {
            Assert.IsNotNull(RepositoryPath.InvalidPathCharsPattern);

            // Try to modify the system array through a pointer
            string val = RepositoryPath.InvalidPathCharsPattern;
            val = null;
            Assert.IsNotNull(RepositoryPath.InvalidPathCharsPattern);
        }

        //[TestMethod()]
        //public void Path_InvalidPathChars_Modify2()
        //{
        //    RepositoryPath.InvalidPathChars[1] = 'f';
        //    Assert.AreEqual(':', RepositoryPath.InvalidPathChars[1]);
        //}

        [TestMethod()]
        public void Path_PathSeparator()
        {
            Assert.IsNotNull(RepositoryPath.PathSeparator);
            Assert.IsTrue(RepositoryPath.PathSeparator.Length > 0);
        }


        #region IsValidPath use cases

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Path_IsValidPath_Null()
        {
            RepositoryPath.IsValidPath(null);
        }

		[DeploymentItem("SenseNet.Storage.dll")]
        [TestMethod()]
        public void Path_IsValidPath_PathMaxLengthCheck()
        { 
			int maxPathLength = RepositoryPath.MaxLength;

            // Check at the limit first
            StringBuilder sb = new StringBuilder(maxPathLength + 1);
            sb.Append(RepositoryPath.PathSeparator);
            for(int i = RepositoryPath.PathSeparator.Length; i < maxPathLength; i++)
            {
                sb.Append("x");
            }

			Assert.IsTrue(RepositoryPath.IsValidPath(sb.ToString()) == RepositoryPath.PathResult.Correct);

            // Add 1 char to the max limit
            sb.Append("x");

            Assert.IsTrue(RepositoryPath.IsValidPath(sb.ToString()) == RepositoryPath.PathResult.TooLong);
        }

        [TestMethod()]
        public void Path_IsValidPath_Valid()
        {
            Assert.IsTrue(RepositoryPath.IsValidPath("/Root folder/System folder/Sub.folder/File name.extension") == RepositoryPath.PathResult.Correct);
        }

        [TestMethod()]
        public void Path_IsValidPath_Empty()
        {
            Assert.IsTrue(RepositoryPath.IsValidPath("") == RepositoryPath.PathResult.Empty);
        }

        [TestMethod()]
        public void Path_IsValidPath_InvalidRoot()
        {
			Assert.IsTrue(RepositoryPath.IsValidPath("root") == RepositoryPath.PathResult.InvalidFirstChar);
        }

		[TestMethod()]
		public void Path_IsValid_WhiteSpaces()
		{
            // tab is invalid character. space is allowed, but not at beginning or end
			Assert.IsTrue(RepositoryPath.IsValidPath("/Root/ alma") == RepositoryPath.PathResult.StartsWithSpace, "#1");
			Assert.IsTrue(RepositoryPath.IsValidPath("/Root/alma ") == RepositoryPath.PathResult.EndsWithSpace, "#2");
			Assert.IsTrue(RepositoryPath.IsValidPath("/Root/	alma") == RepositoryPath.PathResult.InvalidPathChar, "#3");
            Assert.IsTrue(RepositoryPath.IsValidPath("/Root/alma	") == RepositoryPath.PathResult.InvalidPathChar, "#4");
			Assert.IsTrue(RepositoryPath.IsValidPath("/Root//alma") == RepositoryPath.PathResult.Empty, "#5");
            Assert.IsTrue(RepositoryPath.IsValidPath("/Root/\t/alma") == RepositoryPath.PathResult.InvalidPathChar, "#6");
			Assert.IsTrue(RepositoryPath.IsValidName(" alma") == RepositoryPath.PathResult.StartsWithSpace, "#7");
			Assert.IsTrue(RepositoryPath.IsValidName("alma ") == RepositoryPath.PathResult.EndsWithSpace, "#8");
			Assert.IsTrue(RepositoryPath.IsValidName("	alma") == RepositoryPath.PathResult.InvalidNameChar, "#9");
            Assert.IsTrue(RepositoryPath.IsValidName("alma	") == RepositoryPath.PathResult.InvalidNameChar, "#10");
			Assert.IsTrue(RepositoryPath.IsValidName(" ") == RepositoryPath.PathResult.StartsWithSpace, "#11");
            Assert.IsTrue(RepositoryPath.IsValidName("\t") == RepositoryPath.PathResult.InvalidNameChar, "#12");
		}

        [TestMethod()]
        public void Path_InvalidPathChar()
        {
            // try some characters that are invalid (path invalidcharpattern is [^a-zA-Z0-9.()[\\]\\-_ /])
            var chars = new char[] { '\\', ':', '*', '?', '"', '<', '>', '|', ',', ';', '&', '#', '+' };
            foreach (char c in chars)
                Assert.IsTrue(RepositoryPath.IsValidPath(String.Concat("/Root/a", c, "b")) == RepositoryPath.PathResult.InvalidPathChar, string.Concat("Invalid path char: ", c));
        }
        [TestMethod()]
        public void Path_ValidPathChar()
        {
            // try some characters that are valid (path invalidcharpattern is [^a-zA-Z0-9.()[\\]\\-_ /])
            var chars = new char[] { 'b', 'V', '4', '.', '(', ')', '[', ']', '-', '_', ' ', '/' };
            foreach (char c in chars)
                Assert.IsTrue(RepositoryPath.IsValidPath(String.Concat("/Root/a", c, "b")) == RepositoryPath.PathResult.Correct, string.Concat("Invalid path char: ", c));
        }
        [TestMethod()]
		public void Path_InvalidNameChar()
		{
            // try some characters that are invalid (name invalidcharpattern is [^a-zA-Z0-9.()[\\]\\-_ ])
            var chars = new char[] { '\\', ':', '*', '?', '"', '<', '>', '|', ',', ';', '&', '#', '+', '/' };
			foreach (char c in chars)
				Assert.IsTrue(RepositoryPath.IsValidName(String.Concat("a", c, "b")) == RepositoryPath.PathResult.InvalidNameChar, string.Concat("Invalid path char: ", c));
		}
        [TestMethod()]
        public void Path_ValidNameChar()
        {
            // try some characters that are valid (name invalidcharpattern is [^a-zA-Z0-9.()[\\]\\-_ ])
            var chars = new char[] { 'b', 'V', '4', '.', '(', ')', '[', ']', '-', '_', ' ' };
            foreach (char c in chars)
                Assert.IsTrue(RepositoryPath.IsValidName(String.Concat("a", c, "b")) == RepositoryPath.PathResult.Correct, string.Concat("Invalid path char: ", c));
        }

        #endregion

        #region GetParentPath use cases

        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Path_GetParentPath_Null()
        {
            RepositoryPath.GetParentPath(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidPathException))]
        public void Path_GetParentPath_InvalidPath()
        {
            RepositoryPath.GetParentPath("");
        }

        [TestMethod()]
        public void Path_GetParentPath_RootPath()
        {
            string val = RepositoryPath.GetParentPath("/RootSample");
            Assert.IsNull(val);
        }

        [TestMethod()]
        public void Path_GetParentPath_UseCase1()
        {
            string val = RepositoryPath.GetParentPath("/Root/System");
            Assert.AreEqual("/Root", val);
        }

        [TestMethod()]
        public void Path_GetParentPath_UseCase2()
        {
            string val = RepositoryPath.GetParentPath("/Root path/subPath/fileName.otherpart.extension");
            Assert.AreEqual("/Root path/subPath", val);
        }

        #endregion


    }

}