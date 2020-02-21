/*
   Copyright 2011-2020 Rolf Michelsen

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

*/

using System;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape
{
    /// <summary>
    /// Ecapsulates a file in the Dragon tape filesystem.
    /// </summary>
    public sealed class DragonFile : IFile
    {
        /// <summary>
        /// File payload data.
        /// </summary>
        private byte[] data;


        /// <summary>
        /// The file type.
        /// </summary>
        public DragonFileType FileType { get; private set; }


        /// <summary>
        /// The fully qualified name of the file.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// The size of this file in the filesystem.  This length includes any filesystem
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
        /// Return a concise one-line representation of relevant filesystem specific file attributes, suitable for
        /// showing in a directory listing.
        /// </summary>
        /// <returns>String representation of file attributes.</returns>
        public string GetAttributes()
        {
            return "";
        }

        /// <summary>
        /// Set to indicate that this file is ASCII encoded.
        /// </summary>
        public bool IsAscii { get; private set; }


        /// <summary>
        /// Set to indicate that this file is recorded with gaps between each tape block.
        /// </summary>
        public bool IsGapped { get; private set; }


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
        public IFileInfo FileInfo
        {
            get { throw new NotImplementedException(); }
        }


        /// <summary>
        /// The size of the file payload data.
        /// </summary>
        public int Length { get { return data == null ? 0 : data.Length; } }


        /// <summary>
        /// The load address for a machine code program.  
        /// </summary>
        public int LoadAddress { get; private set; }


        /// <summary>
        /// The execution start address for a machine code program.
        /// </summary>
        public int StartAddress { get; private set; }


        /// <summary>
        /// Create a Dragon file object.
        /// </summary>
        /// <param name="name">File name.</param>
        /// <param name="type">File type.</param>
        /// <param name="data">File payload data.</param>
        /// <param name="isascii">Indicates that the file is ASCII encoded.</param>
        /// <param name="isgapped">Indicates that the file is recorded with gaps between each block.</param>
        /// <param name="loadAddress">Load address for machine code programs.</param>
        /// <param name="startAddress">Execution start address for machine code programs.</param>
        /// <remarks>
        /// This class assumes ownership of the data parameter and assumes that the caller will not make changes to the contents of this
        /// array.
        /// </remarks>
        internal DragonFile(string name, DragonFileType type, byte[] data, bool isascii, bool isgapped, int loadAddress, int startAddress)
        {
            Name = name;
            FileType = type;
            this.data = data;
            IsAscii = isascii;
            IsGapped = isgapped;
            LoadAddress = loadAddress;
            StartAddress = startAddress;
        }



        /// <summary>
        /// Creates a file object for a Dragon data file.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="data">File payload data.</param>
        /// <returns>The Dragon file object,</returns>
        public static DragonFile CreateDataFile(string name, byte[] data, bool isascii, bool isgapped)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (data == null) throw new ArgumentNullException("data");

            return new DragonFile(name, DragonFileType.Data, (byte[])data.Clone(), isascii, isgapped, 0, 0);
        }


        /// <summary>
        /// Creates a file object for a Dragon BASIC file.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="data">File payload data.</param>
        /// <returns>The Dragon file object.</returns>
        public static DragonFile CreateBasicFile(string name, byte[] data, bool isascii, bool isgapped)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (data == null) throw new ArgumentNullException("data");

            return new DragonFile(name, DragonFileType.Basic, (byte[])data.Clone(), isascii, isgapped, 0, 0);
        }


        /// <summary>
        /// Create a file object for a Dragon machine code program.
        /// </summary>
        /// <param name="name">Name of file.</param>
        /// <param name="data">File payload data.</param>
        /// <param name="loadAddress">Program load address in memory.</param>
        /// <param name="startAddress">Program execution start address in memory.</param>
        /// <returns>The Dragon file object.</returns>
        public static DragonFile CreateMachineCodeFile(string name, byte[] data, int loadAddress, int startAddress, bool isascii, bool isgapped)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (data == null) throw new ArgumentNullException("data");

            return new DragonFile(name, DragonFileType.MachineCode, (byte[])data.Clone(), isascii, isgapped, loadAddress, startAddress);
        }





    }




    /// <summary>
    /// Identifier for the file types supported by the Dragon tape filesystem.
    /// </summary>
    /// <remarks>
    /// Note that the numerical values for these identifiers corresponds to the file type
    /// identifiers used in the Dragon file header structure.
    /// </remarks>
    public enum DragonFileType
    {
        Basic = 0,
        Data = 1,
        MachineCode = 2
    }

}
