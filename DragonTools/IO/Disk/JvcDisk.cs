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

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// Provides support for virtual disks in the JVC format.
    /// </summary>
    /// <seealso cref="http://tlindner.macmess.org/?page_id=86">Tim Lindner's JVC format documentation</seealso>
    public sealed class JvcDisk : AbstractDisk
    {
        /// <summary>
        /// Stream used for reading and writing the JVC disk image.  This stream is owned by this object and will be properly disposed of by this object.
        /// </summary>
        private Stream diskStream;

        /// <summary>
        /// The size of the JVC header in the disk image.
        /// </summary>
        private int diskHeaderSize;

        /// <summary>
        /// The size of the sector header in the disk image.
        /// </summary>
        private int sectorHeaderSize;


        private JvcDisk(Stream image, int heads, int tracks, int sectors, int sectorsize)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (heads < 1 || heads > 2) throw new ArgumentException("heads");
            if (tracks < 1) throw new ArgumentException("tracks");
            if (sectors < 1) throw new ArgumentException("sectors");
            if (sectorsize != 128 && sectorsize != 256 && sectorsize != 512 && sectorsize != 1024) throw new ArgumentException("sectorsize");
            Heads = heads;
            Tracks = tracks;
            Sectors = sectors;
            SectorSize = sectorsize;
            diskStream = image;
        }


        /// <summary>
        /// Flushes any pending write operations to the backing store.
        /// </summary>
        public override void Flush()
        {
            if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
            if (!IsModified) return;
            if (!IsWriteable) throw new DiskNotWriteableException();

            diskStream.Seek(0, SeekOrigin.Begin);
            diskStream.Write(DiskData, 0, DiskData.Length);

            IsModified = false;
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Dispose()
        {
            if (!IsDisposed)
            {
                base.Dispose();
                diskStream.Close();
                diskStream = null;                
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
            return (track * Sectors * Heads + head * Sectors + sector - 1) * (SectorSize + sectorHeaderSize) + diskHeaderSize;
        }


        /// <summary>
        /// Returns a very brief string representation of this object.
        /// </summary>
        /// <returns>String representation of this object.</returns>
        public override string ToString()
        {
            return String.Format("JVC Disk: heads={0}, tracks={1}, sectors={2}, sectorsize={3} bytes)", Heads, Tracks, Sectors, SectorSize);
        }


        /// <summary>
        /// Create a new JVC disk associated with the given stream.
        /// </summary>
        /// <param name="image">Stream for storing the disk image.</param>
        /// <param name="heads">Number of disk heads.</param>
        /// <param name="tracks">Number of tracks per head.</param>
        /// <param name="sectors">Number of sectors per track.</param>
        /// <param name="sectorsize">Sector size in bytes</param>
        /// <returns>A disk object</returns>
        public static JvcDisk Create(Stream image, int heads, int tracks, int sectors, int sectorsize)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (!image.CanRead) throw new NotSupportedException("Disk image stream does not support reading");
            if (!image.CanSeek) throw new NotSupportedException("Disk image stream does not support seeking");
            if (!image.CanWrite) throw new NotSupportedException("Disk image stream does not support writing");

            var disk = new JvcDisk(image, heads, tracks, sectors, sectorsize);

            var header = new JvcHeader(heads, tracks, sectors, sectorsize, false);
            var headerRaw = header.Encode();
            disk.diskHeaderSize = headerRaw.Length;
            disk.sectorHeaderSize = 0;
            disk.IsWriteable = true;
            disk.IsModified = true;

            disk.DiskData = new byte[heads*tracks*sectors*sectorsize + disk.diskHeaderSize];
            Array.Copy(headerRaw, disk.DiskData, headerRaw.Length);

            return disk;
        }


        /// <summary>
        /// Reads a JVC disk image from a given stream.
        /// </summary>
        /// <param name="image">Stream containing the disk image.</param>
        /// <param name="isWriteable">Allow write operations to this disk.</param>
        /// <returns>A disk object.</returns>
        public static JvcDisk Open(Stream image, bool isWriteable)
        {
            if (image == null) throw new ArgumentNullException("image");
            if (!image.CanRead) throw new NotSupportedException("Disk image stream does not support reading");

            var imageData = IOUtils.ReadStreamFully(image);
            var header = JvcHeader.Parse(imageData);

            var disk = new JvcDisk(image, header.Heads, header.Tracks, header.Sectors, header.SectorSize);
            disk.DiskData = imageData;
            disk.diskHeaderSize = header.HeaderSize;
            disk.sectorHeaderSize = header.SectorAttribute ? 1 : 0;
            disk.IsWriteable = (isWriteable && image.CanSeek && image.CanWrite);

            return disk;
        }


        /// <summary>
        /// Encapsulates the header of a JVC disk image file.
        /// </summary>
        private class JvcHeader
        {
            public const int DefaultSectors = 18;
            public const int DefaultHeads = 1;
            public const int DefaultSectorSize = 256;
            public const bool DefaultSectorAttribute = false;

            public int HeaderSize { get; private set; }
            public readonly int Heads;
            public readonly int Tracks;
            public readonly int Sectors;
            public readonly int SectorSize;
            public readonly bool SectorAttribute;
            

            public JvcHeader(int heads, int tracks, int sectors, int sectorsize, bool sectorattribute)
            {
                Heads = heads;
                Tracks = tracks;
                Sectors = sectors;
                SectorSize = sectorsize;
                SectorAttribute = sectorattribute;
            }


            public static JvcHeader Parse(byte[] raw)
            {
                int headersize = raw.Length%256;
                int sectors = headersize >= 1 ? raw[0] : DefaultSectors;
                int heads = headersize >= 2 ? raw[1] : DefaultHeads;
                int sectorsize = headersize >= 3 ? (128 << raw[2]) : DefaultSectorSize;
                bool sectorattribute = headersize >= 5 ? (raw[4] != 0) : DefaultSectorAttribute;

                if (sectorattribute)
                {
                    headersize = raw.Length % 257;
                }

                int tracks = (raw.Length - headersize)/heads/sectors/sectorsize;

                var header = new JvcHeader(heads, tracks, sectors, sectorsize, sectorattribute);
                header.HeaderSize = headersize;
                return header;
            }



            public byte[] Encode()
            {
                var header = new byte[5];
                header[0] = (byte) Sectors;
                header[1] = (byte) Heads;
                header[3] = 1;
                header[4] = (byte) (SectorAttribute ? 0xff : 0);

                switch (SectorSize) 
                {
                    case 128:
                        header[2] = 0;
                        break;
                    case 256:
                        header[2] = 1;
                        break;
                    case 512:
                        header[2] = 2;
                        break;
                    case 1024:
                        header[3] = 3;
                        break;
                    default:
                        throw new ArgumentException();
                }

                HeaderSize = header.Length;
                return header;
            }
        }

    }
}
