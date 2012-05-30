using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.UI
{
	internal class PageZone
	{
		private string _name;
		public string Name
		{
			get { return _name; }
		}

		private string _innerText;
		public string InnerText
		{
			get { return _innerText; }
		}

		private string _attrListText;
		public string AttrListText
		{
			get { return _attrListText; }
		}

		public PageZone(string name, string innerText, string attrListText)
		{
			_name = name;
			_innerText = innerText;
			_attrListText = attrListText;
		}
	}
}