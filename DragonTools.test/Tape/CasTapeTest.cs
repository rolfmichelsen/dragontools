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
