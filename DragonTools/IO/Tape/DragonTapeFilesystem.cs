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
    /// A Dragon tape filesystem.
    /// </summary>
    public sealed class DragonTapeFilesystem : ITapeFilesystem
    {
        public const byte LeaderByte = 0x55;
        public const byte SyncByte = 0x3c;
        public const int DefaultLeaderLength = 128;
        
        private bool IsDisposed = false;

        private ITapeReader reader = null;
        private ITapeWriter writer = null;


        /// <summary>
        /// <value>true</value> if the filesystem supports write operations.
        /// </summary>
        public bool IsWriteable { get {return (writer != null); }}

        /// <summary>
        /// <value>true</value> if the filesystem supports read operations.
        /// </summary>
        public bool IsReadable { get { return (reader != null); } }


        /// <summary>
        /// The length of a block leader written to tape to allow re-synchronization of the tape.  A full block leader
        /// is always required for the first block after stopping the cassette motor or otherwise loosing 
        /// synchronization.
        /// </summary>
        public int LeaderLength { get; set; }


        /// <summary>
        /// When set, all blocks read from and written to the tape will be validated and only valid blocks will be processed.
        /// Invalid blocks will result in an exception being thrown.
        /// </summary>
        public bool ValidateBlocks { get; set; }


        public const bool DefaultValidateBlocks = true;

        /// <summary>
        /// Create a read-only tape filesystem and associate it with a tape reader.
        /// </summary>
        /// <param name="reader">Tape reader.</param>
        public DragonTapeFilesystem(ITapeReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            this.reader = reader;
            LeaderLength = DefaultLeaderLength;
            ValidateBlocks = DefaultValidateBlocks;
        }


        /// <summary>
        /// Create a write-only tape filesystem and associate it with a tape writer.
        /// </summary>
        /// <param name="writer">Tape writer.</param>
        public DragonTapeFilesystem(ITapeWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            this.writer = writer;
            LeaderLength = DefaultLeaderLength;
            ValidateBlocks = DefaultValidateBlocks;
        }


        /// <summary>
        /// Read and parse a file.  
        /// The returned object contains the file data and any meta-information related to the file.
        /// </summary>
        /// <param name="filename">Name of file to read, or <value>null</value> to read the next file from the stream.</param>
        /// <returns>File object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile ReadFile(string filename)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write a file to the filesystem.
        /// The provided filename will override any filename specified in the file object.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="file">File object to write.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        public void WriteFile(string filename, IFile file)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns a class for parsing and encoding files for this filesystem.
        /// Applications will normally use the <see cref="ITapeFilesystem.ReadFile">ReadFile</see> and <see cref="ITapeFilesystem.WriteFile">WriteFile</see> functions instead of directly using
        /// the file parser class.
        /// </summary>
        /// <returns>File parser class.</returns>
        public IFileParser GetFileParser()
        {
            return new DragonTapeFileParser();
        }


        /// <summary>
        /// Read a raw file and return it as a byte array.
        /// The file is not parsed in any way and all filesystem headers and meta-data are included
        /// in the returned byte array.
        /// </summary>
        /// <param name="filename">Name of file to read, or <value>null</value> to read the next file from the stream.</param>
        /// <returns>Raw file contents.</returns>
        /// <exception cref="FileNotFoundException">The file does not exist.</exception>
        public byte[] ReadFileRaw(string filename)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Write a raw file to the filesystem.  
        /// The data must include any filesystem headers and meta-data required by the filesystem.
        /// </summary>
        /// <param name="filename">Name of file to write.</param>
        /// <param name="data">Raw file data.</param>
        /// <exception cref="FilesystemNotWriteableException">This filesystem does not support write operations.</exception>
        /// <exception cref="InvalidFilenameException">The file name is invalid for this filesystem.</exception>
        public void WriteFileRaw(string filename, byte[] data)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Returns <value>true</value> if the filename is valid for a Dragon tape filesystem.
        /// </summary>
        /// <param name="filename">Filename to validate.</param>
        /// <returns><value>true</value> if the filename is valid.</returns>
        public bool IsValidFilename(string filename)
        {
            return true;
            //TODO Implement ValidateFilename
        }


        /// <summary>
        /// Read a byte from the tape.
        /// </summary>
        /// <returns>Byte read.</returns>
        /// <exception cref="EndOfTapeException">Thrown if the operation attempted to read past the end of the tape.</exception>
        /// <exception cref="NotSupportedException">Thrown if trying to perform this operation on an object not associated with an ITapeReader.</exception>
        public byte ReadByte()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader == null) throw new NotSupportedException();

            byte value = 0;
            for (int i=0; i<8; i++)
            {
                value <<= 1;
                if (reader.ReadBit()) value |= 1;
            }
            return value;
        }


        /// <summary>
        /// Write a byte to the tape.
        /// </summary>
        /// <param name="value">Value to write to the tape.</param>
        /// <exception cref="NotSupportedException">Thrown if trying to perform this operation on an object not associated with an ITapeWriter.</exception>
        public void WriteByte(byte value)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (writer == null) throw new NotSupportedException();

            for (int i=0; i<8; i++)
            {
                writer.WriteBit((value & 0x80) != 0);
                value <<= 1;
            }
        }


        /// <summary>
        /// Write a full block leader to the tape.
        /// </summary>
        /// <seealso cref="LeaderLength"/>
        /// <exception cref="NotSupportedException">Thrown if trying to perform this operation on an object not associated with an ITapeWriter.</exception>
        public void WriteBlockLeader()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (writer == null) throw new NotSupportedException();

            for (int i=0; i<LeaderLength; i++)
            {
                WriteByte(LeaderByte);
            }
        }


        /// <summary>
        /// Write a block to the tape.
        /// </summary>
        /// <param name="block">Block to write.</param>
        /// <exception cref="NotSupportedException">Thrown if trying to perform this operation on an object not associated with an ITapeWriter.</exception>
        public void WriteBlock(DragonTapeBlock block)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (writer == null) throw new NotSupportedException();
            if (ValidateBlocks) block.Validate();

            WriteByte(LeaderByte);
            WriteByte(SyncByte);
            var encodedblock = block.Encode();
            foreach (var b in encodedblock)
            {
                WriteByte(b);
            }
            WriteByte(LeaderByte);
        }


        /// <summary>
        /// Read a block from the tape.
        /// </summary>
        /// <param name="synchronzed">When true this methods assumes the tape to be synchronized.</param>
        /// <returns>Block read from tape.</returns>
        /// <exception cref="EndOfTapeException">Thrown if the operation attempted to read past the end of the tape.</exception>
        /// <exception cref="NotSupportedException">Thrown if trying to perform this operation on an object not associated with an ITapeReader.</exception>
        /// <exception cref="InvalidBlockChecksumException">Block validation is enabled and the block contains an invalid checksum.</exception>
        /// <exception cref="InvalidBlockTypeException">Block validation is enabled and the block type is invalid.</exception>
        public DragonTapeBlock ReadBlock(bool synchronzed)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (reader == null) throw new NotSupportedException();

            Sync(synchronzed);
            var type = ReadByte();                      // block type
            var length = ReadByte();                    // block payload size
            var data = new byte[length];                // block payload data
            for (int i = 0; i < length; i++ )
            {
                data[i] = ReadByte();
            }
            var checksum = ReadByte();                  // block checksum

            var block = new DragonTapeBlock((DragonTapeBlockType) type, data, 0, length, checksum);
            if (ValidateBlocks) block.Validate();
            return block;
        }


        /// <summary>
        /// Synchronize the tape stream.  This method will read the tape data stream until it finds a leader followed by a sync byte.
        /// </summary>
        /// <param name="synchronized">When true, the tape is assumed to be synchronized.</param>
        /// <exception cref="EndOfTapeException">Thrown if the operation attempted to read past the end of the tape.</exception>
        /// <exception cref="SynchronizationException">Unable to synchronize</exception>
        private void Sync(bool synchronized)
        {
            byte sync = 0;
            int leaderlength = 0;
            
            if (!synchronized)
            {
                //TODO Avoid hardcoding the leader length
                while (sync != 0x55 || leaderlength < 24)
                {
                    while (sync != 0x55)
                    {
                        leaderlength = 0;
                        sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    }
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    leaderlength += 2;
                }
            }

            while (true)
            {
                sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                if ((sync & 0x03) != 0x01)
                {
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    sync = (byte)((sync << 1) | (reader.ReadBit() ? 1 : 0));
                    if (sync == 0x3c) break;
                    throw new SynchronizationException();
                }
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (reader != null) reader.Dispose();
                if (writer != null) writer.Dispose();
                reader = null;
                writer = null;
            }
        }
    }
}
