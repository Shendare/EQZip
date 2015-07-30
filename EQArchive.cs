/*
 *  EQArchive.cs - Classes for reading and writing compressed PFS-format archives for EVerQuest
 *  
 *  By Shendare (Jon D. Jackson)
 * 
 *  The PFS file format parsing in use for the EverQuest archives was determined primarily from examination
 *  of the Delphi code in S3DSpy, by Windcatcher, without whom this project would be impossible.
 *  
 *  http://sourceforge.net/projects/eqemulator/files/OpenZone/S3DSpy%201.3/ 
 * 
 *  Portions of this code not covered by another author's or entity's copyright are released under
 *  the Creative Commons Zero (CC0) public domain license.
 *  
 *  To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and
 *  related or neighboring rights to this EQArchive class. This work is published from: The United States. 
 *  
 *  You may copy, modify, and distribute the work, even for commercial purposes, without asking permission.
 * 
 *  For more information, read the CC0 summary and full legal text here:
 *  
 *  https://creativecommons.org/publicdomain/zero/1.0/
 * 
 */

/*
 * It's a little strange that there is a separate EQArchive class and PFSFormat class to handle EQ package
 * files. When I began the project, I was under the mistaken impression that S3D and EQG files used different
 * file formats for storing their contents, so I had a PFSArchives.cs and an EQGArchives.cs both tied to
 * EQArchive.cs. When I learned that both used the same format, I removed EQGArchives.cs, but because the
 * remaining two classes are working fine together, I have not merged them at this point.
 * 
 * - Shendare (Jon D. Jackson)
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Paloma;
using DDS;

namespace EQ_Zip
{
    public class EQArchive
    {
        static EQArchive()
        {
            // Build Filename CRC Table

            for (UInt32 _entry = 0; _entry < 256; _entry++)
            {
                UInt32 _crc = _entry << 24;

                for (int _round = 0; _round < 8; _round++)
                {
                    _crc = ((_crc & 0x80000000) != 0) ? ((_crc << 1) ^ 0x04C11DB7) : (_crc << 1);
                }

                _FilenameCRCTable[_entry] = _crc;
            }
        }

        public static EQArchive Load(string FilePath) { return Load(FilePath, Util.GetFileContents(FilePath)); }
        public static EQArchive Load(string Filename, byte[] Contents)
        {
            if (Util.IsBlank(Filename) || (Contents == null))
            {
                // We got a bad filename, or the file is zero length, and thus not a PFS archive.

                return null;
            }

            Header _header = new Header();

            using (BinaryReader _input = new BinaryReader(new MemoryStream(Contents)))
            {
                try
                {
                    //
                    //   1. Read the file header
                    //

                    _header.IndexPointer = _input.ReadUInt32();
                    _header.MagicNumber = _input.ReadUInt32();
                    _header.VersionNumber = _input.ReadUInt32();
                }
                catch
                {
                    // Too small to be a PFS archive

                    return null;
                }

                if (_header.MagicNumber != _MagicNumber)
                {
                    // Not a PFS archive

                    return null;
                }

                EQArchive _archive = new EQArchive();
                _archive.Filename = Filename;
                _archive.SizeOnDisk = (UInt32)Contents.Length;

                try
                {
                    //
                    //   2. Read Index of File Pointers and Sizes in Archive
                    //

                    _input.BaseStream.Seek(_header.IndexPointer, SeekOrigin.Begin);

                    _header.EntryCount = _input.ReadUInt32();

                    if (_header.EntryCount == 0)
                    {
                        // Empty archive...?
                        _archive.Files = new SortedList<string, EQArchiveFile>();
                    }
                    else
                    {
                        _archive.Files = new SortedList<string, EQArchiveFile>((int)_header.EntryCount);
                    }

                    // Filename directory is the "file" at the end of the archive (with the highest FilePointer)
                    EQArchiveFile _directory = null;

                    // For verification later, which is optional, but will catch a malformed/corrupted archive.
                    Dictionary<UInt32, UInt32> _filenameCRCs = new Dictionary<UInt32, UInt32>();

                    // Files in a PFS archive tend to be stored by ascending order of FilenameCRC.
                    // However, the filename directory is sorted by FilePointer
                    SortedList<UInt32, EQArchiveFile> _files = new SortedList<UInt32, EQArchiveFile>();

                    for (UInt32 _index = 0; _index < _header.EntryCount; _index++)
                    {
                        EQArchiveFile _file = new EQArchiveFile();
                        UInt32 _filenameCRC = _input.ReadUInt32();
                        _file.FilePointer = _input.ReadUInt32();
                        _file.Size.Uncompressed = _input.ReadUInt32();
                        _filenameCRCs.Add(_file.FilePointer, _filenameCRC);

                        if ((_directory == null) || (_file.FilePointer > _directory.FilePointer))
                        {
                            _directory = _file;
                        }

                        _files.Add(_file.FilePointer, _file);
                    }

                    if ((_input.BaseStream.Length - _input.BaseStream.Position) >= 9)
                    {
                        // PFS Footer

                        char[] _token = _input.ReadChars(5);

                        if (new String(_token).Equals("STEVE"))
                        {
                            // Valid Footer Token

                            _header.DateStamp = _input.ReadUInt32();
                        }
                    }

                    //
                    //   3. Read the compressed file entries (each split into compressed chunks)
                    //

                    foreach (EQArchiveFile _file in _files.Values)
                    {
                        // Seek to entry position in stream
                        _input.BaseStream.Seek(_file.FilePointer, SeekOrigin.Begin);

                        UInt32 _totalUncompressedBytes = _file.Size.Uncompressed;
                        _file.Size.Uncompressed = 0;

                        while ((_file.Size.Uncompressed < _totalUncompressedBytes) && (_input.BaseStream.Position < _input.BaseStream.Length))
                        {
                            UInt32 _blockSizeCmp = _input.ReadUInt32();
                            UInt32 _blockSizeUnc = _input.ReadUInt32();

                            // Sanity Check 1: Uncompressed data larger than what we were told?
                            if ((_blockSizeUnc + _file.Size.Uncompressed) > _totalUncompressedBytes)
                            {
                                throw new Exception();
                            }

                            // Sanity Check 2: Compressed data goes past the end of the file?
                            if ((_input.BaseStream.Position + _blockSizeCmp) > _input.BaseStream.Length)
                            {
                                throw new Exception();
                            }

                            _file.AddChunk(new EQArchiveFile.Chunk(Contents, (UInt32)_input.BaseStream.Position, _blockSizeCmp, _blockSizeUnc, true));

                            _input.BaseStream.Position += _blockSizeCmp;
                        }
                    }

                    //
                    //    4. Unpack and parse the directory of filenames from the "file" at the end of the archive (highest FilePointer)
                    //

                    // Remove directory from file entries in archive. We'll have to rebuild it when saving the archive anyway.
                    _files.Remove(_directory.FilePointer);
                    _header.EntryCount--;

                    // Load filenames from directory
                    BinaryReader _dirStream = new BinaryReader(new MemoryStream(_directory.GetContents()));

                    UInt32 _filenameCount = _dirStream.ReadUInt32();

                    if (_filenameCount > _header.EntryCount)
                    {
                        // If we somehow have more filenames than entries in the archive, ignore the glitched extras

                        _filenameCount = _header.EntryCount;
                    }

                    _archive.Files = new SortedList<string, EQArchiveFile>();

                    foreach (EQArchiveFile _file in _files.Values)
                    {
                        Int32 _len = _dirStream.ReadInt32();
                        char[] _inputname = _dirStream.ReadChars(_len);
                        UInt32 _crc = GetFilenameCRC(_inputname);

                        if (_crc != _filenameCRCs[_file.FilePointer])
                        {
                            // Filename doesn't match with what we were given in Step 2

                            throw new Exception();
                        }

                        _file.Filename = new string(_inputname, 0, _len - 1);

                        _archive.Files.Add(_file.Filename.ToLower(), _file);
                    }

                    // All entries loaded and filenames read from directory.

                    _archive.Status = Result.OK;
                }
                catch
                {
                    _archive.Status = Result.MalformedFile;
                }

                return _archive;
            }
        }

        #region Public Properties

        public string FilePath = "";
        
        public SortedList<string, EQArchiveFile> Files = new SortedList<string, EQArchiveFile>();
        
        public bool IsDirty = false;
        
        public UInt32 SizeOnDisk = 0;
        
        public Result Status = Result.NotImplemented;

        public string Filename
        {
            get
            {
                return _Filename;
            }
            set
            {
                if (Util.IsBlank(value))
                {
                    this._Filename = "(Untitled)";
                }
                else
                {
                    string _path = System.IO.Path.GetDirectoryName(value);

                    if (_path != "")
                    {
                        this.FilePath = _path;
                    }

                    this._Filename = System.IO.Path.GetFileName(value);
                }
            }
        }

        #endregion

        #region Public Methods

        public EQArchiveFile FindFile(string Filename)
        {
            try
            {
                return this.Files[System.IO.Path.GetFileName(Filename).ToLower()];
            }
            catch
            {
                return null;
            }
        }

        public EQArchiveFile FindFileOrSimilarImage(string Filename)
        {
            if (!Util.IsImage(Filename))
            {
                return this.FindFile(Filename);
            }

            Filename = System.IO.Path.GetFileName(Filename);
            string _prefix = System.IO.Path.GetFileNameWithoutExtension(Filename);

            foreach (EQArchiveFile _file in this.Files.Values)
            {
                if (Util.IsImage(_file.Filename))
                {
                    if (_prefix.Equals(System.IO.Path.GetFileNameWithoutExtension(_file.Filename), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return _file;
                    }
                }
            }

            return null;
        }

        public Result Add(string Filename, bool ReplaceSimilarImage) { return this.Add(new EQArchiveFile(Filename, Util.GetFileContents(Filename)), ReplaceSimilarImage); }
        public Result Add(string Filename, bool ReplaceSimilarImage, byte[] FileContents) { return this.Add(new EQArchiveFile(Filename, FileContents), ReplaceSimilarImage); }
        public Result Add(EQArchiveFile File) { return this.Add(File, false); }
        public Result Add(EQArchiveFile File, bool ReplaceSimilarImage)
        {
            if (File == null)
            {
                return Result.InvalidArgument;
            }

            EQArchiveFile _archiveFile;

            if (ReplaceSimilarImage)
            {
                _archiveFile = this.FindFileOrSimilarImage(Filename);
            }
            else
            {
                _archiveFile = this.FindFile(Filename);
            }

            if (_archiveFile != null)
            {
                // We already have a file that has a similar name. Import the new file AS that file, replacing it.

                this.Files.RemoveAt(this.Files.IndexOfValue(_archiveFile));

                File.Filename = _archiveFile.Filename;
            }

            this.Files[File.Filename.ToLower()] = File;

            this.IsDirty = true;

            return File.Status;
        }

        public Result Remove(EQArchiveFile File)
        {
            if (File == null)
            {
                return Result.FileNotFound;
            }

            this.Files.RemoveAt(this.Files.IndexOfValue(File));

            this.IsDirty = true;

            return Result.OK;
        }
        public Result Remove(string Filename)
        {
            return Remove(this.FindFile(Filename));
        }

        public Result Save(string Filename) { this.Filename = Filename; return Save(); }
        public Result Save()
        {
            if ((this.FilePath == "") || (this.Filename == "(Untitled)") || (this.Filename == ""))
            {
                return Result.InvalidArgument;
            }

            Result _result = Result.OK;

            try
            {
                using (BinaryWriter _file = new BinaryWriter(File.Create(this.FilePath + @"\" + this.Filename)))
                {
                    //
                    //    Step 1 - Get an order of files by filename CRC, per standard PFS archive practice.
                    //

                    SortedList<UInt32, EQArchiveFile> _filesByCRC = new SortedList<UInt32, EQArchiveFile>();

                    foreach (EQArchiveFile _entry in this.Files.Values)
                    {
                        _filesByCRC.Add(GetFilenameCRC(_entry.Filename), _entry);
                    }

                    //
                    //    Step 2 - Build the directory of filenames and compress it for adding at the end of the archive
                    //

                    EQArchiveFile _directory = new EQArchiveFile();

                    byte[] _directoryBytes = new byte[16 * 1024];

                    using (BinaryWriter _stream = new BinaryWriter(new MemoryStream(_directoryBytes)))
                    {
                        UInt32 _directorySize = 0;

                        _stream.Write((UInt32)this.Files.Count);

                        foreach (EQArchiveFile _entry in _filesByCRC.Values)
                        {
                            _stream.Write((UInt32)_entry.Filename.Length + 1);
                            foreach (char _c in _entry.Filename)
                            {
                                _stream.Write(_c);
                            }
                            _stream.Write('\0');
                        }

                        _directorySize = (UInt32)_stream.BaseStream.Position;

                        Array.Resize<byte>(ref _directoryBytes, (int)_directorySize);

                        _directory.SetContents(_directoryBytes);
                    }

                    //
                    //    Step 3 - Build the file header
                    //

                    Header _header = new Header();
                    _header.MagicNumber = _MagicNumber;
                    _header.VersionNumber = 0x00020000;

                    // a. Index Pointer must be determined. Start with the size after the header itself
                    _header.IndexPointer = 4 + 4 + 4;

                    // b. Add in the size of all of the compressed chunks and their two size values
                    foreach (EQArchiveFile _entry in this.Files.Values)
                    {
                        _header.IndexPointer += (4 + 4) * (_entry.CompressedChunks == null ? 1 : (UInt32)_entry.CompressedChunks.Count);
                        _header.IndexPointer += _entry.Size.Compressed;
                    }

                    // c. Add in the size of the compressed filename directory and its size values
                    _header.IndexPointer += (4 + 4) * (UInt32)_directory.CompressedChunks.Count + _directory.Size.Compressed;

                    //
                    //    Step 4 - Write the file Header
                    //

                    _file.Write(_header.IndexPointer);
                    _file.Write(_header.MagicNumber);
                    _file.Write(_header.VersionNumber);

                    //
                    //    Step 5 - Compressed File Chunks       
                    //
                    foreach (EQArchiveFile _entry in _filesByCRC.Values)
                    {
                        _entry.FilePointer = (UInt32)_file.BaseStream.Position;

                        foreach (EQArchiveFile.Chunk _chunk in _entry.CompressedChunks)
                        {
                            _file.Write(_chunk.Size.Compressed);
                            _file.Write(_chunk.Size.Uncompressed);
                            _file.Write(_chunk.CompressedData, 0, (int)_chunk.Size.Compressed);
                        }
                    }

                    //
                    //    Step 6 - Filename Directory compressed chunks at the end
                    //
                    _directory.FilePointer = (UInt32)_file.BaseStream.Position;

                    foreach (EQArchiveFile.Chunk _chunk in _directory.CompressedChunks)
                    {
                        _file.Write(_chunk.Size.Compressed);
                        _file.Write(_chunk.Size.Uncompressed);
                        _file.Write(_chunk.CompressedData, 0, (int)_chunk.Size.Compressed);
                    }

                    //
                    //    Step 7 - Index of File Entries
                    //
                    _file.Write((UInt32)(this.Files.Count + 1));

                    foreach (KeyValuePair<UInt32, EQArchiveFile> _kvp in _filesByCRC)
                    {
                        _file.Write(_kvp.Key);
                        _file.Write(_kvp.Value.FilePointer);
                        _file.Write(_kvp.Value.Size.Uncompressed);
                    }

                    //
                    //    Step 8 - Add filename directory to end of index
                    //

                    _file.Write(0xFFFFFFFFU);
                    _file.Write(_directory.FilePointer);
                    _file.Write(_directory.Size.Uncompressed);

                    //
                    //    Step 9 - PFS Footer
                    //

                    foreach (char _letter in _FooterToken)
                    {
                        _file.Write(_letter);
                    }

                    _file.Write(_header.DateStamp);

                    _file.Close();
                }
            }
            catch
            {
                return Result.FileWriteError;
            }

            if (_result == Result.OK)
            {
                this.IsDirty = false;
            }

            return _result;
        }

        #endregion

        #region Protected Members
        protected string _Filename = "(Untitled)";

        protected static UInt32 _MagicNumber = 0x20534650;
        protected static UInt32[] _FilenameCRCTable = new UInt32[256];

        protected static string _FooterToken = "STEVE";

        protected static UInt32 GetFilenameCRC(string Filename)
        {
            if (Filename == null)
            {
                return 0;
            }

            UInt32 _crc = 0;
            UInt32 _index;
            char _char;

            for (int _pointer = 0; _pointer <= Filename.Length; _pointer++)
            {
                if (_pointer == Filename.Length)
                {
                    _char = '\0';
                }
                else
                {
                    _char = Filename[_pointer];
                }

                _index = ((_crc >> 24) ^ _char) & 0xFF;

                _crc = ((_crc << 8) ^ _FilenameCRCTable[_index]);
            }

            return _crc;
        }
        protected static UInt32 GetFilenameCRC(char[] Filename) { return GetFilenameCRC(Filename, (Filename == null) ? 0 : Filename.Length); }
        protected static UInt32 GetFilenameCRC(char[] Filename, int Length)
        {
            UInt32 _crc = 0;
            UInt32 _index;

            if (Filename != null)
            {
                for (int _pointer = 0; _pointer < Length; _pointer++)
                {
                    _index = ((_crc >> 24) ^ Filename[_pointer]) & 0xFF;

                    _crc = ((_crc << 8) ^ _FilenameCRCTable[_index]);
                }
            }

            return _crc;
        }

        protected class Header
        {
            public UInt32 IndexPointer;
            public UInt32 MagicNumber;
            public UInt32 VersionNumber;
            public UInt32 EntryCount;

            public UInt32 DateStamp;
        }
        #endregion
    }

    public class EQArchiveFile
    {
        #region Public Properties

        public List<Chunk> CompressedChunks;

        public UInt32 FilePointer;

        public string ImageFormat;
        public string ImageSubformat;

        public Sizes Size;

        public Result Status;

        public string Filename
        {
            get { return this._Filename; }
            set { this._Filename = Util.IsBlank(value) ? "(Untitled)" : System.IO.Path.GetFileName(value); }
        }

        #endregion

        public EQArchiveFile() { }
        public EQArchiveFile(string FilePath)
        {
            if (!Util.IsBlank(Filename))
            {
                this.Filename = FilePath;
                this.SetContents(Util.GetFileContents(FilePath));
            }
        }
        public EQArchiveFile(string Filename, byte[] Contents)
        {
            if (!Util.IsBlank(Filename))
            {
                this.Filename = Filename;
                this.SetContents(Contents);
            }
        }

        #region Public Methods

        public void AddChunk(Chunk Chunk)
        {
            if (Chunk == null)
            {
                this.Status = Result.InvalidArgument;

                return;
            }

            if (this.CompressedChunks == null)
            {
                this.CompressedChunks = new List<Chunk>();
                this.Size.Compressed = 0;
                this.Size.Uncompressed = 0;
            }

            this.Size.Compressed += Chunk.Size.Compressed;
            this.Size.Uncompressed += Chunk.Size.Uncompressed;

            this.CompressedChunks.Add(Chunk);
        }

        public EQArchiveFile AsFormat(string NewFormat, bool ChangeExtension)
        {
            if ((NewFormat == null) || (NewFormat == "") || (this.GetImage() == null))
            {
                return this;
            }

            NewFormat = NewFormat.ToLower();

            if (NewFormat == "auto")
            {
                switch (GetAlphaBits())
                {
                    case 0:
                    case 1:
                        NewFormat = "16-bit";
                        break;
                    
                    case 8:
                        NewFormat = "32-bit";
                        break;
                    
                    default: // ?
                        NewFormat = "32-bit";
                        break;
                }
            }

            EQArchiveFile _newFile = null;

            if (NewFormat != this.ImageFormat)
            {
                _newFile = new EQArchiveFile();
                _newFile.Filename = this.Filename;
                _newFile.SetImage(this.GetImage(), NewFormat);
            }

            if (ChangeExtension && !System.IO.Path.GetExtension(this.Filename).Equals((NewFormat[0] == '.' ? NewFormat : ".dds"), StringComparison.CurrentCultureIgnoreCase))
            {
                // Gotta change the extension

                if (_newFile == null)
                {
                    _newFile = new EQArchiveFile();
                    _newFile.SetContents(this.GetContents());
                }

                _newFile.Filename = System.IO.Path.GetFileNameWithoutExtension(this.Filename) + (NewFormat[0] == '.' ? NewFormat : ".dds");
            }
            else
            {
                if (_newFile == null)
                {
                    _newFile = this; // Unchanged from our current contents
                }
                else
                {
                    _newFile.Filename = this.Filename;
                }
            }

            return _newFile;
        }

        public int GetAlphaBits()
        {
            if (_AlphaBits == -1)
            {
                _AlphaBits = Util.GetAlphaBits((Bitmap)this.GetImage());
            }

            return _AlphaBits;
        }
        
        public byte[] GetContents()
        {
            if (!this._IsUncompressed)
            {
                if (CompressedChunks == null)
                {
                    return null;
                }

                if (this.Size.Uncompressed == 0)
                {
                    if (CompressedChunks.Count >= 1)
                    {
                        // Somebody borked us.

                        this.Status = Result.DataError;

                        return null;
                    }

                    this._IsUncompressed = true;

                    return null; // Empty data buffer. Zero length.
                }

                try
                {
                    try
                    {
                        this._UncompressedData = new byte[this.Size.Uncompressed];
                    }
                    catch
                    {
                        this._UncompressedData = null;
                    }

                    if (this._UncompressedData == null)
                    {
                        this.Status = Result.OutOfMemory;

                        return null;
                    }

                    UInt32 _bytesSoFar = 0;

                    foreach (Chunk _chunk in this.CompressedChunks)
                    {
                        if (_chunk.DecompressTo(this._UncompressedData, _bytesSoFar) != _chunk.Size.Uncompressed)
                        {
                            this._UncompressedData = null;

                            this.Status = Result.DecompressionError;

                            return null;
                        }

                        _bytesSoFar += _chunk.Size.Uncompressed;
                    }

                    if (_bytesSoFar != this.Size.Uncompressed)
                    {
                        this._UncompressedData = null;

                        this.Status = Result.DecompressionError;

                        return null;
                    }

                    this._IsUncompressed = true;

                    this.Status = Result.OK;
                }
                catch
                {
                    this._UncompressedData = null;

                    this.Status = Result.DecompressionError;
                }
            }

            return this._UncompressedData;
        }

        public Image GetImage()
        {
            if (this._IsImageChecked)
            {
                return this._Image;
            }

            byte[] _bytes = this.GetContents();

            this.ImageSubformat = "";
            this.ImageFormat = "";
            this._AlphaBits = -1;

            if (_bytes == null)
            {
                this._IsImageChecked = true;

                this.Status = Result.OutOfMemory;

                return null;
            }

            Bitmap _loading = null;

            using (MemoryStream _stream = new MemoryStream(_bytes))
            {
                // Natively Supported Image File?
                try
                {
                    _loading = new Bitmap(_stream);

                    this.ImageFormat = System.IO.Path.GetExtension(this.Filename).ToLower();
                }
                catch
                {
                    _loading = null;

                    _stream.Seek(0, SeekOrigin.Begin);
                }

                if (_loading == null)
                {
                    // TGA image?

                    try
                    {
                        TargaImage _tga = new TargaImage(_stream);

                        _loading = _tga.Image;

                        this.ImageFormat = ".tga";
                    }
                    catch
                    {
                        _loading = null;
                    }
                }
            }

            if (_loading == null)
            {
                // DDS Texture?

                try
                {
                    DDSImage _dds = DDSImage.Load(_bytes);

                    _loading = _dds.Images[0];

                    this.ImageFormat = ".dds";
                    this.ImageSubformat = _dds.FormatName;
                }
                catch
                {
                    _loading = null;
                }
            }

            if (_loading == null)
            {
                // Unsupported file.
                this.Status = Result.WrongFileType;
            }

            this._IsImageChecked = true;

            this._Image = _loading;

            return this._Image;
        }

        public Image GetThumbnail()
        {
            if (_Thumbnail == null)
            {
                return GetImage();
            }

            return _Thumbnail;
        }

        public void SetImage(Image NewImage, string Format)
        {
            this._Image = NewImage;
            this._IsImageChecked = true;
            this._Thumbnail = null;

            Format = Format.ToLower();
            this.ImageFormat = Format;

            if (NewImage == null)
            {
                this.SetContents(null);
            }
            else
            {
                using (MemoryStream _newStream = new MemoryStream())
                {
                    switch (Format)
                    {
                        case "16-bit":
                        case "rgb16":
                        case "argb16":
                        case "dxt1":
                            if (GetAlphaBits() == 0)
                            {
                                DDSImage.Save((Bitmap)this.GetImage(), _newStream, DDSImage.CompressionMode.R5G6B5);
                                this.ImageFormat = ".dds";
                                this.ImageSubformat = "RGB16";
                            }
                            else
                            {
                                DDSImage.Save((Bitmap)this.GetImage(), _newStream, DDSImage.CompressionMode.A1R5G5B5);
                                this.ImageFormat = ".dds";
                                this.ImageSubformat = "ARGB16";
                            }
                            break;
                        case "24-bit":
                        case "rgb24":
                            DDSImage.Save((Bitmap)this.GetImage(), _newStream, DDSImage.CompressionMode.RGB24);
                            this.ImageFormat = ".dds";
                            this.ImageSubformat = "RGB24";
                            break;
                        case "32-bit":
                        case "rgb32":
                        case "dxt2":
                        case "dxt3":
                        case "dxt4":
                        case "dxt5":
                            DDSImage.Save((Bitmap)this.GetImage(), _newStream, DDSImage.CompressionMode.RGB32);
                            this.ImageFormat = ".dds";
                            this.ImageSubformat = "RGB32";
                            break;
                        case ".bmp":
                            this.GetImage().Save(_newStream, System.Drawing.Imaging.ImageFormat.Bmp);
                            break;
                        case ".png":
                            this.GetImage().Save(_newStream, System.Drawing.Imaging.ImageFormat.Png);
                            break;
                        case ".gif":
                            this.GetImage().Save(_newStream, System.Drawing.Imaging.ImageFormat.Gif);
                            break;
                        case ".tif":
                        case ".tiff":
                            this.GetImage().Save(_newStream, System.Drawing.Imaging.ImageFormat.Tiff);
                            break;
                        case ".jpg":
                        case ".jpeg":
                            this.GetImage().Save(_newStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            break;
                        default:
                            throw new Exception("Unsupported conversion format");
                    }

                    this.SetContents(_newStream.ToArray());
                }
            }
        }

        public Result SetContents(byte[] Data)
        {
            this._UncompressedData = Data;
            this.Size.Compressed = 0;
            this.Size.Uncompressed = 0;
            UInt32 _sizeUncompressed = (Data == null) ? 0 : (UInt32)Data.Length;
            this._IsUncompressed = true;
            this.CompressedChunks = new List<Chunk>();
            this._Image = null;

            if (Data == null)
            {
                this._IsImageChecked = true;

                return Result.OK;
            }

            this._IsImageChecked = false;

            // Create compressed chunks for saving into archive
            try
            {
                UInt32 _chunkSize = (_sizeUncompressed < MAX_BLOCK_SIZE) ? _sizeUncompressed : MAX_BLOCK_SIZE;
                byte[] _chunkBytes = new byte[_chunkSize];

                while (this.Size.Uncompressed < Data.Length)
                {
                    if ((Data.Length - this.Size.Uncompressed) < _chunkSize)
                    {
                        _chunkSize = ((UInt32)Data.Length - this.Size.Uncompressed);
                    }

                    Chunk _chunk = new Chunk();

                    _chunk.CompressFrom(Data, this.Size.Uncompressed, _chunkSize);

                    this.AddChunk(_chunk);
                }

                this.Status = Result.OK;
            }
            catch
            {
                this.Size.Compressed = 0;
                this.Size.Uncompressed = 0;
                this.CompressedChunks = new List<Chunk>();
                this._IsUncompressed = true;
                this._UncompressedData = null;

                this.Status = Result.DataError;
            }

            return this.Status;
        }

        public void SetThumbnail(Image Thumbnail)
        {
            this._Thumbnail = Thumbnail;
        }

        #endregion

        #region Protected Members

        protected const UInt32 MAX_BLOCK_SIZE = 8192; // Per System.IO.Compression.Deflate documentation

        protected string _Filename = "(Untitled)";

        protected Image _Image;
        protected bool _IsImageChecked;

        protected Image _Thumbnail;

        protected byte[] _UncompressedData;
        protected bool _IsUncompressed;

        protected int _AlphaBits = -1;

        #endregion

        public class Chunk
        {
            public byte[] CompressedData;
            public Sizes Size;

            public Chunk() { }

            public Chunk(byte[] Data, UInt32 Offset, UInt32 CompressedSize, UInt32 UncompressedSize, bool IsCompressed)
            {
                if (IsCompressed)
                {
                    this.CompressedData = new byte[(int)CompressedSize];
                    this.Size.Compressed = CompressedSize;
                    this.Size.Uncompressed = UncompressedSize;

                    Array.Copy(Data, (int)Offset, this.CompressedData, 0, (int)CompressedSize);
                }
                else
                {
                    CompressFrom(Data, Offset, UncompressedSize);
                }
            }

            public UInt32 CompressFrom(byte[] RawData, UInt32 Offset, UInt32 RawBytes)
            {
                using (MemoryStream _data = new MemoryStream(RawData))
                {
                    _data.Seek((int)Offset, SeekOrigin.Begin);

                    this.CompressedData = zlib.Compress(_data);
                    this.Size.Compressed = (UInt32)this.CompressedData.Length;
                    this.Size.Uncompressed = RawBytes;
                }

                return this.Size.Compressed;
            }

            public UInt32 DecompressTo(byte[] Destination, UInt32 Offset)
            {
                using (MemoryStream _compressed = new MemoryStream(this.CompressedData))
                {
                    return (UInt32)zlib.Decompress(_compressed, Destination, (int)Offset, (int)this.Size.Uncompressed);
                }
            }
        }
    }
}
