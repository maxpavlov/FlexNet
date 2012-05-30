using System;
using System.Collections.Generic;
using System.Web;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository;
using System.Collections.Specialized;
using SenseNet.Diagnostics;
using SenseNet.Portal.Virtualization;
using System.Linq;
using System.Configuration;

namespace SenseNet.Portal.Handlers
{
    public class UploadHandler : IHttpHandler
    {

        internal class ContentFacade
        {
            public Content CurrentContent { get; set; }

            public ContentFacade()
            {
                    
            }

            /// <summary>
            /// Creates a new content with the specified contenttypename.
            /// </summary>
            /// <param name="contentTypeName">ContentTypeName is used for creating content</param>
            /// <param name="parentNode">Parent node which stores the new content.</param>
            public void CreateNew(string contentTypeName, Node parentNode)
            {
                if (String.IsNullOrEmpty(contentTypeName))
                    throw new ArgumentException("ContentTypeName is null");
                if (parentNode == null)
                    throw new ArgumentException("Parent node is null.");

                CurrentContent = Content.CreateNew(contentTypeName, parentNode, string.Empty);
                if (CurrentContent == null)
                    throw new InvalidOperationException(String.Format("Couldn't create '{0}' content.", contentTypeName));

            }

            /// <summary>
            /// Loads the specified content by content id.
            /// </summary>
            /// <param name="contentId">ContentId</param>
            public void Load(int contentId)
            {
                CurrentContent = Content.Load(contentId);
            }

            /// <summary>
            /// Loads the specified content by content path.
            /// </summary>
            /// <param name="path">Path of the specified content.</param>
            public void Load(string path)
            {
                CurrentContent = Content.Load(path);
            }


        }


        private const string ParentIdRequestParameterName = "ParentId";

        private bool _errorHandled;

        #region IHttpHandler Members

        public bool IsReusable
        {
            get { return true; }
        }
        /// <summary>
        /// Gets the ContentType name which is used for creating new content by typename.
        /// </summary>
        private string ContentType
        {
            get
            {
                var result = HttpContext.Current.Request.Form["ContentType"];

                return !string.IsNullOrEmpty(result) ? result : null;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            HttpFileCollection uploadedFiles = context.Request.Files;
            var path = string.Empty;

            // actual info: we handle one file
            if (uploadedFiles.Count == 1)
            {
                var postedFile = uploadedFiles[0];
                var parentId = HttpContext.Current.Request.Form[ParentIdRequestParameterName];

                if (String.IsNullOrEmpty(parentId))
                    parentId = TryToGetParentId(context);
               
                if (!String.IsNullOrEmpty(parentId))
                {
                    IFolder parentNode = null;
                    var parentNodeId = Convert.ToInt32(parentId);
                    parentNode = parentNodeId == 0 ? Repository.Root : Node.LoadNode(parentNodeId) as IFolder;
                    
                    if (parentNode != null)
                    {
                        string fileName = System.IO.Path.GetFileName(postedFile.FileName);
                        path = RepositoryPath.Combine((parentNode as Node).Path, fileName);
                        //PathInfoRemove:
						//object t = RepositoryPathInfo.GetPathInfo(path);
                        var contentType = UploadHelper.GetContentType(postedFile.FileName, ContentType);
                        var t = NodeHead.Get(path);
                        if (t == null)
                        {
                            if (!String.IsNullOrEmpty(contentType))
                                CreateSpecificContent(context, contentType, parentNode, postedFile);
                            else
                                SaveNewFile(context, postedFile, parentNode, path);
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(contentType))
                            {
                                ModifySpecificContent(parentNode, postedFile, path);
                            } else
                                ModifyFile(context, postedFile, parentNode, path);
                        }
                            
                    }
                    else
                        this.SetError(context, String.Format("Couldn't find parent node with {0} id.", parentId), 500);
                }
                else
                    this.SetError(context, "Post parameter error: ParentId is null or empty!", 500);
            }

            var backUrl = PortalContext.Current.BackUrl;
            if (!String.IsNullOrEmpty(backUrl))
            {
                context.Response.Redirect(backUrl);      
            }

            // everything's fine
            context.Response.StatusCode = 200;
            context.Response.Write(Environment.NewLine);
            context.Response.End();
        }

        private static string TryToGetParentId(HttpContext context)
        {
            foreach(var item in context.Request.Form)
            {
                var id = item as string;
                if (String.IsNullOrEmpty(id) || !id.Contains("ParentId")) 
                    continue;
                var value = context.Request[id];
                return value;
            }
            return null;
        }

        private void ModifySpecificContent(IFolder node, HttpPostedFile file, string path)
        {
            ContentFacade contentFacade = new ContentFacade();
            contentFacade.Load(path);
            if (contentFacade.CurrentContent != null)
            {
                BinaryField bf = contentFacade.CurrentContent.Fields["Binary"] as BinaryField;
                if (bf != null)
                {
                    BinaryData bd = CreateBinaryData(file);
                    bf.SetData(bd);
                }
            }
            contentFacade.CurrentContent.Save();

        }

        private void CreateSpecificContent(HttpContext context, string contentTypeName, IFolder parentNode, HttpPostedFile postedFile)
        {
            ContentFacade contentFacade = new ContentFacade();
            contentFacade.CreateNew(contentTypeName,((Node)parentNode));

            if (CheckAllowedContentType(parentNode as GenericContent, contentFacade.CurrentContent.ContentHandler, context))
            {
                // set name
                ShortTextField sf = contentFacade.CurrentContent.Fields["Name"] as ShortTextField;
                var fn = System.IO.Path.GetFileName(postedFile.FileName);
                sf.SetData(fn);

                // set binary
                BinaryData b = CreateBinaryData(postedFile);
                BinaryField bf = contentFacade.CurrentContent.Fields["Binary"] as BinaryField;

                bf.SetData(b);

                contentFacade.CurrentContent.Save();
            }
        }

        #endregion
        
        // ------------------------------------------------------------------ Methods

        private void SaveNewFile(HttpContext context, HttpPostedFile postedFile, IFolder parentNode, string path)
        {
            try
            {
                if (postedFile.ContentType.StartsWith("image/") || IsImageFileExtension(postedFile.FileName))
                {
                    var image = Image.CreateByBinary(parentNode, CreateBinaryData(postedFile));
                    if (image != null)
                    {
                        if (CheckAllowedContentType(parentNode as GenericContent, image, context))
                        {
                            image.Name = postedFile.FileName;
                            image.Save();
                        }
                    }
                    else
                        this.SetError(context, String.Format("Error creating content with {0} path.", path), 500);
                }
                else
                {
                    var file = File.CreateByBinary(parentNode, CreateBinaryData(postedFile));
                    if (file != null)
                    {
                        if (CheckAllowedContentType(parentNode as GenericContent, file, context))
                        {
                            file.Name = System.IO.Path.GetFileName(postedFile.FileName);
                            file.Save();
                        }
                    }
                    else
                        this.SetError(context, String.Format("Error creating content with {0} path.", path), 500);
                
                }
            }
            catch(Exception e) //logged
            {
                Logger.WriteException(e);
                 this.SetError(context, String.Format("Error creating content with {0} path.", path), 500);
            }            
        }

        private bool IsImageFileExtension(string fileName)
        {
            return fileName.ToLower().EndsWith(".jpg") 
                || fileName.ToLower().EndsWith(".jpeg") 
                || fileName.ToLower().EndsWith(".gif") 
                || fileName.ToLower().EndsWith(".png") 
                || fileName.ToLower().EndsWith(".bmp");
        }

        private void ModifyFile(HttpContext context, HttpPostedFile postedFile, IFolder parentNode, string path)
        {
            File file = Node.LoadNode(path) as File;
            if (file != null)
            {
                file.Binary.SetStream(postedFile.InputStream);
                file.Save();
            }
        }

        private bool CheckAllowedContentType(GenericContent parent, Node child, HttpContext context)
        {
            var result = UploadHelper.CheckAllowedContentType(parent, child);
            if (!result)
                RedirectToErrorPage(String.Format("Cannot upload this type of content to {0}.", parent.Path), context);

            return result;
        }

        private void RedirectToErrorPage(string errorMessage, HttpContext context)
        {
            _errorHandled = true;

            var errorPagePath = ConfigurationManager.AppSettings["UploadErrorPage"];
            var back = PortalContext.Current.BackUrl ?? string.Empty;

            if (string.IsNullOrEmpty(errorPagePath))
            {
                if (!string.IsNullOrEmpty(back))
                {
                    context.Response.Redirect(back, true);
                }
                else
                {
                    this.SetError(context, errorMessage, 500);
                }
            }
            else
            {
                errorPagePath = string.Format("{0}?back={1}", errorPagePath, back);

                context.Response.Redirect(errorPagePath, true);
            }
        }

        private void SetError(HttpContext context, string message, int statusCode)
        {
            //if we already handled an error
            if (_errorHandled)
                return;

            _errorHandled = true;

            Logger.WriteError(message, new Dictionary<string, object> { { "StatusCode", statusCode } });

            context.Response.StatusCode = statusCode;
            context.Response.Write(message);
            context.Response.End();
        }


        // ------------------------------------------------------------------ Static methods
        public static BinaryData CreateBinaryData(HttpPostedFile file)
        {
            return UploadHelper.CreateBinaryData(file);
        }
    }
}
