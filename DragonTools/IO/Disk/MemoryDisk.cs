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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// This class provides a virtual disk where all disk data is stored in a memory buffer.
    /// </summary>
    public sealed class MemoryDisk : IDisk
    {
        /// <summary>
        /// <value>true</value> if this disk supports write operations.  All write operations will fail with a <code>DiskNotWriteableException</code> 
        /// if this property is <value>false</value>.
        /// </summary>
        public bool IsWriteable { get { return true; } }


        /// <summary>
        /// The number of disk heads.
        /// </summary>
        public int Heads { get; private set; }


        /// <summary>
        /// The number of disk tracks per head.
        /// </summary>
        public int Tracks { get; private set; }


        /// <summary>
        /// Collection of disk sectors.
        /// </summary>
        private Dictionary<SectorId, ISector> DiskSectors;


        /// <summary>
        /// Read a sector from disk and return its data as an array of bytes.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>The sector data as an array of bytes.  The size of the array will always be SectorSize.</returns>
        /// <exception cref="SectorNotFoundException">The sector does not exist on the disk.</exception>
        public byte[] ReadSector(int head, int track, int sector)
        {
            if (head < 0 || head >= Heads) throw new SectorNotFoundException(head, track, sector);
            if (track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);

            ISector s;
            if (!DiskSectors.TryGetValue(new SectorId(head, track, sector), out s))
                throw new SectorNotFoundException(head, track, sector);

            OnSectorRead(new SectorReadEventArgs(head, track, sector));
            return s.Data;
        }


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
        public void ReadSector(int head, int track, int sector, byte[] data, int offset, int length)
        {
            var sectorData = ReadSector(head, track, sector);
            Array.Copy(sectorData, 0, data, offset, length);
        }


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
        public void WriteSector(int head, int track, int sector, byte[] data)
        {
            WriteSector(head, track, sector, data, 0, data.Length);
        }


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
        public void WriteSector(int head, int track, int sector, byte[] data, int offset, int length)
        {
            if (head < 0 || head >= Heads || track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);

            ISector s;
            var sectorId = new SectorId(head, track, sector);
            if (!DiskSectors.TryGetValue(sectorId, out s))
                throw new SectorNotFoundException(head, track, sector);

            var sectordata = new byte[s.Data.Length];
            Array.Copy(data, offset, sectordata, 0, Math.Min(sectordata.Length, length));

            DiskSectors.Remove(sectorId);
            DiskSectors.Add(sectorId, new BasicSector(head, track, sector, sectordata));
            OnSectorWritten(new SectorWrittenEventArgs(head, track, sector));
        }


        /// <summary>
        /// Flushes any pending write operations to the backing store.
        /// </summary>
        public void Flush()
        {  }


        /// <summary>
        /// Returns true if the specified sector exists on the disk.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>True if the sector exists, otherwise false.</returns>
        public bool SectorExists(int head, int track, int sector)
        {
            if (head < 0 || head >= Heads || track < 0 || track >= Tracks) return false;
            return DiskSectors.ContainsKey(new SectorId(head, track, sector));
        }


        /// <summary>
        /// This event is triggered after writing to a disk sector using the <c>WriteSector</c> function.  The sector may not have been written out to
        /// the backing store when the event is fired.
        /// </summary>
        public event EventHandler<SectorWrittenEventArgs> SectorWritten;


        /// <summary>
        /// This event is triggered after reading a disk sector using the <c>ReadSector</c> function.
        /// </summary>
        public event EventHandler<SectorReadEventArgs> SectorRead;


        /// <summary>
        /// Signals that a sector has been written and invokes any registered event handles for the SectorWritten event.
        /// </summary>
        /// <param name="e"></param>
        private void OnSectorWritten(SectorWrittenEventArgs e)
        {
            if (SectorWritten != null)
                SectorWritten(this, e);
        }


        /// <summary>
        /// Signals that a sector has been read and invokes any registered event handles for the SectorRead event.
        /// </summary>
        /// <param name="e"></param>
        private void OnSectorRead(SectorReadEventArgs e)
        {
            if (SectorRead != null)
                SectorRead(this, e);
        }


        /// <summary>
        /// Constructs an empty memory-based virtual disk with the given geometry.
        /// </summary>
        /// <param name="heads">Number of disk heads.</param>
        /// <param name="tracks">Number of tracks per side.</param>
        /// <param name="sectors">Number of sectors per track.</param>
        /// <param name="sectorSize">The number of bytes in each sector.</param>
        public MemoryDisk(int heads, int tracks, int sectors, int sectorSize)
        {
            Heads = heads;
            Tracks = tracks;
            DiskSectors = new Dictionary<SectorId, ISector>();

            for (var h = 0; h < Heads; h++)
            {
                for (var t = 0; t < Tracks; t++)
                {
                    for (var s = 1; s <= sectors; s++)
                    {
                        DiskSectors.Add(new SectorId(h, t, s), new BasicSector(h, t, s, new byte[sectorSize]));
                    }
                }
            }
        }


        /// <summary>
        /// Constructs a memory-based virtual disk as a sector by sector clone of another disk.
        /// </summary>
        /// <param name="source">Disk to clone.</param>
        public MemoryDisk(IDisk source)
        {
            Heads = source.Heads;
            Tracks = source.Tracks;
            DiskSectors = new Dictionary<SectorId, ISector>();

            foreach (var sector in source)
            {
                DiskSectors.Add(new SectorId(sector.Head, sector.Track, sector.Sector), new BasicSector(sector.Head, sector.Track, sector.Sector, sector.Data));
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        { }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<ISector> GetEnumerator()
        {
            return ((IEnumerable<ISector>) DiskSectors.Values).GetEnumerator();
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
