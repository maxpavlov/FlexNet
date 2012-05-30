using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SnC = SenseNet.ContentRepository;
using System.Xml;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository;

namespace SenseNet.Packaging.Internal
{
	internal class ContentManager
	{
		public static class Path
		{
			internal static string Combine(string path1, string path2)
			{
				return RepositoryPath.Combine(path1, path2);
			}
			internal static string GetParentPath(string path)
			{
				return RepositoryPath.GetParentPath(path);
			}
			internal static string GetFileName(string path)
			{
				return RepositoryPath.GetFileName(path);
			}
			internal static bool IsPathExists(string path)
			{
				return Node.Exists(path);
			}
		}

		//================================================= Static model

		public static string ContentViewsFolderPath { get { return Repository.ContentViewGlobalFolderPath; } }
		public static string PageTemplatesFolderPath { get { return Repository.PageTemplatesFolderPath; } }
		public static string ResourceFolderPath { get { return Repository.ResourceFolderPath; } }

		public static void InstallContentTypes(IEnumerable<string> contentTypeDefinitions)
		{
			var installer = ContentTypeInstaller.CreateBatchContentTypeInstaller();
			foreach (var contentTypeDefinition in contentTypeDefinitions)
				installer.AddContentType(contentTypeDefinition);
			installer.ExecuteBatch();
		}

		public static StepResult InstallContent(Content content)
		{
			bool isNewContent;
			try
			{
				SnC.Content snContent = CreateOrLoadContent(content, out isNewContent);
				foreach (var attachment in content.Attachments)
				{
					var data = new BinaryData() { FileName = attachment.FileName };
					data.SetStream(attachment.Manifest.GetStream(attachment.FileName));
					snContent[attachment.FieldName] = data;
				}
				snContent.Save();
				content.IsNewContent = isNewContent;
                if (!SetMetadata(snContent, content, isNewContent, false))
                {
                    Logger.LogWarningMessage(PrintFieldErrors(snContent));
                    return new StepResult { Kind = StepResultKind.Warning };
                }
			}
			catch (Exception transferEx)
			{
                Logger.LogException(transferEx);
				return new StepResult { Kind = StepResultKind.Error };
			}
			return new StepResult { Kind = StepResultKind.Successful };
		}
		public static StepResult UpdateReferences(Content content)
		{
			try
			{
				SnC.Content snContent = SnC.Content.Load(content.Path);
				//snContent.Save();
				if (!SetMetadata(snContent, content, content.IsNewContent, true))
                {
                    Logger.LogWarningMessage(PrintFieldErrors(snContent));
                    return new StepResult { Kind = StepResultKind.Warning };
                }
			}
			catch (Exception transferEx)
			{
                Logger.LogException(transferEx);
				return new StepResult { Kind = StepResultKind.Error };
			}
			return new StepResult { Kind = StepResultKind.Successful };
		}

		private static SnC.Content CreateOrLoadContent(Content contentInfo, out bool isNewContent)
		{
			var parentPath = ContentManager.Path.GetParentPath(contentInfo.Path);
			Node parent = Node.LoadNode(parentPath);
			if(parent == null)
				parent = EnsureFolder(parentPath);

			//if (!(parent is IFolder))
			//    throw new InstallException("");

			SnC.Content content = SnC.Content.Load(contentInfo.Path);
			isNewContent = content == null;
			if(isNewContent)
				content = SnC.Content.CreateNew(contentInfo.ContentType, parent, contentInfo.Name);

			return content;
		}
		private static bool SetMetadata(SnC.Content snContent, Content content, bool isNewContent, bool updateReferences)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(content.Data);
			var context = new ImportContext(xml.SelectNodes("/ContentMetaData/Fields/*"), null, isNewContent, true, updateReferences);
			bool result = snContent.ImportFieldData(context);
			var contentId = snContent.ContentHandler.Id;
			content.HasReference = context.HasReference;
			return result;
		}
		private static Node EnsureFolder(string path)
		{
			var node = Node.LoadNode(path);
			if (node != null)
				return node;
			var parentPath = ContentManager.Path.GetParentPath(path);
			var folderName =  ContentManager.Path.GetFileName(path);
			var parent = EnsureFolder(parentPath);
			var folder = new Folder(parent);
			folder.Name = folderName;
			folder.Save();
			return folder;
		}

		private static string PrintException(Exception e)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(e.GetType().Name);
			sb.Append(": ");
			sb.AppendLine(e.Message);
			PrintTypeLoadError(e as System.Reflection.ReflectionTypeLoadException, sb);
			sb.AppendLine(e.StackTrace);
			while ((e = e.InnerException) != null)
			{
				sb.AppendLine("---- Inner Exception:");
				sb.Append(e.GetType().Name);
				sb.Append(": ");
				sb.AppendLine(e.Message);
				PrintTypeLoadError(e as System.Reflection.ReflectionTypeLoadException, sb);
				sb.AppendLine(e.StackTrace);
			}
			return sb.ToString();
		}
		private static void PrintTypeLoadError(System.Reflection.ReflectionTypeLoadException exc, StringBuilder sb)
		{
			if (exc == null)
				return;
			sb.AppendLine("LoaderExceptions:");
			foreach (var e in exc.LoaderExceptions)
			{
				sb.Append("-- ");
				sb.Append(e.GetType().FullName);
				sb.Append(": ");
				sb.AppendLine(e.Message);

				var fileNotFoundException = e as FileNotFoundException;
				if (fileNotFoundException != null)
				{
					sb.AppendLine("FUSION LOG:");
					sb.AppendLine(fileNotFoundException.FusionLog);
				}
			}
		}
		private static string PrintFieldErrors(SnC.Content content)
		{
			var sb = new StringBuilder();
			sb.AppendLine("---------- Field Errors:");
			foreach (string fieldName in content.Fields.Keys)
			{
				Field field = content.Fields[fieldName];
				if (!field.IsValid)
				{
					sb.Append(field.Name);
					sb.Append(": ");
					sb.AppendLine(field.GetValidationMessage());
				}
			}
			sb.AppendLine("------------------------");
			return sb.ToString();
		}

		internal static object GetContentType(string ContentTypeParentName)
		{
			return ContentType.GetByName(ContentTypeParentName);
		}
	}

}
