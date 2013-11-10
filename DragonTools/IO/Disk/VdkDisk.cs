/*
Copyright (c) 2011-2013, Rolf Michelsen
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

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Provides support for a virtual disk represented in the VDK format.
    /// </summary>
    public sealed class VdkDisk : AbstractDisk
    {
        /// <summary>
        /// Virtual disks in the VDK format has a fixed sector size.
        /// </summary>
        private const int VdkSectorSize = 256;

        /// <summary>
        /// The default size of the VDK file header.
        /// </summary>
        private const int VdkDefaultHeaderSize = 12;

        /// <summary>
        /// Stream used for reading and writing the VDK disk image.  This stream is owned by this object and will be properly disposed of by this object.
        /// </summary>
        private Stream vdkDiskStream;

        /// <summary>
        /// The size of the VDK header.
        /// </summary>
        private int vdkHeaderSize;


        /// <summary>
        /// Flushes any pending write operations to the backing store.
        /// </summary>
        public override void Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (!IsModified) return;
            if (!IsWriteable) throw new DiskNotWriteableException();

            vdkDiskStream.Seek(0, SeekOrigin.Begin);
            vdkDiskStream.Write(DiskData, 0, DiskData.Length);

            IsModified = false;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (!IsDisposed)
            {
                base.Dispose();
                vdkDiskStream.Close();
                vdkDiskStream = null;                
            }
        }


        /// <summary>
        /// Returns the offset into the <see cref="AbstractDisk.DiskData">diskData</see> array of the first byte of the identified sector.
        /// </summary>
        /// <param name="head">Disk head.</param>
        /// <param name="track">Disk track.</param>
        /// <param name="sector">Disk sector.</param>
        /// <returns>The byte offset of the first byte of this sector.</returns>
        protected override int SectorOffset(int head, int track, int sector)
        {
            return (track * Sectors * Heads + head * Sectors + sector - 1) * SectorSize + vdkHeaderSize;
        }


        /// <summary>
        /// Returns a very brief string representation of this object.
        /// </summary>
        /// <returns>String representation of this object.</returns>
        public override string ToString()
        {
            return String.Format("VDK Disk: heads={0}, tracks={1}, sectors={2}, sectorsize={3} bytes, header={4} bytes)", Heads, Tracks, Sectors, SectorSize, vdkHeaderSize);
        }


        private VdkDisk(Stream image, int heads, int tracks, int sectors) 
        {
            if (heads < 1 || heads > 2) throw new ArgumentOutOfRangeException("heads", heads, "VDK disks only supports 1 or 2 heads");
            if (tracks < 1 || tracks > 255) throw new ArgumentOutOfRangeException("tracks", tracks, "VDK disks only supports 1-255 tracks per head");
            Heads = heads;
            Tracks = tracks;
            Sectors = sectors;
            SectorSize = VdkSectorSize;
            vdkDiskStream = image;
        }


        /// <summary>
        /// Create a new VDK disk associated with the given stream.
        /// </summary>
        /// <param name="image">Stream for storing the disk image.</param>
        /// <param name="heads">Number of disk heads.</param>
        /// <param name="tracks">Number of tracks per head.</param>
        /// <param name="sectors">Number of sectors per track.</param>
        /// <returns>A disk object</returns>
        public static VdkDisk Create(Stream image, int heads, int tracks, int sectors)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (!image.CanRead) throw new NotSupportedException("Disk image stream does not support reading");
            if (!image.CanSeek) throw new NotSupportedException("Disk image stream does not support seeking");
            if (!image.CanWrite) throw new NotSupportedException("Disk image stream does not support writing");

            var disk = new VdkDisk(image, heads, tracks, sectors);
            var header = new VdkHeader(heads, tracks);
            var headerRaw = header.Encode();
            disk.vdkHeaderSize = headerRaw.Length;
            disk.DiskData = new byte[disk.vdkHeaderSize + heads * tracks * sectors * VdkSectorSize];
            Array.Copy(headerRaw, disk.DiskData, disk.vdkHeaderSize);
            disk.IsWriteable = true;
            disk.IsModified = true;

            return disk;
        }

        /// <summary>
        /// Reads a VDK disk image from a given stream.
        /// </summary>
        /// <param name="image">Stream containing the VDK disk image.</param>
        /// <param name="isWriteable">Allow write operations to this disk.</param>
        /// <returns>A disk object.</returns>
        public static VdkDisk Open(Stream image, bool isWriteable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (!image.CanRead) throw new NotSupportedException("Disk image stream does not support reading");

            var imageData = IOUtils.ReadStreamFully(image);
            var header = VdkHeader.Parse(imageData);
            int sectors = (imageData.Length - header.HeaderSize)/(header.Heads*header.Tracks*VdkSectorSize);

            var disk = new VdkDisk(image, header.Heads, header.Tracks, sectors);
            disk.DiskData = imageData;
            disk.vdkHeaderSize = header.HeaderSize;
            disk.IsWriteable = (isWriteable && image.CanSeek && image.CanWrite);

            return disk;
        }



        /// <summary>
        /// Encapsulates the header of a VDK virtual disk file.
        /// </summary>
        private class VdkHeader
        {
            public const int VdkVersion = 0x10;
            public const int VdkCompatibilityVersion = 0x10;
            public const int VdkSourceId = 0;
            public const int VdkSourceVersion = 0;

            public readonly int HeaderSize;
            public readonly int Heads;
            public readonly int Tracks;

            public VdkHeader(int heads, int tracks)
            {
                HeaderSize = VdkDefaultHeaderSize;
                Heads = heads;
                Tracks = tracks;
            }


            public VdkHeader(int heads, int tracks, int headerSize)
            {
                HeaderSize = headerSize;
                Heads = heads;
                Tracks = tracks;
            }


            /// <summary>
            /// Encodes the VDK header and returns it as a byte array.
            /// </summary>
            /// <returns>Encoded VDK header.</returns>
            public byte[] Encode()
            {
                var header = new byte[HeaderSize];
                header[0] = (byte) 'd';
                header[1] = (byte) 'k';
                header[2] = (byte) (HeaderSize & 0xff);
                header[3] = (byte) ((HeaderSize >> 8) & 0xff);
                header[4] = (byte) VdkVersion;
                header[5] = (byte) VdkCompatibilityVersion;
                header[6] = (byte) VdkSourceId;
                header[7] = (byte) VdkSourceVersion;
                header[8] = (byte) Tracks;
                header[9] = (byte) Heads;
                header[10] = 0;
                header[11] = 0;
                return header;
            }

            /// <summary>
            /// Creates a VDK header object by parsing a byte array.
            /// </summary>
            /// <remarks>
            /// The parser ignores a number of VDK header fields.
            /// </remarks>
            /// <param name="raw">The buffer containing the encoded VDK header.</param>
            /// <returns>A VDK header object.</returns>
            public static VdkHeader Parse(byte[] raw)
            {
                if (raw == null) throw new ArgumentNullException("raw");
                if (raw.Length < VdkDefaultHeaderSize) throw new DiskImageFormatException("Disk image format too small to contain a VDK header");

                if (raw[0] != 'd' || raw[1] != 'k') throw new DiskImageFormatException("VDK image header does not contain magic cookie");

                int headerSize = raw[2] + raw[3]*256;
                if (headerSize < VdkDefaultHeaderSize) throw new DiskImageFormatException("VDK header size is too small");

                int tracks = raw[8];
                if (tracks < 1) throw new DiskImageFormatException("VDK header specifies invalid number of disk tracks");

                int heads = raw[9];
                if (heads < 1 || heads > 2) throw new DiskImageFormatException("VDK header specifies invalid number of disk heads");

                var header = new VdkHeader(heads, tracks, headerSize);
                return header;
            }
        }
    }
}
