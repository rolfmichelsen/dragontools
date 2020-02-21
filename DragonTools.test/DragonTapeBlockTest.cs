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
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape;
using System;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    
    public class DragonTapeBlockTest
    {
        [Fact]
        public void CreateBlock_FileHeader()
        {
            var blocktype = DragonTapeBlockType.Header;
            var payload = new byte[] { 66, 65, 82, 66, 65, 82, 32, 32, 2, 0, 0, 195, 80, 39, 16 };
            var checksum = 0x45;

            var block = DragonTapeBlock.CreateBlock(blocktype, payload, 0, payload.Length, checksum);
            
            Assert.Equal(blocktype, block.BlockType);
            Assert.Equal(checksum, block.Checksum);
            Assert.Equal(payload.Length, block.Length);
            var data = block.Data;
            for (int i = 0; i < payload.Length; i++ ) Assert.Equal(payload[i], data[i]);
            Assert.True(block is DragonTapeHeaderBlock);
            var header = (DragonTapeHeaderBlock) block;
            Assert.Equal("BARBAR", header.Filename);
            Assert.Equal(DragonFileType.MachineCode, header.FileType);
            Assert.False(header.IsAscii);
            Assert.False(header.IsGapped);
            Assert.Equal(10000, header.LoadAddress);
            Assert.Equal(50000, header.StartAddress);

            block.Validate();
        }


        [Fact]
        public void CreateBlock_UnknownBlockType()
        {
            var blocktype = (DragonTapeBlockType) 20;
            var payload = new byte[] { 66, 65, 82, 66, 65, 82, 32, 32, 2, 0, 0, 195, 80, 39, 16 };
            var checksum = 0x45;

            var block = DragonTapeBlock.CreateBlock(blocktype, payload, 0, payload.Length, checksum);

            Assert.Equal(blocktype, block.BlockType);
            Assert.Equal(checksum, block.Checksum);
            Assert.Equal(payload.Length, block.Length);
            var data = block.Data;
            for (int i=0; i<data.Length; i++) Assert.Equal(payload[i], data[i]);

            try
            {
                block.Validate();
                Assert.True(false, "This block has an invalid block type and should not pass validation.");
            }
            catch (InvalidBlockTypeException) { }
        }


        [Fact]
        public void CreateBlock_InvalidChecksum()
        {
            var blocktype = DragonTapeBlockType.Header;
            var payload = new byte[] { 66, 65, 82, 66, 65, 82, 32, 32, 2, 0, 0, 195, 80, 39, 16 };
            var checksum = 0;

            var block = DragonTapeBlock.CreateBlock(blocktype, payload, 0, payload.Length, checksum);
            
            Assert.Equal(blocktype, block.BlockType);
            Assert.Equal(checksum, block.Checksum);
            Assert.Equal(payload.Length, block.Length);
            var data = block.Data;
            for (int i=0; i<data.Length; i++) Assert.Equal(payload[i], data[i]);

            try
            {
                block.Validate();
                Assert.True(false, "This block has an invalid checksum and should not pass validation.");
            }
            catch (InvalidBlockChecksumException) {}
        }

        
        
        [Fact]
        public void ReadBlock_Synchronized()
        {
            var tapedata = new byte[] {0x55, 0x3c, 0x00, 0x0f, 0x46, 0x4f, 0x4f, 0x42, 0x41, 0x52, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x55, 0x00};
            var tapefile = new MemoryStream(tapedata, false);
            var tape = new CasTape(tapefile);

            var block = DragonTapeBlock.ReadBlock(tape, DragonTapeBlock.DefaultShortLeaderLength);

            Assert.Equal(DragonTapeBlockType.Header, block.BlockType);
            Assert.Equal(15, block.Length);
            Assert.Equal(0x08, block.Checksum);

            var headerblock = (DragonTapeHeaderBlock) block;
            Assert.Equal("FOOBAR", headerblock.Filename);
            Assert.Equal(DragonFileType.Basic, headerblock.FileType);
            Assert.False(headerblock.IsAscii);
            Assert.False(headerblock.IsGapped);

            block.Validate();
        }




        [Fact]
        public void ReadBlock_NotSynchronized()
        {
            var tapedata = new byte[] { 0xbc, 0x45, 0x55, 0x55, 0x55, 0x55, 0x55, 0x55, 0x3c, 0x00, 0x0f, 0x46, 0x4f, 0x4f, 0x42, 0x41, 0x52, 0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x55, 0x00 };
            var tapefile = new MemoryStream(tapedata, false);
            var tape = new CasTape(tapefile);

            var block = DragonTapeBlock.ReadBlock(tape, 5);

            Assert.Equal(DragonTapeBlockType.Header, block.BlockType);
            Assert.Equal(15, block.Length);
            Assert.Equal(0x08, block.Checksum);

            var headerblock = (DragonTapeHeaderBlock)block;
            Assert.Equal("FOOBAR", headerblock.Filename);
            Assert.Equal(DragonFileType.Basic, headerblock.FileType);
            Assert.False(headerblock.IsAscii);
            Assert.False(headerblock.IsGapped);

            block.Validate();            
        }





        //TODO Create tests for DragonTapeBlock.ReadBlock for non byte-aligned data
        //TODO Create tests for DragonTapeBlock.WriteBlock
        //TODO Create tests for DragonTapeBlock.Sync
    }
}
