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
        /// Directory information for this file, or <value>null</value> if the file isn't associated
        /// with a directory entry.
        /// </summary>
        private DragonDosFileInfo fileinfo = null;


        /// <summary>
        /// The file type.
        /// </summary>
        public DragonDosFileType FileType { get; private set; }


        /// <summary>
        /// Returns the size of the file payload data.
        /// It is equivalent to <c>GetData().Length</c>.
        /// </summary>
        public int Length { get { return data.Length; } }


        /// <summary>
        /// The load address for a machine code program.  
        /// </summary>
        public int LoadAddress { get; private set; }


        /// <summary>
        /// The execution start address for a machine code program.
        /// </summary>
        public int StartAddress { get; private set; }


        /// <summary>
        /// Return the file payload data.
        /// </summary>
        public byte[] GetData()
        {
            return (byte[]) data.Clone();
        }


        /// <summary>
        /// Return file information from the disk directory.
        /// This property will be <value>null</value> if the file is not associated with a directory entry.
        /// </summary>
        public IFileInfo FileInfo { get { return fileinfo; } }


        /// <summary>
        /// Create a DragonDos file object.
        /// </summary>
        /// <param name="type">File type.</param>
        /// <param name="data">File payload data.</param>
        /// <param name="loadAddress">Load address for machine code programs.</param>
        /// <param name="startAddress">Execution start address for machine code programs.</param>
        /// <remarks>
        /// This class assumes ownership of the data parameter and assumes that the caller will not make changes to the contents of this
        /// array.
        /// </remarks>
        private DragonDosFile(DragonDosFileType type, byte[] data, int loadAddress = 0, int startAddress = 0)
        {
            FileType = type;
            this.data = data;
            LoadAddress = loadAddress;
            StartAddress = startAddress;
        }


        /// <summary>
        /// Create a DragonDos file object that is associated with a filesystem directory entry.
        /// </summary>
        /// <param name="info">File directory information.</param>
        /// <param name="type">File type.</param>
        /// <param name="data">File payload data.</param>
        /// <param name="loadaddress">Load address for machine code programs.</param>
        /// <param name="startaddress">Execution start address for machine code programs.</param>
        /// <remarks>
        /// This class assumes ownership of the data parameter and assumes that the caller will not make changes to the contents of this
        /// array.
        /// </remarks>
        private DragonDosFile(DragonDosFileInfo info, DragonDosFileType type, byte[] data, int loadaddress = 0, int startaddress = 0)
            : this(type, data, loadaddress, startaddress)
        {
            fileinfo = info;
        }



        /// <summary>
        /// Creates a file object for a DragonDos data file.
        /// </summary>
        /// <param name="data">File payload data.</param>
        /// <returns>The DragonDos file object,</returns>
        public static DragonDosFile CreateDataFile(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            
            return new DragonDosFile(DragonDosFileType.Data, (byte[]) data.Clone());
        }


        /// <summary>
        /// Creates a file object for a DragonDos BASIC file.
        /// </summary>
        /// <param name="data">File payload data.</param>
        /// <returns>The DragonDos file object.</returns>
        public static DragonDosFile CreateBasicFile(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");

            return new DragonDosFile(DragonDosFileType.Basic, (byte[]) data.Clone());
        }


        /// <summary>
        /// Create a file object for a DragonDos machine code program.
        /// </summary>
        /// <param name="data">File payload data.</param>
        /// <param name="loadAddress">Program load address in memory.</param>
        /// <param name="startAddress">Program execution start address in memory.</param>
        /// <returns>The DragonDos file object.</returns>
        public static DragonDosFile CreateMachineCodeFile(byte[] data, int loadAddress, int startAddress)
        {
            if (data == null) throw new ArgumentNullException("data");

            return new DragonDosFile(DragonDosFileType.MachineCode, (byte[]) data.Clone(), loadAddress, startAddress);
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
                return new DragonDosFile(fileinfo, DragonDosFileType.Data, raw);

            /* Extract the file payload data. */
            var data = new byte[raw.Length - FileHeaderSize];
            Array.Copy(raw, FileHeaderSize, data, 0, data.Length);

            /* Create the correct file object. */
            switch ((DragonDosFileType) raw[1])
            {
                case DragonDosFileType.Basic:
                    return new DragonDosFile(fileinfo, DragonDosFileType.Basic, data);
                case DragonDosFileType.MachineCode:
                    int loadAddress = (raw[2] << 8) | raw[3];
                    int startAddress = (raw[6] << 8) | raw[7];
                    return new DragonDosFile(fileinfo, DragonDosFileType.MachineCode, data, loadAddress, startAddress);
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
            return String.Format("DragonDos file: name={0} type={1}", (fileinfo == null ? "n/a" : fileinfo.Name), FileType);
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
