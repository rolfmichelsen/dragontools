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

using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    
    public class DragonDosFileNameTest
    {


        [Fact]
        public void Equality()
        {
            var filename1 = new DragonDosFileName("FOOBAR.DAT");
            var filename2 = new DragonDosFileName("FOOFOO.DAT");
            var filename3 = new DragonDosFileName("FOOBAR.DAT");
            var filename4 = new DragonDosFileName("FOOBAR.dat");
            var filename5 = filename1;
            var filename6 = filename1.Clone();

            Assert.NotEqual(filename1, filename2);
            Assert.Equal(filename1, filename3);
            Assert.NotEqual(filename1, filename4);
            Assert.Equal(filename1, filename5);
            Assert.Same(filename1, filename5);
            Assert.Equal(filename1, filename6);
            Assert.NotSame(filename1, filename6);
        }
    }
}
