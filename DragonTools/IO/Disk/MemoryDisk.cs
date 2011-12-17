/*
Copyright (c) 2011, Rolf Michelsen
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
    /// This class provides a virtual disk where all disk data is stored in a memory buffer.
    /// </summary>
    public sealed class MemoryDisk : AbstractDisk
    {
        /// <summary>
        /// Flushes any pending write operations to the backing store.
        /// </summary>
        public override void Flush()
        {  }

        /// <summary>
        /// Returns the offset into the <see cref="AbstractDisk.diskData">diskData</see> array of the first byte of the identified sector.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>The byte offset of the first byte of this sector.</returns>
        protected override int SectorOffset(int head, int track, int sector)
        {
            return (head * Tracks * Sectors + track * Sectors + sector) * SectorSize;
        }


        /// <summary>
        /// Constructs an empty memory-based virtual disk with the given geometry.
        /// </summary>
        /// <param name="heads">Number of disk heads.</param>
        /// <param name="tracks">Number of tracks per side.</param>
        /// <param name="sectors">Number of sectors per track.</param>
        /// <param name="sectorSize">The number of bytes in each sector.</param>
        public MemoryDisk(int heads, int tracks, int sectors, int sectorSize)
        {
            IsWriteable = true;
            Heads = heads;
            Tracks = tracks;
            Sectors = sectors;
            SectorSize = sectorSize;
            diskData = new byte[Heads * Tracks * Sectors * SectorSize];
        }


        /// <summary>
        /// Constructs a memory-based virtual disk as a sector by sector clone of another disk.
        /// </summary>
        /// <param name="source">Disk to clone.</param>
        public MemoryDisk(IDisk source) : this(source.Heads, source.Tracks, source.Sectors, source.SectorSize)
        {
            var data = new byte[SectorSize];
            for (int h=0; h<Heads; h++)
            {
                for (int t=0; t<Tracks; t++)
                {
                    for (int s=0; s<Sectors; s++)
                    {
                        source.ReadSector(h, t, s, data, 0, SectorSize);
                        WriteSector(h, t, s, data);
                    }
                }
            }
        }

    }
}
