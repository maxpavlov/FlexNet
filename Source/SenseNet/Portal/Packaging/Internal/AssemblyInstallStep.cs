using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace SenseNet.Packaging.Internal
{
	internal class AssemblyInstallStep : InstallStep
	{
		public override string StepShortName { get { return "Assembly"; } }
		public AssemblyInstallStep(IManifest manifest, CustomAttributeData rawData) : base(manifest, rawData) { }
		public string AssemblyPath { get; set; }

		private AssemblyName _assemblyName;

		public override void Initialize()
		{
			_assemblyName = AssemblyName.GetAssemblyName(AssemblyPath);
		}
		public override StepResult Probe()
		{
			var prevState = PackageManager.GetPreviousAssemblyState(AssemblyPath, _assemblyName);
            Logger.LogInstallStep(InstallStepCategory.Assembly, StepShortName, _assemblyName.FullName, _assemblyName.FullName, true, prevState != PreviousItemState.NotInstalled, false, null);
            return GetResult(false);
		}
		public override StepResult Install()
		{
			var prevState = PackageManager.GetPreviousAssemblyState(AssemblyPath, _assemblyName);
            Logger.LogInstallStep(InstallStepCategory.Assembly, StepShortName, _assemblyName.FullName, _assemblyName.FullName, false, prevState != PreviousItemState.NotInstalled, false, null);

			var asmFullPath = this.AssemblyPath; // Manifest.InstallerAssembly.OriginalPath;
			var directory = Path.GetDirectoryName(asmFullPath);
			var asmName = Path.GetFileNameWithoutExtension(asmFullPath);

			var pdbSourcePath = Path.Combine(directory, asmName + ".pdb");
			var configSourcePath = asmFullPath + ".config";

			var needRestart = true;
			CopyFile(asmFullPath);
			if (File.Exists(pdbSourcePath))
				CopyFile(pdbSourcePath);
			if (File.Exists(configSourcePath))
				CopyFile(configSourcePath);

			return GetResult(needRestart);
		}
		private void CopyFile(string sourcePath)
		{
			string targetPath = Path.Combine(PackageManager.PluginsPath, Path.GetFileName(sourcePath));
			File.Copy(sourcePath, targetPath, true);
		}

		private StepResult GetResult(bool needRestart)
		{
			return new StepResult { Kind = StepResultKind.Successful, NeedRestart = needRestart };
		}

		public override string ToString()
		{
			if(RawData != null)
				return base.ToString();
			return String.Concat(StepShortName, ": ", AssemblyPath); 
		}
	}

}
