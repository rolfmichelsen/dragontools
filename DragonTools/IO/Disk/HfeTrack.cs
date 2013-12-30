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
using System.Linq;
using System.IO;


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Represents a track on a virtual HFE format disk.
    /// This class can handle tracks written by the WD279X floppy disk controller and is
    /// further limited to only handle MFM encoded tracks.
    /// </summary>
    internal sealed class HfeTrack : IEnumerable<HfeSector>
    {
        private static readonly byte ID_ADDRESS_MARK = 0xfe;
        private static readonly byte DATA_ADDRESS_MARK = 0xfb;

        /// <summary>
        /// Collection containing all sectors in this track.
        /// </summary>
        private List<HfeSector> sectors = null;


        /// <summary>
        /// Create a track object.
        /// </summary>
        /// <param name="diskImageStream">Stream for accessing the HFE disk image.</param>
        /// <param name="trackOffset">Offset to start of track data in bytes.</param>
        /// <param name="trackLength">Length of encoded track data in bytes.</param>
        /// <exception cref="DiskImageFormatException">The disk track cannot be correctly decoded.</exception>
        public HfeTrack(Stream diskImageStream, int trackOffset, int trackLength, int head)
        {
            sectors = ReadTrack(diskImageStream, trackOffset, trackLength, head);
        }


        /// <summary>
        /// Retrieve a sector from this track.
        /// </summary>
        /// <param name="head">Head number.</param>
        /// <param name="track">Track number.</param>
        /// <param name="sector">Sector number.</param>
        /// <returns>Sector object.</returns>
        /// <exception cref="SectorNotFoundException">Thrown if the sector cannot be found on this track.</exception>
        public HfeSector GetSector(int head, int track, int sector)
        {
            foreach (var s in sectors)
            {
                if (s.Head == head && s.Track == track && s.Sector == sector)
                    return s;
            }
            throw new SectorNotFoundException(head, track, sector);
        }


        /// <summary>
        /// Check whether a sector exists in this track.
        /// </summary>
        /// <param name="head">Head number.</param>
        /// <param name="track">Track number.</param>
        /// <param name="sector">Sector number.</param>
        /// <returns>True if the sector exists.</returns>
        public bool SectorExists(int head, int track, int sector)
        {
            return sectors.Any(s => s.Head == head && s.Track == track && s.Sector == sector);
        }


        /// <summary>
        /// Read a track from the disk image, decode it and return its sectors.
        /// </summary>
        /// <param name="diskImageStream">Stream for accessing the HFE disk image.</param>
        /// <param name="trackOffset">Offset to start of track data in bytes.</param>
        /// <param name="trackLength">Length of encoded track data in bytes.</param>
        /// <param name="head">Disk head.</param>
        /// <returns>Track sectors.</returns>
        private static List<HfeSector> ReadTrack(Stream diskImageStream, int trackOffset, int trackLength, int head)
        {
            var rawTrackDataAllHeads = ReadRawTrack(diskImageStream, trackOffset, trackLength);
            var rawTrackData = ExtractTrackSide(rawTrackDataAllHeads, head);
            var trackData = DecodeMFM(rawTrackData);
            var sectors = ExtractSectors(trackData);
            return sectors;
        }


        private static byte[] ReadRawTrack(Stream diskImageStream, int trackOffset, int trackLength)
        {
            var trackData = new byte[trackLength];
            diskImageStream.Seek(trackOffset, SeekOrigin.Begin);
            IOUtils.ReadBlock(diskImageStream, trackData, 0, trackLength);
            return trackData;
        }


        private static byte[] ExtractTrackSide(byte[] rawTrackDataAllHeads, int head)
        {
            // No attempts at cleverness here.  This will have to be redesigned to support writing sectors anyway...
            var rawTrackData = new byte[rawTrackDataAllHeads.Length];
            var destOffset = 0;
            var srcOffset = head*HfeDisk.BlockSize/2;
            while (srcOffset < rawTrackDataAllHeads.Length)
            {
                var blockSize = Math.Min(HfeDisk.BlockSize/2, rawTrackDataAllHeads.Length - srcOffset);
                Array.Copy(rawTrackDataAllHeads, srcOffset, rawTrackData, destOffset, blockSize);
                destOffset += blockSize;
                srcOffset += HfeDisk.BlockSize;
            }
            Array.Resize(ref rawTrackData, destOffset);
            return rawTrackData;
        }


        private static byte[] DecodeMFM(byte[] encodedData)
        {
            var decodedData = new byte[encodedData.Length / 2];
            for (int i = 0; i < decodedData.Length; i++)
            {
                byte decodedValue = 0;
                byte encodedValue1 = encodedData[i * 2];
                byte encodedValue2 = encodedData[i * 2 + 1];
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue1 & 0x02) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue1 & 0x08) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue1 & 0x20) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue1 & 0x80) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue2 & 0x02) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue2 & 0x08) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue2 & 0x20) == 0 ? 0 : 1));
                decodedValue = (byte)((decodedValue << 1) | ((encodedValue2 & 0x80) == 0 ? 0 : 1));
                decodedData[i] = decodedValue;
            }
            return decodedData;
        }


        private static List<HfeSector> ExtractSectors(byte[] trackData)
        {
            var sectors = new List<HfeSector>();
            var trackDataOffset = 0;
            var firstSector = true;

            try
            {
                while (true)  
                {
                    trackDataOffset += SkipSectorIdGap(trackData, trackDataOffset, firstSector);
                    var crc = new Crc16Ccitt();
                    crc.Add(0xa1);
                    crc.Add(0xa1);
                    crc.Add(0xa1);
                    crc.Add(trackData, trackDataOffset - 1, 5);
                    int track = trackData[trackDataOffset++];
                    int side = trackData[trackDataOffset++];
                    int sector = trackData[trackDataOffset++];
                    int sectorSize = 128 << trackData[trackDataOffset++];
                    uint idcrc = (uint) (trackData[trackDataOffset++] << 8) | (uint) trackData[trackDataOffset++];
                    if (crc.Crc != idcrc)
                        throw new CrcException(side, track, sector, crc.Crc, idcrc);
                    trackDataOffset += SkipSectorDataGap(trackData, trackDataOffset);
                    var s = new HfeSector(side, track, sector, trackData, trackDataOffset, sectorSize);
                    crc = new Crc16Ccitt();
                    crc.Add(0xa1);
                    crc.Add(0xa1);
                    crc.Add(0xa1);
                    crc.Add(DATA_ADDRESS_MARK);
                    crc.Add(trackData, trackDataOffset, sectorSize);
                    trackDataOffset += sectorSize;
                    uint sectorCrc = (uint) (trackData[trackDataOffset++] << 8) | (uint) trackData[trackDataOffset++];
                    if (crc.Crc != sectorCrc)
                        throw new CrcException(side, track, sector, crc.Crc, sectorCrc);
                    sectors.Add(s);
                    firstSector = false;
                }    
            }
            catch (IndexOutOfRangeException) {}   // Very primitive way of terminating the loop when all sectors have been extracted
            
            return sectors;
        }


        /// <summary>
        /// Read the gap preceeding a sector ID record, including the ID address mark byte.
        /// </summary>
        /// <param name="trackData"></param>
        /// <param name="trackDataOffset"></param>
        /// <param name="firstSector"></param>
        /// <returns>The number of raw track bytes actually read.</returns>
        private static int SkipSectorIdGap(byte[] trackData, int trackDataOffset, bool firstSector)
        {
            var length1 = SkipBytes(trackData, trackDataOffset, 0x4e);
            if (length1 < 22) throw new DiskImageFormatException(String.Format("Reading sector ID gap and found {0} bytes of 0x4e", length1));
            trackDataOffset += length1;

            var length2 = SkipBytes(trackData, trackDataOffset, 0x00);
            if (firstSector && length2 != 12) throw new DiskImageFormatException(String.Format("Reading sector ID gap and found {0} bytes of 0x00", length2));
            if (!firstSector && length2 < 8) throw new DiskImageFormatException(String.Format("Reading sector ID gap and found {0} bytes of 0x00", length2));
            trackDataOffset += length2;

            var length3 = SkipBytes(trackData, trackDataOffset, 0xa1);
            if (length3 != 3) throw new DiskImageFormatException(String.Format("Reading sector ID gap and found {0} bytes of 0xa1", length2));
            trackDataOffset += length3;

            if (trackData[trackDataOffset] != ID_ADDRESS_MARK)
                throw new DiskImageFormatException("Reading sector ID gap but did not find data address mark");

            return length1 + length2 + length3 + 1;
        }


        /// <summary>
        /// Read the gap preceeding a sector data record, including the data address mark byte.
        /// This gap is always a minimum of 22 bytes of ox4e, exactly 12 bytes of 0x00, exactly
        /// 3 bytes of 0xa1 and finally the sector data record address mark.
        /// </summary>
        /// <param name="trackData"></param>
        /// <param name="trackDataOffset"></param>
        /// <returns></returns>
        private static int SkipSectorDataGap(byte[] trackData, int trackDataOffset)
        {
            var length1 = SkipBytes(trackData, trackDataOffset, 0x4e);
            if (length1 < 22) throw new DiskImageFormatException(String.Format("Reading sector data gap and found {0} bytes of 0x4e", length1));
            trackDataOffset += length1;

            var length2 = SkipBytes(trackData, trackDataOffset, 0x00);
            if (length2 != 12) throw new DiskImageFormatException(String.Format("Reading sector data gap and found {0} bytes of 0x00", length2));
            trackDataOffset += length2;

            var length3 = SkipBytes(trackData, trackDataOffset, 0xa1);
            if (length3 != 3) throw new DiskImageFormatException(String.Format("Reading sector data gap and found {0} bytes of 0xa1", length2));
            trackDataOffset += length3;

            if (trackData[trackDataOffset] != DATA_ADDRESS_MARK)
                throw new DiskImageFormatException("Reading sector data gap but did not find data address mark");

            return length1 + length2 + length3 + 1;
        }




        private static int SkipBytes(byte[] trackData, int trackDataOffset, byte value)
        {
            int length = 0;
            while (trackData[trackDataOffset++] == value) length++;
            return length;
        }


        public IEnumerator<HfeSector> GetEnumerator()
        {
            return ((IEnumerable<HfeSector>) sectors).GetEnumerator();
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
