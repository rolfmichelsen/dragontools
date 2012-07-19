/*
Copyright (c) 2011-2012, Rolf Michelsen
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


using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DragonTools.unit
{
    [TestClass()]
    public class DragonTapeDataBlockTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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


        [TestMethod()]
        public void CreateDragonTapeDataBlock()
        {
            var payload = new byte[] {0x10, 0x20, 0x30, 0x01, 0x02, 0x03, 0x55, 0xaa};
            var block = new DragonTapeDataBlock(payload);
            
            Assert.AreEqual(DragonTapeBlockType.Data, block.BlockType);
            Assert.AreEqual(payload.Length, block.Length);
            var data = block.Data;
            for (int i = 0; i < data.Length; i++ ) Assert.AreEqual(payload[i], data[i]);
            Assert.AreEqual(0x6e, block.Checksum);   
            block.Validate();
        }


        [TestMethod]
        public void CreateDragonTapeDataBlock_Empty()
        {
            var block = new DragonTapeDataBlock(null);
            Assert.AreEqual(DragonTapeBlockType.Data, block.BlockType);
            Assert.AreEqual(0, block.Length);
            Assert.AreEqual(null, block.Data);
            Assert.AreEqual(1, block.Checksum);
            block.Validate();
        }



        [TestMethod]
        public void CreateDragonTapeDataBlock_PayloadTooLarge_ThrowsException()
        {
            var payload = new byte[256];
            DragonTapeBlock block = null;
            try
            {
                block = new DragonTapeDataBlock(payload);
                Assert.Fail("Block with too large payload incorrectly created.");
            }
            catch (ArgumentOutOfRangeException e) {}
        }

        
    }
}
