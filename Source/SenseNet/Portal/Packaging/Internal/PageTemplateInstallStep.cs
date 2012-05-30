using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class PageTemplateInstallStep : ContentInstallStep
	{
		public override string StepShortName { get { return "PageTemplate"; } }
		public PageTemplateInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			ContentName = GetParameterValue<string>("ContentName");
		}

		public override void Initialize()
		{
			ContentTypeName = "PageTemplate";

			ContainerPath = String.Concat(ContentManager.PageTemplatesFolderPath);
			if (ContentName == null)
				throw new InvalidManifestException("Expected 'ContentName' parameter is missing in an InstallPageTemplate step. " + ToString());

			RawAttachments = "Binary:" + ResourceName;

			base.Initialize();
		}
	}
}
