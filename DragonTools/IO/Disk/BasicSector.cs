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
using System.Collections.ObjectModel;


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Basic implementation of the ISector interface.
    /// Does only provide the minimum functionality specified by IDisk.  It is suitable
    /// for very basic virtual disk representations.
    /// </summary>
    public sealed class BasicSector : ISector
    {
        private byte[] data;

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
        public int Size { get; private set; }

        /// <summary>
        /// Sector payload data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return data;
            }
            
        }

        /// <summary>
        /// Data at a given position within the sector payload.
        /// </summary>
        public byte this[int offset]
        {
            get { return data[offset]; }
        }


        /// <summary>
        /// Create a sector object.
        /// </summary>
        /// <param name="head">Head number.</param>
        /// <param name="track">Track number.</param>
        /// <param name="sector">Sector number.</param>
        /// <param name="data">Sector data.</param>
        public BasicSector(int head, int track, int sector, byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");

            Head = head;
            Track = track;
            Sector = sector;
            Size = data.Length;
            this.data = data;
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
            return String.Format("Sector head={0} track={1} sector={2} size={3}", Head, Track, Sector, Size);
        }
    }
}
