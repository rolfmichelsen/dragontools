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
using RolfMichelsen.Dragon.DragonTools.IO.Tape;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape
{
    /// <summary>
    /// Represents a tape block in a Dragon tape filesystem and provides functionality for creating, reading and writing tape blocks
    /// to a virtual tape.
    /// Subclasses of this class provide specialized support for all known block types.
    /// </summary>
    /// <seealso cref="DragonTape"/>
    /// <seealso cref="DragonTapeDataBlock"/>
    /// <seealso cref="DragonTapeHeaderBlock"/>
    /// <seealso cref="DragonTapeEofBlock"/>
    public class DragonTapeBlock
    {
        public const byte LeaderByte = 0x55;
        public const byte SyncByte = 0x3c;
        public const int DefaultLongLeaderLength = 128;
        public const int DefaultShortLeaderLength = 1;


        /// <summary>
        /// The maximum size of the data payload for a single tape block.
        /// </summary>
        public const int MaxPayloadLength = 255;

        /// <summary>
        /// Block payload data, or <value>null</value> if the block has no payload.
        /// </summary>
        private byte[] blockData = null;

        /// <summary>
        /// Block type identifier.
        /// </summary>
        public DragonTapeBlockType BlockType { get; private set; }

        /// <summary>
        /// Size of the block payload data.
        /// </summary>
        public int Length { get { return Data == null ? 0 : Data.Length; } }

        /// <summary>
        /// Block payload data.
        /// </summary>
        public byte[] Data
        {
            get { return blockData == null ? null : (byte[]) blockData.Clone(); }
        }


        /// <summary>
        /// Block checksum.
        /// </summary>
        public int Checksum { get; protected set; }



        /// <summary>
        /// Set the block data payload.
        /// Block objects should be immutable so this method should only be used during object construction.
        /// </summary>
        /// <param name="data">Memory area containing the block data payload or <value>null</value> for no payload.</param>
        /// <param name="offset">Offset into the memory are containing the first byte of block data payload.</param>
        /// <param name="length">Length of the block data payload.</param>
        protected void SetData(byte[] data, int offset, int length)
        {
            if (length > MaxPayloadLength) 
                throw new ArgumentOutOfRangeException(String.Format("Trying to create a block with payload of {0} bytes, exceeding the maximum of {1} bytes", length, MaxPayloadLength));
            if (data == null || length == 0)
            {
                blockData = null;
            }
            else
            {
                blockData = new byte[length];
                Array.Copy(data, offset, blockData, 0, length);
            }
        }



        /// <summary>
        /// Create a Dragon tape block object.  
        /// </summary>
        /// <param name="blocktype">Block type identifier.</param>
        internal DragonTapeBlock(DragonTapeBlockType blocktype)
        {
            BlockType = blocktype;
        }



        /// <summary>
        /// Create a Dragon tape block object.
        /// Note that it is possible to create illegal blocks using this method by providing an unsupported block type or an invalid
        /// checksum.  Use the <see cref="Validate">Validate</see> method to verify that the block is actually valid.
        /// For supported block types, the returned object will be of the appropriate type.
        /// </summary>
        /// <param name="blocktype">Block type idenfifier.</param>
        /// <param name="data">Array containing the block payload data, or <value>null</value> for no payload.</param>
        /// <param name="offset">Offset into the data array of first byte of block payload data.</param>
        /// <param name="length">Size of the block payload data.</param>
        /// <param name="checksum">Block checksum.</param>
        /// <returns>Dragon tape block object.</returns>
        public static DragonTapeBlock CreateBlock(DragonTapeBlockType blocktype, byte[] data, int offset, int length, int checksum)
        {
            DragonTapeBlock block = null;
            switch (blocktype)
            {
                case DragonTapeBlockType.Header:
                    block = new DragonTapeHeaderBlock(data, offset, length, checksum);
                    break;
                case DragonTapeBlockType.Data:
                    block = new DragonTapeDataBlock(data, offset, length, checksum);
                    break;
                case DragonTapeBlockType.EndOfFile:
                    block = new DragonTapeEofBlock(data, offset, length, checksum);
                    break;
                default:
                    block = new DragonTapeBlock(blocktype);
                    block.SetData(data, offset, length);
                    block.Checksum = checksum;
                    break;
            }

            return block;
        }



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



        /// <summary>
        /// Return <value>true</value> if the block checksum is valid.
        /// </summary>
        public bool IsChecksumValid()
        {
            return (Checksum == ComputeChecksum());
        }



        public override string ToString()
        {
            return String.Format("Block: Type={0} (Unknown) Length={1} Checksum={2} ({3})", (int) BlockType, Length, Checksum, (IsChecksumValid() ? "Valid" : "Invalid"));
        }




        /// <summary>
        /// Synchronize tape.
        /// Reads the tape until a sufficiently long sequence of leader bytes and a single sync byte has been read.
        /// </summary>
        /// <param name="tape">Object for reading bits from tape.</param>
        /// <param name="minLeaderLength">The minimum required number of leader bits before the sync byte.  Can be 0.</param>
        internal static void Sync(ITape tape, int minLeaderLength)
        {
            if (tape == null) throw new ArgumentNullException("tape");
            if (minLeaderLength < 0) throw new ArgumentOutOfRangeException("minLeaderLength", minLeaderLength, "minLeaderLength cannot be negative");

            int leaderlength = 0;           
            int leaderpos = 0;
            int synclength = 0;
            int syncpos = 0;
            bool bit;

            while (synclength < 8)
            {
                bit = tape.ReadBit();

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
        }


        /// <summary>
        /// Read the next tape block.
        /// Assumes that the tape reader is positioned just after the block sync byte.
        /// This method will not validate the block and may return invalid blocks.  Use <see cref="Validate"/> to 
        /// verify the validity of a block.
        /// </summary>
        /// <param name="tape">Tape reader.</param>
        /// <returns>Tape block object.</returns>
        internal static DragonTapeBlock ReadBlockSynced(ITape tape)
        {
            int blocktype = tape.ReadByte();
            int blocklength = tape.ReadByte();
            var data = new byte[blocklength];
            for (int i = 0; i < blocklength; i++)
                data[i] = tape.ReadByte();
            int checksum = tape.ReadByte();

            return CreateBlock((DragonTapeBlockType) blocktype, data, 0, blocklength, checksum);
        }



        /// <summary>
        /// Read the next block from the tape.
        /// This method will not validate the block read and may return an invalid block.  Use <see cref="Validate">Validate</see>
        /// to verify the validity of the returned block.
        /// </summary>
        /// <param name="tape">Virtual tape.</param>
        /// <param name="minLeaderLength">The minimum leader length (in bits) required for synchronizing the block.  Can be <value>0</value>.</param>
        /// <returns>Tape block object.</returns>
        public static DragonTapeBlock ReadBlock(ITape tape, int minLeaderLength)
        {
            Sync(tape, minLeaderLength);
            return ReadBlockSynced(tape);
        }






        /// <summary>
        /// Write a block to tape.
        /// This will write the block to tape, including leader, sync byte and trailing leader.
        /// </summary>
        /// <param name="tape">Tape writer.</param>
        /// <param name="sync">When set, the tape is assumed to be synchronized and only a short leader is output.</param>
        public void WriteBlock(ITape tape, bool sync)
        {
            /* Write leader and sync byte. */
            var leaderlength = sync ? DefaultShortLeaderLength : DefaultLongLeaderLength;
            while (leaderlength-- > 0)
            {
                tape.WriteByte(LeaderByte);
            }
            tape.WriteByte(SyncByte);

            /* Write the block header, payload and checksum. */
            tape.WriteByte((byte) BlockType);
            tape.WriteByte((byte) Length);
            for (var i = 0; i < Length; i++ )
                tape.WriteByte(Data[i]);
            tape.WriteByte((byte) Checksum);

            /* Write the trailing leader byte. */
            tape.WriteByte(LeaderByte);
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


}
