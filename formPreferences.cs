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
            bool _tryAgain = false;

            do
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
                    case ".dds":
                        checkExportConvert.Checked = true;
                        if (Settings.MipMapsOnDDSExport)
                        {
                            listExportFormat.Text = ".dds (with MipMaps)";
                        }
                        else
                        {
                            listExportFormat.Text = ".dds (no MipMaps)";
                        }
                        break;
                    case "":
                        checkExportConvert.Checked = false;
                        listExportFormat.Enabled = false;
                        break;
                    default:
                        Settings.ExportFormat = ".dds";
                        _tryAgain = true;
                        break;
                }
            } while (_tryAgain);

            _tryAgain = false;

            do
            {
                switch (Settings.ImportFormat)
                {
                    case ".dds":
                        checkImportConvert.Checked = true;
                        checkImportCompress.Checked = Settings.CompressImportDDS;
                        break;
                    case "":
                        checkImportConvert.Checked = false;
                        checkImportCompress.Enabled = false;
                        break;
                    default:
                        Settings.ImportFormat = ".dds";
                        _tryAgain = true;
                        break;
                }
            } while (_tryAgain);

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
            
            checkImportCompress.Checked = false; // Not yet supported
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
            //checkImportCompress.Enabled = checkImportConvert.Checked;
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
                switch (listExportFormat.Text)
                {
                    case ".dds (no MipMaps)":
                        Settings.ExportFormat = ".dds";
                        Settings.MipMapsOnDDSExport = false;
                        break;
                    case ".dds (with MipMaps)":
                        Settings.ExportFormat = ".dds";
                        Settings.MipMapsOnDDSExport = true;
                        break;
                    default:
                        Settings.ExportFormat = listExportFormat.Text;
                        break;
                }
            }

            if (checkImportConvert.Checked == false)
            {
                Settings.ImportFormat = "";
            }
            else
            {
                Settings.ImportFormat = ".dds";
                Settings.CompressImportDDS = checkImportCompress.Checked;
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
    }
}
