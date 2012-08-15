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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    /// <summary>
    /// Represents a file in an OS9 filesystem.
    /// </summary>
    public sealed class OS9File : IFile
    {
        private byte[] data;


        /// <summary>
        /// Filename.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Size of file, in bytes
        /// </summary>
        public int Size { get; private set; }


        /// <summary>
        /// Indicates that this file is actually a directory.
        /// </summary>
        public bool IsDirectory
        {
            get { throw new NotImplementedException(); }
        }


        /// <summary>
        /// Returns a brief string representation of any file attributes supported by the filesystem.  Subclasses will typically
        /// override this method to provide a summary of filesystem specific file attributes not represented by the base class.
        /// </summary>
        /// <returns>String representation of file attributes.</returns>
        public string GetAttributes()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Return the file payload data.
        /// </summary>
        public byte[] GetData()
        {
            return (byte[])data.Clone();
        }


        /// <summary>
        /// Return file information from the disk directory.
        /// This property will be <value>null</value> if the file is not associated with a directory entry.
        /// </summary>
        public IFileInfo FileInfo
        {
            get { throw new NotImplementedException(); }
        }


        internal OS9File(string filename, byte[] data)
        {
            Name = filename;
            this.data = data;
        }
    }
}
