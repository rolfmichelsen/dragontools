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

using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using System;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using Xunit;


namespace RolfMichelsen.Dragon.DragonTools.test
{
    /// <summary>
    /// Tests all common functionality for the IDiskFilesystem interface.
    /// </summary>    
    public class DiskFilesystem
    {

        private readonly string testdata = "Testdata\\";

        [Theory]
        [InlineData("os9-empty.vdk", "os9", 177664)]
        [InlineData("rsdos-empty.dsk", "rsdos", 156672)]
        public void Free(string diskimage, string filesystem, int freespace)
        {
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                Assert.Equal(freespace, dos.Free());
            }
        }


        [Theory]
        [InlineData("os9-empty.vdk", "os9", ". ..")]
        [InlineData("rsdos-empty.dsk", "rsdos", "")]
        public void ListFiles(string diskimage, string filesystem, string filelist)
        {
            var expectedfiles = filelist.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(expectedfiles);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                var files = dos.ListFiles();
                Array.Sort(files);
                Assert.Equal(expectedfiles, files);
            }
        }


        [Theory]
        [InlineData("os9-empty.vdk", "os9", "foo.bar", false)]
        public void FileExists(string diskimage, string filesystem, string file, bool exists)
        {
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                Assert.Equal(exists, dos.FileExists(file));
            }
        }



        public void IsValidFilename(string diskimage, string filesystem, string filename, bool isvalid)
        {
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                Assert.Equal(isvalid, dos.IsValidFilename(filename));
            }
        }



        [Theory]
        [InlineData("os9-empty.vdk", "os9", true)]
        public void Check(string diskimage, string filesystem, bool isvalid)
        {
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                try
                {
                    dos.Check();
                    Assert.True(isvalid);
                }
                catch (FilesystemConsistencyException e)
                {
                    if (isvalid) throw e;
                }
            }
        }


//        [Theory]
        public void DeleteFile(string diskimage, string filesystem, bool writeprotected, string filename, string exception)
        {
            var readwrite = !writeprotected;
            using (var srcdisk = DiskFactory.OpenDisk(testdata + diskimage, false))
            {
                using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), new MemoryDisk(srcdisk), readwrite))
                {
                    try
                    {
                        dos.DeleteFile(filename);
                        Assert.True(exception == null);
                        Assert.False(dos.FileExists(filename));
                        dos.Check();
                    }
                    catch (Exception e)
                    {
                        if (exception == null)
                        {
                            Assert.True(false, e.ToString());
                        }
                        else
                        {
                            Assert.Equal(exception, e.GetType().FullName);
                        }
                    }
                }
            }
        }


  //      [Theory]
        public void RenameFile(string diskimage, string filesystem, string oldfilename, string newfilename, bool writeprotected, string exception)
        {
            var readwrite = !writeprotected;
            using (var srcdisk = DiskFactory.OpenDisk(testdata + diskimage, false))
            {
                using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), new MemoryDisk(srcdisk), readwrite))
                {
                    try
                    {
                        dos.RenameFile(oldfilename, newfilename);
                        Assert.True(exception == null);
                        Assert.False(dos.FileExists(oldfilename));
                        Assert.True(dos.FileExists(newfilename));
                        dos.Check();
                    }
                    catch (Exception e)
                    {
                        if (exception == null)
                        {
                            Assert.True(false, e.ToString());
                        }
                        else
                        {
                            Assert.Equal(e.GetType().FullName, exception);
                        }
                    }
                }
            }
        }


    //    [Theory]
        public void ReadFile(string diskimage, string filesystem, string filename, int length, string filetype, string exception)
        {
            //TODO Rewrite test to properly test for file type.
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                try
                {
                    var file = dos.ReadFile(filename);
                    var fileinfo = file.FileInfo;
                    Assert.True(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
                    Assert.Equal(filetype, file.GetType().FullName);
                    Assert.Equal(filename, fileinfo.Name);
                    Assert.Equal(length, fileinfo.Size);
                }
                catch (Exception e)
                {
                    if (!String.Equals(exception, e.GetType().FullName))
                        throw e;
                }
            }            
        }




        [Theory]
        [InlineData("dragondos", 1, 40, 18, 256, 175104)]
        [InlineData("dragondos", 2, 40, 18, 256, 359424)]
        [InlineData("dragondos", 1, 80, 18, 256, 359424)]
        [InlineData("dragondos", 2, 80, 18, 256, 728064)]
        public void Initialize(string filesystem, int heads, int tracks, int sectors, int sectorsize, int freespace)
        {
            Assert.Equal("dragondos", filesystem);
            using (var disk = new MemoryDisk(heads, tracks, sectors, sectorsize))
            {
                var dos = DragonDos.Initialize(disk);
                Assert.Equal(tracks, dos.Tracks);
                Assert.Equal(sectors*heads, dos.Sectors);
                dos.Check();
                Assert.Equal(freespace, dos.Free());
            }
        }



        private static DiskFilesystemIdentifier ParseFilesystemID(string filesystem)
        {
            switch (filesystem.ToLowerInvariant())
            {
                case "dragondos":
                    return DiskFilesystemIdentifier.DragonDos;
                case "rsdos":
                    return DiskFilesystemIdentifier.RsDos;
                case "os9":
                    return DiskFilesystemIdentifier.OS9;
                case "flex":
                    return DiskFilesystemIdentifier.Flex;
                default:
                    throw new ArgumentException(String.Format("Invalid filesystem identifier \"{0}\"", filesystem));
            }
        }
    }
}
