using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;
using System.Xml;
using System.Xml.Xsl;
using SenseNet.ContentRepository.Storage.Caching.Dependency;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository;
using System.Web;
using SenseNet.Portal.Virtualization;
using System.Web.Hosting;
using System.Xml.XPath;
using System.Web.UI;
using System.Diagnostics;

namespace SenseNet.Portal.UI.PortletFramework
{
    public class XsltCreatableAttribute : Attribute
    {
    }

    [XsltCreatable]
    public class XsltUtil
    {
        public string HW() { return "HW"; }
    }

    public static class Xslt
    {
        public class RepositoryPathResolver : XmlResolver
        {
            public List<string> DependencyPathCollection = new List<string>();
            public List<string> ImportNamespaceCollection = new List<string>();
            public List<string> ImportCssCollection = new List<string>();
            public List<string> ImportScriptCollection = new List<string>();
            public override System.Net.ICredentials Credentials
            {
                set { }
            }

            private XmlNamespaceManager GetNamespaceManager()
            {
                NameTable nt = new NameTable();
                XmlNamespaceManager mgr = new XmlNamespaceManager(nt);
                mgr.AddNamespace("sn", "http://www.sensenet.com/2010");
                return mgr;
            }
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                

                //TODO: unix like filesystem support
                string nodePath = absoluteUri.AbsolutePath.Remove(0, (absoluteUri.AbsolutePath.IndexOf(":") + 1));
                DependencyPathCollection.Add(nodePath);

                System.IO.Stream xsltStream = null;

                // 20120222: dobsonl: we may use sensenet dlls from a webapp, and yet use no virtualpathprovider
                //if (HttpContext.Current != null)
                if (RepositoryPathProvider.DiskFSSupportMode == DiskFSSupportMode.Prefer)
                {
                    xsltStream = VirtualPathProvider.OpenFile(nodePath);
                }
                else
                {
                    var xsltFile = Node.Load<File>(nodePath);
                    xsltStream = xsltFile.Binary.GetStream();
                }
                XmlDocument doc = new XmlDocument();
                doc.Load(xsltStream);
                var result = doc.DocumentElement.Attributes.Cast<XmlAttribute>()
                            .Where(attrib => attrib.InnerText.StartsWith("sn://"))
                            .Select(attrib => attrib.InnerText)
                            .ToArray();
                ImportNamespaceCollection.AddRange(result);

                var scripts = doc.SelectNodes("//sn:scriptrequest", GetNamespaceManager());
                
                foreach (var scripelm in scripts.Cast<XmlElement>())
                {
                    this.ImportScriptCollection.Add(scripelm.GetAttribute("path"));
                }

                var csss = doc.SelectNodes("//sn:cssrequest", GetNamespaceManager());
                foreach (var csselm in csss.Cast<XmlElement>())
                {
                    this.ImportCssCollection.Add(csselm.GetAttribute("path"));
                }
                

                xsltStream.Position = 0;
                return xsltStream;
            }
        }

        public class XslTransformExecutionContext
        {
            public List<string> ImportCssCollection = new List<string>();
            public List<string> ImportScriptCollection = new List<string>();

            public XslCompiledTransform XslCompiledTransform { get; set; }
            public string[] NamespaceExtensions { get; set; }

            public void Transform(IXPathNavigable input, XsltArgumentList arguments, XmlWriter writer)
            {
                if (arguments == null && NamespaceExtensions.Length > 0)
                {
                    arguments = new XsltArgumentList();
                }

                foreach (var namespaceExtension in NamespaceExtensions)
                {
                    var typename = namespaceExtension.Substring(5);
                    var o = arguments.GetExtensionObject(namespaceExtension);
                    if (o == null)
                    {
                        var instance = CreateInstance(typename);
                        arguments.AddExtensionObject(namespaceExtension, instance);
                    }
                }

                this.XslCompiledTransform.Transform(input, arguments, writer);
            }

            public void Transform(IXPathNavigable input, XsltArgumentList arguments, HtmlTextWriter writer)
            {
                if (arguments == null && NamespaceExtensions.Length> 0)
                {
                    arguments = new XsltArgumentList();                    
                }

                foreach (var namespaceExtension in NamespaceExtensions)
                {
                    var typename = namespaceExtension.Substring(5);
                    var o = arguments.GetExtensionObject(namespaceExtension);
                    if (o == null)
                    {
                        var instance = CreateInstance(typename);
                        arguments.AddExtensionObject(namespaceExtension, instance);
                    }
                }

                this.XslCompiledTransform.Transform(input, arguments, writer);
            }

            public object CreateInstance(string typename)
            {
                return TypeHandler.CreateInstance(typename);
            }
            //public XsltArgumentList XsltArgumentList { get; set; }
        }

        public static XslTransformExecutionContext GetXslt(string nodePath, bool resolveScripts)
        {
            string nodeKey = "xslt:" + nodePath;

            XslTransformExecutionContext context = (XslTransformExecutionContext)(DistributedApplication.Cache.Get(nodeKey));
            if (context == null)
            {
                context = new XslTransformExecutionContext();
                
                context.XslCompiledTransform = new XslCompiledTransform(Debugger.IsAttached);
                RepositoryPathResolver resolver = new RepositoryPathResolver();

                try
                {
                    context.XslCompiledTransform.Load(nodePath, XsltSettings.Default, resolver);
                }
                catch (NullReferenceException e) // rethrow
                {
                    throw new NullReferenceException(e.Message + "<br/>" + nodePath + " (or include)");
                }

                AggregateCacheDependency aggregatedDependency = new AggregateCacheDependency();
                
                context.NamespaceExtensions = resolver.ImportNamespaceCollection.Distinct().ToArray();
                context.ImportScriptCollection = resolver.ImportScriptCollection;
                context.ImportCssCollection = resolver.ImportCssCollection;

                foreach (var dependencyPath in resolver.DependencyPathCollection)
                {
                    // Create an aggregate cache dependeny that includes NodeId, Path, NodeTypeId
                    // Our cache item will be invalidated if the dependency node is invalidated
                    //  - by node id
                    //  - by path
                    //  - by nodeType
                    string fsFilePath = null;
                    if (HttpContext.Current != null &&
                        RepositoryPathProvider.DiskFSSupportMode == DiskFSSupportMode.Prefer)
                        fsFilePath = HttpContext.Current.Server.MapPath(dependencyPath);
                    if (!string.IsNullOrEmpty(fsFilePath) && System.IO.File.Exists(fsFilePath))
                    {
                        aggregatedDependency.Add(new CacheDependency(fsFilePath));                        
                    }
                    else 
                    {
                        var nodeHead = NodeHead.Get(dependencyPath);
                        aggregatedDependency.Add(
                            new PathDependency(nodeHead.Path),
                            new NodeIdDependency(nodeHead.Id),
                            new NodeTypeDependency(nodeHead.NodeTypeId)
                            );
                    }
                }

                DistributedApplication.Cache.Insert(nodeKey, context, aggregatedDependency);
            }

            if (resolveScripts)
            {
                foreach (var script in context.ImportScriptCollection)
                {
                    UITools.AddScript(script);
                }
                foreach (var css in context.ImportCssCollection)
                {
                    UITools.AddStyleSheetToHeader(UITools.GetHeader(), css);
                }
            }

            return context;
        }
    }
}
