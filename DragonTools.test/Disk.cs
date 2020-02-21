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
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    
    /// <summary>
    /// Tests all common functionality for the Disk interface.
    /// </summary>
    public class Disk
    {

        private readonly string testdata = "Testdata\\";


        [Theory]
        [InlineData("80t_18spt_256bps_2s.dsk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.JvcDisk", 2, 80)]
        [InlineData("128t_255spt_256bps_2s.dsk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.JvcDisk", 2, 128)]
        [InlineData("40t_255spt_256bps_2s.dsk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.JvcDisk", 2, 40)]
        [InlineData("80t_18spt_1024bps_2s.dsk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.JvcDisk", 2, 80)]
        [InlineData("40t_18spt_256bps_2s.vdk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.VdkDisk", 2, 40)]
        [InlineData("40t_18spt_256bps_1s.vdk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.VdkDisk", 1, 40)]
        [InlineData("80t_18spt_256bps_2s.vdk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.VdkDisk", 2, 80)]
        [InlineData("80t_18spt_256bps_1s.vdk", "RolfMichelsen.Dragon.DragonTools.IO.Disk.VdkDisk", 1, 80)]
        public void DiskGeometry(string filename, string classtype, int heads, int tracks)
        {
            using (var disk = DiskFactory.OpenDisk(testdata + filename, false))
            {
                Assert.Equal(classtype, disk.GetType().FullName);
                Assert.Equal(heads, disk.Heads);
                Assert.Equal(tracks, disk.Tracks);
            }

        }


//        [Theory]
        public void ReadSector(string filename, int head, int track, int sector, string data)
        {
            var expectedSectorDataRaw = data.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var expectedSectorData = new byte[expectedSectorDataRaw.Length];
            for (int i = 0; i < expectedSectorDataRaw.Length; i++)
            {
                expectedSectorData[i] = Convert.ToByte(expectedSectorDataRaw[i], 16);
            }

            using (var disk = DiskFactory.OpenDisk(testdata + filename, false))
            {
                var sectorData = disk.ReadSector(head, track, sector);
                for (int i = 0; i < expectedSectorData.Length; i++)
                    Assert.Equal(expectedSectorData[i], sectorData[i]);
            }
        }
    }
}
