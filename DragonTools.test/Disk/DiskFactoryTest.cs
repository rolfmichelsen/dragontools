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
using Xunit;

using RolfMichelsen.Dragon.DragonTools.IO.Disk;


namespace RolfMichelsen.Dragon.DragonTools.Test.Disk
{
    public class DiskFactoryTest
    {
        private readonly string testdata = "Testdata\\Disk\\";


        [Theory]
        [InlineData("testdisk-1s-40t.vdk", typeof(VdkDisk))]
        [InlineData("testdisk-2s-80t.vdk", typeof(VdkDisk))]
        [InlineData("testdisk-1s-40t.hfe", typeof(HfeDisk))]
        [InlineData("testdisk-1s-40t.dmk", typeof(DmkDisk))]
        [InlineData("testdisk-1s-40t.dsk", typeof(JvcDisk))]
        public void OpenDisk(string imagename, Type imagetype)
        {
            var imagepath = testdata + imagename;
            using (var disk = DiskFactory.OpenDisk(imagepath, false))
            {
                disk.GetType().Should().Be(imagetype);
            }
        }

        [Theory]
        [InlineData("testdisk-1s-40t.xxx")]
        public void OpenDiskUnknownFiletype(string imagename)
        {
            var imagepath = testdata + imagename;
            using (var disk = DiskFactory.OpenDisk(imagepath, false))
            {
                disk.Should().BeNull();
            }
        }

    }
}
