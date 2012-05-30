using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace SenseNet.Packaging.Internal
{
	internal class ContentTypeInstallStep : ItemInstallStep
	{
		public override string StepShortName { get { return "ContentType"; } }
		public string ContentTypeName { get; private set; }
		public string ContentTypeParentName { get; private set; }
		public BatchContentTypeInstallStep BatchStep { get; set; }

		public ContentTypeInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
		}

		public override void Initialize()
		{
			base.Initialize();

			XmlDocument xml = new XmlDocument();
			xml.Load(GetResourceStream());
			//using (var reader = XmlReader.Create(GetResourceStream()))
			//    xml.Load(reader);

			ContentTypeName = xml.DocumentElement.Attributes["name"].Value;
			ContentTypeParentName = null;
			var attr = xml.DocumentElement.Attributes["parentType"];
			if (attr != null)
				ContentTypeParentName = attr.Value;
		}
		public override StepResult Probe()
		{
            LogStep(true);
            return Check(true) ?? CreateResult(true);
		}
		public override StepResult Install()
		{
			var checkError = Check(false);
			if (checkError != null)
				return checkError;
            LogStep(false);
			using (var reader = new StreamReader(GetResourceStream(), true))
			{
				var ctd = reader.ReadToEnd();
				BatchStep.AddContentType(ctd);
			}
			return CreateResult(false);
		}
        private StepResult Check(bool probe)
        {
            if (ContentTypeParentName != null)
            {
                if (ContentManager.GetContentType(ContentTypeParentName) == null)
                {
                    if (!BatchStep.ContentTypeNamesInPackage.Contains(ContentTypeParentName))
                    {
                        Logger.LogMessage("Cannot install ContentType: {0}. ParentType does not exist: {1}", ContentTypeName, ContentTypeParentName);
                        return new StepResult { Kind = StepResultKind.Error };
                    }
                }
            }
            return null;
        }
        private void LogStep(bool probe)
        {
            var overwrite = CheckExistence();
            var targetName = String.Concat(ContentTypeParentName == null ? "" : String.Concat(ContentTypeParentName, "/"), ContentTypeName);
            Logger.LogInstallStep(InstallStepCategory.ContentType, StepShortName, ResourceName, targetName, probe, overwrite, false, null);
        }
		private StepResult CreateResult(bool probe)
		{
			return new StepResult { Kind = StepResultKind.Successful };
		}
		private bool CheckExistence()
		{
			return ContentManager.GetContentType(ContentTypeName) != null;
		}
	}

}
