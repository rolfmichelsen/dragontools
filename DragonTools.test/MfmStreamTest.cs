/*
Copyright (c) 2011-2014, Rolf Michelsen
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
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;

namespace RolfMichelsen.Dragon.DragonTools.test
{
    [TestClass]
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


        [TestMethod]
        public void DecodeMfm()
        {
            using (var stream = new MfmStream(new MemoryStream(encodedData, false)))
            {
                int readData;
                int readCount = 0;
                while ((readData = stream.ReadByte()) != -1)
                {
                    Assert.AreEqual(decodedData[readCount], readData);
                    readCount++;
                }
                Assert.AreEqual(decodedData.Length, readCount);                
            }
        }


        [TestMethod]
        public void DetectSync()
        {
            using (var stream = new MfmStream(new MemoryStream(encodedData, false)))
            {
                int readCount = 0;
                bool sync;
                while (stream.ReadByte(out sync) != -1)
                {
                    Assert.AreEqual(syncPosition[readCount], sync);
                    readCount++;
                }
                Assert.AreEqual(decodedData.Length, readCount);                
            }
        }


        [TestMethod]
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
                    Assert.AreEqual(encodedData[i], encoded[i]);
                }
            }
        }
    }
}
