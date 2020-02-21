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
using System;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class DragonTapeDataBlockTest
    {

        [Fact]
        public void CreateDragonTapeDataBlock()
        {
            var payload = new byte[] {0x10, 0x20, 0x30, 0x01, 0x02, 0x03, 0x55, 0xaa};
            var block = new DragonTapeDataBlock(payload);
            
            Assert.Equal(DragonTapeBlockType.Data, block.BlockType);
            Assert.Equal(payload.Length, block.Length);
            var data = block.Data;
            for (int i = 0; i < data.Length; i++ ) Assert.Equal(payload[i], data[i]);
            Assert.Equal(0x6e, block.Checksum);   
            block.Validate();
        }


        [Fact]
        public void CreateDragonTapeDataBlock_Empty()
        {
            var block = new DragonTapeDataBlock(null);
            Assert.Equal(DragonTapeBlockType.Data, block.BlockType);
            Assert.Equal(0, block.Length);
            Assert.Null(block.Data);
            Assert.Equal(1, block.Checksum);
            block.Validate();
        }



        [Fact]
        public void CreateDragonTapeDataBlock_PayloadTooLarge_ThrowsException()
        {
            var payload = new byte[256];
            DragonTapeBlock block = null;
            try
            {
                block = new DragonTapeDataBlock(payload);
                Assert.True(false, "Block with too large payload incorrectly created.");
            }
            catch (ArgumentOutOfRangeException) {}
        }

        
    }
}
