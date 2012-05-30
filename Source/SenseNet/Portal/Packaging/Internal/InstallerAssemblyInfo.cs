using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class InstallerAssemblyInfo : ManifestItem
	{
		public string OriginalPath { get; set; }
		public InstallerAssemblyInfo(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData) { }

		public override string ToString()
		{
			if (this.RawData != null)
				return base.ToString();
			return "InstallerAssemblyInfo: " + OriginalPath;
		}
	}
}
