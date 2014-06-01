using System.Collections.Generic;

namespace PluginManager.Modules
{
	internal class CommandHistory
	{
		private int _currentPosition;
		private string _lastCommand;
		private List<string> _commandHistory = new List<string>();

		internal void Add(string command)
		{
			if (command == _lastCommand) 
				return;

			_commandHistory.Add(command);
			_lastCommand = command;
			_currentPosition = _commandHistory.Count;
		}

		internal bool DoesPreviousCommandExist()
		{
			return _currentPosition > 0;
		}

		internal bool DoesNextCommandExist()
		{
			return _currentPosition < _commandHistory.Count - 1;
		}

		internal string GetPreviousCommand()
		{
			_lastCommand = (string)_commandHistory[--_currentPosition];
			return _lastCommand;
		}

		internal string GetNextCommand()
		{
			_lastCommand = (string)_commandHistory[++_currentPosition];
			return LastCommand;
		}

		internal string LastCommand
		{
			get { return _lastCommand; }
		}

		internal string[] GetCommandHistory()
		{
			return _commandHistory.ToArray();
		}
	}
}
