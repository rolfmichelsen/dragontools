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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    public sealed class OS9DirectoryEntry
    {
        public const int RawEntrySize = 32;

        public bool IsValid;

        public string Filename;

        public int Sector;

        public OS9DirectoryEntry(byte[] raw, int index)
        {
            int offset = index*RawEntrySize;
            if (raw[offset] == 0)
            {
                IsValid = false;
            }
            else
            {
                IsValid = true;
                Sector = (raw[offset + 29] << 16) | (raw[offset + 30] << 8) | raw[offset + 31];
                Filename = ParseFilename(raw, offset);
            }
        }


        public static string ParseFilename(byte[] raw, int offset)
        {
            var filename = new char[29];
            int len = 0;
            while (len < filename.Length && (raw[offset + len] & 0x80) == 0)
            {
                filename[len] = (char) raw[offset + len];
                len++;
            }
            if (len < filename.Length)
            {
                filename[len] = (char) (raw[offset + len] & 0x7f);
                len++;
            }
            return new string(filename, 0, len);
        }
    }
}
