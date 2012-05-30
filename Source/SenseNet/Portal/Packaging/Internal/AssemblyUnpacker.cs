using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal class AssemblyUnpacker : IUnpacker, IManifest
	{
		private Assembly _reflectedAssembly;

		//============================================================= IManifest Members

		PackageInfo _packageInfo;
		InstallerAssemblyInfo _installerAssembly;
        List<string> _manifestResourceNames;
		List<PrerequisitStep> _prerequisits = new List<PrerequisitStep>();
		List<AssemblyInstallStep> _executables = new List<AssemblyInstallStep>();
		List<ContentTypeInstallStep> _contentTypes = new List<ContentTypeInstallStep>();
		List<ContentViewInstallStep> _contentViews = new List<ContentViewInstallStep>();
		List<PageTemplateInstallStep> _pageTemplates = new List<PageTemplateInstallStep>();
		List<ResourceInstallStep> _resources = new List<ResourceInstallStep>();
		List<FileInstallStep> _files = new List<FileInstallStep>();
		List<ContentInstallStep> _contents = new List<ContentInstallStep>();
        List<DbScriptInstallStep> _dbScripts = new List<DbScriptInstallStep>();

		public PackageInfo PackageInfo { get { return _packageInfo; } }
		public InstallerAssemblyInfo InstallerAssembly { get { return _installerAssembly; } }
		public string InstallerAssemblyFullName { get { return _reflectedAssembly.FullName; } }
		public string InstallerAssemblyName { get { return _reflectedAssembly.GetName().Name; } }
        private List<string> ManifestResourceNames
        {
            get
            {
                if (_manifestResourceNames == null)
                    _manifestResourceNames = new List<string>(_reflectedAssembly.GetManifestResourceNames());
                return _manifestResourceNames;
            }
        }

		public PrerequisitStep[] Prerequisits { get { return _prerequisits.ToArray(); } }
		public AssemblyInstallStep[] Executables { get { return _executables.ToArray(); } }
		public ContentTypeInstallStep[] ContentTypes { get { return _contentTypes.ToArray(); } }
		public ContentViewInstallStep[] ContentViews { get { return _contentViews.ToArray(); } }
		public PageTemplateInstallStep[] PageTemplates { get { return _pageTemplates.ToArray(); } }
		public ResourceInstallStep[] Resources { get { return _resources.ToArray(); } }
		public FileInstallStep[] Files { get { return _files.ToArray(); } }
		public ContentInstallStep[] Contents { get { return _contents.ToArray(); } }
        public DbScriptInstallStep[] DbScripts { get { return _dbScripts.ToArray(); } }

		public Stream GetStream(string name)
		{
            var fullName = GetFullStreamName(name);
            var stream = _reflectedAssembly.GetManifestResourceStream(fullName);
			if(stream == null)
				throw new InvalidManifestException("Resource not found: " + fullName);
			return stream;
		}
        public bool ResourceExists(string streamName)
        {
            return ManifestResourceNames.Contains(GetFullStreamName(streamName));
        }
        private string GetFullStreamName(string streamName)
        {
            var fullName = streamName;
            if (streamName.StartsWith("/"))
			{
				if(_packageInfo.ResourceRoot  == null)
					throw new InvalidManifestException("Expected 'ResourceRoot' parameter is missing in an PackageDescription. " + _packageInfo.ToString());
                fullName = String.Concat(_packageInfo.ResourceRoot, '.', streamName.Substring(1));
			}
            return fullName;
        }

		//============================================================= IUnpacker Members

        public IManifest[] Unpack(string fsPath)
		{
            Logger.LogMessage("Extract: " + fsPath);
			_installerAssembly = new InstallerAssemblyInfo(this, null) {/* ResourceData = stream,*/ OriginalPath = fsPath };
			_reflectedAssembly = AssemblyHandler.ReflectionOnlyLoadInstallerAssembly(fsPath);
			bool installerOnlyAssembly = false;
			bool hasCustomSteps;

			foreach (var attr in _reflectedAssembly.GetAllAttributes(out hasCustomSteps))
			{
				var meta = new Dictionary<string, CustomAttributeNamedArgument>();
				foreach (var item in attr.NamedArguments)
					meta.Add(item.MemberInfo.Name, item);

				var attrName = attr.Constructor.DeclaringType.Name;
				switch (attrName)
				{
					case "InstallerOnlyAssemblyAttribute":
						installerOnlyAssembly = true;
						break;
					case "PackageDescriptionAttribute":
						_packageInfo = new PackageInfo(this, attr);
						break;
					//---------------------------------------------
					case "RequiredPackageAttribute":
						_prerequisits.Add(new RequiredPackageStep(this, attr));
						break;
					case "RequiredSenseNetVersionAttribute":
						_prerequisits.Add(new RequiredSenseNetVersionStep(this, attr));
						break;
					//---------------------------------------------
					case "InstallContentTypeAttribute":
						_contentTypes.Add(new ContentTypeInstallStep(this, attr));
						break;
					case "InstallContentViewAttribute":
						_contentViews.Add(new ContentViewInstallStep(this, attr));
						break;
					case "InstallPageTemplateAttribute":
						_pageTemplates.Add(new PageTemplateInstallStep(this, attr));
						break;
					case "InstallResourceAttribute":
						_resources.Add(new ResourceInstallStep(this, attr));
						break;
					case "InstallFileAttribute":
						_files.Add(new FileInstallStep(this, attr));
						break;
					case "InstallContentAttribute":
						_contents.Add(new ContentInstallStep(this, attr));
						break;
                    case "InstallDatabaseScriptAttribute":
                        _dbScripts.Add(new DbScriptInstallStep(this, attr));
                        break;
					default:
						continue;
				}
			}
			if (!installerOnlyAssembly)
				_executables.Add(new AssemblyInstallStep(this, null) { AssemblyPath = _installerAssembly.OriginalPath });
			if (hasCustomSteps)
				CustomInstallStep.AddCustomStepTypes(AssemblyHandler.GetCustomStepTypes(_installerAssembly));

			return new IManifest[] { this };
		}
	}
}
