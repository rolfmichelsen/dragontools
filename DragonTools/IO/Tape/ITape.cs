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
