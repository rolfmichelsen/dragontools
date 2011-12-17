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
    /// A block read from or to be written to a Dragon tape.
    /// </summary>
    /// <seealso cref="DragonTapeFilesystem"/>
    public sealed class DragonTapeBlock
    {
        /// <summary>
        /// The maximum size of the data payload for a block.
        /// </summary>
        public const int MaxDataLength = 255;

        /// <summary>
        /// The normal (minimum) size of the payload for a header block.
        /// </summary>
        public const int HeaderLength = 15;

        private byte[] data = null;

        /// <summary>
        /// The block type.
        /// </summary>
        public DragonTapeBlockType Type { get; private set; }


        /// <summary>
        /// The length of the block payload (number of bytes).
        /// </summary>
        public int Length { get { return data.Length; } }


        /// <summary>
        /// Get a copy of the block payload.
        /// </summary>
        public byte[] Data 
        { 
            get
            {
                var copy = new byte[data.Length];
                Array.Copy(data, copy, data.Length);
                return copy;
            } 
        }


        /// <summary>
        /// The block checksum.
        /// </summary>
        public byte Checksum { get; private set; }


        /// <summary>
        /// Get the byte at a specific position in the block payload.
        /// </summary>
        /// <param name="index">Byte index in the block payload.</param>
        /// <returns>A byte from the block payload.</returns>
        /// <exception cref="IndexOutOfRangeException">Index is less than zero or greated than the length of the block payload.</exception>
        public byte this[int index] { get { return data[index];} }


        /// <summary>
        /// Create a tape block object.
        /// </summary>
        /// <param name="type">Block type.</param>
        /// <param name="data">Array containing the block payload data. Can be <value>null</value> for blocks without payload.</param>
        /// <param name="offset">Offset into array of first payload byte.</param>
        /// <param name="length">Length of block payload.</param>
        public DragonTapeBlock(DragonTapeBlockType type, byte[] data, int offset, int length)
        {
            if (data == null && length > 0) throw new ArgumentNullException("data");
            if (length > MaxDataLength) throw new ArgumentOutOfRangeException("length", length, "Block payload cannot exceed " + MaxDataLength + " bytes");
            Type = type;
            this.data = new byte[length];
            if (data != null)
                Array.Copy(data, offset, this.data, 0, length);
            Checksum = ComputeChecksum();
        }


        /// <summary>
        /// Create a tape block object, allowing manually setting the block checksum.
        /// </summary>
        /// <param name="type">Block type.</param>
        /// <param name="data">Array containing the block payload data. Can be <value>null</value> for blocks without payload.</param>
        /// <param name="offset">Offset into array of first payload byte.</param>
        /// <param name="length">Length of block payload.</param>
        /// <param name="checksum">Block checksum value.</param>
        public DragonTapeBlock(DragonTapeBlockType type, byte[] data, int offset, int length, byte checksum)
        {
            if (data == null && length > 0) throw new ArgumentNullException("data");
            if (length > MaxDataLength) throw new ArgumentOutOfRangeException("length", length, "Block payload cannot exceed " + MaxDataLength + " bytes");
            Type = type;
            this.data = new byte[length];
            if (data != null)
                Array.Copy(data, offset, this.data, 0, length);
            Checksum = checksum;
        }


        /// <summary>
        /// Create a tape block object.
        /// </summary>
        /// <param name="type">Block type.</param>
        /// <param name="data">Block payload data.</param>
        public DragonTapeBlock(DragonTapeBlockType type, byte[] data) : this(type, data, 0, data == null ? 0 : data.Length) {}



        /// <summary>
        /// Create a header tape block.
        /// </summary>
        /// <param name="filename">Filename to encode in the header block.</param>
        /// <param name="filetype">File type.</param>
        /// <param name="isascii">File ASCII flag.</param>
        /// <param name="isgapped">File gap flag.</param>
        /// <param name="startaddress">Start address for machine code programs.</param>
        /// <param name="loadaddress">Load address for machine code programs.</param>
        public DragonTapeBlock(string filename, DragonTapeFileType filetype , bool isascii, bool isgapped, uint startaddress, uint loadaddress)
        {
            if (filename == null) throw new ArgumentNullException("filename");

            data = new byte[HeaderLength];
            Array.Copy(EncodeFilename(filename), 0, data, 0, 8);
            data[8] = (byte) filetype;
            data[9] = (byte) (isascii ? 0xff : 0);
            data[10] = (byte) (isgapped ? 0xff : 0);
            data[11] = (byte) ((startaddress >> 8) & 0xff);
            data[12] = (byte) (startaddress & 0xff);
            data[13] = (byte) ((loadaddress >> 8) & 0xff);
            data[14] = (byte) (loadaddress & 0xff);

            Type = DragonTapeBlockType.Header;
            Checksum = ComputeChecksum();
        }


        /// <summary>
        /// Validate the block.  An exception is thrown if the block is invalid.
        /// </summary>
        /// <exception cref="InvalidBlockTypeException">The block type is invalid.</exception>
        /// <exception cref="InvalidBlockChecksumException">The block checksum is invalid.</exception>
        public void Validate()
        {
            if (!Enum.IsDefined(typeof(DragonTapeBlockType), Type)) throw new InvalidBlockTypeException(String.Format("Invalid block type {0}", (int) Type));
            if (Checksum != ComputeChecksum()) throw new InvalidBlockChecksumException();
            if (Type == DragonTapeBlockType.Header && Length < HeaderLength) throw new InvalidHeaderBlockException(String.Format("Header block is only {0} bytes and minimum size is {1} bytes", Length, HeaderLength));
        }


        /// <summary>
        /// Calculate the block checksum.
        /// </summary>
        /// <returns></returns>
        private byte ComputeChecksum()
        {
            int checksum = (byte) Type + (byte) Length;
            foreach (var b in data)
            {
                checksum += b;
            }
            return (byte) (checksum & 0xff);
        }


        /// <summary>
        /// Returns the encoded version of the block as it appears on the tape.  The encoded version does not include leading and trailing leader bytes
        /// or sync bytes.
        /// </summary>
        /// <returns>Array containing the encoded block.</returns>
        public byte[] Encode()
        {
            var encoded = new byte[data.Length + 3];
            encoded[0] = (byte) Type;
            encoded[1] = (byte) data.Length;
            Array.Copy(data, 0, encoded, 2, data.Length);
            encoded[data.Length + 2] = Checksum;
            return encoded;
        }


        /// <summary>
        /// Returns the filename for a header block.
        /// </summary>
        /// <exception cref="NotSupportedException">If this method is called for a block that is not a header block.</exception>
        public string Filename
        {
            get
            {
                if (Type != DragonTapeBlockType.Header) throw new NotSupportedException(String.Format("Operation not permitted for block of type {0}", (int) Type));
                return ParseFilename(data, 0);
            }
        }


        /// <summary>
        /// Returns the ASCII flag for a header block.
        /// </summary>
        /// <exception cref="NotSupportedException">If this method is called for a block that is not a header block.</exception>
        public bool IsAscii
        {
            get
            {
                if (Type != DragonTapeBlockType.Header) throw new NotSupportedException(String.Format("Operation not permitted for block of type {0}", (int)Type));
                return (data[9] != 0);
            }
        }


        /// <summary>
        /// Returns the gap flag for a header block.
        /// </summary>
        /// <exception cref="NotSupportedException">If this method is called for a block that is not a header block.</exception>
        public bool IsGapped
        {
            get
            {
                if (Type != DragonTapeBlockType.Header) throw new NotSupportedException(String.Format("Operation not permitted for block of type {0}", (int)Type));
                return (data[10] != 0);                
            }
        }


        /// <summary>
        /// Returns the start address for a header block.  The start address is only used for machine code programs.
        /// </summary>
        /// <exception cref="NotSupportedException">If this method is called for a block that is not a header block.</exception>
        public uint StartAddress
        {
            get
            {
                if (Type != DragonTapeBlockType.Header) throw new NotSupportedException(String.Format("Operation not permitted for block of type {0}", (int)Type));
                return (uint) (data[11] << 8 | data[12]);
            }
        }


        /// <summary>
        /// Returns the load address from a header block.  The load address is only used for machine code programs.
        /// </summary>
        /// <exception cref="NotSupportedException">If this method is called for a block that is not a header block.</exception>
        public uint LoadAddress
        {
            get
            {
                if (Type != DragonTapeBlockType.Header) throw new NotSupportedException(String.Format("Operation not permitted for block of type {0}", (int)Type));
                return (uint) (data[13] << 8 | data[14]);
            }
        }


        /// <summary>
        /// Returns the file type from a header block.
        /// </summary>
        /// <exception cref="NotSupportedException">If this method is called for a block that is not a header block.</exception>
        public DragonTapeFileType FileType
        {
            get
            {
                if (Type != DragonTapeBlockType.Header) throw new NotSupportedException(String.Format("Operation not permitted for block of type {0}", (int)Type));
                return (DragonTapeFileType) data[8];
            }
        }



        public override string ToString()
        {
            switch (Type)
            {
                case DragonTapeBlockType.Header:
                    return String.Format("Header: Length={0} Checksum={1} Filename={2} Type={3}", Length, Checksum, Filename, FileType);
                case DragonTapeBlockType.Data:
                    return string.Format("Data: Length={0} Checksum={1}", Length, Checksum);
                case DragonTapeBlockType.EndOfFile:
                    return string.Format("EOF: Length={0} Checksum={1}", Length, Checksum);
                default:
                    return string.Format("Type {0}: Length={1} Checksum={2}", (int) Type, Length, Checksum);
            }
        }


        private static string ParseFilename(byte[] raw, int offset)
        {
            var filename = new char[8];
            for (int i = 0; i < 8; i++) filename[i] = (char) raw[offset + i];
            return new string(filename).TrimEnd();
        }


        private static byte[] EncodeFilename(string filename)
        {
            var encoded = new byte[8];
            int i = 0;
            while (i<Math.Min(8, filename.Length))
            {
                encoded[i] = (byte) filename[i++];
            }
            while (i<8)
            {
                encoded[i] = 0x20;
            }
            return encoded;
        }

    }



    /// <summary>
    /// Valid block types for a Dragon tape filesystem.  The values can be cast to a byte to get the encoded value used on tape.
    /// </summary>
    /// <seealso cref="DragonTapeBlock"/>
    public enum DragonTapeBlockType : byte
    {
        /// <summary>
        /// Header block
        /// </summary>
        Header = 0x00,

        /// <summary>
        /// Data block.
        /// </summary>
        Data = 0x01,

        /// <summary>
        /// End of file block.
        /// </summary>
        EndOfFile = 0xff,
    }


    /// <summary>
    /// Valid file types for a Dragon tape filesystem.  The values can be cast to a byte to get the encoded value used on tape.
    /// </summary>
    public enum DragonTapeFileType : byte
    {
        BasicProgram = 0x00,
        DataFile = 0x01,
        NativeProgram = 0x02
    }
}
