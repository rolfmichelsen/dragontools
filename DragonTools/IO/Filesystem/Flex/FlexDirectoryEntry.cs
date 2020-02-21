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
            catch (ArgumentException)
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
