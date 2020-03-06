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

using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class MemoryDiskTest
    {
        private MemoryDisk disk = null;

        private const int Heads = 1;
        private const int Tracks = 2;
        private const int Sectors = 2;
        private const int SectorSize = 10;

        public MemoryDiskTest()
        {
            disk = new MemoryDisk(Heads, Tracks, Sectors, SectorSize);
        }



        [Fact]
        public void SectorEnumeratorNotInitialized()
        {
            var enumerator = ((IEnumerable<ISector>) disk).GetEnumerator();
            var sector = enumerator.Current;
            Assert.Null(sector);
        }


        [Fact]
        public void SectorEnumeratorReturnsAllSectors()
        {
            var enumerator = ((IEnumerable<ISector>) disk).GetEnumerator();
            int sectorCount = 0;
            while (enumerator.MoveNext())
            {
                sectorCount++;
            }
            Assert.Equal(Heads*Tracks*Sectors, sectorCount);
        }

    }
}
