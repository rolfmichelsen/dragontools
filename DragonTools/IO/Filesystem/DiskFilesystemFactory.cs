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
