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
