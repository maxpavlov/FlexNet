using System;
using System.Collections.Generic;
using System.Web;
using SenseNet.ContentRepository;
using System.Reflection;
using SenseNet.ContentRepository.Storage;
using System.IO;
using SenseNet.ContentRepository.Storage.Data;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using IO = System.IO;
using SenseNet.ContentRepository.Schema;
using SNC = SenseNet.ContentRepository;
using System.Diagnostics;
using System.Xml;
using SenseNet.Diagnostics;
using SenseNet.Search.Indexing;
using System.Collections.Specialized;
using File = System.IO.File;

namespace SenseNet.Portal.Setup
{
    internal class RunOnce
    {
        private static object _locker = new object();
        internal static string RunOnceGuid = "101C50EF-24FD-441A-A15B-BD33DE431665";

        internal static int TotalCount { get; private set; }
        internal static int ImportedCount { get; private set; }
        internal static string ImportError { get; private set; }

        internal static int Run(System.Web.UI.Page caller)
        {
            var runOnceMarkerPath = caller.MapPath("/" + RunOnceGuid);
            if (!IO.File.Exists(runOnceMarkerPath))
            {
                return 0;
            }
            var runningMarkerPath = caller.MapPath("/779B94A7-7204-45b4-830F-10CC5B5BC0F2");
            lock (_locker)
            {
                if (IO.File.Exists(runningMarkerPath))
                    return 2;
                IO.File.Create(runningMarkerPath).Close();
            }

            var ctdPath = caller.MapPath("/Root/System/Schema/ContentTypes");
            var sourcePath = caller.MapPath("/Root");
            var targetPath = "/Root";
            var asmPath = caller.MapPath("/bin");
            var logPath = caller.MapPath("/install.log");
            var scriptsPath = caller.MapPath("/Scripts");
            var installerUser = HttpContext.Current.Application["SNInstallUser"] as string;

            try
            {
                CreateLog(logPath);
                LoadAssemblies(asmPath);
            }
            catch (Exception e)
            {
                Logger.WriteException(e);

                LogWriteLine();
                LogWriteLine("========================================");
                LogWriteLine("Import ends with error:");
                PrintException(e);

                ImportError = e.Message;
                return 2;
            }

            TotalCount = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories).Length;

            var runOnce = new RunOnce();
            var importDelegate = new ImportDelegate(runOnce.Import);
            importDelegate.BeginInvoke(ctdPath, sourcePath, targetPath, asmPath, runOnceMarkerPath, runningMarkerPath, scriptsPath, logPath, installerUser, null, null);
            return 1;
        }

        delegate void ImportDelegate(string ctdPath, string sourcePath, string targetPath, string asmPath, string runOnceMarkerPath, string runningMarkerPath, string scriptsPath, string logPath, string installerUser);
        private static int exceptions;
        private static string CR = Environment.NewLine;
        private void Import(string ctdPath, string sourcePath, string targetPath, string asmPath, string runOnceMarkerPath, string runningMarkerPath, string scriptsPath, string logPath, string installerUser)
        {
            try
            {
                var originalIsVisitor = false;

                if (AccessProvider.Current.GetCurrentUser().Id == RepositoryConfiguration.VisitorUserId)
                {
                    originalIsVisitor = true;
                    AccessProvider.Current.SetCurrentUser(User.Administrator);
                }

                using (new SystemAccount())
                {
                    CreateRefLog();

                    RunImport(ctdPath, asmPath, sourcePath, targetPath, scriptsPath);
                }

                if (originalIsVisitor)
                    AccessProvider.Current.SetCurrentUser(User.Visitor);
            }
            catch (Exception e)
            {
                Logger.WriteException(e);

                LogWriteLine();
                LogWriteLine("========================================");
                LogWriteLine("Import ends with error:");
                PrintException(e);

                ImportError = e.Message;
            }

            LogWriteLine("========================================");
            if (exceptions == 0)
                LogWriteLine("Import is successfully finished.");
            else
                LogWriteLine("Import is finished with ", exceptions, " errors.");
            LogWriteLine("Read log file: ", _logFilePath);

            lock (_locker)
            {
                new Virtualization.PortalContext.ReloadSiteListDistributedAction().Execute();
                IO.File.Delete(runOnceMarkerPath);
                IO.File.Delete(runningMarkerPath);

                if (string.IsNullOrEmpty(installerUser)) 
                    return;

                var user = User.RegisterUser(installerUser);
                DirectoryServices.Common.SyncInitialUserProperties(user);
            }
        }

        internal static void RunImport(string ctdPath, string asmPath, string fsPath, string repositoryPath, string scriptsPath)
        {
            //Load Engine
            LogWrite("Connecting to Repository ...");
            try
            {
                var root = Repository.Root;
            }
            catch (ReflectionTypeLoadException e)
            {
                PrintException(e);
                ImportError = e.Message;

                return;
            }

            LogWriteLine("Ok");
            LogWriteLine();

            //switch off Lucene while installing content types
            StorageContext.Search.DisableOuterEngine();
            LogWriteLine("Indexing is temporarily switched off." + CR);

            //-- Install ContentTypes
            InstallContentTypeDefinitions(ctdPath);

            //-- Create missing index documents
            SaveInitialIndexDocuments();

            //-- Import Contents
            ImportContents(fsPath, repositoryPath);

            //switch on Lucene again and create initial index
            StorageContext.Search.EnableOuterEngine();
            CreateInitialIndex();
            LogWriteLine("Indexing is switched on." + CR);

            //-- Set permissions
            SetPermissions();
        }
        private static void SaveInitialIndexDocuments()
        {
            LogWriteLine("Create initial index documents.");
            var idSet = SenseNet.ContentRepository.Storage.Data.DataProvider.LoadIdsOfNodesThatDoNotHaveIndexDocument();
            var nodes = Node.LoadNodes(idSet);
            foreach (var node in nodes)
            {
                DataBackingStore.SaveIndexDocument(node);
                LogWriteLine("    ", node.Path);
            }
            LogWriteLine("Ok.");
        }

        private static void InstallContentTypeDefinitions(string ctdPath)
        {
            if (IO.File.Exists(ctdPath))
            {
                LogWrite("Installing content type ");
                LogWrite(IO.Path.GetFileName(ctdPath));
                LogWriteLine("...");
                var stream = new FileStream(ctdPath, FileMode.Open, FileAccess.Read);
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

                    var stream = new FileStream(ctdFilePath, FileMode.Open, FileAccess.Read);
                    try
                    {
                        installer.AddContentType(stream);
                    }
                    catch (ApplicationException e)
                    {
                        exceptions++;
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

        private static void ImportContents(string srcPath, string targetPath)
        {
            var pathIsFile = false;
            if (IO.File.Exists(srcPath))
            {
                pathIsFile = true;
            }
            else if (!Directory.Exists(srcPath))
            {
                const string error = "Source directory or file was not found: ";
                LogWrite(error);
                LogWriteLine(srcPath);
                ImportError = error + srcPath;
                return;
            }

            var importTarget = Repository.Root as Node;
            LogWriteLine();
            LogWriteLine("=========================== Import ===========================");
            LogWriteLine("From: ", srcPath);
            LogWriteLine("To:   ", targetPath);
            LogWriteLine("==============================================================");

            if (targetPath != null)
            {
                importTarget = Node.LoadNode(targetPath);
                if (importTarget == null)
                {
                    const string error = "Target container was not found: ";
                    LogWrite(error);
                    LogWriteLine(targetPath);
                    ImportError = error + targetPath;
                    return;
                }
            }

            try
            {
                var postponedList = new List<ContentInfo>();
                TreeWalker(srcPath, pathIsFile, importTarget, "  ", postponedList);

                if (postponedList.Count != 0)
                    UpdateReferences(/*postponedList*/);
            }
            catch (Exception e)
            {
                PrintException(e);
                ImportError = e.Message;
            }
        }

        private static void TreeWalker(string path, bool pathIsFile, Node folder, string indent, List<ContentInfo> postponedList)
        {
            if (folder.Path.StartsWith(Repository.ContentTypesFolderPath))
            {
                //-- skip CTD folder
                LogWrite("Skipped path: ");
                LogWriteLine(path);
                return;
            }

            var currentDir = pathIsFile ? Path.GetDirectoryName(path) : path;
            var contentInfos = new List<ContentInfo>();
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
                    var contentInfo = new ContentInfo(contentPath);
                    if (!contentInfo.IsHidden)
                        contentInfos.Add(contentInfo);
                    foreach (string attachmentName in contentInfo.Attachments)
                        paths.Remove(Path.Combine(path, attachmentName));
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
                    var contentInfo = new ContentInfo(paths[0]);
                    if (!contentInfo.IsHidden)
                        contentInfos.Add(contentInfo);
                }
                catch (Exception ex)
                {
                    PrintException(ex, paths[0]);
                }

                paths.RemoveAt(0);
            }

            foreach (ContentInfo contentInfo in contentInfos)
            {
                var isNewContent = true;
                Content content = null;

                try
                {
                    content = CreateOrLoadContent(contentInfo, folder, out isNewContent);
                }
                catch (Exception ex)
                {
                    PrintException(ex, contentInfo.MetaDataPath);
                    continue;
                }

                LogWriteLine(indent, contentInfo.Name, " : ", contentInfo.ContentTypeName, isNewContent ? " [new]" : " [update]");

                //-- SetMetadata without references. Return if the setting is false or exception was thrown.
                try
                {
                    if (!contentInfo.SetMetadata(content, currentDir, isNewContent, true, false))
                        PrintFieldErrors(content, contentInfo.MetaDataPath);

                    if (content.ContentHandler.Id == 0)
                        content.ContentHandler.Save();

                    ImportedCount++;
                }
                catch (Exception ex)
                {
                    PrintException(ex, contentInfo.MetaDataPath);
                    ImportError = ex.Message;
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

                //-- recursion
                if (contentInfo.IsFolder)
                {
                    if (content.ContentHandler != null)
                        TreeWalker(contentInfo.ChildrenFolder, false, content.ContentHandler, indent + "  ", postponedList);
                }
            }
        }

        private static void UpdateReferences()
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
                    UpdateReference(id, path);
                    idList.Add(id);
                }
            }

            LogWriteLine();
        }

        private static void UpdateReference(int contentId, string metadataPath)
        {
            var contentInfo = new ContentInfo(metadataPath);

            LogWrite("  ");
            LogWriteLine(contentInfo.Name);
            var content = Content.Load(contentId);
            if (content != null)
            {
                try
                {
                    if (!contentInfo.UpdateReferences(content, true))
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

        private static void SetPermissions()
        {
            try
            {
                var devs = Node.Load<OrganizationalUnit>("/Root/IMS/Demo/Developers");
                var admins = Node.Load<Group>("/Root/IMS/BuiltIn/Portal/Administrators");
                var evr = Group.Everyone;
                var ctSystem = ContentType.GetByName("SystemFolder");

                // Break the permission inheritance on the SystemFolder type
                ctSystem.Security.BreakInheritance();

                // Allow See, Open on SystemFolder for Developers
                // Allow full control on SystemFolder for Administrators
                // Remove (eliminiate) inherited permissions for Everyone and Visitor 
                var acle = ctSystem.Security.GetAclEditor();
                acle.SetPermission(devs, true, PermissionType.See, PermissionValue.Allow)
                    .SetPermission(devs, true, PermissionType.Open, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.See, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.Open, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.OpenMinor, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.AddNew, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.Approve, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.Delete, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.DeleteOldVersion, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.ForceCheckin, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.ManageListsAndWorkspaces, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.Publish, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.RecallOldVersion, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.RunApplication, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.Save, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.SeePermissions, PermissionValue.Allow)
                    .SetPermission(admins, true, PermissionType.SetPermissions, PermissionValue.Allow)
                    .SetPermission(evr, true, PermissionType.See, PermissionValue.NonDefined)
                    .SetPermission(evr, true, PermissionType.Open, PermissionValue.NonDefined)
                    .SetPermission(User.Visitor, true, PermissionType.See, PermissionValue.NonDefined)
                    .SetPermission(User.Visitor, true, PermissionType.Open, PermissionValue.NonDefined);
                acle.Apply();
            }
            catch (Exception ex)
            {
                PrintException(ex);
                ImportError = ex.Message;
            }
        }

        //================================================================================================================= Helper methods

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

        private static void CreateInitialIndex()
        {
            try
            {
                LogWriteLine("========================================");
                LogWriteLine("Create initial index.");
                LogWriteLine();

                var p = StorageContext.Search.SearchEngine.GetPopulator();
                p.ClearAndPopulateAll();

                LuceneManager.Start();

                LogWriteLine("Ok.");
                LogWriteLine("========================================");
            }
            catch (Exception ex)
            {
                LogWriteLine(ex.ToString());
                ImportError = ex.Message;
            }
        }

        //================================================================================================================= ReferenceLog

        private static string _refLogFilePath;

        public static void LogWriteReference(ContentInfo contentInfo)
        {
            StreamWriter writer = OpenRefLog();
            WriteToRefLog(writer, contentInfo.ContentId, '\t', contentInfo.MetaDataPath);
            CloseLog(writer);
        }
        private static void CreateRefLog()
        {
            _refLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "import.reflog");
            if (IO.File.Exists(_refLogFilePath)) 
                return;

            var fs = new FileStream(_refLogFilePath, FileMode.Create);
            var wr = new StreamWriter(fs);
            wr.Close();
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

        private static void PrintException(Exception e)
        {
            PrintException(e, null);
        }
        private static void PrintException(Exception e, string path)
        {
            exceptions++;
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
            exceptions++;
            LogWriteLine("---------- Field Errors (path: ", path, "):");
            foreach (string fieldName in content.Fields.Keys)
            {
                var field = content.Fields[fieldName];
                if (field.IsValid) 
                    continue;

                LogWrite(field.Name);
                LogWrite(": ");
                LogWriteLine(field.GetValidationMessage());
            }
            LogWriteLine("------------------------");
        }
        
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
        private static void CreateLog(string logPath)
        {
            _logFilePath = logPath;
            FileStream fs = new FileStream(_logFilePath, FileMode.Create);
            StreamWriter wr = new StreamWriter(fs);
            wr.WriteLine("Start importing ", DateTime.Now, Environment.NewLine, "Log file: ", _logFilePath);
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
                Trace.Write(value);
                writer.Write(value);
            }
            if (newLine)
            {
                Trace.WriteLine(String.Empty);
                writer.WriteLine();
            }
        }
        private static void CloseLog(StreamWriter writer)
        {
            writer.Flush();
            writer.Close();
        }

        [DebuggerDisplay("ContentInfo: Name={Name}; ContentType={ContentTypeName}; IsFolder={IsFolder} ({Attachments.Count} Attachments)")]
        internal class ContentInfo
        {
            private string _metaDataPath;
            private int _contentId;
            private bool _isFolder;
            private string _name;
            private List<string> _attachments;
            private string _contentTypeName;
            private XmlDocument _xmlDoc;
            private string _childrenFolder;
            private ImportContext _transferringContext;

            public string MetaDataPath
            {
                get { return _metaDataPath; }
            }
            public int ContentId
            {
                get { return _contentId; }
            }
            public bool IsFolder
            {
                get { return _isFolder; }
            }
            public string Name
            {
                get { return _name; }
            }
            public List<string> Attachments
            {
                get { return _attachments; }
            }
            public string ContentTypeName
            {
                get { return _contentTypeName; }
            }
            public string ChildrenFolder
            {
                get { return _childrenFolder; }
            }
            public bool HasReference
            {
                get
                {
                    if (_transferringContext == null)
                        return false;
                    return _transferringContext.HasReference;
                }
            }
            public bool HasPermissions { get; private set; }
            public bool HasBreakPermissions { get; private set; }
            public bool ClearPermissions { get; private set; }
            public bool IsHidden { get; private set; }
            private static NameValueCollection FileExtensions
            {
                get { return System.Configuration.ConfigurationManager.GetSection("sensenet/uploadFileExtensions") as NameValueCollection; }
            }

            public ContentInfo(string path)
            {
                try
                {
                    _metaDataPath = path;
                    _attachments = new List<string>();

                    string directoryName = Path.GetDirectoryName(path);
                    _name = Path.GetFileName(path);
                    string extension = Path.GetExtension(_name);
                    if (extension.ToLower() == ".content")
                    {
                        var fileInfo = new FileInfo(path);
                        IsHidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

                        _xmlDoc = new XmlDocument();
                        _xmlDoc.Load(path);

                        XmlNode nameNode = _xmlDoc.SelectSingleNode("/ContentMetaData/ContentName");
                        _name = nameNode == null ? Path.GetFileNameWithoutExtension(_name) : nameNode.InnerText;

                        _contentTypeName = _xmlDoc.SelectSingleNode("/ContentMetaData/ContentType").InnerText;

                        ClearPermissions = _xmlDoc.SelectSingleNode("/ContentMetaData/Permissions/Clear") != null;
                        HasBreakPermissions = _xmlDoc.SelectSingleNode("/ContentMetaData/Permissions/Break") != null;
                        HasPermissions = _xmlDoc.SelectNodes("/ContentMetaData/Permissions/Identity").Count > 0;

                        // /ContentMetaData/Properties/*/@attachment
                        foreach (XmlAttribute attachmentAttr in _xmlDoc.SelectNodes("/ContentMetaData/Fields/*/@attachment"))
                        {
                            string attachment = attachmentAttr.Value;
                            _attachments.Add(attachment);
                            bool isFolder = Directory.Exists(Path.Combine(directoryName, attachment));
                            if (isFolder)
                            {
                                if (_isFolder)
                                    throw new ApplicationException(String.Concat("Two or more attachment folder is not enabled. ContentName: ", _name));
                                _isFolder = true;
                                _childrenFolder = Path.Combine(directoryName, attachment);
                            }
                        }
                        //-- default attachment
                        var defaultAttachmentPath = Path.Combine(directoryName, _name);
                        if (!_attachments.Contains(_name))
                        {
                            string[] paths;
                            if (Directory.Exists(defaultAttachmentPath))
                                paths = new string[] { defaultAttachmentPath };
                            else
                                paths = new string[0];

                            //string[] paths = Directory.GetDirectories(directoryName, _name);
                            if (paths.Length == 1)
                            {
                                if (_isFolder)
                                    throw new ApplicationException(String.Concat("Two or more attachment folder is not enabled. ContentName: ", _name));
                                _isFolder = true;
                                _childrenFolder = defaultAttachmentPath;
                                _attachments.Add(_name);
                            }
                            else
                            {
                                if (System.IO.File.Exists(defaultAttachmentPath))
                                    _attachments.Add(_name);
                            }
                        }
                    }
                    else
                    {
                        _isFolder = Directory.Exists(path);
                        if (_isFolder)
                        {
                            var dirInfo = new DirectoryInfo(path);
                            IsHidden = (dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

                            _contentTypeName = "Folder";
                            _childrenFolder = path;
                        }
                        else
                        {
                            var fileInfo = new FileInfo(path);
                            IsHidden = (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;

                            _xmlDoc = new XmlDocument();
                            _contentTypeName = GetContentTypeName(path) ?? "File";

                            var contentMetaData = String.Concat("<ContentMetaData><ContentType>{0}</ContentType><Fields><Binary attachment='", _name.Replace("'", "&apos;"), "' /></Fields></ContentMetaData>");
                            _xmlDoc.LoadXml(String.Format(contentMetaData, _contentTypeName));
                            _attachments.Add(_name);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Cannot create a ContentInfo. Path: " + path, e);
                }
            }

            public bool SetMetadata(SNC.Content content, string currentDirectory, bool isNewContent, bool needToValidate, bool updateReferences)
            {
                if (_xmlDoc == null)
                    return true;
                _transferringContext = new ImportContext(
                    _xmlDoc.SelectNodes("/ContentMetaData/Fields/*"), currentDirectory, isNewContent, needToValidate, updateReferences);
                bool result = content.ImportFieldData(_transferringContext);
                _contentId = content.ContentHandler.Id;
                return result;
            }

            internal bool UpdateReferences(SNC.Content content, bool needToValidate)
            {
                if (_transferringContext == null)
                    _transferringContext = new ImportContext(_xmlDoc.SelectNodes("/ContentMetaData/Fields/*"), null, false, needToValidate, true);
                else
                    _transferringContext.UpdateReferences = true;
                if (!content.ImportFieldData(_transferringContext))
                    return false;
                if (!HasPermissions && !HasBreakPermissions)
                    return true;
                var permissionsNode = _xmlDoc.SelectSingleNode("/ContentMetaData/Permissions");
                content.ContentHandler.Security.ImportPermissions(permissionsNode, this._metaDataPath);

                return true;
            }

            private static string GetContentTypeName(string fileName)
            {
                if (FileExtensions == null)
                    return null;

                int extStart = fileName.LastIndexOf('.');
                if (extStart != -1)
                {
                    var extension = fileName.Substring(extStart);

                    if (!string.IsNullOrEmpty(extension))
                    {
                        var fileType = FileExtensions[extension];
                        if (!string.IsNullOrEmpty(fileType))
                        {
                            return fileType;
                        }
                    }
                }

                return null;
            }

        }

    }
}
