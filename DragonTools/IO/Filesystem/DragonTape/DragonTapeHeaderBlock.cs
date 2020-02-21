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
    /// A Dragon tape header block.
    /// </summary>
    public sealed class DragonTapeHeaderBlock : DragonTapeBlock
    {
        public const int FileHeaderSize = 15;

        /// <summary>
        /// The file name.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// The file type.
        /// </summary>
        public DragonFileType FileType { get; private set; }

        /// <summary>
        /// Set to indicate that this file is recorded in ASCII format.
        /// </summary>
        public bool IsAscii { get; private set; }

        /// <summary>
        /// Set to indicate that this file is recorded with gaps between each block.
        /// </summary>
        public bool IsGapped { get; private set; }

        /// <summary>
        /// The loading address for a machine code program.
        /// </summary>
        public int LoadAddress { get; private set; }

        /// <summary>
        /// The execution start address for a machine code program.
        /// </summary>
        public int StartAddress { get; private set; }


        /// <summary>
        /// Create a header block by parsing the block payload.
        /// </summary>
        /// <param name="data">Memory area containing the block payload data.</param>
        /// <param name="length">Length of the block payload data.</param>
        /// <param name="offset">Offset into memory area containing the first byte of block payload data.</param>
        /// <param name="checksum">Block checksum.</param>
        /// <exception cref="InvalidHeaderBlockException">The header block is invalid and cannot be decoded.</exception>
        internal DragonTapeHeaderBlock(byte[] data, int offset, int length, int checksum)
            : base(DragonTapeBlockType.Header)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (length < FileHeaderSize)
                throw new InvalidHeaderBlockException(
                    String.Format("Header blocks must contain at least {0} bytes of data.  This block only contained {1} bytes.",
                    FileHeaderSize, length));

            SetData(data, offset, length);
            Checksum = checksum;

            Filename = DecodeFilename(data, offset);
            FileType = (DragonFileType) data[offset + 8];
            IsAscii = (data[offset + 9] != 0);
            IsGapped = (data[offset + 10] != 0);
            StartAddress = ((data[offset + 11] << 8) | data[offset + 12]);
            LoadAddress = ((data[offset + 13] << 8) | data[offset + 14]);
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
        public DragonTapeHeaderBlock(string filename, DragonFileType filetype, bool isascii, bool isgapped, int loadaddress = 0, int execaddress = 0) : base(DragonTapeBlockType.Header)
        {
            Filename = filename;
            FileType = filetype;
            IsAscii = isascii;
            IsGapped = isgapped;
            LoadAddress = loadaddress;
            StartAddress = execaddress;
            SetData(EncodeHeaderBlock(), 0, FileHeaderSize);
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

            if (FileType != DragonFileType.Basic && FileType != DragonFileType.MachineCode && FileType != DragonFileType.Data)
                throw new InvalidHeaderBlockException(String.Format("Invalid file type {0}", (int)FileType));
        }



        /// <summary>
        /// Encode the fields of the header block and returns the encoded block payload.
        /// </summary>
        /// <returns>Encoded block payload.</returns>
        private byte[] EncodeHeaderBlock()
        {
            var data = new byte[FileHeaderSize];
            EncodeFilename(Filename, data, 0);
            data[8] = (byte)FileType;
            data[9] = (byte)(IsAscii ? 0xff : 0);
            data[10] = (byte)(IsGapped ? 0xff : 0);
            data[11] = (byte)((StartAddress >> 8) & 0xff);
            data[12] = (byte)(StartAddress & 0xff);
            data[13] = (byte)((LoadAddress >> 8) & 0xff);
            data[14] = (byte)(LoadAddress & 0xff);
            return data;
        }


        /// <summary>
        /// Encode a filename.
        /// </summary>
        /// <param name="filename">Filename to encode.</param>
        /// <param name="data">Byte array to receive the encoded filename.</param>
        /// <param name="offset">Offset into the data array for storing the filename.</param>
        private static void EncodeFilename(string filename, byte[] data, int offset)
        {
            for (int i = 0; i < 8; i++)
                data[offset + i] = (byte)(i < filename.Length ? filename[i] : 0x20);
        }


        /// <summary>
        /// Decode a filename.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private static string DecodeFilename(byte[] data, int offset)
        {
            var filename = new char[8];
            for (int i = 0; i < 8; i++)
                filename[i] = (char)data[i + offset];
            return new string(filename).Trim();
        }


        public override string ToString()
        {
            return String.Format("Block: Type={0} (Header) Length={1} Name={2} Filetype={3} Load={4} Start={5} Checksum={6} ({7})", 
                (int)BlockType, Length, Filename, (int) FileType, LoadAddress, StartAddress, Checksum, (IsChecksumValid() ? "Valid" : "Invalid"));
        }

    }
}
