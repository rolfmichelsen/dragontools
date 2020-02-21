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
using System.IO;


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Virtual disk in the DMK format.
    /// </summary>
    public sealed class DmkDisk : IDisk
    {
        /// <summary>
        /// Set when the object is disposed.
        /// </summary>
        private bool isDisposed = false;


        /// <summary>
        /// Stream used for reading and writing the HFE disk image.  This stream is owned by this object and will be properly
        /// disposed of by this object.
        /// </summary>
        private Stream diskImageStream;

        
        /// <summary>
        /// <value>true</value> if this disk supports write operations.  All write operations will fail with a <code>DiskNotWriteableException</code> 
        /// if this property is <value>false</value>.
        /// </summary>
        public bool IsWriteable { get; private set; }


        /// <summary>
        /// The number of disk heads.
        /// </summary>
        public int Heads { get; private set; }


        /// <summary>
        /// The number of disk tracks per head.
        /// </summary>
        public int Tracks { get; private set; }


        private int TrackLength;


        private DmkDisk() { }



        /// <summary>
        /// Return an enumerator of disk sectors.
        /// Note that accessing the sectors in this manner will not invoke the SectorRead event
        /// handler.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ISector> GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);

            for (var t = 0; t < Tracks; t++)
            {
                for (var h = 0; h < Heads; h++)
                {
                    var track = GetTrack(t, h);
                    foreach (var sector in track)
                    {
                        yield return sector;
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
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (head < 0 || head >= Heads) throw new SectorNotFoundException(head, track, sector);
            if (track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);
            var trackdata = GetTrack(track, head);
            var sectordata = trackdata.ReadSector(head, track, sector);
            OnSectorRead(new SectorReadEventArgs(head, track, sector));
            return sectordata.Data;
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
            var sectordata = ReadSector(head, track, sector);
            Array.Copy(sectordata, 0, data, offset, Math.Min(sectordata.Length, length));
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
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (head < 0 || head >= Heads) throw new SectorNotFoundException(head, track, sector);
            if (track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);
            using (var trackdata = GetTrack(track, head))
            {
                trackdata.WriteSector(head, track, sector, data, offset, length);
            }
            OnSectorWritten(new SectorWrittenEventArgs(head, track, sector));
        }


        /// <summary>
        /// Flushes any pending write operations to the backing store.  It is always safe to call this method on disks even if they do not
        /// support write operations.
        /// </summary>
        public void Flush()
        {
        }


        /// <summary>
        /// Returns true if the specified sector exists on the disk.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>True if the sector exists, otherwise false.</returns>
        public bool SectorExists(int head, int track, int sector)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (head < 0 || head >= Heads) throw new SectorNotFoundException(head, track, sector);
            if (track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);
            var trackdata = GetTrack(track, head);
            return trackdata.SectorExists(head, track, sector);
        }


        /// <summary>
        /// Return an disk track.
        /// Applications will normally not use this method but rather ReadSector or GetEnumerator to access disk data.
        /// </summary>
        /// <param name="track">Track number.</param>
        /// <param name="head">Head number.</param>
        /// <returns>A disk track.</returns>
        /// <seealso cref="ReadSector">ReadSector</seealso>
        /// <seealso cref="GetEnumerator">GetEnumerator</seealso>
        /// <exception cref="SectorNotFoundException">The head and track number is out of range.</exception>
        public DmkTrack GetTrack(int track, int head)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (track < 0 || track >= Tracks) throw new ArgumentOutOfRangeException("track", track, String.Format("Disk contanins {0} tracks", Tracks));
            if (head < 0 || head >= Heads) throw new ArgumentOutOfRangeException("head", head, String.Format("Disk contains {0} sides", Heads));            
            return new DmkTrack(diskImageStream, TrackOffset(track, head), TrackLength, head);
        }


        private int TrackOffset(int track, int head)
        {
            return 16 + TrackLength*(track*Heads + head);
        }


        /// <summary>
        /// Reads a DMK disk image from a given stream.
        /// </summary>
        /// <param name="image">Stream containing the HFE disk image.</param>
        /// <param name="isWriteable">Allow write operations to this disk.</param>
        /// <returns>A disk object.</returns>
        public static DmkDisk Open(Stream image, bool isWriteable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (!image.CanRead) throw new NotSupportedException("Disk image stream does not support reading");
            if (!image.CanSeek) throw new NotSupportedException("Disk image stream does not support seeking");
            if (isWriteable && !image.CanWrite)
                throw new NotSupportedException("Disk image stream does not support writing");

            var header = new DmkDiskHeader(image);
            if (header.IsSingleDensity) throw new NotSupportedException("Single density disks not supported");

            var disk = new DmkDisk();
            disk.diskImageStream = image;
            disk.IsWriteable = isWriteable && header.IsWriteable;
            disk.Heads = header.Heads;
            disk.Tracks = header.Tracks;
            disk.TrackLength = header.EncodedTrackLength;

            return disk;
        }


        /// <summary>
        /// Create a new DMK disk associated with the given stream.
        /// </summary>
        /// <param name="image">Stream for storing the disk image.</param>
        /// <param name="heads">Number of disk heads.</param>
        /// <param name="tracks">Number of tracks per head.</param>
        /// <param name="sectors">Number of sectors per track.</param>
        /// <param name="sectorsize">Sector size in bytes</param>
        /// <returns>A disk object</returns>
        public static DmkDisk Create(Stream image, int heads, int tracks, int sectors, int sectorsize)
        {
            throw new NotImplementedException();
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
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return String.Format("DMK disk: {0} heads, {1} tracks, {2}", Heads, Tracks,
                (IsWriteable ? "read/write" : "read only"));
        }


        private void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                Flush();
                diskImageStream.Close();
                isDisposed = true;
                diskImageStream = null;
            }
            isDisposed = true;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        ~DmkDisk()
        {
            Dispose(false);
        }


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

    }
}
