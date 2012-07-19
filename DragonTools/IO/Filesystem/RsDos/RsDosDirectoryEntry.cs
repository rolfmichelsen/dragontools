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
