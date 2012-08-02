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

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.Flex;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.RsDos;

namespace RolfMichelsen.Dragon.DragonTools.acceptance
{
    [TestClass]
    public class FileName
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


        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\FileName.xml", "FileNameAccess", DataAccessMethod.Sequential)]
        [DeploymentItem("DragonLib.AcceptanceTest\\FileName.xml")]
        [TestMethod()]
        public void FileNameAccess()
        {
            var filesystem = Convert.ToString(TestContext.DataRow["filesystem"]);
            var pathname = Convert.ToString(TestContext.DataRow["pathname"]);
            var name = Convert.ToString(TestContext.DataRow["name"]);
            var basename = Convert.ToString(TestContext.DataRow["base"]);
            var extension = Convert.ToString(TestContext.DataRow["extension"]);

            IFileName filename = null;
            switch (filesystem.ToLower())
            {
                case "dragondos":
                    filename = DragonDos.GetFileName(pathname);
                    break;
                case "flex":
                    filename = Flex.GetFileName(pathname);
                    break;
                case "os9":
                    filename = OS9.GetFileName(pathname);
                    break;
                case "rsdos":
                    filename = RsDos.GetFileName(pathname);
                    break;
                default:
                    Assert.Fail("Unknown filesystem " + filesystem);
                    break;
            }

            Assert.AreEqual(name, filename.Name);
            Assert.AreEqual(basename, filename.Base);
            Assert.AreEqual(extension, filename.Extension);
            //TODO Add test for Ascend method
            //TODO Add test for Descend method
        }


        //TODO Add tests for IFileName.IsValid method
    }
}
