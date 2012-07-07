using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SenseNet.ContentRepository.Storage;
using System.Web;
using SenseNet.ContentRepository.Versioning;
using System.Globalization;
using System.Linq;
using SenseNet.Diagnostics;
using System.Security.Cryptography;

namespace SenseNet.ContentRepository
{
	public static class Tools
	{
		public static string GetStreamString(Stream stream)
		{
			StreamReader sr = new StreamReader(stream);
			stream.Position = 0;
			return sr.ReadToEnd();
		}
		public static Stream GetStreamFromString(string textData)
		{
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
			writer.Write(textData);
			writer.Flush();

			return stream;
		}

        public static CultureInfo GetUICultureByNameOrDefault(string cultureName)
        {
            CultureInfo cultureInfo = null;

            if (!String.IsNullOrEmpty(cultureName))
            {
                cultureInfo = (from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
                               where c.Name == cultureName
                               select c).FirstOrDefault();
            }
            if (cultureInfo == null)
                cultureInfo = CultureInfo.CurrentUICulture;

            return cultureInfo;
        }

		public static string GetVersionString(Node node)
		{
			string extraText = string.Empty;
			switch (node.Version.Status)
			{
				case VersionStatus.Pending: extraText = HttpContext.GetGlobalResourceObject("Portal", "Approving") as string; break;
				case VersionStatus.Draft: extraText = HttpContext.GetGlobalResourceObject("Portal", "Draft") as string; break;
				case VersionStatus.Locked:
			        var lockedByName = node.Lock.LockedBy == null ? "" : node.Lock.LockedBy.Name;
                    extraText = string.Concat(HttpContext.GetGlobalResourceObject("Portal", "CheckedOutBy") as string, " ", lockedByName);
                    break;
				case VersionStatus.Approved: extraText = HttpContext.GetGlobalResourceObject("Portal", "Public") as string; break;
				case VersionStatus.Rejected: extraText = HttpContext.GetGlobalResourceObject("Portal", "Reject") as string; break;
			}

            var content = node as GenericContent;
            var vmode = VersioningType.None;
            if (content != null)
                vmode = content.VersioningMode;

            if (vmode == VersioningType.None)
                return extraText;
            if (vmode == VersioningType.MajorOnly)
                return string.Concat(node.Version.Major, " ", extraText);
            return string.Concat(node.Version.Major, ".", node.Version.Minor, " ", extraText);
		}

        [Obsolete("Use ContentNamingHelper.EnsureContentName(string, Node) instead", true)]
        public static string EnsureContentName(string nameBase, Node container)
        {
            return ContentNamingHelper.EnsureContentName(nameBase, container);
        }
        [Obsolete("Use RepositoryPath.GetParentPath(string) instead", true)]
        public static string GetParentPathSafe(string path)
        {
            return RepositoryPath.GetParentPath(path);
        }
        [Obsolete("Use RepositoryPath.GetFileNameSafe(string) instead", true)]
        public static string GetFileNameSafe(string path)
        {
            return RepositoryPath.GetFileNameSafe(path);
        }

        public static string CalculateMD5(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            using (var stream = new MemoryStream(bytes))
            {
                return CalculateMD5(stream, 64 * 1024);
            }
        }

        public static string CalculateMD5(Stream stream, int bufferSize)
        {
            MD5 md5Hasher = MD5.Create();

            byte[] buffer = new byte[bufferSize];
            int readBytes;

            while ((readBytes = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                md5Hasher.TransformBlock(buffer, 0, readBytes, buffer, 0);
            }

            md5Hasher.TransformFinalBlock(new byte[0], 0, 0);

            var result = md5Hasher.Hash.Aggregate(string.Empty, (full, next) => full + next.ToString("x2"));
            return result;
        }

        // Structure building ==================================================================

        public static Content CreateStructure(string path)
        {
            return CreateStructure(path, "Folder");
        }

	    public static Content CreateStructure(string path, string containerTypeName)
        {
            //check path validity before calling the recursive method
            if (string.IsNullOrEmpty(path))
                return null;

	        RepositoryPath.CheckValidPath(path);

            return EnsureContainer(path, containerTypeName);
        }

        private static Content EnsureContainer(string path, string containerTypeName)
        {
            if (Node.Exists(path))
                return null;

            var name = RepositoryPath.GetFileName(path);
            var parentPath = RepositoryPath.GetParentPath(path);

            //recursive call to create parent containers
            EnsureContainer(parentPath, containerTypeName);

            return CreateContent(parentPath, name, containerTypeName);
        }

        private static Content CreateContent(string parentPath, string name, string typeName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            var parent = Node.LoadNode(parentPath);

            if (parent == null)
                throw new ApplicationException("Parent does not exist: " + parentPath);

            //don't use admin account here, that should be 
            //done in the calling 'client' code if needed
            var content = Content.CreateNew(typeName, parent, name);
            content.Save();
            
            return content;
        }

        // Diagnostics =========================================================================

        public static string CollectExceptionMessages(Exception ex)
        {
            var sb = new StringBuilder();
            var e = ex;
            while (e != null)
            {
                sb.AppendLine(e.Message).AppendLine(e.StackTrace).AppendLine("-----------------");
                e = e.InnerException;
            }
            return sb.ToString();
        }
	}
}
