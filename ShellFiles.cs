/*
 *  ShellFiles.cs - Helper classes for working with the VirtualFileDataObject class
 *  
 *  By Shendare (Jon D. Jackson)
 * 
 *  The VirtualFileDataObject class can be found in VirtualFileDataObject.cs. It was made by
 *  David Anson at Microsoft, and released under the MIT License. https://dlaa.me/blog/post/9923072 
 * 
 *  Portions of this code not covered by another author's or entity's copyright are released under
 *  the Creative Commons Zero (CC0) public domain license.
 *  
 *  To the extent possible under law, Shendare (Jon D. Jackson) has waived all copyright and
 *  related or neighboring rights to this ShellFile class. This work is published from: The United States. 
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

using Delay;

namespace EQ_Zip
{
    public class ShellFileGroup
    {
        public delegate void ActionEvent(ShellFileGroup FileGroup, DragDropEffects Effect);

        public event ActionEvent ActionStarted;
        public event ActionEvent ActionCompleted;
        
        public List<string> Filenames = new List<string>();
        public List<ShellFile> Files = new List<ShellFile>();

        protected IntPtr _hGlobal = IntPtr.Zero;

        public ShellFileGroup() { }

        public static ShellFileGroup FromClipboard(System.Windows.Forms.IDataObject FromDataObject)
        {
            ShellFileGroup _group = new ShellFileGroup();

            System.Runtime.InteropServices.ComTypes.IDataObject DataObject = (System.Runtime.InteropServices.ComTypes.IDataObject)FromDataObject;

            if (DataObject != null)
            {
                NativeMethods.FILEDESCRIPTOR[] _files = NativeMethods.GetFileDescriptors(DataObject, out _group._hGlobal);

                for (int _f = 0; _f < _files.Length; _f++)
                {
                    _group.Add(_files[_f].cFileName, NativeMethods.GetFileContents(DataObject, _f, _files[_f].nFileSizeLow));
                }

                GlobalUnlock(_group._hGlobal);
                _group._hGlobal = IntPtr.Zero;
            }

            return _group;
        }

        public void Add(string Filename, byte[] Contents) { Add(Filename, Contents, (Contents == null) ? 0 : Contents.Length); }
        public void Add(string Filename, byte[] Contents, int Size)
        {
            ShellFile _file = new ShellFile();

            _file.Name = Filename;
            _file.Size = (Contents == null) ? 0 : Size;

            if ((Contents != null) && (Size != Contents.Length))
            {
                _file.Contents = new byte[Size];
                Array.Copy(Contents, _file.Contents, Size);
            }
            else
            {
                _file.Contents = Contents;
            }

            Add(_file);
        }
        
        public void Add(ShellFile File)
        {
            if (File != null)
            {
                Files.Add(File);
                Filenames.Add(File.Name.ToLower());
            }
        }
        
        public void Clear()
        {
            Filenames.Clear();
            Files.Clear();
        }

        public bool Contains(string Filename)
        {
            if ((Filename == null) || (Filename == ""))
            {
                return false;
            }

            return Filenames.Contains(System.IO.Path.GetFileName(Filename).ToLower());
        }

        public VirtualFileDataObject GetDataObject(DragDropEffects ActionToTake)
        {
            List<VirtualFileDataObject.FileDescriptor> _descs = new List<VirtualFileDataObject.FileDescriptor>();

            foreach (ShellFile _file in Files)
            {
                VirtualFileDataObject.FileDescriptor _fd = new VirtualFileDataObject.FileDescriptor();

                _fd.Name = _file.Name;
                _fd.Length = _file.Size;
                _fd.StreamContents = _file.WriteContentsToStream;

                _descs.Add(_fd);
            }

            VirtualFileDataObject _vdo = new VirtualFileDataObject(GetActionStarted, GetActionComplete);

            _vdo.SetData(_descs);
            _vdo.PreferredDropEffect = (System.Windows.DragDropEffects)ActionToTake;
            _vdo.IsAsynchronous = false;

            return _vdo;
        }

        protected void GetActionStarted(VirtualFileDataObject vdo)
        {
            if (ActionStarted != null)
            {
                ActionStarted(this, (DragDropEffects)vdo.PreferredDropEffect);
            }
        }
        
        protected void GetActionComplete(VirtualFileDataObject vdo)
        {
            if (ActionCompleted != null)
            {
                ActionCompleted(this, (DragDropEffects)vdo.PerformedDropEffect);
            }
        }

        public class ShellFile
        {
            public string Name;
            public byte[] Contents;
            public int Size;

            public void WriteContentsToStream(Stream Destination)
            {
                Destination.Write(Contents, 0, Size);
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);

        public class NativeMethods
        {
            [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Structure exists for interop.")]
            [StructLayout(LayoutKind.Sequential)]
            public struct FILEGROUPDESCRIPTOR
            {
                public UInt32 cItems;
                // Followed by 0 or more FILEDESCRIPTORs
            }

            [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Structure exists for interop.")]
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct FILEDESCRIPTOR
            {
                public UInt32 dwFlags;
                public Guid clsid;
                public Int32 sizelcx;
                public Int32 sizelcy;
                public Int32 pointlx;
                public Int32 pointly;
                public UInt32 dwFileAttributes;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
                public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
                public UInt32 nFileSizeHigh;
                public UInt32 nFileSizeLow;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
                public string cFileName;
            }

            public FILEGROUPDESCRIPTOR FGD;

            public static FILEDESCRIPTOR[] GetFileDescriptors(System.Runtime.InteropServices.ComTypes.IDataObject DataObject, out IntPtr hGlobal)
            {
                FORMATETC _req = new FORMATETC();
                _req.tymed = TYMED.TYMED_HGLOBAL;
                _req.dwAspect = DVASPECT.DVASPECT_CONTENT;
                _req.cfFormat = (short)(System.Windows.DataFormats.GetDataFormat("FileGroupDescriptorW").Id);
                _req.lindex = -1;

                STGMEDIUM _val;
                DataObject.GetData(_req, out _val);

                hGlobal = _val.unionmember;
                IntPtr _fgptr = GlobalLock(_val.unionmember);

                NativeMethods.FILEGROUPDESCRIPTOR _fg;

                _fg = (NativeMethods.FILEGROUPDESCRIPTOR)Marshal.PtrToStructure(_fgptr, typeof(NativeMethods.FILEGROUPDESCRIPTOR));

                _fgptr = new IntPtr(_fgptr.ToInt64() + 4);

                NativeMethods.FILEDESCRIPTOR[] _fileDesc = new NativeMethods.FILEDESCRIPTOR[_fg.cItems];

                for (int _i = 0; _i < _fg.cItems; _i++)
                {
                    _fileDesc[_i] = (NativeMethods.FILEDESCRIPTOR)Marshal.PtrToStructure(_fgptr, typeof(NativeMethods.FILEDESCRIPTOR));

                    _fgptr = new IntPtr(_fgptr.ToInt64() + Marshal.SizeOf(typeof(NativeMethods.FILEDESCRIPTOR)));
                }

                return _fileDesc;
            }

            public static byte[] GetFileContents(System.Runtime.InteropServices.ComTypes.IDataObject DataObject, int Index, uint Size)
            {
                byte[] _contents = null;
                
                FORMATETC _req = new FORMATETC();
                _req.tymed = TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM;
                _req.dwAspect = DVASPECT.DVASPECT_CONTENT;
                _req.cfFormat = (short)(System.Windows.DataFormats.GetDataFormat("FileContents").Id);
                _req.lindex = Index;

                STGMEDIUM _val = new STGMEDIUM();
                DataObject.GetData(_req, out _val);

                switch (_val.tymed)
                {
                    case TYMED.TYMED_ISTREAM:
                        IStream _stream = (IStream)Marshal.GetObjectForIUnknown(_val.unionmember);

                        MemoryStream _newStream = new MemoryStream();
                        IntPtr _bytesReadPtr = Marshal.AllocHGlobal(8);
                        int _bytesRead = 0;

                        try
                        {
                            _stream.Seek(0, 0, _bytesReadPtr);
                        }
                        catch
                        {
                            // Maybe we're already at the start of the stream and it doesn't support Seek.
                        }

                        do
                        {
                            byte[] _buffer = new byte[4096]; // Not reading the whole file at once. We wanna be nice to the IStream.

                            _stream.Read(_buffer, _buffer.Length, _bytesReadPtr);

                            _bytesRead = (int)Marshal.PtrToStructure(_bytesReadPtr, typeof(int));

                            if (_bytesRead > 0)
                            {
                                _newStream.Write(_buffer, 0, _bytesRead);
                            }
                        } while (_bytesRead > 0);

                        _contents = _newStream.ToArray();

                        Marshal.FreeHGlobal(_bytesReadPtr);
                        break;
                    case TYMED.TYMED_HGLOBAL:
                        IntPtr _hGlobal = GlobalLock((IntPtr)_val.unionmember);

                        if (_hGlobal == IntPtr.Zero)
                        {
                            _contents = null;
                        }
                        else
                        {
                            try
                            {
                                Marshal.Copy(_hGlobal, _contents, 0, (int)Size);
                            }
                            finally
                            {
                                GlobalUnlock(_hGlobal);
                            }
                        }

                        Marshal.FreeHGlobal((IntPtr)_val.pUnkForRelease);
                        break;
                    default:
                        _contents = null;
                        break;
                }

                return _contents;
            }
        }
    }
}
