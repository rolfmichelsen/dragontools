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


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// A sector from a DMK virtual disk.
    /// </summary>
    public sealed class DmkSector : ISector
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
        public DmkSector(int head, int track, int sector, byte[] payload, int payloadOffset, int payloadSize)
        {
            data = new byte[payloadSize];
            Array.Copy(payload, payloadOffset, data, 0, payloadSize);
            Head = head;
            Track = track;
            Sector = sector;
            Crc = ComputeCrc();
        }


        private uint ComputeCrc()
        {
            var crc = new Crc16Ccitt();
            crc.Add(DmkTrack.SyncMark);
            crc.Add(DmkTrack.SyncMark);
            crc.Add(DmkTrack.SyncMark);
            crc.Add(DmkTrack.DataAddressMark);
            foreach (byte t in data)
                crc.Add(t);
            return crc.Crc;
        }
    }
}
