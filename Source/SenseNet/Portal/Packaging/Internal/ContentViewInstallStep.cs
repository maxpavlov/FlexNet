using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class ContentViewInstallStep : ContentInstallStep
	{
		public override string StepShortName { get { return "ContentView"; } }
		public ContentViewMode ViewMode { get; set; }
		public string ParentFolderName { get; set; }
		public ContentViewInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			ContentName = GetParameterValue<string>("ContentName");
			ParentFolderName = GetParameterValue<string>("ContentTypeName");
			ViewMode = GetParameterValue<ContentViewMode>("ViewMode");
		}

		public override void Initialize()
		{
			ContentTypeName = "File";

			if (ViewMode == ContentViewMode.Custom)
			{
				if (ContentName == null)
					throw new InvalidManifestException(
						"If the ViewMode is Custom then the 'ContentName' parameter is expected in an InstallContentView step. " + ToString());

				if (ParentFolderName == null)
				{
					if (!ContentName.StartsWith("/Root/"))
						throw new InvalidManifestException(
							"If the ViewMode is Custom and ContentTypeName is not specified in an InstallContentView step then 'ContentName' must be an absolute RepositoryPath. " + ToString());
					ContainerPath = ContentManager.Path.GetParentPath(ContentName);
					ContentName = ContentManager.Path.GetFileName(ContentName);
				}
				else
				{
					if (ContentName.StartsWith("/") || ContentName.StartsWith("Root"))
						throw new InvalidManifestException(
							"If ContentTypeName is specified in an InstallContentView step then ContentName must be a relative RepositoryPath. " + ToString());
					ContainerPath = ContentManager.Path.Combine(ContentManager.ContentViewsFolderPath, ParentFolderName);
				}
			}
			else
			{
				if (ParentFolderName == null)
				{
					throw new InvalidManifestException(
						"If the ViewMode is not Custom in an InstallContentView step then ContentTypeName parameter is required. " + ToString());
				}
				if (ContentName != null)
				{
					throw new InvalidManifestException(
						"If the ViewMode is not Custom in an InstallContentView step then ContentName parameter is forbidden. " + ToString());
				}
				ContainerPath = ContentManager.Path.Combine(ContentManager.ContentViewsFolderPath, ParentFolderName);
				ContentName = String.Concat(ViewMode, ".ascx");
			}

			RawAttachments = "Binary:" + ResourceName;

			base.Initialize();
		}
	}

}
