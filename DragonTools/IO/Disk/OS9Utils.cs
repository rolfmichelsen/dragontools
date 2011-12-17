/*
Copyright (c) 2011, Rolf Michelsen
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


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    public sealed class OS9Utils
    {
        /// <summary>
        /// Parse a string encoded in a byte buffer.  OS-9 strings are encoded
        /// with the sign bit set for the last character in the string.
        /// </summary>
        /// <param name="raw">Buffer containing the string.</param>
        /// <param name="offset">Offset into buffer of first character of the string.</param>
        /// <returns>String.</returns>
        public static string ParseString(byte[] raw, int offset)
        {
            int len = 0;
            while ((raw[offset+len] & 0x80) == 0)
            {
                len++;
            }
            len++;

            var tmp = new char[len];
            for (int i = 0; i < len; i++)
                tmp[i] = (char) (raw[offset + i] & 0x7f);

            return new string(tmp);
        }


        /// <summary>
        /// Convert an OS9FileAttributes object to a compact string representation.
        /// </summary>
        /// <param name="attributes">File attribute.</param>
        /// <returns>String representation.</returns>
        public static string AttributeToString(OS9FileAttributes attributes)
        {
            var attrs = new char[8];
            attrs[0] = (attributes & OS9FileAttributes.Directory) != 0 ? 'd' : '-';
            attrs[1] = (attributes & OS9FileAttributes.Shared) != 0 ? 's' : '-';
            attrs[2] = (attributes & OS9FileAttributes.PublicExecute) != 0 ? 'x' : '-';
            attrs[3] = (attributes & OS9FileAttributes.PublicWrite) != 0 ? 'w' : '-';
            attrs[4] = (attributes & OS9FileAttributes.PublicRead) != 0 ? 'r' : '-';
            attrs[5] = (attributes & OS9FileAttributes.Execute) != 0 ? 'x' : '-';
            attrs[6] = (attributes & OS9FileAttributes.Write) != 0 ? 'w' : '-';
            attrs[7] = (attributes & OS9FileAttributes.Read) != 0 ? 'r' : '-';
            return new string(attrs);
        }
    }
}
