using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal abstract class ItemInstallStep : InstallStep
	{
		public string ResourceName { get; set; }

		public ItemInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			ResourceName = GetParameterValue<string>("ResourcePath");
		}

		public Stream GetResourceStream()
		{
			return GetResourceStream(ResourceName);
		}
		public Stream GetResourceStream(string resName)
		{
			return Manifest.GetStream(resName);
		}

		public string GetResourceStreamString()
		{
			using (var stream = GetResourceStream())
			{
				StreamReader sr = new StreamReader(stream);
				stream.Position = 0;
				return sr.ReadToEnd();
			}
		}
		public override void Initialize()
		{
			if (ResourceName == null)
				throw new InvalidManifestException("Expected 'ResourceName' parameter is missing. " + ToString());
            if(!Manifest.ResourceExists(ResourceName))
                throw new InvalidManifestException("Resource not found: " + ResourceName);
        }

		public override string ToString()
		{
			if (RawData != null)
				return base.ToString();
			return String.Concat("Step type = ", StepShortName, ", Resource = ", ResourceName);
		}
	}
}
