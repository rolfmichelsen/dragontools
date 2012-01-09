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
using System.IO;

namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Read data from a CAS virtual tape.
    /// </summary>
    public sealed class CasReader : ITapeReader
    {

        private Stream stream;

        private bool IsDisposed = false;

        private byte buffer;

        private int bufferinx = 8;


        /// <summary>
        /// Create a tape reader for CAS virtual tapes and associate it with a stream for reading a CAS file.
        /// </summary>
        /// <param name="stream"></param>
        public CasReader(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!stream.CanRead) throw new ArgumentException("Passed stream does not support reading");
            this.stream = stream;
        }


        /// <summary>
        /// Read a single bit from the virtual tape and return it.
        /// </summary>
        /// <returns>Bit read from tape.</returns>
        /// <exception cref="EndOfTapeException">Thrown if attempting to read beyond the end of the tape.</exception>
        public bool ReadBit()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (bufferinx == 8)
            {
                int value = stream.ReadByte();
                if (value == -1) throw new EndOfTapeException();
                buffer = (byte) value;
                bufferinx = 0;
            }

            bool ret = ((buffer & 0x80) != 0);
            buffer <<= 1;
            bufferinx++;

            return ret;
        }


        /// <summary>
        /// Read a byte from the virtual stream and return it.
        /// </summary>
        /// <returns>Byte read from tape.</returns>
        /// <exception cref="EndOfTapeException">Trying to read past the end of the tape.</exception>
        public byte ReadByte()
        {
            int value = 0;
            for (int i = 0; i < 8; i++)
            {
                value = (value << 1) | (ReadBit() ? 1 : 0);
            }
            return (byte)value;
        }


        /// <summary>
        /// Rewinds the tape to its beginning.
        /// </summary>
        public void Rewind()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            stream.Seek(0, SeekOrigin.Begin);
            bufferinx = 8;
        }


        /// <summary>
        /// Disposes of this object, including the stream used for reading tape data.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                stream.Dispose();
                stream = null;
            }
        }
    }
}
