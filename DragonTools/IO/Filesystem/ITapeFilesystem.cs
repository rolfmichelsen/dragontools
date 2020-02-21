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
