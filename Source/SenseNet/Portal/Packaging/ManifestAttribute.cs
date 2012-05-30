using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Packaging
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
	public abstract class ManifestAttribute : Attribute
	{
		internal static string ParametersToString(System.Reflection.CustomAttributeData attr)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[").Append(attr.Constructor.ReflectedType.FullName);
			if (attr.NamedArguments.Count > 0)
			{
				sb.Append("(");
				bool first = true;
				foreach (var x in attr.NamedArguments)
				{
					if (first) first = false; else sb.Append(", ");
					sb.Append(x.MemberInfo.Name).Append(" = ").Append(x.TypedValue.Value);
				}
				sb.Append(")");
			}
			sb.Append("]");
			return sb.ToString();
		}
	}
}
