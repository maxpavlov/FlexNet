using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using SenseNet.Packaging;
using SenseNet.ContentRepository;

namespace SenseNet.Tools.Installer
{
	class Installer
	{
		private static string CR = Environment.NewLine;
		private static string ToolTitle = "SenseNet 6.0 Installer tool\r\n===========================";
		private static string UsageScreen = String.Concat(
			//         1         2         3         4         5         6         7         8
			//12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			CR,
			"Usage:", CR,
			"Install <package> [<plugins>] [-Probe] [-Install]", CR,
			CR,
			"Parameters:", CR,
			"<package>: File contains a package (*.dll or *.exe or *.zip or directory).", CR,
			"<plugins>: Directory contains SenseNet binaries if it is not the current directory.", CR,
			"-Probe:    Package install will be simulated before the install (default).", CR,
			"-Install:  Probing phase will be skipped.", CR
		);
		private static string[] _allowedPackageExtensions = new string[] { ".dll", ".exe", ".zip" };
		private static string _packagePath;
		private static string _pluginsPath;
		private static string _verb;
		private static bool _wait;

		static int Main(string[] args)
		{
			if (!ParseParameters(args))
				return -1;
			if (!CheckParameters())
				return -1;

			if (_wait)
			{
				Console.WriteLine("Running in wait mode - now you can attach to the process with a debugger.");
				Console.WriteLine("Press ENTER to continue.");
				Console.ReadLine();
			}

			if (_verb == null)
				return MainWithoutVerb(_packagePath, _pluginsPath);
			return MainWithVerb(_verb, _packagePath, _pluginsPath);
		}
		private static bool ParseParameters(string[] args)
		{
			if (args.Length < 1 || args.Length > 4)
			{
				PrintParameterError("");
				return false;
			}
			foreach (var arg in args)
			{
				if (arg.StartsWith("-"))
				{
					var verb = arg.Substring(1).ToUpper();
					if (verb == "WAIT")
						_wait = true;
					else
						_verb = verb;
				}
				else if (_packagePath == null)
				{
					_packagePath = arg;
				}
				else
				{
					_pluginsPath = arg;
				}
			}
			if (_pluginsPath == null)
				_pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
			return true;
			//if (args.Length == 2)
			//{
			//    _packagePath = args[0];
			//    _pluginsPath = args[1];
			//    return true;
			//}
			//if (args.Length == 3)
			//{
			//    _verb = args[0];
			//    _packagePath = args[1];
			//    _pluginsPath = args[2];
			//    return true;
			//}
			//PrintParameterError("");
			//return false;
		}
		private static bool CheckParameters()
		{
			if (!Directory.Exists(_packagePath))
			{
				if (!System.IO.File.Exists(_packagePath))
				{
					PrintParameterError("Given package file or directory does not exist");
					return false;
				}
				if (!_allowedPackageExtensions.Contains<string>(Path.GetExtension(_packagePath).ToLower()))
				{
					PrintParameterError("Invalid package: file extension must be one of the following: " + String.Join(", ", _allowedPackageExtensions));
					return false;
				}
			}
			if (!Directory.Exists(_pluginsPath))
			{
				PrintParameterError("Given plugins directory does not exist");
				return false;
			}
			return true;
		}
		private static void PrintParameterError(string message)
		{
			Console.WriteLine(ToolTitle);
			Console.WriteLine(message);
			Console.WriteLine(UsageScreen);
			Console.WriteLine("Aborted.");
		}

		static int MainWithoutVerb(string package, string pluginsPath)
		{
			Console.WriteLine();
			Console.WriteLine(ToolTitle);

			var workerExe = Assembly.GetExecutingAssembly().Location;

			var workerDomain = AppDomain.CreateDomain("ProbeWorkerDomain");
			var result = workerDomain.ExecuteAssembly(
				workerExe, AppDomain.CurrentDomain.Evidence, new string[] { "-Probe", package, pluginsPath });
			AppDomain.Unload(workerDomain);

            Logger.LogMessage("");
            if (result < 0)
			{
                Logger.LogMessage("Aborted");
				Console.Write("[press any key]");
				Console.ReadKey();
				Console.WriteLine();
				return result;
			}

			Console.Write("Install [y/n]? ");
			var key = Console.ReadKey().KeyChar;
			Console.WriteLine();

			if (key != 'y')
				return 0;

			var workerDomain1 = AppDomain.CreateDomain("InstallWorkerDomain1");
			result = workerDomain1.ExecuteAssembly(
				workerExe, AppDomain.CurrentDomain.Evidence, new string[] { "-Install", package, pluginsPath });
			AppDomain.Unload(workerDomain1);

			if (result < 1)
				return result;

			var workerDomain2 = AppDomain.CreateDomain("InstallWorkerDomain2");
			result = workerDomain2.ExecuteAssembly(
				workerExe, AppDomain.CurrentDomain.Evidence, new string[] { "-Install_Phase2", package, pluginsPath });
			AppDomain.Unload(workerDomain2);

            Logger.LogMessage("");
            Logger.LogMessage("Ok");
			Console.Write("[press any key] ");
			Console.ReadKey();
			Console.WriteLine();

			return result;
		}
		static int MainWithVerb(string verb, string package, string pluginsPath)
		{
			switch (verb)
			{
				case "PROBE":
					return Probe(package, pluginsPath);
				case "INSTALL":
					return Install(package, pluginsPath);
				case "INSTALL_PHASE2":
					return InstallPhase2(package, pluginsPath);
				default:
					return -1;
			}
		}

		private static int Probe(string packagePath, string pluginsPath)
		{
			LoadAssemblies(pluginsPath);
			var root = Repository.Root;

            Logger.LogMessage("");
            Logger.LogTitle("Install probe");
			var result = PackageManager.InstallProbe(packagePath, pluginsPath);

			//  0: successful + log
			//  1: need restart + copy dll info
			// -1: error + log
			return result.Successful ? 0 : -1;
		}
		private static int Install(string packagePath, string pluginsPath)
		{
            Logger.LogMessage("");
            Logger.LogTitle("Install");
			var result = PackageManager.Install(packagePath, pluginsPath);

			//  0: successful + log
			//  1: need restart
			// -1: error + log
            if (!result.Successful)
                return -1;
            if (result.NeedRestart)
                return 1;
			return 0;
		}
		private static int InstallPhase2(string packagePath, string pluginsPath)
		{
			LoadAssemblies(pluginsPath);
			var root = Repository.Root;

            Logger.LogMessage("");
            Logger.LogTitle("Install phase 2");
			var result = PackageManager.InstallPhase2(packagePath, pluginsPath);

			//  0: successful + log
			// -1: error + log
			var code = result.NeedRestart ? 1 : result.Successful ? 0 : -1;
			return code;
		}

		//private static void LoadAssemblies()
		//{
		//    LoadAssemblies(AppDomain.CurrentDomain.BaseDirectory);
		//}
		private static void LoadAssemblies(string pluginsPath)
		{
			string[] names = SenseNet.ContentRepository.Storage.TypeHandler.LoadAssembliesFrom(pluginsPath);
		}
	}
}
