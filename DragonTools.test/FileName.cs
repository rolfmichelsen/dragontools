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
