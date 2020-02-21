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
