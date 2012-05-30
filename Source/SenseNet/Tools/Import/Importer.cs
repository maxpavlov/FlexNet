using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using IO = System.IO;
using SNC = SenseNet.ContentRepository;
using SenseNet.Portal;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using SenseNet.ContentRepository.Storage.Data;

namespace SenseNet.Tools.ContentImporter
{
    static class Importer
    {
        private static string _continueFrom;
        private static int _exceptions;
        private static string CR = Environment.NewLine;
        private static string UsageScreen = String.Concat(
            //   0         1         2         3         4         5         6         7      |
            //   01234567890123456789012345678901234567890123456789012345678901234567890123456|
            "",
            "Sense/Net Content Repository Import tool Usage:", CR,
            "Import [-?] [-HELP]", CR,
            "Import [-CTD <ctd>] [-SOURCE <source> [-TARGET <target>]] [-ASM <asm>]", CR,
            "       [-CONTINUEFROM <continuepath>] [-NOVALIDATE]", CR,
            CR,
            "Parameters:", CR,
            "<ctd>:          File or directory contains Content Type Definitions", CR,
            "<source>:       File or directory contains contents to import", CR,
            "<target>:       Sense/Net Content Repository container as the import target", CR,
            "                (default: /Root)", CR,
            "<asm>:          FileSystem directory containig the required assemblies", CR,
            "                (default: location of Import.exe)", CR,
            "<continuepath>: FileSystem directory or file as the restart point of the", CR,
            "                <source> tree", CR,
            "NOVALIDATE:     Disables the content validaton.", CR,
            CR,
            "Comments:", CR,
            "The <ctd> and <source> paths can be valid local or network filesystem path.", CR,
            "Schema element import (ContentTypeDefinitions) will be skipped if <ctd> are  ", CR,
            "      contained by <source>.", CR,
            CR
        );

        internal static List<string> ArgNames = new List<string>(new string[] { "CTD", "SOURCE", "TARGET", "ASM", "CONTINUEFROM", "NOVALIDATE", "WAIT" });
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

            string ctdPath = parameters.ContainsKey("CTD") ? parameters["CTD"] : null;
            string asmPath = parameters.ContainsKey("ASM") ? parameters["ASM"] : null;
            string fsPath = parameters.ContainsKey("SOURCE") ? parameters["SOURCE"] : null;
            string repositoryPath = parameters.ContainsKey("TARGET") ? parameters["TARGET"] : "/Root";
            _continueFrom = parameters.ContainsKey("CONTINUEFROM") ? parameters["CONTINUEFROM"] : null;
            bool validate = !parameters.ContainsKey("NOVALIDATE");
            bool waitForAttach = parameters.ContainsKey("WAIT");

            //-- Path existence checks
            StringBuilder errorSb = new StringBuilder();
            if (ctdPath != null && !Directory.Exists(ctdPath) && !IO.File.Exists(ctdPath))
                errorSb.Append("Path does not exist: -CTD \"").Append(ctdPath).Append("\"").Append(CR);
            if (fsPath != null && !Directory.Exists(fsPath) && !IO.File.Exists(fsPath))
                errorSb.Append("Path does not exist: -SOURCE \"").Append(fsPath).Append("\"").Append(CR);
            if (asmPath != null && !Directory.Exists(asmPath) && !IO.File.Exists(asmPath))
                errorSb.Append("Path does not exist: -ASM \"").Append(asmPath).Append("\"").Append(CR);
            if (_continueFrom != null && !Directory.Exists(_continueFrom) && !IO.File.Exists(_continueFrom))
                errorSb.Append("Path does not exist: -CONTINUEFROM \"").Append(_continueFrom).Append("\"").Append(CR);
            if (errorSb.Length > 0)
            {
                Usage(errorSb.ToString());
                return;
            }


            try
            {
                if (waitForAttach)
                {
                    Console.WriteLine("Running in wait mode - now you can attach to the process with a debugger.");
                    Console.WriteLine("Press ENTER to continue.");
                    Console.ReadLine();
                }

                CreateLog(_continueFrom == null);
                CreateRefLog(_continueFrom == null);
                Run(ctdPath, asmPath, fsPath, repositoryPath, validate);
            }
            catch (Exception e)
            {
                LogWriteLine();
                LogWriteLine("========================================");
                LogWriteLine("Import ends with error:");
                PrintException(e, null);
            }

            LogWriteLine("========================================");
            if (_exceptions == 0)
                LogWriteLine("Import is successfully finished.");
            else
                LogWriteLine("Import is finished with ", _exceptions, " errors.");
            LogWriteLine("Read log file: ", _logFilePath);

            //LuceneManager.ShutDown();
        }

        internal static void Run_OLD(string ctdPath, string asmPath, string fsPath, string repositoryPath, bool validate)
        {
            //HACK: Preload assemblies
            var preload = Portal.AppModel.HttpActionManager.PresenterFolderName;
            var preload2 = typeof(Lucene.Net.Util.NumericUtils);

            if (String.IsNullOrEmpty(ctdPath) && String.IsNullOrEmpty(fsPath))
            {
                LogWriteLine("No changes");
                return;
            }

            //-- Loading Assemblies
            if (String.IsNullOrEmpty(asmPath))
                LoadAssemblies();
            else
                LoadAssemblies(asmPath);

            //-- Load Engine
            LogWrite("Connecting to Repository ...");
            Folder root = null;
            try
            {
                root = Repository.Root;
            }
            catch (ReflectionTypeLoadException e)
            {
                PrintException(e, null);
                return;
            }

            LogWriteLine("Ok");
            LogWriteLine();

            var installationMode = false;
            try
            {
                installationMode = IsInstallationMode();
            }
            catch(Exception e)
            {
                PrintException(e, null);
                return;
            }

            //-- Install ContentTypes
            if (String.IsNullOrEmpty(ctdPath))
            {
                LogWriteLine("ContentTypeDefinitions are not changed");
            }
            else
            {
                if (installationMode)
                {
                    StorageContext.Search.DisableOuterEngine();
                    LogWriteLine("Indexing is temporarily switched off." + CR);
                }

                InstallContentTypeDefinitions(ctdPath);

                if (installationMode)
                {
                    StorageContext.Search.EnableOuterEngine();
                    CreateInitialIndex();
                    LogWriteLine("Indexing is switched on." + CR);
                }
            }

            //-- Create missing index documents
            SaveInitialIndexDocuments();

            //-- Import Contents
            if (!String.IsNullOrEmpty(fsPath))
                ImportContents(fsPath, repositoryPath, validate);
            else
                LogWriteLine("Contents are not changed");
        }
        internal static void Run(string ctdPath, string asmPath, string fsPath, string repositoryPath, bool validate)
        {
            if (String.IsNullOrEmpty(ctdPath) && String.IsNullOrEmpty(fsPath))
            {
                LogWriteLine("No changes");
                return;
            }

            var startSettings = new RepositoryStartSettings
            {
                Console = Console.Out,
                StartLuceneManager = StorageContext.Search.IsOuterEngineEnabled,
                PluginsPath = asmPath
            };
            using (Repository.Start(startSettings))
            {
                var installationMode = false;
                try
                {
                    installationMode = IsInstallationMode();
                }
                catch (Exception e)
                {
                    PrintException(e, null);
                    return;
                }

                //-- Install ContentTypes
                if (String.IsNullOrEmpty(ctdPath))
                {
                    LogWriteLine("ContentTypeDefinitions are not changed");
                }
                else
                {
                    if (installationMode)
                    {
                        StorageContext.Search.DisableOuterEngine();
                        LogWriteLine("Indexing is temporarily switched off." + CR);
                    }

                    InstallContentTypeDefinitions(ctdPath);

                    if (installationMode)
                    {
                        StorageContext.Search.EnableOuterEngine();
                        CreateInitialIndex();
                        LogWriteLine("Indexing is switched on." + CR);
                    }
                }

                //-- Create missing index documents
                SaveInitialIndexDocuments();

                //-- Import Contents
                if (!String.IsNullOrEmpty(fsPath))
                    ImportContents(fsPath, repositoryPath, validate);
                else
                    LogWriteLine("Contents are not changed");
            }
        }
        private static void SaveInitialIndexDocuments()
        {
            LogWriteLine("Create initial index documents.");
            var idSet = DataProvider.LoadIdsOfNodesThatDoNotHaveIndexDocument();
            var nodes = Node.LoadNodes(idSet);
            foreach (var node in nodes)
            {
                DataBackingStore.SaveIndexDocument(node);
                LogWriteLine("    ", node.Path);
            }
            LogWriteLine("Ok.");
        }

        private static void LoadAssemblies()
        {
            LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory);
        }
        private static void LoadAssemblies(string localBin)
        {
            LogWrite("Loading Assemblies from ");
            LogWrite(localBin);
            LogWriteLine(":");

            string[] names = TypeHandler.LoadAssembliesFrom(localBin);
            foreach (string name in names)
                LogWriteLine(name);

            LogWriteLine("Ok.");
            LogWriteLine();
        }
        private static bool IsInstallationMode()
        {
            LogWriteLine("========================================");
            LogWriteLine("Indexing: " + (StorageContext.Search.IsOuterEngineEnabled ? "enabled" : "disabled"));

            var startupMode = false;
            if (StorageContext.Search.IsOuterEngineEnabled)
            {
                try
                {
                    startupMode = ContentQuery.Query("Type:PortalRoot").Count == 0;
                }
                catch (Exception e)
                {
                    var s = e.Message;
                }
                LogWriteLine("Startup mode: " + (startupMode ? "ON" : "off"));
                LogWriteLine("========================================");
            }
            return startupMode;
        }
        private static void CreateInitialIndex()
        {
            LogWriteLine("========================================");
            LogWriteLine("Create initial index.");
            LogWriteLine();
            var p = StorageContext.Search.SearchEngine.GetPopulator();
            p.NodeIndexed += new EventHandler<NodeIndexedEvenArgs>(Populator_NodeIndexed);
            p.ClearAndPopulateAll();
            LogWriteLine("Ok.");
            LogWriteLine("========================================");
        }

        private static void InstallContentTypeDefinitions(string ctdPath)
        {
            if (IO.File.Exists(ctdPath))
            {
                LogWrite("Installing content type ");
                LogWrite(Path.GetFileName(ctdPath));
                LogWriteLine("...");
                FileStream stream = new FileStream(ctdPath, FileMode.Open, FileAccess.Read);
                ContentTypeInstaller.InstallContentType(stream);
            }
            else
            {
                LogWriteLine("Loading content types...");

                ContentTypeInstaller installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
                foreach (string ctdFilePath in Directory.GetFiles(ctdPath, "*.xml"))
                {
                    LogWrite(Path.GetFileName(ctdFilePath));
                    LogWrite(" ...");

                    FileStream stream = new FileStream(ctdFilePath, FileMode.Open, FileAccess.Read);
                    try
                    {
                        installer.AddContentType(stream);
                    }
                    catch (ApplicationException e)
                    {
                        _exceptions++;
                        LogWriteLine(" SKIPPED: " + e.Message);
                    }

                    LogWriteLine(" Ok");
                }

                LogWriteLine();
                LogWrite("Installing content types...");
                installer.ExecuteBatch();
            }
            LogWriteLine(" Ok");
            LogWriteLine();
        }

        //-- ImportContents
        private static void ImportContents(string srcPath, string targetPath, bool validate)
        {
            bool pathIsFile = false;
            if (IO.File.Exists(srcPath))
            {
                pathIsFile = true;
            }
            else if (!Directory.Exists(srcPath))
            {
                LogWrite("Source directory or file was not found: ");
                LogWriteLine(srcPath);
                return;
            }

            var importTarget = Repository.Root as Node;
            LogWriteLine();
            LogWriteLine("=================== Continuing Import ========================");
            LogWriteLine("From: ", srcPath);
            LogWriteLine("To:   ", targetPath);
            if(_continueFrom != null)
                LogWriteLine("Continuing from: ", _continueFrom);
            if (!validate)
                LogWriteLine("Content validation: OFF");
            LogWriteLine("==============================================================");

            if (targetPath != null)
            {
                importTarget = Node.LoadNode(targetPath);
                if (importTarget == null)
                {
                    LogWrite("Target container was not found: ");
                    LogWriteLine(targetPath);
                    return;
                }
            }

            try
            {
                List<ContentInfo> postponedList = new List<ContentInfo>();
                TreeWalker(srcPath, pathIsFile, importTarget, "  ", postponedList, validate);

                if (postponedList.Count != 0)
                    UpdateReferences(/*postponedList*/validate);
                
                ////hack: add permissions to Visitor
                //var query = new NodeQuery();
                //var nt = ActiveSchema.NodeTypes["Site"];
                //query.Add(new TypeExpression(nt, false));
                //var sites = query.Execute();

                //foreach (var site in sites.Nodes)
                //{
                //    site.Security.SetPermission(User.Visitor, true, PermissionType.RunApplication, PermissionValue.Allow);
                //}
            }
            catch (Exception e)
            {
                PrintException(e, null);
            }
        }

        private static void TreeWalker(string path, bool pathIsFile, Node folder, string indent, List<ContentInfo> postponedList, bool validate)
        {
            // get entries
            // get contents
            // foreach contents
            //   create contentinfo
            //   entries.remove(content)
            //   entries.remove(contentinfo.attachments)
            // foreach entries
            //   create contentinfo
            if (folder.Path.StartsWith(Repository.ContentTypesFolderPath))
            {
                //-- skip CTD folder
                LogWrite("Skipped path: ");
                LogWriteLine(path);
                return;
            }

            string currentDir = pathIsFile ? Path.GetDirectoryName(path) : path;
            List<ContentInfo> contentInfos = new List<ContentInfo>();
            List<string> paths;
            List<string> contentPaths;
            if (pathIsFile)
            {
                paths = new List<string>(new string[] { path });
                contentPaths = new List<string>();
                if (path.ToLower().EndsWith(".content"))
                    contentPaths.Add(path);
            }
            else
            {
                paths = new List<string>(Directory.GetFileSystemEntries(path));
                contentPaths = new List<string>(Directory.GetFiles(path, "*.content"));
            }

            foreach (string contentPath in contentPaths)
            {
                paths.Remove(contentPath);

                try
                {
                    var contentInfo = new ContentInfo(contentPath, folder);
                    if (!contentInfo.IsHidden)
                        contentInfos.Add(contentInfo);
                    foreach (string attachmentName in contentInfo.Attachments)
                    {
                        var attachmentPath = Path.Combine(path, attachmentName);
                        if (!paths.Remove(attachmentPath))
                        {
                            for (int i = 0; i < paths.Count; i++)
                            {
                                if (paths[i].Equals(attachmentPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    paths.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PrintException(e, contentPath);
                }
            }
            while (paths.Count > 0)
            {
                try
                {
                    var contentInfo = new ContentInfo(paths[0], folder);
                    if (!contentInfo.IsHidden)
                        contentInfos.Add(contentInfo);
                }
                catch (Exception e)
                {
                    PrintException(e, paths[0]);
                }

                paths.RemoveAt(0);
            }

            foreach (ContentInfo contentInfo in contentInfos)
            {
                var continuing = false;
                var stepDown = true;
                if (_continueFrom != null)
                {
                    continuing = true;
                    if (contentInfo.MetaDataPath == _continueFrom)
                    {
                        _continueFrom = null;
                        continuing = false;
                    }
                    else
                    {
                        stepDown = _continueFrom.StartsWith(contentInfo.MetaDataPath);
                    }
                }

                var isNewContent = true;
                Content content = null;

                try
                {
                    content = CreateOrLoadContent(contentInfo, folder, out isNewContent);
                }
                catch (Exception ex)
                {
                    PrintException(ex, contentInfo.MetaDataPath);
                }

                if (!continuing && content != null)
                {
                    LogWriteLine(indent, contentInfo.Name, " : ", contentInfo.ContentTypeName, isNewContent ? " [new]" : " [update]");

                    try
                    {
                        if (Console.KeyAvailable)
                            WriteInfo(contentInfo);
                    }
                    catch { }

                    //-- SetMetadata without references. Continue if the setting is false or exception was thrown.
                    try
                    {
                        if (!contentInfo.SetMetadata(content, currentDir, isNewContent, validate, false))
                            PrintFieldErrors(content, contentInfo.MetaDataPath);
                        if (content.ContentHandler.Id == 0)
                            content.ContentHandler.Save();
                    }
                    catch (Exception e)
                    {
                        PrintException(e, contentInfo.MetaDataPath);
                        continue;
                    }

                    if (contentInfo.ClearPermissions)
                    {
                        content.ContentHandler.Security.RemoveExplicitEntries();
                        if (!(contentInfo.HasReference || contentInfo.HasPermissions || contentInfo.HasBreakPermissions))
                        {
                            content.ContentHandler.Security.RemoveBreakInheritance();
                        }
                    }
                    if (contentInfo.HasReference || contentInfo.HasPermissions || contentInfo.HasBreakPermissions)
                    {
                        LogWriteReference(contentInfo);
                        postponedList.Add(contentInfo);
                    }
                }

                //-- recursion
                if (stepDown && content != null)
                {
                    if (contentInfo.IsFolder)
                    {
                        if (content.ContentHandler != null)
                            TreeWalker(contentInfo.ChildrenFolder, false, content.ContentHandler, indent + "  ", postponedList, validate);
                    }
                }
            }
        }

        private static void UpdateReferences(bool validate)
        {
            LogWriteLine();
            LogWriteLine("=========================== Update references");
            LogWriteLine();
            var idList = new List<int>();
            using (var reader = new StreamReader(_refLogFilePath))
            {
                while (!reader.EndOfStream)
                {
                    var s = reader.ReadLine();
                    var sa = s.Split('\t');
                    var id = int.Parse(sa[0]);
                    var path = sa[1];
                    if (idList.Contains(id))
                        continue;
                    UpdateReference(id, path, validate);
                    idList.Add(id);
                }
            }

            LogWriteLine();
        }
        private static void UpdateReference(int contentId, string metadataPath, bool validate)
        {
            var contentInfo = new ContentInfo(metadataPath, null);

            LogWrite("  ");
            LogWriteLine(contentInfo.Name);
            SNC.Content content = SNC.Content.Load(contentId);
            if (content != null)
            {
                try
                {
                    if (!contentInfo.UpdateReferences(content, validate))
                        PrintFieldErrors(content, contentInfo.MetaDataPath);
                }
                catch (Exception e)
                {
                    PrintException(e, contentInfo.MetaDataPath);
                }
            }
            else
            {
                LogWrite("---------- Content does not exist. MetaDataPath: ");
                LogWrite(contentInfo.MetaDataPath);
                LogWrite(", ContentId: ");
                LogWrite(contentInfo.ContentId);
                LogWrite(", ContentTypeName: ");
                LogWrite(contentInfo.ContentTypeName);
            }
        }
        //private static void UpdateReferences(List<ContentInfo> postponedList)
        //{
        //    LogWriteLine();
        //    LogWriteLine("=========================== Update references");
        //    LogWriteLine();

        //    foreach (ContentInfo contentInfo in postponedList)
        //    {
        //        LogWrite("  ");
        //        LogWriteLine(contentInfo.Name);
        //        SNC.Content content = SNC.Content.Load(contentInfo.ContentId);
        //        if (content != null)
        //        {
        //            try
        //            {
        //                if (!contentInfo.UpdateReferences(content))
        //                    PrintFieldErrors(content, contentInfo.MetaDataPath);
        //            }
        //            catch (Exception e)
        //            {
        //                PrintException(e, contentInfo.MetaDataPath);
        //            }
        //        }
        //        else
        //        {
        //            LogWrite("---------- Content does not exist. MetaDataPath: ");
        //            LogWrite(contentInfo.MetaDataPath);
        //            LogWrite(", ContentId: ");
        //            LogWrite(contentInfo.ContentId);
        //            LogWrite(", ContentTypeName: ");
        //            LogWrite(contentInfo.ContentTypeName);
        //        }
        //    }
        //    LogWriteLine();
        //}
        private static Content CreateOrLoadContent(ContentInfo contentInfo, Node folder, out bool isNewContent)
        {
            var path = RepositoryPath.Combine(folder.Path, contentInfo.Name);
            var content = Content.Load(path);
            if (content == null)
            {
                content = Content.CreateNew(contentInfo.ContentTypeName, folder, contentInfo.Name);
                isNewContent = true;
            }
            else
            {
                isNewContent = false;
            }

            return content;
        }
        private static void PrintException(Exception e, string path)
        {
            _exceptions++;
            LogWriteLine("========== Exception:");
            if (!String.IsNullOrEmpty(path))
                LogWriteLine("Path: ", path);
            LogWrite(e.GetType().Name);
            LogWrite(": ");
            LogWriteLine(e.Message);
            PrintTypeLoadError(e as ReflectionTypeLoadException);
            LogWriteLine(e.StackTrace);
            while ((e = e.InnerException) != null)
            {
                LogWriteLine("---- Inner Exception:");
                LogWrite(e.GetType().Name);
                LogWrite(": ");
                LogWriteLine(e.Message);
                PrintTypeLoadError(e as ReflectionTypeLoadException);
                LogWriteLine(e.StackTrace);
            }
            LogWriteLine("=====================");
        }
        private static void PrintTypeLoadError(ReflectionTypeLoadException exc)
        {
            if (exc == null)
                return;
            LogWriteLine("LoaderExceptions:");
            foreach (var e in exc.LoaderExceptions)
            {
                LogWrite("-- ");
                LogWrite(e.GetType().FullName);
                LogWrite(": ");
                LogWriteLine(e.Message);

                var fileNotFoundException = e as FileNotFoundException;
                if (fileNotFoundException != null)
                {
                    LogWriteLine("FUSION LOG:");
                    LogWriteLine(fileNotFoundException.FusionLog);
                }
            }
        }
        private static void PrintFieldErrors(Content content, string path)
        {
            _exceptions++;
            LogWriteLine("---------- Field Errors (path: ", path, "):");
            foreach (string fieldName in content.Fields.Keys)
            {
                Field field = content.Fields[fieldName];
                if (!field.IsValid)
                {
                    LogWrite(field.Name);
                    LogWrite(": ");
                    LogWriteLine(field.GetValidationMessage());
                }
            }
            LogWriteLine("------------------------");
        }

        private static void WriteInfo(ContentInfo contentInfo)
        {
            Console.ReadKey(true);
            LogWriteLine("PAUSED: ", CR, contentInfo.MetaDataPath);
            Console.WriteLine("Errors: " + _exceptions);
            Console.Write("press any key to continue... ");
            Console.ReadKey();
            LogWriteLine("CONTINUED");
        }

        //================================================================================================================= ReferenceLog

        private static string _refLogFilePath;

        public static void LogWriteReference(ContentInfo contentInfo)
        {
            StreamWriter writer = OpenRefLog();
            WriteToRefLog(writer, contentInfo.ContentId, '\t', contentInfo.MetaDataPath);
            CloseLog(writer);
        }
        private static void CreateRefLog(bool createNew)
        {
            _refLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import.reflog");
            if (!IO.File.Exists(_refLogFilePath) || createNew)
            {
                FileStream fs = new FileStream(_refLogFilePath, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);
                wr.Close();
            }
        }
        private static StreamWriter OpenRefLog()
        {
            return new StreamWriter(_refLogFilePath, true);
        }
        private static void WriteToRefLog(StreamWriter writer, params object[] values)
        {
            foreach (object value in values)
            {
                //Console.Write(value);
                writer.Write(value);
            }
            //Console.WriteLine();
            writer.WriteLine();
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
        private static void CreateLog(bool createNew)
        {
            _lineStart = true;
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "importlog.txt");
            if (!IO.File.Exists(_logFilePath) || createNew)
            {
                FileStream fs = new FileStream(_logFilePath, FileMode.Create);
                StreamWriter wr = new StreamWriter(fs);
                wr.WriteLine("Start importing ", DateTime.Now, CR, "Log file: ", _logFilePath);
                wr.WriteLine();
                wr.Close();
            }
            else
            {
                LogWriteLine(CR, CR, "CONTINUING", CR, CR);
            }
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

        //================================================================================================================= Logger

        private static void Populator_NodeIndexed(object sender, NodeIndexedEvenArgs e)
        {
            LogWriteLine(e.Path);
        }

        //private static bool IsIndexingEnabled
        //{
        //    get { return StorageContext.Search.IsOuterEngineEnabled; }
        //    set
        //    {
        //        SetConfigurationFieldValue<bool?>("_isOuterSearchEngineEnabled", value);
        //        SetConfiguration2FieldValue<bool?>("__isOuterEngineEnabled", value);
        //    }
        //}
        //private static void SetConfigurationFieldValue<T>(string fieldName, T value)
        //{
        //    var field = GetConfigurationField(fieldName);
        //    field.SetValue(null, value);
        //}
        //private static FieldInfo GetConfigurationField(string fieldName)
        //{
        //    var field = typeof(RepositoryConfiguration).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        //    return field;
        //}
        //private static void SetConfiguration2FieldValue<T>(string fieldName, T value)
        //{
        //    var field = GetConfiguration2Field(fieldName);
        //    var instanceInfo = typeof(StorageContext).GetProperty("Instance", BindingFlags.Static | BindingFlags.NonPublic);
        //    var instance = instanceInfo.GetValue(null, null);
        //    field.SetValue(instance, value);
        //}
        //private static FieldInfo GetConfiguration2Field(string fieldName)
        //{
        //    var field = typeof(StorageContext).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        //    return field;
        //}

    }
}
