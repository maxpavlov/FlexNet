using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Portal.UI
{
	public class UserActionEventArgs : EventArgs
	{
		public string ActionName { get; private set; }
		public ContentView ContentView { get; private set; }
		public string EventName { get; private set; }

		public UserActionEventArgs(string actionName, string eventName, ContentView view)
		{
			ActionName = actionName;
			ContentView = view;
			EventName = eventName;
		}
	}
}