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

using System;

namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Read data from a virtual streaming medium, typically a tape.
    /// Concrete subclasses provide support for corresponding
    /// virtual streaming formats.
    /// </summary>
    public interface ITapeReader : IDisposable
    {
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
    }
}
