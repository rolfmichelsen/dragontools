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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
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
