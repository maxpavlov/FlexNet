using System;
using System.IO;
using System.Linq;
using System.Web;
using SenseNet.ContentRepository.Fields;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Diagnostics;


namespace SenseNet.ContentRepository
{
    [ContentHandler]
    public class Image : File, IHttpHandler
    {
        [Obsolete("Use typeof(Image).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(Image).Name;

        //================================================================================= Constructors
        public Image(Node parent) : this(parent, null) { }
        public Image(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
        protected Image(NodeToken nt) : base(nt) { }


        //================================================================================= Properties
        public string Extension
        {
            get
            {
                string[] nameParts = Name.Split('.');
                string extension = nameParts[nameParts.Length - 1].ToLower();
                if (extension == "jpg")
                    extension = "jpeg";
                return extension;
            }
        }
        public string ContentType
        {
            get
            {
                return String.Concat("image/", this.Extension);
            }
        }


        //================================================================================= Methods
        public static System.Drawing.Imaging.ImageFormat getImageFormat(string contentType)
        {
            var lowerContentType = contentType.ToLower();

            if (lowerContentType.EndsWith("png"))
                return System.Drawing.Imaging.ImageFormat.Png;
            if (lowerContentType.EndsWith("bmp"))
                return System.Drawing.Imaging.ImageFormat.Bmp;
            if (lowerContentType.EndsWith("jpeg"))
                return System.Drawing.Imaging.ImageFormat.Jpeg;
            if (lowerContentType.EndsWith("jpg"))
                return System.Drawing.Imaging.ImageFormat.Jpeg;

            // gif -> png! resizing gif with gif imageformat ruins alpha values, therefore we return with png
            if (lowerContentType.EndsWith("gif"))
                return System.Drawing.Imaging.ImageFormat.Png;
            if (lowerContentType.EndsWith("tiff"))
                return System.Drawing.Imaging.ImageFormat.Tiff;
            if (lowerContentType.EndsWith("wmf"))
                return System.Drawing.Imaging.ImageFormat.Wmf;
            if (lowerContentType.EndsWith("emf"))
                return System.Drawing.Imaging.ImageFormat.Emf;
            if (lowerContentType.EndsWith("exif"))
                return System.Drawing.Imaging.ImageFormat.Exif;

            return System.Drawing.Imaging.ImageFormat.Jpeg;
        }
        public static new Image CreateByBinary(IFolder parent, BinaryData binaryData)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            if (binaryData == null)
                return new Image(parent as Node);

            Image image = new Image(parent as Node);;
            // Resolve filetype by binary-config matching
            binaryData.FileName = new BinaryFileName(image.Name, image.Extension);
            image.Binary = binaryData;
            return image;
        }
        public Stream GetDynamicThumbnailStream(int width, int height, string contentType)
        {
            return ImageResizer.CreateResizedImageFile(Binary.GetStream(), width, height, 80, getImageFormat(contentType));
        }
        public static Stream CreateResizedImageFile(Stream originalStream, string ext, double x, double y, double q, string contentType)
        {
            return ImageResizer.CreateResizedImageFile(originalStream, x, y, q, getImageFormat(contentType));
        }
        protected override void OnCreated(object sender, SenseNet.ContentRepository.Storage.Events.NodeEventArgs e)
        {
            var image = sender as Image;
            if (image == null)
                return;

            // thumbnail has been loaded -> reference it in parent's imagefield (if such exists)
            if (image.Name.ToLower().StartsWith("thumbnail"))
            {
                var parent = image.Parent;
                var content = Content.Create(parent);

                // first available imagefield is used
                var imageField = content.Fields.Where(d => d.Value is ImageField).Select(d => d.Value as ImageField).FirstOrDefault();
                if (imageField != null)
                {
                    // initialize field (field inner data is not yet initialized from node properties!)
                    imageField.GetData();

                    // set reference
                    var result = imageField.SetThumbnailReference(image);
                    if (result)
                        content.Save();
                }
            }
            base.OnCreated(sender, e);
        }
        protected override void OnCreating(object sender, Storage.Events.CancellableNodeEventArgs e)
        {
            base.OnCreating(sender, e);
            if(!e.Cancel)
            {
                var img = sender as Image;
                if (img == null)
                    return;

                SetDimension(img);
            }
        }

        protected override void OnModifying(object sender, Storage.Events.CancellableNodeEventArgs e)
        {
            base.OnModifying(sender, e);
            if (!e.Cancel)
            {
                var img = sender as Image;
                if (img == null)
                    return;

                SetDimension(img);
            }
        }

        private void SetDimension(Image imgNode)
        {
            try
            {
                using (var img = System.Drawing.Image.FromStream(imgNode.Binary.GetStream()))
                {
                    imgNode["Width"] = img.Width;
                    imgNode["Height"] = img.Height;
                }
            }
            catch(Exception ex)
            {
                Logger.WriteException(ex);
            }
        }

        //================================================================================= IHttpHandler members
        bool IHttpHandler.IsReusable
        {
            get { return false; }
        }
        void IHttpHandler.ProcessRequest(HttpContext context)
        {
            int bufferSize = 256;
            byte[] buffer = new byte[bufferSize];
            Stream imageStream;
			if (context.Request.QueryString["NodeProperty"] != null)
			{
				string propertyName = context.Request.QueryString["NodeProperty"].Replace("$", "#");

				var property = this.PropertyTypes[propertyName];
				if (property != null && property.DataType == DataType.Binary)
					imageStream = this.GetBinary(property).GetStream();
				else
					imageStream = this.Binary.GetStream();
			}
			else if (context.Request.QueryString["dynamicThumbnail"] != null && context.Request.QueryString["width"] != null && context.Request.QueryString["height"] != null)
			{
                int width;
                int height;
                if (!int.TryParse(context.Request.QueryString["width"], out width))
                    width = 200;
                if (!int.TryParse(context.Request.QueryString["height"], out height))
                    height = 200;

                imageStream = this.GetDynamicThumbnailStream(width, height, this.ContentType);
			}
			else
			{
				imageStream = Binary.GetStream();
			}

            imageStream.Position = 0;
            context.Response.ContentType = this.ContentType;
            int bytesRead = imageStream.Read(buffer, 0, bufferSize);
            while (bytesRead > 0)
            {
                context.Response.OutputStream.Write(buffer, 0, bytesRead);
                bytesRead = imageStream.Read(buffer, 0, bufferSize);
            }
            context.Response.OutputStream.Flush();
        }
    }
}
