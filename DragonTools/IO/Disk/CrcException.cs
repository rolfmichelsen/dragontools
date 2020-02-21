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
    /// Exception thrown for CRC errors during reading of a disk.
    /// </summary>
    public sealed class CrcException : DiskException
    {
        public int Head { get; private set; }
        public int Track { get; private set; }
        public int Sector { get; private set; }

        public CrcException(int head, int track, int sector, uint calcCrc, uint readCrc)
            : base(String.Format("Head={0} Track={1} Sector={2} Calculated CRC={3} Read CRC={4}", head, track, sector, calcCrc, readCrc))
        {
            Head = head;
            Track = track;
            Sector = sector;
        }
    }
}
