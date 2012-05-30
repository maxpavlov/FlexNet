using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Packaging
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public class DisableTypeDiscoverAttribute : ManifestAttribute
	{
	}
}
