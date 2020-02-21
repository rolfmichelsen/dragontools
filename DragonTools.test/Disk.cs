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


        [Theory]
        [InlineData("dragondos-tunes.vdk", 0, 0, 1, "55 02 0c 00 44 de 3c 00 aa 55 55 55 55 55 55 55 55 55 55 55 55 55 55 55")]
        [InlineData("dragondos-tunes.vdk", 0, 21, 6, "22 20 bf 20 38 36 30 00 35 12 03 66 85 20 49 24 cb 22 31 22 20 bf 20 39 30 30 00")]
        [InlineData("dragondos-tunes.dmk", 0, 0, 1, "55 02 0c 00 44 de 3c 00 aa 55 55 55 55 55 55 55 55 55 55 55 55 55 55 55")]
        [InlineData("dragondos-tunes.dmk", 0, 21, 6, "22 20 bf 20 38 36 30 00 35 12 03 66 85 20 49 24 cb 22 31 22 20 bf 20 39 30 30 00")]
        [InlineData("dragondos-tunes.hfe", 0, 0, 1, "55 02 0c 00 44 de 3c 00 aa 55 55 55 55 55 55 55 55 55 55 55 55 55 55 55")]
        [InlineData("dragondos-tunes.hfe", 0, 21, 6, "22 20 bf 20 38 36 30 00 35 12 03 66 85 20 49 24 cb 22 31 22 20 bf 20 39 30 30 00")]
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
