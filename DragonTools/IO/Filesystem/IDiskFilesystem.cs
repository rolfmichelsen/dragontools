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
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.Flex;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.RsDos;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem
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
        /// <exception cref="InvalidFileException">The file format is invalid.</exception>
        IFile ReadFile(string filename);

        /// <summary>
        /// Write a file to the filesystem.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FilesystemFullException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemNotWriteableException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="InvalidFilenameException">This filesystem does not support write operations.</exception>
        /// <exception cref="FileExistsException">The file name is invalid for this filesystem.</exception>
        void WriteFile(string filename, IFile file);

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
        /// Returns meta-information for a named file.
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>File meta-information object.</returns>
        IFileInfo GetFileInfo(string filename);


        /// <summary>
        /// Returns a file name object for manipulating a filename.
        /// </summary>
        IFileName GetFileName(string filename);

        /// <summary>
        /// Check whether a given sector is marked as allocated by the filesystem.
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="track">Track</param>
        /// <param name="sector">Sector</param>
        /// <returns><value>true</value> if the sector is marked as allocated, otherwise <value>false</value>.</returns>
        bool IsSectorAllocated(int head, int track, int sector);
    }
}
