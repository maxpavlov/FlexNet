using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.UI
{
	internal class PageEdit
	{
		private string _name;
		public string Name
		{
			get { return _name; }
		}

		public PageEdit(string name)
		{
			_name = name;
		}
	}
}