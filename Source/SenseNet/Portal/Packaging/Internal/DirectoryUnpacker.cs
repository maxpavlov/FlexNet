using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace SenseNet.Packaging.Internal
{
	internal class DirectoryUnpacker : IUnpacker
	{
		private string _packageDir;
		private string _parentOfRootDir;
		private string _rootDir;
		private string _contentTypeDir;
		private string _checkContentTypeDir;
		private FsManifest _manifest;
		private List<IManifest> _manifests = new List<IManifest>();

		public IManifest[] Unpack(string fsPath)
		{
            Logger.LogMessage("Using Package directory: " + fsPath);

			_packageDir = fsPath;
			_rootDir = Path.Combine(fsPath, "Root");
			_parentOfRootDir = Path.GetDirectoryName(_rootDir);
			_contentTypeDir = Path.Combine(_rootDir, @"System\Schema\ContentTypes");
			_checkContentTypeDir = (_checkContentTypeDir + "/").ToLower();
			_manifest = new FsManifest();

			var descriptionFile = Path.Combine(_packageDir, "Package.Description");
			_manifest.PackageInfo = File.Exists(descriptionFile) ? CretatePackageInfoFromFile(descriptionFile) : CretatePackageInfoFromName(fsPath);

			ProcessBinaries(_packageDir);
			if(Directory.Exists(_rootDir))
				TreeWalker(_rootDir);

			var manifests = new List<IManifest>(_manifests);
			manifests.Add(_manifest);
			return manifests.ToArray();
		}

		private PackageInfo CretatePackageInfoFromFile(string descriptionFile)
		{
			//== Content of Package.manifest file:
			//<?xml version="1.0" encoding="utf-8" ?>
			//<Package>
			//    <Name>Planner Plugin</Name>
			//    <Version>1.1</Version>
			//</Package>

			var xml = new XmlDocument();
			xml.Load(descriptionFile);
			var node = xml.DocumentElement.SelectSingleNode("Name");
			if(node == null)
				throw new InvalidManifestException("Invalid Package.Description file: expected Name element is missing");
			var pkgName = node.InnerText;

			node = xml.DocumentElement.SelectSingleNode("Version");
			if(node == null)
				throw new InvalidManifestException("Invalid Package.Description file: expected Name element is missing");
			var pkgVersion = node.InnerText;

			return new PackageInfo(_manifest, null)
			{
				Name = pkgName,
				Version = pkgVersion
			};
		}
		private PackageInfo CretatePackageInfoFromName(string fsPath)
		{
			var rawName = Path.GetFileNameWithoutExtension(fsPath);
			var rawNameLower = rawName.ToLower();
			var pkgName = rawName;
			var pkgVersion = "0.0";
			var versionStart = rawNameLower.IndexOf("(version");
			var versionEnd = -1;
			if (versionStart >= 0)
			{
				
				versionEnd = rawNameLower.IndexOf(")", versionStart + 8);
				if (versionEnd > versionStart)
				{
					pkgName = rawName.Substring(0, versionStart).Trim();
					pkgVersion = rawName.Substring(versionStart + 8, versionEnd - versionStart - 8).Trim();
				}
			}

			return new PackageInfo(_manifest, null)
			{
				Name = pkgName,
				Version = pkgVersion
			};
		}

        private void ProcessBinaries(string _packageDir)
		{
			var binFiles = new List<string>(Directory.GetFiles(_packageDir));
			var executableFiles = new List<string>(Directory.GetFiles(_packageDir, "*.dll"));
			executableFiles.AddRange(Directory.GetFiles(_packageDir, "*.exe"));
			foreach (var executableFile in executableFiles)
			{
				var asmName = Path.GetFileNameWithoutExtension(executableFile);
				var pdbSourcePath = Path.Combine(_packageDir, asmName + ".pdb");
				var configSourcePath = executableFile + ".config";
				binFiles.Remove(executableFile);
				binFiles.Remove(pdbSourcePath);
				binFiles.Remove(configSourcePath);

				if (AssemblyHandler.IsInstallerAssembly(executableFile))
				{
					var asmUnpacker = new AssemblyUnpacker();
					var asmManifest = asmUnpacker.Unpack(executableFile);
					_manifests.AddRange(asmManifest);
				}
				else
				{
					_manifest.AddExecutable(new AssemblyInstallStep(_manifest, null) { AssemblyPath = executableFile });
				}
			}
			foreach (var binFile in binFiles)
			{
				//_manifest.AddExecutable(new BinaryInstallStep(_manifest, null) { SourcePath = binFile });
			}
		}

		private void TreeWalker(string path)
		{
			var containerPath = TransformFileToContent(path);

			var dirs = Directory.GetDirectories(path);
			var files = new List<string>(Directory.GetFiles(path));
			var contentFiles = new List<string>(Directory.GetFiles(path, "*.content"));
			if (containerPath.ToLower() == "/root/system/schema/contenttypes")
			{
				foreach(var file in files)
					_manifest.AddContentType(new ContentTypeInstallStep(_manifest, null) { ResourceName = file });
				return;
			}

			foreach (string contentFile in contentFiles)
			{
				files.Remove(contentFile);
				var contentInfo = new ContentInstallStep(_manifest, contentFile, containerPath);
				_manifest.AddContent(contentInfo);
				contentInfo.Initialize();

				foreach (var attachment in contentInfo.Content.Attachments)
				    files.Remove(attachment.FileName);
			}
			while (files.Count > 0)
			{
				var filePath = files[0];
				var fileInfo = new FileInstallStep(_manifest, filePath, containerPath);
				_manifest.AddContent(fileInfo);
				files.RemoveAt(0);
			}

			foreach (string subPath in dirs)
				TreeWalker(subPath);
		}

		private string TransformFileToContent(string path)
		{
			// C:\Dev\PackageFramework\ZipHandler\work\Import_Local_Data\Root\Planning\ECMS.Content
			//                                                          /Root/Planning/ECMS.Content
			return path.Remove(0, _parentOfRootDir.Length).Replace("\\", "/");
		}
		private bool IsInContentTypeFolder(string path)
		{
			return path.ToLower().StartsWith(_checkContentTypeDir);
		}

	}
}
