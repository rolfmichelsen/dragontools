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
    /// Abstract representation of a tape based filesystem.  
    /// Subclasses provide support for actual filesystems.
    /// </summary>
    /// <see cref="DragonTapeFilesystem"/>
    public interface ITapeFilesystem : IDisposable
    {
        /// <summary>
        /// <value>true</value> if the filesystem supports write operations.
        /// </summary>
        bool IsWriteable { get; }

        /// <summary>
        /// <value>true</value> if the filesystem supports read operations.
        /// </summary>
        bool IsReadable { get; }

        /// <summary>
        /// Verifies that a filename is valid for this filesystem.
        /// </summary>
        /// <param name="filename">Filename to validate.</param>
        /// <returns><value>true</value> if the filename is valid.</returns>
        bool IsValidFilename(string filename);

        /// <summary>
        /// Read and parse a file.  
        /// The returned object contains the file data and any meta-information related to the file.
        /// </summary>
        /// <param name="filename">Name of file to read, or <value>null</value> to read the next file from the stream.</param>
        /// <returns>File object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        IFile ReadFile(string filename);

        /// <summary>
        /// Write a file to the filesystem.
        /// The provided filename will override any filename specified in the file object.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        void WriteFile(string filename, IFile file);

        /// <summary>
        /// Returns a class for parsing and encoding files for this filesystem.
        /// Applications will normally use the <see cref="ReadFile">ReadFile</see> and <see cref="WriteFile">WriteFile</see> functions instead of directly using
        /// the file parser class.
        /// </summary>
        /// <returns>File parser class.</returns>
        IFileParser GetFileParser();

        /// <summary>
        /// Read a raw file and return it as a byte array.
        /// The file is not parsed in any way and all filesystem headers and meta-data are included
        /// in the returned byte array.
        /// </summary>
        /// <param name="filename">Name of file to read, or <value>null</value> to read the next file from the stream.</param>
        /// <returns>Raw file contents.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        byte[] ReadFileRaw(string filename);

        /// <summary>
        /// Write a raw file to the filesystem.  
        /// The data must include any filesystem headers and meta-data required by the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="data">Raw file data.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        void WriteFileRaw(string filename, byte[] data);
    }
}
