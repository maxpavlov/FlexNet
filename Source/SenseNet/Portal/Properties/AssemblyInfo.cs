using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("SenseNet.Tests")]

#if DEBUG
[assembly: AssemblyTitle("Portal (Debug)")]
#else
[assembly: AssemblyTitle("Portal (Release)")]
#endif
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Sense/Net Inc.")]
[assembly: AssemblyCopyright("Copyright © Sense/Net Inc.")]
[assembly: AssemblyProduct("Sense/Net CMS")]
[assembly: AssemblyTrademark("Sense/Net Inc.")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("6.0.8.3195")]
[assembly: AssemblyFileVersion("6.0.8.3195")]
[assembly: ComVisible(false)]
[assembly: Guid("ae1a54ac-6441-4eac-b4be-8148541b6042")]