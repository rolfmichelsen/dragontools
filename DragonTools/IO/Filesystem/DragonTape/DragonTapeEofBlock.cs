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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape
{
    /// <summary>
    /// A Dragon tape end of file block.
    /// </summary>
    public sealed class DragonTapeEofBlock : DragonTapeBlock
    {
        /// <summary>
        /// Create an end of file (EOF) tape block.
        /// </summary>
        public DragonTapeEofBlock() : base(DragonTapeBlockType.EndOfFile)
        {
            Checksum = ComputeChecksum();
        }



        internal DragonTapeEofBlock(byte[] data, int offset, int length, int checksum) : base(DragonTapeBlockType.EndOfFile)
        {
            SetData(data, offset, length);
            Checksum = checksum;
        }


        public override string ToString()
        {
            return String.Format("Block: Type={0} (EOF) Length={1} Checksum={2} ({3})", (int)BlockType, Length, Checksum, (IsChecksumValid() ? "Valid" : "Invalid"));
        }

    }

}
