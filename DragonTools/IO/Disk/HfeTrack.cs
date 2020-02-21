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
    /// Represents one side of a track on a virtual HFE format disk.
    /// This class can handle tracks written by the WD279X floppy disk controller and is
    /// further limited to only handle MFM encoded tracks.  Applications will normally not
    /// interact directly with this class.  Instances of this class are associated with
    /// the corresponding <see cref="HfeDisk"/> instance.
    /// </summary>
    public sealed class HfeTrack : IEnumerable<HfeSector>, IDisposable
    {
        public static readonly byte IdAddressMark = 0xfe;
        public static readonly byte DataAddressMark = 0xfb;
        public static readonly byte SyncMark = 0xa1;

        /// <summary>
        /// Number of bytes of 0x4e to write before the first GAP in a track.
        /// </summary>
        private static readonly int PreableLength = 8;

        /// <summary>
        /// Number of bytes of 0x4e to write after the last sector in a track.
        /// </summary>
        private static readonly int PostambleLength = 108;


        /// <summary>
        /// The number of bytes required to represent a sector in addition to the sector payload itself.
        /// This overhead includes the gaps preceeding the sector ID record and the sector data record, the length
        /// of the sector ID record and the sector CRC.
        /// </summary>
        private static readonly int SectorOverheadLength = 85;


        /// <summary>
        /// Stream for accessing the track data.  The underlying stream accesses the disk image
        /// file and is not owned by this object and will not be closed when the object is disposed.
        /// </summary>
        private MfmStream trackStream;

        private bool isDisposed = false;


        /// <summary>
        /// Byte offset of start of encoded track data in the virtual disk image.
        /// </summary>
        public int TrackOffset { get; private set; }


        /// <summary>
        /// Length in bytes of the encoded track data in the virtual disk image.
        /// </summary>
        public int TrackLength { get; private set; }


        /// <summary>
        /// Create a track object.
        /// </summary>
        /// <param name="diskStream">Stream for reading disk image data.</param>
        /// <param name="trackOffset">Byte offset of the track representation in the disk image.</param>
        /// <param name="trackLength">Number of bytes in the track representation in the disk image.</param>
        /// <param name="headId">Head ID.</param>
        /// <exception cref="DiskImageFormatException">The disk track cannot be correctly decoded.</exception>
        internal HfeTrack(Stream diskStream, int trackOffset, int trackLength, int headId)
        {
            if (diskStream == null) throw new ArgumentNullException("diskStream");
            if (!diskStream.CanRead) throw new NotSupportedException("Stream does not support read operations");
            if (!diskStream.CanSeek) throw new NotSupportedException("Stream does not support seek operations");
            trackStream = new MfmStream(new HfeRawTrack(diskStream, trackOffset, trackLength, headId));
            TrackOffset = trackOffset;
            TrackLength = trackLength;
        }


        /// <summary>
        /// Retrieve a sector from this track.
        /// </summary>
        /// <param name="headId">Head number.</param>
        /// <param name="trackId">Track number.</param>
        /// <param name="sectorId">Sector number.</param>
        /// <returns>Sector object.</returns>
        /// <exception cref="SectorNotFoundException">Thrown if the sector cannot be found on this track.</exception>
        public HfeSector ReadSector(int headId, int trackId, int sectorId)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            trackStream.Seek(0, SeekOrigin.Begin);
            var sectorInfo = GetSectorInfo(headId, trackId, sectorId);
            if (sectorInfo == null) throw new SectorNotFoundException(headId, trackId, sectorId);
            return GetSectorData(sectorInfo);
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
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            trackStream.Seek(0, SeekOrigin.Begin);
            return (GetSectorInfo(headId, trackId, sectorId) != null);
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
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (data == null) throw new ArgumentNullException("data");
            if (!trackStream.CanWrite) throw new NotSupportedException("Stream does not support write operations");

            trackStream.Seek(0, SeekOrigin.Begin);
            var sectorInfo = GetSectorInfo(headId, trackId, sectorId);
            if (sectorInfo == null) throw new SectorNotFoundException(headId, trackId, sectorId);

            /* Ensure that sector has correct size. */
            var data2 = new byte[sectorInfo.Size];
            Array.Copy(data, dataOffset, data2, 0, Math.Min(sectorInfo.Size, dataLength));

            WriteSectorData(data2, 0, data2.Length);
        }


        /// <summary>
        /// Returns an enumerator that iterates through the sectors in this track.
        /// </summary>
        public IEnumerator<HfeSector> GetEnumerator()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);

            /* First extract all track sectors and store them in a collection. */
            var sectors = new List<HfeSector>();
            trackStream.Seek(0, SeekOrigin.Begin);
            HfeSectorInfo hfeSectorInfo;
            while ((hfeSectorInfo = GetSectorInfo()) != null)
            {
                sectors.Add(GetSectorData(hfeSectorInfo));
            }
            
            /* Now iterate over the sectors. */
            foreach (var sector in sectors)
            {
                yield return sector;
            }
        }


        /// <summary>
        /// Returns an enumerator that iterates through the sectors in this track.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                trackStream.Flush();
                trackStream = null;                            
            }

            isDisposed = true;
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        ~HfeTrack()
        {
            Dispose(false);
        }


        /// <summary>
        /// Reads from the current stream position and until a sync sequence have been fully read.  The stream
        /// will be positioned on the first byte following the sync sequence.
        /// </summary>
        /// <returns>The number of bytes read, including the sync sequence.  Returns -1 if the end of the stream was reached
        /// without finding a sync sequence.</returns>
        private int Sync()
        {
            int syncCount = 0;
            int readCount = 0;
            while (syncCount < 3)
            {
                bool isSync;
                if (trackStream.ReadByte(out isSync) == -1) return -1;
                readCount++;
                syncCount = isSync ? syncCount + 1 : 0;
            }
            return readCount;
        }


        /// <summary>
        /// Reads from the current stream position and returns meta-information for the first sector encountered.  The stream
        /// will be positioned just after the sector ID block.
        /// </summary>
        /// <returns>Returns sector meta-information, or <value>null</value> if no sector was found.</returns>
        private HfeSectorInfo GetSectorInfo()
        {
            int blockId;
            do
            {
                Sync();
                blockId = trackStream.ReadByte();
                if (blockId == -1) return null;
            } while (blockId != IdAddressMark);
            
            int track = trackStream.ReadByte();
            int head = trackStream.ReadByte();
            int sector = trackStream.ReadByte();
            int sectorSizeCode = trackStream.ReadByte();
            int crc1 = trackStream.ReadByte();
            int crc2 = trackStream.ReadByte();
            if (crc2 == -1) throw new EndOfStreamException("Reading beyond end of stream while reading sector ID block");

            return new HfeSectorInfo(head, track, sector, 128 << sectorSizeCode);
        }


        /// <summary>
        /// Reads from the current stream position and returns meta-information for the given sector.  The stream will be 
        /// positioned just after the sector ID block.
        /// </summary>
        /// <param name="headId">Sector head.</param>
        /// <param name="trackId">Sector track.</param>
        /// <param name="sectorId">Sector number.</param>
        /// <returns>Returns sector meta-information, or <value>null</value> if the sector was not found.</returns>
        private HfeSectorInfo GetSectorInfo(int headId, int trackId, int sectorId)
        {
            HfeSectorInfo si;
            do
            {
                si = GetSectorInfo();
                if (si == null) return null;
            } while (si.Head != headId || si.Track != trackId || si.Sector != sectorId);
            return si;
        }


        /// <summary>
        /// Reads sector data and returns a sector object.  The stream must be positioned at the first byte of the sector payload
        /// data.  On exit the stream will be positioned on the first byte following the sector payload CRC.
        /// </summary>
        /// <param name="info">Sector meta-information.</param>
        /// <returns>A sector object.</returns>
        private HfeSector GetSectorData(HfeSectorInfo info)
        {
            Sync();
            var blockId = trackStream.ReadByte();
            if (blockId == -1) return null;
            if (blockId != DataAddressMark) throw new DiskImageFormatException(String.Format("Unexpected block type {0}", blockId));

            var data = new byte[info.Size];
            IOUtils.ReadBlock(trackStream, data, 0, info.Size);
            var crc1 = trackStream.ReadByte();
            var crc2 = trackStream.ReadByte();
            if (crc2 == -1) throw new EndOfStreamException();
            var crc = (crc1 << 8) | crc2;

            // TODO Verify disk sector CRC

            return new HfeSector(info.Head, info.Track, info.Sector, data, 0, data.Length);
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
            var blockId = trackStream.ReadByte();
            if (blockId == -1) throw new DiskImageFormatException("Sector data not found after sector address mark");
            if (blockId != DataAddressMark) throw new DiskImageFormatException(String.Format("Unexpected block type {0}", blockId));

            var crc = new Crc16Ccitt();
            crc.Add(0xa1);
            crc.Add(0xa1);
            crc.Add(0xa1);
            crc.Add(DataAddressMark);
            for (int i = 0; i < length; i++)
                crc.Add(data[offset + i]);

            trackStream.Write(data, offset, length);
            trackStream.WriteByte((byte) ((crc.Crc >> 8) & 0xff));
            trackStream.WriteByte((byte) (crc.Crc & 0xff));
        }


        /// <summary>
        /// Initialize (format) a track by writing headers and sectors to it.  
        /// </summary>
        /// <param name="diskStream">Stream for writing the disk track.</param>
        /// <param name="trackOffset">Byte offset of the track data in the disk image.</param>
        /// <param name="headId">Side of disk for initializing the track.</param>
        /// <param name="sectors">Collection of sectors to be written to the track.</param>
        /// <returns>The track length in the disk image stream.</returns>
        static internal int InitializeTrack(Stream diskStream, int trackOffset, int headId, IEnumerable<HfeSector> sectors)
        {
            var rawStream = new HfeRawTrack(diskStream, trackOffset, headId);
            var trackStream = new MfmStream(rawStream);

            trackStream.WriteBytes(0x4e, PreableLength);

            foreach (var sector in sectors)
            {
                // Write gap preceeding sector ID record
                trackStream.WriteBytes(0x4e, 24);
                trackStream.WriteBytes(0x00, 12);
                trackStream.WriteSync();
                trackStream.WriteSync();
                trackStream.WriteSync();

                // Write sector ID record
                var sectorInfo = sector.GetSectorInfo();
                var encodedIdRecord = sectorInfo.Encode();
                trackStream.Write(encodedIdRecord, 0, encodedIdRecord.Length);

                // Write gap preceeding sector data record
                trackStream.WriteBytes(0x4e, 22);
                trackStream.WriteBytes(0x00, 12);
                trackStream.WriteSync();
                trackStream.WriteSync();
                trackStream.WriteSync();

                // Write sector data record
                var encodedDataRecord = sector.Encode();
                trackStream.Write(encodedDataRecord, 0, encodedDataRecord.Length);
            }

            trackStream.WriteBytes(0x4e, PostambleLength);
            trackStream.Flush();

            return rawStream.TrackLength - trackOffset;
        }

    }
}
