using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;

namespace SenseNet.Packaging.Internal
{
	internal class ContentInstallStep : ItemInstallStep, IComparable<ContentInstallStep>
	{
		private string _stepShortName = "Content";

		public Content Content { get; private set; }

		public override string StepShortName { get { return _stepShortName; } } 
		public virtual string ContentName { get; set; }
		public virtual string ContentTypeName { get; set; }
		public virtual string ContainerPath { get; set; }
		public virtual string ContentPath { get { return ContentManager.Path.Combine(ContainerPath, ContentName); } }
		public virtual string RawAttachments { get; set; }

		public static bool IsMetaFile(string resName)
		{
			return resName.ToLower().EndsWith(".content");
		}
		protected bool FromAttribute { get { return this.RawData != null; } }

		public ContentInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData)
		{
			//[assembly: InstallContent(RepositoryContainerPath = "/Root/Test", ResourcePath = "/res.asdf.css.Content", Attachments = "Binary:/res.asdf.css")]
			ContainerPath = GetParameterValue<string>("RepositoryContainerPath");
			RawAttachments = GetParameterValue<string>("Attachments");
		}
		public ContentInstallStep(IManifest manifest, string metaFilePath, string repositoryContainerPath) : base(manifest, null)
		{
			ResourceName = metaFilePath;
			ContainerPath = repositoryContainerPath;
			//ParseMetaFile();
		}

		bool _initialized = false;
		public override void Initialize()
		{
			if (_initialized)
				return;

			base.Initialize();

			XmlDocument metaFile = null;
			if (IsMetaFile(ResourceName))
				metaFile = ParseMetaFile();

			if (ContentTypeName == null)
				throw new InvalidManifestException("Expected 'ContentTypeName' parameter is missing in an InstallContent step. " + ToString());
			if (ContentName == null)
				throw new InvalidManifestException("Expected 'ContentName' parameter is missing in an InstallContent step. " + ToString());
			if (ContainerPath == null)
				throw new InvalidManifestException("Expected 'RepositoryPath' parameter is missing in an InstallContent step. " + ToString());

			var attachments = ParseAttachments(metaFile);

			if (metaFile == null)
				metaFile = CreateMetaFile();
			RemoveAttachmentsFromMetaData(metaFile);

			this.Content = new Content
			{
				ContentType = ContentTypeName,
				Name = ContentName,
				Path = ContentPath,
				Attachments = attachments,
				Data = metaFile.OuterXml
			};

			_stepShortName = ContentTypeName;
			_initialized = true;
		}
		private Attachment[] ParseAttachments(XmlDocument metaFile)
		{
			if (RawAttachments != null)
				return ParseRawAttachments(RawAttachments);
			if (metaFile != null)
				return ParseMetaAttachments(metaFile);
			return new Attachment[0];
		}
		private Attachment[] ParseRawAttachments(string rawAttachments)
		{
			var segments = rawAttachments.Split(',');
			var attachments = new List<Attachment>();
			foreach (var segment in segments)
			{
				var c = segment.Split(':');
				if(c.Length != 2)
					throw new InvalidManifestException("Invalid 'Attachments' parameter. Expected format: {field}:{resourcename}[,{field}:{resourcename}]*" + ToString());
				var fieldName = c[0].Trim();
				var resName = c[1].Trim();
				attachments.Add(new Attachment
				{
					FieldName = fieldName,
					FileName = resName,
					Manifest = this.Manifest
				});
			}
			return attachments.ToArray();
		}
		private Attachment[] ParseMetaAttachments(XmlDocument metaFile)
		{
			var attachments = new List<Attachment>();
			var attachmentNodes = metaFile.SelectNodes("//*[@attachment]");
			var dir = Path.GetDirectoryName(ResourceName);
			for (int i = 0; i < attachmentNodes.Count; i++)
			{
				attachments.Add(new Attachment
				{
					FieldName = attachmentNodes[i].LocalName,
					FileName = Path.Combine(dir, attachmentNodes[i].Attributes["attachment"].Value),
					Manifest = this.Manifest
				});
				attachmentNodes[i].ParentNode.RemoveChild(attachmentNodes[i]);
			}
			return attachments.ToArray();
		}

		public override StepResult Probe()
		{
			PreviousItemState prevState = PackageManager.GetPreviousContentState(this);
            LogStep(prevState, true);
			return CreateResult();
		}
		public override StepResult Install()
		{
			PreviousItemState prevState = PackageManager.GetPreviousContentState(this);
            LogStep(prevState, false);
            var result = ContentManager.InstallContent(this.Content);
			result.NeedSetReferencePhase = this.Content.HasReference;
			return result;
		}
		public StepResult SetReferences()
		{
            Logger.LogMessage(String.Concat("UPDATE REFERENCES ", StepShortName, ": ", ContentPath));
            return ContentManager.UpdateReferences(this.Content);
		}

		private XmlDocument ParseMetaFile()
		{
			XmlDocument xml = new XmlDocument();
			xml.Load(GetResourceStream());

			XmlNode node;

			node = xml.DocumentElement.SelectSingleNode("ContentType");
			if (node != null)
				ContentTypeName = node.InnerText;
			node = xml.DocumentElement.SelectSingleNode("ContentName");
			if (node != null)
				ContentName = node.InnerText;

			return xml;
		}
		private void RemoveAttachmentsFromMetaData(XmlDocument metaData)
		{
			var attachmentNodes = metaData.SelectNodes("//*[@attachment]");
			for (int i = 0; i < attachmentNodes.Count; i++)
				attachmentNodes[i].ParentNode.RemoveChild(attachmentNodes[i]);
		}
		private XmlDocument CreateMetaFile()
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml(String.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<ContentMetaData>
  <ContentType>{0}</ContentType>
  <ContentName>{1}</ContentName>
  <Fields />
</ContentMetaData>", ContentTypeName, ContentName));
			return xml;
		}

        protected virtual void LogStep(PreviousItemState prevState, bool probe)
        {
            bool overwrite, userModified;
            switch (prevState)
            {
                case PreviousItemState.Installed:
                    overwrite = true;
                    userModified = false;
                    break;
                case PreviousItemState.UserCreated:
                case PreviousItemState.UserModified:
                    overwrite = true;
                    userModified = true;
                    break;
                case PreviousItemState.NotInstalled:
                default:
                    overwrite = false;
                    userModified = false;
                    break;
            }
            var targetPath = ContentManager.Path.Combine(ContainerPath, ContentName);
            Logger.LogInstallStep(InstallStepCategory.Content, StepShortName, ResourceName, targetPath, probe, overwrite, userModified, null);
        }
		protected virtual StepResult CreateResult()
		{
			return new StepResult { Kind = StepResultKind.Successful };
		}

		//================================================= IComparable<ContentInstallStep> Members

		public int CompareTo(ContentInstallStep other)
		{
			return this.ContentPath.CompareTo(other.ContentPath);
		}
	}
}
