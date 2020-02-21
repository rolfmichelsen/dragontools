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
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape
{
    /// <summary>
    /// Provides support for reading and writing Dragon tape filesystems.
    /// The filesystem can be opened for reading or writing, but not both.
    /// </summary>
    public sealed class DragonTape : ITapeFilesystem, IDisposable
    {
        /// <summary>
        /// Set when the Dispose method has been invoked.
        /// </summary>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// The gap (in milliseconds) inserted between gapped tape blocks.
        /// </summary>
        public const int BlockGap = 500;


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (!IsDisposed)
            {
                if (Tape != null && Tape is IDisposable) ((IDisposable)Tape).Dispose();
                Tape = null;
                IsDisposed = true;
            }
        }


        /// <summary>
        /// Reference to the virtual tape containing the filesystem.
        /// </summary>
        public ITape Tape { get; private set; }


        /// <summary>
        /// <value>true</value> if the filesystem supports write operations.
        /// </summary>
        public bool IsWriteable { get { return Tape.IsReadable; }}


        /// <summary>
        /// <value>true</value> if the filesystem supports read operations.
        /// </summary>
        public bool IsReadable { get { return Tape.IsWriteable; }}



        /// <summary>
        /// Create a Dragon tape filesystem and associate it with a virtual tape.
        /// </summary>
        /// <param name="tape">Virtual tape to associate filesystem with.</param>
        public DragonTape(ITape tape)
        {
            if (tape == null) throw new ArgumentNullException("tape");
            IsDisposed = false;
            Tape = tape;
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
        /// Read and parse a file.  
        /// The returned object contains the file data and any meta-information related to the file.  This class will always return an object
        /// of type <see cref="DragonFile"/> or one of its descendants.
        /// </summary>
        /// <param name="filename">Name of file to read, or <value>null</value> to read the next file from the stream.</param>
        /// <returns>File object.</returns>
        /// <exception cref="InvalidFileException">The file format is invalid.</exception>
        public IFile ReadFile(string filename)
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (!IsReadable) throw new InvalidOperationException("Instance does not support reading");

            if (filename == null) return ReadFile();

            while (true)
            {
                var file = (DragonFile) ReadFile();
                if (filename.Equals(file.Name)) return file;
            }
        }


        /// <summary>
        /// Read and parse the next file from tape.
        /// The returned object contains the file data and any meta-information related to the file.  This class will always return an object
        /// of type <see cref="DragonFile"/> or one of its descendants.
        /// </summary>
        /// <returns>File object.</returns>
        public IFile ReadFile()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (!IsReadable) throw new InvalidOperationException("Instance does not support reading");

            var header = ReadHeaderBlock();
            var data = ReadDataBlocks(header.Filename, header.IsGapped);

            return new DragonFile(header.Filename, header.FileType, data, header.IsAscii, header.IsGapped, header.LoadAddress, header.StartAddress);
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
                DragonTapeBlock.Sync(Tape, 0);
                var block = DragonTapeBlock.ReadBlockSynced(Tape);
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
                DragonTapeBlock.Sync(Tape, 1);
                var block = DragonTapeBlock.ReadBlockSynced(Tape);
                if (block is DragonTapeHeaderBlock) return (DragonTapeHeaderBlock) block;
            }
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
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
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
                var block = new DragonTapeDataBlock(file.GetData(), offset, Math.Min(255, file.Length - offset));
                block.WriteBlock(Tape, !(firstblock || file.IsGapped));
                offset += 255;
                firstblock = false;
            }
        }


        private void WriteHeaderBlock(DragonFile file)
        {
            var block = new DragonTapeHeaderBlock(file.Name, file.FileType, file.IsAscii, file.IsGapped, file.LoadAddress, file.StartAddress);
            block.WriteBlock(Tape, false);
        }



        private void WriteEndOfFileBlock(DragonFile file)
        {
            var block = new DragonTapeEofBlock();
            block.WriteBlock(Tape, !file.IsGapped);
        }


        /// <summary>
        /// Rewind the tape.
        /// </summary>
        public void Rewind()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (Tape != null) Tape.Rewind();
        }
    }
}
