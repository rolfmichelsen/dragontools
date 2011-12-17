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
    /// Abstract representation of a disk based filesystem.  Subclasses provide support for actual filesystems.
    /// </summary>
    /// <see cref="Flex"/>
    /// <see cref="OS9"/>
    /// <see cref="RsDos"/>
    /// <see cref="DragonDos"/>
    public interface IDiskFilesystem : IDisposable
    {
        /// <summary>
        /// The disk containing the filesystem.
        /// </summary>
        IDisk Disk { get; }

        /// <summary>
        /// <value>true</value> if the filesystem supports write operations.
        /// </summary>
        bool IsWriteable { get; }

        /// <summary>
        /// Returns a list of all files in filesystem root directory.
        /// </summary>
        /// <returns>A list containing the filename of all files in the filesystem root directory.</returns>
        string[] ListFiles();

        /// <summary>
        /// Read and parse a file.  The returned object contains the file data and any meta-information related to the file.
        /// </summary>
        /// <param name="filename">Name of file to read.</param>
        /// <returns>File object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        IFile ReadFile(string filename);

        /// <summary>
        /// Read a raw file and return it as a byte array.  The file is not parsed in any way and all filesystem headers and meta-data are included
        /// in the returned byte array.
        /// </summary>
        /// <param name="filename">Name of file to read.</param>
        /// <returns>Raw file contents.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        byte[] ReadFileRaw(string filename);

        /// <summary>
        /// Write a file to the filesystem.  The provided filename will override any filename specified in the file object.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FileExistsException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemFullException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        void WriteFile(string filename, IFile file);

        /// <summary>
        /// Write a raw file to the filesystem.  The data must include any filesystem headers and meta-data required by the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="data">Raw file data.</param>
        /// <exception cref="FileExistsException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemFullException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        void WriteFileRaw(string filename, byte[] data);

        /// <summary>
        /// Returns the number of free bytes in the filesystem.
        /// </summary>
        /// <returns>The number of free bytes in the filesystem.</returns>
        int Free();

        /// <summary>
        /// Deletes a file from the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to be deleted.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        void DeleteFile(string filename);

        /// <summary>
        /// Rename a file in the filesystem.
        /// </summary>
        /// <param name="oldname">Old filename.</param>
        /// <param name="newname">New filename.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The new file name is invalid for this filesystem.</exception>
        /// <exception cref="FileExistsException">The new filename is already used by another file.</exception>
        void RenameFile(string oldname, string newname);

        /// <summary>
        /// Verifies that a filename is valid for this filesystem.
        /// </summary>
        /// <param name="filename">Filename to validate.</param>
        /// <returns><value>true</value> if the filename is valid.</returns>
        bool IsValidFilename(string filename);

        /// <summary>
        /// Check whether a file exists.
        /// </summary>
        /// <param name="filename">Name of file.</param>
        /// <returns><value>true</value> if the file exists.</returns>
        bool FileExists(string filename);

        /// <summary>
        /// Checks the filesystem consistency.
        /// </summary>
        /// <exception cref="FilesystemConsistencyException">Thrown if the filesystem is not consistent in a manner that makes write operations unsafe.</exception>
        void Check();

        /// <summary>
        /// Create an empty filesystem.
        /// </summary>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        void Initialize();

        /// <summary>
        /// Returns meta-information for a named file.
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>File meta-information object.</returns>
        FileInfo GetFileInfo(string filename);

        /// <summary>
        /// Check whether a given sector is marked as allocated by the filesystem.
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="track">Track</param>
        /// <param name="sector">Sector</param>
        /// <returns><value>true</value> if the sector is marked as allocated, otherwise <value>false</value>.</returns>
        bool IsSectorAllocated(int head, int track, int sector);

        /// <summary>
        /// Returns a class for parsing and encoding files for this filesystem.
        /// Applications will normally use the <see cref="ReadFile">ReadFile</see> and <see cref="WriteFile">WriteFile</see> functions instead of directly using
        /// the file parser class.
        /// </summary>
        /// <returns>File parser class.</returns>
        IFileParser GetFileParser();
    }
}
