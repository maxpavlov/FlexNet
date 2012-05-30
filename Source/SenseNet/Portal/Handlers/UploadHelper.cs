using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.Portal.Virtualization;
using System.IO;
using System.Web;

namespace SenseNet.Portal.Handlers
{
    public class UploadHelper
    {
        // ============================================================================ Consts
        public const string AUTOELEMENT = "Auto";


        // ============================================================================ Private methods
        private static NameValueCollection FileExtensions
        {
            get
            {
                return System.Configuration.ConfigurationManager.GetSection("sensenet/uploadFileExtensions") as NameValueCollection;
            }
        }


        // ============================================================================ Public methods
        /// <summary>
        /// Determines content type from fileextension or given contentType
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetContentType(string fileName, string contentType)
        {
            if ((contentType == AUTOELEMENT) || (String.IsNullOrEmpty(contentType)))
            {
                // no fileextension data in webconfig
                if (FileExtensions == null)
                    return null;

                int extStart = fileName.LastIndexOf('.');
                if (extStart != -1)
                {
                    string extension = fileName.Substring(extStart);

                    if (!string.IsNullOrEmpty(extension))
                    {
                        string fileType = FileExtensions[extension];
                        if (!string.IsNullOrEmpty(fileType))
                        {
                            return fileType;
                        }
                    }
                }
                // default
                return null;
            }
            return contentType;
        }

        /// <summary>
        /// Checks if a content of the given ContentType is allowed under a given parent content
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child">child node whose NodeType will be checked</param>
        /// <returns></returns>
        public static bool CheckAllowedContentType(GenericContent parent, Node child)
        {
            if (child == null || parent == null)
                return true;
            
            var nodetypeName = child.NodeType.Name;
            return CheckAllowedContentType(parent, nodetypeName);
        }
        /// <summary>
        /// Checks if a content of the given ContentType is allowed under a given parent content
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="contentTypeName"></param>
        /// <returns></returns>
        public static bool CheckAllowedContentType(GenericContent parent, string contentTypeName)
        {
            // check allowed content types
            // true: allowed list is empty, but current user is administrator (if allowed types list is empty: only administrator should upload any type.)
            // true: if this type is allowed 
            var cTypes = parent.GetAllowedChildTypes().ToList();
            if ((cTypes.Count == 0 && PortalContext.Current.ArbitraryContentTypeCreationAllowed) || (cTypes.Any(ct => ct.Name == contentTypeName)))
                return true;

            return false;
        }

        /// <summary>
        /// Creates BinaryData from filename and stream
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static BinaryData CreateBinaryData(string fileName, Stream stream)
        {
            var binaryData = new BinaryData();
            binaryData.FileName = fileName;
            binaryData.SetStream(stream);
            return binaryData;
        }

        /// <summary>
        /// Creates BinaryData from HttpPostedFile
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static BinaryData CreateBinaryData(HttpPostedFile file)
        {
            BinaryData result = new BinaryData();

            string fileName = file.FileName;
            if (file.FileName.LastIndexOf("\\") > -1)
                fileName = file.FileName.Substring(file.FileName.LastIndexOf("\\") + 1);

            result.FileName = new BinaryFileName(fileName);
            result.ContentType = file.ContentType;
            result.SetStream(file.InputStream);

            return result;
        }

        /// <summary>
        /// Creates new Node of specified Content Type
        /// </summary>
        /// <param name="contentTypeName"></param>
        /// <param name="parent"></param>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        public static void CreateNodeOfType(string contentTypeName, Node parent, string fileName, Stream stream)
        {
            var node = new SenseNet.ContentRepository.File(parent, contentTypeName);

            if (CheckAllowedContentType(parent as GenericContent, node))
            {
                node.Name = fileName;
                node.SetBinary("Binary", UploadHelper.CreateBinaryData(fileName, stream));
                node.Save();
            }
        }

        /// <summary>
        /// Modify node's binary
        /// </summary>
        /// <param name="node"></param>
        /// <param name="stream"></param>
        public static void ModifyNode(Node node, Stream stream)
        {
            node.SetBinary("Binary", UploadHelper.CreateBinaryData(node.Name, stream));
            node.Save();
        }
    }
}
