/*
Copyright (c) 2011-2015, Rolf Michelsen
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
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.Flex;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.RsDos;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    public class FileName
    {
        [Theory]
        [InlineData("dragondos", "FOO.BAR", "FOO.BAR", "FOO", "BAR")]
        [InlineData("flex", "FOO.BAR", "FOO.BAR", "FOO", "BAR")]
        [InlineData("rsdos", "FOO.BAR", "FOO.BAR", "FOO", "BAR")]
        public void FileNameAccess(string filesystem, string pathname, string name, string basename, string extension)
        {
            IFileName filename = null;
            switch (filesystem.ToLower())
            {
                case "dragondos":
                    filename = DragonDos.GetFileName(pathname);
                    break;
                case "flex":
                    filename = Flex.GetFileName(pathname);
                    break;
                case "os9":
                    filename = OS9.GetFileName(pathname);
                    break;
                case "rsdos":
                    filename = RsDos.GetFileName(pathname);
                    break;
                default:
                    Assert.True(false, "Unknown filesystem " + filesystem);
                    break;
            }

            Assert.Equal(name, filename.Name);
            Assert.Equal(basename, filename.Base);
            Assert.Equal(extension, filename.Extension);
            //TODO Add test for Ascend method
            //TODO Add test for Descend method
        }


        //TODO Add tests for IFileName.IsValid method
    }
}
