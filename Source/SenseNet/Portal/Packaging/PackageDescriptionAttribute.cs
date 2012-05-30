using System;

namespace SenseNet.Packaging
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class PackageDescriptionAttribute : ManifestAttribute
    {
        public string Name { get; set; }
		public string ResourceRoot { get; set; }
		public string Version { get; set; }
	}
}