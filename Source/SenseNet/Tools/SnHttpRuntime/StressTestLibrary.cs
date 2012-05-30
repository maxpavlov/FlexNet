using System;
using System.Collections.Generic;
using SenseNet.Utilities.ExecutionTesting;
using ConcurrencyTester.Jobs;
using ConcurrencyTester.JobLibrary;
using System.Threading;
using System.Linq;

namespace ConcurrencyTester
{
    
    public static class StressTestLibrary
    {
        // --- Paths
        // ---------

        internal static readonly Dictionary<string, string> Paths = new Dictionary<string, string>()
        {
            {"siteroot", "/Root/Sites/Default_Site"},
            {"page1","/Root/Sites/Default_Site/(apps)/This/Browse"},
            {"page2","/Root/Sites/Default_Site/NewsDemo/(apps)/This/Browse"},
            {"page3","/Root/Sites/Default_Site/features/event-calendar"},
            {"page4","/Root/Sites/Default_Site/features/contentcollection-customization"},
            {"page5","/Root/Sites/Default_Site/features/search"},
            {"page6","/Root/Sites/Default_Site/workspaces/(apps)/This/Browse"},
            {"page7","/Root/Sites/Default_Site/features/menu"},
            {"page8","/Root/Sites/Default_Site/features/form"},
            {"page9","/Root/Sites/Default_Site/features/gallery"},
            {"pdf1","/Root/YourDocuments/PressRoom/SenseNet6-Logos.pdf"},
            {"image1","/Root/YourContents/Images/newlist.png"},
            {"list1", "/Root/Sites/Default_Site/workspaces/Document/romedocumentworkspace/Document_Library"},
            {"list2", "/Root/Sites/Default_Site/workspaces/Document/londondocumentworkspace/Document_Library"}
        };

        internal static readonly Dictionary<string, string> WebFolderPaths = new Dictionary<string, string>()
        {
            {"web1", @"C:\test\web1"},
            {"web2", @"C:\test\web2"}
        };

        internal static StressTest GetTest(string testSelector)
        {
            switch (testSelector)
            {
                case "1":
                    return MultiDomain_1();
                case "2":
                    return MultiDomain_2();
                case "3":
                    return MultiDomain_3();
                case "4":
                    return MultiDomain_4();
                case "5":
                    return MultiDomain_5();
                default:
                    throw new Exception("The requested test doesn't exist.");
            }
        }

        
        //Singledomain tests


        
        // Multidomain tests
        internal static StressTest MultiDomain_1()
        {
            return new StressTest("MultiDomain_1", "First multidomain test",
                                  new List<JobManager>()
                                      {
                                          JobManager1(),
                                          JobManager2()
                                      }
                                  );
        }
        internal static StressTest MultiDomain_2()
        {
            return new StressTest("MultiDomain_2", "Sec multidomain test",
                                  new List<JobManager>()
                                      {
                                          JobManager3(),
                                          JobManager4()
                                      }
                                  );
        }
        internal static StressTest MultiDomain_3()
        {
            return new StressTest("MultiDomain_3", "Sec multidomain test",
                                  new List<JobManager>()
                                      {
                                          JobManager5(),
                                          JobManager6()
                                      }
                                  );
        }
        internal static StressTest MultiDomain_4()
        {
            return new StressTest("MultiDomain_4", "Sec multidomain test",
                                  new List<JobManager>()
                                      {
                                          JobManager7(),
                                          JobManager8()
                                      }
                                  );
        }
        internal static StressTest MultiDomain_5()
        {
            return new StressTest("MultiDomain_5", "Sec multidomain test",
                                  new List<JobManager>()
                                      {
                                          JobManager9(),
                                          JobManager10()
                                      }
                                  );
        }

        // JobManagers
        internal static JobManager JobManager1()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web1"], "localhost", Console.Out, "JobManager1");
            
            manager.AddJob(Save_300_Administrator_page1());

            return manager;
        }

        internal static JobManager JobManager2()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web2"], "localhost", Console.Out, "JobManager2");

            manager.AddJob(Save_350_Administrator_page2());

            return manager;
        }

        internal static JobManager JobManager3()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web1"], "localhost", Console.Out, "JobManager3");

            manager.AddJob(Save_300_Administrator_page1());
            manager.AddJob(CheckOutCheckIn_110_Administrator_page5());

            return manager;
        }

        internal static JobManager JobManager4()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web2"], "localhost", Console.Out, "JobManager4");

            manager.AddJob(Save_350_Administrator_page2());
            manager.AddJob(CheckOutCheckIn_500_Administrator_page1());

            return manager;
        }
        internal static JobManager JobManager5()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web1"], "localhost", Console.Out, "JobManager5");

            manager.AddJob(Save_300_Administrator_page1());
            manager.AddJob(CheckOutCheckIn_110_Administrator_page5());
            manager.AddJob(Query_Page_200_Administrator());

            return manager;
        }

        internal static JobManager JobManager6()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web2"], "localhost", Console.Out, "JobManager6");

            manager.AddJob(Save_350_Administrator_page2());
            manager.AddJob(CheckOutCheckIn_500_Administrator_page1());
            manager.AddJob(Query_Page_140_Administrator());

            return manager;
        }
        internal static JobManager JobManager7()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web1"], "localhost", Console.Out, "JobManager7");

            manager.AddJob(Save_300_Administrator_page1());
            manager.AddJob(CheckOutCheckIn_110_Administrator_page5());
            manager.AddJob(Query_Page_200_Administrator());

            return manager;
        }

        internal static JobManager JobManager8()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web2"], "localhost", Console.Out, "JobManager8");

            manager.AddJob(Save_350_Administrator_page2());
            manager.AddJob(CheckOutCheckIn_1200_Administrator_page4());
            manager.AddJob(Query_Page_140_Administrator());
            manager.AddJob(Publish_40_Administrator_page6());

            return manager;
        }

        internal static JobManager JobManager9()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web1"], "localhost", Console.Out, "JobManager9");

            manager.AddJob(CheckOutCheckIn_50_Administrator_page1());
            manager.AddJob(CheckOutCheckIn_110_Administrator_page5());
            manager.AddJob(Save_10_Administrator_page7());
            manager.AddJob(Query_Page_20_Administrator());
            manager.AddJob(Publish_77_Administrator_page3());


            return manager;
        }

        internal static JobManager JobManager10()
        {
            var manager = new HostedJobManagerMSMQ(WebFolderPaths["web2"], "localhost", Console.Out, "JobManager10");

            manager.AddJob(CheckOutCheckIn_50_Administrator_page6());
            manager.AddJob(Save_15_Administrator_page8());
            manager.AddJob(Query_Page_20_Administrator());
            manager.AddJob(Query_Page_50_Visitor());
            manager.AddJob(Save_350_Administrator_page2());

            return manager;
        }
        
        //Jobs
        internal static NodeSaveJob Save_300_Administrator_page1()
        {
            return new NodeSaveJob("Save_page1_300_Administrator", Paths["page1"], Console.Out)
            {
                SleepTime = 300,
                MaxIterationCount = 600,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_350_Administrator_page2()
        {
            return new NodeSaveJob("Save_page2_350_Administrator", Paths["page2"], Console.Out)
            {
                SleepTime = 350,
                MaxIterationCount = 600,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_10_Administrator_page7()
        {
            return new NodeSaveJob("Save_page7_10_Administrator", Paths["page7"], Console.Out)
            {
                SleepTime = 10,
                MaxIterationCount = 900,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_15_Administrator_page8()
        {
            return new NodeSaveJob("Save_page8_15_Administrator", Paths["page8"], Console.Out)
            {
                SleepTime = 15,
                MaxIterationCount = 900,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
                                                                       
    //    internal static Dictionary<string, IEnumerable<Job>> GetTest(string testSelector)
    //    {
    //        switch (testSelector)
    //        {
    //            case "test1":
    //                return Test1();
                    
    //            case "test2":
    //                return Test2();
                
    //            case "test3":
    //                return Test3();

    //            case "test3d2":
    //                return Test3D2();

    //            case "test4":
    //                return Test4();

    //            case "test5":
    //                return Test5();

    //            case "test5a":
    //                return Test5a();

    //            case "test5d2":
    //                return Test5D2();
                
    //            case "test6":
    //                return Test6();
                
    //            case "test7":
    //                return Test7();

    //            case "test7d2":
    //                return Test7D2();
                
    //            case "test8":
    //                return Test8();

    //            case "test9":
    //                return Test9();

    //            case "test9d2":
    //                return Test9D2();
                
    //            case "quicktest1":
    //                return QuickTest1();

    //            case "quicktest1d2":
    //                return QuickTest1D2();

    //            case "quicktest2":
    //                return QuickTest2();
                
    //            case "quicktest2d2":
    //                return QuickTest2D2();

    //            case "test10":
    //                return Test10();

    //            case "test10d2":
    //                return Test10D2();

    //            case "test11":
    //                return Test11();

    //            case "test20":
    //                return Test20();

    //            case "test21":
    //                return Test21();

    //            case "test22":
    //                return Test22();

    //            case "test23":
    //                return Test23();

    //            case "test24d2":
    //                return Test24D2();

    //            case "test25d3":
    //                return Test25D3();

    //            case "test26d3":
    //                return Test26D3();

    //            case "test27d3":
    //                return Test27D3();
                
    //            case "bug5982_1":
    //                return Bug5982_1();

    //            case "bug5982_2":
    //                return Bug5982_2();

    //            case "bug5982_3":
    //                return Bug5982_3();
                
    //            case "bug5982_d2_1":
    //                return Bug5982_D2_1();

    //            case "bug5982_d2_2":
    //                return Bug5982_D2_2();

    //            case "undo1_1":
    //                return Undo1_1();

    //            case "undo1_2":
    //                return Undo1_2();

    //            case "undo2_1":
    //                return Undo2_1();

    //            case "undo2_2":
    //                return Undo2_2();

    //            case "undo3_1":
    //                return Undo3_1();

    //            case "undo3_2":
    //                return Undo3_2();
    //            case "bug5982_3_noversioning":
    //                return Bug5982_3_noversioning();
    //            case "bug5982_3_noversioning_1":
    //                return Bug5982_3_noversioning_1();
    //            case "bug5982_3_noversioning_2":
    //                return Bug5982_3_noversioning_2();

    //            case "addfield1":
    //                return AddField1();
    //            case "addfield2":
    //                return AddField2();

    //            default:
    //                throw new Exception("The requested test doesn't exist.");
                     
                
    //        }
    //    }

    //    // --- Complete tests
    //    // ------------------
    //    internal static Dictionary<string, IEnumerable<Job>> Test1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Visitor(),
    //                                     Query_Page_200_Visitor()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Save_500_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test3()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_140_Administrator(),
    //                                     Save_900_Administrator_page3(),
    //                                     Save_500_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test3D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Administrator(),
    //                                     Save_500_Administrator_page1()
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Save_900_Administrator_page3(),
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test4()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     CheckOutCheckIn_500_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test5()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     //Query_Page_200_Visitor(),
    //                                     //Query_Page_140_Administrator(),
    //                                     CheckOutCheckIn_900_Administrator_page3(),
    //                                     CheckOutCheckIn_500_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test5a()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     //Query_Page_200_Visitor(),
    //                                     //Query_Page_140_Administrator(),

    //                                     //CheckOutCheckIn_60_Administrator_page3(),
    //                                     CheckOutCheckIn_40_Administrator_page7(),
    //                                     CheckOutCheckIn_50_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test5D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Administrator(),
    //                                     CheckOutCheckIn_900_Administrator_page3(),
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     CheckOutCheckIn_500_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test6()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Administrator(),
    //                                     CheckOutCheckIn_500_Administrator_page1()
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     CheckOutCheckIn_1200_Administrator_page4()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test7()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_140_Administrator(),
    //                                     Query_Page_500_Visitor(),
    //                                     CheckOutCheckIn_900_Administrator_page3(),
    //                                     CheckOutCheckIn_500_Administrator_page1(),
    //                                     CheckOutCheckIn_1200_Administrator_page4()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test7D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Administrator(),
    //                                     CheckOutCheckIn_900_Administrator_page3(),
    //                                     CheckOutCheckIn_1200_Administrator_page4()
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_500_Visitor(),
    //                                     CheckOutCheckIn_500_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test8()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_20_Administrator(),
    //                                     CheckOutCheckIn_110_Administrator_page5()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test9D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Visitor(),
    //                                     Query_Page_500_Visitor(),
    //                                     CheckOutCheckIn_900_Administrator_page3(),
    //                                     CheckOutCheckIn_500_Administrator_page1(),
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_140_Administrator(),
    //                                     CheckOutCheckIn_1200_Administrator_page4(),
    //                                     CheckOutCheckIn_700_Administrator_page2()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test9()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Visitor(),
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_140_Administrator(),
    //                                     Query_Page_500_Visitor(),
    //                                     CheckOutCheckIn_900_Administrator_page3(),
    //                                     CheckOutCheckIn_500_Administrator_page1(),
    //                                     CheckOutCheckIn_1200_Administrator_page4(),
    //                                     CheckOutCheckIn_700_Administrator_page2()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> QuickTest1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_20_Administrator(),
    //                                     Query_Page_20_Visitor(),
    //                                     CheckOutCheckIn_500_Administrator_page1(),
    //                                     CheckOutCheckIn_110_Administrator_page5()
    //                                 });
    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> QuickTest1D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_20_Visitor(),
    //                                     CheckOutCheckIn_110_Administrator_page5()
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_20_Administrator(),
    //                                     CheckOutCheckIn_500_Administrator_page1(),
    //                                 });
    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> QuickTest2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_20_Administrator(),
    //                                     CheckOutCheckIn_50_Administrator_page6(),
    //                                     Query_Page_20_Visitor(),
    //                                     CheckOutCheckIn_40_Administrator_page7()
    //                                 });
    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> QuickTest2D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_20_Administrator(),
    //                                     CheckOutCheckIn_50_Administrator_page6()
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_20_Visitor(),
    //                                     CheckOutCheckIn_40_Administrator_page7()
    //                                 });

    //        return test;

    //    }
    //    // ---> required: approving and not major minor
    //    // -----------------------------------------
    //    internal static Dictionary<string, IEnumerable<Job>> Test20()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_140_Administrator(),
    //                                     Approve_310_Administrator_page1(false),
    //                                     Approve_505_Administrator_page2(false)
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test21()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_50_Visitor(),
    //                                     Approve_90_Administrator_page5(false),
    //                                     Approve_310_Administrator_page1(false),
    //                                     Reject_420_Administrator_page2(false)
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test22()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_50_Visitor(),
    //                                     Query_Page_20_Visitor(),
    //                                     Approve_90_Administrator_page5(false),
    //                                     Approve_37_Administrator_page6(false),
    //                                     Reject_20_Administrator_page7(false)
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test26D3()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_20_Visitor(),
    //                                     CheckOutCheckIn_110_Administrator_page5(),
    //                                     Approve_505_Administrator_page2(false)
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_90_Administrator(),
    //                                     Reject_710_Administrator_page3(false)
    //                                 });

    //        test.Add("manager3", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_500_Visitor(),
    //                                     CheckOutCheckIn_1200_Administrator_page4(),
    //                                     /*Reject_210_Administrator_image1(false)*/
    //                                 });

    //        return test;

    //    }

    //    // ---> required: approving and majorminor versioning
    //    // ---------------------------------------------
    //    internal static Dictionary<string, IEnumerable<Job>> Test10()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Query_Page_140_Administrator(),
    //                                     Publish_890_Administrator_page3(),
    //                                     Publish_300_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test10D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_140_Administrator(),
    //                                     Publish_890_Administrator_page3(),
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_200_Visitor(),
    //                                     Publish_300_Administrator_page1()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test11()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_20_Visitor(),
    //                                     Query_Page_50_Administrator(),
    //                                     Publish_77_Administrator_page7(),
    //                                     Publish_100_Administrator_page5()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test23()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_50_Visitor(),
    //                                     Query_Page_20_Visitor(),
    //                                     Approve_90_Administrator_page5(true),
    //                                     Approve_37_Administrator_page6(true),
    //                                     Reject_20_Administrator_page7(true),
    //                                     Publish_300_Administrator_page1(),
    //                                     Publish_510_Administrator_page2()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test24D2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_20_Visitor(),
    //                                     Publish_300_Administrator_page1(),
    //                                     Approve_505_Administrator_page2(true)
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_50_Visitor(),
    //                                     Approve_90_Administrator_page5(true),
    //                                     Reject_710_Administrator_page3(true)
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test25D3()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_20_Visitor(),
    //                                     Publish_300_Administrator_page1(),
    //                                     Approve_505_Administrator_page2(true)
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_90_Administrator(),
    //                                     Reject_710_Administrator_page3(true)
    //                                 });

    //        test.Add("manager3", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_500_Visitor(),
    //                                     CheckOutCheckIn_1200_Administrator_page4(),
    //                                     Publish_100_Administrator_page5()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Test27D3()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Query_Page_90_Administrator(),
    //                                     Query_Page_20_Visitor(),
    //                                     Publish_300_Administrator_page1(),
    //                                     Approve_505_Administrator_page2(true)
    //                                 });

    //        test.Add("manager2", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_90_Administrator(),
    //                                     Reject_210_Administrator_image1(true),
    //                                     Reject_710_Administrator_page3(true)
    //                                 });

    //        test.Add("manager3", new List<Job>
    //                                 {
    //                                     Query_Page_50_Administrator(),
    //                                     Query_Page_20_Visitor(),
    //                                     CheckOutCheckIn_1200_Administrator_page4(),
    //                                     Publish_100_Administrator_page5()
    //                                 });

    //        return test;

    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     CheckOutCheckIn_40_iluguy_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Publish_50_Administrator_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_3()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                    CheckOutCheckIn_40_iluguy_page1(),
    //                                    Publish_50_Administrator_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_3_noversioning()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                    CheckOutCheckIn_40_iluguy_page1(),
    //                                    CheckOutCheckIn_80_hxn_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_3_noversioning_1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                    CheckOutCheckIn_80_hxn_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_3_noversioning_2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                    CheckOutCheckIn_110_iluguy_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_D2_1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     CheckOutCheckIn_40_iluguy_page1(),
    //                                     Publish_30_robspace_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Bug5982_D2_2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     Publish_50_Administrator_page1(),
    //                                     CheckOutCheckIn_80_hxn_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Undo1_1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     CheckOutCheckIn_40_iluguy_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Undo1_2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     UndoCheckout_60_hxn_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Undo2_1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     CheckOutCheckIn_40_iluguy_page1(),
    //                                     Publish_50_Administrator_page1()

    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Undo2_2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     UndoCheckout_60_hxn_page1(),
    //                                     Publish_30_robspace_page1()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Undo3_1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     CheckOutCheckIn_220_hxn_page1(),
    //                                     Publish_150_Administrator_page1()

    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> Undo3_2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     UndoCheckout_200_robspace_page1(),
    //                                     CheckOutCheckIn_230_iluguy_page1()
    //                                 });

    //        return test;
    //    }

    //    internal static Dictionary<string, IEnumerable<Job>> AddField1()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     FieldJobLibrary.AddFieldJob_List1_Administrator(),
    //                                     FieldJobLibrary.AddFieldJob_List1_ilguy()
    //                                 });

    //        return test;
    //    }
    //    internal static Dictionary<string, IEnumerable<Job>> AddField2()
    //    {
    //        var test = new Dictionary<string, IEnumerable<Job>>();

    //        test.Add("manager1", new List<Job>
    //                                 {
    //                                     FieldJobLibrary.AddFieldJob_List1_Administrator(),
    //                                     FieldJobLibrary.AddFieldJob_List1_ilguy(),
    //                                     FieldJobLibrary.AddFieldJob_List2_hxn(),
    //                                     FieldJobLibrary.AddFieldJob_List2_robspace()
    //                                 });

    //        return test;
    //    }
       

    //    // --- Jobs
    //    // --------

    //    // --- Queries
    //    // -----------
        internal static NodeQueryJob Query_Page_20_Administrator()
        {
            return new NodeQueryJob("Query_Page_20_Administrator", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 20,
                MaxIterationCount = 700,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeQueryJob Query_Page_50_Administrator()
        {
            return new NodeQueryJob("Query_Page_50_Administrator", Paths["siteroot"], Console.Out,
                                        "Page")
                           {
                               SleepTime = 50,
                               MaxIterationCount = 600,
                               UserName = "Administrator",
                               Domain = "BuiltIn"
                           };
        }
        internal static NodeQueryJob Query_Page_90_Administrator()
        {
            return new NodeQueryJob("Query_Page_90_Administrator", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 90,
                MaxIterationCount = 400,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeQueryJob Query_Page_140_Administrator()
        {
            return new NodeQueryJob("Query_Page_140_Administrator", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 140,
                MaxIterationCount = 200,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeQueryJob Query_Page_200_Administrator()
        {
            return new NodeQueryJob("Query_Page_200_Administrator", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 200,
                MaxIterationCount = 180,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeQueryJob Query_Page_500_Administrator()
        {
            return new NodeQueryJob("Query_Page_500_Administrator", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 500,
                MaxIterationCount = 100,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeQueryJob Query_Page_20_Visitor()
        {
            return new NodeQueryJob("Query_Page_20_Visitor", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 20,
                MaxIterationCount = 700
            };
        }
        internal static NodeQueryJob Query_Page_50_Visitor()
        {
            return new NodeQueryJob("Query_Page_50_Visitor", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 50,
                MaxIterationCount = 600
            };
        }
        internal static NodeQueryJob Query_Page_90_Visitor()
        {
            return new NodeQueryJob("Query_Page_90_Visitor", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 90,
                MaxIterationCount = 400
            };
        }
        internal static NodeQueryJob Query_Page_140_Visitor()
        {
            return new NodeQueryJob("Query_Page_140_Visitor", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 140,
                MaxIterationCount = 200
            };
        }
        internal static NodeQueryJob Query_Page_200_Visitor()
        {
            return new NodeQueryJob("Query_Page_200_Visitor", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 200,
                MaxIterationCount = 180
            };
        }
        internal static NodeQueryJob Query_Page_500_Visitor()
        {
            return new NodeQueryJob("Query_Page_500_Visitor", Paths["siteroot"], Console.Out,
                                        "Page")
            {
                SleepTime = 500,
                MaxIterationCount = 100
            };
        }
   

    //    // --- CheckInCheckOut
    //    // -------------------
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_500_Administrator_page1()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page1_500_Administrator", Paths["page1"], Console.Out)
            {
                SleepTime = 500,
                MaxIterationCount = 500,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_700_Administrator_page2()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page2_700_Administrator", Paths["page2"], Console.Out)
            {
                SleepTime = 700,
                MaxIterationCount = 28,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_900_Administrator_page3()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page3_900_Administrator", Paths["page3"], Console.Out)
            {
                SleepTime = 900,
                MaxIterationCount = 22,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_1200_Administrator_page4()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page4_1200_Administrator", Paths["page4"], Console.Out)
            {
                SleepTime = 1200,
                MaxIterationCount = 16,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_110_Administrator_page5()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page5_110_Administrator", Paths["page5"], Console.Out)
            {
                SleepTime = 110,
                MaxIterationCount = 500,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_50_Administrator_page6()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page6_50_Administrator", Paths["page6"], Console.Out)
            {
                SleepTime = 50,
                MaxIterationCount = 200,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_40_Administrator_page7()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page7_40_Administrator", Paths["page7"], Console.Out)
            {
                SleepTime = 40,
                MaxIterationCount = 220,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_50_Administrator_page1()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page1_50_Administrator", Paths["page1"], Console.Out)
            {
                SleepTime = 50,
                MaxIterationCount = 100,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeCheckOutCheckInJob CheckOutCheckIn_60_Administrator_page3()
        {
            return new NodeCheckOutCheckInJob("CheckOutCheckIn_page3_900_Administrator", Paths["page3"], Console.Out)
            {
                SleepTime = 60,
                MaxIterationCount = 80,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }

    //    // --- NodePublish
    //    // -------------------
        internal static NodePublishJob Publish_300_Administrator_page1()
        {
            return new NodePublishJob("Publish_page1_300_Administrator", Paths["page1"], Console.Out)
            {
                SleepTime = 300,
                MaxIterationCount = 40,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodePublishJob Publish_510_Administrator_page2()
        {
            return new NodePublishJob("Publish_page2_510_Administrator", Paths["page2"], Console.Out)
            {
                SleepTime = 510,
                MaxIterationCount = 28,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodePublishJob Publish_890_Administrator_page3()
        {
            return new NodePublishJob("Publish_page3_890_Administrator", Paths["page3"], Console.Out)
            {
                SleepTime = 890,
                MaxIterationCount = 20,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodePublishJob Publish_1300_Administrator_page4()
        {
            return new NodePublishJob("Publish_page4_1300_Administrator", Paths["page4"], Console.Out)
            {
                SleepTime = 1300,
                MaxIterationCount = 14,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodePublishJob Publish_100_Administrator_page5()
        {
            return new NodePublishJob("Publish_page5_100_Administrator", Paths["page5"], Console.Out)
            {
                SleepTime = 100,
                MaxIterationCount = 25,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodePublishJob Publish_77_Administrator_page3()
        {
            return new NodePublishJob("Publish_page3_77_Administrator", Paths["page3"], Console.Out)
            {
                SleepTime = 77,
                MaxIterationCount = 600,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodePublishJob Publish_40_Administrator_page6()
        {
            return new NodePublishJob("Publish_page6_40_Administrator", Paths["page6"], Console.Out)
            {
                SleepTime = 40,
                MaxIterationCount = 700,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }

    //    // --- NodeApprove
    //    // ---------------
        internal static NodeApproveJob Approve_310_Administrator_page1(bool usePublish)
        {
            return new NodeApproveJob("Approve_page1_310_Administrator", Paths["page1"], Console.Out, usePublish)
            {
                SleepTime = 310,
                MaxIterationCount = 40,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeApproveJob Approve_505_Administrator_page2(bool usePublish)
        {
            return new NodeApproveJob("Approve_page2_505_Administrator", Paths["page2"], Console.Out, usePublish)
            {
                SleepTime = 505,
                MaxIterationCount = 28,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeApproveJob Approve_870_Administrator_page3(bool usePublish)
        {
            return new NodeApproveJob("Approve_page3_890_Administrator", Paths["page3"], Console.Out, usePublish)
            {
                SleepTime = 870,
                MaxIterationCount = 20,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeApproveJob Approve_1285_Administrator_page4(bool usePublish)
        {
            return new NodeApproveJob("Approve_page4_1285_Administrator", Paths["page4"], Console.Out, usePublish)
            {
                SleepTime = 1285,
                MaxIterationCount = 14,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeApproveJob Approve_90_Administrator_page5(bool usePublish)
        {
            return new NodeApproveJob("Approve_page5_90_Administrator", Paths["page5"], Console.Out, usePublish)
            {
                SleepTime = 90,
                MaxIterationCount = 25,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeApproveJob Approve_37_Administrator_page6(bool usePublish)
        {
            return new NodeApproveJob("Approve_page6_37_Administrator", Paths["page6"], Console.Out, usePublish)
            {
                SleepTime = 37,
                MaxIterationCount = 35,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }

    //    // --- NodeReject
    //    // --------------
        internal static NodeRejectJob Reject_270_Administrator_page1(bool usePublish)
        {
            return new NodeRejectJob("Reject_page1_270_Administrator", Paths["page1"], Console.Out, usePublish)
            {
                SleepTime = 270,
                MaxIterationCount = 50,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_420_Administrator_page2(bool usePublish)
        {
            return new NodeRejectJob("Reject_page2_420_Administrator", Paths["page2"], Console.Out, usePublish)
            {
                SleepTime = 420,
                MaxIterationCount = 31,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_710_Administrator_page3(bool usePublish)
        {
            return new NodeRejectJob("Reject_page3_710_Administrator", Paths["page3"], Console.Out, usePublish)
            {
                SleepTime = 710,
                MaxIterationCount = 23,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_1210_Administrator_page4(bool usePublish)
        {
            return new NodeRejectJob("Reject_page4_1210_Administrator", Paths["page4"], Console.Out, usePublish)
            {
                SleepTime = 1210,
                MaxIterationCount = 17,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_85_Administrator_page5(bool usePublish)
        {
            return new NodeRejectJob("Reject_page5_85_Administrator", Paths["page5"], Console.Out, usePublish)
            {
                SleepTime = 85,
                MaxIterationCount = 25,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_35_Administrator_page6(bool usePublish)
        {
            return new NodeRejectJob("Reject_page6_35_Administrator", Paths["page6"], Console.Out, usePublish)
            {
                SleepTime = 35,
                MaxIterationCount = 35,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_20_Administrator_page7(bool usePublish)
        {
            return new NodeRejectJob("Reject_page7_20_Administrator", Paths["page7"], Console.Out, usePublish)
            {
                SleepTime = 20,
                MaxIterationCount = 35,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeRejectJob Reject_210_Administrator_image1(bool usePublish)
        {
            return new NodeRejectJob("Reject_image1_210_Administrator", Paths["image1"], Console.Out, usePublish)
            {
                SleepTime = 210,
                MaxIterationCount = 18,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }


    //    // --- NodeSave
    //    // -------------------
        internal static NodeSaveJob Save_500_Administrator_page1()
        {
            return new NodeSaveJob("Save_page1_500_Administrator", Paths["page1"], Console.Out)
            {
                SleepTime = 500,
                MaxIterationCount = 40,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_500_Administrator_page2()
        {
            return new NodeSaveJob("Save_page2_500_Administrator", Paths["page2"], Console.Out)
            {
                SleepTime = 500,
                MaxIterationCount = 40,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_700_Administrator_page2()
        {
            return new NodeSaveJob("Save_page2_700_Administrator", Paths["page2"], Console.Out)
            {
                SleepTime = 700,
                MaxIterationCount = 28,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_900_Administrator_page3()
        {
            return new NodeSaveJob("Save_page3_900_Administrator", Paths["page3"], Console.Out)
            {
                SleepTime = 900,
                MaxIterationCount = 22,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }
        internal static NodeSaveJob Save_1200_Administrator_page4()
        {
            return new NodeSaveJob("Save_page4_1200_Administrator", Paths["page4"], Console.Out)
            {
                SleepTime = 1200,
                MaxIterationCount = 16,
                UserName = "Administrator",
                Domain = "BuiltIn"
            };
        }

    //    // --- Bugs
    //    // --------
    //    internal static NodeCheckOutCheckInJob CheckOutCheckIn_40_iluguy_page1()
    //    {
    //        return new NodeCheckOutCheckInJob("save40ilguypage1", Paths["page1"], Console.Out)
    //                   {
    //                       SleepTime = 40,
    //                       MaxIterationCount = 300,
    //                       UserName = "ilguy",
    //                       Domain = "Demo"
    //                   };
    //    }
    //    internal static NodeCheckOutCheckInJob CheckOutCheckIn_110_iluguy_page1()
    //    {
    //        return new NodeCheckOutCheckInJob("save110ilguypage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 110,
    //            MaxIterationCount = 200,
    //            UserName = "ilguy",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeCheckOutCheckInJob CheckOutCheckIn_230_iluguy_page1()
    //    {
    //        return new NodeCheckOutCheckInJob("save230ilguypage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 230,
    //            MaxIterationCount = 200,
    //            UserName = "ilguy",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodePublishJob Publish_50_Administrator_page1()
    //    {
    //        return new NodePublishJob("publish50administratorpage1", Paths["page1"], Console.Out)
    //            {
    //                SleepTime = 50,
    //                MaxIterationCount = 300,
    //                UserName = "Administrator",
    //                Domain = "BuiltIn"
    //            };
    //    }
    //    internal static NodePublishJob Publish_90_Administrator_page1()
    //    {
    //        return new NodePublishJob("publish90administratorpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 90,
    //            MaxIterationCount = 300,
    //            UserName = "Administrator",
    //            Domain = "BuiltIn"
    //        };
    //    }
    //    internal static NodePublishJob Publish_150_Administrator_page1()
    //    {
    //        return new NodePublishJob("publish150administratorpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 150,
    //            MaxIterationCount = 200,
    //            UserName = "Administrator",
    //            Domain = "BuiltIn"
    //        };
    //    }
    //    internal static NodeCheckOutCheckInJob CheckOutCheckIn_80_hxn_page1()
    //    {
    //        return new NodeCheckOutCheckInJob("save80hxnpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 80,
    //            MaxIterationCount = 350,
    //            UserName = "hxn",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeCheckOutCheckInJob CheckOutCheckIn_130_hxn_page1()
    //    {
    //        return new NodeCheckOutCheckInJob("save130hxnpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 130,
    //            MaxIterationCount = 250,
    //            UserName = "hxn",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeCheckOutCheckInJob CheckOutCheckIn_220_hxn_page1()
    //    {
    //        return new NodeCheckOutCheckInJob("save80hxnpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 220,
    //            MaxIterationCount = 100,
    //            UserName = "hxn",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodePublishJob Publish_30_robspace_page1()
    //    {
    //        return new NodePublishJob("publish30robpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 30,
    //            MaxIterationCount = 400,
    //            UserName = "robspace",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodePublishJob Publish_140_robspace_page1()
    //    {
    //        return new NodePublishJob("publish140robpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 140,
    //            MaxIterationCount = 200,
    //            UserName = "robspace",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeUndoCheckoutJob UndoCheckout_45_robspace_page1()
    //    {
    //        return new NodeUndoCheckoutJob("undo45robpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 45,
    //            MaxIterationCount = 300,
    //            UserName = "robspace",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeUndoCheckoutJob UndoCheckout_100_robspace_page1()
    //    {
    //        return new NodeUndoCheckoutJob("undo100robpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 100,
    //            MaxIterationCount = 200,
    //            UserName = "robspace",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeUndoCheckoutJob UndoCheckout_200_robspace_page1()
    //    {
    //        return new NodeUndoCheckoutJob("undo200robpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 200,
    //            MaxIterationCount = 200,
    //            UserName = "robspace",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeUndoCheckoutJob UndoCheckout_60_hxn_page1()
    //    {
    //        return new NodeUndoCheckoutJob("undo60hxnpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 60,
    //            MaxIterationCount = 300,
    //            UserName = "hxn",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeUndoCheckoutJob UndoCheckout_130_hxn_page1()
    //    {
    //        return new NodeUndoCheckoutJob("undo130hxnpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 130,
    //            MaxIterationCount = 230,
    //            UserName = "hxn",
    //            Domain = "Demo"
    //        };
    //    }
    //    internal static NodeUndoCheckoutJob UndoCheckout_190_hxn_page1()
    //    {
    //        return new NodeUndoCheckoutJob("undo190hxnpage1", Paths["page1"], Console.Out)
    //        {
    //            SleepTime = 190,
    //            MaxIterationCount = 180,
    //            UserName = "hxn",
    //            Domain = "Demo"
    //        };
    //    }

    //    //==============================================================================================================================

        


    }
}
