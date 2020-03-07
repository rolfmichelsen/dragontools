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
using System.Diagnostics;
using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class HfeDiskTest
    {
        [Fact]
        public void Create()
        {
            using (var diskStream = new MemoryStream())
            {
                using (var disk = HfeDisk.Create(diskStream, 1, 40, 18, 256))
                {
                    var diskHeader = disk.DiskHeader;
                    Assert.Equal(0, diskHeader.FileFormatVersion);
                    Assert.Equal(40, diskHeader.Tracks);
                    Assert.Equal(1, diskHeader.Sides);
                    Assert.Equal(HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM, diskHeader.TrackEncoding);
                    Assert.Equal(HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM, diskHeader.TrackEncoding0);

                    var track = disk.GetTrack(0, 0);
                    Assert.NotNull(track);

                    Assert.True(disk.SectorExists(0, 0, 1));
                }
            }
        }


        [Fact]
        public void CreateAndOpen()
        {
            byte[] diskImage;

            /* Create a HFE disk image and keep the image in the diskImage buffer. */
            using (var diskStream = new MemoryStream())
            {
                using (var disk = HfeDisk.Create(diskStream, 1, 40, 18, 256)) { }
                diskImage = diskStream.GetBuffer();
            }

            /* Now open the disk image in diskImage and verify that it is correct. */
            using (var diskStream = new MemoryStream(diskImage, false))
            {
                using (var disk = HfeDisk.Open(diskStream, false))
                {
                    var header = disk.DiskHeader;
                    Assert.Equal(0, header.FileFormatVersion);
                    Assert.Equal(40, header.Tracks);
                    Assert.Equal(1, header.Sides);
                    Assert.Equal(HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM, header.TrackEncoding);
                    Assert.Equal(HfeDiskHeader.TrackEncodingMode.ISOIBM_MFM, header.TrackEncoding0);
                    var track = disk.GetTrack(0, 0);
                    Assert.NotNull(track);
                    Assert.True(disk.SectorExists(0, 0, 1));
                    Assert.True(disk.SectorExists(0, 0, 3));
                    Assert.True(disk.SectorExists(0, 0, 2));
                    Assert.True(disk.SectorExists(0, 0, 17));
                    Assert.True(disk.SectorExists(0, 0, 18));
                    Assert.True(disk.SectorExists(0, 39, 1));
                    Assert.True(disk.SectorExists(0, 39, 18));
                }
            }
        
        }

        

    }
}
