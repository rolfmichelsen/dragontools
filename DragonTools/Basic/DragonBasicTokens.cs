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

using System.Collections;
using System.Collections.Generic;


namespace RolfMichelsen.Dragon.DragonTools.Basic
{
    /// <summary>
    /// Tokens for standard Dragon BASIC.
    /// </summary>
    /// <remarks>
    /// The Dragon BASIC tokens are documented in "Inside the Dragon" by Duncan Smeed and Ian Sommerville
    /// (Addison-Wesley, 1983).  See Appendix 7.2.
    /// </remarks>
    internal sealed class DragonBasicTokens : IBasicTokens
    {
        private static readonly BasicToken[] TokenList =
            {
                new BasicToken(0x80, "FOR"),
                new BasicToken(0x81, "GO"),
                new BasicToken(0x82, "REM"), 
                new BasicToken(0x83, "\'"), 
                new BasicToken(0x84, "ELSE"), 
                new BasicToken(0x85, "IF"), 
                new BasicToken(0x86, "DATA"), 
                new BasicToken(0x87, "PRINT"), 
                new BasicToken(0x88, "ON"), 
                new BasicToken(0x89, "INPUT"), 
                new BasicToken(0x8a, "END"), 
                new BasicToken(0x8b, "NEXT"), 
                new BasicToken(0x8c, "DIM"), 
                new BasicToken(0x8d, "READ"), 
                new BasicToken(0x8e, "LET"), 
                new BasicToken(0x8f, "RUN"), 
                new BasicToken(0x90, "RESTORE"), 
                new BasicToken(0x91, "RETURN"), 
                new BasicToken(0x92, "STOP"), 
                new BasicToken(0x93, "POKE"), 
                new BasicToken(0x94, "CONT"), 
                new BasicToken(0x95, "LIST"), 
                new BasicToken(0x96, "CLEAR"), 
                new BasicToken(0x97, "NEW"), 
                new BasicToken(0x98, "DEF"), 
                new BasicToken(0x99, "CLOAD"), 
                new BasicToken(0x9a, "CSAVE"), 
                new BasicToken(0x9b, "OPEN"), 
                new BasicToken(0x9c, "CLOSE"), 
                new BasicToken(0x9d, "LLIST"), 
                new BasicToken(0x9e, "SET"), 
                new BasicToken(0x9f, "RESET"), 
                new BasicToken(0xa0, "CLS"), 
                new BasicToken(0xa1, "MOTOR"), 
                new BasicToken(0xa2, "SOUND"), 
                new BasicToken(0xa3, "AUDIO"), 
                new BasicToken(0xa4, "EXEC"), 
                new BasicToken(0xa5, "SKIPF"), 
                new BasicToken(0xa6, "DELETE"), 
                new BasicToken(0xa7, "EDIT"), 
                new BasicToken(0xa8, "TRON"), 
                new BasicToken(0xa9, "TROFF"), 
                new BasicToken(0xaa, "LINE"), 
                new BasicToken(0xab, "PCLS"), 
                new BasicToken(0xac, "PSET"), 
                new BasicToken(0xad, "PRESET"), 
                new BasicToken(0xae, "SCREEN"), 
                new BasicToken(0xaf, "PCLEAR"), 
                new BasicToken(0xb0, "COLOR"), 
                new BasicToken(0xb1, "CIRCLE"), 
                new BasicToken(0xb2, "PAINT"), 
                new BasicToken(0xb3, "GET"), 
                new BasicToken(0xb4, "PUT"), 
                new BasicToken(0xb5, "DRAW"), 
                new BasicToken(0xb6, "PCOPY"), 
                new BasicToken(0xb7, "PMODE"), 
                new BasicToken(0xb8, "PLAY"), 
                new BasicToken(0xb9, "DLOAD"), 
                new BasicToken(0xba, "RENUM"), 
                new BasicToken(0xbb, "TAB("), 
                new BasicToken(0xbc, "TO"), 
                new BasicToken(0xbd, "SUB"), 
                new BasicToken(0xbe, "FN"), 
                new BasicToken(0xbf, "THEN"), 
                new BasicToken(0xc0, "NOT"), 
                new BasicToken(0xc1, "STEP"), 
                new BasicToken(0xc2, "OFF"), 
                new BasicToken(0xc3, "+"), 
                new BasicToken(0xc4, "-"), 
                new BasicToken(0xc5, "*"), 
                new BasicToken(0xc6, "/"), 
                new BasicToken(0xc7, "^"),
                new BasicToken(0xc8, "AND"), 
                new BasicToken(0xc9, "OR"), 
                new BasicToken(0xca, ">"), 
                new BasicToken(0xcb, "="), 
                new BasicToken(0xcc, "<"), 
                new BasicToken(0xcd, "USING"), 
                new BasicToken(0xff80, "SGN"), 
                new BasicToken(0xff81, "INT"), 
                new BasicToken(0xff82, "ABS"), 
                new BasicToken(0xff83, "POS"), 
                new BasicToken(0xff84, "RND"), 
                new BasicToken(0xff85, "SQR"), 
                new BasicToken(0xff86, "LOG"), 
                new BasicToken(0xff87, "EXP"), 
                new BasicToken(0xff88, "SIN"), 
                new BasicToken(0xff89, "COS"), 
                new BasicToken(0xff8a, "TAN"), 
                new BasicToken(0xff8b, "ATN"), 
                new BasicToken(0xff8c, "PEEK"), 
                new BasicToken(0xff8d, "LEN"), 
                new BasicToken(0xff8e, "STR$"), 
                new BasicToken(0xff8f, "VAL"), 
                new BasicToken(0xff90, "ASC"), 
                new BasicToken(0xff91, "CHR$"), 
                new BasicToken(0xff92, "EOF"), 
                new BasicToken(0xff93, "JOYSTK"), 
                new BasicToken(0xff94, "FIX"), 
                new BasicToken(0xff95, "HEX$"), 
                new BasicToken(0xff96, "LEFT$"), 
                new BasicToken(0xff97, "RIGHT$"), 
                new BasicToken(0xff98, "MID$"), 
                new BasicToken(0xff99, "POINT"), 
                new BasicToken(0xff9a, "INKEY$"), 
                new BasicToken(0xff9b, "MEM"), 
                new BasicToken(0xff9c, "VARPTR"), 
                new BasicToken(0xff9d, "INSTR"), 
                new BasicToken(0xff9e, "TIMER"), 
                new BasicToken(0xff9f, "PPOINT"), 
                new BasicToken(0xffa0, "STRING$"), 
                new BasicToken(0xffa1, "USR")
            };




        public IEnumerator<BasicToken> GetEnumerator()
        {
            return (IEnumerator<BasicToken>) ((IEnumerable<BasicToken>) TokenList).GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
