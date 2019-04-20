/*
Copyright (c) 2011-2013, Rolf Michelsen
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
    /// <summary>
    /// Calculates the CCITT-CRC using the polynomial x^16 + x^12 + x^5 +1 (0x1021) as
    /// implemented in the WD279X floppy disk controller.
    /// </summary>
    public sealed class Crc16Ccitt
    {
        private uint crc = 0xffff;

        private readonly uint polynomial = 0x1021;


        /// <summary>
        /// Return the current valuue of the CRC register.
        /// </summary>
        public uint Crc { get { return crc & 0xffff; } }


        /// <summary>
        /// Add a single data byte to the CRC.
        /// </summary>
        /// <param name="b">Byte to add.</param>
        /// <returns>The new value of the CRC register.</returns>
        public uint Add(byte b)
        {
            crc ^= ((uint) b << 8);
            for (int j = 0; j < 8; j++)
            {
                crc <<= 1;
                if ((crc & 0x10000) != 0) crc ^= polynomial;
            }
            return Crc;
        }


        /// <summary>
        /// Add an array of data bytes to the CRC.
        /// </summary>
        /// <param name="data">Array of data bytes.</param>
        /// <param name="offset">Offset of first data byte to add.</param>
        /// <param name="length">Number of data bytes to add.</param>
        /// <returns>The new value of the CRC register.</returns>
        public uint Add(byte[] data, int offset, int length)
        {
            while (length-- > 0)
                Add(data[offset++]);
            return Crc;
        }


        /// <summary>
        /// Add an array of data bytes to the CRC.
        /// </summary>
        /// <param name="data">Array of data bytes.</param>
        /// <returns>The new value of the CRC register.</returns>
        public uint Add(byte[] data)
        {
            return Add(data, 0, data.Length);
        }
    }
}
