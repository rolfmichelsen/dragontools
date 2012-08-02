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
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem
{
    /// <summary>
    /// Abstract representation of a tape based filesystem.  
    /// Subclasses provide support for actual filesystems.
    /// </summary>
    /// <see cref="DragonTape"/>
    public interface ITapeFilesystem
    {
        /// <summary>
        /// Reference to the virtual tape containing the filesystem.
        /// </summary>
        ITape Tape { get; }

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
        /// <exception cref="InvalidFileException">The file format is invalid.</exception>
        IFile ReadFile(string filename);

        /// <summary>
        /// Read and parse the next file from tape.
        /// The returned object contains the file data and any meta-information related to the file.
        /// </summary>
        /// <returns>File object.</returns>
        IFile ReadFile();

        /// <summary>
        /// Write a file to the filesystem.
        /// The provided filename will override any filename specified in the file object.
        /// </summary>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        void WriteFile(IFile file);

        /// <summary>
        /// Rewind the tape.
        /// </summary>
        void Rewind();
    }
}
