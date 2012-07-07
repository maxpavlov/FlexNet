using System;
using System.Web;
using SenseNet.Services.Instrumentation;
using SenseNet.Portal.Virtualization;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal.Dws;

namespace SenseNet.Services.WebDav
{
    [TraceSourceName("WebDAV")]
    public class WebDavHandler : System.Web.SessionState.IRequiresSessionState, IHttpHandler
	{
        private WebdavType _wdType = WebdavType.File;

		public string Protocol
		{
			get
			{
			    if(Context.Request.ServerVariables["HTTPS"].ToLower() == "off")
					return "http://";
			    
                return "https://";
			}
		}

		public string Host
		{
			get
			{
				return Context.Request.ServerVariables["HTTP_HOST"];
			}
		}

        public string Path { get; private set; }

        public WebdavType WebdavType
        {
            get { return _wdType; }
            private set { _wdType = value; }
        }

        public string GetGlobalPath(string path)
        {
            if (path.StartsWith("/Root"))
                return path;

            if (path.StartsWith("Root"))
                return string.Concat("/", path);

            return RepositoryPath.Combine("/Root", path.TrimEnd('/')).TrimEnd('/');
        }

        public string GlobalPath
        { 
            get
            {
                return GetGlobalPath(Path);
            }
        }
		
		public HttpContext Context
		{
			get
			{
				return HttpContext.Current;
			}
		}

		public string RepositoryPathToUrl(string repositoryPath)
		{
            var repoPath = repositoryPath;
            if (repoPath.StartsWith("/Root/"))
                repoPath = repoPath.Substring(6);
            var domain = Protocol + Host;

            var url = string.IsNullOrEmpty(repoPath) ? 
                domain : 
                RepositoryPath.Combine(domain, repoPath);

            return url;
		}

        #region IHttpHandler Members

		public void ProcessRequest(HttpContext context)
		{

            #region Debug

            string traceMessage = string.Concat("METHOD: ", context.Request.HttpMethod, " Path: '", context.Request.Path, "'", Environment.NewLine);
            traceMessage = string.Concat(traceMessage, "   Authenticated: ", HttpContext.Current.User.Identity.IsAuthenticated.ToString(), ", ", "UserName: ", HttpContext.Current.User.Identity.Name, Environment.NewLine);
            traceMessage = string.Concat(traceMessage, "   HEADERS: ", Environment.NewLine);

            foreach (var x in context.Request.Headers.AllKeys)
            {
                traceMessage = string.Concat(traceMessage, string.Format("      {0}={1}", x, context.Request.Headers[x]));
                traceMessage = string.Concat(traceMessage, Environment.NewLine);
            }

            System.Diagnostics.Debug.Write(traceMessage);
            
            #endregion

            context.Response.TrySkipIisCustomErrors = true;
            context.Response.Headers.Add("MicrosoftSharePointTeamServices", "14.0.0.5128");

            // check authentication
            if (DwsHelper.CheckVisitor())
                return;

            Path = context.Request.Path;

            if (Path.Contains(PortalContext.InRepositoryPageSuffix))
            {
                Path = "";
            }

            if (Path.ToLower().EndsWith(".content"))
            {
                Path = Path.Remove(Path.LastIndexOf('.'));
                WebdavType = WebdavType.Content;
            }
            else if (Path.ToLower().EndsWith(".aspx"))
            {
                WebdavType = WebdavType.Page;
            }
            else if (Path.ToLower().EndsWith("ctd.xml"))
            {
                WebdavType = WebdavType.ContentType;
            }

            //LATER: do not handle specific types in current version
		    switch (WebdavType)
		    {
                case WebdavType.File:
                case WebdavType.Folder:
		            break;
                default:
                    context.Response.StatusCode = 200;
                    context.Response.Flush();
                    context.Response.End();
		            return;
		    }

            Path = Path.Replace("//", "/");
            Path = Path.TrimStart('/');

			//switch by method type - see RFC 2518
			try
			{ 
				switch(context.Request.HttpMethod)
				{
                    case "OPTIONS":
                        {
                            var o = new Options(this);
                            o.HandleMethod();
                            break;
                        }

                    case "PROPFIND":
                        {
                            var pf = new Propfind(this);
                            pf.HandleMethod();
                            break;
                        }
                    case "GET":
                        {
                            var g = new Get(this);
                            g.HandleMethod();
                            break;
                        }
                    case "HEAD":
                        {
                            var h = new Head(this);
                            h.HandleMethod();
                            break;
                        }
                    case "PUT":
                        {
                            var p = new Put(this);
                            p.HandleMethod();
                            break;
                        }
                    case "PROPPATCH":
                        {
                            var pp = new Proppatch(this);
                            pp.HandleMethod();
                            break;
                        }
                    case "MKCOL":
                        {
                            var md = new MkCol(this);
                            md.HandleMethod();
                            break;
                        }
                    case "MOVE":
                        {
                            var m = new Move(this);
                            m.HandleMethod();
                            break;
                        }
                    case "DELETE":
                        {
                            var d = new Delete(this);
                            d.HandleMethod();
                            break;
                        }
                    case "TRACE":
                        {
                            var t = new Trace(this);
                            t.HandleMethod();
                            break;
                        }
                    case "LOCK":
                        {
                            var l = new Lock(this);
                            l.HandleMethod();
                            break;
                        }
                    case "UNLOCK":
                        {
                            var ul = new UnLock(this);
                            ul.HandleMethod();
                            break;
                        }
                    case "POST":
                        {
                            Context.Response.StatusCode = 404;
                            context.Response.Flush();
                            break;
                        }
					default:
					{
						context.Response.StatusCode = 501; // not implemented
						break;
					}
				}

				context.Response.Flush();
				context.Response.End();
			}
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
			catch (Exception ex)
			{
                //traceMessage = string.Concat(ex.Message, Environment.NewLine);
                //traceMessage = string.Concat(traceMessage, ex.StackTrace, Environment.NewLine);
                //while (ex.InnerException != null)
                //{
                //    traceMessage = string.Concat(traceMessage, "    innerexception: ", ex.InnerException.Message, Environment.NewLine);
                //    ex = ex.InnerException;
                //}
                //System.Diagnostics.Trace.Write(traceMessage);

				try 
				{
					Context.Response.StatusCode = 404;
					context.Response.Flush();
                }
				catch
				{
                    //last catch, can be suppressed
				}
			}
		}

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}

		#endregion

        internal static string GetAttachmentName(Node node, string propertyName)
        {
            var binaryName = RepositoryPath.GetFileName(System.IO.Path.GetFileName(((BinaryData)node[propertyName]).FileName));

            if (string.IsNullOrEmpty(binaryName) || binaryName.StartsWith("."))
                binaryName = node.Name + binaryName;

            return binaryName;
        }

        internal static Node GetNodeByBinaryName(Node parentNode, string fileName, out string binaryPropertyName)
        {
            Node foundNode = null;
            binaryPropertyName = string.Empty;

            if (parentNode == null || !(parentNode is IFolder))
                return null;

            //binary check: if the requested file is the binary of a content
            //with a different name (e.g PersonalizationSettings)
            foreach (var child in ((IFolder)parentNode).Children)
            {
                foreach (var propType in child.PropertyTypes)
                {
                    //TODO: BETTER FILENAME CHECK!
                    if (propType.DataType != DataType.Binary) 
                        continue;

                    var binaryName = GetAttachmentName(child, propType.Name);
                    if (binaryName.CompareTo(fileName) != 0) 
                        continue;

                    binaryPropertyName = propType.Name;
                    foundNode = child;
                    break;
                }

                if (foundNode != null)
                    break;
            }

            return foundNode;
        }
	}
}
