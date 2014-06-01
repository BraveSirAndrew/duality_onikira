using System;

namespace PluginManager.Modules
{
	public class CommandEnteredEventArgs : EventArgs
	{
		public CommandEnteredEventArgs(string command)
		{
			Command = command;
		}

		public string Command { get; private set; }
	}
}