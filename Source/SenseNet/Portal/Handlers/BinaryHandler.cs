using System;
using System.Web;
using System.IO;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.ContentRepository;
using SenseNet.Portal.Virtualization;

namespace SenseNet.Portal.Handlers
{
    public class BinaryHandler : IHttpHandler
    {
        /* ============================================================================= Public Properties */
        public static NodeHead RequestedNodeHead
        {
            get
            {
                var nodeid = RequestedNodeId;
                if (nodeid.HasValue)
                    return NodeHead.Get(nodeid.Value);

                var nodepath = RequestedNodePath;
                if (!string.IsNullOrEmpty(nodepath))
                    return NodeHead.Get(nodepath);

                return null;
            }
        }

        
        /* ============================================================================= Properties */
        private string PropertyName
        {
            get
            {
                var propertyName = HttpContext.Current.Request.QueryString["propertyname"];
                if (string.IsNullOrEmpty(propertyName))
                    return null;

                return propertyName.Replace("$", "#");
            }
        }
        private static int? RequestedNodeId
        {
            get
            {
                var nodeidStr = HttpContext.Current.Request.QueryString["nodeid"];
                if (!string.IsNullOrEmpty(nodeidStr))
                {
                    int nodeid;
                    var success = Int32.TryParse(nodeidStr, out nodeid);
                    if (success)
                        return nodeid;
                }
                return null;
            }
        }
        private static string RequestedNodePath
        {
            get
            {
                var nodePathStr = HttpContext.Current.Request.QueryString["nodepath"];
                return nodePathStr;
            }
        }
        private static Node RequestedNode
        {
            get
            {
                return Node.LoadNode(PortalContext.Current.BinaryHandlerRequestedNodeHead);
            }
        }
        private int? Width
        {
            get
            {
                var widthStr = HttpContext.Current.Request.QueryString["width"];
                if (!string.IsNullOrEmpty(widthStr))
                {
                    int width;
                    var success = Int32.TryParse(widthStr, out width);
                    if (success)
                        return width;
                }
                return null;
            }
        }
        private int? Height
        {
            get
            {
                var heightStr = HttpContext.Current.Request.QueryString["height"];
                if (!string.IsNullOrEmpty(heightStr))
                {
                    int height;
                    var success = Int32.TryParse(heightStr, out height);
                    if (success)
                        return height;
                }
                return null;
            }
        }


        /* ============================================================================= IHttpHandler */
        public bool IsReusable
        {
            get { return false; }
        }
        public void ProcessRequest(HttpContext context)
        {
            var propertyName = PropertyName;
            var requestedNode = RequestedNode;

            if (string.IsNullOrEmpty(propertyName) || requestedNode == null)
                return;

            var property = requestedNode.PropertyTypes[propertyName];
            if (property == null || property.DataType != DataType.Binary)
                return;

            int bufferSize = 256;
            byte[] buffer = new byte[bufferSize];
            Stream imageStream;

            var binary = requestedNode.GetBinary(property);
            imageStream = binary.GetStream();

            if (imageStream == null)
                return;

            context.Response.ContentType = binary.ContentType;

            var resizedStream = imageStream;

            if (Width.HasValue && Height.HasValue)
                resizedStream = Image.CreateResizedImageFile(imageStream, string.Empty, Width.Value, Height.Value, 0, binary.ContentType);

            resizedStream.Position = 0;
            int bytesRead = resizedStream.Read(buffer, 0, bufferSize);
            while (bytesRead > 0)
            {
                context.Response.OutputStream.Write(buffer, 0, bytesRead);
                bytesRead = resizedStream.Read(buffer, 0, bufferSize);
            }

            //let the client code log file downloads
            var file = requestedNode as ContentRepository.File;
            if (file != null)
                ContentRepository.File.Downloaded(file.Id);

            context.Response.OutputStream.Flush();
        }
    }
}
