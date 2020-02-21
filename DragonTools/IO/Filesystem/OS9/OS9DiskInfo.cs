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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    /// <summary>
    /// Contains key parameters for a OS9 disk.  This information corresponds to the information in the
    /// identification sector of RBFMAN filesystems.
    /// </summary>
    public sealed class OS9DiskInfo
    {
        private byte[] IdSector;

        public OS9DiskInfo(byte[] idsector)
        {
            if (idsector == null) throw new ArgumentNullException();
            IdSector = idsector;
        }


        /// <summary>
        /// The total number of sectors on the media.
        /// </summary>
        public int TotalSectors { get { return (IdSector[0] << 16) | (IdSector[1] << 8) | IdSector[2]; } }

        /// <summary>
        /// Number of sectors per track.
        /// </summary>
        public int Sectors { get { return IdSector[3]; } }

        /// <summary>
        /// Number of bytes in the disk allocation map.
        /// </summary>
        public int AllocationMapSize { get { return (IdSector[4] << 8) | IdSector[5]; } }

        /// <summary>
        /// Number of sectors per cluster.
        /// </summary>
        public int ClusterSize { get { return (IdSector[6] << 8) | IdSector[7]; } }

        /// <summary>
        /// The starting sector of the filesystem root directory.
        /// </summary>
        public int RootDirectory { get { return (IdSector[8] << 16) | (IdSector[9] << 8) | IdSector[10]; } }

        /// <summary>
        /// Volume name.
        /// </summary>
        public string VolumeName { get { return OS9Utils.ParseString(IdSector, 0x1f); } }

        /// <summary>
        /// The timestamp for when the filesystem was created.
        /// </summary>
        public Timestamp CreateTime { get { return new Timestamp(IdSector[26], IdSector[27], IdSector[28], IdSector[29], IdSector[30], 0); } }


        public override string ToString()
        {
            return
                String.Format(
                    "OS9DiskInfo: Volume name={0} Create Time={1} Total sectors={2} Cluster size={3} Root directory LSN={4}",
                    VolumeName, CreateTime, TotalSectors, ClusterSize, RootDirectory);
        }
    }
}
