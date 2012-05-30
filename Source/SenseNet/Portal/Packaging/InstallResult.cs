using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Packaging
{
	public class InstallResult
	{
        public bool Successful { get; internal set; }
		public bool NeedRestart { get; internal set; }

        internal void Combine(InstallResult other)
		{
			Successful &= other.Successful;
			NeedRestart |= other.NeedRestart;
        }
    }
}
