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
using RolfMichelsen.Dragon.DragonTools.IO.Disk;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem
{
    /// <summary>
    /// This class provides a number of convenience methods for creating IDiskFilesystem objects.
    /// </summary>
    public sealed class DiskFilesystemFactory
    {
        /// <summary>
        /// Create an IDiskFilesystem object for accessing the filesystem on a disk in a virtual disk file.
        /// </summary>
        /// <param name="fsid">Identifies the filesystem type to create.</param>
        /// <param name="diskfilename">Name of file containing the virtual disk image.</param>
        /// <param name="iswriteable">Returns a writeable filesystem if <value>true</value>.</param>
        /// <returns>A IDiskFilesystem object for accessing the file system on the disk, or <value>null</value> if no filesystem can be created.</returns>
        public static IDiskFilesystem OpenFilesystem(DiskFilesystemIdentifier fsid, string diskfilename, bool iswriteable)
        {
            if (diskfilename == null) throw new ArgumentNullException("diskfilename");

            var disk = DiskFactory.OpenDisk(diskfilename, iswriteable);
            return (disk == null) ? null : OpenFilesystem(fsid, disk, iswriteable);
        }


        /// <summary>
        /// Create an IDiskFilesystem object for accessing the filesystem on a disk.
        /// </summary>
        /// <param name="fsid">Identifies the filesystem type to create.</param>
        /// <param name="disk">Disk containing the filesystem.</param>
        /// <param name="iswriteable">Returns a writeable filesystem if <value>true</value>.</param>
        /// <returns>A IDiskFilesystem object for accessing the file system on the disk, or <value>null</value> if no filesystem can be created.</returns>
        public static IDiskFilesystem OpenFilesystem(DiskFilesystemIdentifier fsid, IDisk disk, bool iswriteable)
        {
            if (disk == null) throw new ArgumentNullException("disk");
            if (iswriteable && !disk.IsWriteable) throw new ArgumentException("Cannot support write operations against a write-protected disk");

            switch (fsid)
            {
                case DiskFilesystemIdentifier.DragonDos:
                    return new DragonDos.DragonDos(disk, iswriteable);
                case DiskFilesystemIdentifier.RsDos:
                    return new RsDos.RsDos(disk, iswriteable);
                case DiskFilesystemIdentifier.Flex:
                    return new Flex.Flex(disk, iswriteable);
                case DiskFilesystemIdentifier.OS9:
                    return new OS9.OS9(disk, iswriteable);
                default:
                    return null;
            }
        }
    }




    public enum DiskFilesystemIdentifier
    {
        DragonDos,
        OS9,
        Flex,
        RsDos
    }
}
