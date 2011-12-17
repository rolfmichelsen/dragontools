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

    internal sealed class DragonDosFileHeader
    {
        public const int HeaderSize = 9;

        public FileType Type { get; set; }

        public int LoadAddress { get; set; }
        public int StartAddress { get; set; }
        public int Length { get; set; }

        public DragonDosFileHeader(FileType type)
        {
            Type = type;
        }


        public static DragonDosFileHeader Parse(byte[] raw, int offset)
        {
            if (raw.Length - offset < HeaderSize) return null;
            if (raw[offset] != 0x55 || raw[offset + 8] != 0xaa) return null;
            var type = (FileType) raw[offset + 1];
            var fileheader = new DragonDosFileHeader(type);
            fileheader.LoadAddress = (raw[offset + 2] << 8) | raw[offset + 3];
            fileheader.Length = (raw[offset + 4] << 8) | raw[offset + 5];
            fileheader.StartAddress = (raw[offset + 6] << 8) | raw[offset + 7];
            return fileheader;
        }
    
        public byte[] Encode()
        {
            var raw = new byte[HeaderSize];
            raw[0] = 0x55;
            raw[1] = (byte) Type;
            raw[2] = (byte) ((LoadAddress >> 8) & 0xff);
            raw[3] = (byte) (LoadAddress & 0xff);
            raw[4] = (byte) ((Length >> 8) & 0xff);
            raw[5] = (byte) (Length & 0xff);
            raw[6] = (byte) ((StartAddress >> 8) & 0xff);
            raw[7] = (byte) (StartAddress & 0xff);
            raw[8] = 0xaa;
            return raw;
        }


        public enum FileType
        {
            Basic = 1,
            Native = 2
        }

    }
}
