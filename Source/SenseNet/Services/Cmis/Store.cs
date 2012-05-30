using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


using SenseNet.ContentRepository;
using SNC = SenseNet.ContentRepository;

namespace SenseNet.Services.Cmis
{
	internal static class Store
	{
		internal static SNC.Content GetContent(int contentId, string repositoryId)
		{
			SNC.Content content = null;
			content = SNC.Content.Load(contentId);
			if (content == null)
				throw new ArgumentException("Object '" + contentId + "' is not exist in '" + repositoryId + "' repository");
			return content;
		}
		internal static List<SNC.Content> GetChildren(int contentId, string repositoryId, out SNC.Content folder)
		{
			SNC.Content content = null;
			content = SNC.Content.Load(contentId);

            if (content == null)
				throw new ArgumentException("Object '" + contentId + "' is not exist in '" + repositoryId + "' repository");

			var ifolder = content.ContentHandler as IFolder;
			if (ifolder == null)
				throw new ArgumentException("Object is not a folder (id = '" + contentId + "')");

			var contentList = new List<SNC.Content>();
			foreach (var node in ifolder.Children)
				contentList.Add(SNC.Content.Create(node));

			folder = content;
			return contentList;
		}

		internal static string GetUserName(User user)
		{
			if (user == null)
				return String.Empty;

			if (String.IsNullOrEmpty(user.FullName))
				return user.Name;
			return user.FullName;
		}

	}
}