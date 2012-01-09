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

namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Base class for all Dragon file types used with the Dragon tape filesystem.
    /// </summary>
    /// <seealso cref="DragonDataFile"/>
    /// <seealso cref="DragonBasicFile"/>
    /// <seealso cref="DragonMachineCodeFile"/>
    public abstract class DragonFile : IFile
    {
        /// <summary>
        /// The fully qualified name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The size of this file in the filesystem.  This length includes any filesystem
        /// headers and meta-information.  The exact defintiion of this property is left to
        /// each concrete filesystem, but it should correspond to the file size shown with
        /// directory listings.
        /// </summary>
        public int Size { get {throw new NotImplementedException();} }

        /// <summary>
        /// True if this file is an executable file.
        /// </summary>
        public abstract bool IsExecutable { get; }

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
        public bool IsAscii { get; set; }

        /// <summary>
        /// Set to indicate that this file is recorded with gaps between each tape block.
        /// </summary>
        public bool IsGapped { get; set; }

        /// <summary>
        /// File payload data.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The size of the file payload data.
        /// </summary>
        public int Length { get { return Data == null ? 0 : Data.Length; } }

        protected DragonFile()
        {
            Name = null;
            IsAscii = false;
            IsGapped = false;
            Data = null;
        }
    }




    /// <summary>
    /// Dragon data file used with the Dragon tape filesystem.
    /// </summary>
    public sealed class DragonDataFile : DragonFile
    {
        /// <summary>
        /// True if this file is an executable file.
        /// </summary>
        public override bool IsExecutable { get { return false; } }


        public override string ToString()
        {
            return String.Format("Dragon data file (Length={0})", Length);
        }


        public DragonDataFile(string filename, byte[] data, bool isascii, bool isgapped)
        {
            Name = filename;
            Data = data;
            IsAscii = isascii;
            IsGapped = isgapped;
        }
    }



    /// <summary>
    /// Dragon BASIC file used with the Dragon tape filesystem.
    /// </summary>
    public sealed class DragonBasicFile : DragonFile
    {
        /// <summary>
        /// True if this file is an executable file.
        /// </summary>
        public override bool IsExecutable { get { return true; }}


        public override string ToString()
        {
            return String.Format("Dragon BASIC program (Length={0})", Length);
        }


        public DragonBasicFile(string filename, byte[] data, bool isascii, bool isgapped)
        {
            Name = filename;
            Data = data;
            IsAscii = isascii;
            IsGapped = isgapped;            
        }
    }



    /// <summary>
    /// Dragon machine code file used with the Dragon tape filesystem.
    /// </summary>
    public sealed class DragonMachineCodeFile : DragonFile
    {
        /// <summary>
        /// True if this file is an executable file.
        /// </summary>
        public override bool IsExecutable { get { return true; } }

        public int LoadAddress { get; set; }

        public int ExecAddress { get; set; }

        public DragonMachineCodeFile(string filename, byte[] data, bool isascii, bool isgapped, int loadaddress, int execaddress)
        {
            Name = filename;
            Data = data;
            IsAscii = isascii;
            IsGapped = isgapped;
            LoadAddress = loadaddress;
            ExecAddress = execaddress;
        }

    }
}
