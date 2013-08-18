/*
Copyright (c) 2011-2013, Rolf Michelsen
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
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.Basic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RolfMichelsen.Dragon.DragonTools.acceptance
{
    [TestClass]
    public class Basic
    {
        readonly String[] BasicProgramText = new string[]
            {
                @"100 'TEST PROGRAM",
                @"110 PRINT ""HELLO, WORLD!""",
                @"120 END"
             };

        readonly byte[] BasicProgramTokens = new byte[]
             {
                0x1E, 0x14, 0x00, 0x64,
                0x3A, 0x83, 0x54, 0x45,
                0x53, 0x54, 0x20, 0x50,
                0x52, 0x4F, 0x47, 0x52,
                0x41, 0x4D, 0x00, 0x1E,
                0x2A, 0x00, 0x6E, 0x87,
                0x20, 0x22, 0x48, 0x45,
                0x4C, 0x4C, 0x4F, 0x2C,
                0x20, 0x57, 0x4F, 0x52,
                0x4C, 0x44, 0x21, 0x22,
                0x00, 0x1E, 0x30, 0x00,
                0x78, 0x8A, 0x00, 0x00
            };

        
        [TestMethod]
        public void DecodeTokenizedBasicToString()
        {
            var basicDecoder = new BasicTokenizer(new DragonBasicTokens());
            var basicProgram = basicDecoder.Decode(BasicProgramTokens).Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

            Assert.AreEqual(BasicProgramText.Length, basicProgram.Length);
            for (int i = 0; i < BasicProgramText.Length; i++ )
                Assert.AreEqual(BasicProgramText[i], basicProgram[i]);
        }


        [TestMethod]
        public void DecodeTokenizedBasicToBytes()
        {
            var basicProgramExpected = ConvertStringsToBytes(BasicProgramText);
            var basicDecoder = new BasicTokenizer(new DragonBasicTokens());
            var basicProgram = basicDecoder.DecodeToBytes(BasicProgramTokens);

            Assert.AreEqual(basicProgramExpected.Length, basicProgram.Length);
            for (int i = 0; i < basicProgramExpected.Length; i++ )
                Assert.AreEqual(basicProgramExpected[i], basicProgram[i]);
        }


        byte[] ConvertStringsToBytes(String[] text)
        {
            var converterText = new List<byte>(5000);
            foreach (var line in text)
            {
                for (int i=0; i<line.Length; i++)
                    converterText.Add((byte) line[i]);
                converterText.Add((byte) '\r');
            }
            return converterText.ToArray();
        }

    }
}
