using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace SenseNet.Benchmarking
{
    public class Logger
    {
        /* ==================================================================================================== General */
        private static double _timerSpeed = 1.0; // in seconds
        private static System.Timers.Timer _logTimer;
        private static Stopwatch _stopper;
        
        public static double Elapsed
        {
            get { return _stopper == null ? 0 : _stopper.Elapsed.TotalSeconds; }
        }

        private static string GetLogDate()
        {
            return DateTime.Now.ToString("MMdd-HHmmss");
        }
        public static void InitLogs() 
        {
            _stopper = Stopwatch.StartNew();

            // make sure Logs folder exists
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");

            var date = GetLogDate();
            var markup = "Logs\\BenchmarkLog-{0}-error-{1}.csv";
            _errorLogPath = string.Format(markup, Configuration.SessionName, date);
            markup = "Logs\\BenchmarkLog-{0}-summary-{1}.csv";
            _summaryLogPath = string.Format(markup, Configuration.SessionName, date);
            using (var fs = new FileStream(_summaryLogPath, FileMode.Create))
            {
                using (var wr = new StreamWriter(fs))
                {
                    wr.WriteLine("elapsedtime (sec);folders(total);files(total);size(total);avg CPS (total);avg MBPs (total);actual folders;actual files;actual size;actual CPS;actual files/folders;timestamp;timestamp hh;timestamp mm;timestamp ss.fff;queue length;" + string.Join(";", Program.ResponseLogHeader));
                }
            }

            if (Configuration.SaveDetailedLog)
            {
                markup = "Logs\\BenchmarkLog-{0}-details-{1}.csv";
                _detailedLogPath = string.Format(markup, Configuration.SessionName, date);

                using (var fs = new FileStream(_detailedLogPath, FileMode.Create))
                {
                    using (var wr = new StreamWriter(fs))
                    {
                        wr.WriteLine("id;url;path;level;contenttype;size;waitlength;responsetime;totalduration;error;" + string.Join(";", Program.ResponseLogHeader));
                    }
                }
            }

            _detailedLogLines = string.Empty;
            _logTimer = new System.Timers.Timer(_timerSpeed * 1000.0);
            _logTimer.Elapsed += LogTimer_Elapsed;
            _logTimer.Start();
        }
        public static void StopLogs()
        {
            _logTimer.Stop();
            _stopper.Stop();
        }
        public static void FlushLogs()
        {
            FlushDetailedLog();
            FlushSummaryLog();
        }
        private static void LogTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            FlushLogs();
        }

        /* ==================================================================================================== Error log */
        private static string _errorLogPath;
        private static object _errorLogSync = new object();
        public static void WriteErrorToLog(string path, Uri uri, WebException webex)
        {
            try
            {
                var reader = new StreamReader(webex.Response.GetResponseStream());
                var content = reader.ReadToEnd();

                lock (_errorLogSync)
                {
                    using (var writer = new StreamWriter(_errorLogPath, true))
                    {
                        writer.WriteLine("------------------------");
                        writer.WriteLine("Timestamp: " + DateTime.Now);
                        writer.WriteLine("------------------------");
                        writer.WriteLine(path);
                        writer.WriteLine("URI: " + uri.ToString()); 
                        writer.WriteLine(webex.Message);
                        writer.WriteLine("      -------------server error-----------");
                        writer.WriteLine(content);
                        writer.WriteLine("      -------------exception-----------");
                        var ex = (Exception)webex;
                        while (ex != null)
                        {
                            writer.WriteLine(ex.ToString());
                            ex = ex.InnerException;
                        }

                        writer.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                WriteErrorToLog(path, uri, ex.Message);
            }
        }
        public static void WriteErrorToLog(string path, Uri uri, string error)
        {
            lock (_errorLogSync)
            {
                using (var writer = new StreamWriter(_errorLogPath, true))
                {
                    writer.WriteLine("------------------------");
                    writer.WriteLine("Timestamp: " + DateTime.Now);
                    writer.WriteLine("------------------------");
                    writer.WriteLine(path);
                    writer.WriteLine("URI: " + uri.ToString());
                    writer.WriteLine(error);
                    writer.WriteLine();
                }
            }
        }
        public static void WriteErrorToConsole(string error)
        {
            Console.WriteLine("ERROR: " + error);
        }

        /* ==================================================================================================== Detailed log */
        private static string _detailedLogPath;
        private static object _detailedLogSync = new object();
        private static string _detailedLogLines;
        private static string FormatTimeStamp(TimeSpan t)
        {
            return Math.Floor(t.TotalSeconds) + "," + t.ToString("fffffff");
        }
        internal static void WriteRequest(string url)
        {
            if (!Configuration.SaveDetailedLog)
                return;

            lock (_detailedLogSync)
            {
                _detailedLogLines += url + Environment.NewLine;
            }
        }
        internal static void WriteDetail(RequestState requestState, bool error)
        {
            if (!Configuration.SaveDetailedLog)
                return;

            var waitLength = (requestState.StartTime - requestState.InitTime);
            var responseTime = requestState.ResponseTime;
            var totalDuration = waitLength + responseTime;

            var line = string.Concat(
                requestState.Id, ";",
                requestState.Request.RequestUri, ";",
                requestState.SnPath, ";",
                requestState.Level, ";",
                requestState.ContentTypeName, ";",
                requestState.FileSize, ";",
                FormatTimeStamp(waitLength), ";",
                FormatTimeStamp(responseTime), ";",
                FormatTimeStamp(totalDuration), ";",
                error ? "ERROR" : "OK",
                GetResponseLogData(requestState.Response == null ? null : requestState.Response.ResponseLog)
                );
            lock (_detailedLogSync)
            {
                _detailedLogLines += line + Environment.NewLine;
            }
        }
        private static void FlushDetailedLog()
        {
            if (!Configuration.SaveDetailedLog)
                return;

            lock (_detailedLogSync)
            {
                if (string.IsNullOrEmpty(_detailedLogLines))
                    return;

                using (var writer = new StreamWriter(_detailedLogPath, true))
                {
                    writer.Write(_detailedLogLines);
                }
                _detailedLogLines = string.Empty;
            }
        }

        private static string GetResponseLogData(IDictionary<string, string> logData)
        {
            if (logData == null || logData.Count == 0)
                return new string(';', Program.ResponseLogHeader.Count);

            var sb = new StringBuilder();
            foreach (var header in Program.ResponseLogHeader)
            {
                sb.AppendFormat(";{0}", logData[header]);
            }

            return sb.ToString();
        }

        /* ==================================================================================================== Summary log */
        private static string _summaryLogPath;
        private static object _summaryLogSync = new object();
        private static int divider = (1024 * 1024);
        private static List<long> _cps20Values = new List<long>();
        private static long _cps20Sum;

        private static void FlushSummaryLog()
        {
            var actualSize = (Program._size - Program._sizeprev);
            var sizeMB = String.Format("{0:0.00}", Program._size / divider);
            var sizeprevMB = String.Format("{0:0.00}", actualSize / divider);
            var elapsed = (int)_stopper.Elapsed.TotalSeconds;
            var avgcpstotal = String.Format("{0:0.00}", elapsed == 0 ? 0.0 : (Program._files + Program._folders) / elapsed);
            var avgmbps = String.Format("{0:0.00}", elapsed == 0 ? 0.0 : Program._size / divider / elapsed);
            var actualFiles = (Program._files - Program._filesprev);
            var actualFolders = (Program._folders - Program._foldersprev);
            var ff = String.Format("{0:0.00}", actualFolders == 0 ? (double)0 : (double)actualFiles / (double)actualFolders);
            var actualCps = (actualFiles + actualFolders) / _timerSpeed;
            var queueLength = Program.TaskQueueCount;
            var savedQueueLength = Program.GetUnprocessedFolderTasksCount();

            var actualCpsLong = Convert.ToInt64(actualCps);
            Program._cpsPerfCounter.RawValue = actualCpsLong;

            var avg = Convert.ToInt64((Program._files + Program._folders) / Math.Max(1, elapsed));
            Program._cpsAvgPerfCounter.RawValue = avg;

            _cps20Values.Add(actualCpsLong);
            _cps20Sum += actualCpsLong;
            if (_cps20Values.Count > 20)
            {
                _cps20Sum -= _cps20Values[0];
                _cps20Values.RemoveAt(0);
            }
            Program._cpsAvg20PerfCounter.RawValue = _cps20Sum / _cps20Values.Count;

            var now = DateTime.Now;
            Console.Write("{0}\t{1}/{2}\t{3}/", elapsed, actualFolders, Program._folders, actualFiles);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Program._files);
            Console.ResetColor();
            Console.WriteLine("\t{0}/{1}MB\tQ:{2}->{3}", sizeprevMB, sizeMB, savedQueueLength, queueLength);

            Console.Write("\t");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("CPS: {0}", actualCps);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("/{0}", avgcpstotal);
            Console.ResetColor();
            Console.WriteLine("\tF/F: {0}\tMBps: {1}\tT:{2}", ff, avgmbps, Program._threadCount);

            var logline = string.Concat(
                elapsed, ";",
                Program._folders, ";",
                Program._files, ";",
                Program._size, ";",
                avgcpstotal, ";",
                avgmbps, ";",
                actualFolders, ";", 
                actualFiles, ";",
                actualSize, ";",
                actualCps, ";",
                ff, ";",
                now.ToString("yyyy-MM-dd HH:mm:ss.ffff"), ";",
                now.ToString("HH;mm;ss,ffff"), ";",
                queueLength, ";",
                string.Join(";", Program.GetResponseLogCountersInSecondAndReset())
                );

            lock (_summaryLogSync)
            {
                using (var writer = new StreamWriter(_summaryLogPath, true))
                {
                    writer.WriteLine(logline);
                }
            }

            Program._foldersprev = Program._folders;
            Program._filesprev = Program._files;
            Program._sizeprev = Program._size;
        }
    }
}
