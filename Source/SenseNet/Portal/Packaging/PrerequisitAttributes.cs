using System;

namespace SenseNet.Packaging
{
	public abstract class RequirementAttribute : ManifestAttribute
	{
	}
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class RequiredSenseNetVersionAttribute : RequirementAttribute
	{
		public string Version { get; set; }
	}
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
    public class RequiredPackageAttribute : RequirementAttribute
	{
		public string Name { get; set; }
		public string Version { get; set; }
	}
}
