using System.Drawing;
using System.Windows.Forms;

namespace PluginManager.Modules
{
	public partial class ConsoleControl : UserControl
	{
		public event EventCommandEntered CommandEntered;

		public ConsoleControl()
		{
			InitializeComponent();
		}

		public Color ShellTextForeColor
		{
			get { return ConsoleTextBox != null ? ConsoleTextBox.ForeColor : Color.FromArgb(241, 241, 241); }
			set
			{
				if (ConsoleTextBox != null)
					ConsoleTextBox.ForeColor = value;
			}
		}

		public Color ShellTextBackColor
		{
			get { return ConsoleTextBox != null ? ConsoleTextBox.BackColor : Color.FromArgb(37, 37, 37); }
			set
			{
				if (ConsoleTextBox != null)
					ConsoleTextBox.BackColor = value;
			}
		}

		public Font ShellTextFont
		{
			get { return ConsoleTextBox != null ? ConsoleTextBox.Font : new Font("Consolas", 10); }
			set
			{
				if (ConsoleTextBox != null)
					ConsoleTextBox.Font = value;
			}
		}

		public string Prompt
		{
			get { return ConsoleTextBox.Prompt; }
			set { ConsoleTextBox.Prompt = value; }
		}

		public void Clear()
		{
			ConsoleTextBox.Clear();
		}

		public void WriteText(string text)
		{
			ConsoleTextBox.WriteText(text);
		}

		public string[] GetCommandHistory()
		{
			return ConsoleTextBox.GetCommandHistory();
		}

		internal void FireCommandEntered(string command)
		{
			OnCommandEntered(command);
		}

		protected virtual void OnCommandEntered(string command)
		{
			if (CommandEntered != null)
				CommandEntered(command, new CommandEnteredEventArgs(command));
		}
	}
}
