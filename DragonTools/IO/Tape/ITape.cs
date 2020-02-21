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



namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Abstract representation of a virtual tape.  Subclasses provide functionality for reading
    /// and writing various virtual tape formats.  A tape basically supports reading and writing 
    /// of bits.
    /// </summary>
    public interface ITape
    {
        /// <summary>
        /// True if this tape supports read operations.
        /// </summary>
        bool IsReadable { get; }

        /// <summary>
        /// True if this tape supports write operations.
        /// </summary>
        bool IsWriteable { get; }
        
        /// <summary>
        /// Read a single bit from the virtual stream and return it.
        /// </summary>
        /// <returns>Bit read from tape.</returns>
        /// <exception cref="EndOfTapeException">Trying to read past the end of the tape.</exception>
        bool ReadBit();

        /// <summary>
        /// Read a byte from the virtual stream and return it.
        /// </summary>
        /// <returns>Byte read from tape.</returns>
        /// <exception cref="EndOfTapeException">Trying to read past the end of the tape.</exception>
        byte ReadByte();

        /// <summary>
        /// Rewinds the stream to its beginning.
        /// </summary>
        void Rewind();

        /// <summary>
        /// Write a single bit to the virtual streaming medium.
        /// </summary>
        /// <param name="value">Bit to write to tape.</param>
        void WriteBit(bool value);

        /// <summary>
        /// Write a byte to the virtual streaming medium.
        /// </summary>
        /// <param name="value">Byte to write to tape.</param>
        void WriteByte(byte value);

        /// <summary>
        /// Inserts a gap or blank section on the virtual streaming medium.
        /// </summary>
        /// <param name="length">Approximate length of gap (in milliseconds).</param>
        void InsertGap(int length);
    }
}
