using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SenseNet.Packaging.Internal
{
	internal class FsManifest : IManifest
	{
		List<PrerequisitStep> _prerequisits = new List<PrerequisitStep>();
		List<AssemblyInstallStep> _executables = new List<AssemblyInstallStep>();
		List<ContentTypeInstallStep> _contentTypes = new List<ContentTypeInstallStep>();
		List<ContentViewInstallStep> _contentViews = new List<ContentViewInstallStep>();
		List<PageTemplateInstallStep> _pageTemplates = new List<PageTemplateInstallStep>();
		List<ResourceInstallStep> _resources = new List<ResourceInstallStep>();
		List<FileInstallStep> _files = new List<FileInstallStep>();
        List<ContentInstallStep> _contents = new List<ContentInstallStep>();
        List<DbScriptInstallStep> _dbScripts = new List<DbScriptInstallStep>();

		public PackageInfo PackageInfo { get; set; }
		public InstallerAssemblyInfo InstallerAssembly { get; set; }
		public string InstallerAssemblyFullName { get; set; }

		public PrerequisitStep[] Prerequisits { get { return _prerequisits.ToArray(); } }
		public AssemblyInstallStep[] Executables { get { return _executables.ToArray(); } }
		public ContentTypeInstallStep[] ContentTypes { get { return _contentTypes.ToArray(); } }
		public ContentViewInstallStep[] ContentViews { get { return _contentViews.ToArray(); } }
		public PageTemplateInstallStep[] PageTemplates { get { return _pageTemplates.ToArray(); } }
		public ResourceInstallStep[] Resources { get { return _resources.ToArray(); } }
		public FileInstallStep[] Files { get { return _files.ToArray(); } }
        public ContentInstallStep[] Contents { get { return _contents.ToArray(); } }
        public DbScriptInstallStep[] DbScripts { get { return _dbScripts.ToArray(); } }

		public Stream GetStream(string streamName)
		{
			var stream = new FileStream(streamName, FileMode.Open);
			return stream;
		}
        public bool ResourceExists(string streamName)
        {
            return File.Exists(streamName);
        }

		internal void AddContent(ContentInstallStep step)
		{
			_contents.Add(step);
		}
		internal void AddExecutable(AssemblyInstallStep step)
		{
			_executables.Add(step);
		}
		internal void AddContentType(ContentTypeInstallStep step)
		{
			_contentTypes.Add(step);
		}
        internal void AddDbScript(DbScriptInstallStep step)
        {
            _dbScripts.Add(step);
        }
	}
}
