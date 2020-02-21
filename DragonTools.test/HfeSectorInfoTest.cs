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


using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class HfeSectorInfoTest
    {
        [Fact]
        public void CalculateCrc()
        {
            var sectorInfo = new HfeSectorInfo(0, 1, 1, 256);
            Assert.Equal((uint) 0x8cb8, sectorInfo.Crc);
        }


        [Fact]
        public void EncodeSectorIdRecord()
        {
            var sectorInfo = new HfeSectorInfo(0, 1, 1, 256);
            var idRecord = new byte[] { 0xfe, 0x01, 0x00, 0x01, 0x01, 0x8c, 0xb8 };
            var encoded = sectorInfo.Encode();
            Assert.Equal(idRecord.Length, encoded.Length);
            for (var i = 0; i < idRecord.Length; i++ )
                Assert.Equal(idRecord[i], encoded[i]);
        }
    }
}
