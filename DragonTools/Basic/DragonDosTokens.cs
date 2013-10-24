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

using System.Collections;
using System.Collections.Generic;

namespace RolfMichelsen.Dragon.DragonTools.Basic
{
    /// <summary>
    /// Tokens for additional DragonDos BASIC.
    /// </summary>
    /// <remarks>
    /// The DragonDos BASIC tokens are documented in "Inside the Dragon" by Duncan Smeed and Ian Sommerville
    /// (Addison-Wesley, 1983).  See Appendix 8.2.
    /// </remarks>
    internal sealed class DragonDosTokens : IBasicTokens
    {
        public IEnumerator<BasicToken> GetEnumerator()
        {
            return (IEnumerator<BasicToken>)((IEnumerable<BasicToken>)TokenList).GetEnumerator();
        }



        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private static readonly BasicToken[] TokenList =
            {
                new BasicToken(0xce, "AUTO"),
                new BasicToken(0xcf, "BACKUP"), 
                new BasicToken(0xd0, "BEEP"), 
                new BasicToken(0xd1, "BOOT"), 
                new BasicToken(0xd2, "CHAIN"), 
                new BasicToken(0xd3, "COPY"), 
                new BasicToken(0xd4, "CREATE"), 
                new BasicToken(0xd5, "DIR"), 
                new BasicToken(0xd6, "DRIVE"), 
                new BasicToken(0xd7, "DSKINIT"), 
                new BasicToken(0xd8, "FREAD"), 
                new BasicToken(0xd9, "FWRITE"), 
                new BasicToken(0xda, "ERROR"), 
                new BasicToken(0xdb, "KILL"), 
                new BasicToken(0xdc, "LOAD"), 
                new BasicToken(0xdd, "MERGE"), 
                new BasicToken(0xde, "PROTECT"), 
                new BasicToken(0xdf, "WAIT"), 
                new BasicToken(0xe0, "RENAME"), 
                new BasicToken(0xe1, "SAVE"), 
                new BasicToken(0xe2, "SREAD"), 
                new BasicToken(0xe3, "SWRITE"), 
                new BasicToken(0xe4, "VERIFY"), 
                new BasicToken(0xe5, "FROM"), 
                new BasicToken(0xe6, "FLREAD"), 
                new BasicToken(0xe7, "SWAP"), 
                new BasicToken(0xffa2, "LOF"), 
                new BasicToken(0xffa3, "FREE"), 
                new BasicToken(0xffa4, "ERL"), 
                new BasicToken(0xffa5, "ERR"), 
                new BasicToken(0xffa6, "HIMEM"), 
                new BasicToken(0xffa7, "LOC"), 
                new BasicToken(0xffa8, "FRE$"), 
            };
    }
}
