using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Schema;
using SenseNet.Portal;

namespace SenseNet.ContentRepository.Tests
{
	internal class Common
	{
		public static bool CompareByteArray(byte[] expected, byte[] actual)
		{
			bool result = true;

			if (expected.Length == actual.Length)
			{
				for (int i = 0; i < actual.Length; i++)
				{
					if (actual[i] != expected[i])
					{
						result = false;
						break;
					}
				}
			}
			else
			{
				result = false;
			}

			return result;
		}

		//public static void InstallNodeTypes()
		//{
		//    bool exists = false;
		//    foreach (NodeType nodeType in ActiveSchema.NodeTypes)
		//    {
		//        if (nodeType.Name == "SenseNet.ContentRepository.Tests.RichTestNode")
		//        {
		//            exists = true;
		//        }
		//    }
		//    if (!exists)
		//    {
		//        new PluginInstaller().Install(Assembly.GetExecutingAssembly());
		//    }
		//}

		public static object GetMemberValue(string memberName, object target)
		{
			MemberInfo[] miArray = target.GetType().GetMember(memberName, BindingFlags.NonPublic | BindingFlags.Instance);
			if (miArray != null && miArray.Length > 0)
			{
				return ((FieldInfo)miArray[0]).GetValue(target);
			}
			return null;
		}

		public static Page CreateTestPage()
		{
			return CreateTestPage("TestPage");
		}

		public static Page CreateTestPage(string name)
		{
			Page page = null;
			page = Node.LoadNode(string.Concat("/Root/", name)) as Page;
			if (page == null)
			{
				page = new Page(Repository.Root);
				page.Name = name;
				page.Save();
			}
			return page;
		}

		public static Site CreateTestSite()
		{
			return CreateTestSite("TestSite");
		}

		public static Site CreateTestSite(string name)
		{
			Site site = null;
			site = Node.LoadNode(string.Concat("/Root/", name)) as Site;
			if (site == null)
			{
				site = new Site(Repository.Root);
				site.Name = name;
				site.Save();
			}
			return site;
		}

		internal static void CreateFolderStructure(string folderPath)
		{
			if (!string.IsNullOrEmpty(folderPath))
			{
				string[] folderArray = folderPath.Split('/');
				string path = string.Concat(Repository.Root.Path);
				foreach (string folderName in folderArray)
				{
					string parentPath = path;
					path = string.Concat(path, "/", folderName);
					Folder folder = Node.LoadNode(path) as Folder;
					if (folder == null)
					{
						Node parent = Node.LoadNode(parentPath);
						if (parent != null)
						{
							folder = new Folder(parent);
							folder.Name = folderName;
							folder.Save();
						}
					}
				}
			}
		}
	}
}