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

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Provides functionality for parsing and encoding DragonDos files.
    /// </summary>
    public class DragonDosFileParser : IFileParser
    {
        /// <summary>
        /// Header size (in bytes) for BASIC and machine code programs.
        /// </summary>
        public const int FileHeaderSize = 9;

        /// <summary>
        /// The value of the first byte of any DragonDos file header.
        /// </summary>
        public const byte FileHeaderFirst = 0x55;

        /// <summary>
        /// The value of the last byte of any DragonDos file header.
        /// </summary>
        public const byte FileHeaderLast = 0xaa;

        /// <summary>
        /// DragonDos file header type value for a BASIC program file.
        /// </summary>
        public const byte FileTypeBasic = 1;

        /// <summary>
        /// DragonDos file header type value for a machine code program file.
        /// </summary>
        public const byte FileTypeMachineCode = 2;

        /// <summary>
        /// Parse a raw file and return the corresponding file object.  The raw file data is typically obtained by calling
        /// <c>IDiskFilesystem.ReadFile</c>.
        /// </summary>
        /// <param name="raw">Raw file data.</param>
        /// <returns>A file object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile Parse(byte[] raw)
        {
            if (raw == null) throw new ArgumentNullException("raw");
            byte[] data;

            /* Parse DragonDosDataFile. */
            if (raw.Length < FileHeaderSize || raw[0] != FileHeaderFirst || raw[8] != FileHeaderLast)
            {
                data = new byte[raw.Length];
                Array.Copy(raw, data, raw.Length);
                return new DragonDosDataFile(data);
            }

            data = new byte[raw.Length - FileHeaderSize];
            Array.Copy(raw, FileHeaderSize, data, 0, data.Length);
            switch (raw[1]) 
            {
                case FileTypeBasic:
                    return new DragonDosBasicFile(data);
                case FileTypeMachineCode:
                    int loadaddress = (raw[2] << 8) | raw[3];
                    int execaddress = (raw[6] << 8) | raw[7];
                    return new DragonDosMachineCodeFile(data, loadaddress, execaddress);
                default:
                    throw new FileFormatException(String.Format("Unknown file format identifier {0}", raw[1]));
            }
        }


        /// <summary>
        /// Parse a raw file and return the corresponding file object.  The raw file data is typically obtained by calling
        /// <c>IDiskFilesystem.ReadFile</c>.
        /// </summary>
        /// <param name="raw">Raw file data.</param>
        /// <param name="info">File meta-information from the filesystem.</param>
        /// <returns>A file object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile Parse(byte[] raw, FileInfo info)
        {
            if (raw == null) throw new ArgumentNullException("raw");
            if (info == null) throw new ArgumentNullException("info");
            if (!(info is DragonDosFileInfo)) throw new ArgumentException("Unexpected type of info parameter: " + info.GetType().FullName);

            var file = (DragonDosFile) Parse(raw);
            file.Name = info.Name;
            file.IsProtected = ((DragonDosFileInfo) info).IsProtected;
            return file; 
        }


        /// <summary>
        /// Encode a file object as a raw byte stream suitable for writing to a concrete filesystem.
        /// </summary>
        /// <param name="file">File to encode.</param>
        /// <returns>Raw byte stream for file.</returns>
        /// <exception cref="FileFormatException">The file object cannot be encoded for this file system.</exception>
        public byte[] Encode(IFile file)
        {
            if (file == null) throw new ArgumentNullException("file");
            if (!(file is DragonDosFile)) throw new ArgumentException("Unexpected class type for file: " + file.GetType().FullName);

            var dragonfile = (DragonDosFile) file;
            byte[] raw;

            if (file is DragonDosDataFile)
            {
                raw = new byte[dragonfile.Length];
                Array.Copy(dragonfile.GetData(), raw, raw.Length);
                return raw;
            }

            if (file is DragonDosBasicFile)
            {
                raw = new byte[dragonfile.Length + FileHeaderSize];
                raw[0] = FileHeaderFirst;
                raw[1] = FileTypeBasic;
                raw[8] = FileHeaderLast;
                Array.Copy(dragonfile.GetData(), 0, raw, FileHeaderSize, raw.Length-FileHeaderSize);
                return raw;
            }

            if (file is DragonDosMachineCodeFile)
            {
                raw = new byte[dragonfile.Length + FileHeaderSize];
                raw[0] = FileHeaderFirst;
                raw[1] = FileTypeMachineCode;
                raw[2] = (byte) ((((DragonDosMachineCodeFile) file).LoadAddress >> 8) & 0xff);              // machine code load address
                raw[3] = (byte) (((DragonDosMachineCodeFile) file).LoadAddress & 0xff);
                raw[4] = (byte) ((dragonfile.Length >> 8) & 0xff);                                          // machine code program length
                raw[5] = (byte) (dragonfile.Length & 0xff);
                raw[6] = (byte) ((((DragonDosMachineCodeFile) file).ExecAddress >> 8) & 0xff);              // machine code start address
                raw[7] = (byte) (((DragonDosMachineCodeFile) file).ExecAddress & 0xff);
                raw[8] = FileHeaderLast;
                Array.Copy(dragonfile.GetData(), 0, raw, FileHeaderSize, raw.Length - FileHeaderSize);
                return raw;                
            }

            throw new ApplicationException("Unexpected type " + file.GetType().FullName);
        }
    }
}
