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
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.Flex
{
    /// <summary>
    /// Provides support for a Flex filesystem.
    /// </summary>
    public sealed class Flex : IDiskFilesystem
    {
        /// <summary>
        /// Sector index of the first sector in the filesystem directory.
        /// </summary>
        private const int DirectoryIndex = 5;


        public IDisk Disk { get; private set; }


        public bool IsWriteable { get; private set; }


        private bool IsDisposed = false;

        /// <summary>
        /// Number of sectors per track
        /// </summary>
        private const int Sectors = 18;         //TODO Validate that this is correct for all FLEX filesystems.  It probably isn't...



        /// <summary>
        /// Instantiate a FLEX filesystem representation associated with a virtual disk.
        /// </summary>
        /// <param name="disk">Disk to associate the filesystem to.</param>
        /// <param name="isWriteable">If set write operations will be permitted to the filesystem.</param>
        public Flex(IDisk disk, bool isWriteable)
        {
            if (disk == null) throw new ArgumentNullException("disk");

            //TODO Validate that the disk geometry is valid for a FLEX disk

            Disk = disk;
            IsWriteable = isWriteable;
            IsDisposed = false;
        }


        /// <summary>
        /// Returns a list of all files in filesystem.
        /// </summary>
        /// <returns>A list containing the filename of all files in the filesystem.</returns>
        public string[] ListFiles()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            var directory = ReadDirectory();
            var files = new List<string>();
            foreach (var entry in directory)
            {
                if (entry.IsValid)
                {
                    files.Add(entry.Filename);
                }
            }
            return files.ToArray();
        }

        /// <summary>
        /// Read and parse a file.  The returned object contains the file data and any meta-information related to the file.
        /// </summary>
        /// <param name="filename">Name of file to read.</param>
        /// <returns>File object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile ReadFile(string filename)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Write a file to the filesystem.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FilesystemFullException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemNotWriteableException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="InvalidFilenameException">This filesystem does not support write operations.</exception>
        /// <exception cref="FileExistsException">The file name is invalid for this filesystem.</exception>
        public void WriteFile(string filename, IFile file)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns the number of free bytes in the filesystem.
        /// </summary>
        /// <returns>The number of free bytes in the filesystem.</returns>
        public int Free()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Deletes a file from the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to be deleted.</param>
        public void DeleteFile(string filename)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Rename a file in the filesystem.
        /// </summary>
        /// <param name="oldname">Old filename.</param>
        /// <param name="newname">New filename.</param>
        public void RenameFile(string oldname, string newname)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Verifies that a filename is valid for this filesystem.
        /// </summary>
        /// <param name="filename">Filename to validate.</param>
        /// <returns><value>true</value> if the filename is valid.</returns>
        public bool IsValidFilename(string filename)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Check whether a file exists.
        /// </summary>
        /// <param name="filename">Name of file.</param>
        /// <returns><value>true</value> if the file exists.</returns>
        public bool FileExists(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException(filename);

            var directory = ReadDirectory();
            return (FindDirectoryEntry(directory, filename) != -1);
        }


        /// <summary>
        /// Checks the filesystem consistency.
        /// </summary>
        /// <exception cref="FilesystemConsistencyException">Thrown if the filesystem is not consistent in a manner that makes write operations unsafe.</exception>
        public void Check()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            Disk.Dispose();
            Disk = null;
            IsDisposed = true;
        }


        /// <summary>
        /// Returns meta-information for a named file.
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>File meta-information object.</returns>
        public IFileInfo GetFileInfo(string filename)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns a file name object for manipulating a filename.
        /// </summary>
        IFileName IDiskFilesystem.GetFileName(string filename)
        {
            return GetFileName(filename);
        }


        /// <summary>
        /// Returns a file name object for manipulating a filename.
        /// </summary>
        public static FlexFileName GetFileName(string filename)
        {
            return new FlexFileName(filename);
        }

        /// <summary>
        /// Check whether a given sector is marked as allocated by the filesystem.
        /// </summary>
        /// <param name="head">Head</param>
        /// <param name="track">Track</param>
        /// <param name="sector">Sector</param>
        /// <returns><value>true</value> if the sector is marked as allocated, otherwise <value>false</value>.</returns>
        public bool IsSectorAllocated(int head, int track, int sector)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Read the entire disk directory.
        /// </summary>
        /// <returns>An array of directory entries.</returns>
        internal FlexDirectoryEntry[] ReadDirectory()
        {
            var directory = new List<FlexDirectoryEntry>();
            int sectorIndex = DirectoryIndex;
            while (sectorIndex != 0)
            {
                var sector = ReadSector(sectorIndex);
                sectorIndex = (sector[0] << 8) | sector[1];
                for (int i = 0; i < 10; i++ )
                {
                    directory.Add(new FlexDirectoryEntry(sector, 16+i*24));
                }
            }
            return directory.ToArray();
        }

        
        /// <summary>
        /// Locates the directory entry corresponding to a given filename.
        /// </summary>
        /// <param name="directory">Directory entries.</param>
        /// <param name="filename">Name of file to locate.</param>
        /// <returns>The index of the directory entry matching the filename, or -1 if the file was not found.</returns>
        /// <seealso cref="ReadDirectory"/>
        internal int FindDirectoryEntry(FlexDirectoryEntry[] directory, string filename)
        {
            for (int i = 0; i < directory.Length; i++)
            {
                if (directory[i].IsValid && string.Equals(directory[i].Filename, filename))
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Convert a FLEX sector index to head, track and sector number.  Flex numbers sectors in a logical sequence starting at 1.
        /// </summary>
        /// <param name="index">Sector index, starting at 1.</param>
        /// <param name="head">Disk head</param>
        /// <param name="track">Disk track</param>
        /// <param name="sector">Disk sector</param>
        internal void IndexToSector(int index, out int head, out int track, out int sector)
        {
            track = (index-1) / (Sectors * Disk.Heads);
            head = (index-1) % (Sectors * Disk.Heads) / Sectors;
            sector = (index-1) % (Sectors * Disk.Heads) % Sectors + 1;
        }


        /// <summary>
        /// Reads a sector addressed by its sector index.
        /// </summary>
        /// <param name="index">Sector index</param>
        /// <returns>Sector payload data</returns>
        internal byte[] ReadSector(int index)
        {
            int head, track, sector;
            IndexToSector(index, out head, out track, out sector);
            return Disk.ReadSector(head, track, sector);
        }


    }
}
