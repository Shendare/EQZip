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
    public partial class formPreferences : Form
    {
        public formPreferences()
        {
            InitializeComponent();
        }

        private void formPreferences_Load(object sender, EventArgs e)
        {
            switch (Settings.ExportFormat)
            {
                case ".png":
                case ".bmp":
                case ".jpg":
                case ".gif":
                    checkExportConvert.Checked = true;
                    listExportFormat.Text = Settings.ExportFormat;
                    break;
                case "":
                    checkExportConvert.Checked = false;
                    listExportFormat.Enabled = false;
                    break;
                default:
                    checkExportConvert.Checked = true;
                    listExportFormat.Text = ".png";
                    break;
            }

            switch (Settings.ImportFormat)
            {
                case "":
                    checkImportConvert.Checked = false;
                    listImportFormat.Text = "";
                    break;
                case "Auto":
                case "16-bit":
                case "24-bit":
                case "32-bit":
                    checkImportConvert.Checked = true;
                    listImportFormat.Text = Settings.ImportFormat;
                    break;
                default:
                    checkImportConvert.Checked = true;
                    listImportFormat.Text = "Auto";
                    break;
            }

            if (Settings.RememberMRUs == 0)
            {
                checkMRU.Checked = false;
                listMRU.Enabled = false;
            }
            else if ((Settings.RememberMRUs > 0) && (Settings.RememberMRUs < 9))
            {
                checkMRU.Checked = true;
                listMRU.Text = Settings.RememberMRUs.ToString();
            }
            else
            {
                checkMRU.Checked = true;
                listMRU.Text = "9";
            }

            checkConfirmImportOverwrite.Checked = Settings.ConfirmImportOverwrite;
            checkConfirmExportOverwrite.Checked = Settings.ConfirmExportOverwrite;
            checkConfirmRenameOverwrite.Checked = Settings.ConfirmRenameOverwrite;
        }

        private void checkExportConvert_CheckedChanged(object sender, EventArgs e)
        {
            listExportFormat.Enabled = checkExportConvert.Checked;

            if (checkExportConvert.Checked)
            {
                listExportFormat.Focus();
            }
        }

        private void checkImportConvert_CheckedChanged(object sender, EventArgs e)
        {
            listImportFormat.Enabled = checkImportConvert.Checked;

            if (listImportFormat.Text == "")
            {
                listImportFormat.Text = "Auto";
            }
        }

        private void checkMRU_CheckedChanged(object sender, EventArgs e)
        {
            listMRU.Enabled = checkMRU.Checked;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Settings.Changed = false;
            
            this.Close();
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            if (checkExportConvert.Checked == false)
            {
                Settings.ExportFormat = "";
            }
            else
            {
                Settings.ExportFormat = listExportFormat.Text;
            }

            if (checkImportConvert.Checked == false)
            {
                Settings.ImportFormat = "";
            }
            else
            {
                Settings.ImportFormat = listImportFormat.Text;
            }

            if (checkMRU.Checked == false)
            {
                Settings.RememberMRUs = 0;
                
                for (int _i = 0; _i < 9; _i++)
                {
                    Settings.MRUs[_i] = "";
                }
            }
            else
            {
                Settings.RememberMRUs = int.Parse(listMRU.Text);

                for (int _i = Settings.RememberMRUs; _i < 9; _i++)
                {
                    Settings.MRUs[_i] = "";
                }
            }

            Settings.ConfirmImportOverwrite = checkConfirmImportOverwrite.Checked;
            Settings.ConfirmExportOverwrite = checkConfirmExportOverwrite.Checked;
            Settings.ConfirmRenameOverwrite = checkConfirmRenameOverwrite.Checked;

            Settings.Changed = true;

            this.Close();
        }

        private void buttonHelpExport_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Game textures are stored internally as DirectDraw Surface (.dds) files, because it is the only format that supports MipMaps, a way of scaling textures for Level of Detail based on distance from the camera so they don't get horribly pixellated and flicker between light and dark pixels.\n\nDDS support in Windows and in graphics software tends to be very spotty, however, so when exporting a texture from an EQ archive, EQ-Zip can automatically convert it into a .png image to preserve colors and alpha channels, or another format if you are not worried about that and don't want a .png.", "EQ-Zip Export Auto-Conversion Description", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void buttonHelpImport_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, "Game textures are stored internally as DirectDraw Surface (.dds) files, because it is the only format that supports MipMaps, a way of scaling textures for Level of Detail based on distance from the camera so they don't get horribly pixellated and flicker between light and dark pixels.\n\nBecause most replacement textures you come across will likely be in a format other than .dds, when importing a texture to an EQ archive, EQ-Zip can automatically convert it into a .dds image and generate high quality bicubic MipMaps.\n\nYou can choose a specific number of bits per pixel to import at, or leave it at 'Auto', which will determine the best (smallest without losing visual quality) format based on the image's alpha channel use or absence.", "EQ-Zip Import Auto-Conversion Description", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
