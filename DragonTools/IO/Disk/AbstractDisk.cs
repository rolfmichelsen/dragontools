/*
Copyright (c) 2011, Rolf Michelsen
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

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Abstract representation of a disk.  Subclasses provide support for actual disk representations.
    /// A disk is represented as a number of sectors.  Each sector is addressed by a unique combination of head, track and sector number.
    /// Head, track and sector numbers are all zero-based.
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
        /// The total number of sectors on the disk.
        /// </summary>
        public virtual int DiskSectors
        {
            get { return Heads*Tracks*Sectors; }
        }


        /// <summary>
        /// Returns the total capacity of the disk, in number of bytes.
        /// </summary>
        public virtual int Capacity
        {
            get { return Heads*Tracks*Sectors*SectorSize; }
        }


        /// <summary>
        /// Byte array holding the entire disk image, including any disk image header and meta-data.  Use the <see cref="SectorOffset">SectorOffset</see> method
        /// to compute the offset into this array of a specific sector.
        /// </summary>
        protected byte[] diskData = null;


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
            ValidateGeometryParameters(head, track, sector);
            var sectorOffset = SectorOffset(head, track, sector);
            Array.Copy(diskData, sectorOffset, data, offset, Math.Min(SectorSize, length));            
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
            ValidateGeometryParameters(head, track, sector);
            var sectorOffset = SectorOffset(head, track, sector);
            var copySize = Math.Min(SectorSize, length);
            Array.Copy(data, offset, diskData, sectorOffset, copySize);
            IsModified = true;
            OnSectorWritten(new SectorWrittenEventArgs(head, track, sector));
        }


        /// <summary>
        /// Flushes any pending write operations to the backing store.  It is always safe to call this method on disks even if they do not
        /// support write operations.
        /// </summary>
        public abstract void Flush();


        /// <summary>
        /// Returns the offset into the <see cref="diskData">diskData</see> array of the first byte of the identified sector.
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
        /// Validates the disk geometry parameters against the actual disk geometry and throws an <see cref="ArgumentOutOfRangeException">ArgumentOutOfRangeException</see> if they are out of range.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if any of the geometry parameters is out of range.</exception>
        public virtual void ValidateGeometryParameters(int head, int track, int sector)
        {
            if (head < 0 || head >= Heads) throw new ArgumentOutOfRangeException("head", head, String.Format("Side must be from 0 to {0}", Heads - 1));
            if (track < 0 || track >= Tracks) throw new ArgumentOutOfRangeException("track", track, String.Format("Track must be in the range 0 to {0}", Tracks - 1));
            if (sector < 0 || sector >= Sectors) throw new ArgumentOutOfRangeException("sector", sector, String.Format("Sector must be in the range 0 to {0}", Sectors - 1));
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
            diskData = null;
            IsDisposed = true;
        }

    }


}
