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


namespace RolfMichelsen.Dragon.DragonTools.Basic
{
    /// <summary>
    /// Decode a tokenized Dragon BASIC program.
    /// </summary>
    public sealed class DragonBasicTokenizer
    {
        private readonly Dictionary<int, string> Tokens = new Dictionary<int, string>();

        private static readonly byte EOL = 0;


        /// <summary>
        /// Create a Dragon BASIC tokenizer.
        /// </summary>
        /// <param name="dialect">Specify a variation of the standard Dragon BASIC.</param>
        public DragonBasicTokenizer(DragonBasicDialect dialect = DragonBasicDialect.None) 
        {
            Add(new DragonBasicTokens());
            if ((dialect & DragonBasicDialect.DragonDos) != 0)
                Add(new DragonDosTokens());
        }


        /// <summary>
        /// Add a set of BASIC tokens to the tokenizer.
        /// </summary>
        /// <param name="tokens">Collection of BASIC tokens.</param>
        private void Add(IEnumerable<BasicToken> tokens)
        {
            foreach (var token in tokens)
            {
                Tokens[token.Id] = token.Token;
            }
        }


        /// <summary>
        /// Decode a tokenized BASIC program and returns its text representation.
        /// </summary>
        /// <param name="tokenizedBasic">Buffer containing the tokenized BASIC program.</param>
        /// <returns>Text representation of the BASIC program.</returns>
        /// <exception cref="BasicTokenizerException">Exception during decoding of BASIC tokens.</exception>
        public String Decode(IEnumerable<byte> tokenizedBasic)
        {
            var asciiBasic = DecodeToBytes(tokenizedBasic);
            var charBasic = new char[asciiBasic.Length];
            for (int i = 0; i < asciiBasic.Length; i++)
                charBasic[i] = Convert.ToChar(asciiBasic[i]);
            return new string(charBasic);
        }



        /// <summary>
        /// Decode a tokenized BASIC program and returns its text representation as a byte array.
        /// </summary>
        /// <param name="tokenizedBasic">Buffer containing the tokenized BASIC program.</param>
        /// <returns>Text representation of the BASIC program.</returns>
        /// <exception cref="BasicTokenizerException">Exception during decoding of BASIC tokens.</exception>
        public byte[] DecodeToBytes(IEnumerable<byte> tokenizedBasic)
        {
            if (tokenizedBasic == null)
                throw new ArgumentNullException("tokenizedBasic");

            var decoded = new List<byte>();
            
            using (var enumerator = ((IEnumerable<byte>) tokenizedBasic).GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new BasicTokenizerException();
                while (enumerator.Current != 0)
                {
                    DecodeLineToBytes(enumerator, decoded);
                }
            }

            return decoded.ToArray();
        }



        /// <summary>
        /// Decode one line from a tokenized BASIC program to its ASCII representation.
        /// The enumerator must point to the first byte of the tokenized BASIC string.  When the function returns it will point to
        /// the first byte following the EOL marker.
        /// </summary>
        /// <param name="tokenizedBasic">Enumerator over the tokenized BASIC program.</param>
        /// <param name="textBasic">Container for appending the BASIC program text.</param>
        /// <exception cref="BasicTokenizerException">Exception during decoding of BASIC tokens.</exception>
        private void DecodeLineToBytes(IEnumerator<byte> tokenizedBasic, List<byte> textBasic)
        {
            DecodeLink(tokenizedBasic);
            var basicLineNumber = DecodeLineNumber(tokenizedBasic);
            EncodeLineNumber(basicLineNumber, textBasic);
            textBasic.Add(Convert.ToByte(' '));
            DecodeBasicLineTokens(tokenizedBasic, textBasic);
            textBasic.Add(Convert.ToByte('\r'));
        }



        private void EncodeLineNumber(int basicLineNumber, ICollection<byte> textBasic)
        {
            var lineNumberString = Convert.ToString(basicLineNumber);
            foreach (char t in lineNumberString)
                textBasic.Add(Convert.ToByte(t));
        }



        /// <summary>
        /// Decode a single line of tokenized BASIC to its ASCII representation.
        /// The tokenized BASIC enumerator must be positioned at the first BASIC token, just after the BASIC line number.
        /// When done, the enumerator will point to the first byte following the EOL token.
        /// </summary>
        /// <param name="tokenizedBasic">Enumerator for reading the tokenized BASIC program.</param>
        /// <param name="textBasic">Collection for adding the ASCII representation of the BASIC program.</param>
        /// <exception cref="BasicTokenizerException">Something is wrong with the tokenized BASIC program.</exception>
        private void DecodeBasicLineTokens(IEnumerator<byte> tokenizedBasic, ICollection<byte> textBasic)
        {
            int token;
            while ((token = tokenizedBasic.Current) != EOL)
            {
                if ((token & 0x80) != 0)
                {
                    if (token == 0xff)
                    {
                        if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
                        token = (token << 8) | tokenizedBasic.Current;
                    }
                    if (Tokens.ContainsKey(token))
                    {
                        foreach (char b in Tokens[token])
                        {
                            textBasic.Add(Convert.ToByte(b));
                        }
                    }
                    else
                    {
                        throw new BasicTokenizerException();
                    }
                }
                else if (token == ':')   //TODO Kludge! Colon followed by ampersand is not represented in BASIC text
                {
                    if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
                    if (tokenizedBasic.Current != 0x83)
                        textBasic.Add(Convert.ToByte(token));
                    continue;
                }
                else
                {
                    textBasic.Add(Convert.ToByte(token));
                }

                if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();

            }

            if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
        }


        private int DecodeLineNumber(IEnumerator<byte> tokenizedBasic)
        {
            int lineHigh = tokenizedBasic.Current;
            if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
            int lineLow = tokenizedBasic.Current;
            if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
            return (lineHigh << 8) | lineLow;
        }



        private int DecodeLink(IEnumerator<byte> tokenizedBasic)
        {
            int linkHigh = tokenizedBasic.Current;
            if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
            int linkLow = tokenizedBasic.Current;
            if (!tokenizedBasic.MoveNext()) throw new BasicTokenizerException();
            return (linkHigh << 8) | linkLow;
        }

    }


    [Flags]
    public enum DragonBasicDialect
    {
        None = 0,
        DragonDos = 1
    }
}
