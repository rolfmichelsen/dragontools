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
using System.ComponentModel;
using System.IO;


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Provide access to raw HFE disk track data for a single side.  This class abstracts
    /// away the block structure of the HFE disk format.
    /// </summary>
    /// 
    /// <remarks>
    /// A HFE disk track is represented as a sequence of 512 byte blocks. The first 256 bytes of each block
    /// represents data on disk side 1, and the last 256 bytes of a block represents data on side 2.  The constructor
    /// reads the data for a single disk side into memory, and subsequent operations on the track uses this in memory
    /// buffer.  Flush will write any modified in memory data back to the underlying stream.  The underlying stream
    /// is only accessed in the constructor and by Flush.
    /// 
    /// A special constructor is used for initializing a track in a new HFE disk image.  This constructor does not read
    /// track data from disk and initializes an empty in memory buffer.  Track write operations are allowed to extend
    /// the size of this buffer.
    /// </remarks>
    public sealed class HfeRawTrack : Stream
    {

        /// <summary>
        /// Stream for accessing the disk image.
        /// </summary>
        private Stream diskStream;


        /// <summary>
        /// Offset of the beginning of this track in <see cref="diskStream"/>.
        /// </summary>
        private readonly long trackOffset;


        /// <summary>
        /// Length of this track (both sides) in <see cref="diskStream"/>.  The value is 0 when a track is being initialized and only
        /// set when Flush is called.
        /// </summary>
        public int TrackLength { get; private set; }


        /// <summary>
        /// Identifies the disk head of this track.
        /// </summary>
        private readonly int head;


        /// <summary>
        /// Buffer containing the track data for the specified disk side.
        /// </summary>
        private byte[] trackData;


        /// <summary>
        /// Length of the track data buffer.
        /// </summary>
        private int trackDataLength;


        /// <summary>
        /// Read/write position within the stream, or track data buffer.
        /// </summary>
        private int trackDataPosition;


        /// <summary>
        /// Set to true to indicate that <see cref="trackData"/> has been modified and the modifications
        /// have not been written back to the stream.
        /// </summary>
        private bool trackDataDirty = false;


        /// <summary>
        /// Initial size of track data buffer when initializing a new track.
        /// </summary>
        private readonly int defaultTrackDataLength = 16384;


        /// <summary>
        /// Set when the object has been disposed.
        /// </summary>
        private bool isDisposed = false;


        /// <summary>
        /// Create a stream for accessing raw HFE disk track data.
        /// </summary>
        /// <param name="diskStream">Stream for reading the HFE disk file.</param>
        /// <param name="trackOffset">Offset of the first byte of the track.</param>
        /// <param name="trackLength">Length of the track.</param>
        /// <param name="head">Disk head.</param>
        public HfeRawTrack(Stream diskStream, long trackOffset, int trackLength, int head)
        {
            if (diskStream == null) throw new ArgumentNullException("diskStream");
            if (!diskStream.CanRead) throw new NotSupportedException("Stream does not support reading");
            if (!diskStream.CanSeek) throw new NotSupportedException("Stream does not support seeking");
            if (head<0 || head>1) throw new ArgumentOutOfRangeException("head", head, "Only 1 or 2 heads supported");
            
            this.diskStream = diskStream;
            this.trackOffset = trackOffset;
            this.TrackLength = trackLength;
            this.head = head;
            
            ReadTrack();
        }


        /// <summary>
        /// Create a stream for creating a raw HFE disk track.  This constructor is typically only used when first creating
        /// a HFE virtual disk image.  
        /// </summary>
        /// <param name="diskStream">Stream for writing the HFE disk file.</param>
        /// <param name="trackOffset">Offset of the first byte of the track.</param>
        /// <param name="head">Disk head.</param>
        public HfeRawTrack(Stream diskStream, long trackOffset, int head)
        {
            if (diskStream == null) throw new ArgumentNullException("diskStream");
            if (!diskStream.CanRead) throw new NotSupportedException("Stream does not support reading");
            if (!diskStream.CanSeek) throw new NotSupportedException("Stream does not support seeking");
            if (!diskStream.CanWrite) throw new NotSupportedException("Stream does not support writing");
            if (head < 0 || head > 1) throw new ArgumentOutOfRangeException("head", head, "Only 1 or 2 heads supported");

            this.diskStream = diskStream;
            this.trackOffset = trackOffset;
            this.TrackLength = 0;
            this.head = head;

            trackData = new byte[defaultTrackDataLength];
            trackDataLength = 0;
            trackDataPosition = 0;
        }



        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            WriteTrack();
            diskStream.Flush();
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
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0 || offset>= trackDataLength) throw new EndOfStreamException();
                    trackDataPosition = (int) offset;
                    break;
                case SeekOrigin.Current:
                    var newPosition = trackDataPosition + offset;
                    if (newPosition < 0 || newPosition >= trackDataLength) throw new EndOfStreamException();
                    trackDataPosition = (int) newPosition;
                    break;
                case SeekOrigin.End:
                    newPosition = trackDataLength + offset;
                    if (newPosition < 0 || newPosition >= trackDataLength) throw new EndOfStreamException();
                    trackDataPosition = (int) newPosition;
                    break;
                default:
                    throw new InvalidEnumArgumentException("origin", (int) origin, origin.GetType());
            }
            return trackDataPosition;
        }


        /// <summary>
        /// This operation is not supported by this class as the track length is specified in the constructor and
        /// cannot be changed.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.NotSupportedException">Operation not supported by this class.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        /// Reads a sequence of bytes from the stream and advances the position within the stream by the number of bytes read.
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
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (isDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (buffer == null) throw new ArgumentNullException("buffer");
            int readCount = 0;
            int readValue;
            while (readCount < count && (readValue = ReadByte()) != -1)
            {
                buffer[offset++] = (byte) readValue;
                readCount++;
            }
            return readCount;
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
            return (trackDataPosition == trackDataLength) ? -1 : trackData[trackDataPosition++];
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
            while (count-- > 0)
            {
                WriteByte(buffer[offset++]);
            }
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
            if (!CanWrite) throw new NotSupportedException("Stream does not support writing");
            if (trackDataPosition == trackDataLength && TrackLength > 0) throw new EndOfStreamException("Writing past end of track");
            if (trackDataPosition == trackData.Length) throw new EndOfStreamException("Writing past end of track");
            trackData[trackDataPosition++] = value;
            trackDataLength = Math.Max(trackDataLength, trackDataPosition);
            trackDataDirty = true;

            // TODO Support increasing the size of the trackData buffer when initializing a new track (tracklength = 0).
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// </returns>
        public override bool CanRead
        {
            get { return true; }
        }


        /// <summary>
        /// Return <value>true</value> to indicate that this stream supports seeking.
        /// </summary>
        /// <returns>
        /// </returns>
        public override bool CanSeek
        {
            get { return true; }
        }


        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// </returns>
        public override bool CanWrite
        {
            get { return diskStream.CanWrite; }
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
            get { return trackDataLength; }
        }


        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking.</exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
        public override long Position
        {
            get { return trackDataPosition; } 
            set { Seek(value, SeekOrigin.Begin); }
        }


        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                Flush();
                diskStream = null;
            }

            base.Dispose(disposing);
            isDisposed = true;
        }


        /// <summary>
        /// Read the track data into the <see cref="trackData">trackData</see> buffer and sets
        /// <see cref="trackDataLength"/> and <see cref="trackDataPosition"/>.  The actual size of the buffer may be
        /// different from <see cref="trackDataLength"/>.
        /// </summary>
        private void ReadTrack()
        {
            var blockOffset = head*HfeDisk.BlockSize/2;
            var blockBuffer = new byte[HfeDisk.BlockSize/2];
            var trackBuffer = new byte[TrackLength];
            trackDataLength = 0;
            trackDataPosition = 0;
            trackDataDirty = false;

            while (blockOffset < TrackLength)
            {
                var blockSize = Math.Min(HfeDisk.BlockSize/2, TrackLength - blockOffset);
                diskStream.Position = trackOffset + blockOffset;
                IOUtils.ReadBlock(diskStream, blockBuffer, 0, blockSize);
                Array.Copy(blockBuffer, 0, trackBuffer, trackDataLength, blockSize);
                trackDataLength += blockSize;
                blockOffset += HfeDisk.BlockSize;
            }

            trackData = new byte[trackDataLength];
            Array.Copy(trackBuffer, trackData, trackDataLength);
        }


        /// <summary>
        /// Write the track data back to the disk image.
        /// </summary>
        private void WriteTrack()
        {
            if (!trackDataDirty) return;

            var blockOffset = head*HfeDisk.BlockSize/2;
            var trackDataOffset = 0;

            while (trackDataOffset < trackDataLength)
            {
                var blockSize = Math.Min(HfeDisk.BlockSize/2, trackDataLength - trackDataOffset);
                diskStream.Position = trackOffset + blockOffset;
                diskStream.Write(trackData, trackDataOffset, blockSize);
                blockOffset += HfeDisk.BlockSize;
                trackDataOffset += HfeDisk.BlockSize/2;
            }

            trackDataDirty = false;
            if (TrackLength == 0)
                TrackLength = (int) diskStream.Position;
        }
    }
}
