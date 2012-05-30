using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal abstract class PrerequisitStep : InstallStep
	{
		public string RequiredVersion { get; set; }
		public Version ParsedVersion { get; private set; }
		public PrerequisitStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData) { }

		public override void Initialize()
		{
			if (RequiredVersion == null)
				return;
			try
			{
				ParsedVersion = new Version(RequiredVersion);
			}
			catch (Exception e)
			{
				throw new InvalidManifestException(String.Concat("The 'Version' parameter is invalid. ", ToString(), ". ", e.Message));
			}
		}

		public override string ToString()
		{
			if (RawData != null)
				return base.ToString();
			return String.Concat(StepShortName, ": Version = ", ParsedVersion == null ? "[null]" : ParsedVersion.ToString());
		}
	}
	internal class RequiredSenseNetVersionStep : PrerequisitStep
	{
		public override string StepShortName { get { return "RequiredSenseNetVersion"; } }
		public RequiredSenseNetVersionStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			RequiredVersion = GetParameterValue<string>("Version");
		}
		public override void Initialize()
		{
			base.Initialize();
			if (ParsedVersion == null)
				throw new InvalidManifestException(String.Concat("Missing 'Version' parameter. ", ToString()));
		}
		public override StepResult Probe()
		{
			throw new NotImplementedException("RequiredSenseNetVersionStep.Probe");
		}
		public override StepResult Install()
		{
			throw new NotImplementedException("RequiredSenseNetVersionStep.Install");
		}
	}
	internal class RequiredPackageStep : PrerequisitStep
	{
		public override string StepShortName { get { return "RequiredPackage"; } }
		public string RequiredPackageName { get; set; }
		public RequiredPackageStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			RequiredPackageName = GetParameterValue<string>("Name");
			RequiredVersion = GetParameterValue<string>("Version");
		}

		public override void Initialize()
		{
			base.Initialize();
			if (RequiredPackageName == null)
				throw new InvalidManifestException(String.Concat("Package 'Name' parameter cannot be null. ", ToString()));
			if (RequiredPackageName.Length == 0)
				throw new InvalidManifestException(String.Concat("Package 'Name' parameter cannot be empty. ", ToString()));
		}
		public override StepResult Probe()
		{
			throw new NotImplementedException("RequiredPackageStep.Probe");
		}
		public override StepResult Install()
		{
			throw new NotImplementedException("RequiredPackageStep.Install");
		}

		public override string ToString()
		{
			if (RawData != null)
				return base.ToString();
			return String.Concat(StepShortName
				, "PackageName = ", RequiredPackageName ?? "[null]"
				, ": Version = ", ParsedVersion == null ? "[null]" : ParsedVersion.ToString());
		}
	}
}
