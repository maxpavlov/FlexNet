using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal abstract class InstallStep : ManifestItem
	{
		public abstract string StepShortName { get; }

		public InstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
		}

		public abstract void Initialize();
		public abstract StepResult Probe();
		public abstract StepResult Install();

	}

}
