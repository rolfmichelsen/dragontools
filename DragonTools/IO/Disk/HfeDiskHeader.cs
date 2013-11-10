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
    /// The file header for a HFE virtual disk file.
    /// </summary>
    public sealed class HfeDiskHeader
    {
        /// <summary>
        /// Magic number used to identify HFE disk images.
        /// </summary>
        private static readonly byte[] HeaderSignature = { 0x48, 0x58, 0x43, 0x50, 0x49, 0x43, 0x46, 0x45 };

        public int FileFormatVersion { get; private set; }
        
        public int Tracks { get; private set; }
        
        public int Sides { get; private set; }
        
        public TrackEncodingMode TrackEncoding { get; private set; }
        public TrackEncodingMode TrackEncoding0 { get; private set; }
        public TrackEncodingMode TrackEncoding1 { get; private set; }

        public int DiskBitRate { get; private set; }
        
        public int DiskRotationSpeed { get; private set; }
        
        public FloppyInterfaceMode FloppyInterface { get; private set; }

        /// <summary>
        /// Offset of block containing the disk track list.
        /// </summary>
        public int TrackListBlock { get; private set; }

        public bool IsDiskWriteProtected { get; private set; }

        public bool IsSingleStepMode { get; private set; }



        /// <summary>
        /// Create a HFE disk header object by parsing raw HFE disk diskImage data.
        /// </summary>
        /// <param name="diskImage">The first 512 bytes from the HFE disk diskImage.</param>
        public HfeDiskHeader(byte[] diskImage)
        {
            if (diskImage == null) throw new NullReferenceException("diskImage");
            Decode(diskImage);
        }


        /// <summary>
        /// Create a HFE dik header object by reading the encoded HFE header from the stream.
        /// </summary>
        /// <param name="diskImageStream">Stream for reading the HFE disk image.</param>
        public HfeDiskHeader(Stream diskImageStream)
        {
            if (diskImageStream == null) throw new ArgumentNullException("diskImageStream");
            var headerEncoded = new byte[HfeDisk.BlockSize];
            diskImageStream.Seek(0, SeekOrigin.Begin);
            IOUtils.ReadBlock(diskImageStream, headerEncoded, 0, headerEncoded.Length);
            Decode(headerEncoded);
        }



        /// <summary>
        /// Create a HFE disk header object by parsing raw HFE disk diskImage data.
        /// </summary>
        /// <param name="diskImage">The first 512 bytes from the HFE disk diskImage.</param>
        private void Decode(byte[] diskImage)
        {
            if (diskImage.Length < 26) throw new ArgumentException("diskImage too small to contain a complete HFE disk header");

            // Verify header signature
            int offset = 0;
            for (var i = 0; i < HeaderSignature.Length; i++, offset++)
            {
                if (diskImage[offset] != HeaderSignature[i]) throw new ArgumentException("Invalid HFE disk header signature");
            }

            // Parse header fields
            FileFormatVersion = diskImage[offset++];
            Tracks = diskImage[offset++];
            Sides = diskImage[offset++];
            TrackEncoding = (TrackEncodingMode) diskImage[offset++];
            DiskBitRate = diskImage[offset++] | (diskImage[offset++] << 8);
            DiskRotationSpeed = diskImage[offset++] | (diskImage[offset++] << 8);
            FloppyInterface = (FloppyInterfaceMode) diskImage[offset++];
            offset++;
            TrackListBlock = diskImage[offset++] | (diskImage[offset++] << 8);
            IsDiskWriteProtected = (diskImage[offset++] != 0);
            IsSingleStepMode = (diskImage[offset++] != 0);
            TrackEncoding0 = (diskImage[offset] == 0 ? (TrackEncodingMode)(diskImage[offset + 1] | (diskImage[offset + 2] << 8)) : TrackEncoding);
            offset += 3;
            TrackEncoding1 = (diskImage[offset] == 0 ? (TrackEncodingMode)(diskImage[offset + 1] | (diskImage[offset + 2] << 8)) : TrackEncoding);

            // Validate header fields
            if (FileFormatVersion != 0) throw new ArgumentException("Unsupported file format version");
            if (!Enum.IsDefined(typeof(TrackEncodingMode), TrackEncoding)) throw new ArgumentException("Invalid track encoding mode");
            if (!Enum.IsDefined(typeof(TrackEncodingMode), TrackEncoding0)) throw new ArgumentException("Invalid track encoding mode (track 1, side 0)");
            if (!Enum.IsDefined(typeof(TrackEncodingMode), TrackEncoding1)) throw new ArgumentException("Invalid track encoding mode (track 1, side 1)");
            if (!Enum.IsDefined(typeof(FloppyInterfaceMode), FloppyInterface)) throw new ArgumentException("Invalid floppy interface mode");
        }



        public enum TrackEncodingMode
        {
            ISOIBM_MFM = 0x00,
            AMIGA_MFM = 0x01,
            ISOIBM_FM = 0x02,
            EMU_FM = 0x03,
            UNKNOWN = 0xff
        }


        public enum FloppyInterfaceMode
        {
            IBMPC_DD = 0x00,
            IBMPC_HD = 0x01,
            ATARIST_DD = 0x02,
            ATARIST_HD = 0x03,
            AMIGA_DD = 0x04,
            AMIGA_HD = 0x05,
            CPC_DD = 0x06,
            GENERIC_SHUGART_DD = 0x07,
            IBMPC_ED = 0x08,
            MSX2_DD = 0x09,
            C64_DD = 0x0a,
            EMU_SHUGART = 0x0b,
            S950_DD = 0x0c,
            S950_HD = 0x0d,
            DISABLED = 0xfe
        }

    }
}
