using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class PackageInfo : ManifestItem
	{
		public string Name { get; set; }
		public string ResourceRoot { get; set; }
		public string Version { get; set; }
		public PackageInfo(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			Name = GetParameterValue<string>("Name");
			ResourceRoot = GetParameterValue<string>("ResourceRoot");
			Version = GetParameterValue<string>("Version");
		}
	}
}
