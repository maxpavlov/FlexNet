using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal static class Extensions
	{
		internal static CustomAttributeData[] GetAllAttributes(this Assembly assembly, out bool hasCustomSteps)
		{
			hasCustomSteps = false;

			List<CustomAttributeData> result = new List<CustomAttributeData>();
			result.AddRange(CustomAttributeData.GetCustomAttributes(assembly));

			var attrName = typeof(DisableTypeDiscoverAttribute).FullName;
			foreach (var attrInfo in result)
				if (attrInfo.Constructor.DeclaringType.FullName == attrName)
					return result.ToArray();

			AssemblyHandler.ReflectionOnlyPreloadReferences(assembly);
			var customStepTypeList = new List<Type>();
			var customInstallStepTypeName = typeof(CustomInstallStep).FullName;
			foreach (var t in assembly.GetTypes())
			{
				var type = Type.ReflectionOnlyGetType(String.Concat(t.FullName, ",", assembly.FullName), true, false);
				result.AddRange(CustomAttributeData.GetCustomAttributes(type));
				if(type.BaseType.FullName == customInstallStepTypeName)
					hasCustomSteps = true;
			}
			return result.ToArray();
		}
		internal static bool IsInstallerAssembly(this Assembly assembly)
		{
			var attrName = typeof(PackageDescriptionAttribute).FullName;
			foreach (var attrInfo in CustomAttributeData.GetCustomAttributes(assembly))
				if (attrInfo.Constructor.DeclaringType.FullName == attrName)
					return true;
			return false;
		}
		internal static Type[] GetCustomStepTypes(this Assembly assembly)
		{
			var result = new List<Type>();
			foreach (var type in assembly.GetTypes())
				if (type.IsSubclassOf(typeof(CustomInstallStep)))
					result.Add(type);
			return result.ToArray();
		}
	}
}
