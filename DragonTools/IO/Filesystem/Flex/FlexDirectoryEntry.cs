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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.Flex
{
    /// <summary>
    /// Represents an entry in the FLEX disk directory structure.
    /// </summary>
    internal class FlexDirectoryEntry
    {
        public string Filename { get; private set; }
        public DateTime CreateTime { get; private set; }
        public bool IsRandomAccess { get; private set; }
        public int StartTrack { get; private set; }
        public int StartSector { get; private set; }
        public int EndTrack { get; private set; }
        public int EndSector { get; private set; }
        public int Sectors { get; private set; }

        public bool IsValid { get; private set; }


        public FlexDirectoryEntry(byte[] raw, int offset)
        {
            if (raw[offset] == 0)
            {
                IsValid = false;
                return;
            }

            IsValid = true;
            Filename = DecodeFilename(raw, offset);
            StartTrack = raw[13 + offset];
            StartSector = raw[14 + offset];
            EndTrack = raw[15 + offset];
            EndSector = raw[16 + offset];
            Sectors = (raw[17 + offset] << 8) | raw[18 + offset];
            try
            {
                CreateTime = new DateTime(raw[23 + offset] + 1900, raw[21 + offset], raw[22 + offset]); 
            }
            catch (ArgumentException e)
            {
                CreateTime = new DateTime(1980, 1, 1);
            }
            IsRandomAccess = (raw[19 + offset] != 0);
        }



        private static string DecodeFilename(byte[] raw, int offset)
        {
            var tmp = new char[12];
            int len = 0;
            for (int i=0; i<8 && raw[offset+i] != 0; i++)
            {
                tmp[len++] = (char) raw[offset + i];
            }
            tmp[len++] = '.';
            for (int i=0; i<3 && raw[offset+8+i] != 0; i++)
            {
                tmp[len++] = (char) raw[offset + 8 + i];
            }
            return new string(tmp, 0, len);
        }
    }
}
