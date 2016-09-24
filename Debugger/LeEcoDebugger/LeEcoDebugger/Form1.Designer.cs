

namespace LeEcoDebugger
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
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnElfFolder = new System.Windows.Forms.Button();
            this.txtElf = new System.Windows.Forms.TextBox();
            this.Elftxt = new System.Windows.Forms.Label();
            this.btnDumpFolder = new System.Windows.Forms.Button();
            this.txtDump = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.targetLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(51, 186);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 31);
            this.button1.TabIndex = 0;
            this.button1.Text = "DebugModem";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "SelectDump";
            this.openFileDialog1.Filter = "\"Bin Files|*.bin|*.cmm|All Files\"";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.targetLabel);
            this.groupBox1.Controls.Add(this.btnElfFolder);
            this.groupBox1.Controls.Add(this.txtElf);
            this.groupBox1.Controls.Add(this.Elftxt);
            this.groupBox1.Controls.Add(this.btnDumpFolder);
            this.groupBox1.Controls.Add(this.txtDump);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBox1.Location = new System.Drawing.Point(51, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(645, 130);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // btnElfFolder
            // 
            this.btnElfFolder.Image = global::LeEcoDebugger.Properties.Resources.openfolder;
            this.btnElfFolder.Location = new System.Drawing.Point(470, 41);
            this.btnElfFolder.Name = "btnElfFolder";
            this.btnElfFolder.Size = new System.Drawing.Size(60, 23);
            this.btnElfFolder.TabIndex = 5;
            this.btnElfFolder.UseVisualStyleBackColor = true;
            this.btnElfFolder.Click += new System.EventHandler(this.btnElfFolder_Click);
            // 
            // txtElf
            // 
            this.txtElf.Location = new System.Drawing.Point(94, 41);
            this.txtElf.Name = "txtElf";
            this.txtElf.Size = new System.Drawing.Size(360, 20);
            this.txtElf.TabIndex = 4;
            // 
            // Elftxt
            // 
            this.Elftxt.AutoSize = true;
            this.Elftxt.ForeColor = System.Drawing.Color.Maroon;
            this.Elftxt.Location = new System.Drawing.Point(6, 44);
            this.Elftxt.Name = "Elftxt";
            this.Elftxt.Size = new System.Drawing.Size(26, 13);
            this.Elftxt.TabIndex = 3;
            this.Elftxt.Text = "ELF";
            this.Elftxt.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnDumpFolder
            // 
            this.btnDumpFolder.Image = global::LeEcoDebugger.Properties.Resources.openfolder;
            this.btnDumpFolder.Location = new System.Drawing.Point(470, 9);
            this.btnDumpFolder.Name = "btnDumpFolder";
            this.btnDumpFolder.Size = new System.Drawing.Size(60, 23);
            this.btnDumpFolder.TabIndex = 2;
            this.btnDumpFolder.UseVisualStyleBackColor = true;
            this.btnDumpFolder.Click += new System.EventHandler(this.btnDumpFolder_Click);
            // 
            // txtDump
            // 
            this.txtDump.Location = new System.Drawing.Point(94, 9);
            this.txtDump.Name = "txtDump";
            this.txtDump.Size = new System.Drawing.Size(360, 20);
            this.txtDump.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Maroon;
            this.label2.Location = new System.Drawing.Point(6, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "DumpFolder";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(48, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // targetLabel
            // 
            this.targetLabel.AutoSize = true;
            this.targetLabel.ForeColor = System.Drawing.Color.Blue;
            this.targetLabel.Location = new System.Drawing.Point(6, 76);
            this.targetLabel.Name = "targetLabel";
            this.targetLabel.Size = new System.Drawing.Size(38, 13);
            this.targetLabel.TabIndex = 3;
            this.targetLabel.Text = "Target";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(758, 288);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "LeMobile_Debugger";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnDumpFolder;
        private System.Windows.Forms.TextBox txtDump;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtElf;
        private System.Windows.Forms.Label Elftxt;
        private System.Windows.Forms.Button btnElfFolder;
        private System.Windows.Forms.Label targetLabel;
    }
}

