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

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Abstract representation of a disk.  Subclasses provide support for actual disk representations.
    /// A disk is represented as a number of sectors.  Each sector is addressed by a unique combination of head, track and sector number.
    /// Head and track are zero-based.  Sector numbering starts at 1.
    /// </summary>
    /// <see cref="JvcDisk"/>
    /// <see cref="VdkDisk"/>
    /// <see cref="MemoryDisk"/>
    public abstract class AbstractDisk : IDisk
    {
        protected bool IsDisposed = false;
        protected bool IsModified = false;

        /// <summary>
        /// <value>true</value> if this disk supports write operations.  All write operations will fail with a <code>DiskNotWriteableException</code> 
        /// if this property is <value>false</value>.
        /// </summary>
        public virtual bool IsWriteable { get; protected set; }

        /// <summary>
        /// The number of disk heads.
        /// </summary>
        public virtual int Heads { get; protected set; }

        /// <summary>
        /// The number of disk tracks per head.
        /// </summary>
        public virtual int Tracks { get; protected set; }

        /// <summary>
        /// The number of disk sectors per head and track.
        /// </summary>
        public virtual int Sectors { get; protected set; }


        /// <summary>
        /// The size in bytes of a disk sector.
        /// </summary>
        public virtual int SectorSize { get; protected set; }


        /// <summary>
        /// Byte array holding the entire disk image, including any disk image header and meta-data.  Use the <see cref="SectorOffset">SectorOffset</see> method
        /// to compute the offset into this array of a specific sector.
        /// </summary>
        protected byte[] DiskData = null;


        /// <summary>
        /// Read a sector from disk and return its data as an array of bytes.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>The sector data as an array of bytes.  The size of the array will always be SectorSize.</returns>
        public virtual byte[] ReadSector(int head, int track, int sector)
        {
            var data = new byte[SectorSize];
            ReadSector(head, track, sector, data, 0, SectorSize);
            return data;
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
        public virtual void ReadSector(int head, int track, int sector, byte[] data, int offset, int length)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (data == null) throw new ArgumentNullException("data");
            if (!SectorExists(head, track, sector)) throw new SectorNotFoundException(head, track, sector);
            var sectorOffset = SectorOffset(head, track, sector);
            Array.Copy(DiskData, sectorOffset, data, offset, Math.Min(SectorSize, length));            
            OnSectorRead(new SectorReadEventArgs(head, track, sector));
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
        public virtual void WriteSector(int head, int track, int sector, byte[] data)
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
        public virtual void WriteSector(int head, int track, int sector, byte[] data, int offset, int length)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (!IsWriteable) throw new DiskNotWriteableException();
            if (!SectorExists(head, track, sector)) throw new SectorNotFoundException(head, track, sector);
            var sectorOffset = SectorOffset(head, track, sector);
            var copySize = Math.Min(SectorSize, length);
            Array.Copy(data, offset, DiskData, sectorOffset, copySize);
            IsModified = true;
            OnSectorWritten(new SectorWrittenEventArgs(head, track, sector));
        }


        /// <summary>
        /// Flushes any pending write operations to the backing store.  It is always safe to call this method on disks even if they do not
        /// support write operations.
        /// </summary>
        public abstract void Flush();


        /// <summary>
        /// Returns the offset into the <see cref="DiskData">diskData</see> array of the first byte of the identified sector.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>The byte offset of the first byte of this sector.</returns>
        protected abstract int SectorOffset(int head, int track, int sector);


        /// <summary>
        /// Signals that a sector has been written and invokes any registered event handles for the SectorWritten event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSectorWritten(SectorWrittenEventArgs e)
        {
            if (SectorWritten != null)
                SectorWritten(this, e);
        }


        /// <summary>
        /// Signals that a sector has been read and invokes any registered event handles for the SectorRead event.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSectorRead(SectorReadEventArgs e)
        {
            if (SectorRead != null)
                SectorRead(this, e);
        }


        /// <summary>
        /// Returns true if the specified sector exists on the disk.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>True if the sector exists, otherwise false.</returns>
        public virtual bool SectorExists(int head, int track, int sector)
        {
            if (head < 0 || head >= Heads) return false;
            if (track < 0 || track >= Tracks) return false;
            if (sector < 1 || sector > Sectors) return false;
            return true;
        }


        public event EventHandler<SectorWrittenEventArgs> SectorWritten;
        public event EventHandler<SectorReadEventArgs> SectorRead;


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            if (IsDisposed) return;
            Flush();
            DiskData = null;
            IsDisposed = true;
        }


        /// <summary>
        /// Return an enumerator of disk sectors.
        /// Note that accessing the sectors in this manner will not invoke the SectorRead event
        /// handler.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ISector> GetEnumerator()
        {
            for (int h = 0; h < Heads; h++)
            {
                for (int t = 0; t < Tracks; t++)
                {
                    for (int s = 1; s <= Sectors; s++)
                    {
                        var sectorData = new byte[SectorSize];
                        Array.Copy(DiskData, SectorOffset(h, t, s), sectorData, 0, SectorSize);
                        yield return new BasicSector(h, t, s, sectorData);
                    }
                }
            }
        }


        /// <summary>
        /// Return an enumerator of disk sectors.
        /// Note that accessing the sectors in this manner will not invoke the SectorRead event
        /// handler.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }


}
