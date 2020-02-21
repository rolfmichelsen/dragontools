﻿/*
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

using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    
    public class OS9FileTest
    {

        [Fact]
        public void CreateFileTest()
        {
            var fileinfo = new OS9FileInfo("list", 0x4f, OS9FileAttributes.Execute|OS9FileAttributes.Read, null, null, 0, 0, null);
            byte[] data = {
                              0x87, 0xcd, 0x00, 0x4f, 0x00, 0x0d, 0x11, 0x81,
                              0x67, 0x00, 0x12, 0x02, 0x8d, 0x4c, 0x69, 0x73,
                              0xf4, 0x05, 0x9f, 0x01, 0x86, 0x01, 0x10, 0x3f,
                              0x84, 0x25, 0x2e, 0x97, 0x00, 0x9f, 0x01, 0x96,
                              0x00, 0x30, 0x43, 0x10, 0x8e, 0x00, 0xc8, 0x10,
                              0x3f, 0x8b, 0x25, 0x09, 0x86, 0x01, 0x10, 0x3f,
                              0x8c, 0x24, 0xec, 0x20, 0x14, 0xc1, 0xd3, 0x26,
                              0x10, 0x96, 0x00, 0x10, 0x3f, 0x8f, 0x25, 0x09,
                              0x9e, 0x01, 0xa6, 0x84, 0x81, 0x0d, 0x26, 0xca,
                              0x5f, 0x10, 0x3f, 0x06, 0x58, 0xbc, 0x12
                          };

            var file = (OS9File) OS9File.CreateFile(fileinfo, data);
            Assert.True(file is OS9ModuleFile);
            
            var module = (OS9ModuleFile) file;
            Assert.Equal("List", module.ModuleName);
            Assert.Equal(OS9ModuleType.Program, module.ModuleType);
            Assert.Equal(1, module.ModuleLanguage);
            Assert.Equal(8, module.ModuleAttributes);
            Assert.Equal(1, module.ModuleRevision);
            Assert.Equal(0x67, module.HeaderParity);
            Assert.Equal(0x58bc12, module.ModuleCRC);
        }
    }
}
