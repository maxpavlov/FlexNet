using System;
using System.IO;
using System.Security;
using System.Xml;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Portal;
using SenseNet.Services.Instrumentation;
using SN = SenseNet.ContentRepository;
using SenseNet.Diagnostics;

namespace SenseNet.Services.WebDav
{
    public class Put : IHttpMethod
    {
        private WebDavHandler _handler;
        private const string FILEPROPERTYNAME = "Binary";

        public Put(WebDavHandler handler)
        {
            _handler = handler;
        }

        private void SetBinaryStream(Node node, Stream fileStream)
        {
            SetBinaryStream(node, fileStream, FILEPROPERTYNAME);
        }

        private void SetBinaryStream(Node node, Stream fileStream, string propertyName)
        {
            SetBinaryStream(node, fileStream, propertyName, null);
        }

        private void SetBinaryStream(Node node, Stream fileStream, string propertyName, string binaryName)
        {
            using (Stream inputStream = fileStream)
            {
                inputStream.Seek(0, SeekOrigin.Begin);

                ((BinaryData)node[propertyName]).SetStream(inputStream);

                if (!string.IsNullOrEmpty(binaryName))
                    ((BinaryData)node[propertyName]).FileName = binaryName;

                node.Save();
            }
        }

        /// <summary>
        /// Checks an existing node whether it has a binary property with 
        /// matching name and sets the binary value if needed
        /// </summary>
        /// <returns>True, if binary value was set</returns>
        private bool SetBinaryAttachment(Node existingNode, string binaryName)
        {
            bool foundBin = false;

            foreach (var propType in existingNode.PropertyTypes)
            {
                //TODO: BETTER FILENAME CHECK!
                if (propType.DataType == DataType.Binary)
                {
                    string tempBinaryName = WebDavHandler.GetAttachmentName(existingNode, propType.Name);

                    if (tempBinaryName.CompareTo(binaryName) == 0)
                    {
                        SetBinaryStream(existingNode, _handler.Context.Request.InputStream, propType.Name);
                        foundBin = true;
                        break;
                    }
                }
            }

            return foundBin;
        }

        /// <summary>
        /// Determine content type from file extension
        /// </summary>
        /// <returns>Content Type name</returns>
        private static string GetContentTypeName(string fileName)
        {
            // no fileextension data in webconfig
            if (Config.FileExtensions == null)
                return null;

            int extStart = fileName.LastIndexOf('.');
            if (extStart != -1)
            {
                var extension = fileName.Substring(extStart);

                if (!string.IsNullOrEmpty(extension))
                {
                    var fileType = Config.FileExtensions[extension];
                    if (!string.IsNullOrEmpty(fileType))
                    {
                        return fileType;
                    }
                }
            }

            return null;
        }

        //===========================================================================================================

        public void HandleMethod()
        {
            var parentPath = RepositoryPath.GetParentPath(_handler.GlobalPath);
            var fileName = RepositoryPath.GetFileName(_handler.GlobalPath);
            var parentNode = Node.LoadNode(parentPath);
            var node = Node.LoadNode(_handler.GlobalPath);

            switch (_handler.WebdavType)
            {
                case WebdavType.File:
                case WebdavType.Folder:
                case WebdavType.Page:
                    if (node == null || node is IFile)
                        HandleFile(parentPath, fileName, parentNode, node);
                    return;
                case WebdavType.Content:
                    HandleContent(parentPath, fileName, parentNode, node);
                    return;
                case WebdavType.ContentType:
                    InstallContentType();
                    return;
                default:
                    throw new NotImplementedException("Unknown WebdavType" + _handler.WebdavType);
            }
        }

        private void HandleFile(string parentPath, string fileName, Node parentNode, Node node)
        {
            if (_handler.Context.Request.InputStream.Length == 0)
            {
                _handler.Context.Response.StatusCode = 200;
                _handler.Context.Response.ContentType = "text/html";
                _handler.Context.Response.AddHeader("Content-Length", "0");
                _handler.Context.Response.Flush();
                return;
            }

            var file = node as IFile;
            if (file == null)
                CreateFile(parentPath, fileName, parentNode);
            else
                UpdateFile(node);
        }
        private void CreateFile(string parentPath, string fileName, Node parentNode)
        {
            string realFileName = fileName;

            _handler.Context.Response.StatusCode = 201; // created

            if ((parentNode == null) || !(parentNode is IFolder)) //TODO: Webdav: Force content type restrictions during content creation
            {
                _handler.Context.Response.StatusCode = 403;
                _handler.Context.Response.Flush();
                return;
            }
            try
            {
                #region debug

                string traceMessage = string.Concat("    parentpath: ", parentPath, Environment.NewLine);
                traceMessage = string.Concat(traceMessage, "    filename: ", fileName, Environment.NewLine);
                traceMessage = string.Concat(traceMessage, "    parentnode: ", parentNode.Id.ToString(), "-", parentNode.Name, Environment.NewLine);

                System.Diagnostics.Trace.Write(traceMessage);

                #endregion

                //search for contents referring to this binary
                foreach (Node existingNode in ((IFolder)parentNode).Children)
                {
                    if (SetBinaryAttachment(existingNode, realFileName))
                        return;
                }

                // special filetype, referred in web.config
                string contentType = GetContentTypeName(realFileName);
                if (contentType != null)
                {
                    traceMessage = string.Concat("    NEW ", contentType, Environment.NewLine);
                    System.Diagnostics.Trace.Write(traceMessage);

                    var nodeType = ActiveSchema.NodeTypes[contentType];
                    var specialFile = nodeType.CreateInstance(parentNode) as IFile;

                    if (specialFile == null)
                    {
                        _handler.Context.Response.StatusCode = 405;
                    }
                    else
                    {
                        specialFile.Binary = new BinaryData();

                        //TODO: find a way to generalize this...
                        if (specialFile is Page)
                        {
                            if (fileName.ToLower().EndsWith(".aspx"))
                                fileName = fileName.Remove(fileName.LastIndexOf('.'));

                            ((Page)specialFile).Binary.FileName = realFileName;
                            ((Page)specialFile).PersonalizationSettings = new BinaryData { FileName = string.Concat(fileName, ".PersonalizationSettings") };
                            ((Page)specialFile).PageTemplateNode = Config.DefaultPageTemplate;
                        }

                        ((Node)specialFile).Name = fileName;

                        SetBinaryStream(specialFile as Node, _handler.Context.Request.InputStream, FILEPROPERTYNAME, realFileName);
                    }

                    return;
                }

                //---- general file
                //var file = new SN.File(parentNode) { Binary = new BinaryData() };
                //file.Name = fileName;
                //SetBinaryStream(file as Node, _handler.Context.Request.InputStream, FILEPROPERTYNAME, realFileName);
                var fileContent = Content.CreateNew(typeof(SN.File).Name, parentNode, fileName);
                var fileNode = (SN.File)fileContent.ContentHandler;
                fileNode.Binary = new BinaryData();
                SetBinaryStream(fileNode, _handler.Context.Request.InputStream, FILEPROPERTYNAME, realFileName);
            }
            catch (SecurityException e) //logged
            {
                Logger.WriteException(e);
                _handler.Context.Response.StatusCode = 403;
            }
            catch (SenseNetSecurityException ee) //logged
            {
                Logger.WriteException(ee);
                _handler.Context.Response.StatusCode = 403;
            }

            return;
        }
        private void UpdateFile(Node file)
        {
            _handler.Context.Response.StatusCode = 200; // created

            try
            {
                // existing file
                SetBinaryStream(file, _handler.Context.Request.InputStream);
            }
            catch (SenseNetSecurityException e) //logged
            {
                Logger.WriteException(e);
                _handler.Context.Response.StatusCode = 403;
            }
            catch (Exception ee) //logged
            {
                //in some cases content validation fails for the first 
                //webdav request with "empty" stream - swallow this exception
                Logger.WriteException(ee);
                _handler.Context.Response.StatusCode = 200;
            }
        }

        private void HandleContent(string parentPath, string fileName, Node parentNode, Node node)
        {
            Content content = null;
            var contentXml = new XmlDocument();
            string contentString = Tools.GetStreamString(_handler.Context.Request.InputStream);

            if (!string.IsNullOrEmpty(contentString))
            {
                contentXml.LoadXml(contentString);

                //new content or previously uploded binary exists!
                if (node == null || node is IFile)
                {
                    string ctName = contentXml.SelectSingleNode("/ContentMetaData/ContentType").InnerText;

                    if (node == null || ctName.CompareTo(node.NodeType.Name) != 0)
                    {
                        var nodeType = ActiveSchema.NodeTypes[ctName];
                        node = nodeType.CreateInstance(parentNode);
                        node.Name = fileName;
                    }
                }

                content = Content.Create(node);

                XmlNodeList allFields = contentXml.SelectNodes("/ContentMetaData/Fields/*");
                XmlNodeList binaryFields = contentXml.SelectNodes("/ContentMetaData/Fields/*[@attachment]");

                //set attachments first
                foreach (XmlNode field in binaryFields)
                {
                    string attachmentName = field.Attributes["attachment"].Value;
                    var attachment = Node.LoadNode(RepositoryPath.Combine(parentPath, attachmentName)) as SN.File;

                    //previously uploaded attachment found
                    if (attachment != null && attachment.Id != node.Id)
                    {
                        attachment.Name = Guid.NewGuid().ToString();
                        attachment.Save();

                        SetBinaryStream(content.ContentHandler, attachment.Binary.GetStream(), field.Name, attachmentName);
                        attachment.Delete();
                    }
                }

                var transferringContext = new ImportContext(allFields, "", node.Id == 0, true, false);

                //import flat properties
                content.ImportFieldData(transferringContext);

                //update references
                transferringContext.UpdateReferences = true;
                content.ImportFieldData(transferringContext);
            }

            _handler.Context.Response.StatusCode = 200;
        }

        private void InstallContentType()
        {
            string contentString = Tools.GetStreamString(_handler.Context.Request.InputStream);
            if (!string.IsNullOrEmpty(contentString))
                ContentTypeInstaller.InstallContentType(contentString);
            _handler.Context.Response.StatusCode = 200;
        }

    }
}
