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

using RolfMichelsen.Dragon.DragonTools.IO.Tape;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RolfMichelsen.Dragon.DragonTools.acceptance
{
    
    [TestClass()]
    public class DragonTapeBlockTest
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


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DragonTapeBlockTest.xml", "Sync", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DragonTapeBlockTest.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void Sync()
        {
            var tapedataraw = Convert.ToString(TestContext.DataRow["TapeData"]).Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var minLeaderLength = Convert.ToInt32(TestContext.DataRow["MinLeaderLength"]);
            var nextbyte = (byte) StringToInt(Convert.ToString(TestContext.DataRow["NextByte"]));
            var exception = Convert.ToString(TestContext.DataRow["Exception"]);

            var tapedata = new byte[tapedataraw.Length];
            for (int i = 0; i < tapedataraw.Length; i++)
                tapedata[i] = (byte) StringToInt(tapedataraw[i]);

            var reader = new CasReader(new System.IO.MemoryStream(tapedata));

            try
            {
                DragonTapeBlock.Sync(reader, minLeaderLength);
                Assert.IsTrue(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
                Assert.AreEqual(nextbyte, reader.ReadByte());
            }
            catch (Exception e)
            {
                if (!String.Equals(exception, e.GetType().FullName))
                    throw e;
            }
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DragonTapeBlockTest.xml", "ReadBlock", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DragonTapeBlockTest.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void ReadBlockAndValidate()
        {
            var tapedataraw = Convert.ToString(TestContext.DataRow["TapeData"]).Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var blockclass = Convert.ToString(TestContext.DataRow["BlockClass"]);
            var blocktype = StringToInt(Convert.ToString(TestContext.DataRow["BlockType"]));
            var datalength = Convert.ToInt32(TestContext.DataRow["DataLength"]);
            var checksum = StringToInt(Convert.ToString(TestContext.DataRow["Checksum"]));
            var exception = Convert.ToString(TestContext.DataRow["Exception"]);

            var tapedata = new byte[tapedataraw.Length];
            for (int i = 0; i < tapedataraw.Length; i++)
                tapedata[i] = (byte)StringToInt(tapedataraw[i]);
            var reader = new CasReader(new System.IO.MemoryStream(tapedata));

            DragonTapeBlock block = DragonTapeBlock.ReadBlock(reader);

            Assert.AreEqual(blockclass, block.GetType().FullName);
            Assert.AreEqual(blocktype, (int)block.BlockType);
            Assert.AreEqual(datalength, block.Length);
            Assert.AreEqual(checksum, block.Checksum);
            Assert.AreEqual(0x55, reader.ReadByte());

            try
            {
                block.Validate();
            }
            catch (Exception e)
            {
                if (!String.Equals(exception, e.GetType().FullName))
                    throw e;
                return;                
            }

            Assert.IsTrue(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
        }



        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DragonTapeBlockTest.xml", "HeaderBlock", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DragonTapeBlockTest.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void HeaderBlock()
        {
            var blockdataraw = Convert.ToString(TestContext.DataRow["BlockData"]).Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var checksum = StringToInt(Convert.ToString(TestContext.DataRow["BlockChecksum"]));
            var filename = Convert.ToString(TestContext.DataRow["FileName"]).Trim();
            var filetype = Convert.ToInt32(TestContext.DataRow["FileType"]);
            var isascii = Convert.ToBoolean(TestContext.DataRow["AsciiFlag"]);
            var isgapped = Convert.ToBoolean(TestContext.DataRow["GapFlag"]);
            var loadaddress = StringToInt(Convert.ToString(TestContext.DataRow["LoadAddress"]));
            var execaddress = StringToInt(Convert.ToString(TestContext.DataRow["ExecAddress"]));
            var exception = Convert.ToString(TestContext.DataRow["Exception"]);

            var blockdata = new byte[blockdataraw.Length];
            for (int i = 0; i < blockdataraw.Length; i++)
                blockdata[i] = (byte)StringToInt(blockdataraw[i]);

            var block = new DragonTapeHeaderBlock(blockdata, checksum);

            Assert.AreEqual(filename, block.Filename);
            Assert.AreEqual(filetype, (int) block.FileType);
            Assert.AreEqual(isascii, block.IsAscii);
            Assert.AreEqual(isgapped, block.IsGapped);
            Assert.AreEqual(loadaddress, block.LoadAddress);
            Assert.AreEqual(execaddress, block.ExecAddress);

            try
            {
                block.Validate();
            }
            catch (Exception e)
            {
                if (!String.Equals(exception, e.GetType().FullName))
                    throw e;
                return;                
            }

            Assert.IsTrue(String.IsNullOrWhiteSpace(exception), "Expected exception " + exception);
        }



        [TestMethod()]
        public void WriteBlock()
        {
            Assert.Inconclusive();
        }


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\DragonTapeBlockTest.xml", "WriteHeaderBlock", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\DragonTapeBlockTest.xml")]
        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void WriteHeaderBlock()
        {
            var filename = Convert.ToString(TestContext.DataRow["FileName"]).Trim();
            var filetype = Convert.ToInt32(TestContext.DataRow["FileType"]);
            var isascii = Convert.ToBoolean(TestContext.DataRow["AsciiFlag"]);
            var isgapped = Convert.ToBoolean(TestContext.DataRow["GapFlag"]);
            var loadaddress = StringToInt(Convert.ToString(TestContext.DataRow["LoadAddress"]));
            var execaddress = StringToInt(Convert.ToString(TestContext.DataRow["ExecAddress"]));
            var sync = Convert.ToBoolean(TestContext.DataRow["IsSynchronized"]);
            var blockdataraw = Convert.ToString(TestContext.DataRow["BlockData"]).Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var blockdata = new byte[blockdataraw.Length];
            for (int i = 0; i < blockdataraw.Length; i++)
                blockdata[i] = (byte) StringToInt(blockdataraw[i]);

            var block = new DragonTapeHeaderBlock(filename, (DragonTapeFileType) filetype, isascii, isgapped, loadaddress, execaddress);
            var stream = new System.IO.MemoryStream();
            var writer = new CasWriter(stream);
            block.WriteBlock(writer, sync);

            var encodedblock = stream.GetBuffer();
            for (int i = 0; i < blockdata.Length; i++ )
                Assert.AreEqual(blockdata[i], encodedblock[i], "Encoded block unexpected value at index {0}", i);

            writer.Dispose();
        }


        int StringToInt(string value)
        {
            if (value.Equals("0")) return 0;
            if (value.StartsWith("0x")) return Convert.ToInt32(value.Substring(2), 16);
            if (value.StartsWith("0")) return Convert.ToInt32(value.Substring(1), 8);
            return Convert.ToInt32(value);
        }

    }
}
