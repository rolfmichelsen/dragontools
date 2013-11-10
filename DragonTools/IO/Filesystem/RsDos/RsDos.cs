/*
Copyright (c) 2011-2013, Rolf Michelsen
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
using System.Text.RegularExpressions;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.RsDos
{
    /// <summary>
    /// Provides support for a RSDos filesystem used by the Tandy CoCo.
    /// </summary>
    public sealed class RsDos : IDiskFilesystem
    {
        public bool IsWriteable { get; private set; }
        public IDisk Disk { get; private set; }
        private bool IsDisposed = false;
        
        /// <summary>
        /// Pattern that defines valid filenames in the RSDos filesystem.
        /// </summary>
        private const string ValidFilenamePattern = @"^[a-z0-9][a-z0-9-]{1,7}(\.[a-z0-9]{0,3})?$";


        /// <summary>
        /// The number of sectors in each granule.
        /// </summary>
        private const int GranuleSectors = 9;


        /// <summary>
        /// The number of bytes in each granule.
        /// </summary>
        private const int GranuleSize = 2304;


        /// <summary>
        /// The total number of granules on a disk.
        /// </summary>
        private const int GranuleCount = 68;


        /// <summary>
        /// Sectors per track.
        /// </summary>
        private const int Sectors = 18;


        /// <summary>
        /// Bytes per sector.
        /// </summary>
        private const int SectorSize = 256;


        /// <summary>
        /// RSDos directory track.
        /// </summary>
        private const int DirectoryTrack = 17;


        /// <summary>
        /// The sector offset of the granule allocation map within the directory track.
        /// </summary>
        private const int AllocationMapSector = 1;


        private const int DirectoryEntrySector = 2;
        private const int DirectoryEntrySectorCount = 9;
        private const int DirectoryEntrySize = 32;
        private const int DirectoryEntryCount = 8;


        /// <summary>
        /// If set, all filename comparisons are made to be case sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; set; }


        /// <summary>
        /// Instantiate an RSDos filesystem representation associated with a virtual disk.
        /// </summary>
        /// <param name="disk">Disk to associate the filesystem to.</param>
        /// <param name="isWriteable">If set write operations will be permitted to the filesystem.</param>
        public RsDos(IDisk disk, bool isWriteable)
        {
            if (disk == null) throw new ArgumentNullException("disk");
            if (disk.Heads != 1 || disk.Tracks != 35) 
                throw new DiskImageFormatException(string.Format("Invalid disk geometry {0} heads {1} tracks", disk.Heads, disk.Tracks));

            Disk = disk;
            IsWriteable = isWriteable;
            IsDisposed = false;
            IsCaseSensitive = false;
        }


        /// <summary>
        /// Returns a list of all files in filesystem.
        /// </summary>
        /// <returns>A list containing the filename of all files in the filesystem.</returns>
        public string[] ListFiles()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            var files = new List<string>();
            var directory = ReadDirectory();
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
        /// Returns the number of free bytes in the filesystem.
        /// </summary>
        /// <returns>The number of free bytes in the filesystem.</returns>
        public int Free()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            var granules = ReadGranuleMap();
            int free = 0;
            foreach (var granule in granules)
            {
                if (granule == 0xff) free++;
            }
            return free * GranuleSize;
        }


        /// <summary>
        /// Read a raw file and return it as a byte array.  The file is not parsed in any way and all filesystem headers and meta-data are included
        /// in the returned byte array.
        /// </summary>
        /// <param name="filename">Name of file to read.</param>
        /// <returns>Raw file contents.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        private byte[] ReadFileRaw(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException();

            var directory = ReadDirectory();
            var dirinx = FindDirectoryEntry(directory, filename);
            if (dirinx == -1) throw new FileNotFoundException(filename);
            var granulemap = ReadGranuleMap();
            var direntry = directory[dirinx];
            var granules = GetGranuleChain(direntry.FirstGranule, granulemap);
            return ReadFileData(granules, direntry.LastSectorSize);
        }


        /// <summary>
        /// Write a file to the filesystem.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FileExistsException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemFullException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        public void WriteFile(string filename, IFile file)
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
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException("filename");
            return Regex.IsMatch(filename, ValidFilenamePattern, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Check whether a file exists.
        /// </summary>
        /// <param name="filename">Name of file.</param>
        /// <returns><value>true</value> if the file exists.</returns>
        public bool FileExists(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException("filename");

            var directory = ReadDirectory();
            return (FindDirectoryEntry(directory, filename) != -1);
        }


        /// <summary>
        /// Checks the filesystem consistency.
        /// </summary>
        /// <exception cref="FilesystemConsistencyException">Thrown if the filesystem is not consistent in a manner that makes write operations unsafe.</exception>
        public void Check()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            var directory = ReadDirectory();
            var granulemap = ReadGranuleMap();
            var granuleused = new bool[granulemap.Length];
            Array.Clear(granuleused, 0, granuleused.Length);

            foreach (var entry in directory)
            {
                if (entry.IsValid)
                {
                    var granulechain = GetGranuleChain(entry.FirstGranule, granulemap);
                    foreach (var granule in granulechain.granules)
                    {
                        if (granuleused[granule]) throw new FilesystemConsistencyException(String.Format("Granule {0} is used by several files", granule));
                        granuleused[granule] = true;
                    }
                }
            }
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
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException("filename");

            var directory = ReadDirectory();
            int dirinx = FindDirectoryEntry(directory, filename);
            if (dirinx == -1) throw new FileNotFoundException("filename");
            var granulemap = ReadGranuleMap();
            var granulechain = GetGranuleChain(directory[dirinx].FirstGranule, granulemap);
            int size = GetFileSize(granulechain, directory[dirinx].LastSectorSize);

            return new RsDosFileInfo(filename, size);
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
        public static RsDosFileName GetFileName(string filename)
        {
            return new RsDosFileName(filename);
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
            if (!Disk.SectorExists(head, track, sector)) throw new SectorNotFoundException(head, track, sector);

            if (track == DirectoryTrack) return true;

            var granulemap = ReadGranuleMap();
            var granule = SectorToGranule(head, track, sector);
            return (granulemap[granule] != 0xff);
        }


        /// <summary>
        /// Returns a concise string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("RSDOS Filesystem (Sides={0} Tracks={1}", 
                                 Disk.Heads, Disk.Tracks);
        }


        /// <summary>
        /// Returns the logical sector number (LSN) for the first sector of a given granule.
        /// </summary>
        /// <param name="granule">The granule, starting at 0.</param>
        /// <returns>The LSN of the first sector of this granule.</returns>
        internal int GranuleToLsn(int granule)
        {
            int track = (granule < 34) ? granule/2 : granule/2 + 1;
            int sector = (granule%2 == 0) ? 0 : GranuleSectors;
            return track*Sectors*Disk.Heads + sector;
        }



        /// <summary>
        /// Returns the logical sector number (LSN) of the first sector of a given track.
        /// </summary>
        /// <param name="track">Track number, starting at 0.</param>
        /// <returns>The LSN of the first sector of this track.</returns>
        internal int TrackToLsn(int track)
        {
            return track * Sectors * Disk.Heads;
        }


        internal int SectorToGranule(int head, int track, int sector)
        {
            int lsn = track*Disk.Heads*Sectors + head*Sectors + sector;
            return (track < DirectoryTrack) ? lsn/GranuleSectors : (lsn-Disk.Heads*Sectors)/GranuleSectors;
        }


        internal void LsnToSector(int lsn, out int head, out int track, out int sector)
        {
            track = lsn / (Sectors * Disk.Heads);
            head = lsn % (Sectors * Disk.Heads) / Sectors;
            sector = lsn % (Sectors * Disk.Heads) % Sectors + 1;
        }


        internal byte[] ReadSector(int lsn)
        {
            int head, track, sector;
            LsnToSector(lsn, out head, out track, out sector);
            return Disk.ReadSector(head, track, sector);
        }



        /// <summary>
        /// Read the entire disk directory.
        /// </summary>
        /// <returns>An array of directory entries.</returns>
        internal RsDosDirectoryEntry[] ReadDirectory()
        {
            var directory = new List<RsDosDirectoryEntry>();
            var lsn = TrackToLsn(DirectoryTrack) + DirectoryEntrySector;
            for (int i = 0; i < DirectoryEntrySectorCount; i++)
            {
                var raw = ReadSector(lsn++);
                for (int j = 0; j < DirectoryEntryCount; j++)
                {
                    directory.Add(new RsDosDirectoryEntry(raw, j * DirectoryEntrySize));
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
        internal int FindDirectoryEntry(RsDosDirectoryEntry[] directory, string filename)
        {
            for (int i=0; i<directory.Length; i++)
            {
                if (directory[i].IsValid && string.Equals(directory[i].Filename, filename, IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }



        /// <summary>
        /// Reads the granule map and returns it.
        /// </summary>
        /// <returns>An array containing the granule map.</returns>
        internal byte[] ReadGranuleMap()
        {
            var sector = ReadSector(TrackToLsn(DirectoryTrack) + AllocationMapSector);
            var granulemap = new byte[GranuleCount];
            Array.Copy(sector, granulemap, GranuleCount);
            return granulemap;
        }


        /// <summary>
        /// Returns the granule chain for a file starting at a given granule.
        /// </summary>
        /// <param name="granule">First granule of the file chain.</param>
        /// <param name="granulemap">Granule map.</param>
        /// <returns>The granule chain for the file.</returns>
        /// <seealso cref="ReadGranuleMap" />
        internal GranuleChain GetGranuleChain(int granule, byte[] granulemap)
        {
            var granulelist = new List<int>();
            while ((granule & 0x80) == 0)
            {
                if (granule < 0 || granule >= granulemap.Length) throw new FilesystemConsistencyException(String.Format("Granule index {0} is outside the permitted range of 0-{1}", granule, granulemap.Length - 1));
                granulelist.Add(granule);
                granule = (int) granulemap[granule];
            }
            
            if (granule == 0xff) throw new FilesystemConsistencyException(string.Format("Granule {0} is part of file chain but marked as free", granule));
            if (granule < 0xc0 || granule > 0xc9) throw new FilesystemConsistencyException(string.Format("Unexpected granule map value {0}", granule));
            int sectors = granule & 0x0f;

            return new GranuleChain {granules = granulelist.ToArray(), sectors = sectors};
        }



        internal int GetFileSize(GranuleChain granulechain, int lastsectorsize)
        {
            int completegranules = Math.Max(0, granulechain.granules.Length - 1) * GranuleSize;
            int lastgranule = (granulechain.sectors == 0)
                                  ? 0
                                  : (granulechain.sectors - 1)*SectorSize + lastsectorsize;
            return completegranules + lastgranule;
        }



        internal byte[] ReadFileData(GranuleChain granulechain, int lastsectorsize)
        {
            int filesize = GetFileSize(granulechain, lastsectorsize);
            var data = new byte[filesize];
            int dataoffset = 0;
            int granulecount = granulechain.granules.Length;                                    
            int sectorcount = (granulecount - 1)*GranuleSectors + granulechain.sectors;         // The total number of sectors holding file payload data

            for (int i = 0; i < granulecount; i++ )                                             // Once for each granule
            {
                int lsn = GranuleToLsn(granulechain.granules[i]);
                for (int j=0; j < Math.Min(GranuleSectors,sectorcount); j++)                    // Once for each granule sector, paying special attention to the last granule
                {
                    var sector = ReadSector(lsn++);
                    int sectorsize = Math.Min(SectorSize, filesize);                       // Pay special attention to the last sector
                    Array.Copy(sector, 0, data, dataoffset, sectorsize);
                    dataoffset += sectorsize;
                    filesize -= sectorsize;
                }
            }
            return data;
        }


        /// <summary>
        /// Represents a chain of granules making up a complete file.
        /// </summary>
        internal struct GranuleChain
        {
            /// <summary>
            /// Orderes list of granule indexes making up the file.
            /// </summary>
            public int[] granules;

            /// <summary>
            /// The number of sectors that are actually used in the last granule.
            /// </summary>
            public int sectors;
        }

    }
}
