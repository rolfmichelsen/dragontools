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
using System.IO;


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Virtual disk in the HFE format.
    /// </summary>
    /// <remarks>The HFE format is documented SD HxC Floppy Emulator HFE File Format, rev 1.1, 2012-06-20.
    /// http://hxc2001.com/download/floppy_drive_emulator/SDCard_HxC_Floppy_Emulator_HFE_file_format.pdf.
    /// This implementation only supports the GENERIC_SHUGART_DD floppy disk interface mode and the
    /// ISOIBM_MFM track encoding mode.</remarks>
    public sealed class HfeDisk : IDisk
    {
        /// <summary>
        /// HFE disk files are organized in blocks of this size.
        /// </summary>
        internal static readonly int BlockSize = 512;


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
        public int Heads
        {
            get { return DiskHeader.Sides; }
        }


        /// <summary>
        /// The number of disk tracks per head.
        /// </summary>
        public int Tracks
        {
            get { return DiskHeader.Tracks; }
        }


        /// <summary>
        /// The HFE disk file header.
        /// </summary>
        public HfeDiskHeader DiskHeader { get; private set; }


        /// <summary>
        /// Index of the first block containing track data.
        /// </summary>
        private int[] trackBlock = null;


        /// <summary>
        /// Size of the track (in bytes) in the disk image.
        /// </summary>
        private int[] trackLength = null;


        private HfeDisk()
        {
        }


        /// <summary>
        /// Reads a HFE disk image from a given stream.
        /// </summary>
        /// <param name="image">Stream containing the HFE disk image.</param>
        /// <param name="isWriteable">Allow write operations to this disk.</param>
        /// <returns>A disk object.</returns>
        public static HfeDisk Open(Stream image, bool isWriteable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (!image.CanRead) throw new NotSupportedException("Disk image stream does not support reading");
            if (!image.CanSeek) throw new NotSupportedException("Disk image stream does not support seeking");
            if (isWriteable && !image.CanWrite)
                throw new NotSupportedException("Disk image stream does not support writing");

            var diskHeader = new HfeDiskHeader(image);

            if (diskHeader.FloppyInterface != HfeDiskHeader.FloppyInterfaceMode.GENERIC_SHUGART_DD)
                throw new DiskImageFormatException(String.Format("Unsupported floppy interface mode {0}",
                    diskHeader.FloppyInterface));
            if (diskHeader.TrackEncoding != HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM)
                throw new DiskImageFormatException(String.Format("Unsupported track encoding mode {0}",
                    diskHeader.TrackEncoding));
            if (diskHeader.TrackEncoding0 != HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM)
                throw new DiskImageFormatException(
                    String.Format("Unsupported track encoding mode {0} for track 0 head 0", diskHeader.TrackEncoding0));
            if (diskHeader.TrackEncoding1 != HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM)
                throw new DiskImageFormatException(
                    String.Format("Unsupported track encoding mode {0} for track 0 head 1", diskHeader.TrackEncoding1));

            var disk = new HfeDisk {DiskHeader = diskHeader, IsWriteable = isWriteable, diskImageStream = image};
            disk.ReadTrackList();

            return disk;
        }


        /// <summary>
        /// Read the track list from the disk image.
        /// </summary>
        private void ReadTrackList()
        {
            trackBlock = new int[Tracks];
            trackLength = new int[Tracks];

            var trackListOffset = DiskHeader.TrackListBlock*BlockSize;
            var blockBuffer = new byte[BlockSize];
            diskImageStream.Seek(trackListOffset, SeekOrigin.Begin);
            IOUtils.ReadBlock(diskImageStream, blockBuffer, 0, blockBuffer.Length);

            for (var i = 0; i < Tracks; i++)
            {
                trackBlock[i] = blockBuffer[i*4] | (blockBuffer[i*4 + 1] << 8);
                trackLength[i] = blockBuffer[i*4 + 2] | (blockBuffer[i*4 + 3] << 8);
            }
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
            if (head <0 || head >= Heads) throw new SectorNotFoundException(head, track, sector);
            if (track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);
            var trackdata = new HfeTrack(diskImageStream, trackBlock[track]*BlockSize, trackLength[track], head);
            var sectordata = trackdata.GetSector(head, track, sector);
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
            throw new NotImplementedException();
            //TODO HfeDisk.WriteSector not implemented
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
            throw new NotImplementedException();
            //TODO HfeDisk.WriteSector not implemented
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
            if (head < 0 || head >= Heads) throw new SectorNotFoundException(head, track, sector);
            if (track < 0 || track >= Tracks) throw new SectorNotFoundException(head, track, sector);
            var trackdata = new HfeTrack(diskImageStream, trackBlock[track] * BlockSize, trackLength[track], head);
            return trackdata.SectorExists(head, track, sector);
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return String.Format("HFE disk: {0} heads, {1} tracks, {2}", Heads, Tracks,
                (IsWriteable ? "read/write" : "read only"));
        }


        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<ISector> GetEnumerator()
        {
            for (var h = 0; h < Heads; h++)
            {
                for (var t = 0; t < Tracks; t++)
                {
                    var track = new HfeTrack(diskImageStream, trackBlock[t]*BlockSize, trackLength[t], h);
                    foreach (var sector in track)
                    {
                        yield return sector;
                    }
                }
            }
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


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (isDisposed) return;
            Flush();
            diskImageStream.Close();
            isDisposed = true;
            diskImageStream = null;
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

    }
}
