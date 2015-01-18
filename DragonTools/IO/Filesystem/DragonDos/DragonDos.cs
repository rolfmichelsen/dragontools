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
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos
{
    /// <summary>
    /// Provides support for a DragonDos filesystem.
    /// DragonDos is the standard disk filesystem for the Dragon family of computers.
    /// It supports single or double sided disks, 40 or 80 trakcs, 18 sectors per track
    /// and a sector size of 256 bytes.  Heads and tracks are numbered from 0.  Sectors are
    /// numbered from 1.
    /// </summary>
    /// <remarks>
    /// This implementation is based primarily on information from "DragonDos: A Programmers Guide"
    /// (Grosvenor Software, 1985) and "Into the Dragon" by Paul Dagleish (Dragon User, May 1987).
    /// </remarks>
    public sealed class DragonDos : IDiskFilesystem
    {
        /// <summary>
        /// Primary directory track for DragonDos filesystems.
        /// </summary>
        public const int DirectoryTrackPrimary = 20;

        /// <summary>
        /// Backup directory track for DragonDos filesystems.
        /// </summary>
        public const int DirectoryTrackBackup = 16;

        /// <summary>
        /// Number of sectors per head per track supported by DragonDos.
        /// </summary>
        public const int SectorsPerHead = 18;

        private const string ValidFilenamePattern = @"^[a-z0-9][a-z0-9-]{1,7}(\.[a-z0-9]{0,3})?$";

        /// <summary>
        /// This array containts a copy of the directory track.  The first index is the sector number and the second index
        /// is the byte offset into this sector.
        /// </summary>
        private byte[][] directoryTrack = new byte[SectorsPerHead][];

        /// <summary>
        /// Set to indicate that the directory track cache does not reflect the actual directory on disk.  The cache must be
        /// re-populated before using it.
        /// </summary>
        private bool directoryIsDirty = true;

        /// <summary>
        /// Number of disk tracks.  This is 40 or 80 for DragonDos disks.
        /// </summary>
        public int Tracks { get; private set; }

        /// <summary>
        /// Number of sectors per track.  This is 18 for single-sided or 36 for double-sided disks.
        /// </summary>
        public int Sectors { get; private set; }

        /// <summary>
        /// Size (in bytes) of a single sector.
        /// </summary>
        public const int SectorSize = 256;

        /// <summary>
        /// If set, all filename comparisons are made to be case sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; set; }


        /// <summary>
        /// Set when the object has been disposed.
        /// </summary>
        private bool IsDisposed = false;

        /// <summary>
        /// Set if the filesystem supports write operations.
        /// </summary>
        public bool IsWriteable { get; private set; }

        /// <summary>
        /// Reference to the virtual disk where the filesystem is hosted.
        /// </summary>
        public IDisk Disk { get; private set; }


        /// <summary>
        /// Create a DragonDos filesystem interface to a virtual diskette.
        /// </summary>
        /// <param name="disk">Virtual diskette holding the filesystem.</param>
        /// <param name="isWriteable">Set if the filesystem will permit write operations.</param>
        /// <exception cref="UnsupportedGeometryException">The disk geometry is not supported by DragonDos.</exception>
        public DragonDos(IDisk disk, bool isWriteable)
        {
            if (disk == null) throw new ArgumentNullException("disk");
            if (disk.Heads > 2) throw new UnsupportedGeometryException("DragonDos only supports single or double sided disks");
            if (disk.Tracks != 40 && disk.Tracks != 80) throw new UnsupportedGeometryException("DragonDos only supports 40 or 80 track disks");

            IsCaseSensitive = false;
            Disk = disk;
            Tracks = disk.Tracks;
            Sectors = disk.Heads*SectorsPerHead;
            IsWriteable = isWriteable;
            IsDisposed = false;
            Disk.SectorWritten += SectorWrittenHandler;

            ReadDirectoryTrack();

            int tracks = directoryTrack[0][252];
            int sectors = directoryTrack[0][253];
            if (tracks != Tracks) throw new FilesystemConsistencyException(String.Format("Unexpected number of tracks {0}", tracks));
            if (sectors != SectorsPerHead*disk.Heads) throw new FilesystemConsistencyException(String.Format("Unexpected number of sectors per track {0}", sectors));
        }


        /// <summary>
        /// Returns a list of all files in filesystem.
        /// </summary>
        /// <returns>A list containing the filename of all files in the filesystem.</returns>
        public string[] ListFiles()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            var files = new List<string>();
            ReadDirectoryTrack();
            for (int i = 0; i < DirectoryEntries; i++ )
            {
                var dir = GetDirectoryEntry(i);
                if (dir.IsMainEntry && dir.IsValid)
                    files.Add(dir.Filename);
            }
            return files.ToArray();
        }


        /// <summary>
        /// Returns the number of free bytes in the filesystem.
        /// </summary>
        /// <returns>The number of free bytes in the filesystem.</returns>
        public int Free()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            ReadDirectoryTrack();
            int free = 0;
            int sectors = Tracks*Sectors;
            for (int i = 0; i < sectors; i++ )
            {
                if (!IsSectorAllocated(i))
                    free++;
            }
            return free*SectorSize;
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
            if (filename == null) throw new ArgumentNullException("filename");
            var info = GetDirectoryItem(filename);
            return GetFileData(info);
        }

        
        /// <summary>
        /// Read and parse a file.  The returned object contains the file data and any meta-information related to the file.
        /// </summary>
        /// <param name="filename">Name of file to read.</param>
        /// <returns>File object.</returns>
        /// <exception cref="InvalidFileException">The file format is invalid.</exception>
        public IFile ReadFile(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException("filename");
            return DragonDosFile.DecodeFile((DragonDosFileInfo)GetFileInfo(filename), ReadFileRaw(filename));
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
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (!IsWriteable) throw new FilesystemNotWriteableException();
            if (file == null) throw new ArgumentNullException("file");
            if (!(file is DragonDosFile)) throw new ArgumentException("file is of unexpected type " + file.GetType().FullName);
            var dragonfile = (DragonDosFile) file;
            WriteFileRaw(filename, dragonfile.EncodeFile());
        }


        /// <summary>
        /// Write a raw file to the filesystem.  The data must include any filesystem headers and meta-data required by the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="data">Raw file data.</param>
        /// <exception cref="FileExistsException">A file with the specified name already exists.</exception>
        /// <exception cref="FilesystemFullException">The filesystem does not have remaining capacity to store the file.</exception>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        private void WriteFileRaw(string filename, byte[] data)
        {
            if (!IsValidFilename(filename)) throw new InvalidFilenameException(filename);
            if (FileExists(filename)) throw new FileExistsException(filename);

            /* Compute the number of sectors needed for storing the file and find a list of extents for storing the file data. */
            ReadDirectoryTrack();
            int sectorCount = (data.Length + SectorSize - 1) / SectorSize;
            int lastSectorSize = data.Length - (sectorCount - 1) * SectorSize;
            var extents = FindFreeExtents(sectorCount);
            if (extents == null) throw new FilesystemFullException(data.Length);

            /* Compute the number of directory entries needed to store information about this file and create a list of free directory
             * entries to use. */
            var dirInx = FindFreeDirectoryEntries(extents.Length > 4 ? (extents.Length + 2) / 7 : 1);
            if (dirInx == null) throw new DirectoryFullException();

            /* Write file payload to disk and mark sectors as allocated. */
            WriteFileData(data, extents);

            /* Create directory entries. */
            var dirEntries = CreateDirectoryEntries(filename, extents, dirInx, lastSectorSize);
            
            /* Write the directory entries to the directory track. */
            for (int i = 0; i < dirInx.Length; i++ )
            {
                SetDirectoryEntry(dirInx[i], dirEntries[i]);
            }

            /* Write the directory track to disk. */
            directoryIsDirty = false;
            WriteDirectoryTrack(DirectoryTrackPrimary);
            directoryIsDirty = false;
            WriteDirectoryTrack(DirectoryTrackBackup);
        }


        /// <summary>
        /// Return an array of extents that combined have the capacity to store the required number of sectors.  All sectors must be marked as unallocated by
        /// the filesystem to be returned as part of an extent.
        /// </summary>
        /// <param name="sectors">Number of sectors needed.</param>
        /// <returns>List of extents, or <value>null</value> if a sufficient number of free extents cannot be found.</returns>
        private DragonDosDirectoryEntry.Extent[] FindFreeExtents(int sectors)
        {
            int lsn = 0;
            while (lsn < Tracks*Sectors)
            {
                int len = FindFreeExtentLength(lsn, sectors);
                if (len == sectors)
                {
                    return new[] {new DragonDosDirectoryEntry.Extent(lsn, len)};
                }
                lsn += len + 1;
            }
            return FindFreeFragmentedExtents(sectors);
        }


        private int FindFreeExtentLength(int lsn, int maxLength)
        {
            int length = 0;
            while (!IsSectorAllocated(lsn) && length < maxLength && lsn < Tracks*Sectors)
            {
                length++;
                lsn++;
            }
            return length;
        }



        private DragonDosDirectoryEntry.Extent[] FindFreeFragmentedExtents(int sectors)
        {
            var extents = new List<DragonDosDirectoryEntry.Extent>();
            int lsn = 0;
            while (lsn < Tracks*Sectors && sectors > 0)
            {
                int len = FindFreeExtentLength(lsn, sectors);
                if (len > 0)
                {
                    extents.Add(new DragonDosDirectoryEntry.Extent(lsn, len));
                    sectors -= len;
                }
                lsn += len + 1;
            }
            return (sectors == 0) ? extents.ToArray() : null;
        }


        /// <summary>
        /// Create a set of directory entries for a file.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <param name="extents">Ordered list of extents containing the file payload data.</param>
        /// <param name="dirInx">Orderes list of directory entry indexes to use.</param>
        /// <returns>Orderes list of directory entries</returns>
        private DragonDosDirectoryEntry[] CreateDirectoryEntries(string filename, DragonDosDirectoryEntry.Extent[] extents, int[] dirInx, int lastSectorSize)
        {
            var dirEntries = new DragonDosDirectoryEntry[dirInx.Length];

            int maxExtents;
            int extentOffset = 0;
            for (int i = 0; i < dirInx.Length; i++ )
            {
                var dir = new DragonDosDirectoryEntry() {Flags = 0};
                if (i == 0)
                {
                    dir.Filename = filename;
                    dir.IsExtensionEntry = false;
                    maxExtents = 4;
                }
                else
                {
                    dir.IsExtensionEntry = true;
                    maxExtents = 7;
                }

                if (i == dirInx.Length-1)
                {
                    dir.LastSectorSize = lastSectorSize;
                }
                else
                {
                    dir.NextEntry = dirInx[i + 1];
                }

                dir.Extents = new DragonDosDirectoryEntry.Extent[Math.Min(maxExtents, extents.Length - extentOffset)];
                Array.Copy(extents, extentOffset, dir.Extents, 0, dir.Extents.Length);
                extentOffset += dir.Extents.Length;

                dirEntries[i] = dir;
            }

            return dirEntries;
        }



        /// <summary>
        /// Returns a collection of free directory entries.
        /// </summary>
        /// <param name="entries">The number of direcory entries needed.</param>
        /// <returns>A collection of directory entries, or <value>null</value> if a sufficient number of directory entries cannot be found.</returns>
        private int[] FindFreeDirectoryEntries(int entries)
        {
            var direntries = new List<int>();
            for (int i = 0; i < DirectoryEntries && entries > 0; i++ )
            {
                var entry = GetDirectoryEntry(i);
                if (!entry.IsValid)
                {
                    direntries.Add(i);
                    entries--;
                }
            }
            return entries == 0 ? direntries.ToArray() : null;
        }

        /// <summary>
        /// Deletes a file from the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to be deleted.</param>
        /// <exception cref="FileNotFoundException">The file does not exist</exception>
        public void DeleteFile(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException("filename");
            if (!IsWriteable) throw new FilesystemNotWriteableException();

            var info = GetDirectoryItem(filename);

            /* Mark all directory entries for this file as invalid. */
            foreach (var dirInx in info.DirEntries)
            {
                var dirEntry = GetDirectoryEntry(dirInx);
                dirEntry.IsValid = false;
                SetDirectoryEntry(dirInx, dirEntry);
            }

            /* Mark all sectors used by this file as unallocated. */
            foreach (var extent in info.Extents)
            {
                var lsn = extent.Lsn;
                for (int i=0; i<extent.Length; i++)
                {
                    SetSectorAllocated(lsn++, false);
                }
            }

            /* Write the modified primary and backup directory track back to disk. */
            WriteDirectoryTrack(DirectoryTrackPrimary);
            directoryIsDirty = false;
            WriteDirectoryTrack(DirectoryTrackBackup);
        }

        /// <summary>
        /// Rename a file in the filesystem.
        /// </summary>
        /// <param name="oldname">Old filename.</param>
        /// <param name="newname">New filename.</param>
        public void RenameFile(string oldname, string newname)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (oldname == null) throw new ArgumentNullException("oldname");
            if (newname == null) throw new ArgumentNullException("newname");
            if (!IsValidFilename(newname)) throw new InvalidFilenameException(newname);
            if (FileExists(newname)) throw new FileExistsException(newname);
            if (!IsWriteable) throw new FilesystemNotWriteableException();

            var info = GetDirectoryItem(oldname);
            var dirEntry = GetDirectoryEntry(info.DirEntries[0]);
            dirEntry.Filename = newname;
            SetDirectoryEntry(info.DirEntries[0], dirEntry);

            WriteDirectoryTrack(DirectoryTrackPrimary);
            directoryIsDirty = false;
            WriteDirectoryTrack(DirectoryTrackBackup);
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

            try
            {
                ReadDirectoryTrack();
                FindDirectoryEntry(filename);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the filesystem consistency.
        /// </summary>
        /// <exception cref="FilesystemConsistencyException">Thrown if the filesystem is not consistent in a manner that makes write operations unsafe.</exception>
        public void Check()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            /* Compare the primary and secondary directory track. */
            if (!ComparePrimarySecondaryDirectoryTrack())
            {
                throw new FilesystemConsistencyException("Primary and secondary directory track are not equal");
            }

            /* Each bit in this array represents a directory entry, set to false for entries that are not used. */
            var allocatedDirEntries = new BitArray(DirectoryEntries);

            /* Each bit in this array represents a disk sector, set to false for entries that are not used. */
            var allocatedSectors = new BitArray(Tracks*Sectors);

            /* Mark primary and secondary directory track sectors as allocated. */
            int lsn1 = TrackToLsn(DirectoryTrackPrimary);
            int lsn2 = TrackToLsn(DirectoryTrackBackup);
            for (int i=0; i<SectorsPerHead; i++)
            {
                allocatedSectors[lsn1++] = true;
                allocatedSectors[lsn2++] = true;
            }

            /* Find all main directory entries.  Update allocatedDirEntries and allocatedSectors. */
            ReadDirectoryTrack();
            for (int i=0; i<DirectoryEntries; i++)
            {
                var direntry = GetDirectoryEntry(i);
                if (direntry.IsMainEntry && direntry.IsValid)
                {
                    var item = GetDirectoryItem(i);
                    foreach (var dirinx in item.DirEntries)
                    {
                        if (allocatedDirEntries[dirinx])
                        {
                            throw new FilesystemConsistencyException(String.Format("Directory entry {0} is referenced twice", dirinx));
                        }
                        allocatedDirEntries[dirinx] = true;
                    }
                    foreach (var extent in item.Extents)
                    {
                        int lsn = extent.Lsn;
                        for (int j=0; j<extent.Length; j++)
                        {
                            if (allocatedSectors[lsn])
                            {
                                throw new FilesystemConsistencyException(String.Format("LSN {0} is used by two files", lsn));
                            }
                            allocatedSectors[lsn++] = true;
                        }
                    }
                }
            }

            /* Check that all sectors that are part of a file are actually marked as allocated in the sector allocation map.  We permit sectors to be marked as allocated
               in the sector allocated map even when they are not part of a file as this cannot cause data corruption in subsequent write operations to the disk. */
            for (int lsn = 0; lsn < Tracks * Sectors; lsn++ )
            {
                if (allocatedSectors[lsn] && !IsSectorAllocated(lsn))
                {
                    throw new FilesystemConsistencyException(string.Format("LSN {0} is in use by a file but is not marked as allocated by the filesystem", lsn));
                }
            }
        }


        /// <summary>
        /// Initialize an empty DragonDos filesystem on a disk.
        /// </summary>
        /// <param name="disk">Disk to initialize filesystem on.</param>
        /// <returns>DragonDos filesystem.</returns>
        /// <exception cref="DiskNotWriteableException">The disk does not support write operations.</exception>
        /// <exception cref="UnsupportedGeometryException">The disk geometry is not supported by DragonDos.</exception>
        public static DragonDos Initialize(IDisk disk)
        {
            if (disk == null) throw new ArgumentNullException("disk");
            if (!disk.IsWriteable) throw new DiskNotWriteableException();

            if (disk.Tracks != 40 && disk.Tracks != 80)
                throw new UnsupportedGeometryException(String.Format("DragonDos only supports 40 or 80 tracks while this disk has {0} tracks", disk.Tracks));
            if (disk.Heads != 1 && disk.Heads != 2)
                throw new UnsupportedGeometryException(String.Format("DragonDos only supports single or double sided disks while this disk has {0} sides", disk.Heads));

            /* Write blank data to all sectors. */
            var sectorData = new byte[SectorSize];
            for (int t = 0; t < disk.Tracks; t++)
            {
                for (int h = 0; h < disk.Heads; h++)
                {
                    for (int s = 1; s < SectorsPerHead; s++)
                    {
                        disk.WriteSector(h, t, s, sectorData);
                    }
                }
            }

            /* Write empty directory entries to the directory track. */
            var emptyDirEntry = DragonDosDirectoryEntry.GetEmptyEntry();
            var emptyEncodedEntry = new byte[DirectoryEntrySize];
            emptyDirEntry.Encode(emptyEncodedEntry, 0);
            for (int i = 0; i < DirectoryEntryCount; i++)
            {
                Array.Copy(emptyEncodedEntry, 0, sectorData, i*DirectoryEntrySize, DirectoryEntrySize);
            }
            for (int s = DirectoryEntryOffset+1; s <= SectorsPerHead; s++)
            {
                disk.WriteSector(0, DirectoryTrackPrimary, s, sectorData);
                disk.WriteSector(0, DirectoryTrackBackup, s, sectorData);
            }

            /* Write the sector allocation map. */
            var allocationmap = new byte[2][];
            allocationmap[0] = new byte[SectorSize];
            allocationmap[1] = new byte[SectorSize];

            int sectors = SectorsPerHead*disk.Heads;
            allocationmap[0][252] = (byte) disk.Tracks;                         // encode disk geometry
            allocationmap[0][253] = (byte) sectors;
            allocationmap[0][254] = (byte) (~disk.Tracks & 0xff);
            allocationmap[0][255] = (byte) (~sectors & 0xff);

            for (var i=0; i<disk.Tracks*disk.Heads*SectorsPerHead; i++)              // mark all sectors as unallocated
                SetSectorAllocated(i, false, allocationmap);

            int lsnPrimaryDirectory = DirectoryTrackPrimary*disk.Heads*SectorsPerHead;      // mark directory track as allocated
            int lsnBackupDirectory = DirectoryTrackBackup*disk.Heads*SectorsPerHead;
            for (var i = 0; i < SectorsPerHead; i++)
            {
                SetSectorAllocated(lsnPrimaryDirectory++, true, allocationmap);
                SetSectorAllocated(lsnBackupDirectory++, true, allocationmap);
            }

            for (var i = 0; i < 2; i++)
            {
                disk.WriteSector(0, DirectoryTrackPrimary, i+1, allocationmap[i]);
                disk.WriteSector(0, DirectoryTrackBackup, i+1, allocationmap[i]);        
            }

            return new DragonDos(disk, true);
        }



        internal bool ComparePrimarySecondaryDirectoryTrack()
        {
            int lsn1 = TrackToLsn(DirectoryTrackPrimary);
            int lsn2 = TrackToLsn(DirectoryTrackBackup);
            for (int i = 0; i < SectorsPerHead; i++ )
            {
                var sector1 = ReadSector(lsn1++);
                var sector2 = ReadSector(lsn2++);
                for (int j=0; j<SectorSize; j++)
                {
                    if (sector1[j] != sector2[j])
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;

            Disk.SectorWritten -= SectorWrittenHandler;
            Disk.Dispose();
            Disk = null;
            directoryTrack = null;

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

            var dosinfo = GetDirectoryItem(filename);

            return new DragonDosFileInfo(filename, dosinfo.Size, dosinfo.IsProtected);
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
        public static IFileName GetFileName(string filename)
        {
            return new DragonDosFileName(filename);
        }


        /// <summary>
        /// Returns the protection state for a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns><value>true</value> if the file is marked as protected by the filesystem, otherwise false.</returns>
        public bool IsProtected(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException();

            var info = (DragonDosFileInfo) GetFileInfo(filename);
            return info.IsProtected;
        }



        /// <summary>
        /// Set the protection state of a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="isprotected"><value>true</value> to set the protection state, otherwise the protection state will be cleared.</param>
        public void SetProtected(string filename, bool isprotected)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (filename == null) throw new ArgumentNullException();

            ReadDirectoryTrack();
            int dirinx = FindDirectoryEntry(filename);
            var dir = GetDirectoryEntry(dirinx);
            dir.IsProtected = isprotected;
            SetDirectoryEntry(dirinx, dir);

            /* Write the modified primary and backup directory track back to disk. */
            WriteDirectoryTrack(DirectoryTrackPrimary);
            directoryIsDirty = false;
            WriteDirectoryTrack(DirectoryTrackBackup);
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
            ReadDirectoryTrack();
            int lsn = SectorToLsn(head, track, sector);
            return IsSectorAllocated(lsn);
        }


        /// <summary>
        /// Reads the directory track from disk and populates the directory track cache.
        /// </summary>
        private void ReadDirectoryTrack()
        {
            if (!directoryIsDirty) return;
            for (int sector = 1; sector <= SectorsPerHead; sector++ )
            {
                var sectorData = Disk.ReadSector(0, DirectoryTrackPrimary, sector);
                if (sectorData.Length != SectorSize) 
                    throw new FilesystemConsistencyException(String.Format("Unexpected sector size of {0} bytes", sectorData.Length));
                directoryTrack[sector-1] = sectorData;
            }
            directoryIsDirty = false;
        }


        /// <summary>
        /// Writes the cached directory track back to disk.
        /// </summary>
        /// <param name="track">Track to write directory to.  This should be DirectoryTrackPrimary or DirectoryTrackBackup</param>
        /// <seealso cref="DirectoryTrackPrimary"/>
        /// <seealso cref="DirectoryTrackBackup"/>
        private void WriteDirectoryTrack(int track)
        {
            if (directoryIsDirty) throw new FilesystemConsistencyException();

            for (int sector = 1; sector <= SectorsPerHead; sector++ )
            {
                Disk.WriteSector(0, track, sector, directoryTrack[sector-1]);
            }
        }


        /// <summary>
        /// Event handler for the SectorWritten event of the associated disk object.  
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private void SectorWrittenHandler(Object obj, SectorWrittenEventArgs e)
        {
            if (e.Track == DirectoryTrackPrimary)
            {
                directoryIsDirty = true;
            }
        }


        /// <summary>
        /// The number of sectors that can be represented by the allocation map in the first sector of the directory track.  Up to 180 bytes are
        /// reserved for the sector allocation map.
        /// </summary>
        private const int EntriesInFirstAllocSector = 180*8;


        /// <summary>
        /// Returns true if the given sector is marked as allocated by the filesystem.
        /// </summary>
        /// <param name="lsn">LSN of sector</param>
        /// <returns>True if the sector is marked as allocated.</returns>
        private bool IsSectorAllocated(int lsn)
        {
            int sector = 0;
            if (lsn >= EntriesInFirstAllocSector)
            {
                sector = 1;
                lsn -= EntriesInFirstAllocSector;
            }
            
            return (directoryTrack[sector][lsn/8] & (1 << (lsn%8))) == 0;
        }


        /// <summary>
        /// Marks a sector as allocated or unallocated by the filesystem.
        /// </summary>
        /// <remarks>
        /// This method will not write the modified directory track back to disk.  This is the responsibility of the caller.
        /// </remarks>
        /// <param name="lsn">LSN of sector.</param>
        /// <param name="allocated">True if the sector is to be marked as allocated.</param>
        private void SetSectorAllocated(int lsn, bool allocated)
        {
            SetSectorAllocated(lsn, allocated, directoryTrack);
        }


        /// <summary>
        /// Marks a sector as allocated or unallocated by the filesystem.
        /// </summary>
        /// <remarks>
        /// This method will not write the modified directory track back to disk.  This is the responsibility of the caller.
        /// </remarks>
        /// <param name="lsn">LSN of sector.</param>
        /// <param name="allocated">True if the sector is to be marked as allocated.</param>
        /// <param name="allocationmap">Byte array containing the allocation map.</param>
        private static void SetSectorAllocated(int lsn, bool allocated, byte[][] allocationmap)
        {
            int offset = 0;
            if (lsn >= EntriesInFirstAllocSector)
            {
                offset = 1;
                lsn -= EntriesInFirstAllocSector;
            }

            if (allocated)
            {
                allocationmap[offset][lsn/8] &= (byte) ~(1 << lsn%8);
            }
            else
            {
                allocationmap[offset][lsn/8] |= (byte) (1 << lsn%8);
            }
        }


        /// <summary>
        /// The encoded size (in bytes) of a directory entry.
        /// </summary>
        private const int DirectoryEntrySize = 25;

        /// <summary>
        /// The number of directory entries per sector in the directory track.
        /// </summary>
        private const int DirectoryEntryCount = 10;

        /// <summary>
        /// Offset of the first sector in the directory track containing directory track entries.
        /// </summary>
        private const int DirectoryEntryOffset = 2;

        /// <summary>
        /// Total number of directory entries in the directory track.
        /// </summary>
        private int DirectoryEntries = (SectorsPerHead - DirectoryEntryOffset)*DirectoryEntryCount;

        /// <summary>
        /// Returns the directory entry at a given index.
        /// </summary>
        /// <param name="index">Index of directory entry to return.</param>
        /// <returns>Directory entry.</returns>
        private DragonDosDirectoryEntry GetDirectoryEntry(int index)
        {
            int sector = index/DirectoryEntryCount + DirectoryEntryOffset;
            int offset = index%DirectoryEntryCount*DirectoryEntrySize;
            return new DragonDosDirectoryEntry(directoryTrack[sector], offset);
        }

        /// <summary>
        /// Sets the directory entry at a given index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="entry"></param>
        private void SetDirectoryEntry(int index, DragonDosDirectoryEntry entry)
        {
            int sector = index / DirectoryEntryCount + DirectoryEntryOffset;
            int offset = index % DirectoryEntryCount * DirectoryEntrySize;
            entry.Encode(directoryTrack[sector], offset);
        }


        /// <summary>
        /// Returns the logical sector number (LSN) of the first sector of a given track.
        /// </summary>
        /// <param name="track">Track number, starting at 0.</param>
        /// <returns>The LSN of the first sector of this track.</returns>
        internal int TrackToLsn(int track)
        {
            return track*Sectors;
        }


        internal int SectorToLsn(int head, int track, int sector)
        {
            return track*Sectors + head*SectorsPerHead + sector - 1;
        }

        internal void LsnToSector(int lsn, out int head, out int track, out int sector)
        {
            track = lsn/(SectorsPerHead*Disk.Heads);
            head = lsn%(SectorsPerHead*Disk.Heads) / SectorsPerHead;
            sector = lsn % (SectorsPerHead * Disk.Heads) % SectorsPerHead + 1;
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


        /// <summary>
        /// Returns the directory information associated with a given filename.
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>Directory information for this file.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist in the filesystem.</exception>
        internal DragonDosDirectoryItem GetDirectoryItem(string filename)
        {
            ReadDirectoryTrack();
            return GetDirectoryItem(FindDirectoryEntry(filename));
        }

        /// <summary>
        /// Returns the directory information associated with a main directory entry at a given index.
        /// </summary>
        /// <param name="dirIndex">Index of the main directory entry.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist in the filesystem.</exception>
        internal DragonDosDirectoryItem GetDirectoryItem(int dirIndex)
        {
            ReadDirectoryTrack();

            var extents = new List<DragonDosDirectoryEntry.Extent>();
            var direntries = new List<int>();
            var item = new DragonDosDirectoryItem();

            var dirent = GetDirectoryEntry(dirIndex);
            item.Filename = dirent.Filename;
            item.IsProtected = dirent.IsProtected;
            direntries.Add(dirIndex);
            foreach (var extent in dirent.Extents)
            {
                extents.Add(extent);
            }

            while (dirent.IsExtended)
            {
                dirIndex = dirent.NextEntry;
                dirent = GetDirectoryEntry(dirIndex);
                direntries.Add(dirIndex);
                foreach (var extent in dirent.Extents)
                {
                    extents.Add(extent);
                }
            }

            item.Extents = extents.ToArray();
            item.DirEntries = direntries.ToArray();
            item.LastSectorSize = dirent.LastSectorSize;

            return item;
            
        }


        /// <summary>
        /// Returns the index of the main entry in the filesystem directory for a given file.
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>Index of main directory entry for this file (0 based).</returns>
        /// <exception cref="FileNotFoundException">Thrown if the file does not exist.</exception>
        private int FindDirectoryEntry(string filename)
        {
            for (int i=0; i<DirectoryEntries; i++)
            {
                var dir = GetDirectoryEntry(i);
                if (dir.IsMainEntry && dir.IsValid && string.Equals(filename, dir.Filename, IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
                if (dir.IsEndOfDirectory)
                    break;
            }
            throw new FileNotFoundException(filename);
        }


        /// <summary>
        /// Returns the payload data for a given file.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal byte[] GetFileData(DragonDosDirectoryItem item)
        {
            int size = item.Size;
            byte[] data = new byte[size];
            int offset = 0;
            foreach (var extent in item.Extents)
            {
                int lsn = extent.Lsn;
                int len = extent.Length;
                while (len-- > 0)
                {
                    var sector = ReadSector(lsn++);
                    Array.Copy(sector, 0, data, offset, Math.Min(size, SectorSize));
                    size -= SectorSize;
                    offset += SectorSize;
                }
            }
            return data;
        }


        /// <summary>
        /// Writes file payload to the filesystem and mark the sectors as allocated.
        /// </summary>
        /// <param name="data">Data to write to the filesystem.</param>
        /// <param name="extents">Information about extents to write the data to.</param>
        internal void WriteFileData(byte[] data, DragonDosDirectoryEntry.Extent[] extents)
        {
            int dataOffset = 0;
            foreach (var extent in extents)
            {
                int lsn = extent.Lsn;
                int sectorCnt = extent.Length;
                while (sectorCnt-- > 0)
                {
                    int length = Math.Min(SectorSize, data.Length - dataOffset);
                    WriteSector(lsn, data, dataOffset, length);
                    SetSectorAllocated(lsn++, true);
                    dataOffset += length;
                }
            }
        }


        /// <summary>
        /// Returns a concise string representation of this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("DragonDos Filesystem (Sides={0} Tracks={1} Sectors={2} Sector Size={3})",
                                 Disk.Heads, Disk.Tracks, SectorsPerHead, SectorSize);
        }
    }
}
