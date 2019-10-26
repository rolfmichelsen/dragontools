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
