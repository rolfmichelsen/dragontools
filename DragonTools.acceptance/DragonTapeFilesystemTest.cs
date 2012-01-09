/*
Copyright (c) 2012, Rolf Michelsen
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

using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace RolfMichelsen.Dragon.DragonTools.acceptance
{
    
    
    [TestClass()]
    public class DragonTapeFilesystemTest
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
        public void IsValidFilenameTest()
        {
            var tape = new DragonTapeFilesystem(new CasWriter(new System.IO.MemoryStream()));

            Assert.AreEqual(true, tape.IsValidFilename("FOOBAR22"));


            Assert.Inconclusive("Test not yet implemented.");
        }
        

        [TestMethod()]
        public void WriteFileTest()
        {
            Assert.Inconclusive("Test not yet implemented");
        }


        [DeploymentItem("DragonLib.AcceptanceTest\\Testdata\\")]
        [TestMethod()]
        public void ReadFileTest()
        {
            var tape = new DragonTapeFilesystem(new CasReader(new System.IO.FileStream("Dancer.cas", FileMode.Open)));
            var file = tape.ReadFile();
            Assert.IsTrue(file is DragonMachineCodeFile);
            var mcfile = (DragonMachineCodeFile) file;
            Assert.AreEqual("DANCER", mcfile.Name);
            Assert.AreEqual(13635, mcfile.Length);
            Assert.AreEqual(3072, mcfile.LoadAddress);
            Assert.AreEqual(13824, mcfile.ExecAddress);
            Assert.IsTrue(mcfile.IsExecutable);
            // TODO Improve test by also verifying the file data
        }
    }
}
