/*
Copyright (c) 2011-2015, Rolf Michelsen
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


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Provides MFM encoding and decoding of a stream, including A1 sync detection and generation.  The codec also flips
    /// bytes. The byte 0x4e can be encoded as 0x9254.  The byte is flipped so that the data actually output is 0x492a.
    /// </summary>
    public sealed class MfmStream : Stream
    {
        /// <summary>
        /// Stream for accessing MFM encoded data.
        /// </summary>
        private Stream encodedStream;

        /// <summary>
        /// The last encoded byte output before the current write position in <see cref="encodedStream"/>.  We need to keep this
        /// information since MFM encoding depends on the the previsoly written byte to encode the next output byte.
        /// </summary>
        private int lastOutput = 0;


        private bool isDisposed = false;


        /// <summary>
        /// Create a stream for MFM encoding or decoding.
        /// </summary>
        /// <param name="encodedStream">Encoded stream.</param>
        public MfmStream(Stream encodedStream)
        {
            if (encodedStream == null) throw new ArgumentNullException("encodedStream");
            this.encodedStream = encodedStream;
        }


        /// <summary>
        /// Writes an A1 sync sequence to the stream.
        /// </summary>
        public void WriteSync()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            encodedStream.WriteByte(0x22);
            encodedStream.WriteByte(0x91);
        }


        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing, or the stream is already closed. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void WriteByte(byte value)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);

            byte out1 = EncoderTable[((lastOutput << 4) | (value >> 4)) & 0x1f];
            byte out2 = EncoderTable[value & 0x1f];

            encodedStream.WriteByte(out1);
            encodedStream.WriteByte(out2);
            lastOutput = value;
        }


        public void WriteBytes(byte value, int count)
        {
            while (count-- > 0)
                WriteByte(value);
        }



        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <param name="sync">Set to <value>true</value> if the byte read was an A1 sync sequence.</param>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public int ReadByte(out bool sync)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            sync = false;
            int encodedValue1 = encodedStream.ReadByte();
            int encodedValue2 = encodedStream.ReadByte();
            if (encodedValue1 == -1 || encodedValue2 == -1) return -1;

            if (encodedValue1 == 0x22 && encodedValue2 == 0x91)
            {
                sync = true;
                return 0xa1;
            }

            byte value = 0;
            value = (byte)((value << 1) | ((encodedValue1 & 0x02) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue1 & 0x08) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue1 & 0x20) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue1 & 0x80) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue2 & 0x02) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue2 & 0x08) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue2 & 0x20) == 0 ? 0 : 1));
            value = (byte)((value << 1) | ((encodedValue2 & 0x80) == 0 ? 0 : 1));

            return value;
        }


        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int ReadByte()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            bool sync;
            return ReadByte(out sync);
        }


        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            encodedStream.Flush();
        }


        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. </param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            return encodedStream.Seek(offset*2, origin)/2;
        }


        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            encodedStream.SetLength(value*2);
        }


        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (buffer == null) throw new ArgumentNullException("buffer");
            int readCount = 0;
            int value;
            while (count-- > 0 && (value = ReadByte()) != -1)
            {
                buffer[offset + readCount] = (byte) value;
                readCount++;
            }
            return readCount;    
        }


        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream. </param>
        /// <param name="count">The number of bytes to be written to the current stream. </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (buffer == null) throw new ArgumentNullException("buffer");
            while (count-- > 0)
            {
                WriteByte(buffer[offset++]);
            }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        public override bool CanRead
        {
            get { return encodedStream.CanRead; }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// </returns>
        public override bool CanSeek
        {
            get { return encodedStream.CanSeek; }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        public override bool CanWrite
        {
            get { return encodedStream.CanWrite; }
        }


        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get { return encodedStream.Length/2; }
        }


        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get { return encodedStream.Position/2; }
            set { encodedStream.Position = value*2; }
        }


        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                Flush();
                encodedStream = null;
            }

            base.Dispose(disposing);
            isDisposed = true;
        }




        /// <summary>
        /// MFM encoding table used by <see cref="WriteByte"/>.  The table index is 5 bits.  The MSB is the last bit from the
        /// previously encoded byte followed by the nibble to be encoded.
        /// </summary>
        private static readonly byte[] EncoderTable =
        {
            0x55, 0x95, 0x25, 0xa5,
            0x49, 0x89, 0x29, 0xa9,
            0x52, 0x92, 0x22, 0xa2,
            0x4a, 0x8a, 0x2a, 0xaa,
            0x54, 0x94, 0x24, 0xa4,
            0x48, 0x88, 0x28, 0xa8,
            0x52, 0x92, 0x22, 0xa2,
            0x4a, 0x8a, 0x2a, 0xaa
        };
    }
}
