using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace SenseNet.Portal.UI.Controls
{
	public abstract class ErrorControl : ViewControlBase
	{
		private bool _debug;

		[PersistenceMode(PersistenceMode.Attribute)]
		public bool Debug
		{
			get { return _debug; }
			set { _debug = value; }
		}

	}
}