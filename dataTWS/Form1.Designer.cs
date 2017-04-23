namespace dataTWS
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lstData = new System.Windows.Forms.ListBox();
            this.btnCopyListbox = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.chkComplete = new System.Windows.Forms.CheckBox();
            this.lblCounting = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(47, 22);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "&Load Data";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(1057, 22);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "E&xit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lstData
            // 
            this.lstData.FormattingEnabled = true;
            this.lstData.Location = new System.Drawing.Point(47, 73);
            this.lstData.Name = "lstData";
            this.lstData.Size = new System.Drawing.Size(1006, 56);
            this.lstData.TabIndex = 2;
            // 
            // btnCopyListbox
            // 
            this.btnCopyListbox.Location = new System.Drawing.Point(976, 22);
            this.btnCopyListbox.Name = "btnCopyListbox";
            this.btnCopyListbox.Size = new System.Drawing.Size(75, 23);
            this.btnCopyListbox.TabIndex = 3;
            this.btnCopyListbox.Text = "&Copy Data";
            this.btnCopyListbox.UseVisualStyleBackColor = true;
            this.btnCopyListbox.Click += new System.EventHandler(this.btnCopyListbox_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(57, 154);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = ".";
            // 
            // chkComplete
            // 
            this.chkComplete.AutoSize = true;
            this.chkComplete.Location = new System.Drawing.Point(138, 27);
            this.chkComplete.Name = "chkComplete";
            this.chkComplete.Size = new System.Drawing.Size(130, 17);
            this.chkComplete.TabIndex = 5;
            this.chkComplete.Text = "Close after completion";
            this.chkComplete.UseVisualStyleBackColor = true;
            // 
            // lblCounting
            // 
            this.lblCounting.AutoSize = true;
            this.lblCounting.Location = new System.Drawing.Point(57, 132);
            this.lblCounting.Name = "lblCounting";
            this.lblCounting.Size = new System.Drawing.Size(10, 13);
            this.lblCounting.TabIndex = 6;
            this.lblCounting.Text = ".";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 215);
            this.Controls.Add(this.lblCounting);
            this.Controls.Add(this.chkComplete);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCopyListbox);
            this.Controls.Add(this.lstData);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ListBox lstData;
        private System.Windows.Forms.Button btnCopyListbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkComplete;
        private System.Windows.Forms.Label lblCounting;
    }
}

