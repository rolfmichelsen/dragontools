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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.RsDos
{
    /// <summary>
    /// Represents an entry in the RSDOS disk directory structure.
    /// </summary>
    internal class RsDosDirectoryEntry
    {
        public string Filename { get; private set; }
        public int Filetype { get; private set; }
        public bool IsAscii { get; private set; }
        public int FirstGranule { get; private set; }
        public int LastSectorSize { get; private set; }

        public bool IsValid { get; private set; }
        public bool IsEndOfDirectory { get; private set; }

        public RsDosDirectoryEntry(byte[] raw, int offset)
        {
            if (raw == null) throw new ArgumentNullException("raw");

            Filename = null;
            IsValid = true;
            IsEndOfDirectory = false;
            
            if (raw[offset] == 0)
            {
                IsValid = false;
            }
            else if (raw[offset] == 0xff)
            {
                IsValid = false;
                IsEndOfDirectory = true;
            }
            else
            {
                var tmp = new char[11];
                for (int i = 0; i < tmp.Length; i++)
                {
                    tmp[i] = (char) raw[i + offset];
                }
                var basename = new string(tmp, 0, 8).TrimEnd(null);
                var extension = new string(tmp, 8, 3).TrimEnd(null);
                Filename = (extension.Length > 0) ? basename + "." + extension : basename;
            }

            Filetype = raw[11 + offset];
            IsAscii = (raw[12 + offset] != 0);
            FirstGranule = raw[13 + offset];
            LastSectorSize = (raw[14 + offset] << 8) | raw[15 + offset];

            //TODO How do we validate the entires?
        }
    }
}
