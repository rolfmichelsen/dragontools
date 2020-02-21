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

using System;
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.Basic;
using Xunit;

namespace RolfMichelsen.Dragon.DragonTools.test
{
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

        
        [Fact]
        public void DecodeTokenizedBasicToString()
        {
            var basicDecoder = new DragonBasicTokenizer();
            var basicProgram = basicDecoder.Decode(BasicProgramTokens).Split(new char[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

            Assert.Equal(BasicProgramText.Length, basicProgram.Length);
            for (int i = 0; i < BasicProgramText.Length; i++ )
                Assert.Equal(BasicProgramText[i], basicProgram[i]);
        }


        [Fact]
        public void DecodeTokenizedBasicToBytes()
        {
            var basicProgramExpected = ConvertStringsToBytes(BasicProgramText);
            var basicDecoder = new DragonBasicTokenizer();
            var basicProgram = basicDecoder.DecodeToBytes(BasicProgramTokens);

            Assert.Equal(basicProgramExpected.Length, basicProgram.Length);
            for (int i = 0; i < basicProgramExpected.Length; i++ )
                Assert.Equal(basicProgramExpected[i], basicProgram[i]);
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
