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

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Abstract representation of a virtual disk.  Subclasses provide support for actual disk representations.
    /// A disk is represented as a number of sectors.  Each sector is addressed by a unique combination of head, track and sector number.
    /// Head and track numbers are zero-based.
    /// </summary>
    /// <see cref="JvcDisk"/>
    /// <see cref="VdkDisk"/>
    /// <see cref="MemoryDisk"/>
    public interface IDisk : IDisposable, IEnumerable<ISector>
    {
        /// <summary>
        /// <value>true</value> if this disk supports write operations.  All write operations will fail with a <code>DiskNotWriteableException</code> 
        /// if this property is <value>false</value>.
        /// </summary>
        bool IsWriteable { get; }

        /// <summary>
        /// The number of disk heads.
        /// </summary>
        int Heads { get; }

        /// <summary>
        /// The number of disk tracks per head.
        /// </summary>
        int Tracks { get; }

        /// <summary>
        /// Read a sector from disk and return its data as an array of bytes.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>The sector data as an array of bytes.  The size of the array will always be SectorSize.</returns>
        /// <exception cref="SectorNotFoundException">The sector does not exist on the disk.</exception>
        byte[] ReadSector(int head, int track, int sector);

        /// <summary>
        /// Read a sector from disk and return its data in an existing byte array.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <param name="data">Array to receive the sector data.</param>
        /// <param name="offset">Offset into the data array of first byte written.</param>
        /// <param name="length">Maximum number of bytes to write to the data array.  This function will never write more than SectorSize bytes.</param>
        /// <exception cref="SectorNotFoundException">The sector does not exist on the disk.</exception>
        void ReadSector(int head, int track, int sector, byte[] data, int offset, int length);

        /// <summary>
        /// Write data to a given sector on disk.  If the amount of data to write is larger than the capacity of the sector, the data is truncated.
        /// If the data is less than the capacity of the sector, then the sector will not be completely overwritten.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <param name="data">Data to write to the disk sector.</param>
        /// <exception cref="DiskNotWriteableException">This disk does not support write operations.</exception>
        /// <exception cref="SectorNotFoundException">The sector does not exist on the disk.</exception>
        void WriteSector(int head, int track, int sector, byte[] data);

        /// <summary>
        /// Write data to a given sector on disk.  If the amount of data to write is larger than the capacity of the sector, the data is truncated.
        /// If the data is less than the capacity of the sector, then the sector will not be completely overwritten.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <param name="data">Data to write to disk sector.</param>
        /// <param name="offset">Offset into the data array of first byte to write to the disk sector.</param>
        /// <param name="length">The number of bytes from the data array to write to the disk sector.</param>
        /// <exception cref="DiskNotWriteableException">This disk does not support write operations.</exception>
        /// <exception cref="SectorNotFoundException">The sector does not exist on the disk.</exception>
        void WriteSector(int head, int track, int sector, byte[] data, int offset, int length);

        /// <summary>
        /// Flushes any pending write operations to the backing store.  It is always safe to call this method on disks even if they do not
        /// support write operations.
        /// </summary>
        void Flush();

        /// <summary>
        /// Returns true if the specified sector exists on the disk.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>True if the sector exists, otherwise false.</returns>
        bool SectorExists(int head, int track, int sector);

        /// <summary>
        /// This event is triggered after writing to a disk sector using the <c>WriteSector</c> function.  The sector may not have been written out to
        /// the backing store when the event is fired.
        /// </summary>
        event EventHandler<SectorWrittenEventArgs> SectorWritten;

        /// <summary>
        /// This event is triggered after reading a disk sector using the <c>ReadSector</c> function.
        /// </summary>
        event EventHandler<SectorReadEventArgs> SectorRead;
    }




    /// <summary>
    /// Parameters for the SectorWritten event.  Contains information about which sector was written.
    /// </summary>
    public sealed class SectorWrittenEventArgs : EventArgs
    {
        public readonly int Head;
        public readonly int Track;
        public readonly int Sector;

        public SectorWrittenEventArgs(int head, int track, int sector)
        {
            Head = head;
            Track = track;
            Sector = sector;
        }


        public override string ToString()
        {
            return String.Format("Write sector h={0} t={1} s={2}", Head, Track, Sector);
        }

    }


    /// <summary>
    /// Parameters for the SectorRead event.  Contains information about which sector was read.
    /// </summary>
    public sealed class SectorReadEventArgs : EventArgs
    {
        public readonly int Head;
        public readonly int Track;
        public readonly int Sector;

        public SectorReadEventArgs(int head, int track, int sector)
        {
            Head = head;
            Track = track;
            Sector = sector;
        }


        public override string ToString()
        {
            return String.Format("Read sector h={0} t={1} s={2}", Head, Track, Sector);
        }
    }

}
