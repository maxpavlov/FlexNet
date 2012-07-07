using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ApplicationModel;
using System.Web;
using SnContent = SenseNet.ContentRepository.Content;
using System.Diagnostics;
using SenseNet.Diagnostics;

namespace SenseNet.Benchmarking
{
    [ContentHandler]
    public class Benchmark : IHttpHandler
    {
        internal Stopwatch benchmarkTimer;

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var action = GetRequestParameter("benchmarkaction");
            if (action == null)
                action = string.Empty;

            switch (action.ToLower())
            {
                case "featuretest":
                    FeatureTest(context);
                    break;
                case "createcontent":
                    CreateContent(context);
                    break;
                case "delete":
                    Delete(context);
                    break;
                case "counterheader":
                    GetCounterHeader(context);
                    break;
                default:
                    context.Response.Write("no action<br/>ok");
                    break;
            }
        }

        private void FeatureTest(HttpContext context)
        {
            context.Response.Write("featuretest<br/>ok");
        }

        private void GetCounterHeader(HttpContext context)
        {
            context.Response.Write(string.Join(";", Enum.GetNames(typeof(BenchmarkCounter.CounterName))));
        }

        private void CreateContent(HttpContext context)
        {
            try
            {
                //---- content type

                var contentTypeName = GetRequestParameter("contenttype");
                if (String.IsNullOrEmpty(contentTypeName))
                {
                    WriteError("Parameter 'contenttype' cannot be null or empty.", context);
                    return;
                }

                var contentType = ContentType.GetByName(contentTypeName);
                if (contentType == null)
                {
                    WriteError("Content type not found: " + contentTypeName, context);
                    return;
                }
                //---- create content

                var snPath = GetRequestParameter("snpath");
                if (String.IsNullOrEmpty(snPath))
                {
                    WriteError("Parameter 'snpath' cannot be null or empty.", context);
                    return;
                }

                using (new SenseNet.ContentRepository.Storage.Security.SystemAccount())
                {
                    BenchmarkCounter.Reset();
                    benchmarkTimer = Stopwatch.StartNew();
                    benchmarkTimer.Restart();

                    var parentPath = RepositoryPath.GetParentPath(snPath);

                    BenchmarkCounter.IncrementBy(BenchmarkCounter.CounterName.GetParentPath, benchmarkTimer.ElapsedTicks);
                    benchmarkTimer.Restart();

                    var parent = Node.LoadNode(parentPath);
                    if (parent == null)
                    {
                        WriteError("Cannot load the parent: " + snPath, context);
                        return;
                    }

                    BenchmarkCounter.IncrementBy(BenchmarkCounter.CounterName.LoadParent, benchmarkTimer.ElapsedTicks);
                    benchmarkTimer.Restart();



                    var contentName = RepositoryPath.GetFileName(snPath);
                    var content = SnContent.CreateNew(contentTypeName, parent, contentName);

                    BenchmarkCounter.IncrementBy(BenchmarkCounter.CounterName.ContentCreate, benchmarkTimer.ElapsedTicks);
                    benchmarkTimer.Restart();

                    //---- create binary

                    if (contentTypeName == "File")
                    {
                        var fsPath = GetRequestParameter("fspath");
                        if (String.IsNullOrEmpty(snPath))
                        {
                            WriteError("Parameter 'fspath' cannot be null or empty if the content type is 'File'.", context);
                            return;
                        }

                        using (var stream = context.Request.InputStream)
                        {

                            var binaryData = new BinaryData();
                            binaryData.FileName = fsPath; //.Replace("$amp;", "&");
                            binaryData.SetStream(stream);

                            content["Binary"] = binaryData;
                            BenchmarkCounter.IncrementBy(BenchmarkCounter.CounterName.BinarySet, benchmarkTimer.ElapsedTicks);
                            benchmarkTimer.Restart();


                            using (new SenseNet.ContentRepository.Storage.Security.SystemAccount())
                                content.Save();
                        }
                    }
                    else
                    {
                        BenchmarkCounter.IncrementBy(BenchmarkCounter.CounterName.BinarySet, benchmarkTimer.ElapsedTicks);
                        benchmarkTimer.Restart();

                        content.Save();
                    }

                    BenchmarkCounter.IncrementBy(BenchmarkCounter.CounterName.FullSave, benchmarkTimer.ElapsedTicks);
                    benchmarkTimer.Restart();

                }
            }
            catch (Exception e)
            {
                //WriteError(String.Concat(e.Message, "\r", e.StackTrace), context);
                Logger.WriteException(e);
                WriteError(e, context);
                return;
            }

            WriteCounters(context);

            context.Response.StatusCode = 200;
            context.Response.Write("ok");
        }
        private void WriteCounters(HttpContext context)
        {
            var counters = BenchmarkCounter.GetAll();
            var names = Enum.GetNames(typeof(BenchmarkCounter.CounterName));
            for (int i = 0; i < names.Length; i++)
            {
                context.Response.Write(names[i]);
                context.Response.Write(": ");
                context.Response.Write(counters[i]);
                context.Response.Write("<br/>");
                context.Response.Write(Environment.NewLine);
            }
        }
        private void WriteError(Exception e, HttpContext context)
        {
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.StatusCode = 500;
            while (e != null)
            {
                context.Response.Write(String.Concat(e.Message, "\r", e.StackTrace, "\r\r"));
                e = e.InnerException;
            }
        }
        private void WriteError(string msg, HttpContext context)
        {
            context.Response.TrySkipIisCustomErrors = true;
            context.Response.StatusCode = 500;
            context.Response.Write(msg);
        }
        private static string GetRequestParameter(string paramName)
        {
            return HttpUtility.UrlDecode(HttpContext.Current.Request.Params[paramName]);
        }


        private void Delete(HttpContext context)
        {
            var snPath = GetRequestParameter("snpath");
            if (String.IsNullOrEmpty(snPath))
            {
                WriteError("Parameter 'snpath' cannot be null or empty.", context);
                return;
            }

            try
            {
                var node = Node.LoadNode(snPath);
                if (node != null)
                    using(new SenseNet.ContentRepository.Storage.Security.SystemAccount())
                        node.ForceDelete();
            }
            catch (Exception e)
            {
                //WriteError(String.Concat(e.Message, "\r", e.StackTrace), context);
                WriteError(e, context);
                return;
            }

            context.Response.StatusCode = 200;
            context.Response.Write("ok");

        }

    }
}
