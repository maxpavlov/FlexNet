using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal;
using SenseNet.ApplicationModel;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.ContentRepository.Storage.Search;

namespace SenseNet.ContentRepository.Tests.AppModel
{
    /// <summary>
    /// Summary description for ActionTest
    /// </summary>
    [TestClass]
    public class ActionTest : TestBase
    {

        #region Testcontext
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
        #endregion

        private const string _testRootName = "TestSiteForActionFramework";
        private static readonly string _testRootPath = String.Concat("/Root/", _testRootName);

        //Test structure
        //
        //TestRoot-------|-------(apps)--------|-------Folder--------|----App1(sc1,sc4,sc5)
        //               |                     |                     |----App2(sc1,Settings)
        //               |                     |                     |----App3(sc1,sc2,sc7,sc10) Req: 5
        //               |                     |                     |----App4(sc2,sc6,sc10) Req: 5
        //               |                     |                     |----App5(sc11) Req: 5,10,11
        //               |                     |                     |----App6(sc12)
        //               |                     |                     |----App12(sc20)
        //               |                     |                     |----App21(sc21)
        //               |                     |                     |----App22(sc22)
        //               |                     |                     |----App23(sc23) disabled
        //               |                     |                     |----App24(sc24) disabled, clear
        //               |                     |                     |----App25(sc25)
        //               |                     |
        //               |                     |-------ContentList---|----App1(sc4,sc5)
        //               |                                           |----ContentListApp1(sc2,sc10)
        //               |                                           |----ContentListApp2(sc1)
        //               |                                           |----ContentListApp3(sc1,sc2,sc3,mySettings)
        //               |                                           |----App6(sc12) Req: 11
        //               |
        //               |-------Sample-----|---------(apps)---------|----This---------|-----App12(sc20)
        //               |         folder   |                        |----Folder-------|-----App21(sc21)
        //               |                  |                                        |-----App22(sc22) disabled
        //               |                  |                                        |-----App23(sc23) disabled
        //               |                  |                                        |-----App24(sc24) Req: 5
        //               |                  |                                        |-----App25(sc25) clear
        //               |                  |
        //               |                  |
        //               |                  |---------SubFolder----|----(apps)---------|-----This------|-----App24(sc24) disabled
        //               |                                folder
        //               |
        //               |-------Sample2
        //                         contentlist
        //

        //TODO: a tesztkornyezet felepiteset ujragondolni
        [ClassInitialize]
        public static void CreateSandbox(TestContext testContext)
        {

            var site = new Site(Repository.Root);
            site.Name = "TestSiteForActionFramework";
                var urlList = new Dictionary<string, string>();
                urlList.Add("newtesthost", "Windows");
                site.UrlList = urlList;
                site.Save();
            

            //---- Appmodel
            //-------------

                //Root/TestRoot/(apps)
            var siteAppsFolder = new SystemFolder(site);
                siteAppsFolder.Name = "(apps)";
                siteAppsFolder.Save();
            
            //---- Folder
            //-----------

                // /TestRoot/(apps)/Folder
            var siteAppsFolderFolder = new SystemFolder(siteAppsFolder);
                siteAppsFolderFolder.Name = "Folder";
                siteAppsFolderFolder.Save();

                // /TestRoot/(apps)/Folder/App1
            var siteAppsApp1 = new Application(siteAppsFolderFolder);
                siteAppsApp1.Name = "App1";
                siteAppsApp1.Scenario = "sc1,sc4,sc5";
                siteAppsApp1.Save();

                // /TestRoot/(apps)/Folder/App2
            var siteAppsApp2 = new Application(siteAppsFolderFolder);
                siteAppsApp2.Name = "App2";
                siteAppsApp2.Scenario = "sc1,Settings";
                siteAppsApp2.Save();

                // TestRoot/(apps)/Folder/App3
            var siteAppsApp3 = new Application(siteAppsFolderFolder);
                siteAppsApp3.Name = "App3";
                siteAppsApp3.Scenario = "sc1,sc2,sc7,sc10";
                siteAppsApp3.RequiredPermissions = "5";
                siteAppsApp3.Save();

                // TestRoot/(apps)/Folder/App4
            var siteAppsApp4 = new Application(siteAppsFolderFolder);
                siteAppsApp4.Name = "App4";
                siteAppsApp4.Scenario = "sc2,sc6,sc10";
                siteAppsApp4.RequiredPermissions = "5";
                siteAppsApp4.Save();

                //Root/TestRoot/(apps)/Folder/App5
            var siteAppsApp5 = new Application(siteAppsFolderFolder);
                siteAppsApp5.Name = "App5";
                siteAppsApp5.Scenario = "sc11";
                siteAppsApp5.RequiredPermissions = "5;10;11";
                siteAppsApp5.Save();

                //Root/TestRoot/(apps)/Folder/App6
            var siteAppsApp6 = new Application(siteAppsFolderFolder);
                siteAppsApp6.Name = "App6";
                siteAppsApp6.Scenario = "sc12";
                siteAppsApp6.Save();

                //Root/TestRoot/(apps)/Folder/App12
            var siteSampleAppsApp12 = new Application(siteAppsFolderFolder);
                siteSampleAppsApp12.Name = "App12";
                siteSampleAppsApp12.Scenario = "sc20";
                siteSampleAppsApp12.Save();

                //Root/TestRoot/(apps)/Folder/App21
            var siteSampleAppsApp21 = new Application(siteAppsFolderFolder);
                siteSampleAppsApp21.Name = "App21";
                siteSampleAppsApp21.Scenario = "sc21";
                siteSampleAppsApp21.Save();

                //Root/TestRoot/(apps)/Folder/App22
            var siteSampleAppsApp22 = new Application(siteAppsFolderFolder);
                siteSampleAppsApp22.Name = "App22";
                siteSampleAppsApp22.Scenario = "sc22";
                siteSampleAppsApp22.Save();

                //Root/TestRoot/(apps)/Folder/App23
                var siteSampleAppsApp23 = new Application(siteAppsFolderFolder);
                siteSampleAppsApp23.Name = "App23";
                siteSampleAppsApp23.Scenario = "sc23";
                siteSampleAppsApp23.Disabled = true;
                siteSampleAppsApp23.Save();

                //Root/TestRoot/(apps)/Folder/App24
                var siteSampleAppsApp24 = new Application(siteAppsFolderFolder);
                siteSampleAppsApp24.Name = "App24";
                siteSampleAppsApp24.Scenario = "sc24";
                siteSampleAppsApp24.Disabled = true;
                siteSampleAppsApp24.Clear = true;
                siteSampleAppsApp24.Save();

                //Root/TestRoot/(apps)/Folder/App25
                var siteSampleAppsApp25 = new Application(siteAppsFolderFolder);
                siteSampleAppsApp25.Name = "App25";
                siteSampleAppsApp25.Scenario = "sc25";
                siteSampleAppsApp25.Save();

                //Root/TestRoot/Sample
            var siteSample = new SystemFolder(site);
                siteSample.Name = "Sample";
                siteSample.Save();
           
            //---- ContentList
            //----------------

                //Root/TestRoot/(apps)/ContentList
            var siteAppsFolderContentList = new SystemFolder(siteAppsFolder);
                siteAppsFolderContentList.Name = "ContentList";
                siteAppsFolderContentList.Save();

                //Root/TestRoot/Sample2
            var siteSample2 = new ContentList(site);
                siteSample2.Name = "Sample2";
                siteSample2.Save();

                //Root/TestRoot/(apps)/ContentList/App1
            var siteAppsContentListOverrideApp1 = new Application(siteAppsFolderContentList);
                siteAppsContentListOverrideApp1.Name = "App1";
                siteAppsContentListOverrideApp1.Scenario = "sc4,sc5";
                siteAppsContentListOverrideApp1.Save();


                //Root/TestRoot/(apps)/ContentList/ContentListApp1
            var siteAppsContentListApp1 = new Application(siteAppsFolderContentList);
                siteAppsContentListApp1.Name = "ContentListApp1";
                siteAppsContentListApp1.Scenario = "sc2,sc10";
                siteAppsContentListApp1.Save();

                //Root/TestRoot/Apps/ContentList/ContentListApp2
            var siteAppsContentListApp2 = new Application(siteAppsFolderContentList);
                siteAppsContentListApp2.Name = "ContentListApp2";
                siteAppsContentListApp2.Scenario = "sc1";
                siteAppsContentListApp2.Save();

                //Root/TestRoot/(apps)/ContentList/ContentListApp3
            var siteAppsContentListApp3 = new Application(siteAppsFolderContentList);
                siteAppsContentListApp3.Name = "ContentListApp3";
                siteAppsContentListApp3.Scenario = "sc1,sc2,sc3,mySettings";
                siteAppsContentListApp3.Save();

                //Root/TestRoot/(apps)/ContentList/App6
            var siteAppsContentListOverrideApp6 = new Application(siteAppsFolderContentList);
                siteAppsContentListOverrideApp6.Name = "App6";
                siteAppsContentListOverrideApp6.Scenario = "sc12";
                siteAppsContentListOverrideApp6.RequiredPermissions = "11";
                siteAppsContentListOverrideApp6.Save();
            
            //---- This structure under Sample
            //--------------------------------

                //Root/TestRoot/Sample/(apps)
            var siteSampleApps = new SystemFolder(siteSample);
                siteSampleApps.Name = "(apps)";
                siteSampleApps.Save();

                //Root/TestRoot/Sample/(apps)/This
            var siteSampleAppsThis = new SystemFolder(siteSampleApps);
                siteSampleAppsThis.Name = "This";
                siteSampleAppsThis.Save();

                //Root/TestRoot/Sample/(apps)/This/App12
            var siteSampleAppsThisApp12 = new Application(siteSampleAppsThis);
                siteSampleAppsThisApp12.Name = "App12";
                siteSampleAppsThisApp12.Scenario = "sc20";
                siteSampleAppsThisApp12.Save();

                //Root/TestRoot/Sample/(apps)/Folder
            var siteSampleAppsFolder = new SystemFolder(siteSampleApps);
                siteSampleAppsFolder.Name = "Folder";
                siteSampleAppsFolder.Save();

                //Root/TestRoot/Sample/(apps)/Folder/App21
            var siteSampleAppsFolderApp21 = new Application(siteSampleAppsFolder);
                siteSampleAppsFolderApp21.Name = "App21";
                siteSampleAppsFolderApp21.Scenario = "sc21";
                siteSampleAppsFolderApp21.Save();

                //Root/TestRoot/Sample/(apps)/Folder/App22
                var siteSampleAppsFolderApp22 = new Application(siteSampleAppsFolder);
                siteSampleAppsFolderApp22.Name = "App22";
                siteSampleAppsFolderApp22.Scenario = "sc22";
                siteSampleAppsFolderApp22.Disabled = true;
                siteSampleAppsFolderApp22.Save();

                //Root/TestRoot/Sample/(apps)/Folder/App23
                var siteSampleAppsFolderApp23 = new Application(siteSampleAppsFolder);
                siteSampleAppsFolderApp23.Name = "App23";
                siteSampleAppsFolderApp23.Scenario = "sc23";
                siteSampleAppsFolderApp23.Disabled = true;
                siteSampleAppsFolderApp23.Save();

                //Root/TestRoot/Sample/(apps)/Folder/App24
                var siteSampleAppsFolderApp24 = new Application(siteSampleAppsFolder);
                siteSampleAppsFolderApp24.Name = "App24";
                siteSampleAppsFolderApp24.Scenario = "sc24";
                siteSampleAppsFolderApp24.RequiredPermissions = "5";
                siteSampleAppsFolderApp24.Save();

                //Root/TestRoot/Sample/(apps)/Folder/App25
                var siteSampleAppsFolderApp25 = new Application(siteSampleAppsFolder);
                siteSampleAppsFolderApp25.Name = "App25";
                siteSampleAppsFolderApp25.Scenario = "sc25";
                siteSampleAppsFolderApp25.Clear = true;
                siteSampleAppsFolderApp25.Save();



            
            // ---- Subfolder
            // --------------

                //Root/TestRoot/Sample/SubFolder
            var siteSampleSubFolder = new SystemFolder(siteSample);
                siteSampleSubFolder.Name = "SubFolder";
                siteSampleSubFolder.Save();

                //Root/TestRoot/Sample/SubFolder/(apps)
            var siteSampleSubFolderApps = new SystemFolder(siteSampleSubFolder);
            siteSampleSubFolderApps.Name = "(apps)";
            siteSampleSubFolderApps.Save();

            //Root/TestRoot/Sample/SubFolder/(apps)/This
            var siteSampleSubFolderAppsThis = new SystemFolder(siteSampleSubFolderApps);
            siteSampleSubFolderAppsThis.Name = "This";
            siteSampleSubFolderAppsThis.Save();

            //Root/TestRoot/Sample/SubFolder/(apps)/This/App24
            var siteSampleSubFolderAppsThisApp24 = new Application(siteSampleSubFolderAppsThis);
            siteSampleSubFolderAppsThisApp24.Name = "App24";
            siteSampleSubFolderAppsThisApp24.Scenario = "sc24";
            siteSampleSubFolderAppsThisApp24.Disabled = true;
            siteSampleSubFolderAppsThisApp24.Save();


        }
        
        [ClassCleanup]
        public static void DestroySandbox()
        {
            var site = Node.Load<Site>(_testRootPath);
            if (site != null)
                site.ForceDelete();
        }

        [TestMethod]
        public void ActionFramework_AppStorageLoad()
        {
            var nt = ActiveSchema.NodeTypes["Application"];
            var nq = new NodeQuery();
            nq.Add(new TypeExpression(nt));
            nq.Add(new StringExpression(StringAttribute.Path, StringOperator.Contains, string.Format("/{0}/", "(apps)")));
            nq.Orders.Add(new SearchOrder(StringAttribute.Path, OrderDirection.Desc));
            var result1 = nq.Execute(ExecutionHint.ForceRelationalEngine);
            var result2 = nq.Execute(ExecutionHint.ForceIndexedEngine);

            var paths1 = String.Join(Environment.NewLine, result1.Nodes.Select(n => n.Path).ToArray());
            var paths2 = String.Join(Environment.NewLine, result2.Nodes.Select(n => n.Path).ToArray());

            Assert.IsTrue(paths1 == paths2);

            //	/Root/TestSiteForActionFramework/Sample/SubFolder/(apps)/This/App24
            //	/Root/TestSiteForActionFramework/Sample/(apps)/This/App12
            //	/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App25
            //	/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App24
            //	/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App23
            //	/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App22
            //	/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App21
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App6
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App5
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App4
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App3
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App25
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App24
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App23
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App22
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App21
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App2
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App12
            //	/Root/TestSiteForActionFramework/(apps)/Folder/App1
            //	/Root/TestSiteForActionFramework/(apps)/ContentList/ContentListApp3
            //	/Root/TestSiteForActionFramework/(apps)/ContentList/ContentListApp2
            //	/Root/TestSiteForActionFramework/(apps)/ContentList/ContentListApp1
            //	/Root/TestSiteForActionFramework/(apps)/ContentList/App6
            //	/Root/TestSiteForActionFramework/(apps)/ContentList/App1

        }

        [TestMethod]
        public void ActionFramework_GetApplicationTest()
        {
            var context = Content.Load("/Root/TestSiteForActionFramework/Sample");
            var app = Node.Load<Application>("/Root/TestSiteForActionFramework/(apps)/Folder/App1");
            var receivedApp = ApplicationStorage.Instance.GetApplication("App1", context, null);

            Assert.AreEqual(app.Id, receivedApp.Id);
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAGivenScenarioTest()
        {
            var siteSample = Content.Load("/Root/TestSiteForActionFramework/Sample");

            var actions = ActionFramework.GetActions(siteSample, "sc2", "");

            var application3 = actions.Where(action => action.Name == "App3");
            var application4 = actions.Where(action => action.Name == "App4");

            Assert.IsTrue(actions.Count() == 2, "Number of returned actions doesn't match the expected value.");
            Assert.IsTrue(application3.Count() == 1, "One action related to the App3 application should be in the actions collection.");
            Assert.IsTrue(application4.Count() == 1, "One action related to the App4 application should be in the actions collection.");

        }

        [TestMethod]
        public void ActionFramework_GetActionsForANotExistingScenarioTest()
        {
            var siteSample = Content.Load("/Root/TestSiteForActionFramework/Sample");

            var actions = ActionFramework.GetActions(siteSample, "gfdhkjgjdfhkgdfghj", "");

            Assert.IsTrue(actions.Count() == 0, "The returned action collection should be empty.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAScenarioThatIsNotRelevantToTheGivenContentTest()
        {
            var siteSample = Content.Load("/Root/TestSiteForActionFramework/Sample");

            var actions = ActionFramework.GetActions(siteSample, "sc3", "");

            Assert.IsTrue(actions.Count() == 0, "The returned action collection should be empty.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAGivenScenarioForInheritedTypesTest()
        {
            var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");

            var actions = ActionFramework.GetActions(siteSample2, "sc2", "");

            var application3 = actions.Where(action => action.Name == "App3");
            var application4 = actions.Where(action => action.Name == "App4");
            var clApp1 = actions.Where(action => action.Name == "ContentListApp1");
            var clApp3 = actions.Where(action => action.Name == "ContentListApp3");

            Assert.IsTrue(actions.Count() == 4, "Number of returned actions doesn't match the expected value.");
            Assert.IsTrue(application3.Count() == 1, "One action related to the App3 application should be in the actions collection.");
            Assert.IsTrue(application4.Count() == 1, "One action related to the App4 application should be in the actions collection.");
            Assert.IsTrue(clApp1.Count() == 1, "One action related to the ContentListApp1 application should be in the actions collection.");
            Assert.IsTrue(clApp3.Count() == 1, "One action related to the ContentListApp3 application should be in the actions collection.");

        }

        [TestMethod]
        public void ActionFramework_GetActionsWithSimilarScenarioNames()
        {
            var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");

            var actions = ActionFramework.GetActions(siteSample2, "Settings", "");

            var application2 = actions.Where(action => action.Name == "App2");

            Assert.IsTrue(actions.Count() == 1, "The returned action collection should contain only one element.");
            Assert.IsTrue(application2.Count() == 1, "One action related to the App2 application should be in the actions collection.");

        }

        [TestMethod]
        public void ActionFramework_GetApplicationosWithTheSameApplicationName()
        {
            var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");

            var contentlistApp1 = Content.Load("/Root/TestSiteForActionFramework/(apps)/ContentList/App1");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App1", siteSample2, null);

            Assert.AreEqual(contentlistApp1.Id, receivedApp.Id);

        }

        [TestMethod]
        public void ActionFramework_GetApplicationsWithTheSameApplicationNameUsingThisOverride()
        {
            var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");

            var app12 = Content.Load("/Root/TestSiteForActionFramework/Sample/(apps)/This/App12");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App12", siteSample1, null);

            Assert.AreEqual(app12.Id, receivedApp.Id);

        }

        [TestMethod]
        public void ActionFramework_GetApplicationsWithNotExistingApplicationName()
        {
            var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App0", siteSample1, null);

            Assert.IsNull(receivedApp, "GetApplication should return null.");

        }

        //ki kell javitani, hogy ne dobjon exceptiont
        //[TestMethod]
        //public void GetActionsWithoutOpenPermissionTest()
        //{
            
        //    IEnumerable<ActionBase> actions;

        //    //set required permission for the test - each requirement one descriptor
        //    var pdescriptors = new List<TestEquipment.PermissionDescriptor>
        //      {
        //          new TestEquipment.PermissionDescriptor
        //              {
        //                  AffectedPath = "/Root/TestSiteForAppModelTest/(apps)/Folder/App1",
        //                  AffectedUser = User.Visitor,
        //                  PType = PermissionType.Open,
        //                  NewValue = PermissionValue.Deny
        //              }
        //      };
            
            
        //    //actions
        //    using (new TestEquipment.PermissionSetter(pdescriptors,User.Visitor))
        //    {
        //        var siteSample1 = Content.Load("/Root/TestSiteForAppModelTest/Sample");
        //        actions = ActionFramework.GetActions(siteSample1, "sc5", "");
        //    } 
            
            
        //    //asserts
        //    var app1 = actions.Where(action => action.Name == "App1");
        //    Assert.IsTrue(app1.Count() == 0, "SampleUser shouldn't have permission to get App1 application.");
        //}

        [TestMethod]
        public void ActionFramework_GetActionsWithoutSeePermissionTestA()
        {
            IEnumerable<ActionBase> actions;

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/(apps)/Folder/App1",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.See,
                          NewValue = PermissionValue.Deny
                      }
              };

            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");
                actions = ActionFramework.GetActions(siteSample1, "sc5", "");
            } 

            //asserts
            var app1 = actions.Where(action => action.Name == "App1");
            Assert.IsTrue(app1.Count() == 0, "SampleUser shouldn't have permission to get App1 application.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsWithoutSeePermissionTestB()
        {
            IEnumerable<ActionBase> actions;
            
            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/(apps)/Folder/App1",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.See,
                          NewValue = PermissionValue.Deny
                      },
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/(apps)/ContentList/App1",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.See,
                          NewValue = PermissionValue.Deny
                      }
              };


            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");
                actions = ActionFramework.GetActions(siteSample2, "sc5", "");
            }
            
            //asserts
            var app1 = actions.Where(action => action.Name == "App1");
            Assert.IsTrue(app1.Count() == 0, "SampleUser shouldn't have permission to get App1 application.");

        }

        [TestMethod]
        public void ActionFramework_GetActionsWithoutRunApplicationPermissionTest()
        {
            IEnumerable<ActionBase> actions;

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/(apps)/Folder/App3",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.RunApplication,
                          NewValue = PermissionValue.Deny
                      }
              };


            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");
                actions = ActionFramework.GetActions(siteSample1, "sc7", "");
            }
            

            //asserts
            var app3 = actions.Where(action => action.Name == "App3");
            Assert.IsTrue(app3.Count() == 1, "App3 should be returned (with true Forbidden attribute).");
            Assert.IsTrue(app3.First().Forbidden, "The Forbidden attribute of App3 should be true");
            
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAContentWithoutSufficientPermissionsATest()
        {
            IEnumerable<ActionBase> actions;
            
            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Publish,
                          NewValue = PermissionValue.Deny
                      }
              };


            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");
                actions = ActionFramework.GetActions(siteSample1, "sc6", "");
            }
            
            //asserts
            var app4 = actions.Where(action => action.Name == "App4");
            Assert.IsTrue(app4.Count() == 1, "App4 should be returned (with true Forbidden attribute).");
            Assert.IsTrue(app4.First().Forbidden, "The Forbidden attribute of App4 should be true.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAContentWithoutSufficientPermissionsBTest()
        {
            IEnumerable<ActionBase> actions;

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample2",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Publish,
                          NewValue = PermissionValue.Deny
                      }
              };

            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");
                actions = ActionFramework.GetActions(siteSample2, "sc10", "");
            }
            
            //asserts
            var app3 = actions.Where(action => action.Name == "App3");
            var app4 = actions.Where(action => action.Name == "App4");
            var clapp1 = actions.Where(action => action.Name == "ContentListApp1");
            Assert.IsTrue(actions.Count() == 3, "GetActions should return 3 actions.");
            Assert.IsTrue(app3.Count() == 1, "One action related to the App3 application should be in the actions collection.");
            Assert.IsTrue(app4.Count() == 1, "One action related to the App4 application should be in the actions collection.");
            Assert.IsTrue(clapp1.Count() == 1, "One action related to the ContentListApp1 application should be in the actions collection.");
            Assert.IsTrue(app3.First().Forbidden, "App3 should not be Forbidden.");
            Assert.IsTrue(app4.First().Forbidden, "App4 should not be Forbidden.");
            Assert.IsTrue(clapp1.First().Forbidden == false, "ContentListApp1 should not be Forbidden.");
            
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAContentWithSufficientPermissionsTest()
        {
            IEnumerable<ActionBase> actions;
            
            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample2",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Publish,
                          NewValue = PermissionValue.Allow
                      }
              };

            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");
                actions = ActionFramework.GetActions(siteSample2, "sc10", "");
            }

            //asserts
            var app3 = actions.Where(action => action.Name == "App3");
            var app4 = actions.Where(action => action.Name == "App4");
            var clapp1 = actions.Where(action => action.Name == "ContentListApp1");
            Assert.IsTrue(actions.Count() == 3, "GetActions should return 3 actions.");
            Assert.IsTrue(app3.Count() == 1, "One action related to the App3 application should be in the actions collection.");
            Assert.IsTrue(app4.Count() == 1, "One action related to the App4 application should be in the actions collection.");
            Assert.IsTrue(clapp1.Count() == 1, "One action related to the ContentListApp1 application should be in the actions collection.");
            Assert.IsTrue(app3.First().Forbidden == false, "App3 should not be Forbidden.");
            Assert.IsTrue(app4.First().Forbidden == false, "App4 should not be Forbidden.");
            Assert.IsTrue(clapp1.First().Forbidden == false, "ContentListApp1 should not be Forbidden.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAContentMultiplePermissionsRequiered()
        {
            IEnumerable<ActionBase> actions;
            
            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Publish,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.RecallOldVersion,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.DeleteOldVersion,
                          NewValue = PermissionValue.Allow
                      }
              };

            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");
                actions = ActionFramework.GetActions(siteSample1, "sc11", "");
            }
            
            //asserts
            var app5 = actions.Where(action => action.Name == "App5");
            Assert.IsTrue(actions.Count() == 1, "GetActions should return one action.");
            Assert.IsTrue(app5.Count() == 1, "One action related to the App5 application should be in the actions collection.");
            Assert.IsTrue(app5.First().Forbidden == false, "App5 should not be Forbidden.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsForAContentMultiplePermissionsRequieredButNot()
        {
            IEnumerable<ActionBase> actions;

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.Publish,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.RecallOldVersion,
                          NewValue = PermissionValue.Allow
                      },
                      new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.DeleteOldVersion,
                          NewValue = PermissionValue.Deny
                      }
              };


            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample1 = Content.Load("/Root/TestSiteForActionFramework/Sample");
                actions = ActionFramework.GetActions(siteSample1, "sc11", "");
            }

            //asserts
            var app5 = actions.Where(action => action.Name == "App5");
            Assert.IsTrue(actions.Count() == 1, "GetActions should return one action.");
            Assert.IsTrue(app5.Count() == 1, "One action related to the App5 application should be in the actions collection.");
            Assert.IsTrue(app5.First().Forbidden, "App5 should be Forbidden.");
        }

        [TestMethod]
        public void ActionFramework_GetActionsWithTheSameScenarioWithTheSameApplicationNamesWithPermissionTest()
        {
            IEnumerable<ActionBase> actions;

            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample2",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.DeleteOldVersion,
                          NewValue = PermissionValue.Deny
                      }
              };


            var app6 = Content.Load("/Root/TestSiteForActionFramework/(apps)/ContentList/App6");
            Application receivedApp;
            
            //actions
            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {
                var siteSample2 = Content.Load("/Root/TestSiteForActionFramework/Sample2");

                receivedApp = ApplicationStorage.Instance.GetApplication("App6", siteSample2, null);
                actions = ActionFramework.GetActions(siteSample2, "sc12", "");
             
            }

            Assert.AreEqual(app6.Id, receivedApp.Id, "Not the most specific App6 were returned (ContentList's App6 should be returned).");

            //asserts
            var app6Action = actions.Where(action => action.Name == "App6");
            Assert.IsTrue(app6Action.Count() == 1, "One action related to the App6 application should be in the actions collection.");
            Assert.IsTrue(app6Action.First().Forbidden, "App6 should be Forbidden.");

            

        }

        [TestMethod]
        public void ActionFramework_GetTheMostSpecificApplication()
        {
            var subFolder = Content.Load("/Root/TestSiteForActionFramework/Sample/SubFolder");

            var app21 = Content.Load("/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App21");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App21", subFolder, null);

            Assert.AreEqual(app21.Id, receivedApp.Id,"Not the expected application were returned");
        }

        [TestMethod]
        public void ActionFramework_GetTheMostSpecificApplicationUsingDisabled()
        {
            var subFolder = Content.Load("/Root/TestSiteForActionFramework/Sample/SubFolder");

            var app22 = Content.Load("/Root/TestSiteForActionFramework/(apps)/Folder/App22");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App22", subFolder, null);

            Assert.AreEqual(app22.Id, receivedApp.Id, "Not the expected application were returned.");

        }

        [TestMethod]
        public void ActionFramework_GetTheMostSpecificApplicationAllDisabled()
        {
            var subFolder = Content.Load("/Root/TestSiteForActionFramework/Sample/SubFolder");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App23", subFolder, null);

            Assert.IsNull(receivedApp,"We shouldn't receive any application.");

        }

        [TestMethod]
        public void ActionFramework_GetTheMostSpecificApplicationUsingClear()
        {
            var subFolder = Content.Load("/Root/TestSiteForActionFramework/Sample/SubFolder");

            var receivedApp = ApplicationStorage.Instance.GetApplication("App25", subFolder, null);

            Assert.IsNull(receivedApp, "We shouldn't receive any application.");

        }

        [TestMethod]
        public void ActionFramework_GetTheMostSpecificApplication2()
        {
            IEnumerable<ActionBase> actions;
            Application application;
            
            //set required permission for the test - each requirement one descriptor
            var pdescriptors = new List<TestEquipment.PermissionDescriptor>
              {
                  new TestEquipment.PermissionDescriptor
                      {
                          AffectedPath = "/Root/TestSiteForActionFramework/Sample/SubFolder",
                          AffectedUser = User.Visitor,
                          PType = PermissionType.RecallOldVersion,
                          NewValue = PermissionValue.Deny
                      }
              };

            using (new TestEquipment.ContextSimulator(pdescriptors, User.Visitor))
            {

                var subFolder = Content.Load("/Root/TestSiteForActionFramework/Sample/SubFolder");
                actions = ActionFramework.GetActions(subFolder, "sc24", "");
                application = ApplicationStorage.Instance.GetApplication("App24", subFolder, null);
            }

            var action = actions.SingleOrDefault(act => act.Name == "App24");

            Assert.IsTrue(actions.Count() == 1,"Only one application should be returned.");
            Assert.IsNotNull(action,"App24 should be in the resultset.");
            Assert.IsTrue(application.Path == "/Root/TestSiteForActionFramework/Sample/(apps)/Folder/App24");
            Assert.IsTrue(action.Forbidden,"App24 should be forbidden.");
            
        }

    }
}
