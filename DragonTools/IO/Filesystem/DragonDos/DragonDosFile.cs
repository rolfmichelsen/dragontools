/*
Copyright (c) 2011-2012, Rolf Michelsen
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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos
{
    /// <summary>
    /// Class encapsulating a DragonDos file.
    /// </summary>
    public sealed class DragonDosFile : IFile
    {
        /// <summary>
        /// Size of a DragonDos file header.
        /// </summary>
        private const int FileHeaderSize = 9;

        private const byte FileHeaderFirstByte = 0x55;
        private const byte FileHeaderLastByte = 0xaa;


        /// <summary>
        /// File payload data.
        /// </summary>
        private byte[] data;

        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// The file type.
        /// </summary>
        public DragonDosFileType FileType { get; private set; }


        /// <summary>
        /// The size of this file in the filesystem.  
        /// This length includes any filesystem
        /// headers and meta-information.  The exact defintiion of this property is left to
        /// each concrete filesystem, but it should correspond to the file size shown with
        /// directory listings.
        /// </summary>
        public int Size { get; private set; }


        /// <summary>
        /// Indicates that this file is actually a directory.
        /// </summary>
        public bool IsDirectory { get { return false; }}


        /// <summary>
        /// Returns the size of the file payload data.
        /// This length is less than the value returned by <see cref="Size">Size</see> for files with file headers.  
        /// It is equivalent to <c>GetData().Length</c>.
        /// </summary>
        public int Length { get { return data.Length; } }


        /// <summary>
        /// True if this file is an executable file.
        /// </summary>
        public bool IsExecutable { get; private set; }


        /// <summary>
        /// True if this file is marked as protected by the filesystem.
        /// </summary>
        public bool IsProtected { get; private set; }


        /// <summary>
        /// The load address for a machine code program.  
        /// </summary>
        public int LoadAddress { get; private set; }


        /// <summary>
        /// The execution start address for a machine code program.
        /// </summary>
        public int StartAddress { get; private set; }


        /// <summary>
        /// Return a concise one-line representation of relevant filesystem specific file attributes, suitable for
        /// showing in a directory listing.
        /// </summary>
        /// <returns>String representation of file attributes.</returns>
        public string GetAttributes()
        {
            return IsProtected ? "P" : "";
        }


        /// <summary>
        /// Return the file payload data.
        /// </summary>
        public byte[] GetData()
        {
            return (byte[]) data.Clone();
        }


        /// <summary>
        /// Create a DragonDos file object.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="type">File type.</param>
        /// <param name="data">File payload data.</param>
        /// <param name="isProtected">True if the file has the protected flag set.</param>
        /// <param name="loadAddress">Load address for machine code programs.</param>
        /// <param name="startAddress">Execution start address for machine code programs.</param>
        /// <remarks>
        /// This class assumes ownership of the data parameter and assumes that the caller will not make changes to the contents of this
        /// array.
        /// </remarks>
        internal DragonDosFile(string name, DragonDosFileType type, byte[] data, bool isProtected, int loadAddress, int startAddress)
        {
            Name = name;
            FileType = type;
            this.data = data;
            IsProtected = isProtected;
            LoadAddress = loadAddress;
            StartAddress = startAddress;
        }


        /// <summary>
        /// Creates a file object for a DragonDos data file.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="data">File payload data.</param>
        /// <returns>The DragonDos file object,</returns>
        public static DragonDosFile CreateDataFile(string name, byte[] data)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (data == null) throw new ArgumentNullException("data");
            
            return new DragonDosFile(name, DragonDosFileType.Data, (byte[]) data.Clone(), false, 0, 0) { Size = data.Length};
        }


        /// <summary>
        /// Creates a file object for a DragonDos BASIC file.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="data">File payload data.</param>
        /// <returns>The DragonDos file object.</returns>
        public static DragonDosFile CreateBasicFile(string name, byte[] data)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (data == null) throw new ArgumentNullException("data");

            return new DragonDosFile(name, DragonDosFileType.Basic, (byte[]) data.Clone(), false, 0, 0) { Size = data.Length + FileHeaderSize};
        }


        /// <summary>
        /// Create a file object for a DragonDos machine code program.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="data">File payload data.</param>
        /// <param name="loadAddress">Program load address in memory.</param>
        /// <param name="startAddress">Program execution start address in memory.</param>
        /// <returns>The DragonDos file object.</returns>
        public static DragonDosFile CreateMachineCodeFile(string name, byte[] data, int loadAddress, int startAddress)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (data == null) throw new ArgumentNullException("data");

            return new DragonDosFile(name, DragonDosFileType.MachineCode, (byte[]) data.Clone(), false, loadAddress, startAddress) { Size = data.Length + FileHeaderSize };
        }


        /// <summary>
        /// Encode a DragonDos file to the raw on-disk representation by adding appropriate file headers.
        /// </summary>
        /// <returns>Raw on-disk file data.</returns>
        internal byte[] EncodeFile()
        {
            /* A data file does not contain any headers.  Just return the payload data unmodified. */
            if (FileType == DragonDosFileType.Data)
                return data;

            /* Allocate buffer for file payload and header and then copy the file payload data. */
            var raw = new byte[data.Length + FileHeaderSize];
            Array.Copy(data, 0, raw, FileHeaderSize, data.Length);

            /* Construct the file header. */
            raw[0] = FileHeaderFirstByte;
            raw[1] = (byte) FileType;
            raw[2] = (byte) ((LoadAddress >> 8) & 0xff);
            raw[3] = (byte) (LoadAddress & 0xff);
            raw[4] = (byte) ((Length >> 8) & 0xff);
            raw[5] = (byte) (Length & 0xff);
            raw[6] = (byte) ((StartAddress >> 8) & 0xff);
            raw[7] = (byte) (StartAddress & 0xff);
            raw[8] = FileHeaderLastByte;

            return raw;
        }


        /// <summary>
        /// Decode the raw on-disk representation of a DragonDos file to a file object.
        /// </summary>
        /// <param name="fileinfo">File information.</param>
        /// <param name="raw">Raw on-disk file data.</param>
        /// <returns>File object.</returns>
        /// <exception cref="InvalidFileException">Thrown if the file is invalid and cannot be properly decoded.</exception>
        internal static DragonDosFile DecodeFile(DragonDosFileInfo fileinfo, byte[] raw)
        {
            /* If the file does not contain a valid file header it must be a data file. */
            if (raw.Length < FileHeaderSize || raw[0] != FileHeaderFirstByte || raw[8] != FileHeaderLastByte)
                return CreateDataFile(fileinfo.Name, raw);

            /* Extract the file payload data. */
            var data = new byte[raw.Length - FileHeaderSize];
            Array.Copy(raw, FileHeaderSize, data, 0, data.Length);

            /* Create the correct file object. */
            switch ((DragonDosFileType) raw[1])
            {
                case DragonDosFileType.Basic:
                    return CreateBasicFile(fileinfo.Name, data);
                case DragonDosFileType.MachineCode:
                    int loadAddress = (raw[2] << 8) | raw[3];
                    int startAddress = (raw[6] << 8) | raw[7];
                    return CreateMachineCodeFile(fileinfo.Name, data, loadAddress, startAddress);
                default:
                    throw new InvalidFileException(fileinfo.Name, String.Format("Unknown file type identifier {0}", raw[1]));
            }
        }


        /// <summary>
        /// Return a concise string description of the file object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("DragonDos file: name={0} type={1} size={2}", Name, FileType, Size);
        }
    }


    /// <summary>
    /// Identifier for the file types supported by DragonDos.
    /// </summary>
    /// <remarks>
    /// Note that the numerical values for these identifiers corresponds to the file type
    /// identifiers used in the DragonDos file header structure.
    /// </remarks>
    public enum DragonDosFileType
    {
        Data = 0,
        Basic = 1,
        MachineCode = 2
    }

}
