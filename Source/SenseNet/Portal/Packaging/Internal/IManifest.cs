namespace SenseNet.Packaging.Internal
{
	internal interface IManifest
	{
		PackageInfo PackageInfo { get; }
		InstallerAssemblyInfo InstallerAssembly { get; }
		string InstallerAssemblyFullName { get; }
		PrerequisitStep[] Prerequisits { get; }
		AssemblyInstallStep[] Executables { get; }
		ContentTypeInstallStep[] ContentTypes { get; }
		ContentViewInstallStep[] ContentViews { get; }
		PageTemplateInstallStep[] PageTemplates { get; }
		ResourceInstallStep[] Resources { get; }
		FileInstallStep[] Files { get; }
		ContentInstallStep[] Contents { get; }
        DbScriptInstallStep[] DbScripts { get; }

		System.IO.Stream GetStream(string streamName);
        bool ResourceExists(string streamName);
	}
}
