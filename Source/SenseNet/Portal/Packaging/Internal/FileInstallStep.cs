using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class FileInstallStep : ContentInstallStep
	{
		public override string StepShortName { get { return "File"; } }
		public FileInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			//[assembly: InstallFile(RepositoryPath = "/Root/Test/asdf.css", ResourcePath = "/res.asdf.css")]
			var repositoryPath = GetParameterValue<string>("RepositoryPath");
			var contentName = ContentManager.Path.GetFileName(repositoryPath);
			var containerPath = ContentManager.Path.GetParentPath(repositoryPath);

			ContainerPath = containerPath;
			ContentName = contentName;
			RawAttachments = "Binary:" + ResourceName;
		}
		public FileInstallStep(IManifest manifest, string filePath, string repositoryContainerPath) : base(manifest, null)
		{
			ResourceName = filePath;
			ContainerPath = repositoryContainerPath;
		}


		public override void Initialize()
		{
			ContentTypeName = "File";
			base.Initialize();
		}
	}
}
