//#define TESTING_WITHOUT_DEVICE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Search;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.Diagnostics;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Communication.Messaging;
using System.Diagnostics;

namespace SenseNet.ApplicationModel
{
#if TESTING_WITHOUT_DEVICE
    public sealed class ApplicationStorage
    {
        [DebuggerDisplay("{ToString()}")]
        private class AppPath
        {
            private static readonly char[] _pathSeparatorChars = RepositoryPath.PathSeparatorChars;

            public int[] Indices;
            public AppNodeType[] AppNodeTypes;
            public int TypeIndex;
            public int ActionIndex;
            public bool Truncated;

            internal int GetNextIndex(int currentIndex)
            {
                var i = currentIndex + 1;
                if (PathSegments[this.Indices[i]] == AppFolderName)
                    i++;
                return (i >= this.Indices.Length) ? -1 : i;
            }

            public static AppPath MakePath(string path)
            {
                var words = path.Split(_pathSeparatorChars, StringSplitOptions.RemoveEmptyEntries).ToList();
                var result = new List<int>();
                var typeIndex = -1;
                var actionIndex = -1;
                var isType = false;
                var i = 0;
                while (i < words.Count)
                {
                    var word = words[i];
                    if (isType)
                    {
                        // remove current word and insert words of type path
                        if (word != "This")
                        {
                            var ntype = ActiveSchema.NodeTypes[word];
                            if (ntype == null)
                                return null;
                            var typeNames = ntype.NodeTypePath.Split('/');
                            words.RemoveAt(i);
                            words.InsertRange(i, typeNames);
                            word = words[i];
                            actionIndex = i + typeNames.Length;
                        }
                        else
                        {
                            actionIndex = i + 1;
                        }
                        isType = false;
                    }
                    word = word.ToLower();
                    if (word == AppFolderName)
                    {
                        typeIndex = i;
                        isType = true;
                        words.RemoveAt(i);
                        continue;
                    }

                    var index = PathSegments.IndexOf(word);
                    if (index < 0)
                    {
                        index = PathSegments.Count;
                        PathSegments.Add(word);
                    }
                    result.Add(index);
                    i++;
                }
                var appPath = new AppPath { Indices = result.ToArray(), TypeIndex = typeIndex, ActionIndex = actionIndex };
                appPath.Initialize();
                return appPath;
            }
            public static AppPath MakePath(NodeHead head, string actionName, string[] device)
            {
                var actionNameIndex = -1;
                if (actionName != null)
                {
                    actionName = actionName.ToLower();
                    actionNameIndex = PathSegments.IndexOf(actionName);
                    if (actionNameIndex < 0)
                        return null;
                }

                var words = head.Path.Split(_pathSeparatorChars, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < words.Count; i++)
                    words[i] = words[i].ToLower();

                var result = new List<int>();
                var typeIndex = -1;
                var actionIndex = -1;
                var isTruncated = false;
                foreach (var word in words)
                {
                    var index = PathSegments.IndexOf(word);
                    if (index < 0)
                    {
                        isTruncated = true;
                        break;
                    }
                    result.Add(index);
                }
                typeIndex = result.Count;

                var ntype = ActiveSchema.NodeTypes.GetItemById(head.NodeTypeId);
                var typeNames = ntype.NodeTypePath.Split('/');
                foreach (var typeName in typeNames)
                {
                    var index = PathSegments.IndexOf(typeName.ToLower());
                    if (index < 0)
                        break;
                    result.Add(index);
                }

                actionIndex = result.Count;
                result.Add(actionNameIndex); // can be -1
                for (int i = device.Length - 1; i >= 0; i--)
                {
                    var index = PathSegments.IndexOf(device[i]);
                    if (index < 0)
                        break;
                    result.Add(index);
                }

                var appPath = new AppPath { Indices = result.ToArray(), TypeIndex = typeIndex, ActionIndex = actionIndex, Truncated = isTruncated };
                appPath.Initialize();
                return appPath;
            }

            private void Initialize()
            {
                AppNodeTypes = new AppNodeType[Indices.Length];
                for (int i = 0; i < Indices.Length; i++)
                    AppNodeTypes[i] = GetNodeType(i);
            }
            private AppNodeType GetNodeType(int pathIndex)
            {
                if (pathIndex < TypeIndex)
                    return AppNodeType.Path;
                if (pathIndex < ActionIndex)
                    return AppNodeType.Type;
                if (pathIndex == ActionIndex)
                    return AppNodeType.Action;
                return AppNodeType.Device;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                for (int i = 0; i < Indices.Length; i++)
                    sb.Append('/').Append(Indices[i] < 0 ? "[null]" : PathSegments[Indices[i]]);
                if (Truncated)
                    sb.Append(", truncated");
                return sb.ToString();
            }
        }

        private enum AppNodeType { Path, Type, Action, Device }
        private List<Application> EmptyApplicationList = new List<Application>(0);

        [DebuggerDisplay("{ToString()}")]
        private class AppNode
        {
            public AppNode(int name, AppNodeType type, AppNode parent) : this(name, type, parent, null) { }
            public AppNode(int name, AppNodeType type, AppNode parent, Application app)
            {
                Name = name;
                AppNodeType = type;
                Application = app;
                Children = new List<AppNode>();
                Parent = parent;
                if (parent != null)
                {
                    Parent.Children.Add(this);
                    Level = Parent.Level + 1;
                }
                if (app != null)
                {
                    Disabled = app.Disabled;
                    var list = app.ScenarioList;
                    _scenarioList = list.Count > 0 ? list : null;
                }
            }

            public int Name;
            public AppNodeType AppNodeType;
            public AppNode Parent;
            public List<AppNode> Children;
            public Application Application { get; private set; }
            public int Level;

            public void AddChild(Application app)
            {
                //-- appPath is null if app is invalid (e.g. path: .../(apps)/browse
                var appPath = AppPath.MakePath(app.Path);
                if (appPath != null)
                    AddChild(app, appPath, 1);
            }
            private void AddChild(Application app, AppPath appPath, int pathIndex)
            {
                if (pathIndex < 0)
                    return;
                var name = appPath.Indices[pathIndex];
                var appNodeType = appPath.AppNodeTypes[pathIndex];
                if (pathIndex < appPath.Indices.Length - 1)
                {
                    AppNode child = null;
                    foreach (var pathChild in this.Children)
                    {
                        if (pathChild.Name == name && pathChild.AppNodeType == appNodeType)
                        {
                            child = (AppNode)pathChild;
                            break;
                        }
                    }
                    var nextIndex = appPath.GetNextIndex(pathIndex);
                    if (child == null)
                    {
                        //child = new AppNode(name, appPath.GetNodeType(pathIndex));
                        child = new AppNode(name, appNodeType, this);
                        //this.Children.Add(child);
                    }
                    child.AddChild(app, appPath, nextIndex);
                }
                else
                {
                    var child = new AppNode(name, appNodeType, this, app);
                    //this.Children.Add(child);
                }
            }
            public string GetPathString()
            {
                var names = new List<string>();
                var node = this;
                while (node != null)
                {
                    names.Insert(0, PathSegments[node.Name]);
                    node = node.Parent;
                }
                return String.Join("/", names);
            }

            public override string ToString()
            {
                return String.Concat(AppNodeType, ": ", GetPathString());
            }

            public bool Disabled;
            private List<string> _scenarioList;
            public bool HasScenario(string scenario)
            {
                if (_scenarioList == null)
                    return false;
                return _scenarioList.Contains(scenario);
            }
        }

        //----------------------------------------------------------------
        public static string DEVICEPARAMNAME = "SnDevice";

        private static int PathSegmentThisIndex = 2;
        private static List<string> PathSegments;
        private AppNode RootAppNode;

        private AppNode LoadApps2()
        {
            var appList = new List<Application>();

            var nt = ActiveSchema.NodeTypes["Application"];
            var nq = new NodeQuery();
            nq.Add(new TypeExpression(nt));
            nq.Add(new StringExpression(StringAttribute.Path, StringOperator.Contains, string.Format("/{0}/", AppFolderName)));
            nq.Orders.Add(new SearchOrder(StringAttribute.Path, OrderDirection.Asc));

            PathSegments = new List<string>();
            PathSegments.Add("root");
            PathSegments.Add("this");
            PathSegmentThisIndex = 1;
            var root = new AppNode(0, AppNodeType.Path, null);

            using (new SystemAccount())
            {
                var result = nq.Execute();
                foreach (Application node in result.Nodes)
                    root.AddChild(node);
            }

            return root;
        }

        private List<Application> GetApplicationsInternal2(string appName, NodeHead head, string scenarioName, string requestedDevice)
        {
            if (head == null || RootAppNode == null)
                return new List<Application>();
            
            var device = requestedDevice == null ? new string[0] : DeviceManager.GetDeviceChain(requestedDevice.ToLower());

            var appPath = AppPath.MakePath(head, appName, device);
            if (appPath == null)
                return EmptyApplicationList;

            var lastNode = SearchLastNode(appPath);

            if (appName != null)
                return GetApplicationsByAppName(lastNode, appPath);
            return GetApplicationsByScenario(lastNode, appPath, scenarioName, device);
        }
        private AppNode SearchLastNode(AppPath appPath)
        {
            return SearchLastNode(RootAppNode, appPath, 1, true);
        }
        private AppNode SearchLastNode(AppNode appNode, AppPath appPath, int pathIndex, bool thisEnabled)
        {
            if (appNode == null)
                return null;

            if (pathIndex >= appPath.Indices.Length)
                return appNode;

            if (!appPath.Truncated && pathIndex == appPath.TypeIndex && appNode.Level == pathIndex - 1 && thisEnabled)
            {
                foreach (var child in appNode.Children)
                {
                    if (child.Name == PathSegmentThisIndex)
                    {
                        if (appPath.Indices[appPath.ActionIndex] < 0)
                            return child;
                        var thisNode = child;
                        var last = SearchLastNode(thisNode, appPath, appPath.ActionIndex, thisEnabled);
                        if (last != null)
                            return last;
                    }
                }
            }

            var name = appPath.Indices[pathIndex];
            var appNodeType = appPath.AppNodeTypes[pathIndex];
            if (name < 0)
                return appNode;

            foreach (var child in appNode.Children)
                if (child.Name == name && child.AppNodeType == appNodeType)
                    return SearchLastNode(child, appPath, pathIndex + 1, thisEnabled);

            if (pathIndex < appPath.TypeIndex)
            {
                return SearchLastNode(appNode, appPath, appPath.TypeIndex, thisEnabled);
            }
            if (pathIndex < appPath.ActionIndex)
            {
                return SearchLastNode(appNode, appPath, appPath.ActionIndex, thisEnabled);
            }
            return appNode;
        }

        private List<Application> GetApplicationsByAppName(AppNode lastNode, AppPath appPath)
        {
            // resolve one application
            Application app = null;
            Application ovr = null;

            while (lastNode != null)
            {
                if (lastNode.AppNodeType == AppNodeType.Action || lastNode.AppNodeType == AppNodeType.Device)
                {
                    app = SearchAppInAction(lastNode, appPath, null);
                    if (app != null)
                    {
                        if (app.Clear)
                        {
                            app = null;
                            break;
                        }
                        if (app.IsOverride)
                        {
                            if (ovr != null)
                                app = Override(app, ovr);// app.Override = ovr;
                            ovr = app;
                            app = null;
                            lastNode = lastNode.Parent;
                        }
                    }
                    else
                    {
                        while (lastNode.AppNodeType != AppNodeType.Type)
                            lastNode = lastNode.Parent;
                    }
                }
                if (app == null)
                {
                    // parent typelevel or pathlevel
                    if (lastNode.Name == PathSegmentThisIndex)
                    {
                        lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, false);
                    }
                    else if (lastNode.AppNodeType == AppNodeType.Type)
                    {
                        lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.ActionIndex, true);
                    }
                    else if (lastNode.AppNodeType == AppNodeType.Path)
                    {
                        lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, true);
                    }
                    else
                    {
                        throw new NotImplementedException("@@@@");
                    }
                }
                else
                {
                    break;
                }
            }
            if (app == null)
                return new List<Application>();
            if (ovr != null)
                app = Override(app, ovr); // app.Override = ovr;
            return new List<Application>(new[] { app });
        }
        private Application SearchAppInAction(AppNode appNodeInAction, AppPath appPath, string scenario)
        {
            var ovrList = new List<Application>();
            Application resultApp = null;

            var appNode = appNodeInAction;
            while (appNode.AppNodeType != AppNodeType.Type)
            {
                var app = appNode.Application;
                if (app != null)
                {
                    if (app.Clear)
                        return app;

                    if (scenario == null || appNode.HasScenario(scenario))
                    {
                        if (!appNode.Disabled && app.Security.HasPermission(PermissionType.Open))
                        {
                            if (app.IsOverride)
                            {
                                if (resultApp != null)
                                    app = Override(app, resultApp); // app.Override = resultApp;
                                resultApp = app;
                            }
                            else
                            {
                                resultApp = app;
                                break;
                            }
                        }
                    }
                }
                appNode = appNode.Parent;
            }
            return resultApp;
        }
        private Application Override(Application app, Application @override)
        {
            var reloadedApp = Node.Load<Application>(app.Id);
            reloadedApp.Override = @override;
            return reloadedApp;
        }

        private List<Application> GetApplicationsByScenario(AppNode lastNode, AppPath appPath, string scenarioName, string[] device)
        {
            // resolve all applications filtered by scenario
            var apps = new Dictionary<int, Application>();

            while (lastNode != null)
            {
                if (lastNode.AppNodeType == AppNodeType.Type)
                {
                    GetApplicationsInType(lastNode, appPath, scenarioName, device, apps);
                }

                // parent typelevel or pathlevel
                if (lastNode.Name == PathSegmentThisIndex)
                    lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, false);
                else if (lastNode.AppNodeType == AppNodeType.Type)
                    lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.ActionIndex, true);
                else
                    lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, true);
            }
            var result = apps.Values.ToList();
            for (int i = result.Count - 1; i >= 0; i--)
                if (result[i].Clear || result[i].IsOverride)
                    result.RemoveAt(i);

            result.Sort(new ApplicationComparer());
            return result;
        }
        private void GetApplicationsInType(AppNode typeNode, AppPath appPath, string scenario, string[] device, Dictionary<int, Application> allApps)
        {
            foreach (var child in typeNode.Children)
            {
                if (child.AppNodeType == AppNodeType.Action)
                {
                    var search = true;
                    Application existingApp;
                    if (allApps.TryGetValue(child.Name, out existingApp))
                        if (existingApp.Clear || !existingApp.IsOverride)
                            search = false;

                    if (search)
                    {
                        //-- deepest device
                        var lastDeviceNode = SearchLastNode(child, appPath, appPath.ActionIndex + 1, true);
                        var app = SearchAppInAction(lastDeviceNode, appPath, scenario);
                        if (app != null)
                        {
                            if (existingApp == null)
                            {
                                allApps.Add(child.Name, app);
                            }
                            else
                            {
                                app = Override(app, existingApp); //app.Override = existingApp;
                                allApps[child.Name] = app;
                            }
                        }
                    }
                }
            }
        }

        private void CheckResults(List<Application> result, List<Application> result2, string appName, string scenario, string contextPath)
        {
            if (result.Count != result2.Count)
                ResultError("Count error", result, result2, appName, scenario, contextPath);
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Id != result2[i].Id)
                    ResultError("Id error", result, result2, appName, scenario, contextPath);
            }
        }
        private void ResultError(string msg, List<Application> result, List<Application> result2, string appName, string scenario, string contextPath)
        {
            var r = String.Join(", ", result.Select(x => x.Path));
            var r2 = String.Join(", ", result2.Select(x => x.Path));
            var sb = new StringBuilder("#> RESULT ERROR. ");
            sb.Append("#### APP ").Append(msg).Append(": Result: ").Append(r2).Append(", expected: ").Append(r)
                .Append(", appName: ").Append(appName)
                .Append(", scenario: ").Append(scenario)
                .Append(". Context: ").Append(contextPath);
            Trace.WriteLine(sb.ToString());
            throw new Exception(sb.ToString());
        }

        //================================================================

        private const string AppFolderName = "(apps)";

        private static ApplicationStorage _instance;
        private static readonly object LockObject = new Object();

        private List<Application> _appList;
        private List<string> _appNames;
        private Dictionary<string, IEnumerable<Application>> _appsByFolder;

        public static ApplicationStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new ApplicationStorage();
                        }
                    }
                }

                return _instance;
            }
        }

        private List<Application> ApplicationList
        {
            get
            {
                if (_appList == null)
                {
                    lock (LockObject)
                    {
                        if (_appList == null)
                        {
                            var apps = LoadApps();
                            //store applications by parent folder to be able to 
                            //reach them faster during application resolve
                            _appsByFolder = new Dictionary<string, IEnumerable<Application>>();
                            if (apps != null)
                            {
                                foreach (var parentPath in apps.Select(a => a.ParentPath).Distinct())
                                {
                                    _appsByFolder.Add(parentPath, apps.Where(a => a.ParentPath == parentPath).ToList());
                                }
                            }

                            _appList = apps;
                        }
                    }
                }

                return _appList;
            }
        }

        private ApplicationStorage()
        {
        }

        private List<Application> LoadApps()
        {
            RootAppNode = LoadApps2();

            using (var traceOperation = Logger.TraceOperation("ApplicationStorage.LoadApps"))
            {
                var nt = ActiveSchema.NodeTypes["Application"];
                if (nt == null)
                {
                    traceOperation.IsSuccessful = true;
                    return new List<Application>();
                }

                var appList = new List<Application>();

                var nq = new NodeQuery();
                nq.Add(new TypeExpression(nt));
                nq.Add(new StringExpression(StringAttribute.Path, StringOperator.Contains, string.Format("/{0}/", AppFolderName)));

                //We have to load all apps because we need to know about 
                //all the app names, so we can't filter them here

                nq.Orders.Add(new SearchOrder(StringAttribute.Path, OrderDirection.Desc));

                using (new SystemAccount())
                {
                    var result = nq.Execute();

                    try
                    {
                        appList.AddRange(result.Nodes.Where(n => n is Application).Cast<Application>());
                        appList.Sort(new ApplicationComparer());

                        _appNames = (from app in appList
                                     select app.AppName).Distinct().ToList();
                    }
                    catch (NotSupportedException ex)
                    {
                        //exception: #### Storage2: Partial Node technology is removed
                        //sometimes this occurs during page save (fake second version entry in db?)
                        Logger.WriteException(ex);
                        _appNames = null;
                        appList = null;
                    }
                }

                traceOperation.IsSuccessful = true;
                return appList;
            }
        }

        /*=================================================================================== Get Apps */

        // caller: Scenarios and tests
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, Content context)
        {
            return GetApplication(applicationName, context, null);
        }
        public Application GetApplication(string applicationName, Content context, string device)
        {
            bool existingApplication;
            return GetApplication(applicationName, context, out existingApplication, device);
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, NodeHead head)
        {
            return GetApplication(applicationName, head, null);
        }
        public Application GetApplication(string applicationName, NodeHead head, string device)
        {
            bool existingApplication;
            return GetApplication(applicationName, head, out existingApplication, device);
        }

        // caller: ActionFramework
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, Content context, out bool existingApplication)
        {
            return GetApplication(applicationName, context, out existingApplication, null);
        }
        public Application GetApplication(string applicationName, Content context, out bool existingApplication, string device)
        {
            var app = GetApplicationsInternal(applicationName, context, null, device).FirstOrDefault();

            existingApplication = app != null || Exists(applicationName);

            return app;
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, NodeHead head, out bool existingApplication)
        {
            return GetApplication(applicationName, head, out existingApplication, null);
        }
        public Application GetApplication(string applicationName, NodeHead head, out bool existingApplication, string device)
        {
            var app = GetApplicationsInternal(applicationName, head, null, device).FirstOrDefault();

            existingApplication = app != null || Exists(applicationName);

            return app;
        }

        // caller: ApplicationListPresenterPortlet
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(Content context)
        {
            return GetApplications(context, null);
        }
        public List<Application> GetApplications(Content context, string device)
        {
            return GetApplicationsInternal(null, context, null, device);
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(NodeHead head)
        {
            return GetApplications(head, null);
        }
        public List<Application> GetApplications(NodeHead head, string device)
        {
            return GetApplicationsInternal(null, head, null, device);
        }

        // caller: ActionFramework
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(string scenarioName, Content context)
        {
            return GetApplications(scenarioName, context, null);
        }
        public List<Application> GetApplications(string scenarioName, Content context, string device)
        {
            return GetApplicationsInternal(null, context, scenarioName, device);
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(string scenarioName, NodeHead head)
        {
            return GetApplications(scenarioName, head, null);
        }
        public List<Application> GetApplications(string scenarioName, NodeHead head, string device)
        {
            return GetApplicationsInternal(null, head, scenarioName, device);
        }

        private List<Application> GetApplicationsInternal(string appName, Content context, string scenarioName, string device)
        {
            return GetApplicationsInternal(appName, context == null ? null : NodeHead.Get(context.Path), scenarioName, device);
        }

        private List<Application> GetApplicationsInternal(string appName, NodeHead head, string scenarioName, string device)
        {
            var result = new List<Application>();

            if (head == null || ApplicationList == null)
                return result;

//Trace.WriteLine(string.Format("#> old: , new: , appNames: {0}, scenario: {1}, path: {2}", appNames == null ? "" : appNames.First(), scenarioName, head.Path));
var time1 = TimeSpan.MinValue;
long ticks1 = 0;
var result2 = new List<Application>();
var timer = Stopwatch.StartNew();
Exception ex = null;
try
{
    result2 = GetApplicationsInternal2(appName, head, scenarioName, device);
}
catch (Exception e)
{
    Trace.WriteLine("#> RESOLUTION EXCEPTION: " + e.Message);
    Logger.WriteException(e);
    ex = e;
}
finally
{
    timer.Stop();
    time1 = timer.Elapsed;
    ticks1 = timer.ElapsedTicks;
}
timer = Stopwatch.StartNew();

            var paths = GetAppPathList(head);

            List<string> requestedNames;
            if (appName != null)
                requestedNames = new List<string>(new[] { appName.ToLower() });
            else
                requestedNames =  _appNames.Select(s => s.ToLower()).ToList();

            //iterate through the possible paths in the correct order 
            //from leaves to the Root, and get the available apps:
            //...for this node
            //...for this appName
            //...for this path

            var foundApps = new Dictionary<string, List<Application>>();
            foreach (var parentPath in paths)
            {
                IEnumerable<Application> apps;
                _appsByFolder.TryGetValue(parentPath, out apps);

                if (apps == null)
                    continue;

                var relevantApps = string.IsNullOrEmpty(scenarioName)
                                   ? apps.Where(a => !a.Disabled || a.Clear)
                                   : apps.Where(a => (a.ScenarioList.Contains(scenarioName) && !a.Disabled) || a.Clear);

                foreach (var app in relevantApps)
                {
                    var lowerName = app.AppName.ToLower();

                    //skip apps that were not requested
                    if (!requestedNames.Contains(lowerName))
                        continue;

                    //place found apps to a dictionary (key is the app name)
                    if (!foundApps.ContainsKey(lowerName))
                        foundApps.Add(lowerName, new List<Application>());

                    foundApps[lowerName].Add(app);
                }
            }

            //dictionary values are app chains - merge them now
            result.AddRange(foundApps.Values.Select(MergeApplicationChain).Where(mergedApp => mergedApp != null));

            result.Sort(new ApplicationComparer());

timer.Stop();
var q = Convert.ToDouble(timer.ElapsedTicks) / Convert.ToDouble(ticks1);
Trace.WriteLine(string.Format("#> old: {0}, new: {1}, {2}, appCount: {3},  app|scen|dev|ctx: {4} | {5} | {6} | {7}", timer.Elapsed, time1, q, result2.Count, appName ?? String.Empty, scenarioName, device == null ? "[null]" : device, head.Path));
if (!(ex is NotImplementedException))
    CheckResults(result, result2, appName, scenarioName, head.Path);

            return result;
        }

        public bool Exists(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName) || _appNames == null)
                return false;

            applicationName = applicationName.ToLower();
            return (_appNames.Count(a => a.ToLower() == applicationName) > 0);
        }

        //=================================================================================== Helper methods

        private static Application MergeApplicationChain(IList<Application> applications)
        {
            if (applications == null || applications.Count == 0)
                return null;

            Application mainApp = null;
            var index = -1;

            //find the first real app
            foreach (var app in applications)
            {
                index++;

                //skip disabled apps and overrides
                if (app.Disabled || app.NodeType.IsInstaceOfOrDerivedFrom("ApplicationOverride"))
                    continue;

                //if this is a cleaner, than do not discover apps above
                if (app.Clear)
                    break;

                //return the application node only if the user has enough permissions
                if (SecurityHandler.HasPermission(app, PermissionType.Open))
                    mainApp = app;

                if (mainApp != null)
                    break;
            }

            //we did not found a relevant real application
            if (mainApp == null)
                return null;

            //if an override exists, we have to load a new node
            //instead of using the cached (shared) application
            var chainApp = index > 0 ? mainApp = Node.Load<Application>(mainApp.Path) : mainApp;

            for (var i = index - 1; i >= 0; i--)
            {
                Application app = null;

                if (SecurityHandler.HasPermission(applications[i], PermissionType.Open))
                    app = Node.Load<Application>(applications[i].Path);

                if (app == null || app.Disabled)
                    continue;

                chainApp.Override = app;
                chainApp = chainApp.Override;
            }

            return mainApp;
        }

        private static IEnumerable<string> GetAppPathList(NodeHead head)
        {
            var paths = new List<string>();

            if (head == null)
                return paths;

            var contextNodePath = head.Path.TrimEnd('/');
            paths.Add(String.Concat(contextNodePath, "/", AppFolderName, "/This"));

            var parts = contextNodePath.Split('/');
            var probs = new List<string>();
            var nodeType = NodeType.GetById(head.NodeTypeId);

            while (nodeType != null)
            {
                probs.Add(String.Concat("/{0}/", nodeType.Name));
                nodeType = nodeType.Parent;
            }

            var position = parts.Length + 1;
            while (position-- > 2)
            {
                var partpath = string.Join("/", parts, 0, position);
                paths.AddRange(probs.Select(prob => String.Concat(partpath, string.Format(prob, AppFolderName))));
            }

            return paths;
        }

        //=================================================================================== Distributed invalidate

        [Serializable]
        internal class ApplicationStorageInvalidateDistributedAction : DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (onRemote && isFromMe)
                    return;
                ApplicationStorage.InvalidatePrivate();
            }
        }

        private static void DistributedInvalidate()
        {
            new ApplicationStorageInvalidateDistributedAction().Execute();
        }

        private static void InvalidatePrivate()
        {
            Logger.WriteInformation("ApplicationStorage invalidate");

            _instance = null;
        }

        public static void Invalidate()
        {
            DistributedInvalidate();
        }

        public static bool InvalidateByNode(Node node)
        {
            return ApplicationStorage.Instance.InvalidateByNodeInternal(node);
        }

        public static bool InvalidateByPath(string path)
        {
            return ApplicationStorage.Instance.InvalidateByPathInternal(path);
        }

        private bool InvalidateByNodeInternal(Node node)
        {
            if (_appList == null || node == null)
                return false;

            if (node is Application)
            {
                Invalidate();
                return true;
            }

            return InvalidateByPathInternal(node.Path);
        }

        private bool InvalidateByPathInternal(string path)
        {
            if (_appList == null || string.IsNullOrEmpty(path))
                return false;

            if ((from app in _appList
                 where app.Path.StartsWith(path)
                 select app).Count() > 0)
            {
                Invalidate();
                return true;
            }

            return false;
        }

        //=================================================================================== Comparer class
        private class ApplicationComparer : IComparer<Application>
        {
            public int Compare(Application x, Application y)
            {
                if (x == null || y == null)
                    return 0;

                return x.Path.CompareTo(y.Path);
            }
        }
    }
#else
    public sealed class ApplicationStorage
    {
        [DebuggerDisplay("{ToString()}")]
        private class AppPath
        {
            private static readonly char[] _pathSeparatorChars = RepositoryPath.PathSeparatorChars;

            public int[] Indices;
            public AppNodeType[] AppNodeTypes;
            public int TypeIndex;
            public int ActionIndex;
            public bool Truncated;

            internal int GetNextIndex(int currentIndex)
            {
                var i = currentIndex + 1;
                if (PathSegments[this.Indices[i]] == AppFolderName)
                    i++;
                return (i >= this.Indices.Length) ? -1 : i;
            }

            public static AppPath MakePath(string path)
            {
                var words = path.Split(_pathSeparatorChars, StringSplitOptions.RemoveEmptyEntries).ToList();
                var result = new List<int>();
                var typeIndex = -1;
                var actionIndex = -1;
                var isType = false;
                var i = 0;
                while (i < words.Count)
                {
                    var word = words[i];
                    if (isType)
                    {
                        // remove current word and insert words of type path
                        if (word != "This")
                        {
                            var ntype = ActiveSchema.NodeTypes[word];
                            if (ntype == null)
                                return null;
                            var typeNames = ntype.NodeTypePath.Split('/');
                            words.RemoveAt(i);
                            words.InsertRange(i, typeNames);
                            word = words[i];
                            actionIndex = i + typeNames.Length;
                        }
                        else
                        {
                            actionIndex = i + 1;
                        }
                        isType = false;
                    }
                    word = word.ToLower();
                    if (word == AppFolderName)
                    {
                        typeIndex = i;
                        isType = true;
                        words.RemoveAt(i);
                        continue;
                    }

                    var index = PathSegments.IndexOf(word);
                    if (index < 0)
                    {
                        index = PathSegments.Count;
                        PathSegments.Add(word);
                    }
                    result.Add(index);
                    i++;
                }
                var appPath = new AppPath { Indices = result.ToArray(), TypeIndex = typeIndex, ActionIndex = actionIndex };
                appPath.Initialize();
                return appPath;
            }
            public static AppPath MakePath(NodeHead head, string actionName, string[] device)
            {
                var actionNameIndex = -1;
                if (actionName != null)
                {
                    actionName = actionName.ToLower();
                    actionNameIndex = PathSegments.IndexOf(actionName);
                    if (actionNameIndex < 0)
                        return null;
                }

                var words = head.Path.Split(_pathSeparatorChars, StringSplitOptions.RemoveEmptyEntries).ToList();
                for (int i = 0; i < words.Count; i++)
                    words[i] = words[i].ToLower();

                var result = new List<int>();
                var typeIndex = -1;
                var actionIndex = -1;
                var isTruncated = false;
                foreach (var word in words)
                {
                    var index = PathSegments.IndexOf(word);
                    if (index < 0)
                    {
                        isTruncated = true;
                        break;
                    }
                    result.Add(index);
                }
                typeIndex = result.Count;

                var ntype = ActiveSchema.NodeTypes.GetItemById(head.NodeTypeId);
                var typeNames = ntype.NodeTypePath.Split('/');
                foreach (var typeName in typeNames)
                {
                    var index = PathSegments.IndexOf(typeName.ToLower());
                    if (index < 0)
                        break;
                    result.Add(index);
                }

                actionIndex = result.Count;
                result.Add(actionNameIndex); // can be -1
                for (int i = device.Length - 1; i >= 0; i--)
                {
                    var index = PathSegments.IndexOf(device[i]);
                    if (index < 0)
                        break;
                    result.Add(index);
                }

                var appPath = new AppPath { Indices = result.ToArray(), TypeIndex = typeIndex, ActionIndex = actionIndex, Truncated = isTruncated };
                appPath.Initialize();
                return appPath;
            }

            private void Initialize()
            {
                AppNodeTypes = new AppNodeType[Indices.Length];
                for (int i = 0; i < Indices.Length; i++)
                    AppNodeTypes[i] = GetNodeType(i);
            }
            private AppNodeType GetNodeType(int pathIndex)
            {
                if (pathIndex < TypeIndex)
                    return AppNodeType.Path;
                if (pathIndex < ActionIndex)
                    return AppNodeType.Type;
                if (pathIndex == ActionIndex)
                    return AppNodeType.Action;
                return AppNodeType.Device;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                for (int i = 0; i < Indices.Length; i++)
                    sb.Append('/').Append(Indices[i] < 0 ? "[null]" : PathSegments[Indices[i]]);
                if (Truncated)
                    sb.Append(", truncated");
                return sb.ToString();
            }
        }

        private enum AppNodeType { Path, Type, Action, Device }
        private List<Application> EmptyApplicationList = new List<Application>(0);

        [DebuggerDisplay("{ToString()}")]
        private class AppNode
        {
            public AppNode(int name, AppNodeType type, AppNode parent) : this(name, type, parent, null) { }
            public AppNode(int name, AppNodeType type, AppNode parent, Application app)
            {
                Name = name;
                AppNodeType = type;
                Application = app;
                Children = new List<AppNode>();
                Parent = parent;
                if (parent != null)
                {
                    Parent.Children.Add(this);
                    Level = Parent.Level + 1;
                }
                if (app != null)
                {
                    Disabled = app.Disabled;
                    var list = app.ScenarioList;
                    _scenarioList = list.Count > 0 ? list : null;
                }
            }

            public int Name;
            public AppNodeType AppNodeType;
            public AppNode Parent;
            public List<AppNode> Children;
            public Application Application { get; private set; }
            public int Level;

            public void AddChild(Application app)
            {
                //-- appPath is null if app is invalid (e.g. path: .../(apps)/browse
                var appPath = AppPath.MakePath(app.Path);
                if (appPath != null)
                    AddChild(app, appPath, 1);
            }
            private void AddChild(Application app, AppPath appPath, int pathIndex)
            {
                if (pathIndex < 0)
                    return;
                var name = appPath.Indices[pathIndex];
                var appNodeType = appPath.AppNodeTypes[pathIndex];
                if (pathIndex < appPath.Indices.Length - 1)
                {
                    AppNode child = null;
                    foreach (var pathChild in this.Children)
                    {
                        if (pathChild.Name == name && pathChild.AppNodeType == appNodeType)
                        {
                            child = (AppNode)pathChild;
                            break;
                        }
                    }
                    var nextIndex = appPath.GetNextIndex(pathIndex);
                    if (child == null)
                    {
                        //child = new AppNode(name, appPath.GetNodeType(pathIndex));
                        child = new AppNode(name, appNodeType, this);
                        //this.Children.Add(child);
                    }
                    child.AddChild(app, appPath, nextIndex);
                }
                else
                {
                    var child = new AppNode(name, appNodeType, this, app);
                    //this.Children.Add(child);
                }
            }
            public string GetPathString()
            {
                var names = new List<string>();
                var node = this;
                while (node != null)
                {
                    names.Insert(0, PathSegments[node.Name]);
                    node = node.Parent;
                }
                return String.Join("/", names);
            }

            public override string ToString()
            {
                return String.Concat(AppNodeType, ": ", GetPathString());
            }

            public bool Disabled;
            private List<string> _scenarioList;
            public bool HasScenario(string scenario)
            {
                if (_scenarioList == null)
                    return false;
                return _scenarioList.Contains(scenario);
            }
        }

        //----------------------------------------------------------------
        public static string DEVICEPARAMNAME = "SnDevice";

        private static int PathSegmentThisIndex = 2;
        private static List<string> PathSegments;
        private AppNode __rootAppNode;
        private AppNode RootAppNode
        {
            get
            {
                if (__rootAppNode == null)
                {
                    lock (LockObject)
                    {
                        if (__rootAppNode == null)
                        {
                            __rootAppNode = LoadApps(out _appNames, out _appList);
                        }
                    }
                }

                return __rootAppNode;
            }
        }

        private static AppNode LoadApps(out List<string> appNames, out List<Application> appList)
        {
            var nt = ActiveSchema.NodeTypes["Application"];
            var nq = new NodeQuery();
            nq.Add(new TypeExpression(nt));
            //nq.Add(new StringExpression(StringAttribute.Path, StringOperator.Contains, string.Format("/{0}/", AppFolderName)));
            nq.Orders.Add(new SearchOrder(StringAttribute.Path, OrderDirection.Asc));

            using (new SystemAccount())
            {
                var result = nq.Execute();
                appList = result.Nodes.Cast<Application>().ToList();
            }
            PathSegments = new List<string>();
            PathSegments.Add("root");
            PathSegments.Add("this");
            PathSegmentThisIndex = 1;
            var root = new AppNode(0, AppNodeType.Path, null);

            appNames = new List<string>();
            foreach (var node in appList)
            {
                root.AddChild(node);
                var appName = node.AppName;

                //------------PATCH START: set AppName property if null (compatibility reason));
                if (string.IsNullOrEmpty(appName))
                {
                    var originalUser = AccessProvider.Current.GetCurrentUser();

                    try
                    {
                        //We can save content only with the Administrator at this point,
                        //because there is a possibility that the original user is the
                        //STARTUP user that cannot be used to save any content.
                        AccessProvider.Current.SetCurrentUser(User.Administrator);

                        node.Save(SavingMode.KeepVersion);
                        appName = node.AppName;
                    }
                    finally
                    {
                        AccessProvider.Current.SetCurrentUser(originalUser);
                    }
                }
                //------------PATCH END

                if (!string.IsNullOrEmpty(appName) && !appNames.Contains(appName))
                    appNames.Add(appName);
            }
            appList.Sort(new ApplicationComparer());

            return root;
        }

        private List<Application> GetApplicationsInternal(string appName, NodeHead head, string scenarioName, string requestedDevice)
        {
            if (head == null || RootAppNode == null)
                return new List<Application>();

            var device = requestedDevice == null ? new string[0] : DeviceManager.GetDeviceChain(requestedDevice.ToLower());

            var appPath = AppPath.MakePath(head, appName, device);
            if (appPath == null)
                return EmptyApplicationList;

            var lastNode = SearchLastNode(appPath);

            if (appName != null)
                return GetApplicationsByAppName(lastNode, appPath);
            return GetApplicationsByScenario(lastNode, appPath, scenarioName, device);
        }
        private AppNode SearchLastNode(AppPath appPath)
        {
            return SearchLastNode(RootAppNode, appPath, 1, true);
        }
        private AppNode SearchLastNode(AppNode appNode, AppPath appPath, int pathIndex, bool thisEnabled)
        {
            if (appNode == null)
                return null;

            if (pathIndex >= appPath.Indices.Length)
                return appNode;

            if (!appPath.Truncated && pathIndex == appPath.TypeIndex && appNode.Level == pathIndex - 1 && thisEnabled)
            {
                foreach (var child in appNode.Children)
                {
                    if (child.Name == PathSegmentThisIndex)
                    {
                        if (appPath.Indices[appPath.ActionIndex] < 0)
                            return child;
                        var thisNode = child;
                        var last = SearchLastNode(thisNode, appPath, appPath.ActionIndex, thisEnabled);
                        if (last != null)
                            return last;
                    }
                }
            }

            var name = appPath.Indices[pathIndex];
            var appNodeType = appPath.AppNodeTypes[pathIndex];
            if (name < 0)
                return appNode;

            foreach (var child in appNode.Children)
                if (child.Name == name && child.AppNodeType == appNodeType)
                    return SearchLastNode(child, appPath, pathIndex + 1, thisEnabled);

            if (pathIndex < appPath.TypeIndex)
            {
                return SearchLastNode(appNode, appPath, appPath.TypeIndex, thisEnabled);
            }
            if (pathIndex < appPath.ActionIndex)
            {
                return SearchLastNode(appNode, appPath, appPath.ActionIndex, thisEnabled);
            }
            return appNode;
        }

        private List<Application> GetApplicationsByAppName(AppNode lastNode, AppPath appPath)
        {
            // resolve one application
            Application app = null;
            Application ovr = null;

            while (lastNode != null)
            {
                if (lastNode.AppNodeType == AppNodeType.Action || lastNode.AppNodeType == AppNodeType.Device)
                {
                    app = SearchAppInAction(lastNode, appPath, null);
                    if (app != null)
                    {
                        if (app.Clear)
                        {
                            app = null;
                            break;
                        }
                        if (app.IsOverride)
                        {
                            if (ovr != null)
                                app = Override(app, ovr);// app.Override = ovr;
                            ovr = app;
                            app = null;
                            lastNode = lastNode.Parent;
                        }
                    }
                    else
                    {
                        while(lastNode.AppNodeType != AppNodeType.Type)
                            lastNode = lastNode.Parent;
                    }
                }
                if (app == null)
                {
                    // parent typelevel or pathlevel
                    if (lastNode.Name == PathSegmentThisIndex)
                    {
                        lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, false);
                    }
                    else if (lastNode.AppNodeType == AppNodeType.Type)
                    {
                        lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.ActionIndex, true);
                    }
                    else if (lastNode.AppNodeType == AppNodeType.Path)
                    {
                        lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, true);
                    }
                    else
                    {
                        throw new NotImplementedException("@@@@");
                    }
                }
                else
                {
                    break;
                }
            }
            if (app == null)
                return new List<Application>();
            if (ovr != null)
                app = Override(app, ovr); // app.Override = ovr;
            return new List<Application>(new[] { app });
        }
        private Application SearchAppInAction(AppNode appNodeInAction, AppPath appPath, string scenario)
        {
            var ovrList = new List<Application>();
            Application resultApp = null;

            var appNode = appNodeInAction;
            while (appNode.AppNodeType != AppNodeType.Type)
            {
                var app = appNode.Application;
                if (app != null)
                {
                    if (app.Clear)
                        return app;

                    if (String.IsNullOrEmpty(scenario) || appNode.HasScenario(scenario))
                    {
                        if (!appNode.Disabled && app.Security.HasPermission(PermissionType.Open))
                        {
                            if (app.IsOverride)
                            {
                                if (resultApp != null)
                                    app = Override(app, resultApp); // app.Override = resultApp;
                                resultApp = app;
                            }
                            else
                            {
                                resultApp = app;
                                break;
                            }
                        }
                    }
                }
                appNode = appNode.Parent;
            }
            return resultApp;
        }
        private Application Override(Application app, Application @override)
        {
            var reloadedApp = Node.Load<Application>(app.Id);
            reloadedApp.Override = @override;
            return reloadedApp;
        }

        private List<Application> GetApplicationsByScenario(AppNode lastNode, AppPath appPath, string scenarioName, string[] device)
        {
            // resolve all applications filtered by scenario
            var apps = new Dictionary<int, Application>();

            while (lastNode != null)
            {
                if (lastNode.AppNodeType == AppNodeType.Type)
                {
                    GetApplicationsInType(lastNode, appPath, scenarioName, device, apps);
                }

                // parent typelevel or pathlevel
                if (lastNode.Name == PathSegmentThisIndex)
                    lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, false);
                else if (lastNode.AppNodeType == AppNodeType.Type)
                    lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.ActionIndex, true);
                else
                    lastNode = SearchLastNode(lastNode.Parent, appPath, appPath.TypeIndex, true);
            }
            var result = apps.Values.ToList();
            for (int i = result.Count - 1; i >= 0; i--)
                if (result[i].Clear || result[i].IsOverride)
                    result.RemoveAt(i);

            result.Sort(new ApplicationComparer());
            return result;
        }
        private void GetApplicationsInType(AppNode typeNode, AppPath appPath, string scenario, string[] device, Dictionary<int, Application> allApps)
        {
            foreach (var child in typeNode.Children)
            {
                if (child.AppNodeType == AppNodeType.Action)
                {
                    var search = true;
                    Application existingApp;
                    if (allApps.TryGetValue(child.Name, out existingApp))
                        if (existingApp.Clear || !existingApp.IsOverride)
                            search = false;

                    if (search)
                    {
                        //-- deepest device
                        var lastDeviceNode = SearchLastNode(child, appPath, appPath.ActionIndex + 1, true);
                        var app = SearchAppInAction(lastDeviceNode, appPath, scenario);
                        if (app != null)
                        {
                            if (existingApp == null)
                            {
                                allApps.Add(child.Name, app);
                            }
                            else
                            {
                                app = Override(app, existingApp); //app.Override = existingApp;
                                allApps[child.Name] = app;
                            }
                        }
                    }
                }
            }
        }

        //================================================================

        private const string AppFolderName = "(apps)";

        private static ApplicationStorage _instance;
        private static readonly object LockObject = new Object();

        private List<Application> _appList;
        private List<string> _appNames;

        public static ApplicationStorage Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new ApplicationStorage();
                        }
                    }
                }

                return _instance;
            }
        }

        private ApplicationStorage()
        {
        }

        /*=================================================================================== Get Apps */

        // caller: Scenarios and tests
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, Content context)
        {
            return GetApplication(applicationName, context, null);
        }
        public Application GetApplication(string applicationName, Content context, string device)
        {
            bool existingApplication;
            return GetApplication(applicationName, context, out existingApplication, device);
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, NodeHead head)
        {
            return GetApplication(applicationName, head, null);
        }
        public Application GetApplication(string applicationName, NodeHead head, string device)
        {
            bool existingApplication;
            return GetApplication(applicationName, head, out existingApplication, device);
        }

        // caller: ActionFramework
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, Content context, out bool existingApplication)
        {
            return GetApplication(applicationName, context, out existingApplication, null);
        }
        public Application GetApplication(string applicationName, Content context, out bool existingApplication, string device)
        {
            var app = GetApplicationsInternal(applicationName, context, null, device).FirstOrDefault();

            existingApplication = app != null || Exists(applicationName);

            return app;
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public Application GetApplication(string applicationName, NodeHead head, out bool existingApplication)
        {
            return GetApplication(applicationName, head, out existingApplication, null);
        }
        public Application GetApplication(string applicationName, NodeHead head, out bool existingApplication, string device)
        {
            var app = GetApplicationsInternal(applicationName, head, null, device).FirstOrDefault();

            existingApplication = app != null || Exists(applicationName);

            return app;
        }

        // caller: ApplicationListPresenterPortlet
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(Content context)
        {
            return GetApplications(context, null);
        }
        public List<Application> GetApplications(Content context, string device)
        {
            return GetApplicationsInternal(null, context, null, device);
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(NodeHead head)
        {
            return GetApplications(head, null);
        }
        public List<Application> GetApplications(NodeHead head, string device)
        {
            return GetApplicationsInternal(null, head, null, device);
        }

        // caller: ActionFramework
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(string scenarioName, Content context)
        {
            return GetApplications(scenarioName, context, null);
        }
        public List<Application> GetApplications(string scenarioName, Content context, string device)
        {
            return GetApplicationsInternal(null, context, scenarioName, device);
        }

        // caller: nobody
        [Obsolete("Use an override with device")]
        public List<Application> GetApplications(string scenarioName, NodeHead head)
        {
            return GetApplications(scenarioName, head, null);
        }
        public List<Application> GetApplications(string scenarioName, NodeHead head, string device)
        {
            return GetApplicationsInternal(null, head, scenarioName, device);
        }

        private List<Application> GetApplicationsInternal(string appName, Content context, string scenarioName, string device)
        {
            return GetApplicationsInternal(appName, context == null ? null : NodeHead.Get(context.Path), scenarioName, device);
        }

        public bool Exists(string applicationName)
        {
            if (string.IsNullOrEmpty(applicationName) || _appNames == null)
                return false;

            applicationName = applicationName.ToLower();
            return (_appNames.Count(a => a.ToLower() == applicationName) > 0);
        }

        //=================================================================================== Distributed invalidate

        [Serializable]
        internal class ApplicationStorageInvalidateDistributedAction : DistributedAction
        {
            public override void DoAction(bool onRemote, bool isFromMe)
            {
                if (onRemote && isFromMe)
                    return;
                ApplicationStorage.InvalidatePrivate();
            }
        }

        private static void DistributedInvalidate()
        {
            new ApplicationStorageInvalidateDistributedAction().Execute();
        }

        private static void InvalidatePrivate()
        {
            Logger.WriteInformation("ApplicationStorage invalidate");
            _instance = null;
        }

        public static void Invalidate()
        {
            DistributedInvalidate();
        }

        public static bool InvalidateByNode(Node node)
        {
            return ApplicationStorage.Instance.InvalidateByNodeInternal(node);
        }

        public static bool InvalidateByPath(string path)
        {
            return ApplicationStorage.Instance.InvalidateByPathInternal(path);
        }

        private bool InvalidateByNodeInternal(Node node)
        {
            if (__rootAppNode == null || node == null)
                return false;

            if (node is Application)
            {
                Invalidate();
                return true;
            }

            return InvalidateByPathInternal(node.Path);
        }

        private bool InvalidateByPathInternal(string path)
        {
            if (__rootAppNode == null || string.IsNullOrEmpty(path))
                return false;

            if ((from app in _appList
                 where app.Path.StartsWith(path)
                 select app).Count() > 0)
            {
                Invalidate();
                return true;
            }

            return false;
        }

        //=================================================================================== Comparer class

        private class ApplicationComparer : IComparer<Application>
        {
            public int Compare(Application x, Application y)
            {
                if (x == null || y == null)
                    return 0;
                return x.Index.CompareTo(y.Index);
            }
        }
    }
#endif
}
