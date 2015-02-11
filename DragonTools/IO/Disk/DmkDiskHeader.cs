/*
Copyright (c) 2011-2015, Rolf Michelsen
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
