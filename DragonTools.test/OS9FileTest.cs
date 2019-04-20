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

using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace RolfMichelsen.Dragon.DragonTools.test
{
    
    
    [TestClass()]
    public class OS9FileTest
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


        [TestMethod()]
        public void CreateFileTest()
        {
            var fileinfo = new OS9FileInfo("list", 0x4f, OS9FileAttributes.Execute|OS9FileAttributes.Read, null, null, 0, 0, null);
            byte[] data = {
                              0x87, 0xcd, 0x00, 0x4f, 0x00, 0x0d, 0x11, 0x81,
                              0x67, 0x00, 0x12, 0x02, 0x8d, 0x4c, 0x69, 0x73,
                              0xf4, 0x05, 0x9f, 0x01, 0x86, 0x01, 0x10, 0x3f,
                              0x84, 0x25, 0x2e, 0x97, 0x00, 0x9f, 0x01, 0x96,
                              0x00, 0x30, 0x43, 0x10, 0x8e, 0x00, 0xc8, 0x10,
                              0x3f, 0x8b, 0x25, 0x09, 0x86, 0x01, 0x10, 0x3f,
                              0x8c, 0x24, 0xec, 0x20, 0x14, 0xc1, 0xd3, 0x26,
                              0x10, 0x96, 0x00, 0x10, 0x3f, 0x8f, 0x25, 0x09,
                              0x9e, 0x01, 0xa6, 0x84, 0x81, 0x0d, 0x26, 0xca,
                              0x5f, 0x10, 0x3f, 0x06, 0x58, 0xbc, 0x12
                          };

            var file = (OS9File) OS9File.CreateFile(fileinfo, data);
            Assert.IsTrue(file is OS9ModuleFile);
            
            var module = (OS9ModuleFile) file;
            Assert.AreEqual("List", module.ModuleName);
            Assert.AreEqual(OS9ModuleType.Program, module.ModuleType);
            Assert.AreEqual(1, module.ModuleLanguage);
            Assert.AreEqual(8, module.ModuleAttributes);
            Assert.AreEqual(1, module.ModuleRevision);
            Assert.AreEqual(0x67, module.HeaderParity);
            Assert.AreEqual(0x58bc12, module.ModuleCRC);
        }
    }
}
