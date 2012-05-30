using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal abstract class ManifestItem
	{
		private Dictionary<string, CustomAttributeNamedArgument> _rawArgs;
		public CustomAttributeData RawData { get; private set; }

		public IManifest Manifest { get; private set; }

		public ManifestItem(IManifest manifest, CustomAttributeData rawData)
		{
			Manifest = manifest;
			RawData = rawData;

			if (rawData == null)
				return;

			_rawArgs = new Dictionary<string, CustomAttributeNamedArgument>();
			foreach (var item in rawData.NamedArguments)
				_rawArgs.Add(item.MemberInfo.Name, item);
		}

		protected T GetParameterValue<T>(string name)
		{
			if (_rawArgs == null)
				return (T)default(T);
			CustomAttributeNamedArgument value;
			if (_rawArgs.TryGetValue(name, out value))
				return (T)value.TypedValue.Value;
			return default(T);
		}
		protected Type GetParameterType(string name)
		{
			CustomAttributeNamedArgument value;
			if (_rawArgs.TryGetValue(name, out value))
				return value.TypedValue.ArgumentType;
			return null;
		}

		public override string ToString()
		{
			if (this.RawData != null)
				return ManifestAttribute.ParametersToString(RawData);
			return base.ToString();
		}
	}
}
