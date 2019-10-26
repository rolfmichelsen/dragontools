/*
Copyright (c) 2011-2012, Rolf Michelsen
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

using RolfMichelsen.Dragon.DragonTools.IO.Tape;
using System.IO;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class CasTapeTest
    {

        [Fact]
        public void ReadBits()
        {
            var tapedata = new byte[] {0x01, 0x02, 0x10, 0x20, 0xaa, 0x55};
            var expected = new bool[]
                               {
                                   false, false, false, false, false, false, false, true,
                                   false, false, false, false, false, false, true, false,
                                   false, false, false, true, false, false, false, false,
                                   false, false, true, false, false, false, false, false,
                                   true, false, true, false, true, false, true, false,
                                   false, true, false, true, false, true, false, true
                               };
            var tape = new CasTape(new MemoryStream(tapedata));
            foreach (var b in expected)
            {
                Assert.Equal(b, tape.ReadBit());
            }
        }



        [Fact]
        public void ReadBytes()
        {
            var tapedata = new byte[] {0x01, 0x02, 0x10, 0x20, 0xaa, 0x55};
            var tape = new CasTape(new MemoryStream(tapedata));
            foreach (var b in tapedata)
            {
                Assert.Equal(b, tape.ReadByte());
            }
        }



        [Fact]
        public void ReadUnalignedBytes()
        {
            var tapedate = new byte[] {0x01, 0x02, 0x10, 0x20, 0xaa, 0x55};
            var expected = new byte[] {0x08, 0x10, 0x81, 0x05, 0x52};
            var tape = new CasTape(new MemoryStream(tapedate));
            tape.ReadBit();
            tape.ReadBit();
            tape.ReadBit();
            foreach (var b in expected)
            {
                Assert.Equal(b, tape.ReadByte());
            }
        }



        [Fact]
        public void WriteBits()
        {
            var stream = new MemoryStream();
            var tape = new CasTape(stream);
            var data = new bool[]
                               {
                                   false, false, false, false, false, false, false, true,
                                   false, false, false, false, false, false, true, false,
                                   false, false, false, true, false, false, false, false,
                                   false, false, true, false, false, false, false, false,
                                   true, false, true, false, true, false, true, false,
                                   false, true, false, true, false, true, false, true
                               };
            var expected = new byte[] { 0x01, 0x02, 0x10, 0x20, 0xaa, 0x55 };
            foreach (var b in data)
            {
                tape.WriteBit(b);
            }
            var actual = stream.GetBuffer();
            for (var i=0; i<expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }



        [Fact]
        public void WriteBytes()
        {
            var stream = new MemoryStream();
            var tape = new CasTape(stream);
            var data = new byte[] {0x01, 0x02, 0x10, 0x20, 0xaa, 0x55};
            foreach (var b in data)
            {
                tape.WriteByte(b);
            }
            var tapedata = stream.GetBuffer();
            for (var i=0; i<data.Length; i++)
            {
                Assert.Equal(data[i], tapedata[i]);
            }
        }




    }
}
