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
