using System;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class ResourceInstallStep : ContentInstallStep
	{
		public override string StepShortName { get { return "Resource"; } }
		public ResourceInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			ContentName = GetParameterValue<string>("ContentName");
		}

		public override void Initialize()
		{
			ContentTypeName = "Resource";

			ContainerPath = String.Concat(ContentManager.ResourceFolderPath);
			if (ContentName == null)
				throw new InvalidManifestException("Expected 'ContentName' parameter is missing in an ResourceInstall step. " + ToString());

			RawAttachments = "Binary:" + ResourceName;

			base.Initialize();
		}
	}
}
