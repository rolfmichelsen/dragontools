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
    /// The file header for a DMK virtual diskette.
    /// </summary>
    public sealed class DmkDiskHeader
    {
        /// <summary>
        /// Size (in bytes) of the encoded disk header.
        /// </summary>
        private readonly int EncodedHeaderSize = 16;

        /// <summary>
        /// Indicates that the disk supports write operations.
        /// </summary>
        public bool IsWriteable { get; private set; }

        /// <summary>
        /// Indicates that the disk is single density.
        /// </summary>
        public bool IsSingleDensity { get; private set; }

        /// <summary>
        /// When set, density information is to be ignored when accessing the disk.
        /// </summary>
        public bool IgnoreDensity { get; private set; }

        /// <summary>
        /// Number of disk tracks.
        /// </summary>
        public int Tracks { get; private set; }

        /// <summary>
        /// Number of disk heads.  The DMK format only supports 1 or 2 sides.
        /// </summary>
        public int Heads { get; private set; }

        /// <summary>
        /// The encoded length (in bytes) of a track in the virtual disk image.
        /// </summary>
        public int EncodedTrackLength { get; private set; }


        /// <summary>
        /// Create a DMK dik header object by reading the encoded DMK header from the stream.
        /// </summary>
        /// <param name="diskImageStream">Stream for reading the disk image.</param>
        public DmkDiskHeader(Stream diskImageStream)
        {
            if (diskImageStream == null) throw new ArgumentNullException("diskImageStream");
            var headerEncoded = new byte[EncodedHeaderSize];
            diskImageStream.Seek(0, SeekOrigin.Begin);
            IOUtils.ReadBlock(diskImageStream, headerEncoded, 0, headerEncoded.Length);
            Decode(headerEncoded);
        }


        /// <summary>
        /// Decode an encoded disk header and populates the object attributes.
        /// </summary>
        /// <param name="encodedHeader">Byte array containing the encoded disk header.</param>
        private void Decode(byte[] encodedHeader)
        {
            if (encodedHeader.Length < EncodedHeaderSize) throw new ArgumentException("Incomplete header");

            IsWriteable = (encodedHeader[0] == 0);
            Tracks = encodedHeader[1];
            EncodedTrackLength = encodedHeader[2] | (encodedHeader[3] << 8);
            Heads = ((encodedHeader[4] & 0x10) == 0) ? 2 : 1;
            IsSingleDensity = ((encodedHeader[4] & 0x40) != 0);
            IgnoreDensity = ((encodedHeader[4] & 0x80) != 0);
        }
    }
}
