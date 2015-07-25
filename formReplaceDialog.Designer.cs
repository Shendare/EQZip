namespace EQ_Zip
{
    partial class formReplaceDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.pictureNewFile = new System.Windows.Forms.PictureBox();
            this.pictureOldFile = new System.Windows.Forms.PictureBox();
            this.labelNewFile = new System.Windows.Forms.Label();
            this.labelOldFile = new System.Windows.Forms.Label();
            this.buttonSeparate = new System.Windows.Forms.Button();
            this.buttonReplace = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureNewFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureOldFile)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(82, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(418, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "You dragged a new file onto an existing one in the archive.";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(256, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "Do you want this new file...";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(321, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(256, 23);
            this.label3.TabIndex = 2;
            this.label3.Text = "...to replace this old one?";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureNewFile
            // 
            this.pictureNewFile.Location = new System.Drawing.Point(14, 116);
            this.pictureNewFile.Name = "pictureNewFile";
            this.pictureNewFile.Size = new System.Drawing.Size(256, 256);
            this.pictureNewFile.TabIndex = 3;
            this.pictureNewFile.TabStop = false;
            // 
            // pictureOldFile
            // 
            this.pictureOldFile.Location = new System.Drawing.Point(321, 116);
            this.pictureOldFile.Name = "pictureOldFile";
            this.pictureOldFile.Size = new System.Drawing.Size(256, 256);
            this.pictureOldFile.TabIndex = 4;
            this.pictureOldFile.TabStop = false;
            // 
            // labelNewFile
            // 
            this.labelNewFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelNewFile.Location = new System.Drawing.Point(14, 375);
            this.labelNewFile.Name = "labelNewFile";
            this.labelNewFile.Size = new System.Drawing.Size(256, 20);
            this.labelNewFile.TabIndex = 3;
            this.labelNewFile.Text = "filename1.xyz";
            this.labelNewFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelOldFile
            // 
            this.labelOldFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelOldFile.Location = new System.Drawing.Point(321, 375);
            this.labelOldFile.Name = "labelOldFile";
            this.labelOldFile.Size = new System.Drawing.Size(256, 20);
            this.labelOldFile.TabIndex = 4;
            this.labelOldFile.Text = "filename2.xyz";
            this.labelOldFile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // buttonSeparate
            // 
            this.buttonSeparate.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonSeparate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonSeparate.Location = new System.Drawing.Point(321, 433);
            this.buttonSeparate.Name = "buttonSeparate";
            this.buttonSeparate.Size = new System.Drawing.Size(256, 41);
            this.buttonSeparate.TabIndex = 6;
            this.buttonSeparate.Text = "NO. Import it as a separate file.";
            this.buttonSeparate.UseVisualStyleBackColor = true;
            // 
            // buttonReplace
            // 
            this.buttonReplace.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonReplace.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReplace.Location = new System.Drawing.Point(14, 433);
            this.buttonReplace.Name = "buttonReplace";
            this.buttonReplace.Size = new System.Drawing.Size(256, 41);
            this.buttonReplace.TabIndex = 5;
            this.buttonReplace.Text = "Yes! I\'m swapping it out.";
            this.buttonReplace.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonCancel.Location = new System.Drawing.Point(139, 499);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(335, 42);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "No. It was an accident. Go away.";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // formReplaceDialog
            // 
            this.AcceptButton = this.buttonSeparate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(610, 569);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonReplace);
            this.Controls.Add(this.buttonSeparate);
            this.Controls.Add(this.labelOldFile);
            this.Controls.Add(this.labelNewFile);
            this.Controls.Add(this.pictureOldFile);
            this.Controls.Add(this.pictureNewFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "formReplaceDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Replace This File?";
            this.Activated += new System.EventHandler(this.formReplaceDialog_Activated);
            this.Load += new System.EventHandler(this.formReplaceDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureNewFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureOldFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.PictureBox pictureNewFile;
        private System.Windows.Forms.PictureBox pictureOldFile;
        private System.Windows.Forms.Label labelNewFile;
        private System.Windows.Forms.Label labelOldFile;
        private System.Windows.Forms.Button buttonSeparate;
        private System.Windows.Forms.Button buttonReplace;
        private System.Windows.Forms.Button buttonCancel;
    }
}