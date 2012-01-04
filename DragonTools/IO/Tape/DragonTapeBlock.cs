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
    /// A Dragon tape filesystem block.
    /// </summary>
    /// <seealso cref="DragonTapeFilesystem"/>
    public class DragonTapeBlock
    {
        public const byte LeaderByte = 0x55;
        public const byte SyncByte = 0x3c;
        public const int DefaultLeaderLength = 128;

        public DragonTapeBlockType BlockType { get; protected set; }

        public int Length { get { return Data == null ? 0 : Data.Length; } }

        public byte[] Data { get; protected set; }

        public int Checksum { get; protected set; }


        /// <summary>
        /// Validate a block object and throw exception if the block is invalid.
        /// </summary>
        /// <exception cref="InvalidBlockTypeException">Block type is invalid.</exception>
        /// <exception cref="InvalidBlockChecksumException">Block checksum is invalid.</exception>
        /// <exception cref="InvalidHeaderBlockException">Header is invalid.</exception>
        public virtual void Validate()
        {
            if (BlockType != DragonTapeBlockType.Data && BlockType != DragonTapeBlockType.Header && BlockType!= DragonTapeBlockType.EndOfFile)
                throw new InvalidBlockTypeException(String.Format("Invalid block type {0}", (int) BlockType));

            if (Checksum != ComputeChecksum())
                throw new InvalidBlockChecksumException();
        }


        /// <summary>
        /// Compute the block checksum based on the block data.
        /// </summary>
        /// <returns>The computed block checksum.</returns>
        public int ComputeChecksum()
        {
            int checksum = (int) BlockType + Length;
            if (Data != null)
            {
                foreach (var b in Data)
                {
                    checksum += b;
                }
            }
            return checksum & 0xff;
        }


        public override string ToString()
        {
            return String.Format("Dragon tape block (Type={0} Length={1})", (int) BlockType, Length);
        }


        public DragonTapeBlock()
        {
            BlockType = DragonTapeBlockType.Data;
            Checksum = ComputeChecksum();
        }


        public DragonTapeBlock(byte[] data)
        {
            BlockType = DragonTapeBlockType.Data;
            Data = data;
            Checksum = ComputeChecksum();
        }



        public DragonTapeBlock(byte[] data, int offset, int length)
        {
            BlockType = DragonTapeBlockType.Data;
            Data = new byte[length];
            Array.Copy(data, offset, Data, 0, length);
            Checksum = ComputeChecksum();
        }


        /// <summary>
        /// Synchronize tape.
        /// Reads the tape until a sufficiently long sequence of leader bytes and a single sync byte has been read.
        /// </summary>
        /// <param name="reader">Object for reading bits from tape.</param>
        /// <param name="minLeaderLength">The minimum required number of leader bits before the sync byte.  Can be 0.</param>
        /// <returns>The number of leader bits actually read.</returns>
        public static int Sync(ITapeReader reader, int minLeaderLength)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (minLeaderLength < 0) throw new ArgumentOutOfRangeException("minLeaderLength", minLeaderLength, "minLeaderLength cannot be negative");

            int leaderlength = 0;           
            int leaderpos = 0;
            int synclength = 0;
            int syncpos = 0;
            bool bit;

            // TODO Document this algorithm in comments
            // TODO Fix bug with incorrect calculation of leader length
            while (synclength < 8)
            {
                bit = reader.ReadBit();

                if (leaderlength >= minLeaderLength && bit == ((SyncByte & (0x80 >> syncpos)) != 0))
                {
                    synclength++;
                    syncpos = (syncpos + 1)%8;
                    leaderpos = (leaderpos + 1)%8;
                }
                else if (bit == ((LeaderByte & (0x80 >> leaderpos)) != 0))
                {
                    synclength = 0;
                    syncpos = 0;
                    leaderlength++;
                    leaderpos = (leaderpos + 1)%8;
                }
                else
                {
                    synclength = leaderlength = 0;
                    syncpos = leaderpos = 0;
                }
            }
            return leaderlength;
        }


        /// <summary>
        /// Read the next tape block.
        /// Assumes that the tape reader is positioned just after the block sync byte.
        /// This method will not validate the block and may return invalid blocks.  Use <see cref="Validate"/> to 
        /// verify the validity of a block.
        /// </summary>
        /// <param name="reader">Tape reader.</param>
        /// <returns>Tape block object.</returns>
        public static DragonTapeBlock ReadBlock(ITapeReader reader)
        {
            int blocktype = reader.ReadByte();
            int blocklength = reader.ReadByte();
            var data = new byte[blocklength];
            for (int i = 0; i < blocklength; i++)
                data[i] = reader.ReadByte();
            int checksum = reader.ReadByte();

            DragonTapeBlock block = null;
            switch (blocktype)
            {
                case (int) DragonTapeBlockType.Header:
                    block = new DragonTapeHeaderBlock(data, checksum);
                    break;
                case (int) DragonTapeBlockType.EndOfFile:
                    block = new DragonTapeEofBlock {Data = data, Checksum = checksum};
                    break;
                default:
                    block = new DragonTapeBlock {BlockType  = (DragonTapeBlockType) blocktype, Data = data, Checksum = checksum};
                    break;
            }
            return block;
        }


        /// <summary>
        /// Write a block to tape.
        /// This will write the block to tape, including leader, sync byte and trailing leader.
        /// </summary>
        /// <param name="writer">Tape writer.</param>
        /// <param name="sync">When set, the tape is assumed to be synchronized and only a short leader is output.</param>
        public void WriteBlock(ITapeWriter writer, bool sync)
        {
            /* Write leader and sync byte. */
            var leaderlength = sync ? 1 : DefaultLeaderLength;
            while (leaderlength-- > 0)
            {
                writer.WriteByte(LeaderByte);
            }
            writer.WriteByte(SyncByte);

            /* Write the block header, payload and checksum. */
            writer.WriteByte((byte) BlockType);
            writer.WriteByte((byte) Length);
            for (var i = 0; i < Length; i++ )
                writer.WriteByte(Data[i]);
            writer.WriteByte((byte) Checksum);

            /* Write the trailing leader byte. */
            writer.WriteByte(LeaderByte);
        }
    }


    /// <summary>
    /// A Dragon tape header block.
    /// </summary>
    public sealed class DragonTapeHeaderBlock : DragonTapeBlock
    {
        public string Filename { get; private set; }

        public DragonTapeFileType FileType { get; private set; }

        public bool IsAscii { get; private set; }

        public bool IsGapped { get; private set; }

        public int LoadAddress { get; private set; }

        public int ExecAddress { get; private set; }


        /// <summary>
        /// Create a header block by parsing the block payload.
        /// </summary>
        /// <param name="data">Block payload.</param>
        /// <param name="checksum">Block checksum.</param>
        public DragonTapeHeaderBlock(byte[] data, int checksum)
        {
            BlockType = DragonTapeBlockType.Header;
            Data = data;
            Checksum = checksum;

            Filename = ParseFilename(data, 0);
            FileType = (DragonTapeFileType) data[8];
            IsAscii = (data[9] != 0);
            IsGapped = (data[10] != 0);
            ExecAddress = ((data[11] << 8) | data[12]);
            LoadAddress = ((data[13] << 8) | data[14]);
        }


        /// <summary>
        /// Create a header block by specifying the individual header fields.
        /// The block checksum will be computed based on the header fields.
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="filetype">File type.</param>
        /// <param name="isascii">Set for an ASCII encoded file.</param>
        /// <param name="isgapped">Set for a file recorded with gaps between each block.</param>
        /// <param name="loadaddress">Load address for machine code programs.</param>
        /// <param name="execaddress">Exec address for machine code programs.</param>
        public DragonTapeHeaderBlock(string filename, DragonTapeFileType filetype, bool isascii, bool isgapped, int loadaddress = 0, int execaddress = 0)
        {
            BlockType = DragonTapeBlockType.Header;
            Filename = filename;
            FileType = filetype;
            IsAscii = isascii;
            IsGapped = isgapped;
            LoadAddress = loadaddress;
            ExecAddress = execaddress;
            Data = Encode();
            Checksum = ComputeChecksum();
        }



        /// <summary>
        /// Validate a block object and throw exception if the block is invalid.
        /// </summary>
        /// <exception cref="InvalidBlockTypeException">Block type is invalid.</exception>
        /// <exception cref="InvalidBlockChecksumException">Block checksum is invalid.</exception>
        /// <exception cref="InvalidHeaderBlockException">Header is invalid.</exception>
        public override void Validate()
        {
            base.Validate();

            if (FileType != DragonTapeFileType.Basic && FileType != DragonTapeFileType.MachineCode && FileType != DragonTapeFileType.Data)
                throw new InvalidHeaderBlockException(String.Format("Invalid file type {0}", (int) FileType));
        }



        /// <summary>
        /// Encode the fields of the header block and returns the encoded block payload.
        /// </summary>
        /// <returns>Encoded block payload.</returns>
        private byte[] Encode()
        {
            var data = new byte[15];
            EncodeFilename(Filename, data, 0);
            data[8] = (byte) FileType;
            data[9] = (byte) (IsAscii ? 0xff : 0);
            data[10] = (byte) (IsGapped ? 0xff : 0);
            data[11] = (byte) ((ExecAddress >> 8) & 0xff);
            data[12] = (byte) (ExecAddress & 0xff);
            data[13] = (byte) ((LoadAddress >> 8) & 0xff);
            data[14] = (byte) (LoadAddress & 0xff);
            return data;
        }


        /// <summary>
        /// Encode a filename.
        /// </summary>
        /// <param name="filename">Filename to encode.</param>
        /// <param name="data">Byte array to receive the encoded filename.</param>
        /// <param name="offset">Offset into the data array for storing the filename.</param>
        private void EncodeFilename(string filename, byte[] data, int offset)
        {
            for (int i = 0; i < 8; i++)
                data[offset + i] = (byte) (i < filename.Length ? filename[i] : 0x20);
        }


        /// <summary>
        /// Decode a filename.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private string ParseFilename(byte[] data, int offset)
        {
            var filename = new char[8];
            for (int i = 0; i < 8; i++)
                filename[i] = (char) data[i + offset];
            return new string(filename).Trim();
        }
    }


    /// <summary>
    /// A Dragon tape end of file block.
    /// </summary>
    public sealed class DragonTapeEofBlock : DragonTapeBlock
    {
        public DragonTapeEofBlock()
        {
            BlockType = DragonTapeBlockType.EndOfFile;
            Checksum = ComputeChecksum();
        }
    }


    /// <summary>
    /// Valid Dragon tape block types.
    /// </summary>
    public enum DragonTapeBlockType
    {
        Header = 0,
        Data = 1,
        EndOfFile = 0xff
    }


    /// <summary>
    /// Valid Dragon tape file types.
    /// </summary>
    public enum DragonTapeFileType
    {
        Basic = 0,
        Data = 1,
        MachineCode = 2
    }
}
