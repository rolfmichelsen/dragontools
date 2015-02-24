/*
Copyright (c) 2011-2015, Rolf Michelsen
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
    public sealed class DmkTrack : IEnumerable<DmkSector>, IDisposable
    {
        public static readonly byte IdAddressMark = 0xfe;
        public static readonly byte DataAddressMark = 0xfb;
        public static readonly byte SyncMark = 0xa1;

        private Stream diskStream ;
        private int trackOffset;
        private int trackLength;

        private IList<int> sectorOffset;


        /// <summary>
        /// Create a track object.
        /// </summary>
        /// <param name="diskStream">Stream for reading disk image data.</param>
        /// <param name="trackOffset">Byte offset of the track representation in the disk image.  This is the beginning of the DMK track header.</param>
        /// <param name="trackLength">Number of bytes in the track representation in the disk image.</param>
        /// <param name="headId">Head ID.</param>
        /// <exception cref="DiskImageFormatException">The disk track cannot be correctly decoded.</exception>
        internal DmkTrack(Stream diskStream, int trackOffset, int trackLength, int headId)
        {
            if (diskStream == null) throw new ArgumentNullException("diskStream");
            if (!diskStream.CanRead) throw new NotSupportedException("Stream does not support read operations");
            if (!diskStream.CanSeek) throw new NotSupportedException("Stream does not support seek operations");

            this.diskStream = diskStream;
            this.trackOffset = trackOffset;
            this.trackLength = trackLength;

            GetSectorOffsets();
        }



        /// <summary>
        /// Read the track header containing the byte offset of each sector in the track.
        /// </summary>
        private void GetSectorOffsets()
        {
            diskStream.Seek(trackOffset, SeekOrigin.Begin);
            var header = new byte[80];
            IOUtils.ReadBlock(diskStream, header, 0, 80);
            
            sectorOffset = new List<int>();
            for (var i = 0; i < 80; i += 2)
            {
                var offset = header[i] | (header[i + 1] << 8);
                if (offset == 0) break;
                sectorOffset.Add(offset & 0x7fff);
            }
        }


        /// <summary>
        /// Reads the specified sector ID block.  The stream will be positioned just after the sector ID block.
        /// </summary>
        /// <returns>Returns sector meta-information.</returns>
        private DmkSectorInfo GetSectorInfo(int sectorIndex)
        {
            var offset = sectorOffset[sectorIndex];
            diskStream.Seek(trackOffset + offset, SeekOrigin.Begin);
            var blockId = diskStream.ReadByte();
            var trackId = diskStream.ReadByte();
            var headId = diskStream.ReadByte();
            var sectorId = diskStream.ReadByte();
            var sectorSizeCode = diskStream.ReadByte();
            var crc1 = diskStream.ReadByte();
            var crc2 = diskStream.ReadByte();
            if (blockId != IdAddressMark) throw new DiskImageFormatException("Cannot find sector address mark at expected position");
            if (crc2 == -1) throw new EndOfStreamException("Reading beyond end of stream while reading sector ID block");
            return new DmkSectorInfo(headId, trackId, sectorId, 128 << sectorSizeCode);
        }


        /// <summary>
        /// Returns meta-information for the given sector.  The stream will be positioned just after the sector ID block.
        /// </summary>
        /// <param name="headId">Sector head.</param>
        /// <param name="trackId">Sector track.</param>
        /// <param name="sectorId">Sector number.</param>
        /// <returns>Returns sector meta-information, or <value>null</value> if the sector was not found.</returns>
        private DmkSectorInfo GetSectorInfo(int headId, int trackId, int sectorId)
        {
            for (var i = 0; i < sectorOffset.Count; i++)
            {
                var sector = GetSectorInfo(i);
                if (sector.Head == headId && sector.Track == trackId && sector.Sector == sectorId)
                    return sector;
            }
            return null;
        }


        /// <summary>
        /// Skips GAP2 between the sector ID record and the sector data record.  The DMK format omits the A1 sync bytes at the end of GAP2 so
        /// we are done when 12 x 0x00 have been read.
        /// </summary>
        /// <returns>The number of bytes read.  Returns -1 if the end of the stream was reached
        /// without finding a sync sequence.</returns>
        private int Sync()
        {
            var syncCount = 0;
            var readCount = 0;
            while (syncCount < 12)
            {
                var data = diskStream.ReadByte();
                if (data == -1) return -1;
                readCount++;
                syncCount = (data == 0) ? syncCount + 1 : 0;
            }
            return readCount;
        }


        /// <summary>
        /// Reads sector data and returns a sector object.  The stream must be positioned at the first byte of the sector payload
        /// data.  On exit the stream will be positioned on the first byte following the sector payload CRC.
        /// </summary>
        /// <param name="info">Sector meta-information.</param>
        /// <returns>A sector object.</returns>
        private DmkSector GetSectorData(DmkSectorInfo info)
        {
            Sync();
            var blockId = diskStream.ReadByte();
            if (blockId == -1) return null;
            if (blockId != DataAddressMark) throw new DiskImageFormatException(String.Format("Unexpected block type {0}", blockId));

            var data = new byte[info.Size];
            IOUtils.ReadBlock(diskStream, data, 0, info.Size);
            var crc1 = diskStream.ReadByte();
            var crc2 = diskStream.ReadByte();
            if (crc2 == -1) throw new EndOfStreamException();
            var crc = (crc1 << 8) | crc2;

            // TODO Verify disk sector CRC

            return new DmkSector(info.Head, info.Track, info.Sector, data, 0, data.Length);
        }


        /// <summary>
        /// Check whether a sector exists in this track.
        /// </summary>
        /// <param name="headId">Head number.</param>
        /// <param name="trackId">Track number.</param>
        /// <param name="sectorId">Sector number.</param>
        /// <returns>True if the sector exists.</returns>
        public bool SectorExists(int headId, int trackId, int sectorId)
        {
            return GetSectorInfo(headId, trackId, sectorId) != null;
        }

        
        /// <summary>
        /// Retrieve a sector from this track.
        /// </summary>
        /// <param name="headId">Head number.</param>
        /// <param name="trackId">Track number.</param>
        /// <param name="sectorId">Sector number.</param>
        /// <returns>Sector object.</returns>
        /// <exception cref="SectorNotFoundException">Thrown if the sector cannot be found on this track.</exception>
        public DmkSector ReadSector(int headId, int trackId, int sectorId)
        {
            var sectorInfo = GetSectorInfo(headId, trackId, sectorId);
            if (sectorInfo == null) throw new SectorNotFoundException(headId, trackId, sectorId);
            return GetSectorData(sectorInfo);
        }


        /// <summary>
        /// Write a sector to this track.
        /// </summary>
        /// <param name="headId">Head number.</param>
        /// <param name="trackId">Track number.</param>
        /// <param name="sectorId">Sector number.</param>
        /// <param name="data">Sector payload data.</param>
        /// <param name="dataOffset">Offset into sector payload data of first byte to write.</param>
        /// <param name="dataLength">Size of sector payload data.</param>
        /// <exception cref="SectorNotFoundException">Thrown if the sector cannot be found on this track.</exception>
        public void WriteSector(int headId, int trackId, int sectorId, byte[] data, int dataOffset, int dataLength)
        {
            var sectorInfo = GetSectorInfo(headId, trackId, sectorId);
            if (sectorInfo == null) throw new SectorNotFoundException(headId, trackId, sectorId);

            /* Ensure that sector has correct size. */
            var data2 = new byte[sectorInfo.Size];
            Array.Copy(data, dataOffset, data2, 0, Math.Min(sectorInfo.Size, dataLength));

            WriteSectorData(data2, 0, data2.Length);
        }



        /// <summary>
        /// Write sector data, including the CRC, to the stream.  The stream must be positioned at the first byte of the sector
        /// payload data.
        /// </summary>
        /// <param name="data">Sector data buffer.</param>
        /// <param name="offset">Offset of the first byte of sector data into the data buffer.</param>
        /// <param name="length">Size of the sector data.</param>
        private void WriteSectorData(byte[] data, int offset, int length)
        {
            Sync();
            var blockId = diskStream.ReadByte();
            if (blockId == -1) throw new DiskImageFormatException("Sector data not found after sector address mark");
            if (blockId != DataAddressMark) throw new DiskImageFormatException(String.Format("Unexpected block type {0}", blockId));

            var crc = new Crc16Ccitt();
            crc.Add(0xa1);
            crc.Add(0xa1);
            crc.Add(0xa1);
            crc.Add(DataAddressMark);
            for (int i = 0; i < length; i++)
                crc.Add(data[offset + i]);

            diskStream.Write(data, offset, length);
            diskStream.WriteByte((byte)((crc.Crc >> 8) & 0xff));
            diskStream.WriteByte((byte)(crc.Crc & 0xff));
        }


        public IEnumerator<DmkSector> GetEnumerator()
        {
            /* First extract all track sectors and store them in a collection. */
            var sectors = new List<DmkSector>();
            for (var i = 0; i < sectorOffset.Count; i++)
            {
                sectors.Add(GetSectorData(GetSectorInfo(i)));
            }
            
            /* Now iterate over the sectors. */
            foreach (var sector in sectors)
            {
                yield return sector;
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Dispose()
        {        
        }
    }
}
