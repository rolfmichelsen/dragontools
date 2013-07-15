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

using System;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace RolfMichelsen.Dragon.DragonTools.acceptance
{
    
    /// <summary>
    /// Tests all common functionality for the Disk interface.
    /// </summary>
    [TestClass()]
    public class Disk
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

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Disk.xml", "DiskGeometry", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\Disk.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void DiskGeometry()
        {
            var filename = Convert.ToString(TestContext.DataRow["file"]);
            var classtype = Convert.ToString(TestContext.DataRow["class"]);
            var heads = Convert.ToInt32(TestContext.DataRow["heads"]);
            var tracks = Convert.ToInt32(TestContext.DataRow["tracks"]);
            var sectors = Convert.ToInt32(TestContext.DataRow["sectors"]);
            var sectorsize = Convert.ToInt32(TestContext.DataRow["sectorsize"]);

            using (var disk = DiskFactory.OpenDisk(filename, false))
            {
                Assert.AreEqual(classtype, disk.GetType().FullName);
                Assert.AreEqual(heads, disk.Heads);
                Assert.AreEqual(tracks, disk.Tracks);
                Assert.AreEqual(sectors, disk.Sectors);
                Assert.AreEqual(sectorsize, disk.SectorSize);
            }

        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Disk.xml", "ReadSector", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\Disk.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void ReadSector()
        {
            var filename = Convert.ToString(TestContext.DataRow["file"]);
            var head = Convert.ToInt32(TestContext.DataRow["head"]);
            var track = Convert.ToInt32(TestContext.DataRow["track"]);
            var sector = Convert.ToInt32(TestContext.DataRow["sector"]);
            var expectedSectorDataRaw =
                Convert.ToString(TestContext.DataRow["data"])
                       .Split(new char[] {' ', '\t', '\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

            var expectedSectorData = new byte[expectedSectorDataRaw.Length];
            for (int i = 0; i < expectedSectorDataRaw.Length; i++)
            {
                expectedSectorData[i] = Convert.ToByte(expectedSectorDataRaw[i], 16);
            }

            using (var disk = DiskFactory.OpenDisk(filename, false))
            {
                var sectorData = disk.ReadSector(head, track, sector);
                for (int i = 0; i < expectedSectorData.Length; i++)
                    Assert.AreEqual(expectedSectorData[i], sectorData[i]);
            }
        }
    }
}
