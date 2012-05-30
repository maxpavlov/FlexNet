using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SenseNet.Packaging.Internal
{
	internal interface IUnpacker
	{
		IManifest[] Unpack(string fsPath);
	}

}
