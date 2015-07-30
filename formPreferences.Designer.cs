namespace EQ_Zip
{
    partial class formPreferences
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
            this.listExportFormat = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkImportConvert = new System.Windows.Forms.CheckBox();
            this.checkExportConvert = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkConfirmRenameOverwrite = new System.Windows.Forms.CheckBox();
            this.checkConfirmExportOverwrite = new System.Windows.Forms.CheckBox();
            this.checkConfirmImportOverwrite = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listMRU = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkMRU = new System.Windows.Forms.CheckBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.listImportFormat = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listExportFormat
            // 
            this.listExportFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listExportFormat.FormattingEnabled = true;
            this.listExportFormat.Items.AddRange(new object[] {
            ".png",
            ".gif",
            ".bmp",
            ".jpg"});
            this.listExportFormat.Location = new System.Drawing.Point(263, 30);
            this.listExportFormat.Name = "listExportFormat";
            this.listExportFormat.Size = new System.Drawing.Size(117, 21);
            this.listExportFormat.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listImportFormat);
            this.groupBox1.Controls.Add(this.checkImportConvert);
            this.groupBox1.Controls.Add(this.checkExportConvert);
            this.groupBox1.Controls.Add(this.listExportFormat);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(399, 108);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " Auto-Conversion ";
            // 
            // checkImportConvert
            // 
            this.checkImportConvert.AutoSize = true;
            this.checkImportConvert.Location = new System.Drawing.Point(36, 68);
            this.checkImportConvert.Name = "checkImportConvert";
            this.checkImportConvert.Size = new System.Drawing.Size(241, 17);
            this.checkImportConvert.TabIndex = 2;
            this.checkImportConvert.Text = "When &Importing images, auto-convert to DDS";
            this.checkImportConvert.UseVisualStyleBackColor = true;
            this.checkImportConvert.CheckedChanged += new System.EventHandler(this.checkImportConvert_CheckedChanged);
            // 
            // checkExportConvert
            // 
            this.checkExportConvert.AutoSize = true;
            this.checkExportConvert.Location = new System.Drawing.Point(36, 34);
            this.checkExportConvert.Name = "checkExportConvert";
            this.checkExportConvert.Size = new System.Drawing.Size(222, 17);
            this.checkExportConvert.TabIndex = 0;
            this.checkExportConvert.Text = "When &Extracting images, auto-convert to:";
            this.checkExportConvert.UseVisualStyleBackColor = true;
            this.checkExportConvert.CheckedChanged += new System.EventHandler(this.checkExportConvert_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkConfirmRenameOverwrite);
            this.groupBox2.Controls.Add(this.checkConfirmExportOverwrite);
            this.groupBox2.Controls.Add(this.checkConfirmImportOverwrite);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.listMRU);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.checkMRU);
            this.groupBox2.Location = new System.Drawing.Point(12, 138);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(399, 139);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " Other Preferences ";
            // 
            // checkConfirmRenameOverwrite
            // 
            this.checkConfirmRenameOverwrite.AutoSize = true;
            this.checkConfirmRenameOverwrite.Location = new System.Drawing.Point(263, 97);
            this.checkConfirmRenameOverwrite.Name = "checkConfirmRenameOverwrite";
            this.checkConfirmRenameOverwrite.Size = new System.Drawing.Size(74, 17);
            this.checkConfirmRenameOverwrite.TabIndex = 9;
            this.checkConfirmRenameOverwrite.Text = "Ren&aming";
            this.checkConfirmRenameOverwrite.UseVisualStyleBackColor = true;
            // 
            // checkConfirmExportOverwrite
            // 
            this.checkConfirmExportOverwrite.AutoSize = true;
            this.checkConfirmExportOverwrite.Location = new System.Drawing.Point(160, 97);
            this.checkConfirmExportOverwrite.Name = "checkConfirmExportOverwrite";
            this.checkConfirmExportOverwrite.Size = new System.Drawing.Size(70, 17);
            this.checkConfirmExportOverwrite.TabIndex = 8;
            this.checkConfirmExportOverwrite.Text = "E&xporting";
            this.checkConfirmExportOverwrite.UseVisualStyleBackColor = true;
            // 
            // checkConfirmImportOverwrite
            // 
            this.checkConfirmImportOverwrite.AutoSize = true;
            this.checkConfirmImportOverwrite.Location = new System.Drawing.Point(62, 97);
            this.checkConfirmImportOverwrite.Name = "checkConfirmImportOverwrite";
            this.checkConfirmImportOverwrite.Size = new System.Drawing.Size(69, 17);
            this.checkConfirmImportOverwrite.TabIndex = 7;
            this.checkConfirmImportOverwrite.Text = "I&mporting";
            this.checkConfirmImportOverwrite.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(59, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(278, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Ask for confirmation before overwriting existing files when:";
            // 
            // listMRU
            // 
            this.listMRU.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listMRU.FormattingEnabled = true;
            this.listMRU.Items.AddRange(new object[] {
            "9",
            "8",
            "7",
            "6",
            "5",
            "4",
            "3",
            "2",
            "1"});
            this.listMRU.Location = new System.Drawing.Point(315, 30);
            this.listMRU.Name = "listMRU";
            this.listMRU.Size = new System.Drawing.Size(32, 21);
            this.listMRU.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(195, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "&Number to Remember:";
            // 
            // checkMRU
            // 
            this.checkMRU.AutoSize = true;
            this.checkMRU.Location = new System.Drawing.Point(36, 34);
            this.checkMRU.Name = "checkMRU";
            this.checkMRU.Size = new System.Drawing.Size(153, 17);
            this.checkMRU.TabIndex = 4;
            this.checkMRU.Text = "&Remember recent archives";
            this.checkMRU.UseVisualStyleBackColor = true;
            this.checkMRU.CheckedChanged += new System.EventHandler(this.checkMRU_CheckedChanged);
            // 
            // buttonApply
            // 
            this.buttonApply.Location = new System.Drawing.Point(74, 311);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(95, 30);
            this.buttonApply.TabIndex = 9;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(240, 311);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(95, 30);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // listImportFormat
            // 
            this.listImportFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listImportFormat.FormattingEnabled = true;
            this.listImportFormat.Items.AddRange(new object[] {
            "16-bit",
            "24-bit",
            "32-bit"});
            this.listImportFormat.Location = new System.Drawing.Point(283, 64);
            this.listImportFormat.Name = "listImportFormat";
            this.listImportFormat.Size = new System.Drawing.Size(97, 21);
            this.listImportFormat.TabIndex = 3;
            // 
            // formPreferences
            // 
            this.AcceptButton = this.buttonApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(424, 375);
            this.ControlBox = false;
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formPreferences";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EQ-Zip Preferences";
            this.Load += new System.EventHandler(this.formPreferences_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox listExportFormat;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkImportConvert;
        private System.Windows.Forms.CheckBox checkExportConvert;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox listMRU;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkMRU;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.CheckBox checkConfirmExportOverwrite;
        private System.Windows.Forms.CheckBox checkConfirmImportOverwrite;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkConfirmRenameOverwrite;
        private System.Windows.Forms.ComboBox listImportFormat;

    }
}