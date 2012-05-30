using System;
using System.IO;
using System.Reflection;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Search;
using System.Collections.Generic;

namespace SenseNet.Portal.Portlets.ContentHandlers
{
	[ContentHandler]
	public class ImageGallery : ContentList
	{
		//================================================================================= Variables

        [Obsolete("Use typeof(ImageGallery).Name instead.", true)]
        public static readonly string NodeTypeName = typeof(ImageGallery).Name;

		//================================================================================= Constructors

        public ImageGallery(Node parent) : this(parent, null) { }
		public ImageGallery(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
		protected ImageGallery(NodeToken nt) : base(nt) { }

		//================================================================================= Properties

		[RepositoryProperty("Tag", RepositoryDataType.String)]
		public string Tag
		{
			get
			{
				return GetProperty<string>("Tag");
			}
			set
			{
				this["Tag"] = value;
			}
		}

		[RepositoryProperty("ViewSizeX", RepositoryDataType.Int)]
		public int ViewSizeX
		{
			get
			{
				int x = GetProperty<int>("ViewSizeX");
				return x > 0 ? x : 600;
			}
			set
			{
				this["ViewSizeX"] = value;
			}
		}

		[RepositoryProperty("ViewSizeY", RepositoryDataType.Int)]
		public int ViewSizeY
		{
			get
			{
				int y = GetProperty<int>("ViewSizeY");
				return y > 0 ? y : 375;
			}
			set
			{
				this["ViewSizeY"] = value;
			}
		}

		[RepositoryProperty("ViewQuality", RepositoryDataType.Int)]
		public int ViewQuality
		{
			get
			{
				int q = GetProperty<int>("ViewQuality");
				return q > 0 ? q : 100;
			}
			set
			{
				this["ViewQuality"] = value;
			}
		}

		[RepositoryProperty("ThumbSizeX", RepositoryDataType.Int)]
		public int ThumbSizeX
		{
			get
			{
				int x = GetProperty<int>("ThumbSizeX");
				return x > 0 ? x : 200;
			}
			set
			{
				this["ThumbSizeX"] = value;
			}
		}

		[RepositoryProperty("ThumbSizeY", RepositoryDataType.Int)]
		public int ThumbSizeY
		{
			get
			{
				int y = GetProperty<int>("ThumbSizeY");
				return y > 0 ? y : 100;
			}
			set
			{
				this["ThumbSizeY"] = value;
			}
		}

		[RepositoryProperty("ThumbQuality", RepositoryDataType.Int)]
		public int ThumbQuality
		{
			get
			{
				int q = GetProperty<int>("ThumbQuality");
				return q > 0 ? q : 100;
			}
			set
			{
				this["ThumbQuality"] = value;
			}
		}

        public Image FirstImage
        {
            get
            {
                var images = GetImages();
                if (images.Count > 0)
                    return images[0];
                else
                    return null;
            }
        }

        public IList<Image> GetImages()
        {
            NodeQuery query = new NodeQuery();
            query.Add(new TypeExpression(ActiveSchema.NodeTypes["Image"])); // Only find image items
            query.Add(new IntExpression(IntAttribute.ParentId, ValueOperator.Equal, this.Id));
            query.Orders.Add(new SearchOrder(StringAttribute.Name));
            var imageList = query.Execute();
            List<Image> images = new List<Image>();
            foreach (Image image in imageList.Nodes)
            {
                images.Add(image);
            }
            return images;
        }

		//================================================================================= Generic Property handling

		public override object GetProperty(string name)
		{
			switch (name)
			{
                case "DisplayName":
                    return this.DisplayName;
				case "Tag":
					return this.Tag;
				case "ViewSizeX":
					return this.ViewSizeX;
				case "ViewSizeY":
					return this.ViewSizeY;
				case "ViewQuality":
					return this.ViewQuality;
				case "ThumbSizeX":
					return this.ThumbSizeX;
				case "ThumbSizeY":
					return this.ThumbSizeY;
				case "ThumbQuality":
					return this.ThumbQuality;
				default:
					return base.GetProperty(name);
			}
		}

		public override void SetProperty(string name, object value)
		{
			switch (name)
			{
                case "DisplayName":
                    this.DisplayName = (string)value;
					break;
				case "Tag":
					this.Tag = (string)value;
					break;
				case "ViewSizeX":
					this.ViewSizeX = (int)value;
					break;
				case "ViewSizeY":
					this.ViewSizeY = (int)value;
					break;
				case "ViewQuality":
					this.ViewQuality = (int)value;
					break;
				case "ThumbSizeX":
					this.ThumbSizeX = (int)value;
					break;
				case "ThumbSizeY":
					this.ThumbSizeY = (int)value;
					break;
				case "ThumbQuality":
					this.ThumbQuality = (int)value;
					break;
				default:
					base.SetProperty(name, value);
					break;
			}
		}

		//public static void GenerateImages(ImageGallery imageGallery)
		//{
		//    bool isLocalTransaction = !TransactionScope.IsActive;
		//    if (isLocalTransaction)
		//        TransactionScope.Begin();
		//    try
		//    {
		//        foreach (File file in imageGallery.Children)
		//        {
		//            BinaryData view = new BinaryData();
		//            view.ContentType = file.Binary.ContentType;
		//            view.FileName = string.Concat(file.Binary.FileName.FileNameWithoutExtension, "_view.", file.Binary.FileName.Extension);
		//            view.SetStream(ImageResizer.CreateResizedImageFile(file.Binary.GetStream(), file.Binary.FileName.Extension, imageGallery.ViewSizeX, imageGallery.ViewSizeY, imageGallery.ViewQuality, true));
		//            file.SetProperty("#View", view);

		//            BinaryData thumb = new BinaryData();
		//            thumb.ContentType = file.Binary.ContentType;
		//            thumb.FileName = string.Concat(file.Binary.FileName.FileNameWithoutExtension, "_thumb.", file.Binary.FileName.Extension);
		//            thumb.SetStream(ImageResizer.CreateResizedImageFile(file.Binary.GetStream(), file.Binary.FileName.Extension, imageGallery.ThumbSizeX, imageGallery.ThumbSizeY, imageGallery.ThumbQuality, true));
		//            file.SetProperty("#Thumb", thumb);

		//            file.Save();

		//            //File image = new File(imageGallery);
		//            //image.Name = file.Name;

		//            //image.Binary = new BinaryData();
		//            //image.Binary.ContentType = file.Binary.ContentType;
		//            //image.Binary.FileName = file.Binary.FileName;
		//            //image.Binary.SetStream(file.Binary.GetStream());

		//            //image.View = new BinaryData();
		//            //image.View.ContentType = file.Binary.ContentType;
		//            //image.View.FileName = image.Name;
		//            //image.View.SetStream(ImageResizer.CreateResizedImageFile(file.Binary.GetStream(), file.Binary.FileName.Extension, imageGallery.ViewSizeX, imageGallery.ViewSizeY, imageGallery.ViewQuality, true));

		//            //image.Thumb = new BinaryData();
		//            //image.Thumb.ContentType = file.Binary.ContentType;
		//            //image.Thumb.FileName = image.Name;
		//            //image.Thumb.SetStream(ImageResizer.CreateResizedImageFile(file.Binary.GetStream(), file.Binary.FileName.Extension, imageGallery.ThumbSizeX, imageGallery.ThumbSizeY, imageGallery.ThumbQuality, true));

		//            //file.DeletePhysical();
		//            //image.Save();
		//        }
		//        if (isLocalTransaction)
		//            TransactionScope.Commit();
		//    }
		//    catch (Exception ex)
		//    {
		//        throw ex;
		//    }
		//    finally
		//    {
		//        if (isLocalTransaction && TransactionScope.IsActive)
		//            TransactionScope.Rollback();
		//    }
		//}
	}
}