namespace TanScreenshot
{
    partial class MainForm
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
            _hook?.Dispose();
            _mutex?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.consoleBox = new System.Windows.Forms.RichTextBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.logPanel = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbCopy = new System.Windows.Forms.CheckBox();
            this.cbAutorun = new System.Windows.Forms.CheckBox();
            this.lblActiveHotKey = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnChangeHotkey = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.logPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // consoleBox
            // 
            this.consoleBox.Location = new System.Drawing.Point(3, 3);
            this.consoleBox.Name = "consoleBox";
            this.consoleBox.Size = new System.Drawing.Size(770, 298);
            this.consoleBox.TabIndex = 0;
            this.consoleBox.Text = "";
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(715, 3);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(58, 24);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // logPanel
            // 
            this.logPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logPanel.Controls.Add(this.consoleBox);
            this.logPanel.Location = new System.Drawing.Point(12, 12);
            this.logPanel.MinimumSize = new System.Drawing.Size(776, 304);
            this.logPanel.Name = "logPanel";
            this.logPanel.Size = new System.Drawing.Size(776, 304);
            this.logPanel.TabIndex = 2;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.cbCopy);
            this.panel1.Controls.Add(this.cbAutorun);
            this.panel1.Controls.Add(this.lblActiveHotKey);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnChangeHotkey);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Location = new System.Drawing.Point(12, 319);
            this.panel1.MinimumSize = new System.Drawing.Size(776, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(776, 30);
            this.panel1.TabIndex = 3;
            // 
            // cbCopy
            // 
            this.cbCopy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbCopy.AutoSize = true;
            this.cbCopy.Location = new System.Drawing.Point(448, 7);
            this.cbCopy.Name = "cbCopy";
            this.cbCopy.Size = new System.Drawing.Size(70, 17);
            this.cbCopy.TabIndex = 2;
            this.cbCopy.Text = "Clipboard";
            this.cbCopy.UseVisualStyleBackColor = true;
            // 
            // cbAutorun
            // 
            this.cbAutorun.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cbAutorun.AutoSize = true;
            this.cbAutorun.Location = new System.Drawing.Point(524, 7);
            this.cbAutorun.Name = "cbAutorun";
            this.cbAutorun.Size = new System.Drawing.Size(63, 17);
            this.cbAutorun.TabIndex = 2;
            this.cbAutorun.Text = "Autorun";
            this.cbAutorun.UseVisualStyleBackColor = true;
            this.cbAutorun.CheckedChanged += new System.EventHandler(this.cbAutorun_CheckedChanged);
            // 
            // lblActiveHotKey
            // 
            this.lblActiveHotKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblActiveHotKey.AutoSize = true;
            this.lblActiveHotKey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveHotKey.Location = new System.Drawing.Point(138, 7);
            this.lblActiveHotKey.Name = "lblActiveHotKey";
            this.lblActiveHotKey.Size = new System.Drawing.Size(96, 20);
            this.lblActiveHotKey.TabIndex = 0;
            this.lblActiveHotKey.Text = "Print Screen";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Screenshot Key :";
            // 
            // btnChangeHotkey
            // 
            this.btnChangeHotkey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeHotkey.Location = new System.Drawing.Point(593, 3);
            this.btnChangeHotkey.Name = "btnChangeHotkey";
            this.btnChangeHotkey.Size = new System.Drawing.Size(116, 24);
            this.btnChangeHotkey.TabIndex = 1;
            this.btnChangeHotkey.Text = "Change HotKeys";
            this.btnChangeHotkey.UseVisualStyleBackColor = true;
            this.btnChangeHotkey.Click += new System.EventHandler(this.btnChangeHotkey_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Text = "notifyIcon";
            this.notifyIcon.Visible = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 361);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.logPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(816, 400);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TanScreenshot";
            this.logPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox consoleBox;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel logPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnChangeHotkey;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblActiveHotKey;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.CheckBox cbAutorun;
        private System.Windows.Forms.CheckBox cbCopy;
    }
}

