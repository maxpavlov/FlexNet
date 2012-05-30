using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SenseNet.Packaging.Internal
{
	internal class BatchContentTypeInstallStep : ItemInstallStep
	{
		private List<string> _contentTypes = new List<string>();

		public List<ContentTypeInstallStep> ChildSteps { get; set; }
		public List<string> ContentTypeNamesInPackage { get; private set; }

        public override string StepShortName { get { return "ContentTypeTree"; } }

		public BatchContentTypeInstallStep(ContentTypeInstallStep[] typeSteps) : base(typeSteps[0].Manifest, typeSteps[0].RawData)
		{
			ChildSteps = new List<ContentTypeInstallStep>(typeSteps);
		}

		public override void Initialize()
		{
			//-- do not call base.Initialize()

			ContentTypeNamesInPackage = new List<string>();
			foreach (var step in ChildSteps)
			{
				step.Initialize();
				step.BatchStep = this;
				if (ContentTypeNamesInPackage.Contains(step.ContentTypeName))
					throw new InvalidManifestException("Duplicated ContentType: " + step.ContentTypeName);
				ContentTypeNamesInPackage.Add(step.ContentTypeName);
			}
		}
		public override StepResult Probe()
		{
			StepResultKind kind = StepResultKind.Successful;
			foreach (var step in ChildSteps)
			{
				var result = step.Probe();
				if (result.Kind == StepResultKind.Error)
				{
					kind = StepResultKind.Error;
					break;
				}
				if (result.Kind == StepResultKind.Warning)
					kind = StepResultKind.Warning;
			}
			return new StepResult { Kind = kind };
		}
		public override StepResult Install()
		{
			StepResultKind kind = StepResultKind.Successful;
			foreach (var step in ChildSteps)
			{
				var result = step.Install();
				if (result.Kind == StepResultKind.Error)
				{
					kind = StepResultKind.Error;
					break;
				}
				if (result.Kind == StepResultKind.Warning)
					kind = StepResultKind.Warning;
			}
			ContentManager.InstallContentTypes(_contentTypes);
			return new StepResult { Kind = kind };
		}

		internal void AddContentType(string ctd)
		{
			_contentTypes.Add(ctd);
		}
	}
}
