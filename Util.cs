/*
 *  Util.cs - Just some basic utility functions to make the application read and run smoothly
 *  
 *  By Shendare (Jon D. Jackson)
 * 
 *  Portions of this code not covered by another author's or entity's copyright are released under
 *  the Creative Commons Zero (CC0) public domain license.
 *  
 *  To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and
 *  related or neighboring rights to these utility classes. This work is published from: The United States. 
 *  
 *  You may copy, modify, and distribute the work, even for commercial purposes, without asking permission.
 * 
 *  For more information, read the CC0 summary and full legal text here:
 *  
 *  https://creativecommons.org/publicdomain/zero/1.0/
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace EQ_Zip
{
    public enum Result
    {
        OK,
        InvalidArgument,
        FileNotFound,
        DirectoryNotFound,
        FileTooLarge,
        FileReadError,
        FileWriteError,
        DataError,
        WrongFileType,
        NotImplemented,
        MalformedFile,
        CompressionError,
        DecompressionError,
        OutOfMemory
    }

    public struct Sizes
    {
        public UInt32 Compressed;
        public UInt32 Uncompressed;

        public override string ToString()
        {
            return Compressed.ToString() + "/" + Uncompressed.ToString();
        }
    }

    public class Util
    {
        public static List<string> ImageFiles = new List<string> { ".dds", ".bmp", ".png", ".jpeg", ".jpg", ".gif", ".tga", ".tiff", ".tif" };
        public static List<string> ArchiveFiles = new List<string>() { ".s3d", ".eqg", ".pfs", ".pak" };

        public static bool IsArchive(string Filename)
        {
            if (IsBlank(Filename))
            {
                return false;
            }

            return ArchiveFiles.Contains(System.IO.Path.GetExtension(Filename).ToLower());
        }
        
        public static bool IsImage(string Filename)
        {
            if (IsBlank(Filename))
            {
                return false;
            }

            return ImageFiles.Contains(System.IO.Path.GetExtension(Filename).ToLower());
        }

        public static bool IsBlank(string Text) { return ((Text == null) || (Text == "")); }

        public static byte[] GetFileContents(string FilePath)
        {
            if (IsBlank(FilePath) || !File.Exists(FilePath))
            {
                return null;
            }
            
            FileStream _file = null;
            byte[] _data = null;

            try
            {
                _file = File.OpenRead(FilePath);

                if ((_file.Length > 0) && (_file.Length <= Int32.MaxValue))
                {
                    _data = new byte[_file.Length];

                    if (_data != null)
                    {
                        if (_file.Read(_data, 0, _data.Length) != _data.Length)
                        {
                            _data = null;
                        }
                    }
                }
            }
            catch
            {
                _data = null;
            }

            if (_file != null)
            {
                _file.Close();
            }

            return _data;
        }
    }

    public class zlib
    {
        public static int Decompress(Stream Source, byte[] Destination, int Offset)
        {
            return Decompress(Source, Destination, Offset, (Destination == null) ? 0 : (Destination.Length - Offset));
        }
        public static int Decompress(Stream Source, byte[] Destination, int Offset, int BlockSize)
        {
            if (Source.CanSeek)
            {
                Source.Seek(2, System.IO.SeekOrigin.Current);
            }
            else
            {
                Source.ReadByte();
                Source.ReadByte();
            }

            int _bytesRead = 0;

            using (DeflateStream _inflater = new DeflateStream(Source, CompressionMode.Decompress, true))
            {
                _bytesRead = _inflater.Read(Destination, Offset, BlockSize);
                _inflater.Close();
            }

            return _bytesRead;
        }

        public static byte[] Compress(Stream Source) { return Compress(Source, 8192); }
        public static byte[] Compress(Stream Source, int BlockSize)
        {
            byte[] _chunk;

            using (MemoryStream _compressed = new MemoryStream())
            {
                _compressed.WriteByte(0x58);
                _compressed.WriteByte(0x85);

                byte[] _data = new byte[BlockSize];

                int _dataSize = Source.Read(_data, 0, BlockSize);

                using (DeflateStream _deflater = new DeflateStream(_compressed, CompressionMode.Compress, true))
                {
                    _deflater.Write(_data, 0, _dataSize);
                    _deflater.Close();
                }

                uint _adler32 = Adler32(_data, 0, _dataSize);

                _compressed.WriteByte((byte)(_adler32 >> 24));
                _compressed.WriteByte((byte)(_adler32 >> 16));
                _compressed.WriteByte((byte)(_adler32 >> 8));
                _compressed.WriteByte((byte)_adler32);

                _compressed.Flush();

                _chunk = _compressed.ToArray();
            }

            return _chunk;
        }

        public static uint Adler32(byte[] Data, int Offset, int Length)
        {
            uint _adler32a = 1;
            uint _adler32b = 0;

            if (Data != null)
            {
                while (Length > 0)
                {
                    int _rounds = (Length < 3800) ? Length : 3800;
                    Length -= _rounds;

                    while (_rounds-- > 0)
                    {
                        _adler32a += Data[Offset++];
                        _adler32b += _adler32a;
                    }

                    _adler32a %= 65521;
                    _adler32b %= 65521;
                }
            }

            return (_adler32b << 16) | _adler32a;
        }
    }
}
