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
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.IO;


namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Provides support for reading and writing Dragon tape filesystems.
    /// The filesystem can be opened for reading or writing, but not both.
    /// </summary>
    public sealed class DragonTapeFilesystem : ITapeFilesystem
    {
        private bool isdisposed = false;

        private ITapeReader reader = null;
        private ITapeWriter writer = null;

        /// <summary>
        /// The gap (in milliseconds) inserted between gapped tape blocks.
        /// </summary>
        public const int BlockGap = 500;

        /// <summary>
        /// The number of leader bytes to preceede a block where synchronization must be regained.
        /// The default value is <see cref="DefaultLeaderLength">DefaultLeaderLength</see>.
        /// </summary>
        public int LeaderLength { get; set; }


        /// <summary>
        /// When set, blocks read from tape will be validated an an exception will be thrown if the block is invalid.
        /// This flag is set by default.
        /// </summary>
        public bool ValidateBlocks { get; set; }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!isdisposed)
            {
                if (reader != null) reader.Dispose();
                if (writer != null) writer.Dispose();
                reader = null;
                writer = null;
                isdisposed = true;
            }
        }


        /// <summary>
        /// <value>true</value> if the filesystem supports write operations.
        /// </summary>
        public bool IsWriteable
        {
            get { return writer != null; }
        }


        /// <summary>
        /// <value>true</value> if the filesystem supports read operations.
        /// </summary>
        public bool IsReadable
        {
            get { return writer != null; }
        }


        public DragonTapeFilesystem(ITapeWriter writer)
        {
            this.writer = writer;
        }


        public DragonTapeFilesystem(ITapeReader reader)
        {
            this.reader = reader;
        }


        /// <summary>
        /// Verifies that a filename is valid for this filesystem.
        /// </summary>
        /// <param name="filename">Filename to validate.</param>
        /// <returns><value>true</value> if the filename is valid.</returns>
        public bool IsValidFilename(string filename)
        {
            return true;
            //TODO Implement proper checking for valid filenames
        }



        /// <summary>
        /// Synchronize tape.
        /// Reads the tape until a sufficiently long sequence of leader bytes and a single sync byte has been read.
        /// </summary>
        /// <param name="minLeaderLength">The minimum required number of leader bits before the sync byte.  Can be 0.</param>
        /// <returns>The number of leader bits read.</returns>
        public int Sync(int minLeaderLength)
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader == null) throw new InvalidOperationException("Instance does not support reading");
            return DragonTapeBlock.Sync(reader, minLeaderLength);
        }


        /// <summary>
        /// Read the next byte from the tape.
        /// </summary>
        /// <returns>Next byte read from tape.</returns>
        /// <exception cref="EndOfTapeException">Thrown if attempting to read beyond the end of the tape.</exception>
        public byte ReadByte()
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader == null) throw new InvalidOperationException("Instance does not support reading");
            return reader.ReadByte();
        }


        /// <summary>
        /// Read the next block from the tape.
        /// Assumes that the tape is positioned at the first bit following the block sync byte by a previous call to Sync.
        /// </summary>
        /// <returns>Tape block.</returns>
        /// <exception cref="EndOfTapeException">Attempting to read beyond the end of the tape.</exception>
        /// <exception cref="InvalidBlockChecksumException">The block checksum is invalid.</exception>
        /// <exception cref="InvalidBlockTypeException">The block type is invalid.</exception>
        /// <exception cref="InvalidHeaderBlockException">Reading a header block that is invalid.</exception>
        /// <seealso cref="Sync"/>
        public DragonTapeBlock ReadBlock()
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader == null) throw new InvalidOperationException("Instance does not support reading");
            return DragonTapeBlock.ReadBlock(reader);
        }


        /// <summary>
        /// Read and parse a file.  
        /// The returned object contains the file data and any meta-information related to the file.  This class will always return an object
        /// of type <see cref="DragonFile"/> or one of its descendants.
        /// </summary>
        /// <param name="filename">Name of file to read, or <value>null</value> to read the next file from the stream.</param>
        /// <returns>File object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile ReadFile(string filename)
        {
            if (filename == null) return ReadFile();

            throw new NotImplementedException();
        }


        /// <summary>
        /// Read and parse the next file from tape.
        /// The returned object contains the file data and any meta-information related to the file.  This class will always return an object
        /// of type <see cref="DragonFile"/> or one of its descendants.
        /// </summary>
        /// <returns>File object.</returns>
        public IFile ReadFile()
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader == null) throw new InvalidOperationException("Instance does not support reading");

            var header = ReadHeaderBlock();
            var data = ReadDataBlocks(header.Filename, header.IsGapped);

            DragonFile file = null;
            switch (header.FileType)
            {
                case DragonTapeFileType.Basic:
                    file = new DragonBasicFile(header.Filename, data, header.IsAscii, header.IsGapped);
                    break;
                case DragonTapeFileType.MachineCode:
                    file = new DragonMachineCodeFile(header.Filename, data, header.IsAscii, header.IsGapped, header.LoadAddress, header.ExecAddress);
                    break;
                case DragonTapeFileType.Data:
                    file = new DragonDataFile(header.Filename, data, header.IsAscii, header.IsGapped);
                    break;
                default:
                    throw new InvalidHeaderBlockException(String.Format("Unrecognized file type {0} in header block", (int) header.FileType));
            }
            return file;
        }





        /// <summary>
        /// Reads a sequence of data blocks.
        /// Reads data blocks until an EOF block is encoutered.  Return the combined payload from the data blocks.  Assumes that the tape is located
        /// just following the header block.
        /// </summary>
        /// <param name="isgapped">Set if the blocks are recorded with gaps.</param>
        /// <returns>Data payload</returns>
        private byte[] ReadDataBlocks(string filename, bool isgapped)
        {
            /* Read blocks and put the block payloads into the blocks list.  End when the EOF block is encountered. */
            var blocks = new List<byte[]>();
            var datalength = 0;
            var eof = false;
            while (!eof)
            {
                DragonTapeBlock.Sync(reader, 0);
                var block = DragonTapeBlock.ReadBlock(reader);
                switch (block.BlockType)
                {
                    case DragonTapeBlockType.Data:
                        blocks.Add(block.Data);
                        datalength += block.Length;
                        break;
                    case DragonTapeBlockType.EndOfFile:
                        eof = true;
                        break;
                    default:
                        throw new InvalidFileException(filename, String.Format("Unexpected block of type {0}", (int) block.BlockType));
                }
            }

            /* Convert the individual blocks into a single byte array. */
            var data = new byte[datalength];
            var offset = 0;
            for (int i = 0; i < blocks.Count; i++ )
            {
                Array.Copy(blocks[i], 0, data, offset, blocks[i].Length);
                offset += blocks[i].Length;
            }

            return data;
        }


        /// <summary>
        /// Reads the next file header block from tape.
        /// This function will continue reading tape data until it finds a header block.
        /// </summary>
        /// <returns>The header block.</returns>
        private DragonTapeHeaderBlock ReadHeaderBlock()
        {
            while (true)
            {
                DragonTapeBlock.Sync(reader, 1);
                var block = DragonTapeBlock.ReadBlock(reader);
                if (block is DragonTapeHeaderBlock) return (DragonTapeHeaderBlock) block;
            }
        }


        /// <summary>
        /// Write a byte to the tape.
        /// </summary>
        /// <param name="value">Byte value to write to the tape.</param>
        public void WriteByte(byte value)
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (writer == null) throw new InvalidOperationException("Instance does not supporting writing");
            writer.WriteByte(value);
        }


        /// <summary>
        /// Write a block to tape.
        /// </summary>
        /// <param name="block">Block to write to tape.</param>
        /// <param name="sync">Set if the tape is synchronized.  This will generate a short block leader.</param>
        public void WriteBlock(DragonTapeBlock block, bool sync)
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (writer == null) throw new InvalidOperationException("Instance does not supporting writing");
            block.WriteBlock(writer, sync);         // TODO Take the LeaderLength field into account when writing blocks to tape
        }


        /// <summary>
        /// Write a file to the filesystem.
        /// The provided filename will override any filename specified in the file object.
        /// </summary>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        public void WriteFile(IFile file)
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (writer == null) throw new InvalidOperationException("Instance does not support writing");
            if (file == null) throw new ArgumentNullException("file");
            if (!(file is DragonFile)) throw new ArgumentException("Unexpected type of file parameter : " + file.GetType().FullName);

            var dragonfile = (DragonFile) file;
            WriteHeaderBlock(dragonfile);
            WriteDataBlocks(dragonfile);
            WriteEndOfFileBlock(dragonfile);
        }


        private void WriteDataBlocks(DragonFile file)
        {
            int blocks = (file.Length + 254)/255;                   // number of data blocks to write
            int offset = 0;
            bool firstblock = true;                                 // make sure to create a long leader for the first data block
            for (int i = 0; i < blocks; i++ )
            {
                var block = new DragonTapeBlock(file.Data, offset, Math.Min(255, file.Length - offset));
                block.WriteBlock(writer, firstblock || !file.IsGapped);
                offset += 255;
                firstblock = false;
            }
        }


        private void WriteHeaderBlock(DragonFile file)
        {
            var filetype = DragonTapeFileType.Data;
            var loadaddress = 0;
            var execaddress = 0;

            if (file is DragonMachineCodeFile)
            {
                filetype = DragonTapeFileType.MachineCode;
                loadaddress = ((DragonMachineCodeFile) file).LoadAddress;
                execaddress = ((DragonMachineCodeFile) file).ExecAddress;
            }
            if (file is DragonBasicFile) filetype = DragonTapeFileType.Basic;

            var block = new DragonTapeHeaderBlock(file.Name, filetype, file.IsAscii, file.IsGapped, loadaddress, execaddress);
            block.WriteBlock(writer, false);
        }



        private void WriteEndOfFileBlock(DragonFile file)
        {
            var block = new DragonTapeEofBlock();
            block.WriteBlock(writer, !file.IsGapped);
        }


        /// <summary>
        /// Rewind the tape.
        /// </summary>
        public void Rewind()
        {
            if (isdisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader != null) reader.Rewind();
            if (writer != null) writer.Rewind();
        }
    }
}
