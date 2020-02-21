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
        [InlineData("dragondos-empty.vdk", "dragondos", 175104)]
        [InlineData("dragondos-empty-1s-40t.dmk", "dragondos", 175104)]
        [InlineData("dragondos-empty-2s-40t.dmk", "dragondos", 359424)]
        [InlineData("dragondos-empty-1s-80t.vdk", "dragondos", 359424)]
        [InlineData("dragondos-empty-2s-40t.vdk", "dragondos", 359424)]
        [InlineData("dragondos-empty-2s-80t.vdk", "dragondos", 728064)]
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
        [InlineData("dragondos-empty.vdk", "dragondos", "")]
        [InlineData("dragondos-empty-1s-40t.dmk", "dragondos", "")]
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
        [InlineData("dragondos-empty.vdk", "dragondos", "FOO.BAR", false)]
        [InlineData("dragondos-empty-1s-40t.dmk", "dragondos", "FOO.BAR", false)]
        [InlineData("os9-empty.vdk", "os9", "foo.bar", false)]
        public void FileExists(string diskimage, string filesystem, string file, bool exists)
        {
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                Assert.Equal(exists, dos.FileExists(file));
            }
        }



        [Theory]
        [InlineData("dragondos-empty.vdk", "dragondos", "FOO.BAR", true)]
        [InlineData("dragondos-empty.vdk", "dragondos", "FOO-BAR.BAR", true)]
        [InlineData("dragondos-empty.vdk", "dragondos", "42FOO.BAR", true)]
        [InlineData("dragondos-empty.vdk", "dragondos", "FOO.BARZ", false)]
        [InlineData("dragondos-empty.vdk", "dragondos", "FOOBARFOO.BAR", false)]
        public void IsValidFilename(string diskimage, string filesystem, string filename, bool isvalid)
        {
            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), testdata + diskimage, false))
            {
                Assert.Equal(isvalid, dos.IsValidFilename(filename));
            }
        }



        [Theory]
        [InlineData("dragondos-empty.vdk", "dragondos", true)]
        [InlineData("dragondos-empty-1s-40t.dmk", "dragondos", true)]
        [InlineData("dragondos-empty-1s-80t.vdk", "dragondos", true)]
        [InlineData("dragondos-empty-2s-40t.vdk", "dragondos", true)]
        [InlineData("dragondos-empty-2s-80t.vdk", "dragondos", true)]
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
