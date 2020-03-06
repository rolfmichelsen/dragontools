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


using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class MfmStreamTest
    {
        /// <summary>
        /// MFM encoded data.  Decode data is found in <see cref="decodedData"/>.  Note that the 4th byte is a A1 sync sequence.
        /// </summary>
        private byte[] encodedData = {0x49, 0x2a, 0x49, 0x2a, 0x55, 0x55, 0x22, 0x91, 0x55, 0x55};

        /// <summary>
        /// Uncoded data.  Corresponding MFM encoded data in <see cref="encodedData"/> where the 4th byte is encoded as a sync sequence.
        /// </summary>
        private byte[] decodedData = {0x4e, 0x4e, 0x00, 0xa1, 0x00};

        /// <summary>
        /// Indicates whether a data byte in <see cref="decodeData"/> is a sync sequence.
        /// </summary>
        private bool[] syncPosition = {false, false, false, true, false};


        [Fact]
        public void DecodeMfm()
        {
            using (var stream = new MfmStream(new MemoryStream(encodedData, false)))
            {
                int readData;
                int readCount = 0;
                while ((readData = stream.ReadByte()) != -1)
                {
                    Assert.Equal(decodedData[readCount], readData);
                    readCount++;
                }
                Assert.Equal(decodedData.Length, readCount);                
            }
        }


        [Fact]
        public void DetectSync()
        {
            using (var stream = new MfmStream(new MemoryStream(encodedData, false)))
            {
                int readCount = 0;
                bool sync;
                while (stream.ReadByte(out sync) != -1)
                {
                    Assert.Equal(syncPosition[readCount], sync);
                    readCount++;
                }
                Assert.Equal(decodedData.Length, readCount);                
            }
        }


        [Fact]
        public void EncodeMfmWithSync()
        {
            using (var memstream = new MemoryStream())
            {
                using (var stream = new MfmStream(memstream))
                {
                    for (var i = 0; i < decodedData.Length; i++)
                    {
                        if (syncPosition[i])
                        {
                            stream.WriteSync();
                        }
                        else
                        {
                            stream.WriteByte(decodedData[i]);
                        }
                    }
                }

                var encoded = memstream.GetBuffer();
                for (var i = 0; i < encodedData.Length; i++)
                {
                    Assert.Equal(encodedData[i], encoded[i]);
                }
            }
        }
    }
}
