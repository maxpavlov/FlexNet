using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.UI
{
	internal class PageCatalog
	{
		private string _name;
		public string Name
		{
			get { return _name; }
		}

		public PageCatalog(string name)
		{
			_name = name;
		}
	}
}