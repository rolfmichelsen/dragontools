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
    /// Write data to a CAS virtual tape.
    /// </summary>
    public sealed class CasWriter : ITapeWriter
    {
        private bool IsDisposed = false;

        private Stream stream;

        private byte buffer;

        private int inbuffer = 0;

        
        /// <summary>
        /// Create a reader for CAS virtual tapes and associate it with a stream for writing the CAS file.
        /// </summary>
        /// <param name="stream"></param>
        public CasWriter(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (!stream.CanWrite) throw new ArgumentException("Stream is not writeable");
            this.stream = stream;
        }


        /// <summary>
        /// Write a single bit to the virtual tape.
        /// </summary>
        /// <param name="value">Bit to write to tape.</param>
        public void WriteBit(bool value)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);

            buffer <<= 1;
            if (value) buffer |= 1;
            inbuffer++;
            if (inbuffer == 8) Flush();
        }


        /// <summary>
        /// Write a byte to the virtual streaming medium.
        /// The most significant bit is written first.
        /// </summary>
        /// <param name="value">Byte to write to tape.</param>
        public void WriteByte(byte value)
        {
            byte mask = 0x80;
            for (int i = 0; i < 8; i++ )
            {
                WriteBit((value & mask) != 0);
                mask >>= 1;
            }
        }


        /// <summary>
        /// Inserts a gap or blank section on the virtual tape.
        /// </summary>
        /// <remarks>
        /// The CAS format does not support blank (silent) sections so this method does nothing.
        /// </remarks>
        /// <param name="length">Approximate length of gap (in milliseconds).</param>
        public void InsertGap(int length)
        {
        }


        /// <summary>
        /// Rewind the tape to its beginning.  The stream is flushed before rewinding it.
        /// </summary>
        public void Rewind()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            Flush();
            stream.Seek(0, SeekOrigin.Begin);
        }


        /// <summary>
        /// Flushes any written data to the stream and then closes the stream.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                Flush();
                stream.Dispose();
                stream = null;
            }
        }


        /// <summary>
        /// Flushes any unwritten bits to the output stream.  Since the underlying stream can only deal with
        /// complete bytes, the output will be padded with extra bits if necessary.  Hence, this method can
        /// only be called when it is safe to add extra bits without corrupting the filesystem.
        /// </summary>
        private void Flush()
        {
            if (inbuffer > 0)
            {
                buffer <<= (8 - inbuffer);
                stream.WriteByte(buffer);
                buffer = 0;
                inbuffer = 0;
            }
        }
    }
}
