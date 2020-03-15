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

using FluentAssertions;
using System;
using Xunit;

using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;


namespace RolfMichelsen.Dragon.DragonTools.Test.DragonDos
{
    public class DragonDosTest
    {
        private readonly string testdata = "Testdata\\DragonDos\\";

        [Theory]
        [InlineData("dragondos-empty-1s-40t.dmk", 175104)]
        [InlineData("dragondos-empty-2s-40t.dmk", 359424)]
        [InlineData("dragondos-empty-1s-80t.dmk", 359424)]
        [InlineData("dragondos-empty-2s-80t.dmk", 728064)]
        public void Free(string imagename, int free)
        {
            using (var fs = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, testdata + imagename, false))
            {
                fs.Free().Should().Be(free);
            }
        }
    }
}
