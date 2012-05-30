using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Events;
using SenseNet.ContentRepository;
using SenseNet.Portal.Portlets.ContentHandlers;
using Image=SenseNet.ContentRepository.Image;

namespace SenseNet.Portal.Portlets
{
	public class ImageGalleryObserver : NodeObserver
	{
		protected override void OnNodeModified(object sender, NodeEventArgs e)
		{
			base.OnNodeModified(sender, e);

			if (e.SourceNode is ImageGallery)
			{
				ImageGallery imageGallery = e.SourceNode as ImageGallery;
				foreach (File file in imageGallery.Children)
				{
					GenerateResizedImages(file, imageGallery);
				}
			}
		}

		protected override void OnNodeCreated(object sender, NodeEventArgs e)
		{
			base.OnNodeCreated(sender, e);

			if (e.SourceNode.Parent is ImageGallery && e.SourceNode is File)
			{
				GenerateResizedImages(e.SourceNode as File, e.SourceNode.Parent as ImageGallery);
			}
		}

		private static void GenerateResizedImages(File file, ImageGallery imageGallery)
		{
			bool isLocalTransaction = !TransactionScope.IsActive;
			if (isLocalTransaction)
				TransactionScope.Begin();
			try
			{
				if (file is Image)
				{
					SetResizedBinary(file.Binary, imageGallery, file as Image);
					file.Save();
				}
				else
				{
					Image image = new Image(imageGallery);
					image.Name = file.Name;

					image.Binary = new BinaryData();
					image.Binary.ContentType = file.Binary.ContentType;
					image.Binary.FileName = file.Binary.FileName;
					image.Binary.SetStream(file.Binary.GetStream());

					SetResizedBinary(file.Binary, imageGallery, image);

					file.Delete();
					image.Save();
				}

				if (isLocalTransaction)
					TransactionScope.Commit();
			}
			finally
			{
				if (isLocalTransaction && TransactionScope.IsActive)
					TransactionScope.Rollback();
			}
		}

		private static void SetResizedBinary(BinaryData binaryData, ImageGallery imageGallery, Image image)
		{
			BinaryData view = new BinaryData();
			view.ContentType = binaryData.ContentType;
			view.FileName = string.Concat(binaryData.FileName.FileNameWithoutExtension, "_view.", binaryData.FileName.Extension);
			view.SetStream(ImageResizer.CreateResizedImageFile(binaryData.GetStream(), imageGallery.ViewSizeX, imageGallery.ViewSizeY, imageGallery.ViewQuality));
			image.SetProperty(imageGallery.GetPropertySingleId("#View"), view);

			BinaryData thumb = new BinaryData();
			thumb.ContentType = binaryData.ContentType;
			thumb.FileName = string.Concat(binaryData.FileName.FileNameWithoutExtension, "_thumb.", binaryData.FileName.Extension);
			thumb.SetStream(ImageResizer.CreateResizedImageFile(binaryData.GetStream(), imageGallery.ThumbSizeX, imageGallery.ThumbSizeY, imageGallery.ThumbQuality));
			image.SetProperty(imageGallery.GetPropertySingleId("#Thumb"), thumb);
		}
	}
}