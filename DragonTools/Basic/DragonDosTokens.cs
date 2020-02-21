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
