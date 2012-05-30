//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Text;
//using System.Collections.Generic;
//using SenseNet.ContentRepository.Storage.Search;
//using SenseNet.ContentRepository.Storage;

//namespace SenseNet.ContentRepository.Tests.Data
//{
//    [TestClass()]
//    public class SqlCompilerTest : TestBase
//    {
//        private TestContext testContextInstance;

//        public override TestContext TestContext
//        {
//            get
//            {
//                return testContextInstance;
//            }
//            set
//            {
//                testContextInstance = value;
//            }
//        }
//        #region Additional test attributes
//        // 
//        //You can use the following additional attributes as you write your tests:
//        //
//        //Use ClassInitialize to run code before running the first test in the class
//        //
//        //[ClassInitialize()]
//        //public static void MyClassInitialize(TestContext testContext)
//        //{
//        //}
//        //
//        //Use ClassCleanup to run code after all tests in a class have run
//        //
//        //[ClassCleanup()]
//        //public static void MyClassCleanup()
//        //{
//        //}
//        //
//        //Use TestInitialize to run code before running each test
//        //
//        //[TestInitialize()]
//        //public void MyTestInitialize()
//        //{
//        //}
//        //
//        //Use TestCleanup to run code after each test has run
//        //
//        //[TestCleanup()]
//        //public void MyTestCleanup()
//        //{
//        //}
//        //
//        #endregion

//        private string RemoveComments(string sqlText)
//        {
//            int p0, p1;
//            //-- block comments
//            while((p0 = sqlText.IndexOf("/*")) >= 0)
//            {
//                p1 = sqlText.IndexOf("*/", p0);
//                if (p1 > p0)
//                    sqlText = sqlText.Remove(p0, p1 - p0 + 2);
//            }
//            //-- line comments
//            string[] lines = sqlText.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
//            for (int i = 0; i < lines.Length; i++)
//            {
//                p0 = lines[i].IndexOf("--");
//                if (p0 >= 0)
//                    lines[i] = lines[i].Remove(p0);
//                lines[i] = lines[i].Trim();
//            }
//            return String.Join(" ", lines).Replace("( ", "(").Replace(" )", ")").Replace(" =", "=").Replace("= ", "=").Replace(", ", ",");
//        }
//    }

//}