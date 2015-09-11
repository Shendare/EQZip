/*
 * 
 *  EQ-Zip 1.0
 *  
 *  By Shendare (Jon D. Jackson)
 * 
 *  Portions of this code not covered by another author's or entity's copyright are released under
 *  the Creative Commons Zero (CC0) public domain license.
 *  
 *  To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and
 *  related or neighboring rights to EQ-Zip. This work is published from: The United States. 
 *  
 *  You may copy, modify, and distribute the work, even for commercial purposes, without asking permission.
 * 
 *  For more information, read the CC0 summary and full legal text here:
 *  
 *  https://creativecommons.org/publicdomain/zero/1.0/
 * 
 *  EQ-Zip is not affiliated with, endorsed by, approved by, or in any way associated with Daybreak
 *  Games, the EverQuest franchise, or any of the other compression/archive based applications out
 *  there with the word "Zip" in them.
 * 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace EQ_Zip
{
    public partial class formMain : Form
    {
        public Dictionary<string, string> FileTypes = new Dictionary<string, string>()
        {
            { ".dds", "Texture (DirectDraw)" },
            { ".bmp", "Texture (Bitmap)" },
            { ".png", "Texture (PNG)" },
            { ".tga", "Texture (Targa)" },
            { ".gif", "Texture (GIF)" },
            { ".jpg", "Texture (JPEG)" },
            { ".jpeg", "Texture (JPEG)" },
            { ".tif", "Texture (TIFF)" },
            { ".tiff", "Texture (TIFF)" },
            { ".ani", "Animation Definition" },
            { ".lod", "Model Level-of-Detail Specs" },
            { ".mod", "Model Definition" },
            { ".ter", "Terrain Heightmap" },
            { ".wld", "Zone Information" },
            { ".zon", "Zone Definition File" },
            { ".dat", "Zone Data File " },
            { ".prj", "Camera Projection Specs" },
            { ".eco", "Ecology Specifications" },
            { ".wav", "Sound Effect" },
            { ".lay", "Character/Object Model" },
            { ".rfd", "Radial Flora Definitions" }
        };

        public bool ControlKeyDown = false;
        
        public string VersionNumber = "";

        public EQArchive LastArchive = null;

        public List<int> ThumbnailQueue = new List<int>();
        public Mutex ThumbnailMutex = new Mutex(false);
		public bool ThumbnailsLoaded = true;

        public List<int> DecompressQueue = new List<int>();
        public Mutex DecompressMutex = new Mutex(false);

		public Queue<int> ListViewQueue = new Queue<int>();
        public Mutex ListViewMutex = new Mutex(false);

        public ListViewItem[] ItemsCutToClipboard;

        public EQArchive CurrentArchive;
        public bool ArchiveChanged = false;
		public bool ArchiveLoading = false;
		public EQArchiveFile NewFile = null;

        public System.Diagnostics.Stopwatch[] ThumbnailPerf = new System.Diagnostics.Stopwatch[3];

        public System.Diagnostics.Stopwatch _update = new System.Diagnostics.Stopwatch();

        public formMain()
        {
            InitializeComponent();

            // Example: 13.1.0.0 -> 13.1
            VersionNumber = Application.ProductVersion.Substring(0, Application.ProductVersion.IndexOf('.') + 1);
            VersionNumber += Application.ProductVersion.Substring(VersionNumber.Length).Replace(".", "");
            
            while (VersionNumber.EndsWith("0"))
            {
                VersionNumber = VersionNumber.Substring(0, VersionNumber.Length - 1);
            }

            if (VersionNumber.EndsWith("."))
            {
                VersionNumber += "0";
            }
        }

        #region 1. Form Control Event Handlers

        private void aboutEQZipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formAbout _about = new formAbout();

            _about.VersionNumber = this.VersionNumber;
            _about.ShowDialog(this);
        }

        private void clipboardOperation_Complete(ShellFileGroup FileGroup, DragDropEffects Effect)
        {
            if (Effect == DragDropEffects.Move)
            {
                foreach (string _filename in FileGroup.Filenames)
                {
                    // Successfully cut file and pasted it somewhere

                    DeleteItem(listView1.Items[_filename.ToLower()]);
                }

                Status_Changed();
            }
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((CurrentArchive != null) && (listView1.SelectedItems.Count > 0) && NotInDebugger())
            {
                ReleaseCutItems();

                ShellFileGroup _files = new ShellFileGroup();
                
                foreach (ListViewItem _item in listView1.SelectedItems)
                {
                    EQArchiveFile _file = (EQArchiveFile)_item.Tag;

                    _files.Add(_file.Filename, _file.GetContents());
                }
                
                Clipboard.SetDataObject(_files.GetDataObject(DragDropEffects.Copy));

                CheckClipboard();
            }
        }
        
        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ((CurrentArchive != null) && (listView1.SelectedItems.Count > 0) && NotInDebugger())
            {
                ReleaseCutItems();

                ShellFileGroup _files = new ShellFileGroup();
                ItemsCutToClipboard = new ListViewItem[listView1.SelectedItems.Count];
                listView1.SelectedItems.CopyTo(ItemsCutToClipboard, 0);

                foreach (ListViewItem _item in ItemsCutToClipboard)
                {
                    EQArchiveFile _file = (EQArchiveFile)_item.Tag;

                    _files.Add(_file.Filename, _file.GetContents());

                    _item.ForeColor = SystemColors.GrayText;
                }

                _files.ActionCompleted += clipboardOperation_Complete;
                
                Clipboard.SetDataObject(_files.GetDataObject(DragDropEffects.Move));

                CheckClipboard();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentArchive == null)
            {
                return;
            }

            int _numToDelete = listView1.SelectedItems.Count;

            if ((_numToDelete < 2) ||
                (MessageBox.Show(this, "You are about to Delete " + _numToDelete.ToString() + " files from this archive.\n\nAre you sure you wish to do this?", "File Deletion Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes))
            {
                int _numDeleted = 0;

                foreach (ListViewItem _item in listView1.SelectedItems)
                {
                    if (DeleteItem(_item))
                    {
                        _numDeleted++;
                    }
                }

                //MessageBox.Show(_numDeleted.ToString() + " File" + ((_numDeleted == 1) ? "" : "s") + " Deleted from Archive");
            }
        }

        private void detailsViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void exportAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportFiles(listView1.Items);
        }

        private void exportFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportFiles(listView1.SelectedItems);
        }

        private void flipHorizontallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in listView1.SelectedItems)
            {
                EQArchiveFile _file = (EQArchiveFile)_item.Tag;
                Bitmap _image = (Bitmap)_file.GetImage();
                
                if (_image != null)
                {
                    _image.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    _file.SetImage(_image, (_file.ImageSubformat == "") ? _file.ImageFormat : _file.ImageSubformat);
                    
                    UpdateItem(_item, true);
                }
            }

            CurrentArchive.IsDirty = true;
            Status_Changed();
        }

        private void flipVerticallyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in listView1.SelectedItems)
            {
                EQArchiveFile _file = (EQArchiveFile)_item.Tag;
                Bitmap _image = (Bitmap)_file.GetImage();

                if (_image != null)
                {
                    _image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    _file.SetImage(_image, (_file.ImageSubformat == "") ? _file.ImageFormat : _file.ImageSubformat);

                    UpdateItem(_item, true);
                }
            }

            CurrentArchive.IsDirty = true;
            Status_Changed();
        }

        private void formMain_Activated(object sender, EventArgs e)
        {
            CheckClipboard();
        }

        private void formMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            CancelThumbnailThread();

            switch (e.CloseReason)
            {
                case CloseReason.TaskManagerClosing:
                    // Did something lock up?
                    return;
            }

            if (ConfirmDiscard())
            {
                AddToMRUs(CurrentArchive);
                
                SaveSettings();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void formMain_DragDrop(object sender, DragEventArgs e)
        {
            ShellFileGroup _newFiles = null;
            string[] _newFilenames = null;
            int _numFiles;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                _newFilenames = (string[])e.Data.GetData(DataFormats.FileDrop);
                _numFiles = _newFilenames.Length;
            }
            else if (e.Data.GetDataPresent("FileContents"))
            {
                _newFiles = ShellFileGroup.FromClipboard(e.Data);
                _numFiles = _newFiles.Files.Count;
            }
            else
            {
                return;
            }

            if (_numFiles == 1)
            {
                // Dragged a single file in. Might want to do something special.
                
                ShellFileGroup.ShellFile _newFile = (_newFiles == null) ? null : _newFiles.Files[0];
                string _newFilename = (_newFile == null) ? _newFilenames[0] : _newFile.Name;

                if (Util.IsArchive(_newFilename))
                {
                    // It's a new archive. Load it up (with confirmation if unsaved changes)!
                    
                    LoadArchive(_newFilename);

                    return;
                }

                ListViewItem _item = listView1.HitTest(listView1.PointToClient(new Point(e.X, e.Y))).Item;

                if (_item != null)
                {
                    // User dragged a new file onto an existing item. If both files are textures, or otherwise
                    // have the same extension, they may be wanting to replace the old file with the new one.

                    formReplaceDialog _confirm;

                    if (System.IO.Path.GetExtension(_item.Text).Equals(System.IO.Path.GetExtension(_newFilename), StringComparison.CurrentCultureIgnoreCase) ||
                        (Util.IsImage(_item.Text) && Util.IsImage(_newFilename)))
                    {
                        EQArchiveFile _old = (EQArchiveFile)_item.Tag;
                        EQArchiveFile _new;

                        if (_newFile != null)
                        {
                            _new = new EQArchiveFile(_newFile.Name, _newFile.Contents);
                        }
                        else
                        {
                            _new = new EQArchiveFile(_newFilename);
                        }

                        if (_new.GetImage() == null)
                        {
                            if (Util.IsImage(_new.Filename))
                            {
                                _new.SetThumbnail(itemThumbsLarge.Images[1]);
                            }
                            else if (FileTypes.ContainsKey(System.IO.Path.GetExtension(_new.Filename).ToLower()))
                            {
                                _new.SetThumbnail(itemThumbsLarge.Images[2]);
                            }
                            else
                            {
                                _new.SetThumbnail(itemThumbsLarge.Images[0]);
                            }
                        }
						else
						{
							_new.SetThumbnail(GetSquareImage(_new.GetImage()));
						}

                        _confirm = new formReplaceDialog();
                        _confirm.OldFile = _old;
                        _confirm.NewFile = _new;

                        switch (_confirm.ShowDialog(this))
                        {
                            case DialogResult.Yes:
                                ReplaceFile(_item, _new);
                                Status_Changed(true);
                                return;
                            case DialogResult.No:
                                // Separate file
                                if (CurrentArchive == null)
                                {
                                    LoadArchive("");
                                    ImportFile(_new);
                                    return;
                                }

                                // We'll have to change the filename to avoid overwriting.
                                string _newName = _new.Filename;

                                while (CurrentArchive.FindFileOrSimilarImage(_newName) != null)
                                {
                                    _newName = System.IO.Path.GetFileNameWithoutExtension(_newName) + " (New)" + System.IO.Path.GetExtension(_newName);
                                }

                                ImportFileAs(_new.Filename, _new.GetContents(), _newName);
                                Status_Changed(true);
                                return;
                            case DialogResult.Cancel:
                                // Ignore new file.
                                return;
                        }
                    }    
                }
            }

            if (_newFiles == null)
            {
                ImportFiles(_newFilenames);
            }
            else
            {
                ImportFiles(_newFiles);
            }

            Status_Changed(true);
        }

        private void formMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileContents") ||
                e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void formMain_KeyDown(object sender, KeyEventArgs e)
        {
            ControlKeyDown = e.Control;
        }

        private void formMain_KeyUp(object sender, KeyEventArgs e)
        {
            ControlKeyDown = e.Control;
        }

        private void formMain_Load(object sender, EventArgs e)
        {
            LoadSettings();

            Status_Changed();

            UpdateMRUs();

            string[] _args = Environment.GetCommandLineArgs();

            if (_args.Length > 1)
            {
                string[] _noMe = new string[_args.Length - 1];
                
                for (int _a = 0; _a < _noMe.Length; _a++)
                {
                    _noMe[_a] = _args[_a + 1];
                }

                ImportFiles(_noMe);
            }
        }

        private void importFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dialogImportFiles.ShowDialog(this) == DialogResult.OK)
            {
                ImportFiles(dialogImportFiles.FileNames);
            }
        }

        private void invertSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in listView1.Items)
            {
                _item.Selected = !_item.Selected;
            }

            Selection_Changed();
        }

        private void largeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon;
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if ((e.Label != null) && (e.Label != ""))
            {
                ListViewItem _item = listView1.Items[e.Item];

                string _newName = e.Label;

                EQArchiveFile _thisFile = (EQArchiveFile)_item.Tag;
                EQArchiveFile _existing = CurrentArchive.FindFileOrSimilarImage(_newName);
                
                if (_existing == null)
                {
                    _thisFile.Filename = _newName;

                    CurrentArchive.IsDirty = true;
                }
                else
                {
                    e.CancelEdit = true;

                    if ((!Settings.ConfirmRenameOverwrite) ||
                        (MessageBox.Show(this, "There is a " + (Util.IsImage(_existing.Filename) ? "texture" : "file") + " already in this archive named '" + _existing.Filename + "'.\n\nDo you wish to overwrite the existing file with this one?", "Confirm File Overwrite", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes))
                    {
                        DeleteItem(listView1.Items[_existing.Filename.ToLower()]);

                        _newName = _existing.Filename;

                        _item.Text = _newName;
                        _item.Name = _newName.ToLower();
                        _thisFile.Filename = _newName;

                        CurrentArchive.IsDirty = true;
                    }
                }
            }
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (NotInDebugger())
            {
                ShellFileGroup _files = new ShellFileGroup();

                foreach (ListViewItem _item in listView1.SelectedItems)
                {
                    EQArchiveFile _file = ((EQArchiveFile)_item.Tag).AsFormat(Settings.ExportFormat, true);

                    _files.Add(_file.Filename, _file.GetContents());
                }

                listView1.DoDragDrop(_files.GetDataObject(DragDropEffects.Copy), DragDropEffects.Copy);
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Status_Changed();
        }

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (listView1.Items.Count > 0))
            {
                listViewContextMenuItem.Checked = (listView1.View == View.List);
                detailsViewContextMenuItem.Checked = (listView1.View == View.Details);
                tilesViewContextMenuItem.Checked = (listView1.View == View.Tile);
                galleryViewContextMenuItem.Checked = (listView1.View == View.LargeIcon);
                
                contextMenuItems.Show(Cursor.Position);
            }
        }

        private void listView1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ControlKeyDown)
            {
                if (e.Delta > 0)
                {
                    switch (listView1.View)
                    {
                        case View.List:
                            listView1.View = View.Details;
                            break;
                        case View.Details:
                            listView1.View = View.Tile;
                            break;
                        case View.Tile:
                            listView1.View = View.LargeIcon;
                            break;
                        case View.LargeIcon:
                            break;
                    }
                }
                else if (e.Delta < 0)
                {
                    switch (listView1.View)
                    {
                        case View.List:
                            break;
                        case View.Details:
                            listView1.View = View.List;
                            break;
                        case View.Tile:
                            listView1.View = View.Details;
                            break;
                        case View.LargeIcon:
                            listView1.View = View.Tile;
                            break;
                    }
                }
            }
        }

        private void listViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List;
        }

        private void mruToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int _mru = int.Parse(sender.ToString().Substring(1, 1)) - 1;

            LoadArchive(Settings.MRUs[_mru]);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadArchive("");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dialogOpenArchive.ShowDialog(this) == DialogResult.OK)
            {
                LoadArchive(dialogOpenArchive.FileName);
            }

            dialogOpenArchive.FileName = System.IO.Path.GetFileName(dialogOpenArchive.FileName);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsData("FileContents"))
            {
                // Virtual files copied to clipboard

                ImportFiles(ShellFileGroup.FromClipboard(Clipboard.GetDataObject()));
            }
            else if (Clipboard.ContainsFileDropList())
            {
                // List of filenames to import from the file system

                ImportFiles((string[])Clipboard.GetData(DataFormats.FileDrop));
            }
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            formPreferences _prefs = new formPreferences();

            _prefs.ShowDialog(this);

            if (Settings.Changed)
            {
                SaveSettings();

                UpdateMRUs();
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.LabelEdit)
            {
                listView1.SelectedItems[0].BeginEdit();
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dialogReplaceFile.ShowDialog(this) == DialogResult.OK)
            {
                ReplaceFile(listView1.SelectedItems[0], new EQArchiveFile(dialogReplaceFile.FileName));
            }

            dialogReplaceFile.FileName = System.IO.Path.GetFileName(dialogReplaceFile.FileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentArchive != null)
            {
                if (dialogSaveArchive.ShowDialog(this) == DialogResult.OK)
                {
                    bool _tryAgain = true;

                    while (_tryAgain)
                    {
                        if (CurrentArchive.Save(dialogSaveArchive.FileName) == Result.OK)
                        {
                            Status_Changed();
                            _tryAgain = false;
                        }
                        else
                        {
                            _tryAgain = (MessageBox.Show(this,
                                            "Unable to save " + CurrentArchive.Filename + ". Please make sure it's not currently in use.\n\nTry again?",
                                            "Archive Save Denied", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes);
                        }
                    }
                }

                dialogSaveArchive.FileName = System.IO.Path.GetFileName(dialogSaveArchive.FileName);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentArchive == null)
            {
                MessageBox.Show("Error: No archive to save.");
            }
            else if (CurrentArchive.Filename == "(Untitled)")
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                bool _tryAgain = true;

                while (_tryAgain)
                {
                    if (CurrentArchive.Save() == Result.OK)
                    {
                        Status_Changed();
                        _tryAgain = false;
                    }
                    else
                    {
                        _tryAgain = (MessageBox.Show(this, 
                                        "Unable to save " + CurrentArchive.Filename + ". Please make sure it's not currently in use.\n\nTry again?",
                                        "Archive Save Denied", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes);
                    }
                }
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in listView1.Items)
            {
                _item.Selected = true;
            }

            Selection_Changed();
        }

        private void selectNoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem _item in listView1.Items)
            {
                _item.Selected = false;
            }

            Selection_Changed();
        }

        private void smallIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
        }

        private void threadDecompress_DoWork(object sender, DoWorkEventArgs e)
        {
            ThumbnailPerf[2] = new System.Diagnostics.Stopwatch();
            ThumbnailPerf[2].Start();

            EQArchiveFile[] _files = (EQArchiveFile[])((EQArchiveFile[])e.Argument).Clone();
            int _fileCount = _files.Length;

            for (int _fileNum = 0; _fileNum < _fileCount; _fileNum++)
            {
				if (Util.IsImage(_files[_fileNum].Filename))
				{
					_files[_fileNum].GetContents();

					if (threadDecompress.CancellationPending)
					{
						ThumbnailPerf[2].Stop();
						return;
					}

					DecompressMutex.WaitOne();
					DecompressQueue.Add(_fileNum);
					DecompressMutex.ReleaseMutex();
				}
            }

            ThumbnailPerf[2].Stop();
        }

        private void threadListView_DoWork(object sender, DoWorkEventArgs e)
        {
            ThumbnailPerf[0] = new System.Diagnostics.Stopwatch();
            ThumbnailPerf[0].Start();

            if ((CurrentArchive == null) || (CurrentArchive.Files.Count < 1))
            {
                ThumbnailPerf[0].Stop();
                return;
            }

            int _last = CurrentArchive.Files.Count - 1;
            EQArchiveFile[] _files = new EQArchiveFile[_last + 1];
            CurrentArchive.Files.Values.CopyTo(_files, 0);

			ListViewQueue.Clear();
			ThumbnailQueue.Clear();
            DecompressQueue.Clear();

			ThumbnailsLoaded = false;

			threadThumbnails.RunWorkerAsync();

            int _thumbIndex = 0;

            do
            {
                Thread.Sleep(1);

                ThumbnailMutex.WaitOne();
                _thumbIndex = (ThumbnailQueue.Count > 0) ? ThumbnailQueue[0] : -1;
                ThumbnailMutex.ReleaseMutex();

                if ((_thumbIndex >= 0) && (CurrentArchive != null))
                {
                    EQArchiveFile _file = _files[_thumbIndex];

                    Image _image = _file.GetImage();

					ListViewQueue.Enqueue(_thumbIndex);

                    try
                    {
                        ThumbnailMutex.WaitOne();
                        ThumbnailQueue.RemoveAt(0);
                        ThumbnailMutex.ReleaseMutex();
                    }
                    catch { }
                }
            } while (!threadListView.CancellationPending && ((ThumbnailQueue.Count > 0) || threadThumbnails.IsBusy));

            if (threadListView.CancellationPending)
            {
                threadThumbnails.CancelAsync();

                while (threadThumbnails.IsBusy)
                {
                    Thread.Sleep(1);
                }
            }

			_files = null;

			ThumbnailsLoaded = true;

			ThumbnailPerf[0].Stop();
        }

        private void threadThumbnails_DoWork(object sender, DoWorkEventArgs e)
        {
            ThumbnailPerf[1] = new System.Diagnostics.Stopwatch();
            ThumbnailPerf[1].Start();
            
            int _fileCount;
            EQArchiveFile[] _files = null;

            ThumbnailMutex.WaitOne();

            if (CurrentArchive != null)
            {
                _fileCount = CurrentArchive.Files.Count;

                if (_fileCount > 0)
                {
                    _files = new EQArchiveFile[_fileCount];
                    CurrentArchive.Files.Values.CopyTo(_files, 0);
                }
            }
            else
            {
                _fileCount = 0;
            }

            ThumbnailMutex.ReleaseMutex();

            if (_fileCount < 1)
            {
                ThumbnailPerf[1].Stop();
                return;
            }

            threadDecompress.RunWorkerAsync(_files);

            int _decompressed = 0;
            int _last = (_fileCount - 1);

            do
            {
                DecompressMutex.WaitOne();
                if ((_decompressed = (DecompressQueue.Count == 0) ? -1 : DecompressQueue[0]) >= 0)
                {
                    DecompressQueue.RemoveAt(0);
                }
                DecompressMutex.ReleaseMutex();

                if (_decompressed >= 0)
                {
                    EQArchiveFile _file = _files[_decompressed];

                    Image _image = _file.GetImage();

                    _file.SetThumbnail(GetSquareImage(_image));
                    _file = null;

                    ThumbnailMutex.WaitOne();
                    ThumbnailQueue.Add(_decompressed);
                    ThumbnailMutex.ReleaseMutex();
                }
                else
                {
                    Thread.Sleep(1);
                }
			} while (!threadThumbnails.CancellationPending && ((DecompressQueue.Count > 0) || threadDecompress.IsBusy));

            if (threadThumbnails.CancellationPending)
            {
                threadDecompress.CancelAsync();

                while (threadDecompress.IsBusy)
                {
                    Thread.Sleep(1);
                }
            }

            ThumbnailPerf[1].Stop();
            
            e.Result = threadThumbnails.CancellationPending ? DialogResult.Cancel : DialogResult.OK;
        }

        private void tilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile;
        }

		private void timerThumbnail_Tick(object sender, EventArgs e)
		{
			if (ListViewQueue.Count > 0)
			{
				int _itemNum = ListViewQueue.Dequeue();

				if (listView1.Items.Count <= _itemNum)
				{
					// Thumbnail loading canceled. Abort.
					ListViewQueue.Clear();
					return;
				}

				ListViewItem _item = listView1.Items[_itemNum];
				EQArchiveFile _file = (EQArchiveFile)_item.Tag;

				Image _image = _file.GetImage();

				if (_image != null)
				{
					itemThumbsLarge.Images.Add(_file.GetThumbnail());
					itemThumbsSmall.Images.Add(_file.GetThumbnail());

					_item.ImageIndex = (itemThumbsLarge.Images.Count - 1);
					_item.SubItems[1].Text = GetFileType(_file.ImageFormat) + ((_file.ImageSubformat == null) ? "" : " " + _file.ImageSubformat);
					_item.SubItems[2].Text = _file.GetImage().Width.ToString() + "x" + _file.GetImage().Height.ToString();

					if (_item.Selected && (listView1.SelectedItems.Count == 1))
					{
						Selection_Changed();
					}
				}
				
				toolStripProgressBar1.Value = _itemNum;
			}
			
			if ((ListViewQueue.Count < 1) && ThumbnailsLoaded && toolStripProgressBar1.Visible)
			{
				// Finished.

				toolStripProgressBar1.Visible = false;
				listView1.LabelEdit = true;

				/*
				MessageBox.Show("Decompression: " + ThumbnailPerf[2].ElapsedMilliseconds.ToString() + "ms\n" +
								"Unpacking Images: " + ThumbnailPerf[1].ElapsedMilliseconds.ToString() + "ms\n" +
								"Setting thumbnails into ListView: " + ThumbnailPerf[0].ElapsedMilliseconds.ToString() + "ms");
				*/
			}
		}

		private void viewToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            listViewToolStripMenuItem.Checked = (listView1.View == View.List);
            detailsViewToolStripMenuItem.Checked = (listView1.View == View.Details);
            tilesToolStripMenuItem.Checked = (listView1.View == View.Tile);
            largeIconsToolStripMenuItem.Checked = (listView1.View == View.LargeIcon);
        }

        private void viewToolStripSplitButton_ButtonClick(object sender, EventArgs e)
        {
            viewToolStripSplitButton.ShowDropDown();
        }

        #endregion

        #region 2. Custom Methods

        private void AddToMRUs(EQArchive OldArchive)
        {
            if ((Settings.RememberMRUs > 0) && (OldArchive != null) && (OldArchive.Filename != "(Untitled)") && (OldArchive.FilePath != ""))
            {
                string _fullPath = OldArchive.FilePath + @"\" + OldArchive.Filename;

                int _maxMRU = (Settings.RememberMRUs - 1);
                int _newMRU = _maxMRU;

                for (int _mru = 0; _mru < _maxMRU; _mru++)
                {
                    if (Settings.MRUs[_mru].Equals(_fullPath, StringComparison.CurrentCultureIgnoreCase))
                    {
                        _newMRU = _mru;
                        
                        break;
                    }
                }

                for (int _i = _newMRU; _i > 0; _i--)
                {
                    Settings.MRUs[_i] = Settings.MRUs[_i - 1];
                }

                Settings.MRUs[0] = _fullPath;
            }
        }
        
        private void CancelThumbnailThread()
        {
			ListViewQueue.Clear();

			if (threadListView.IsBusy)
            {
                threadListView.CancelAsync();

                while (threadListView.IsBusy)
                {
                    Application.DoEvents();

                    Thread.Sleep(1);
                }
            }
        }
        
        public void CheckClipboard()
        {
            pasteToolStripMenuItem.Enabled = Clipboard.ContainsFileDropList() | Clipboard.ContainsData("FileContents");
            pasteToolStripButton.Enabled = pasteToolStripMenuItem.Enabled;
            pasteContextMenuItem.Enabled = pasteToolStripMenuItem.Enabled;
        }
        
        private bool ConfirmDiscard()
        {
            if ((CurrentArchive != null) && (CurrentArchive.IsDirty))
            {
                switch (MessageBox.Show(this, "You have unsaved changes in " + CurrentArchive.Filename + ".\r\n\r\nAre you sure you want to discard those changes?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation))
                {
                    case DialogResult.No:
                        return false;
                }
            }

            return true;
        }

        private bool DeleteItem(ListViewItem Item)
        {
            if (CurrentArchive.Remove((EQArchiveFile)Item.Tag) == Result.OK)
            {
                listView1.Items.Remove(Item);

                return true;
            }

            return false;
        }

        private void ExportFiles(IEnumerable Items)
        {
            int _numExported = 0;

            if (dialogExportFiles.ShowDialog(this) == DialogResult.OK)
            {
                bool _continue = true;

                foreach (ListViewItem _item in Items)
                {
                    bool _tryAgain = true;

                    while (_tryAgain && _continue)
                    {
                        string _filename = _item.Text;
                        EQArchiveFile _entry = (EQArchiveFile)_item.Tag;

                        if ((Settings.ExportFormat != "") && Util.IsImage(_filename) && !_entry.ImageFormat.Equals(Settings.ExportFormat))
                        {
                            string _prefix = System.IO.Path.GetFileNameWithoutExtension(_filename);

                            _filename = _prefix + Settings.ExportFormat.ToLower();
                        }

                        _filename = dialogExportFiles.SelectedPath + @"\" + _filename;

                        if (Settings.ConfirmExportOverwrite && File.Exists(_filename))
                        {
                            switch (MessageBox.Show(this, "The file " + System.IO.Path.GetFileName(_filename) + " already exists in the export folder.\n\nDo you wish to overwrite the existing file with the one from the archive?\n\n(This confirmation can be disabled in Preferences.)", "Export File Exists", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                            {
                                case DialogResult.No:
                                    _tryAgain = false;
                                    continue;
                                case DialogResult.Cancel:
                                    _continue = false;
                                    continue;
                            }
                        }

                        try
                        {
                            EQArchiveFile _saveEntry = _entry.AsFormat(Settings.ExportFormat, true);

                            using (BinaryWriter _file = new BinaryWriter(File.Create(_filename)))
                            {
                                _file.Write(_saveEntry.GetContents(), 0, (int)_saveEntry.Size.Uncompressed);

                                _file.Close();
                            }

                            _tryAgain = false;
                            _numExported++;
                        }
                        catch (Exception _ex)
                        {
                            switch (MessageBox.Show(this, "There was a problem exporting the following file:\n\n" + _filename + "\n\nThe error reported was:\n\n" + _ex.Message, "Problem Exporting File", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation))
                            {
                                case DialogResult.Abort:
                                    return;
                                case DialogResult.Ignore:
                                    _tryAgain = false;
                                    break;
                                case DialogResult.Retry:
                                    _tryAgain = true;
                                    break;
                            }
                        }
                    }
                }
            }

			if (_numExported > 3)
			{
				MessageBox.Show(_numExported.ToString() + " File" + ((_numExported == 1) ? "" : "s") + " Exported");
			}
        }

        private string GetFileType(string Filename)
        {
            string _ext = System.IO.Path.GetExtension(Filename).ToLower();

            string _type = "";

            if (!FileTypes.TryGetValue(_ext, out _type))
            {
                return "File";
            }

            return _type;
        }

        private Bitmap GetSquareImage(Image Image)
        {
            Bitmap _thumbnail = (Bitmap)Image;
            
            if (Image != null)
            {
                int _w = Math.Abs(Image.Width);
                int _h = Math.Abs(Image.Height);

                if (_w > _h)
                {
                    // Horizontal orientation. Normalize to square for display.

                    _thumbnail = new Bitmap(_w, _w);
					using (Graphics _blitter = Graphics.FromImage(_thumbnail))
					{
						_blitter.DrawImage(Image, 0, (_w - _h) >> 1);
					}
                }
                else if (_h > _w)
                {
                    // Vertical orientation. Normalize to square for display.

                    _thumbnail = new Bitmap(_h, _h);
					using (Graphics _blitter = Graphics.FromImage(_thumbnail))
					{
						_blitter.DrawImage(Image, (_h - _w) >> 1, 0);
					}
                }
            }

            return _thumbnail;
        }

        private int ImportFiles(ShellFileGroup ShellFiles)
        {
            int _numImported = 0;
			ArchiveLoading = true;

            if (ShellFiles != null)
            {
                if ((ShellFiles.Files.Count == 1) && Util.IsArchive(ShellFiles.Files[0].Name))
                {
                    if (File.Exists(ShellFiles.Files[0].Name))
                    {
                        LoadArchive(ShellFiles.Files[0].Name);

                        return 0;
                    }
                }
                
                foreach (ShellFileGroup.ShellFile _file in ShellFiles.Files)
                {
                    if (ImportFile(_file.Name, _file.Contents))
                    {
                        _numImported++;
                    }
                }
            }

			ArchiveLoading = false;

			if (_numImported > 0)
			{
				Status_Changed(true);
			}

			return _numImported;
        }
        private int ImportFiles(string[] FileList)
        {
            int _numImported = 0;
			ArchiveLoading = true;

            if (FileList != null)
            {
                if ((FileList.Length == 1) && Util.IsArchive(FileList[0]))
                {
                    if (File.Exists(FileList[0]))
                    {
                        LoadArchive(FileList[0]);

                        return 0;
                    }
                }

                foreach (string _filename in FileList)
                {
                    if (Util.IsArchive(_filename))
                    {
                        SpawnArchive(_filename);
                    }
                    else if (ImportFile(_filename))
                    {
                        _numImported++;
                    }
                }
            }

			ArchiveLoading = false;

			if (_numImported > 0)
			{
				Status_Changed(true);
			}

            return _numImported;
        }

        private bool ImportFileAs(string File, byte[] Contents, string NewFilename)
        {
            EQArchiveFile _newFile = new EQArchiveFile(File, Contents);
            _newFile.Filename = NewFilename;

            return ImportFile(_newFile);
        }
        private bool ImportFile(string Filename) { return ImportFile(new EQArchiveFile(Filename, Util.GetFileContents(Filename))); }
        private bool ImportFile(string Filename, byte[] Contents) { return ImportFile(new EQArchiveFile(Filename, Contents)); }
        private bool ImportFile(EQArchiveFile File) { return ImportFile(File, false); }
        private bool ImportFile(EQArchiveFile File, bool Override)
        {
            if (CurrentArchive == null)
            {
                CurrentArchive = new EQArchive();
            }

            EQArchiveFile _existing;

            if (Settings.ImportFormat != "")
            {
                _existing = CurrentArchive.FindFileOrSimilarImage(File.Filename);
            }
            else
            {
                _existing = CurrentArchive.FindFile(File.Filename);
            }

            string _newName = null;

            if (_existing != null)
            {
                _newName = _existing.Filename;

                if (!Override || Settings.ConfirmImportOverwrite)
                {
                    switch (MessageBox.Show(this, "The file " + _existing.Filename + " exists in the archive.\n\nDo you wish to overwrite this file with your new one?", "Importing File Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        case DialogResult.No:
                            _newName = File.Filename;

                            while (CurrentArchive.FindFileOrSimilarImage(_newName) != null)
                            {
                                _newName = System.IO.Path.GetFileNameWithoutExtension(_newName) + " (New)" + System.IO.Path.GetExtension(_newName);
                            }
                            break;
                    }
                }

                File.Filename = _newName;

                if (_newName == _existing.Filename) // We're replacing it.
                {
                    DeleteItem(listView1.Items[_existing.Filename.ToLower()]);
                }
            }

            if ((Settings.ImportFormat != "") && (File.GetImage() != null) && (File.ImageFormat != Settings.ImportFormat))
            {
                File = File.AsFormat(Settings.ImportFormat, (_existing == null));
            }

            bool _ok = (CurrentArchive.Add(File) == Result.OK);

			NewFile = File;
			Status_Changed(true);

            return _ok;
        }

        public void LoadArchive(string FilePath)
        {
            if (!ConfirmDiscard())
            {
                // User canceled the change in archive
                
                return;
            }

            CancelThumbnailThread();

            EQArchive _newArchive;

            if (FilePath == null)
            {
                _newArchive = null;
            }
            else if (FilePath == "")
            {
                // Requesting a new empty archive

                _newArchive = new EQArchive();
            }
            else
            {
                _newArchive = EQArchive.Load(FilePath);

                switch (_newArchive.Status)
                {
                    case Result.OK:
                        // Hey, look at that. Nothing bad happened. Awesome. Let's use it.
                        break;
                    case Result.WrongFileType:
                        MessageBox.Show(this, "The specified file was not recognized as a supported EQ Archive type:\r\n\r\n" + FilePath, "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case Result.MalformedFile:
                        MessageBox.Show(this, "The specified file was recognized as an EQ archive, but contained invalid data:\r\n\r\n" + FilePath, "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case Result.FileTooLarge:
                        MessageBox.Show(this, "The specified file is larger than the 4GB limit supported (WTF?!):\r\n\r\n" + FilePath, "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case Result.FileReadError:
                        MessageBox.Show(this, "The specified file could not be read, or may be corrupted:\r\n\r\n" + FilePath, "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case Result.FileNotFound:
                        MessageBox.Show(this, "The specified file does not exist:\r\n\r\n" + FilePath, "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    case Result.DirectoryNotFound:
                        MessageBox.Show(this, "The specified file location not exist:\r\n\r\n" + FilePath, "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    default:
                        MessageBox.Show(this, "The specified file could not be opened as an archive:\n\n" + FilePath + "\n\nThe error received was: " + _newArchive.Status.ToString(), "Problem Opening Archive", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                }
            }

            AddToMRUs(CurrentArchive);
            
            CurrentArchive = _newArchive;
            ArchiveChanged = true;

            Status_Changed();
            UpdateMRUs();
        }

        public void LoadSettings()
        {
            Settings.Load();
            
            dialogOpenArchive.InitialDirectory = Settings.LastFolder_OpenArchive;
            dialogSaveArchive.InitialDirectory = Settings.LastFolder_SaveAsArchive;
            dialogImportFiles.InitialDirectory = Settings.LastFolder_ImportFiles;
            dialogExportFiles.SelectedPath = Settings.LastFolder_ExportFiles;
            dialogReplaceFile.InitialDirectory = Settings.LastFolder_ReplaceFile;

            ViewMode_Restore();
        }

        public bool NotInDebugger()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                MessageBox.Show(this,
                    "Unfortunately, the Windows Shell will not properly transfer Virtual Files with applications being run within the Visual Studio debugger.\n\n" +
                    "Dragging files -out- of an archive and cutting/copying files -within- an archive will not function with the faulty behavior.\n\n" +
                    "Dragging or pasting files -into- an archive -from- Windows will work, as this does not involve Shell Virtual Files.\n\n" +
                    "I'm sure Microsoft is -very- sorry for the inconvenience.",
                    "Virtual Files Broken within Debugger", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return false;
            }

            return true;
        }
        
        public void ReleaseCutItems()
        {
            if (ItemsCutToClipboard != null)
            {
                foreach (ListViewItem _item in ItemsCutToClipboard)
                {
                    try
                    {
                        _item.ForeColor = SystemColors.WindowText;
                    }
                    catch { } // Maybe it was deleted.
                }

                ItemsCutToClipboard = null;
            }
        }

        public bool ReplaceFile(string Filename, EQArchiveFile NewFile)
        {
            try
            {
                return ReplaceFile(listView1.Items[Filename.ToLower()], NewFile);
            }
            catch
            {
                return false;
            }
        }
        public bool ReplaceFile(ListViewItem OldItem, EQArchiveFile NewFile)
        {
            if ((OldItem == null) || (NewFile == null))
            {
                return false;
            }

            NewFile.Filename = OldItem.Text;
            
            return ImportFile(NewFile, true);
        }
        
        public void SaveSettings()
        {
            Settings.LastFolder_OpenArchive = dialogOpenArchive.InitialDirectory;
            Settings.LastFolder_SaveAsArchive = dialogSaveArchive.InitialDirectory;
            Settings.LastFolder_ImportFiles = dialogImportFiles.InitialDirectory;
            Settings.LastFolder_ExportFiles = dialogExportFiles.SelectedPath;
            Settings.LastFolder_ReplaceFile = dialogReplaceFile.InitialDirectory;

            ViewMode_Changed();
            
            Settings.Save();
        }

        public void Selection_Changed()
        {
            bool _selected = (listView1.SelectedItems.Count > 0);

            cutToolStripMenuItem.Enabled = _selected;
            cutToolStripButton.Enabled = cutToolStripMenuItem.Enabled;
            cutContextMenuItem.Visible = _selected;
            copyToolStripMenuItem.Enabled = _selected;
            copyToolStripButton.Enabled = copyToolStripMenuItem.Enabled;
            copyContextMenuItem.Visible = _selected;
            deleteToolStripMenuItem.Enabled = _selected;
            deleteToolStripButton.Enabled = deleteToolStripMenuItem.Enabled;
            deleteContextMenuItem.Visible = _selected;
            selectAllToolStripMenuItem.Enabled = (listView1.Items.Count > 0);
            selectAllToolStripButton.Enabled = selectAllToolStripMenuItem.Enabled;
            invertSelectionToolStripMenuItem.Enabled = (listView1.Items.Count > 0);
            invertSelectionToolStripButton.Enabled = invertSelectionToolStripMenuItem.Enabled;
            selectNoneToolStripMenuItem.Enabled = (listView1.Items.Count > 0);
            selectNoneToolStripButton.Enabled = selectNoneToolStripMenuItem.Enabled;
            viewToolStripMenuItem.Enabled = (listView1.Items.Count > 0);
            viewToolStripSplitButton.Enabled = viewToolStripMenuItem.Enabled;
            exportFilesToolStripMenuItem.Enabled = _selected;
            exportFileToolStripButton.Enabled = exportFilesToolStripMenuItem.Enabled;
            exportFilesToolStripMenuItem.Text = (listView1.SelectedItems.Count == 1) ? "&Export File..." : "&Export Files...";
            exportFileToolStripButton.Text = (listView1.SelectedItems.Count == 1) ? "Export File..." : "Export Files...";
            exportContextMenuItem.Visible = _selected;
            exportAllToolStripMenuItem.Enabled = ((CurrentArchive != null) && (CurrentArchive.Files.Count > 0));
            exportAllToolStripButton.Enabled = exportAllToolStripMenuItem.Enabled;
            renameToolStripMenuItem.Enabled = (listView1.SelectedItems.Count == 1);
            renameToolStripButton.Enabled = renameToolStripMenuItem.Enabled;
            renameContextMenuItem.Visible = renameToolStripMenuItem.Enabled;

            replaceToolStripMenuItem.Enabled = renameToolStripMenuItem.Enabled;
            replaceToolStripButton.Enabled = replaceToolStripMenuItem.Enabled;
            replaceContextMenuItem.Visible = replaceToolStripMenuItem.Enabled;

            bool _imageSelected = false;

            foreach (ListViewItem _item in listView1.SelectedItems)
            {
                if (((EQArchiveFile)_item.Tag).GetImage() != null)
                {
                    _imageSelected = true;

                    break;
                }
            }

            flipHorizontallyToolStripMenuItem.Enabled = _imageSelected;
            flipHorizontallyToolStripButton.Enabled = flipHorizontallyToolStripMenuItem.Enabled;
            flipHorizontalContextMenuItem.Visible = _imageSelected;
            flipVerticallyToolStripMenuItem.Enabled = _imageSelected;
            flipVerticallyToolStripButton.Enabled = flipVerticallyToolStripMenuItem.Enabled;
            flipVerticalContextMenuItem.Visible = _imageSelected;
            sepFlipContextMenuItem.Visible = _imageSelected;

            if (CurrentArchive == null)
            {
                toolStripStatusLabelItemDetails.Text = ""; // "No archive loaded";
            }
            else
            {
                switch (listView1.SelectedItems.Count)
                {
                    case 0:
                        toolStripStatusLabelItemDetails.Text = ""; //"No item selected";
                        break;
                    case 1:
                        ListViewItem _item = listView1.SelectedItems[0];
                        toolStripStatusLabelItemDetails.Text = _item.Text + " - " + _item.SubItems[2].Text + ((_item.SubItems[2].Text.Length != 0) ? " " : "") + _item.SubItems[1].Text + ", " + _item.SubItems[3].Text + "/" + _item.SubItems[4].Text + " bytes";
                        string _ftype;
                        if (Util.IsImage(_item.Text))
                        {
                            renameToolStripButton.Image = itemIcons.Images[3];
                            renameToolStripButton.Text = "Rename Texture (F2)";
                        }
                        else if ((_ftype = GetFileType(_item.Text)) == "File")
                        {
                            renameToolStripButton.Image = itemIcons.Images[5];
                            renameToolStripButton.Text = "Rename File (F2)";
                        }
                        else
                        {
                            renameToolStripButton.Image = itemIcons.Images[4];
                            renameToolStripButton.Text = "Rename " + _ftype + " (F2)";
                        }
                        break;
                    default:
                        toolStripStatusLabelItemDetails.Text = listView1.SelectedItems.Count + " items selected";
                        break;
                }
            }
        }

        public void SpawnArchive(string Filename)
        {
            if ((CurrentArchive == null) || ((CurrentArchive.Filename == "(Untitled)") && !CurrentArchive.IsDirty))
            {
                LoadArchive(Filename);

                return;
            }

            try
            {
                System.Diagnostics.Process _spawn = new System.Diagnostics.Process();

                _spawn.StartInfo.UseShellExecute = false;
                _spawn.StartInfo.FileName = Environment.GetCommandLineArgs()[0].Replace(".vshost.exe", ".exe");
                _spawn.StartInfo.Arguments = Filename;
                _spawn.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                _spawn.StartInfo.CreateNoWindow = false;

                _spawn.Start();
            }
            catch { }
        }

        public void Status_Changed() { Status_Changed(false); }
        public void Status_Changed(bool ForceListReload)
        {
			if (ArchiveLoading)
			{
				ListViewItem _item = new ListViewItem();

				_item.Tag = NewFile;
				UpdateItem(_item, false);

				listView1.Items.Add(_item);

				Application.DoEvents();
			}
			else
			{
				saveToolStripMenuItem.Enabled = ((CurrentArchive != null) && (CurrentArchive.IsDirty) && (!CurrentArchive.Filename.Equals("(Untitled)")));
				saveToolStripButton.Enabled = saveToolStripMenuItem.Enabled;
				saveAsToolStripMenuItem.Enabled = (CurrentArchive != null);
				saveAsToolStripButton.Enabled = saveAsToolStripMenuItem.Enabled;
				importFileToolStripMenuItem.Enabled = (CurrentArchive != null);
				importFileToolStripButton.Enabled = importFileToolStripMenuItem.Enabled;

				if (ForceListReload || (LastArchive != CurrentArchive) || ArchiveChanged)
				{
					ViewMode_Changed();

					CancelThumbnailThread();

					listView1.Items.Clear();

					while (itemThumbsLarge.Images.Count > 3)
					{
						// Clear all but our default built-in icons

						itemThumbsLarge.Images.RemoveAt(3);
						itemThumbsSmall.Images.RemoveAt(3);
					}

					if (CurrentArchive != null)
					{
						foreach (EQArchiveFile _file in CurrentArchive.Files.Values)
						{
							ListViewItem _item = new ListViewItem();

							_item.Tag = _file;
							UpdateItem(_item, false);

							listView1.Items.Add(_item);
						}
					}

					listView1.Enabled = (CurrentArchive != null);

					UpdateMRUs();

					LastArchive = CurrentArchive;
					ArchiveChanged = false;
					listView1.LabelEdit = false;

					if (CurrentArchive != null)
					{
						toolStripProgressBar1.Value = 0;
						toolStripProgressBar1.Maximum = CurrentArchive.Files.Count;
						ThumbnailsLoaded = false;
						toolStripProgressBar1.Visible = true;
					}

					Application.DoEvents();

					ViewMode_Restore();

					threadListView.RunWorkerAsync();
				}
			}

            if (CurrentArchive == null)
            {
                this.Text = Application.ProductName + " " + VersionNumber;
                toolStripStatusLabelFileCount.Text = ""; // "Files: 0";
                toolStripStatusLabelArchiveSize.Text = ""; // "Size on Disk: 0";
            }
            else
            {
                this.Text = CurrentArchive.Filename + (CurrentArchive.IsDirty ? "* - " : " - ") + Application.ProductName + " " + VersionNumber;
                toolStripStatusLabelFileCount.Text = "Files: " + CurrentArchive.Files.Count;
                toolStripStatusLabelArchiveSize.Text = "Size on Disk: " + CurrentArchive.SizeOnDisk.ToString("###,###,###,##0");
            }

			if (!ArchiveLoading)
			{
				Selection_Changed();
			}
        }

        public void UpdateItem(ListViewItem Item, bool WaitForThumbnail)
        {
            EQArchiveFile _file = (EQArchiveFile)Item.Tag;

            string _fileType = GetFileType(_file.Filename);

            if (Util.IsImage(_file.Filename))
            {
                Item.ImageIndex = 1;
            }
            else if (_fileType.Equals("File"))
            {
                Item.ImageIndex = 0;
            }
            else
            {
                Item.ImageIndex = 2;
            }

            Item.Name = _file.Filename.ToLower();
            Item.Text = _file.Filename;
            Item.Tag = _file;
            
            while (Item.SubItems.Count < 5)
            {
                Item.SubItems.Add("");
            }
            
            Item.SubItems[1].Text = _fileType;
            Item.SubItems[2].Text = "";
            Item.SubItems[3].Text = _file.Size.Compressed.ToString();
            Item.SubItems[4].Text = _file.Size.Uncompressed.ToString();

            if (WaitForThumbnail && (_file.GetImage() != null))
            {
                itemThumbsLarge.Images.Add(_file.GetThumbnail());
                itemThumbsSmall.Images.Add(_file.GetThumbnail());

                Item.ImageIndex = (itemThumbsLarge.Images.Count - 1);
                Item.SubItems[1].Text = GetFileType(_file.ImageFormat) + ((_file.ImageSubformat == null) ? "" : " " + _file.ImageSubformat);
                Item.SubItems[2].Text = _file.GetImage().Width.ToString() + "x" + _file.GetImage().Height.ToString();

                if (Item.Selected && (listView1.SelectedItems.Count == 1))
                {
                    Selection_Changed();
                }
            }
        }
        
        public void UpdateMRUs() { UpdateMRUs(-1); }
        public void UpdateMRUs(int MRU)
        {
            if ((Settings.RememberMRUs == 0) || (Settings.MRUs[0].Length == 0))
            {
                // No archive history remembering, or there are none yet remembered

                recentToolStripMenuItem.Enabled = false;
                recentSepStripMenuItem.Visible = false;
            }
            else if (MRU == -1)
            {
                // Update all MRUs

                for (int _i = 0; _i < 9; _i++)
                {
                    UpdateMRUs(_i);
                }

                recentToolStripMenuItem.Enabled = true;
                recentSepStripMenuItem.Visible = true;
            }
            else
            {
                // Update a specific MRU

                ToolStripMenuItem _mru = (ToolStripMenuItem)recentToolStripMenuItem.DropDownItems[MRU];

                if ((MRU >= Settings.RememberMRUs) || (Settings.MRUs[MRU].Length == 0))
                {
                    _mru.Visible = false;
                }
                else
                {
                    _mru.Visible = true;
                    _mru.Text = "&" + (MRU + 1).ToString() + " - " + System.IO.Path.GetFileName(Settings.MRUs[MRU]);
                    //_mru.Enabled = (CurrentArchive == null) || !Settings.MRUs[MRU].Equals(CurrentArchive.FilePath + @"\" + CurrentArchive.Filename, StringComparison.CurrentCultureIgnoreCase);
                }
            }
        }

        public void ViewMode_Changed()
        {
            switch (listView1.View)
            {
                case View.Tile:
                    Settings.ViewMode = "Tiles";
                    break;
                case View.Details:
                    Settings.ViewMode = "Details";
                    break;
                case View.LargeIcon:
                    Settings.ViewMode = "Thumbnails";
                    break;
                default:
                    Settings.ViewMode = "List";
                    break;
            }
        }

        public void ViewMode_Restore()
        {
            listView1.View = View.SmallIcon;

            switch (Settings.ViewMode)
            {
                case "Tiles":
                    listView1.View = View.Tile;
                    break;
                case "Details":
                    listView1.View = View.Details;
                    break;
                case "Thumbnails":
                    listView1.View = View.LargeIcon;
                    break;
                default:
                    listView1.View = View.List;
                    break;
            }
        }


        #endregion
    }
}
