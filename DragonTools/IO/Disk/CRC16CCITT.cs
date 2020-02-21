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
