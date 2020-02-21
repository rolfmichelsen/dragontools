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
        [InlineData("dragondos-empty.vdk", "dragondos", 175104)]
        [InlineData("dragondos-tunes.vdk", "dragondos", 10752)]
        [InlineData("dragondos-tunes.hfe", "dragondos", 10752)]
        [InlineData("dragondos-empty-1s-40t.dmk", "dragondos", 175104)]
        [InlineData("dragondos-empty-2s-40t.dmk", "dragondos", 359424)]
        [InlineData("dragondos-empty-1s-80t.vdk", "dragondos", 359424)]
        [InlineData("dragondos-empty-2s-40t.vdk", "dragondos", 359424)]
        [InlineData("dragondos-empty-2s-80t.vdk", "dragondos", 728064)]
        [InlineData("os9-empty.vdk", "os9", 177664)]
        [InlineData("os9-system.vdk", "os9", 48896)]
        [InlineData("os9-system.hfe", "os9", 48896)]
        [InlineData("rsdos-empty.dsk", "rsdos", 156672)]
        [InlineData("rsdos-demos01.dsk", "rsdos", 59904)]
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
        [InlineData("dragondos-tunes.vdk", "dragondos", "NOTABENE.BIN DRYBONES.BIN DANCER.BIN BARREL.BIN GARDEN.BIN ORGANIST.BIN SWAN.BIN QUARTETT.BIN")]
        [InlineData("dragondos-tunes.dmk", "dragondos", "NOTABENE.BIN DRYBONES.BIN DANCER.BIN BARREL.BIN GARDEN.BIN ORGANIST.BIN SWAN.BIN QUARTETT.BIN")]
        [InlineData("dragondos-tunes.hfe", "dragondos", "NOTABENE.BIN DRYBONES.BIN DANCER.BIN BARREL.BIN GARDEN.BIN ORGANIST.BIN SWAN.BIN QUARTETT.BIN")]
        [InlineData("os9-empty.vdk", "os9", ". ..")]
        [InlineData("os9-system.vdk", "os9", ".. . OS9Boot CMDS SYS DEFS startup RUN32 RUN51")]
        [InlineData("os9-system.hfe", "os9", ".. . OS9Boot CMDS SYS DEFS startup RUN32 RUN51")]
        [InlineData("flex-system.vdk", "flex", "FLEX.SYS ERRORS.SYS PRINT.SYS SERIAL.SYS APPEND.CMD ASN.CMD BACKUP.CMD BAUD.CMD BUILD.CMD CAT.CMD COPY.CMD CS.CMD DATE.CMD DELETE.CMD DRIVES.CMD EXEC.CMD H.CMD I.CMD JUMP.CMD LINK.CMD LIST.CMD       NEWDISK.CMD O.CMD P.CMD PROT.CMD RENAME.CMD S.CMD SAVE.CMD SAVE.LOW SDC.CMD STEP.CMD TTYSET.CMD VERIFY.CMD VERSION.CMD XOUT.CMD STARTUP.TXT WELCOME.TXT DEMO1.TXT CS.ITA CS.UK CS.FRA CS.GER CS.DEN CS.SWE CS.USA CS.SPA CSTEST.TXT SERSYS.TXT")]
        [InlineData("rsdos-empty.dsk", "rsdos", "")]
        [InlineData("rsdos-demos01.dsk", "rsdos", "SPACEDEM.BIN DANCER.BAS DANCER.BIN GLASDEMO.BIN LINEDEM2.BIN MPATDEMO.BIN STICKS.BIN 28.BAS 3GOONS.BAS DEMO4CC3.BAS FUNDEMO.BAS GETSMART.BAS RSDEMO.BAS SINX.BAS SPINNERS.BAS")]
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
        [InlineData("dragondos-tunes.vdk", "dragondos", "FOO.BAR", false)]
        [InlineData("dragondos-tunes.vdk", "dragondos", "SWAN.BIN", true)]
        [InlineData("dragondos-tunes.hfe", "dragondos", "FOO.BAR", false)]
        [InlineData("dragondos-tunes.hfe", "dragondos", "SWAN.BIN", true)]
        [InlineData("os9-empty.vdk", "os9", "foo.bar", false)]
        [InlineData("os9-system.vdk", "os9", "foo.bar", false)]
        [InlineData("os9-system.vdk", "os9", "startup", true)]
        [InlineData("os9-system.hfe", "os9", "foo.bar", false)]
        [InlineData("os9-system.hfe", "os9", "startup", true)]
        [InlineData("flex-system.vdk", "flex", "FLEX.SYS", true)]
        [InlineData("flex-system.vdk", "flex", "FOO.BAR", false)]
        [InlineData("rsdos-demos01.dsk", "rsdos", "FOO.BAR", false)]
        [InlineData("rsdos-demos01.dsk", "rsdos", "SPINNERS.BAS", true)]
        [InlineData("rsdos-demos01.dsk", "rsdos", "DANCER.BIN", true)]        
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
        [InlineData("dragondos-tunes.vdk", "dragondos", true)]
        [InlineData("dragondos-tunes.hfe", "dragondos", true)]
        [InlineData("os9-empty.vdk", "os9", true)]
        [InlineData("os9-system.vdk", "os9", true)]
        [InlineData("rsdos-demos01.dsk", "rsdos", true)]
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


        [Theory]
        [InlineData("dragondos-tunes.vdk", "dragondos", false, "DRYBONES.BIN", null)]
        [InlineData("dragondos-tunes.dmk", "dragondos", false, "DRYBONES.BIN", null)]
        [InlineData("dragondos-tunes.hfe", "dragondos", false, "DRYBONES.BIN", null)]
        [InlineData("dragondos-tunes.vdk", "dragondos", false, "FOO.BAS", "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileNotFoundException")]
        [InlineData("dragondos-tunes.vdk", "dragondos", true, "DRYBONES.BIN", "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FilesystemNotWriteableException")]
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


        [Theory]
        [InlineData("dragondos-tunes.vdk", "dragondos", "DRYBONES.BIN", "FOO.BAR", false, null)]
        [InlineData("dragondos-tunes.vdk", "dragondos", "FOO.BAS", "FOOBAR.BAS", false, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileNotFoundException")]
        [InlineData("dragondos-tunes.vdk", "dragondos", "DRYBONES.BIN", "FOO.BAR", true, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FilesystemNotWriteableException")]
        [InlineData("dragondos-tunes.vdk", "dragondos", "DRYBONES.BIN", "DANCER.BIN", false, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileExistsException")]
        [InlineData("dragondos-tunes.vdk", "dragondos", "DRYBONES.BIN", "FOOBARFOOBAR.FOO", false, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.InvalidFilenameException")]
        [InlineData("dragondos-tunes.hfe", "dragondos", "DRYBONES.BIN", "FOO.BAR", false, null)]
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


        [Theory]
        [InlineData("dragondos-tunes.vdk", "dragondos", "DRYBONES.BIN", 28490, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos.DragonDosFile", null)]
        [InlineData("dragondos-tunes.vdk", "dragondos", "FOO.BAR", 0, null, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileNotFoundException")]
        [InlineData("dragondos-tunes.hfe", "dragondos", "DRYBONES.BIN", 28490, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos.DragonDosFile", null)]
        [InlineData("dragondos-tunes.hfe", "dragondos", "FOO.BAR", 0, null, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileNotFoundException")]
        [InlineData("os9-system.vdk", "os9", "startup", 38, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9.OS9DataFile", null)]
        [InlineData("os9-system.vdk", "os9", "os9boot", 12172, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9.OS9ModuleFile", null)]
        [InlineData("os9-system.vdk", "os9", "foobar", 0, null, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileNotFoundException")]
        [InlineData("os9-system.vdk", "os9", "sys/errmsg", 3172, "RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9.OS9DataFile", null)]
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
