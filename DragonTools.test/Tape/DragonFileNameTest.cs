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
    
    public class DragonFileNameTest
    {


        [Fact]
        public void FilenameRules()
        {
            var filename1 = new DragonFileName("FOOBAR");
            var filename2 = new DragonFileName("FOOFOO");
            var filename3 = new DragonFileName("FOOBAR");
            var filename4 = new DragonFileName("foobar");
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
