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
