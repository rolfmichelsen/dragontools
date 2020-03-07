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

using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class DragonTapeHeaderBlockTest
    {

        [Fact]
        public void CreateDragonTapeHeaderBlock_BasicProgram()
        {
            var filename = "FOOBAR";
            var filetype = DragonFileType.Basic;
            var isAscii = true;
            var isGapped = false;
            var loadAddress = 0;
            var startAddress = 0;

            var payload = new byte[] {70, 79, 79, 66, 65, 82,32, 32, 0, 255, 0, 0, 0, 0, 0};

            var block = new DragonTapeHeaderBlock(filename, filetype, isAscii, isGapped, loadAddress, startAddress);

            Assert.Equal(DragonTapeBlockType.Header, block.BlockType);
            Assert.Equal(filename, block.Filename);
            Assert.Equal(filetype, block.FileType);
            Assert.Equal(isAscii, block.IsAscii);
            Assert.Equal(isGapped, block.IsGapped);
            Assert.Equal(loadAddress, block.LoadAddress);
            Assert.Equal(startAddress, block.StartAddress);
            Assert.Equal(payload.Length, block.Length);
            var data = block.Data;
            for (int i=0; i<data.Length; i++) Assert.Equal(payload[i], data[i]);
            Assert.Equal(0x07, block.Checksum);

            block.Validate();
        }



        [Fact]
        public void CreateDragonTapeHeaderBlock_MacineCodeProgram()
        {
            var filename = "BARBAR";
            var filetype = DragonFileType.MachineCode;
            var isAscii = false;
            var isGapped = false;
            var loadAddress = 10000;
            var startAddress = 50000;

            var payload = new byte[] {66 ,65, 82, 66, 65, 82, 32, 32, 2, 0, 0, 195, 80, 39, 16};

            var block = new DragonTapeHeaderBlock(filename, filetype, isAscii, isGapped, loadAddress, startAddress);

            Assert.Equal(DragonTapeBlockType.Header, block.BlockType);
            Assert.Equal(filename, block.Filename);
            Assert.Equal(filetype, block.FileType);
            Assert.Equal(isAscii, block.IsAscii);
            Assert.Equal(isGapped, block.IsGapped);
            Assert.Equal(loadAddress, block.LoadAddress);
            Assert.Equal(startAddress, block.StartAddress);
            Assert.Equal(payload.Length, block.Length);
            var data = block.Data;
            for (int i = 0; i < data.Length; i++) Assert.Equal(payload[i], data[i]);
            Assert.Equal(0x45, block.Checksum);

            block.Validate();
        }

    }
}
