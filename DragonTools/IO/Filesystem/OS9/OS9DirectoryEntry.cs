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
