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

using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

using RolfMichelsen.Dragon.DragonTools.IO.Disk;


namespace RolfMichelsen.Dragon.DragonTools.Test.Disk
{
    
    /// <summary>
    /// Tests all common functionality for the Disk interface.
    /// </summary>
    public class DiskTest
    {

        private readonly string testdata = "Testdata\\Disk\\";


        [Theory]
        [InlineData("testdisk-1s-40t.vdk", typeof(VdkDisk), 1, 40)]
        [InlineData("testdisk-2s-40t.vdk", typeof(VdkDisk), 2, 40)]
        [InlineData("testdisk-2s-80t.vdk", typeof(VdkDisk), 2, 80)]
        [InlineData("testdisk-1s-40t.hfe", typeof(HfeDisk), 1, 40)]
        [InlineData("testdisk-1s-40t.dmk", typeof(DmkDisk), 1, 40)]
        [InlineData("testdisk-1s-40t.dsk", typeof(JvcDisk), 1, 40)]
        public void DiskGeometry(string imagename, Type classtype, int heads, int tracks)
        {
            using (var disk = DiskFactory.OpenDisk(testdata + imagename, false))
            {
                disk.GetType().Should().Be(classtype);
                disk.Heads.Should().Be(heads);
                disk.Tracks.Should().Be(tracks);
            }
        }


        [Theory]
        [InlineData("testdisk-1s-40t.vdk")]
        [InlineData("testdisk-2s-40t.vdk")]
        [InlineData("testdisk-2s-80t.vdk")]
        [InlineData("testdisk-1s-40t.hfe")]
        [InlineData("testdisk-1s-40t.dmk")]
        [InlineData("testdisk-1s-40t.dsk")]
        public void ReadSectors(string imagename)
        {
            using (var disk = DiskFactory.OpenDisk(testdata + imagename, false))
            {
                foreach (var sector in disk)
                {
                    int t = sector[0];
                    int h = sector[1];
                    int s = sector[2];
                    t.Should().Be(sector.Track);
                    h.Should().Be(sector.Head);
                    s.Should().Be(sector.Sector);
                }
            }
        }


        [Theory]
        [InlineData("testdisk-1s-40t.vdk", 720)]
        [InlineData("testdisk-2s-40t.vdk", 1440)]
        [InlineData("testdisk-2s-80t.vdk", 2880)]
        [InlineData("testdisk-1s-40t.hfe", 720)]
        [InlineData("testdisk-1s-40t.dmk", 720)]
        [InlineData("testdisk-1s-40t.dsk", 720)]
        public void EnumerateSectors(string imagename, int sectorCount)
        {
            var sectors = new Dictionary<DiskPosition, bool>();
            using (var disk = DiskFactory.OpenDisk(testdata + imagename, false))
            {
                foreach (var sector in disk)
                {
                    var pos = new DiskPosition(sector.Track, sector.Head, sector.Sector);
                    sectors.ContainsKey(pos).Should().BeFalse();
                    sectors[pos] = true;
                }
            }
            sectors.Count.Should().Be(sectorCount);
        }


        private struct DiskPosition
        {
            readonly int Track;
            readonly int Head;
            readonly int Sector;

            public DiskPosition(int t, int h, int s)
            {
                Track = t;
                Head = h;
                Sector = s;
            }
        }
    }
}
