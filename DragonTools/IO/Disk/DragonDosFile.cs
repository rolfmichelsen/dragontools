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
    /// Base class for all DragonDos file types.
    /// </summary>
    /// <seealso cref="DragonDosDataFile"/>
    /// <seealso cref="DragonDosBasicFile"/>
    /// <seealso cref="DragonDosMachineCodeFile"/>
    public abstract class DragonDosFile : IFile
    {
        /// <summary>
        /// File payload data.
        /// </summary>
        protected byte[] data;

        /// <summary>
        /// The fully qualified name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The size of this file in the filesystem.  
        /// This length includes any filesystem
        /// headers and meta-information.  The exact defintiion of this property is left to
        /// each concrete filesystem, but it should correspond to the file size shown with
        /// directory listings.
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// Returns the size of the file payload data.
        /// This length is less than the value returned by <see cref="Size">Size</see> for files with file headers.  
        /// It is equivalent to <c>GetData().Length</c>.
        /// </summary>
        public int Length { get { return data.Length; } }

        /// <summary>
        /// True if this file is an executable file.
        /// </summary>
        public bool IsExecutable { get; protected set; }

        /// <summary>
        /// True if this file is marked as protected by the filesystem.
        /// </summary>
        public bool IsProtected { get; set; }

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
            return data;
        }
    }


    /// <summary>
    /// A DragonDos data file.
    /// </summary>
    public sealed class DragonDosDataFile : DragonDosFile
    {
        /// <summary>
        /// Ceeate a DragonDos data file.
        /// </summary>
        /// <param name="data">File payload data.</param>
        public DragonDosDataFile(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            base.data = data;
            base.Name = null;
            base.Size = data.Length;
            base.IsExecutable = false;
            base.IsProtected = false;
        }


        public override string ToString()
        {
            return String.Format("DragonDos data file: Length={0}", Length);
        }
    }


    /// <summary>
    /// A DragonDos BASIC program.
    /// </summary>
    public sealed class DragonDosBasicFile : DragonDosFile
    {
        /// <summary>
        /// Create a DragonDos BASIC program file.
        /// </summary>
        /// <param name="data">Tokenized BASIC program data.</param>
        public DragonDosBasicFile(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");
            base.data = data;
            base.Name = null;
            base.Size = data.Length + DragonDosFileParser.FileHeaderSize;
            base.IsExecutable = true;
            base.IsProtected = false;
        }


        public override string ToString()
        {
            return String.Format("DragonDos BASIC program: Length={0}", Length);
        }
    }


    /// <summary>
    /// A DragonDos machine code program.
    /// </summary>
    public sealed class DragonDosMachineCodeFile : DragonDosFile
    {
        /// <summary>
        /// The default address in memory where this machine program will be loaded.
        /// </summary>
        public int LoadAddress { get; private set; }

        /// <summary>
        /// The default address in memory where this machine code program will be executed.
        /// </summary>
        public int ExecAddress { get; private set; }

        /// <summary>
        /// Create a DragonDos machine code program file object.
        /// </summary>
        /// <param name="data">File payload data.</param>
        /// <param name="loadaddress">Default load address of this program.</param>
        /// <param name="execaddress">Default execution address of this program.</param>
        public DragonDosMachineCodeFile(byte[] data, int loadaddress, int execaddress)
        {
            if (data == null) throw new ArgumentNullException("data");
            base.data = data;
            base.Name = null;
            base.Size = data.Length + DragonDosFileParser.FileHeaderSize;
            base.IsExecutable = true;
            base.IsProtected = false;
            LoadAddress = loadaddress;
            ExecAddress = execaddress;
        }


        public override string ToString()
        {
            return String.Format("DragonDos machine code program: Load={0} Length={1} Exec={2}", LoadAddress, Length, ExecAddress);
        }
        
    }
}
