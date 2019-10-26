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


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// A sector from a HFE virtual disk.
    /// </summary>
    public sealed class HfeSector : ISector
    {
        /// <summary>
        /// Sector payload data.
        /// </summary>
        private readonly byte[] data;

        /// <summary>
        /// Sector head number.
        /// </summary>
        public int Head { get; private set; }

        /// <summary>
        /// Sector track number.
        /// </summary>
        public int Track { get; private set; }

        /// <summary>
        /// Sector number.
        /// </summary>
        public int Sector { get; private set; }

        /// <summary>
        /// Sector size in bytes.
        /// </summary>
        public int Size { get { return data.Length; } }

        /// <summary>
        /// Sector payload data.
        /// </summary>
        public byte[] Data { get { return data; } }

        /// <summary>
        /// Data at a given position within the sector payload.
        /// </summary>
        public byte this[int offset] { get { return data[offset]; } }


        /// <summary>
        ///  Returns the sector CRC.
        /// </summary>
        public uint Crc { get; private set; }


        /// <summary>
        /// Create a disk sector suitable for the HFE virtual disk format.
        /// </summary>
        /// <param name="head">Sector head number.</param>
        /// <param name="track">Sector track number.</param>
        /// <param name="sector">Sector number.</param>
        /// <param name="payload">Sector payload data.</param>
        /// <param name="payloadOffset">Offset into sector payload array where payload starts.</param>
        /// <param name="payloadSize">Size of sector payload.</param>
        public HfeSector(int head, int track, int sector, byte[] payload, int payloadOffset, int payloadSize)
        {
            data = new byte[payloadSize];
            Array.Copy(payload, payloadOffset, data, 0, payloadSize);
            Head = head;
            Track = track;
            Sector = sector;
            Crc = ComputeCrc();
        }


        public HfeSectorInfo GetSectorInfo()
        {
            return new HfeSectorInfo(Head, Track, Sector, Size);
        }


        /// <summary>
        /// Returns the encoded version of the sector data record.  Thie encoded version includes the DATA address mark and the CRC
        /// checksum, but not the sync and gap bytes.
        /// </summary>
        /// <returns>Encoded sector data record.</returns>
        public byte[] Encode()
        {
            var encoded = new byte[data.Length + 3];
            var i = 0;
            encoded[i++] = HfeTrack.DataAddressMark;
            foreach (byte t in data)
                encoded[i++] = t;
            encoded[i++] = (byte) ((Crc >> 8) & 0xff);
            encoded[i] = (byte) (Crc & 0xff);
            return encoded;
        }



        private uint ComputeCrc()
        {
            var crc = new Crc16Ccitt();
            crc.Add(HfeTrack.SyncMark);
            crc.Add(HfeTrack.SyncMark);
            crc.Add(HfeTrack.SyncMark);
            crc.Add(HfeTrack.DataAddressMark);
            foreach (byte t in data)
                crc.Add(t);
            return crc.Crc;
        }
    }
}
