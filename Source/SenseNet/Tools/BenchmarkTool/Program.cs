using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace SenseNet.Benchmarking
{
    public class Program
    {
        private static List<int> _urlRequests;
        private static object _urlRequestsLock = new object();
        private static int _maxUrlRequests;
        private static int _nextId;
        private static int _contentLeft;
        internal static int _threadCount;

        internal static object _responseLogSync = new object();
        internal static List<string> ResponseLogHeader;
        internal static long[] ResponseLogCounters;

        private static object _taskQueueSync = new object();
        private static object _threadCountSync = new object();
        private static Queue<RequestState> _taskQueue = new Queue<RequestState>();
        internal static int TaskQueueCount { get; private set; }

        private static object _unprocessedFolderTasksSync = new object();
        private static Queue<RequestState> _unprocessedFolderTasks = new Queue<RequestState>();
        internal static int GetUnprocessedFolderTasksCount()
        {
            lock (_unprocessedFolderTasksSync)
                return _unprocessedFolderTasks.Count();
        }

        /*========================================================================================= Main */

        static void Main(string[] args)
        {
            _urlRequests = new List<int>();
            foreach (var urlBase in Configuration.Urls)
            {
                _urlRequests.Add(0);
            }
            _maxUrlRequests = Configuration.Threads / Configuration.Urls.Count();

            System.Net.ServicePointManager.DefaultConnectionLimit = 200;
 
            _contentLeft = Configuration.FileProfile.Length;
            for (int i = 0; i < Configuration.FolderProfile.Length; i++)
                _contentLeft *= Configuration.FolderProfile[i];

            DisplayConfiguration();

            Console.Write("press enter to start...");
            Console.ReadLine();

            if (PerformanceCounterCategory.Exists("SenseNet Benchmark"))
                PerformanceCounterCategory.Delete("SenseNet Benchmark");

            try
            {
                InitializeCounterCategory();
                InitializeCounters();
                InitializeLogHeader();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("An error occured while initializing counters and log header:");
                Console.WriteLine(ex.Message);
                return;
            }

            Console.Write("Deleting " + Configuration.ContentRepositoryPath + "...");
            var req = WebRequest.Create(String.Format("{0}?benchmarkaction=delete&snpath={1}", Configuration.Urls[0], Configuration.ContentRepositoryPath));
            var resp = GetResponse(req);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine();
                Console.WriteLine("An error occured while initializing the server:");
                Console.WriteLine(resp.Content);
                return;
            }
            Console.WriteLine(" ok");

            Logger.InitLogs();
            //Email.InitEmail();

            EnqueueTask(new RequestState
            {
                Id = _nextId++,
                ContentTypeName = "Folder",
                Level = 0,
                SnPath = Configuration.ContentRepositoryPath
            });

            Run();

            Logger.StopLogs();
            Logger.FlushLogs();

            if (Debugger.IsAttached)
            {
                Console.Write("Press enter to exit...");
                Console.ReadLine();
            }

            if (PerformanceCounterCategory.Exists("SenseNet Benchmark"))
                PerformanceCounterCategory.Delete("SenseNet Benchmark");
        }
        private static void Run()
        {
            int threadCount, queueCount;
            while (HasWork(out threadCount, out queueCount))
            {
                if (queueCount == 0 || threadCount >= Configuration.Threads)
                    Thread.Sleep(10);
                else
                    StartWork();
            }
        }
        private static void StartWork()
        {
            var task = DequeueTask();
            if (task != null)
            {
                var thstart = new ParameterizedThreadStart(Work);
                var thread = new Thread(thstart);
                NotifyWorkerStarts();
                thread.Start(task);
            }
        }
        private static void Work(object taskParam)
        {
            var task = (RequestState)taskParam;
            CreateRequest(task);
            task.StartTime = DateTime.Now;
            Exception lastError = null;
            try
            {
                if (task.ContentTypeName == "File")
                    SetRequestStream(task.Request, task.FsPath);

                var stopWatch = Stopwatch.StartNew();
                task.Response = GetResponse(task.Request);
                task.ResponseTime = stopWatch.Elapsed;
                task.EndTime = DateTime.Now;
            }
            catch(Exception ex)
            {
                lastError = ex;
            }
            finally
            {
                if (Configuration.EnableEqualLoad)
                {
                    lock (_urlRequestsLock)
                    {
                        var urlIndex = task.UrlIndex;
                        _urlRequests[urlIndex] = _urlRequests[urlIndex] - 1;
                    }
                }
            }
            if (task.Response != null && task.Response.StatusCode == HttpStatusCode.OK)
            {
                if (task.ContentTypeName == "Folder")
                {
                    _folders++;
                }
                else
                {
                    _files++;
                    _size += task.FileSize;
                }
                Logger.WriteDetail(task, false);

                FinalizeTask(task);
            }
            else
            {
                Logger.WriteDetail(task, true);
                if (lastError != null)
                {
                    var webex = lastError as WebException;
                    if (webex != null)
                    {
                        Logger.WriteErrorToLog(task.SnPath, task.Request.RequestUri, webex);
                    }
                    else
                    {
                        var content = string.Concat(lastError.Message, Environment.NewLine, lastError.StackTrace);
                        Logger.WriteErrorToLog(task.SnPath, task.Request.RequestUri, content);
                    }

                    Logger.WriteErrorToConsole(lastError.Message);
                }
                if (task.Response != null && task.Response.StatusCode != HttpStatusCode.OK)
                {
                    var content = string.Concat(task.Response.StatusCode.ToString(), Environment.NewLine, task.Response.Content);
                    Logger.WriteErrorToLog(task.SnPath, task.Request.RequestUri, content);
                    Logger.WriteErrorToConsole("(see details in error log)");
                }
            }

            NotifyWorkerFinished();
        }
        private static void FinalizeTask(RequestState task)
        {
            if (task.ContentTypeName == "File")
                return;

            task.Request = null;
            task.Response = null;

            lock (_unprocessedFolderTasksSync)
                _unprocessedFolderTasks.Enqueue(task);

            if (TaskQueueCount <= Configuration.MaxTaskQueueSize)
            {
                RequestState t = null;
                lock (_unprocessedFolderTasksSync)
                {
                    if (_unprocessedFolderTasks.Count > 0)
                        t = _unprocessedFolderTasks.Dequeue();
                }
                if (t != null)
                    EnqueueTasks(GetChildren(t));
            }
        }

        private static bool HasWork(out int threadCount, out int taskQueueCount)
        {
            int savedTasksCount;
            lock (_threadCountSync)
            {
                threadCount = _threadCount;
                taskQueueCount = _taskQueue.Count;
                savedTasksCount = _unprocessedFolderTasks.Count;
            }
            while (savedTasksCount > 0 && TaskQueueCount <= Configuration.MaxTaskQueueSize)
            {
                RequestState t = null;
                lock (_unprocessedFolderTasksSync)
                {
                    if (_unprocessedFolderTasks.Count > 0)
                    {
                        t = _unprocessedFolderTasks.Dequeue();
                        savedTasksCount = _unprocessedFolderTasks.Count;
                    }
                }
                //Debug.WriteLine("@@> Dequeue #2 " + t.SnPath); 
                if (t != null)
                {
                    EnqueueTasks(GetChildren(t));
                    //Trace.WriteLine(string.Format("@@> ENQUEUE FINISHER: {0}", _taskQueue.Count));
                }
            }
            lock (_threadCountSync)
            {
                threadCount = _threadCount;
                taskQueueCount = _taskQueue.Count;
            }
            return threadCount > 0 || taskQueueCount > 0 || savedTasksCount > 0;
        }
        private static void NotifyWorkerStarts()
        {
            lock (_threadCountSync)
                _threadCount++;
        }
        private static void NotifyWorkerFinished()
        {
            lock (_threadCountSync)
                _threadCount--;
        }

        private static IEnumerable<RequestState> GetChildren(RequestState requestState)
        {
            var level = requestState.Level;
            var fileLevel = Configuration.FolderProfile.Length;
            if (level > fileLevel)
                return new RequestState[0];

            if (level == fileLevel)
                return GetFiles(requestState);

            var count = Configuration.FolderProfile[level];
            var result = new RequestState[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new RequestState
                {
                    Id = _nextId++,
                    ContentTypeName = "Folder",
                    Level = level + 1,
                    SnPath = String.Concat(requestState.SnPath, "/Folder-", i)
                };
            }
            return result;
        }
        private static IEnumerable<RequestState> GetFiles(RequestState requestState)
        {
            var count = Configuration.FileProfile.Length;
            var result = new RequestState[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = new RequestState
                {
                    Id = _nextId++,
                    ContentTypeName = "File",
                    Level = requestState.Level + 1,
                    SnPath = String.Concat(requestState.SnPath, "/", Configuration.FileProfileNames[i]),
                    FsPath = Configuration.FileProfile[i],
                    FileSize = Configuration.FileProfileSizes[i]
                };
            }
            return result;
        }
        private static void EnqueueTask(RequestState requestState)
        {
            lock (_taskQueueSync)
            {
                _taskQueue.Enqueue(requestState);
                TaskQueueCount = _taskQueue.Count;
            }
        }
        private static void EnqueueTasks(IEnumerable<RequestState> tasks)
        {
            lock (_taskQueueSync)
            {
                foreach (var task in tasks)
                    _taskQueue.Enqueue(task);
                TaskQueueCount = _taskQueue.Count;
            }
        }
        private static RequestState DequeueTask()
        {
            lock (_taskQueueSync)
            {
                RequestState x = null;
                if (_taskQueue.Count > 0)
                    x = _taskQueue.Dequeue();
                TaskQueueCount = _taskQueue.Count;
                return x;
            }
        }

        private static void CreateRequest(RequestState requestState)
        {
            var rootUrls = Configuration.Urls;
            var rootUrlCount = rootUrls.Length;
            var urlidx = requestState.Id % rootUrlCount;
            int finalUrlIdx = urlidx;
            var rootUrl = rootUrls[finalUrlIdx];

            if (Configuration.EnableEqualLoad)
            {
                lock (_urlRequestsLock)
                {
                    var currentCount = _urlRequests[finalUrlIdx];
                    while (currentCount >= _maxUrlRequests)
                    {
                        urlidx++;
                        finalUrlIdx = urlidx % rootUrlCount;
                        rootUrl = rootUrls[finalUrlIdx];
                        currentCount = _urlRequests[finalUrlIdx];
                    }
                    _urlRequests[finalUrlIdx] = _urlRequests[finalUrlIdx] + 1;
                    requestState.UrlIndex = finalUrlIdx;
                }
            }

            var uri = String.Format("{0}?benchmarkaction=createcontent&contenttype={1}&snpath={2}{3}",
                rootUrl, requestState.ContentTypeName, requestState.SnPath, requestState.FsPath == null ? string.Empty : "&fspath=" + requestState.FsPath);
            var req = WebRequest.Create(uri);
            req.Timeout = Configuration.RequestTimeout * 1000;
            requestState.Request = req;
        }
        private static void SetRequestStream(WebRequest request, string fsPath)
        {
            byte[] buffer = null;
            int buflen = 0;
            using (var stream = System.IO.File.OpenRead(fsPath))
            {
                buflen = (int)stream.Length;
                buffer = new byte[buflen];
                stream.Read(buffer, 0, buflen);
            }
            request.ContentLength = buflen;
            request.Method = "POST";
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buflen);
            }
        }
        private static Response GetResponse(WebRequest webRequest)
        {
            HttpWebResponse response = null;
            string content;
            try
            {
                response = (HttpWebResponse)webRequest.GetResponse();
                content = GetResponseContent(response);

                return new Response
                {
                    StatusCode = response.StatusCode,
                    Content = content,
                    ResponseLog = GetResponseLog(content)
                };
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    var stream = e.Response.GetResponseStream();
                    using (var reader = new StreamReader(stream))
                        content = reader.ReadToEnd().Replace("<br/>", Environment.NewLine);
                }
                else
                {
                    content = "<cannot read the server error>";
                }

                return new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = String.Concat(e.Message, Environment.NewLine, e.StackTrace, Environment.NewLine, "-------------- Server error:", Environment.NewLine, content)
                };
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
        private static string GetResponseContent(HttpWebResponse resp)
        {
            if (resp == null)
                return null;
            using (var reader = new StreamReader(resp.GetResponseStream()))
                return reader.ReadToEnd().Replace("<br/>", Environment.NewLine);
        }
        private static List<string> GetResponseLogHeader(HttpWebResponse resp)
        {
            var header = GetResponseContent(resp);
            if (string.IsNullOrEmpty(header))
                return new List<string>();

            return header.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        private static Dictionary<string, string> GetResponseLog(string response)
        {
            var respDict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(response))
                return respDict;

            foreach (var respLine in response.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyval = respLine.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                var index = ResponseLogHeader.IndexOf(keyval[0]);
                if (keyval.Length != 2 || index < 0)
                    continue;
                var trimmed = keyval[1].Trim();
                respDict.Add(keyval[0], trimmed);
                lock (_responseLogSync)
                {
                    ResponseLogCounters[index] += long.Parse(trimmed);
                    //ResponseLogCountersCounter++;
                }
            }

            return respDict;
        }
        public static double[] GetResponseLogCountersInSecondAndReset()
        {
            var result = new double[ResponseLogHeader.Count];
            lock (_responseLogSync)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = /*ResponseLogCountersCounter == 0 ? 0.0 :*/ Convert.ToDouble(ResponseLogCounters[i]) /*/ ResponseLogCountersCounter*/ / TimeSpan.TicksPerSecond;
                    ResponseLogCounters[i] = 0;
                }
                //ResponseLogCountersCounter = 0;
            }
            return result;
        }

        /*========================================================================================= Log and counters */

        internal static int _folders;
        internal static int _files;
        internal static long _size;
        internal static int _foldersprev;
        internal static int _filesprev;
        internal static long _sizeprev;

        private static void DisplayConfiguration()
        {
            var nf = new NumberFormatInfo {NumberDecimalDigits = 0, NumberGroupSeparator = " "};
            
            //calculate folder count
            var folderCount = 0;
            if (Configuration.FolderProfile.Length > 0)
            {
                for (var i = Configuration.FolderProfile.Length; i > 0; i--)
                {
                    var multi = 1;
                    for (var j = 0; j < i; j++)
                    {
                        multi = multi * Configuration.FolderProfile[j];
                    }

                    folderCount += multi;
                }
            }

            Console.WriteLine("Thread count:            " + Configuration.Threads);
            //Console.WriteLine("Max file count:          " + Configuration.MaxFileCount.ToString("N", nf));
            Console.WriteLine("Max task queue length    " + Configuration.MaxTaskQueueSize.ToString("N", nf)); 
            Console.WriteLine("Folder profile:          " + string.Join(", ", Configuration.FolderProfile));
            Console.WriteLine("Actual folder count:     " + folderCount.ToString("N", nf));
            Console.WriteLine("Actual file count:       " + _contentLeft.ToString("N", nf));
            Console.WriteLine("File root path:          " + Configuration.FileRootPath);
            Console.WriteLine("Average file size:       " + Math.Round(Configuration.FileProfileSizes.Average() / 1024) + " KB");
            Console.WriteLine("Content Repository path: " + Configuration.ContentRepositoryPath);
            Console.WriteLine("Urls:                    " + Configuration.Urls.FirstOrDefault());
            foreach (var url in Configuration.Urls.Skip(1))
            {
                Console.WriteLine("                         " + url);
            }
            Console.WriteLine("Session name:            " + Configuration.SessionName);
            Console.WriteLine("Save detailed log:       " + Configuration.SaveDetailedLog);
            Console.WriteLine("Enable equal load:       " + Configuration.EnableEqualLoad);
            Console.WriteLine("Ticks per second:        " + TimeSpan.TicksPerSecond.ToString("N", nf));
        }

        /*========================================================================================= */

        internal static PerformanceCounter _cpsPerfCounter;
        internal static PerformanceCounter _cpsAvgPerfCounter;
        internal static PerformanceCounter _cpsAvg20PerfCounter;

        private static void InitializeCounterCategory()
        {
            if (!PerformanceCounterCategory.Exists("SenseNet Benchmark"))
            {
                PerformanceCounterCategory.Create(
                    "SenseNet Benchmark", "Performance counters of the Sense/Net Benchmark Tool.", PerformanceCounterCategoryType.SingleInstance,
                    new CounterCreationDataCollection(
                        new[] {
                            new CounterCreationData
                            {
                                CounterType = PerformanceCounterType.NumberOfItems32,
                                CounterName = "CPS"
                            },
                            new CounterCreationData
                            {
                                CounterType = PerformanceCounterType.NumberOfItems32,
                                CounterName = "CPSAVG"
                            },
                            new CounterCreationData
                            {
                                CounterType = PerformanceCounterType.NumberOfItems32,
                                CounterName = "CPSAVG20"
                            }
                        }));
            }
        }
        private static void InitializeCounters()
        {
            _cpsPerfCounter = new PerformanceCounter("SenseNet Benchmark", "CPS", false);
            _cpsPerfCounter.RawValue = 0;
            _cpsAvgPerfCounter = new PerformanceCounter("SenseNet Benchmark", "CPSAVG", false);
            _cpsAvgPerfCounter.RawValue = 0;
            _cpsAvg20PerfCounter = new PerformanceCounter("SenseNet Benchmark", "CPSAVG20", false);
            _cpsAvg20PerfCounter.RawValue = 0;
        }
        private static void InitializeLogHeader()
        {
            var req = WebRequest.Create(String.Format("{0}?benchmarkaction=counterheader", Configuration.Urls[0]));
            using (var rp = (HttpWebResponse)req.GetResponse())
            {
                ResponseLogHeader = GetResponseLogHeader(rp);
                ResponseLogCounters = new long[ResponseLogHeader.Count];
            }
        }
    }
}
