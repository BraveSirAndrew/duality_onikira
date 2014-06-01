using System;
using WeifenLuo.WinFormsUI.Docking;

namespace PluginManager.Modules
{
	public partial class PluginManagerView : DockContent
	{
		public event EventHandler<CommandEnteredEventArgs> CommandEntered;

		public PluginManagerView()
		{
			InitializeComponent();

			ConsoleControl.CommandEntered += (sender, args) => OnCommandEntered(args);
		}

		public string RepositoryURI
		{
			get { return RepositoryURITextBox.Text; }
			set { RepositoryURITextBox.Text = value; }
		}

		protected virtual void OnCommandEntered(CommandEnteredEventArgs e)
		{
			var handler = CommandEntered;

			if (handler != null) 
				handler(this, e);
		}

		public void WriteText(string text)
		{
			ConsoleControl.WriteText(text);
		}
	}
}