namespace CustomWindow
{
    partial class Form1
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
            this.checkBoxLocalServersOnly = new System.Windows.Forms.CheckBox();
            this.checkBoxAccessibleOnly = new System.Windows.Forms.CheckBox();
            this.textBoxBackupDirectory = new System.Windows.Forms.TextBox();
            this.labelBackupDirectory = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBoxLocalServersOnly
            // 
            this.checkBoxLocalServersOnly.AutoSize = true;
            this.checkBoxLocalServersOnly.Location = new System.Drawing.Point(25, 13);
            this.checkBoxLocalServersOnly.Name = "checkBoxLocalServersOnly";
            this.checkBoxLocalServersOnly.Size = new System.Drawing.Size(111, 17);
            this.checkBoxLocalServersOnly.TabIndex = 0;
            this.checkBoxLocalServersOnly.Text = "Local servers only";
            this.checkBoxLocalServersOnly.UseVisualStyleBackColor = true;
            // 
            // checkBoxAccessibleOnly
            // 
            this.checkBoxAccessibleOnly.AutoSize = true;
            this.checkBoxAccessibleOnly.Location = new System.Drawing.Point(25, 37);
            this.checkBoxAccessibleOnly.Name = "checkBoxAccessibleOnly";
            this.checkBoxAccessibleOnly.Size = new System.Drawing.Size(136, 17);
            this.checkBoxAccessibleOnly.TabIndex = 1;
            this.checkBoxAccessibleOnly.Text = "Accessible servers only";
            this.checkBoxAccessibleOnly.UseVisualStyleBackColor = true;
            // 
            // textBoxBackupDirectory
            // 
            this.textBoxBackupDirectory.Location = new System.Drawing.Point(25, 85);
            this.textBoxBackupDirectory.Name = "textBoxBackupDirectory";
            this.textBoxBackupDirectory.Size = new System.Drawing.Size(354, 20);
            this.textBoxBackupDirectory.TabIndex = 2;
            // 
            // labelBackupDirectory
            // 
            this.labelBackupDirectory.AutoSize = true;
            this.labelBackupDirectory.Location = new System.Drawing.Point(22, 69);
            this.labelBackupDirectory.Name = "labelBackupDirectory";
            this.labelBackupDirectory.Size = new System.Drawing.Size(123, 13);
            this.labelBackupDirectory.TabIndex = 3;
            this.labelBackupDirectory.Text = "Path to backup directory";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(304, 156);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Ignore";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(214, 155);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 7;
            this.buttonOK.Text = "Apply";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 199);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelBackupDirectory);
            this.Controls.Add(this.textBoxBackupDirectory);
            this.Controls.Add(this.checkBoxAccessibleOnly);
            this.Controls.Add(this.checkBoxLocalServersOnly);
            this.Name = "Form1";
            this.Text = "Application Settings";
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxLocalServersOnly;
        private System.Windows.Forms.CheckBox checkBoxAccessibleOnly;
        private System.Windows.Forms.TextBox textBoxBackupDirectory;
        private System.Windows.Forms.Label labelBackupDirectory;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}

