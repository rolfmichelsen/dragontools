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
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    /// <summary>
    /// Provides support for OS9 filesystems as defined by the OS9 RBFMAN module.
    /// </summary>
    public sealed class OS9 : IDiskHierarchicalFilesystem
    {
        /// <summary>
        /// Location (LSN) of the disk identification sector.
        /// </summary>
        private const int IdentificationSector = 0;

        /// <summary>
        /// Location (LSN) of the disk allocation map sector.  The allocation map is maintained in a single sector.
        /// </summary>
        private const int AllocationMapSector = 1;

        /// <summary>
        /// Valid path separator characters.
        /// </summary>
        public static readonly char[] PathSeparator = new char[] {'/'};

        /// <summary>
        /// This flag is set when the object is disposed.
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Filename comparison will be case sesitive when this flag is set.
        /// </summary>
        public bool IsCaseSensitive = false;

        /// <summary>
        /// The disk containing the filesystem.
        /// </summary>
        public IDisk Disk { get; private set; }


        /// <summary>
        /// <value>true</value> if the filesystem supports write operations.
        /// </summary>
        public bool IsWriteable { get; private set; }


        private readonly int Tracks;
        private readonly int Sectors;

        private OS9DiskInfo DiskInfo = null;
        private OS9AllocationMap AllocationMap = null;


        public OS9(IDisk disk, bool iswriteable)
        {
            if (disk == null) throw new ArgumentNullException();

            Disk = disk;
            IsWriteable = iswriteable;
            Tracks = Disk.Tracks;
            Sectors = Disk.Sectors * Disk.Heads;
            Disk.SectorWritten += SectorWrittenHandler;
        }


        /// <summary>
        /// Event handler for the SectorWritten event of the associated disk object.  
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private void SectorWrittenHandler(Object obj, SectorWrittenEventArgs e)
        {
            int lsn = SectorToLsn(e.Head, e.Track, e.Sector);
            if (lsn == IdentificationSector || lsn == AllocationMapSector)
            {
                DiskInfo = null;
                AllocationMap = null;
            }
        }


        /// <summary>
        /// Parse a pathname and return a collection of path components.
        /// </summary>
        /// <param name="path">Path to parse.</param>
        /// <returns>Path components.</returns>
        public IEnumerable<string> ParsePath(string path)
        {
            if (path == null) throw new ArgumentNullException();
            return path.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (IsDisposed) return;

            Disk.SectorWritten -= SectorWrittenHandler;
            Disk.Dispose();
            Disk = null;
            IsDisposed = true;
        }


        /// <summary>
        /// Returns a list of all files in filesystem root directory.
        /// </summary>
        /// <returns>A list containing the filename of all files in the filesystem root directory.</returns>
        public string[] ListFiles()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            ReadDiskHeader();
            var dir = ReadDirectory(DiskInfo.RootDirectory);
            var filename = new List<string>();
            foreach (var entry in dir)
            {
                if (entry.IsValid)
                    filename.Add(entry.Filename);
            }
            return filename.ToArray();
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
        /// Returns a list of all files in a given filesystem directory.
        /// </summary>
        /// <param name="directory">The directory to list, or <value>null</value> for the root directory.</param>
        /// <returns>A list containing the filenames of all files in the given directory.</returns>
        public string[] ListFiles(string directory)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (directory == null) return ListFiles();

            int sector = FindPath(directory);
            var dir = ReadDirectory(sector);
            var filenames = new List<string>();
            foreach (var entry in dir)
            {
                if (entry.IsValid)
                    filenames.Add(entry.Filename);
            }

            return filenames.ToArray();
        }


        /// <summary>
        /// Create a new directory.
        /// </summary>
        /// <param name="directory">Name of directory to create.</param>
        public void CreateDirectory(string directory)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///  Remove a directory.  The directory must be empty.
        /// </summary>
        /// <param name="directory">Name of directory to remove.</param>
        public void DeleteDirectory(string directory)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Reads a directory starting at a given disk sector.
        /// </summary>
        /// <param name="sector">First sector of the directory file.</param>
        /// <returns>A list of (filename, sector) tuples for the entries in the directory.</returns>
        internal List<OS9DirectoryEntry> ReadDirectory(int sector)
        {
            var header = OS9FileInfo.Parse(ReadSector(sector), null);
            if (!header.IsDirectory) throw new InvalidFileException();

            var dir = new List<OS9DirectoryEntry>();
            var raw = ReadFile(header);
            int direntries = raw.Length/OS9DirectoryEntry.RawEntrySize;
            for (int i = 0; i < direntries; i++ )
            {
                dir.Add(new OS9DirectoryEntry(raw, i));
            }

            return dir;
        }


        /// <summary>
        /// Searches a directory for a given filename and returns the filename sector if found.
        /// </summary>
        /// <param name="dir">Directory listing.</param>
        /// <param name="filename">Filename to search for.</param>
        /// <returns>The sector filename if the file is found, otherwise -1.</returns>
        internal int FindFile(IEnumerable<OS9DirectoryEntry> dir, string filename)
        {
            foreach (var entry in dir)
            {
                if (entry.IsValid && entry.Filename.Equals(filename, IsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase))
                {
                    return entry.Sector;
                }
            }
            return -1;
        }


        /// <summary>
        /// Searches the directory structure starting at the root directory for the given pathname and returns the first sector of
        /// the file.
        /// </summary>
        /// <param name="pathname">Pathname.</param>
        /// <returns>First sector of the identified file, or -1 if the file does not exist.</returns>
        internal int FindPath(string pathname)
        {
            var path = ParsePath(pathname);
            ReadDiskHeader();
            int sector = DiskInfo.RootDirectory;
            foreach (var leg in path)
            {
                var dir = ReadDirectory(sector);
                sector = FindFile(dir, leg);
                if (sector == -1) return -1;
            }
            return sector;
        }


        /// <summary>
        /// Write a file to the filesystem.
        /// </summary>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FileExistsException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemFullException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        public void WriteFile(IFile file)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns the number of free bytes in the filesystem.
        /// </summary>
        /// <returns>The number of free bytes in the filesystem.</returns>
        public int Free()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            ReadDiskHeader();
            int freeClusters = 0;
            int clusters = DiskInfo.TotalSectors/DiskInfo.ClusterSize;
            for (int i = 0; i < clusters; i++ )
            {
                if (!AllocationMap.IsAllocated(i))
                    freeClusters++;
            }
            return freeClusters*DiskInfo.ClusterSize*Disk.SectorSize;
        }



        /// <summary>
        /// Reads a file from the system and returns it, without parsing or removing any file headers or other file meta-infromation.
        /// The file is identified by passing the information from the file description sector.
        /// </summary>
        /// <param name="fileinfo">File description sector.</param>
        /// <returns>The raw data associated with this file.</returns>
        byte[] ReadFile(OS9FileInfo fileinfo)
        {
            var data = new byte[fileinfo.Size];
            if (fileinfo.Size == 0)
                return data;

            int offset = 0;
            foreach (var segment in fileinfo.Segments)
            {
                int sector = segment.Lsn;
                for (int i=0; i<segment.Size; i++)
                {
                    var sectordata = ReadSector(sector++);
                    int sectorsize = Math.Min(sectordata.Length, data.Length - offset);
                    Array.Copy(sectordata, 0, data, offset, sectorsize);
                    offset += sectorsize;
                    if (offset == fileinfo.Size)
                        return data;
                }
            }

            return data;
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
            if (filename == null) throw new ArgumentNullException();
            return FindPath(filename) != -1;
        }


        /// <summary>
        /// Checks the filesystem consistency.
        /// </summary>
        /// <exception cref="FilesystemConsistencyException">Thrown if the filesystem is not consistent in a manner that makes write operations unsafe.</exception>
        public void Check()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            ReadDiskHeader();

            var allocmap = new OS9AllocationMap(DiskInfo.AllocationMapSize);            // mirror the allocation map
            allocmap.SetAllocated(IdentificationSector/DiskInfo.ClusterSize, true);
            allocmap.SetAllocated(AllocationMapSector/DiskInfo.ClusterSize, true);

            var dirs = new Queue<int>();                        // LSN of all directories found while traversing the file structure
            dirs.Enqueue(DiskInfo.RootDirectory);
            VerifyAndUpdateAllocationMap(allocmap, DiskInfo.RootDirectory, DiskInfo.ClusterSize);

            /* Traverse the directory hierarchy and update the cluster allocation map for all files encountered. */
            while (dirs.Count > 0)
            {
                int lsn = dirs.Dequeue();
                var files = ReadDirectory(lsn);
                foreach (var file in files)
                {
                    if (file.IsValid && !String.Equals(file.Filename, ".") && !String.Equals(file.Filename, ".."))
                    {
                        VerifyAndUpdateAllocationMap(allocmap, file.Sector, DiskInfo.ClusterSize);
                        var fileinfo = OS9FileInfo.Parse(ReadSector(file.Sector), file.Filename);
                        if (fileinfo.IsDirectory)
                        {
                            dirs.Enqueue(file.Sector);
                        }
                    }
                }
            }
    
            /* Finally, verify that all clusters used by files found are actually marked as allocated in the disk cluster allocation map. */
            for (int i = 0; i < AllocationMap.AllocationMapSize; i++ )
            {
                if (allocmap.IsAllocated(i) && !AllocationMap.IsAllocated(i))
                {
                    throw new FilesystemConsistencyException(String.Format("Cluster {0} is in use by a file but not marked as allocated", i));
                }
            }
        }


        /// <summary>
        /// Reads the file descriptor sector for a given file and verified that all sectors in the file segment list (and the file descriptor sector itself)
        /// are marked as free in the passed cluster allocation map.  It then marks the clusters as marked.
        /// </summary>
        /// <param name="map">Cluster allocation map.</param>
        /// <param name="filedesc">Logical sector number of the file descriptor sector for the file.</param>
        /// <param name="clustersize">Cluster size for this filesystem.</param>
        /// <exception cref="FilesystemConsistencyException">Thrown if any cluster used for this file is already allocated.</exception>
        private void VerifyAndUpdateAllocationMap(OS9AllocationMap map, int filedesc, int clustersize)
        {
            var fileinfo = OS9FileInfo.Parse(ReadSector(filedesc), null);

            /* Verify that all sectors used by this file are marked as free in the cluster allocation map. */
            if (map.IsAllocated(filedesc/clustersize)) throw new FilesystemConsistencyException(String.Format("Sector {0} is in use by multiple files", filedesc));
            foreach (var segment in fileinfo.Segments)
            {
                int sector = segment.Lsn;
                for (int i=0; i<segment.Size; i++)
                {
                    if (map.IsAllocated(sector/clustersize)) throw new FilesystemConsistencyException(String.Format("Sector {0} is in use by multiple files", sector));
                    sector++;
                }
            }

            /* Mark all sectors as allocated. */
            map.SetAllocated(filedesc/clustersize, true);
            foreach (var segment in fileinfo.Segments)
            {
                int sector = segment.Lsn;
                for (int i=0; i<segment.Size; i++)
                {
                    map.SetAllocated(sector/clustersize, true);
                    sector++;
                }
            }
        }




        /// <summary>
        /// Create an empty filesystem.
        /// </summary>
        public void Initialize()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns meta-information for a named file.
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>File meta-information object.</returns>
        public IFileInfo GetFileInfo(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException();

            int sector = FindPath(filename);
            if (sector == -1) throw new FileNotFoundException(filename);

            var raw = ReadSector(sector);
            return OS9FileInfo.Parse(raw, filename);
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
        /// <param name="filename">The complete pathname of the file.</param>
        public static OS9FileName GetFileName(string filename)
        {
            return new OS9FileName(filename);
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
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            ReadDiskHeader();
            int cluster = SectorToLsn(head, track, sector)/DiskInfo.ClusterSize;
            return AllocationMap.IsAllocated(cluster);
        }


        public override string ToString()
        {
            ReadDiskHeader();
            return String.Format("OS-9 Filesystem (Sides={0} Tracks={1} Sectors={2} Sector Size={3} Cluster Size={4})",
                Disk.Heads, Disk.Tracks, Disk.Sectors, Disk.SectorSize, DiskInfo.ClusterSize);
        }


        internal int SectorToLsn(int head, int track, int sector)
        {
            return track * Sectors + head * Disk.Sectors + sector;
        }

        internal void LsnToSector(int lsn, out int head, out int track, out int sector)
        {
            track = lsn / (Disk.Sectors * Disk.Heads);
            head = lsn % (Disk.Sectors * Disk.Heads) / Disk.Sectors;
            sector = lsn % (Disk.Sectors * Disk.Heads) % Disk.Sectors;
        }

        internal byte[] ReadSector(int lsn)
        {
            int head, track, sector;
            LsnToSector(lsn, out head, out track, out sector);
            return Disk.ReadSector(head, track, sector);
        }

        internal void WriteSector(int lsn, byte[] data, int offset, int length)
        {
            int head, track, sector;
            LsnToSector(lsn, out head, out track, out sector);
            Disk.WriteSector(head, track, sector, data, offset, length);
        }


        internal void ReadDiskHeader()
        {
            if (DiskInfo == null || AllocationMap == null)
            {
                DiskInfo = new OS9DiskInfo(ReadSector(IdentificationSector));
                AllocationMap = new OS9AllocationMap(ReadSector(AllocationMapSector), DiskInfo.AllocationMapSize);
            }
        }

    }
}
