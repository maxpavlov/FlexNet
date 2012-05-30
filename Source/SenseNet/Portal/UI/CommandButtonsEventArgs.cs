using System;
using System.Collections.Generic;
using System.Text;
using SenseNet.Portal.UI.Controls;
using System.ComponentModel;

namespace SenseNet.Portal.UI
{
    public class CommandButtonsEventArgs : CancelEventArgs
	{
		public CommandButtonType ButtonType { get; private set; }
		public ContentView ContentView { get; private set; }
        public string CustomCommand { get; private set; }

        public CommandButtonsEventArgs(CommandButtonType buttonType, ContentView view, string customCommand)
		{
            ButtonType = buttonType;
            ContentView = view;
            CustomCommand = customCommand;
		}
	}
}