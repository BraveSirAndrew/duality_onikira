namespace PluginManager.Modules
{
	partial class ConsoleControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ConsoleTextBox = new ConsoleTextBox();

			this.SuspendLayout();
			// 
			// ConsoleTextBox
			// 
			this.ConsoleTextBox.AcceptsReturn = true;
			this.ConsoleTextBox.AcceptsTab = true;
			this.ConsoleTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
			this.ConsoleTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsoleTextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ConsoleTextBox.Location = new System.Drawing.Point(0, 0);
			this.ConsoleTextBox.MaxLength = 0;
			this.ConsoleTextBox.Multiline = true;
			this.ConsoleTextBox.Name = "ConsoleTextBox";
			this.ConsoleTextBox.Prompt = ">>>";
			this.ConsoleTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.ConsoleTextBox.Size = new System.Drawing.Size(150, 150);
			this.ConsoleTextBox.TabIndex = 0;
			// 
			// ConsoleControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ConsoleTextBox);
			this.Name = "ConsoleControl";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ConsoleTextBox ConsoleTextBox;
	}
}
