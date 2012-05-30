using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.Xml;
using SenseNet.ContentRepository.Schema;
using System.Reflection;
using SenseNet.Search;

namespace SenseNet.Tools.ContentExporter
{
    class Exporter
    {
        private static int exceptions;
        private static string CR = Environment.NewLine;
        private static string UsageScreen = String.Concat(
            //   0         1         2         3         4         5         6         7         8
            //   012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
            CR,
            "Sense/Net Content Repository Export tool Usage:", CR,
            "Export [-?] [-HELP]", CR,
            "Export [-SOURCE <source>] -TARGET <target> [-ASM <asm>]", CR,
            CR,
            "Parameters:", CR,
            "<source>: Sense/Net Content Repository path as the export root (default: /Root)", CR,
            "<target>: Directory that will contain exported contents. ", CR,
            "          Can be valid local or network filesystem path.", CR,
            "<asm>:    FileSystem folder containig the required assemblies", CR,
            "          (default: location of Export.exe)", CR
        );
        private static List<string> ForbiddenFileNames = new List<string>(new string[] { "PRN", "LST", "TTY", "CRT", "CON" });
        internal static List<string> ArgNames = new List<string>(new string[] { "SOURCE", "TARGET", "ASM", "FILTER", "WAIT" });
        internal static bool ParseParameters(string[] args, List<string> argNames, out Dictionary<string, string> parameters, out string message)
        {
            message = null;
            parameters = new Dictionary<string, string>();
            if (args.Length == 0)
                return false;

            int argIndex = -1;
            int paramIndex = -1;
            string paramToken = null;
            while (++argIndex < args.Length)
            {
                string arg = args[argIndex];
                if (arg.StartsWith("-"))
                {
                    paramToken = arg.Substring(1).ToUpper();

                    if (paramToken == "?" || paramToken == "HELP")
                        return false;

                    paramIndex = ArgNames.IndexOf(paramToken);
                    if (!argNames.Contains(paramToken))
                    {
                        message = "Unknown argument: " + arg;
                        return false;
                    }
                    parameters.Add(paramToken, null);
                }
                else
                {
                    if (paramToken != null)
                    {
                        parameters[paramToken] = arg;
                        paramToken = null;
                    }
                    else
                    {
                        message = String.Concat("Missing parameter name before '", arg, "'");
                        return false;
                    }
                }
            }
            return true;
        }
        private static void Usage(string message)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Console.WriteLine("--------------------");
                Console.WriteLine(message);
                Console.WriteLine("--------------------");
            }
            Console.WriteLine(UsageScreen);
        }

        static void Main(string[] args)
        {
            Dictionary<string, string> parameters;
            string message;
            if (!ParseParameters(args, ArgNames, out parameters, out message))
            {
                Usage(message);
                return;
            }

            string repositoryPath = parameters.ContainsKey("SOURCE") ? parameters["SOURCE"] : "/Root";
            string asmPath = parameters.ContainsKey("ASM") ? parameters["ASM"] : null;
            string fsPath = parameters.ContainsKey("TARGET") ? parameters["TARGET"] : null;
            bool waitForAttach = parameters.ContainsKey("WAIT");
            string queryPath = parameters.ContainsKey("FILTER") ? parameters["FILTER"] : null;
            if (fsPath == null)
            {
                Usage("Missing -TARGET parameter" + CR);
                return;
            }

            try
            {
                CreateLog();
                if (waitForAttach)
                {
                    LogWriteLine("Running in wait mode - now you can attach to the process with a debugger.");
                    LogWriteLine("Press ENTER to continue.");
                    Console.ReadLine();
                }

                var startSettings = new RepositoryStartSettings
                {
                    Console = Console.Out,
                    StartLuceneManager = true,
                    StartWorkflowEngine = false
                };

                using (Repository.Start(startSettings))
                {
                    Export(repositoryPath, fsPath, queryPath);
                }
            }
            catch (Exception e)
            {
                LogWriteLine("Export ends with error:");
                LogWriteLine(e);
                LogWriteLine(e.StackTrace);
            }
        }

        private static void Export(string repositoryPath, string fsPath, string queryPath)
        {
            try
            {
                //-- check fs folder
                DirectoryInfo dirInfo = new DirectoryInfo(fsPath);
                if (!dirInfo.Exists)
                {
                    LogWrite("Creating target directory: ", fsPath, " ... ");
                    Directory.CreateDirectory(fsPath);
                    LogWriteLine("Ok");
                }
                else
                {
                    LogWriteLine("Target directory exists: ", fsPath, ". Exported contents will override existing subelements.");
                }

                //-- load export root
                Content root = Content.Load(repositoryPath);
                if (root == null)
                {
                    LogWriteLine();
                    LogWriteLine("Content does not exist: ", repositoryPath);
                }
                else
                {
                    LogWriteLine();
                    LogWriteLine("=========================== Export ===========================");
                    LogWriteLine("From: ", repositoryPath);
                    LogWriteLine("To:   ", fsPath);
                    if (queryPath != null)
                        LogWriteLine("Filter: ", queryPath);
                    LogWriteLine("==============================================================");
                    var context = new ExportContext(repositoryPath, fsPath);
                    if (queryPath != null)
                        ExportByFilter(root, context, fsPath, queryPath);
                    else
                        ExportContentTree(root, context, fsPath, "");
                    LogWriteLine("--------------------------------------------------------------");
                    LogWriteLine("Outer references:");
                    var outerRefs = context.GetOuterReferences();
                    if (outerRefs.Count == 0)
                        LogWriteLine("All references are exported.");
                    else
                        foreach (var item in outerRefs)
                            LogWriteLine(item);
                }
            }
            catch (Exception e)
            {
                PrintException(e, fsPath);
            }

            LogWriteLine("==============================================================");
            if (exceptions == 0)
                LogWriteLine("Export is successfully finished.");
            else
                LogWriteLine("Export is finished with ", exceptions, " errors.");
            LogWriteLine("Read log file: ", _logFilePath);
        }

        private static void ExportContentTree(Content content, ExportContext context, string fsPath, string indent)
        {
            try
            {
                ExportContent(content, context, fsPath, indent);
            }
            catch (Exception ex)
            {
                var path = content == null ? fsPath : content.Path;
                PrintException(ex, path);
                return;
            }
            
            //TODO: SmartFolder may contain real items too
            if (content.ContentHandler is SmartFolder)
                return;
            
            // create folder only if it has children
            var contentAsFolder = content.ContentHandler as IFolder;
            var contentAsGeneric = content.ContentHandler as GenericContent;

            //try everything that can have children (generic content, content types or other non-gc nodes)
            if (contentAsFolder == null && contentAsGeneric == null)
                return;

            try
            {
                var settings = new QuerySettings {EnableAutofilters = false, EnableLifespanFilter = false};
                var queryResult = contentAsFolder == null ? contentAsGeneric.GetChildren(settings) : contentAsFolder.GetChildren(settings);
                if (queryResult.Count == 0)
                    return;

                var children = queryResult.Nodes;
                var newDir = Path.Combine(fsPath, GetSafeFileNameFromContentName(content.Name));

                if (!(content.ContentHandler is ContentType))
                    Directory.CreateDirectory(newDir);

                var newIndent = indent + "  ";
                foreach (var childContent in from node in children select Content.Create(node))
                    ExportContentTree(childContent, context, newDir, newIndent);
            }
            catch (Exception ex)
            {
                PrintException(ex, fsPath);
            }
        }
        private static void ExportContent(Content content, ExportContext context, string fsPath, string indent)
        {
            if (content.ContentHandler is ContentType)
            {
                LogWriteLine(indent, content.Name);
                ExportContentType(content, context, indent);
                return;
            }
            context.CurrentDirectory = fsPath;
            LogWriteLine(indent, content.Name);
            string metaFilePath = Path.Combine(fsPath, content.Name + ".Content");
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            settings.IndentChars = "  ";
            XmlWriter writer = null;
            try
            {
                writer = XmlWriter.Create(metaFilePath, settings);

                //<?xml version="1.0" encoding="utf-8"?>
                //<ContentMetaData>
                //    <ContentType>Site</ContentType>
                //    <Fields>
                //        ...
                writer.WriteStartDocument();
                writer.WriteStartElement("ContentMetaData");
                writer.WriteElementString("ContentType", content.ContentType.Name);
                writer.WriteElementString("ContentName", content.Name);
                writer.WriteStartElement("Fields");
                try
                {
                    content.ExportFieldData(writer, context);
                }
                catch (Exception e)
                {
                    PrintException(e, fsPath);
                    writer.WriteComment(String.Concat("EXPORT ERROR", CR, e.Message, CR, e.StackTrace));
                }
                writer.WriteEndElement();
                writer.WriteStartElement("Permissions");
                writer.WriteElementString("Clear", null);
                content.ContentHandler.Security.ExportPermissions(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                    writer.Close();
                }
            }
        }
        private static string GetSafeFileNameFromContentName(string name)
        {
            if (ForbiddenFileNames.Contains(name.ToUpper()))
                return name + "!";
            return name;
        }
        private static void ExportByFilter(Content root, ExportContext context, string fsRoot, string queryPath)
        {
            string queryText = null;
            using (var reader = new StreamReader(queryPath))
            {
                queryText = reader.ReadToEnd();
            }

            var query = ContentQuery.CreateQuery(queryText);
            var result = query.Execute();
            var maxCount = result.Count;
            var count = 0;
            foreach (var nodeId in result.Identifiers)
            {
                string fsPath = null;
                Content content = null;

                try
                {
                    content = Content.Load(nodeId);
                    var relPath = content.Path.Remove(0, 1).Replace("/", "\\");
                    fsPath = Path.Combine(fsRoot, relPath);
                    var fsDir = Path.GetDirectoryName(fsPath);
                    var dirInfo = new DirectoryInfo(fsDir);
                    if (!dirInfo.Exists)
                        Directory.CreateDirectory(fsDir);

                    ExportContent(content, context, fsDir, String.Concat(++count, "/", maxCount, ": ", relPath, "\\"));
                }
                catch (Exception ex)
                {
                    PrintException(ex, content == null ? fsPath : content.Path);
                }
            }
        }

        private static void ExportContentType(Content content, ExportContext context, string indent)
        {
            BinaryData binaryData = ((ContentType)content.ContentHandler).Binary;

            var name = content.Name + "Ctd.xml";
            var fsPath = Path.Combine(context.ContentTypeDirectory, name);

            Stream source = null;
            FileStream target = null;
            try
            {
                source = binaryData.GetStream();
                target = new FileStream(fsPath, FileMode.Create);
                for (var i = 0; i < source.Length; i++)
                    target.WriteByte((byte)source.ReadByte());
            }
            finally
            {
                if (source != null)
                    source.Close();
                if (target != null)
                {
                    target.Flush();
                    target.Close();
                }
            }
        }

        //================================================================================================================= Logger

        private static string _logFilePath;
        private static bool _lineStart;

        public static void LogWrite(params object[] values)
        {
            StreamWriter writer = OpenLog();
            WriteToLog(writer, values, false);
            CloseLog(writer);
            _lineStart = false;
        }
        public static void LogWriteLine(params object[] values)
        {
            StreamWriter writer = OpenLog();
            WriteToLog(writer, values, true);
            CloseLog(writer);
            _lineStart = true;
        }
        private static void CreateLog()
        {
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exportlog.txt");
            FileStream fs = new FileStream(_logFilePath, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);
            wr.WriteLine("Start exporting ", DateTime.Now, Environment.NewLine, "Log file: ", _logFilePath);
            wr.WriteLine();
            wr.Close();
            _lineStart = true;
        }
        private static StreamWriter OpenLog()
        {
            return new StreamWriter(_logFilePath, true);
        }
        private static void WriteToLog(StreamWriter writer, object[] values, bool newLine)
        {
            if (_lineStart)
            {
                writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"));
                writer.Write("\t");
            }
            foreach (object value in values)
            {
                Console.Write(value);
                writer.Write(value);
            }
            if (newLine)
            {
                Console.WriteLine();
                writer.WriteLine();
            }
        }
        private static void CloseLog(StreamWriter writer)
        {
            writer.Flush();
            writer.Close();
        }

        private static void PrintException(Exception e, string path)
        {
            exceptions++;
            LogWriteLine("========== Exception:");
            if (!String.IsNullOrEmpty(path))
            {
                LogWriteLine("Path: ", path);
                LogWriteLine("---------------------");
            }

            WriteEx(e);
            while ((e = e.InnerException) != null)
            {
                LogWriteLine("---- Inner Exception:");
                WriteEx(e);
            }
            LogWriteLine("=====================");
        }
        private static void WriteEx(Exception e)
        {
            LogWrite(e.GetType().Name);
            LogWrite(": ");
            LogWriteLine(e.Message);
            LogWriteLine(e.StackTrace);
            var ex = e as ReflectionTypeLoadException;
            if (ex != null)
                PrintReflectionTypeLoadException(ex);
        }
        private static void PrintReflectionTypeLoadException(ReflectionTypeLoadException e)
        {
            LogWriteLine("---- LoaderExceptions:");
            var count = 1;
            foreach (var ex in e.LoaderExceptions)
            {
                LogWriteLine("---- LoaderException #" + count++);
                LogWrite(ex.GetType().Name);
                LogWrite(": ");
                LogWriteLine(ex.Message);
                LogWriteLine(ex.StackTrace);
            }
            LogWriteLine("---- LoaderException Types:");
            foreach (var type in e.Types)
            {
                LogWriteLine(type);
            }
        }

    }
}
