﻿/*
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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape
{
    /// <summary>
    /// Represents a data tape block
    /// </summary>
    public sealed class DragonTapeDataBlock : DragonTapeBlock
    {

        /// <summary>
        /// Create a file data block.
        /// </summary>
        /// <param name="data">Memory area containing block data payload.</param>
        /// <param name="offset">Offset into memory area of first byte of block payload.</param>
        /// <param name="length">Length of block payload.</param>
        /// <exception cref="ArgumentOutOfRangeException">The payload size exceeds <see cref="MaxPayloadLength">MaxPayloadLength</see>.</exception>
        public DragonTapeDataBlock(byte[] data, int offset, int length) : base(DragonTapeBlockType.Data)
        {
            if (length > MaxPayloadLength) throw new ArgumentOutOfRangeException("length", length, String.Format("Block payload size cannot exceed {0} bytes", MaxPayloadLength));
            SetData(data, offset, length);
            Checksum = ComputeChecksum();
        }


        /// <summary>
        /// Create a file data block.
        /// </summary>
        /// <param name="data">Data block payload.</param>
        /// <exception cref="ArgumentOutOfRangeException">The payload size exceeds <see cref="MaxPayloadLength">MaxPayloadLength</see>.</exception>
        public DragonTapeDataBlock(byte[] data) : this(data, 0, (data == null) ? 0 : data.Length) { }


        /// <summary>
        /// Create a file data block.
        /// </summary>
        /// <param name="data">Memory area containing block data payload.</param>
        /// <param name="offset">Offset into memory area of first byte of block payload.</param>
        /// <param name="length">Length of block payload.</param>
        /// <param name="checksum">Block checksum.</param>
        internal DragonTapeDataBlock(byte[] data, int offset, int length, int checksum)
            : base(DragonTapeBlockType.Data)
        {
            SetData(data, offset, length);
            Checksum = checksum;
        }


        public override string ToString()
        {
            return String.Format("Block: Type={0} (Data) Length={1} Checksum={2} ({3})", (int)BlockType, Length, Checksum, (IsChecksumValid() ? "Valid" : "Invalid"));
        }
    }
}
