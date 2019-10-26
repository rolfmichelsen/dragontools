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
