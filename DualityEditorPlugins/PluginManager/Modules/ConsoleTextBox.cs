using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PluginManager.Modules
{
	internal class ConsoleTextBox : TextBox
	{
		private readonly CommandHistory _commandHistory = new CommandHistory();
		private readonly Container _components = null;

		private string _prompt = ">>>";

		public ConsoleTextBox()
		{
			InitializeComponent();
			PrintPrompt();
		}

		public string Prompt
		{
			get { return _prompt; }
			set { SetPromptText(value); }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_components != null)
				{
					_components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		// Overridden to protect against deletion of contents
		// cutting the text and deleting it from the context menu
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case 0x0302: //WM_PASTE
				case 0x0300: //WM_CUT
				case 0x000C: //WM_SETTEXT
					if (!IsCaretAtWritablePosition())
						MoveCaretToEndOfText();
					break;
				case 0x0303: //WM_CLEAR
					return;
			}

			base.WndProc(ref m);
		}

		private void PrintPrompt()
		{
			var currentText = Text;
			if (currentText.Length != 0 && currentText[currentText.Length - 1] != '\n')
				PrintLine();

			AddText(_prompt);
		}

		private void PrintLine()
		{
			AddText(Environment.NewLine);
		}

		// Handle Backspace and Enter keys in KeyPress. A bug in .NET 1.1
		// prevents the e.Handled = true from having the desired effect in KeyDown
		private void shellTextBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Handle backspace
			if (e.KeyChar == (char) 8 && IsCaretJustBeforePrompt())
			{
				e.Handled = true;
				return;
			}

			if (!IsTerminatorKey(e.KeyChar)) 
				return;

			e.Handled = true;
			var currentCommand = GetTextAtPrompt();
			if (currentCommand.Length != 0)
			{
				PrintLine();
				((ConsoleControl) Parent).FireCommandEntered(currentCommand);
				_commandHistory.Add(currentCommand);
			}
			PrintPrompt();
		}

		private void ShellControl_KeyDown(object sender, KeyEventArgs e)
		{
			// If the caret is anywhere else, set it back when a key is pressed.
			if (!IsCaretAtWritablePosition() && !(e.Control || IsTerminatorKey(e.KeyCode)))
			{
				MoveCaretToEndOfText();
			}

			// Prevent caret from moving before the prompt
			if (e.KeyCode == Keys.Left && IsCaretJustBeforePrompt())
			{
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (_commandHistory.DoesNextCommandExist())
				{
					ReplaceTextAtPrompt(_commandHistory.GetNextCommand());
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (_commandHistory.DoesPreviousCommandExist())
				{
					ReplaceTextAtPrompt(_commandHistory.GetPreviousCommand());
				}
				e.Handled = true;
			}
			else if (e.KeyCode == Keys.Right)
			{
				// Performs command completion
				var currentTextAtPrompt = GetTextAtPrompt();
				var lastCommand = _commandHistory.LastCommand;

				if (lastCommand != null && (currentTextAtPrompt.Length == 0 || lastCommand.StartsWith(currentTextAtPrompt)))
				{
					if (lastCommand.Length > currentTextAtPrompt.Length)
					{
						AddText(lastCommand[currentTextAtPrompt.Length].ToString());
					}
				}
			}
		}


		private string GetCurrentLine()
		{
			if (Lines.Length > 0)
				return (string) Lines.GetValue(Lines.GetLength(0) - 1);

			return "";
		}

		private string GetTextAtPrompt()
		{
			return GetCurrentLine().Substring(_prompt.Length);
		}

		private void ReplaceTextAtPrompt(string text)
		{
			var currentLine = GetCurrentLine();
			var charactersAfterPrompt = currentLine.Length - _prompt.Length;

			if (charactersAfterPrompt == 0)
			{
				AddText(text);
			}
			else
			{
				Select(TextLength - charactersAfterPrompt, charactersAfterPrompt);
				SelectedText = text;
			}
		}

		private bool IsCaretAtCurrentLine()
		{
			return TextLength - SelectionStart <= GetCurrentLine().Length;
		}

		private void MoveCaretToEndOfText()
		{
			SelectionStart = TextLength;
			ScrollToCaret();
		}

		private bool IsCaretJustBeforePrompt()
		{
			return IsCaretAtCurrentLine() && GetCurrentCaretColumnPosition() == _prompt.Length;
		}

		private int GetCurrentCaretColumnPosition()
		{
			var currentLine = GetCurrentLine();
			var currentCaretPosition = SelectionStart;
			return (currentCaretPosition - TextLength + currentLine.Length);
		}

		private bool IsCaretAtWritablePosition()
		{
			return IsCaretAtCurrentLine() && GetCurrentCaretColumnPosition() >= _prompt.Length;
		}

		private void SetPromptText(string val)
		{
			Select(0, _prompt.Length);
			SelectedText = val;

			_prompt = val;
		}

		public string[] GetCommandHistory()
		{
			return _commandHistory.GetCommandHistory();
		}

		public void WriteText(string text)
		{
			AddText(text);
		}
		
		private bool IsTerminatorKey(Keys key)
		{
			return key == Keys.Enter;
		}

		private bool IsTerminatorKey(char keyChar)
		{
			return keyChar == 13;
		}

		// Substitute for buggy AppendText()
		private void AddText(string text)
		{
			Text += text;
			MoveCaretToEndOfText();
		}

		/// <summary>
		///     Required method for Designer support - do not modify
		///     the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// ConsoleTextBox
			// 
			this.AcceptsReturn = true;
			this.AcceptsTab = true;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.MaxLength = 0;
			this.Multiline = true;
			this.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.Size = new System.Drawing.Size(400, 176);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShellControl_KeyDown);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.shellTextBox_KeyPress);
			this.ResumeLayout(false);

		}
	}
}