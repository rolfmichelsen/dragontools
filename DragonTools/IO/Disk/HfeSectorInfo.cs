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
    /// Information about a disk sector.  This class encapsulates the sector ID record.
    /// </summary>
    public class HfeSectorInfo
    {
        /// <summary>
        /// Head number (0 or 1).
        /// </summary>
        public int Head { get; private set; }

        /// <summary>
        /// Track number.
        /// </summary>
        public int Track { get; private set; }
        
        /// <summary>
        /// Sector number.
        /// </summary>
        public int Sector { get; private set; }
        
        /// <summary>
        /// Sector payload size in bytes.
        /// </summary>
        public int Size { get; private set; }


        /// <summary>
        /// Sector ID record CRC.
        /// </summary>
        public uint Crc { get; private set; }


        /// <summary>
        /// Returns true if the CRC is valid.
        /// </summary>
        public bool IsValid {
            get { return Crc == ComputeCrc(); }
        }


        public HfeSectorInfo(int head, int track, int sector, int size)
        {
            Head = head;
            Track = track;
            Sector = sector;
            Size = size;
            Crc = ComputeCrc();
        }


        public HfeSectorInfo(int head, int track, int sector, int size, uint crc)
        {
            Head = head;
            Track = track;
            Sector = sector;
            Size = size;
            Crc = crc;
        }


        private uint ComputeCrc()
        {
            var crc = new Crc16Ccitt();
            crc.Add(HfeTrack.SyncMark);
            crc.Add(HfeTrack.SyncMark);
            crc.Add(HfeTrack.SyncMark);
            crc.Add(HfeTrack.IdAddressMark);
            crc.Add((byte) Track);
            crc.Add((byte) Head);
            crc.Add((byte) Sector);
            crc.Add(ConvertToEncodedSize(Size));
            return crc.Crc;
        }



        /// <summary>
        /// Returns the encoded version of the sector ID record.  Thie encoded version includes the ID address mark and the CRC
        /// checksum, but not the sync and gap bytes.
        /// </summary>
        /// <returns>Encoded sector ID record.</returns>
        public byte[] Encode()
        {
            var encoded = new byte[7];
            encoded[0] = HfeTrack.IdAddressMark;
            encoded[1] = (byte) Track;
            encoded[2] = (byte) Head;
            encoded[3] = (byte) Sector;
            encoded[4] = ConvertToEncodedSize(Size);
            encoded[5] = (byte) ((Crc >> 8) & 0xff);
            encoded[6] = (byte) (Crc & 0xff);
            return encoded;
        }



        /// <summary>
        /// Convert a sector size in bytes to its encoded representation as used on disk.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static byte ConvertToEncodedSize(int size)
        {
            switch (size)
            {
                case 128:
                    return 0;
                case 256:
                    return 1;
                case 512:
                    return 2;
                case 1024:
                    return 3;
            }
            throw new ArgumentException();
        }
    }

}
