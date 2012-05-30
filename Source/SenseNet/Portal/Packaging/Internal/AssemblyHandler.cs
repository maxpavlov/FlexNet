using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace SenseNet.Packaging.Internal
{
	internal static class AssemblyHandler
	{
		internal static Assembly ReflectionOnlyLoadInstallerAssembly(string fsPath)
		{
			var asmName = AssemblyName.GetAssemblyName(fsPath).FullName;
			foreach (var asm in AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
				if (asm.FullName == asmName)
					return asm;
			var thisAsm = Assembly.ReflectionOnlyLoad(typeof(AssemblyHandler).Assembly.FullName);
			return Assembly.ReflectionOnlyLoadFrom(fsPath);
		}

		internal static void ReflectionOnlyPreloadReferences(Assembly assembly)
		{
			var loadedNames = new List<string>();
			ReflectionOnlyPreloadReferences(assembly, loadedNames);
		}
		private static void ReflectionOnlyPreloadReferences(Assembly assembly, List<string> loadedAsmNames)
		{
			foreach (var asmName in assembly.GetReferencedAssemblies())
			{
				if (!loadedAsmNames.Contains(asmName.FullName))
				{
					if (asmName.FullName == "Microsoft.Web.Preview, Version=1.3.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")
						continue;
					if (asmName.FullName == "ICSharpCode.SharpZipLib, Version=0.84.0.0, Culture=neutral, PublicKeyToken=1b03e6acf1164f73")
						continue;
					if (asmName.FullName == "System.Web.Extensions, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")
						continue;
					else
					{
						var refAsm = Assembly.ReflectionOnlyLoad(asmName.FullName);
						loadedAsmNames.Add(asmName.FullName);
						ReflectionOnlyPreloadReferences(refAsm, loadedAsmNames);
					}
				}
			}
		}

		internal static Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
		{
			var pluginsPath = PackageManager.PluginsPath;
			var pluginFilePaths = Directory.GetFiles(pluginsPath, "*.dll");
			var requestedAsmName = new AssemblyName(args.Name);
			foreach (var pluginFilePath in pluginFilePaths)
			{
				var pluginAsmName = AssemblyName.GetAssemblyName(pluginFilePath);
				if (pluginAsmName.FullName == requestedAsmName.FullName)
					return ReflectionOnlyLoadAssembly(pluginFilePath);
			}
			return null;
		}
		private static Assembly ReflectionOnlyLoadAssembly(string pluginFilePath)
		{
			var asm = Assembly.ReflectionOnlyLoadFrom(pluginFilePath);
			return asm;
		}

		internal static bool IsInstallerAssembly(string fsPath)
		{
			var asm = ReflectionOnlyLoadInstallerAssembly(fsPath);
			return asm.IsInstallerAssembly();
		}

		internal static Type[] GetCustomStepTypes(InstallerAssemblyInfo _installerAssembly)
		{
			var asm = Assembly.LoadFrom(_installerAssembly.OriginalPath);
			var types = asm.GetCustomStepTypes();
			return types;
		}
	}
}
