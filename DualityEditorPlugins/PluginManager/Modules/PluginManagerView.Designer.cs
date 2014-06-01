namespace PluginManager.Modules
{
	partial class PluginManagerView
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.RepositoryURILabel = new System.Windows.Forms.Label();
			this.RepositoryURITextBox = new System.Windows.Forms.TextBox();
			this.ConsoleControl = new PluginManager.Modules.ConsoleControl();
			this.SuspendLayout();
			// 
			// RepositoryURILabel
			// 
			this.RepositoryURILabel.AutoSize = true;
			this.RepositoryURILabel.Location = new System.Drawing.Point(12, 9);
			this.RepositoryURILabel.Name = "RepositoryURILabel";
			this.RepositoryURILabel.Size = new System.Drawing.Size(79, 13);
			this.RepositoryURILabel.TabIndex = 1;
			this.RepositoryURILabel.Text = "Repository URI";
			// 
			// RepositoryURITextBox
			// 
			this.RepositoryURITextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RepositoryURITextBox.Location = new System.Drawing.Point(97, 6);
			this.RepositoryURITextBox.Name = "RepositoryURITextBox";
			this.RepositoryURITextBox.Size = new System.Drawing.Size(690, 20);
			this.RepositoryURITextBox.TabIndex = 2;
			// 
			// ConsoleControl
			// 
			this.ConsoleControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsoleControl.AutoScroll = true;
			this.ConsoleControl.Location = new System.Drawing.Point(0, 32);
			this.ConsoleControl.Name = "ConsoleControl";
			this.ConsoleControl.Prompt = ">>>";
			this.ConsoleControl.ShellTextBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(37)))), ((int)(((byte)(37)))));
			this.ConsoleControl.ShellTextFont = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConsoleControl.ShellTextForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
			this.ConsoleControl.Size = new System.Drawing.Size(799, 473);
			this.ConsoleControl.TabIndex = 0;
			// 
			// PluginManagerView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(196)))), ((int)(((byte)(196)))));
			this.ClientSize = new System.Drawing.Size(799, 505);
			this.Controls.Add(this.RepositoryURITextBox);
			this.Controls.Add(this.RepositoryURILabel);
			this.Controls.Add(this.ConsoleControl);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "PluginManagerView";
			this.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Float;
			this.Text = "Plugin Manager";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ConsoleControl ConsoleControl;
		private System.Windows.Forms.Label RepositoryURILabel;
		private System.Windows.Forms.TextBox RepositoryURITextBox;





	}
}