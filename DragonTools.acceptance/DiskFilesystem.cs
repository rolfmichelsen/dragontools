/*
Copyright (c) 2011, Rolf Michelsen
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RolfMichelsen.Dragon.DragonTools.acceptance
{
    /// <summary>
    /// Tests all common functionality for the IDiskFilesystem interface.
    /// </summary>    
    [TestClass()]
    public class DiskFilesystem
    {


        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "Free", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void Free()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var freespace = Convert.ToInt32(TestContext.DataRow["freespace"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                Assert.AreEqual(freespace, dos.Free());
            }
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "ListFiles", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void ListFiles()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var expectedfiles = Convert.ToString(TestContext.DataRow["filelist"]).Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(expectedfiles);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                var files = dos.ListFiles();
                Array.Sort(files);
                CollectionAssert.AreEqual(expectedfiles, files);
            }
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "FileExists", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void FileExists()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var file = Convert.ToString(TestContext.DataRow["file"]);
            var exists = Convert.ToBoolean(TestContext.DataRow["exists"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                Assert.AreEqual(exists, dos.FileExists(file));
            }
        }



        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "IsValidFilename", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void IsValidFilename()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var filename = Convert.ToString(TestContext.DataRow["file"]);
            var isvalid = Convert.ToBoolean(TestContext.DataRow["valid"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                Assert.AreEqual(isvalid, dos.IsValidFilename(filename));
            }
        }



        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "Check", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void Check()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var isvalid = Convert.ToBoolean(TestContext.DataRow["valid"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                try
                {
                    dos.Check();
                    Assert.IsTrue(isvalid);
                }
                catch (FilesystemConsistencyException e)
                {
                    Assert.IsFalse(isvalid);
                }
            }
        }



        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "DeleteFile", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void DeleteFile()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var filename = Convert.ToString(TestContext.DataRow["filename"]);
            var readwrite = !Convert.ToBoolean(TestContext.DataRow["writeprotected"]);
            var exception = TestContext.DataRow.IsNull("exception") ? null : Convert.ToString(TestContext.DataRow["exception"]);

            using (var srcdisk = DiskFactory.OpenDisk(diskimage, false))
            {
                using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), new MemoryDisk(srcdisk), readwrite))
                {
                    try
                    {
                        dos.DeleteFile(filename);
                        Assert.IsTrue(exception == null);
                        Assert.IsFalse(dos.FileExists(filename));
                        dos.Check();
                    }
                    catch (Exception e)
                    {
                        if (exception == null)
                        {
                            Assert.Fail(e.ToString());
                        }
                        else
                        {
                            Assert.AreEqual(exception, e.GetType().FullName);
                        }
                    }
                }
            }
        }




        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "RenameFile", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void RenameFile()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var oldfilename = Convert.ToString(TestContext.DataRow["oldfilename"]);
            var newfilename = Convert.ToString(TestContext.DataRow["newfilename"]);
            var readwrite = !Convert.ToBoolean(TestContext.DataRow["writeprotected"]);
            var exception = TestContext.DataRow.IsNull("exception") ? null : Convert.ToString(TestContext.DataRow["exception"]);

            using (var srcdisk = DiskFactory.OpenDisk(diskimage, false))
            {
                using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), new MemoryDisk(srcdisk), readwrite))
                {
                    try
                    {
                        dos.RenameFile(oldfilename, newfilename);
                        Assert.IsTrue(exception == null);
                        Assert.IsFalse(dos.FileExists(oldfilename));
                        Assert.IsTrue(dos.FileExists(newfilename));
                        dos.Check();
                    }
                    catch (Exception e)
                    {
                        if (exception == null)
                        {
                            Assert.Fail(e.ToString());
                        }
                        else
                        {
                            Assert.AreEqual(e.GetType().FullName, exception);
                        }
                    }
                }
            }
        }



        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "ReadFileRaw", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void ReadFileRaw()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var filename = Convert.ToString(TestContext.DataRow["filename"]);
            var length = Convert.ToInt32(TestContext.DataRow["length"]);
            var exception = Convert.ToString(TestContext.DataRow["exception"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                try
                {
                    var filedata = dos.ReadFileRaw(filename);
                    Assert.IsTrue(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
                    Assert.AreEqual(length, filedata.Length);
                }
                catch (Exception e)
                {
                    if (!String.Equals(exception, e.GetType().FullName))
                        throw e;
                }
            }
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "ReadFile", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void ReadFile()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var filename = Convert.ToString(TestContext.DataRow["filename"]);
            var length = Convert.ToInt32(TestContext.DataRow["length"]);
            var filetype = Convert.ToString(TestContext.DataRow["filetype"]);
            var exception = Convert.ToString(TestContext.DataRow["exception"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                try
                {
                    var file = dos.ReadFile(filename);
                    Assert.IsTrue(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
                    Assert.AreEqual(filetype, file.GetType().FullName);
                    Assert.AreEqual(filename, file.Name);
                    Assert.AreEqual(length, file.Size);
                }
                catch (Exception e)
                {
                    if (!String.Equals(exception, e.GetType().FullName))
                        throw e;
                }
            }            
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "WriteFileRaw", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void WriteFileRaw()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var filename = Convert.ToString(TestContext.DataRow["filename"]);
            var length = Convert.ToInt32(TestContext.DataRow["length"]);
            var exception = Convert.ToString(TestContext.DataRow["exception"]);
            var readwrite = !Convert.ToBoolean(TestContext.DataRow["writeprotected"]);

            var filedata = new byte[length];

            using (var srcdisk = DiskFactory.OpenDisk(diskimage, false))
            {
                using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), new MemoryDisk(srcdisk), readwrite))
                {
                    try
                    {
                        dos.WriteFileRaw(filename, filedata);
                        Assert.IsTrue(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
                        dos.Check();
                        Assert.IsTrue(dos.FileExists(filename), "File " + filename + " does not exist");
                    }
                    catch (Exception e)
                    {
                        if (!String.Equals(exception, e.GetType().FullName))
                            throw e;
                    }
                }
            }
        }


        [TestMethod()]
        public void WriteFile()
        {
            Assert.Inconclusive();
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "GetFileParser", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void GetFileParser()
        {
            var diskimage = Convert.ToString(TestContext.DataRow["diskimage"]);
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var parserclass = Convert.ToString(TestContext.DataRow["parserclass"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), diskimage, false))
            {
                var parser = dos.GetFileParser();
                Assert.AreEqual(parserclass, parser.GetType().FullName);
            }
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DiskFilesystem.xml", "Initialize", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DiskFilesystem.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void Initialize()
        {
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var heads = Convert.ToInt32(TestContext.DataRow["heads"]);
            var tracks = Convert.ToInt32(TestContext.DataRow["tracks"]);
            var sectors = Convert.ToInt32(TestContext.DataRow["sectors"]);
            var sectorsize = Convert.ToInt32(TestContext.DataRow["sectorsize"]);
            var freespace = Convert.ToInt32(TestContext.DataRow["freespace"]);

            using (var dos = DiskFilesystemFactory.OpenFilesystem(ParseFilesystemID(filesystem), new MemoryDisk(heads, tracks, sectors, sectorsize), true))
            {
                dos.Initialize();
                dos.Check();
                Assert.AreEqual(freespace, dos.Free());
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
