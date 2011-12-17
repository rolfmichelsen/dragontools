/*
Copyright (c) 2011, Rolf Michelsen
All rights reserved.

Redistribution and use in source and binary forms, with or without 
modification, are permitted provided that the following conditions are met:

    * Redistributions of source code must retain the above copyright 
      notice, this list of conditions and the following disclaimer.
    * Redistributions in binary form must reproduce the above copyright 
      notice, this list of conditions and the following disclaimer in the 
      documentation and/or other materials provided with the distribution.
    * Neither the name of Rolf Michelsen nor the 
      names of other contributors may be used to endorse or promote products 
      derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY 
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// This class represents a single entry in the DragonDos directory structure.  Multiple entries may
    /// be required to get the complete directory information for a single file.
    /// </summary>
    internal sealed class DragonDosDirectoryEntry
    {
        public DirectoryFlags Flags { get; set; }
        public string Filename { get; set; }
        public Extent[] Extents { get; set; }
        public int NextEntry { get; set; }
        public int LastSectorSize { get; set; }

        public bool IsExtensionEntry
        {
            get { return (Flags & DirectoryFlags.ExtensionEntry) != 0; }
            set
            {
                if (value) Flags |= DirectoryFlags.ExtensionEntry;
                else Flags &= ~DirectoryFlags.ExtensionEntry;
            }
        }

        public bool IsMainEntry
        {
            get { return !IsExtensionEntry; }
        }

        public bool IsExtended
        {
            get { return (Flags & DirectoryFlags.MoreExtensions) != 0; }
            set
            {
                if (value) Flags |= DirectoryFlags.MoreExtensions;
                else Flags &= ~DirectoryFlags.MoreExtensions;
            }
        }

        public bool IsInvalid
        {
            get { return (Flags & DirectoryFlags.Invalid) != 0; }
        }

        public bool IsValid
        {
            get { return (Flags & DirectoryFlags.Invalid) == 0; }
            set
            {
                if (value) Flags &= ~DirectoryFlags.Invalid;
                else Flags |= DirectoryFlags.Invalid;
            }
        }

        public bool IsProtected
        {
            get { return (Flags & DirectoryFlags.Protected) != 0; }
            set
            {
                if (value) Flags |= DirectoryFlags.Protected;
                else Flags &= ~DirectoryFlags.Protected;
            }
        }

        public bool IsEndOfDirectory
        {
            get { return (Flags & DirectoryFlags.EndOfDirectory) != 0; }
        }

        public DragonDosDirectoryEntry()
        {
            Flags = 0;
            Filename = null;
            Extents = null;
            NextEntry = 0;
            LastSectorSize = 0;
        }

        /// <summary>
        /// Create a directory entry by parsing raw data from the directory track.
        /// </summary>
        /// <param name="data">Byte array containing the raw directory entry.</param>
        /// <param name="offset">Offset of the first byte from the raw byte array.</param>
        public DragonDosDirectoryEntry(byte[] data, int offset)
        {
            Flags = (DirectoryFlags) data[offset];

            int maxextents = 7;
            int firstextent = 1;
            if (!IsExtensionEntry)
            {
                maxextents = 4;
                firstextent = 12;
                var filename = new char[20];
                int inx = 0;
                for (int i = 0; i < 8 && data[offset + i + 1] != 0; i++)
                    filename[inx++] = (char) data[offset + i + 1];
                if (data[offset+9] != 0)
                {
                    filename[inx++] = '.';
                    for (int i = 0; i < 3 && data[offset + i + 9] != 0; i++)
                        filename[inx++] = (char) data[offset + i + 9];
                }
                Filename = new string(filename, 0, inx);
            }
            var extents = new List<Extent>();
            while (maxextents-- > 0)
            {
                int lsn = (data[offset + firstextent] << 8) | data[offset + firstextent + 1];
                int len = data[offset + firstextent + 2];
                if (len > 0)
                {
                    extents.Add(new Extent(lsn,len));
                }
                firstextent += 3;
            }
            Extents = extents.ToArray();

            if (IsExtended)
            {
                NextEntry = data[offset + 24];
            }
            else
            {
                LastSectorSize = data[offset + 24];
            }
        }

        /// <summary>
        /// Encodes a directory entry to its raw representation.
        /// </summary>
        /// <param name="data">Byte array for storing the encoded directory entry.</param>
        /// <param name="offset">Offset into byte array for storing the encoded entry.</param>
        public void Encode(byte[] data, int offset)
        {
            data[offset] = (byte) Flags;
            data[offset + 24] = (byte) (((Flags & DirectoryFlags.MoreExtensions) != 0) ? NextEntry : LastSectorSize);

            if ((Flags & DirectoryFlags.ExtensionEntry) == 0)
            {
                EncodeFilename(Filename, data, offset+1);
                offset = offset + 12;
            }
            else
            {
                offset = offset + 1;
            }


            if (Extents != null) {
                for (int i = 0; i < Extents.Length; i++ )
                {
                    data[offset++] = (byte) ((Extents[i].Lsn >> 8) & 0xff);
                    data[offset++] = (byte) (Extents[i].Lsn & 0xff);
                    data[offset++] = (byte) Extents[i].Length;
                }
            }
        }


        /// <summary>
        /// Encode the filename.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="offset"></param>
        public static void EncodeFilename(string filename, byte[] dest, int offset)
        {
            Array.Clear(dest, offset, 11);
            if (filename == null) return;

            int separator = filename.IndexOf('.');

            int len = separator == -1 ? Math.Min(8, filename.Length) : Math.Min(8, separator);
            for (int i = 0; i < len; i++)
            {
                dest[i + offset] = (byte) filename[i];
            }

            if (separator != -1)
            {
                int extoffset = separator + 1;
                len = Math.Min(3, filename.Length - extoffset);
                for (int i=0; i<len; i++)
                {
                    dest[i + 8 + offset] = (byte) filename[extoffset + i];
                }
            }
        }

        /// <summary>
        /// Returns an empty directory entry, suitable for writing to a newly created disk or sanitizing a directory track.
        /// </summary>
        /// <returns></returns>
        public static DragonDosDirectoryEntry GetEmptyEntry()
        {
            var entry = new DragonDosDirectoryEntry();
            entry.Flags = DirectoryFlags.Invalid | DirectoryFlags.EndOfDirectory;
            return entry;
        }

        [Flags]
        public enum DirectoryFlags
        {
            ExtensionEntry = 0x01,
            Protected = 0x02,
            EndOfDirectory = 0x08,
            MoreExtensions = 0x20,
            Invalid = 0x80
        }


        public struct Extent
        {
            public int Lsn;
            public int Length;

            public Extent(int lsn, int length)
            {
                Lsn = lsn;
                Length = length;
            }
        }

    }
}
