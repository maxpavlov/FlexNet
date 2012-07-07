using System;
using System.Xml;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal;
using SenseNet.Services.Instrumentation;

namespace SenseNet.Services.WebDav
{
    [TraceSourceName("WebDAV - Propfind")]
    public class Propfind : IHttpMethod
	{
		private WebDavHandler _handler;
		private XmlTextWriter _writer;
		public Propfind(WebDavHandler handler)
		{
			_handler = handler;
		}

        private Depth RequestDepth { get; set; }

		#region IHttpMethod Members

		public void HandleMethod()
		{
			_handler.Context.Response.StatusCode = 207;
			_handler.Context.Response.ContentType = "text/xml";
			_handler.Context.Response.CacheControl = "no-cache";
			_handler.Context.Response.AddHeader("Content-Location", _handler.RepositoryPathToUrl(_handler.Path));

			RequestDepth = Common.GetDepth(_handler.Context.Request.Headers["Depth"]);

            switch (RequestDepth)
			{
				case Depth.Current:
				{
					ProcessCurrent();
					break;
				}
				case Depth.Children:
				{
					ProcessChildren();
					break;
				}
				case Depth.Infinity:
				{
					ProcessChildren();
					break;
					//throw (new NotImplementedException("Infinity request is not implemented."));
				}
			}
			if(_writer == null)
				return;
			_writer.Flush();
			_writer.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

            var reader = new System.IO.StreamReader(_writer.BaseStream, System.Text.Encoding.UTF8);
            string ret = reader.ReadToEnd();

			_writer.Close();

            #region Debug

            System.Diagnostics.Debug.Write(string.Concat("RESPONSE: ", ret));

            #endregion

			_handler.Context.Response.Write(ret);
		}

		#endregion

		internal void ProcessChildren()
		{
            #region Debug

            //System.Diagnostics.Trace.Write(string.Concat("** PROCESSChildren '", _handler.Path, "'"));

            #endregion
            
            Node node = null;

			if(_handler.Path != string.Empty)
			{  
                node = Node.LoadNode(_handler.GlobalPath);

                if (node == null) 
				{
					_handler.Context.Response.StatusCode = 404;	// not found
					return;
				}
			}

			_writer = Common.GetXmlWriter();
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "multistatus", XmlNS.DAV);
	
			if(_handler.Path != string.Empty)
			{
                WriteSingleItem(node, WebdavType.Folder);
                var folder = node as IFolder;

                //LATER: SmartFolder handling. The problem is: in WebDav environment
                //there is no CurrentWorkspace! This means that SmartFolder queries
                //are geting corrupted.
                if (folder != null && !(folder is SmartFolder))
				{
                    foreach (var childNode in folder.Children)
                    {
                        if (!childNode.NodeType.IsInstaceOfOrDerivedFrom("SystemFolder") && 
                            (childNode is IFile || childNode is IFolder))
                            WriteItem(childNode, Depth.Current, true);
                    }

                    //LATER: CTD handling
                    //include contentType under itself
                    //if (node is ContentType)
                    //{
                    //    WriteSingleItem(node, WebdavType.ContentType);
                    //}                    
				}
			}
			else
			{
				WriteRoot();

                foreach (var childNode in Repository.Root.Children) 
				{
                    WriteItem(childNode);
                }
			}

			_writer.WriteEndElement(); 
        }

		internal void WriteRoot()
		{

			_writer.WriteStartElement(XmlNS.DAV_Prefix, "response", XmlNS.DAV);
			//href
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "href", XmlNS.DAV);
			_writer.WriteString(_handler.RepositoryPathToUrl(_handler.Path));
			_writer.WriteEndElement();

			//propstat
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "propstat", XmlNS.DAV);
			// propstat/status
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "status", XmlNS.DAV);
			_writer.WriteString("HTTP/1.1 200 OK");
			_writer.WriteEndElement(); // propstat/status
			
			// propstat/prop
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "prop", XmlNS.DAV);
			
			// propstat/prop/getcontentlenght
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getcontentlength", XmlNS.DAV);
			_writer.WriteString("0");
			_writer.WriteEndElement(); // propstat/prop/getcontentlenght

			// propstat/prop/creationdate
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "creationdate", XmlNS.DAV);
			_writer.WriteString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sZ"));
			_writer.WriteEndElement(); // propstat/prop/creationdate

			// propstat/prop/displayname
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "displayname", XmlNS.DAV);
			_writer.WriteString(string.Empty);
			_writer.WriteEndElement(); // propstat/prop/displayname
			
			// propstat/prop/getetag
            var eTag = Guid.NewGuid().ToString();
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getetag", XmlNS.DAV);
			_writer.WriteString(eTag);
			_writer.WriteEndElement(); // propstat/prop/getetag

			// propstat/prop/getlastmodified
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getlastmodified", XmlNS.DAV);
			_writer.WriteString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sZ"));
			_writer.WriteEndElement(); // propstat/prop/getlastmodified

			// propstat/prop/resourcetype
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "resourcetype", XmlNS.DAV);
			
			// propstat/prop/resourcetype/collection
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "collection", XmlNS.DAV);
			_writer.WriteEndElement(); // propstat/prop/resourcetype/collection

			//			// propstat/prop/resourcetype/Repl
            //_writer.WriteStartElement(XmlNS.MSRepl_Prefix, "Repl", XmlNS.MSRepl);
            //_writer.WriteString("t");
            //_writer.WriteEndElement(); // propstat/prop/resourcetype/Repl
			
			_writer.WriteEndElement(); // propstat/prop/resourcetype

            _writer.WriteStartElement(XmlNS.MSRepl_Prefix, "authoritative-directory", XmlNS.MSRepl);
			_writer.WriteString("t");
			_writer.WriteEndElement(); // propstat/prop/resourcetype/Repl

            _writer.WriteStartElement(XmlNS.MSRepl_Prefix, "repl-uid", XmlNS.MSRepl);
            _writer.WriteString("rid:{");
            _writer.WriteString(eTag);
            _writer.WriteString("}");
            _writer.WriteEndElement();

            _writer.WriteStartElement(XmlNS.MSRepl_Prefix, "resourcetag", XmlNS.MSRepl);
            _writer.WriteString("rt:");
            _writer.WriteString(eTag);
            _writer.WriteString("@00000000000");
            _writer.WriteEndElement();

			// propstat/prop/isFolder
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "isfolder", XmlNS.DAV);
			_writer.WriteString("t");
			_writer.WriteEndElement(); // propstat/prop/isFolder
			
			// propstat/prop/iscollection
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "iscollection", XmlNS.DAV);
			_writer.WriteString("1");
			_writer.WriteEndElement(); // propstat/prop/iscollection
			
			// propstat/prop/getcontenttype
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getcontenttype", XmlNS.DAV);
			_writer.WriteString("application/octet-stream");
			_writer.WriteEndElement(); // propstat/prop/getcontenttype

			_writer.WriteEndElement(); // propstat/prop
			_writer.WriteEndElement(); // propstat
			
			_writer.WriteEndElement();// response
		}

		internal void ProcessRoot()
		{
            _writer = Common.GetXmlWriter();
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "multistatus", XmlNS.DAV);
			WriteRoot();
			_writer.WriteEndElement(); // multistatus
		}

		internal void ProcessCurrent()
		{
			if(_handler.Path == string.Empty)
			{
				ProcessRoot();
				return;
			}

            var node = Node.LoadNode(_handler.GlobalPath);

            if(node == null)
			{
                var parentPath = RepositoryPath.GetParentPath(_handler.GlobalPath);
                var currentName = RepositoryPath.GetFileName(_handler.GlobalPath);

                node = Node.LoadNode(parentPath);

                var binaryPropertyName = string.Empty;
                var foundNode = WebDavHandler.GetNodeByBinaryName(node, currentName, out binaryPropertyName);

                if (foundNode != null)
                {
                    node = foundNode;
                }
                else
                {
                    //content type check
                    //LATER: CTD handling
                    //var parentIsContentType = (node != null && node is ContentType);

                    //if (!parentIsContentType)
                    //{

                    // desktop.ini, thumbs.db, and other files that are not present are mocked instead of returning 404
                    if ((Config.MockExistingFiles != null) && (Config.MockExistingFiles.Contains(currentName)))
                    {
                        _writer = Common.GetXmlWriter();
                        _writer.WriteStartElement(XmlNS.DAV_Prefix, "multistatus", XmlNS.DAV);
                        _writer.WriteEndElement(); // multistatus

                        _handler.Context.Response.Flush();
                        return;
                    }
                    _handler.Context.Response.StatusCode = 404;
                    _handler.Context.Response.Flush();
                    return;
                    //}

                    // parent is contenttype, continue operation on parent (foldernode's name is valid CTD name)
                }
			}

			_writer = Common.GetXmlWriter();
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "multistatus", XmlNS.DAV);

            WriteItem(node);

            _writer.WriteEndElement(); // multistatus
		}

        internal void WriteItem(Node node)
        {
            WriteItem(node, Depth.Current);
        }

        internal void WriteItem(Node node, Depth itemDepth)
        {
            WriteItem(node, itemDepth, false);
        }

        internal void WriteItem(Node node, Depth itemDepth, bool child)
        {
            var done = false;

            //.Content files are not written to the client currently

            switch (_handler.WebdavType)
            {
                case WebdavType.Content: /*WriteSingleItem(node, WebdavType.Content);*/ done = true; break;
                case WebdavType.ContentType: WriteSingleItem(node, WebdavType.ContentType); done = true; break;
                case WebdavType.Page: WriteSingleItem(node, WebdavType.File); done = true; break;
                case WebdavType.File:
                    if (node is IFolder)
                    {
                        WriteSingleItem(node, WebdavType.Folder);
                        done = true;
                    }

                    if (child && (node is Page || (!(node is IFile) && (node.NodeType.Name != "Folder"))))
                    {
                        //WriteSingleItem(node, WebdavType.Content);
                        done = true;
                    }
                    break;
            }

            //write items for all the binary properties (e.g: .aspx, .xml, .PersonalizationSettings)
            if (child && itemDepth == Depth.Current &&
                (_handler.WebdavType == WebdavType.File || _handler.WebdavType == WebdavType.Folder))
            {
                foreach (var propType in node.PropertyTypes)
                {
                    if (propType.DataType != DataType.Binary || propType.Name.CompareTo("Binary") != 0) 
                        continue;

                    WriteSingleItem(node, WebdavType.File, propType.Name);
                    done = true;
                }
            }

            //hack
            if (!done && node is IFile)
                WriteSingleItem(node, WebdavType.File);
        }

        internal void WriteSingleItem(Node node, WebdavType wdType)
        {
            WriteSingleItem(node, wdType, "Binary");
        }

        internal void WriteSingleItem(Node node, WebdavType wdType, string binaryPropertyName)
		{
            // set nodeName extensions
            var nodeName = node.Name;
            var nodePath = node.Path;

            if (wdType == WebdavType.Content)
            {
                nodeName = nodeName + ".Content";
                nodePath = nodePath + ".Content";
            }
            else if (wdType == WebdavType.ContentType)
            {
                nodeName = nodeName + "Ctd.xml";
                nodePath = nodePath + "Ctd.xml";
            }
            else if (wdType != WebdavType.Folder)
            {
                if (binaryPropertyName != "Binary")
                    nodeName = string.Concat(node.Name, ".", binaryPropertyName);

                nodePath = RepositoryPath.Combine(node.ParentPath, nodeName);                   
            }

            // response
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "response", XmlNS.DAV);
			//href
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "href", XmlNS.DAV);
            _writer.WriteString(_handler.RepositoryPathToUrl(nodePath));
			_writer.WriteEndElement();

			//propstat
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "propstat", XmlNS.DAV);
			// propstat/status
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "status", XmlNS.DAV);

			_writer.WriteString("HTTP/1.1 200 OK");
			_writer.WriteEndElement(); // propstat/status
			
			// propstat/prop
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "prop", XmlNS.DAV);
			
			// propstat/prop/getcontentlenght
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getcontentlength", XmlNS.DAV);

            if (wdType == WebdavType.File)
            {
                var f = node as IFile;
                if (f != null)
                {
                    _writer.WriteString(((BinaryData)node[binaryPropertyName]).Size.ToString());
                }
                else
                    _writer.WriteString("0");
            }
            else
                _writer.WriteString("0");

			_writer.WriteEndElement(); // propstat/prop/getcontentlenght

            // lockdiscovery
            if (node.Locked)
                Lock.WriteLockDiscovery(_writer, node, node.LockTimeout);

            // supportedlock
            _writer.WriteStartElement(XmlNS.DAV_Prefix, "supportedlock", XmlNS.DAV);
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "lockentry", XmlNS.DAV);
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "lockscope", XmlNS.DAV);
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "exclusive", XmlNS.DAV);
            _writer.WriteEndElement(); // exclusive
            _writer.WriteEndElement(); // lockscope
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "locktype", XmlNS.DAV);
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "write", XmlNS.DAV);
            _writer.WriteEndElement(); // write
            _writer.WriteEndElement(); // locktype
            _writer.WriteEndElement(); // lockentry
            _writer.WriteEndElement(); // supportedlock

			// propstat/prop/creationdate
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "creationdate", XmlNS.DAV);
            _writer.WriteString(node.CreationDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sZ"));
			_writer.WriteEndElement(); // propstat/prop/creationdate

			// propstat/prop/displayname
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "displayname", XmlNS.DAV);
			//_writer.WriteString(entry.Name);

            //_writer.WriteString(node.Name);
            _writer.WriteString(nodeName);
			_writer.WriteEndElement(); // propstat/prop/displayname

            var eTag = Guid.NewGuid().ToString();
			// propstat/prop/getetag
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getetag", XmlNS.DAV);
			_writer.WriteString(eTag);
			_writer.WriteEndElement(); // propstat/prop/getetag

			// propstat/prop/getlastmodified
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getlastmodified", XmlNS.DAV);
            _writer.WriteString(node.ModificationDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sZ"));
			_writer.WriteEndElement(); // propstat/prop/getlastmodified

			// propstat/prop/resourcetype
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "resourcetype", XmlNS.DAV);
            if (wdType == WebdavType.Folder)
			{
				// propstat/prop/resourcetype/collection
				_writer.WriteStartElement(XmlNS.DAV_Prefix, "collection", XmlNS.DAV);
				_writer.WriteEndElement(); // propstat/prop/resourcetype/collection

				// propstat/prop/resourcetype/Repl
                //_writer.WriteStartElement(XmlNS.MSRepl_Prefix, "Repl", XmlNS.MSRepl);
                //_writer.WriteString("t");
                //_writer.WriteEndElement(); // propstat/prop/resourcetype/Repl
			}
			_writer.WriteEndElement(); // propstat/prop/resourcetype

			_writer.WriteStartElement(XmlNS.MSRepl_Prefix, "repl-uid", XmlNS.MSRepl);
			_writer.WriteString("rid:{");
			_writer.WriteString(eTag);
			_writer.WriteString("}");
			_writer.WriteEndElement();

			_writer.WriteStartElement(XmlNS.MSRepl_Prefix, "resourcetag", XmlNS.MSRepl);
			_writer.WriteString("rt:");
			_writer.WriteString(eTag);
			_writer.WriteString("@00000000000");
			_writer.WriteEndElement();

			 // propstat/prop/isFolder
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "isfolder", XmlNS.DAV);
            if (wdType == WebdavType.Folder)
				_writer.WriteString("t");
			else
				_writer.WriteString("0");
			_writer.WriteEndElement(); // propstat/prop/isFolder
			
			// propstat/prop/iscollection
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "iscollection", XmlNS.DAV);
			if(wdType == WebdavType.Folder)
				_writer.WriteString("1");
			else
				_writer.WriteString("0");
			_writer.WriteEndElement(); // propstat/prop/iscollection
			
			// propstat/prop/getcontenttype
			_writer.WriteStartElement(XmlNS.DAV_Prefix, "getcontenttype", XmlNS.DAV);
			if(wdType == WebdavType.Folder)
				_writer.WriteString("application/octet-stream");
			else
                _writer.WriteString("text/xml");
			_writer.WriteEndElement(); // propstat/prop/getcontenttype
			_writer.WriteEndElement(); // propstat/prop
			_writer.WriteEndElement(); // propstat
			_writer.WriteEndElement();// response
		}
	}
}
