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

using System;
using System.IO;


namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Represents a CAS virtual tape.
    /// </summary>
    public sealed class CasTape : ITape, IDisposable
    {
        /// <summary>
        /// The stream holding the CAS tape image.
        /// </summary>
        private Stream tape;


        /// <summary>
        /// Set when the Dispose method has been called.
        /// </summary>
        private bool isDisposed = false;


        /// <summary>
        /// Used to keep track of whether this object is reading or writing.  Reset to None when the tape is
        /// repositioned.
        /// </summary>
        private OperationType lastOperation = OperationType.None;

        
        private byte buffer = 0;

        private int bufferinx = 8;


        /// <summary>
        /// True if this tape supports read operations.
        /// </summary>
        public bool IsReadable { get { return tape.CanRead; }}


        /// <summary>
        /// True if this tape supports write operations.
        /// </summary>
        public bool IsWriteable { get { return tape.CanWrite; }}


        /// <summary>
        /// Creates a tape object for accessing CAS virtual tapes and associates it with a CAS file.
        /// </summary>
        /// <param name="tape">Stream containing the CAS file.</param>
        public CasTape(Stream tape)
        {
            if (tape == null) throw new ArgumentNullException("tape");
            this.tape = tape;
        }


        /// <summary>
        /// Read a single bit from the virtual stream and return it.
        /// </summary>
        /// <returns>Bit read from tape.</returns>
        /// <exception cref="EndOfTapeException">Trying to read past the end of the tape.</exception>
        public bool ReadBit()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (lastOperation != OperationType.None && lastOperation != OperationType.Read) throw new InvalidOperationException("Cannot mix read and write operations without repositioning the tape");

            lastOperation = OperationType.Read;
            if (bufferinx == 8)
            {
                int value = tape.ReadByte();
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
        /// Rewinds the stream to its beginning.
        /// </summary>
        // BUG Does not properly flush the write buffer before rewinding.
        public void Rewind()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            
            tape.Seek(0, SeekOrigin.Begin);
            bufferinx = 8;
            buffer = 0;
            lastOperation = OperationType.None;
        }


        /// <summary>
        /// Write a single bit to the virtual streaming medium.
        /// </summary>
        /// <param name="value">Bit to write to tape.</param>
        public void WriteBit(bool value)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (lastOperation != OperationType.None && lastOperation != OperationType.Write) throw new InvalidOperationException("Cannot mix read and write operations without repositioning the tape");

            lastOperation = OperationType.Write;

            if (value)
                buffer |= (byte) (1 << (bufferinx - 1));
            bufferinx--;
            if (bufferinx == 0) 
                Flush();
        }



        /// <summary>
        /// Flushes the write buffer to the stream.  Since the underlying stream can only deal with bytes, the data will be padded
        /// if there are less than 8 bits waiting in the buffer.  
        /// </summary>
        private void Flush()
        {
            if (lastOperation == OperationType.Write && bufferinx != 8)
            {
                tape.WriteByte(buffer);
                bufferinx = 8;
                buffer = 0;                
            }
        }


        /// <summary>
        /// Write a byte to the virtual streaming medium.
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
        /// Inserts a gap or blank section on the virtual streaming medium.
        /// </summary>
        /// <param name="length">Approximate length of gap (in milliseconds).</param>
        /// <remarks>
        /// The CAS tape format does not support gaps so this method does nothing.
        /// </remarks>
        public void InsertGap(int length)
        {
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (isDisposed) return;

            Flush();
            isDisposed = true;
            tape.Dispose();
            tape = null;
        }


        private enum OperationType
        {
            None,
            Read,
            Write
        }
    }
}
