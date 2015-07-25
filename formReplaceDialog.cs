using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EQ_Zip
{
    public partial class formReplaceDialog : Form
    {
        public EQArchiveFile OldFile;
        public EQArchiveFile NewFile;
        
        public formReplaceDialog()
        {
            InitializeComponent();

            this.DialogResult = DialogResult.Cancel;
        }

        private void formReplaceDialog_Load(object sender, EventArgs e)
        {
            if ((OldFile == null) || (NewFile == null))
            {
                this.Close();
            }

            pictureNewFile.Image = NewFile.GetThumbnail();
            pictureOldFile.Image = OldFile.GetThumbnail();

            labelNewFile.Text = NewFile.Filename;
            labelOldFile.Text = OldFile.Filename;
        }

        private void formReplaceDialog_Activated(object sender, EventArgs e)
        {
            buttonSeparate.Focus();
        }
    }
}
