/*
Copyright (c) 2013-2014, Rolf Michelsen
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
using RolfMichelsen.Dragon.DragonTools.IO.Disk;


namespace HfeDiskInfo
{
    /// <summary>
    /// Program for displaying key information for HFE virtual diskette images.
    /// </summary>
    sealed class HfeDiskInfo
    {
        static void Main(string[] args)
        {
            var prog = new HfeDiskInfo();
            prog.Run(args);
        }



        void Run(string[] args)
        {
            if (args.Length != 1)
            {
                ShowHelp();
                return;
            }

            var diskFilename = args[0];

            DumpDiskInfo(diskFilename);
        }



        void ShowHelp()
        {
            Console.WriteLine("HfeDiskInfo - Display information from HFE virtual diskettes");
            Console.WriteLine("(C) Rolf Michelsen, 2013");
            Console.WriteLine();
            Console.WriteLine("Usage: hfediskinfo filename");
        }


        void DumpDiskInfo(string diskFilename)
        {
            using (var diskStream = new FileStream(diskFilename, FileMode.Open, FileAccess.Read))
            {
                using (var disk = HfeDisk.Open(diskStream, false))
                {
                    Console.WriteLine(diskFilename);
                    Console.WriteLine();

                    var diskHeader = disk.DiskHeader;
                    Console.WriteLine("File format version              : {0}", diskHeader.FileFormatVersion);
                    Console.WriteLine("Number of disk sides             : {0}", diskHeader.Sides);
                    Console.WriteLine("Number of tracks per side        : {0}", diskHeader.Tracks);
                    Console.WriteLine("Track encoding                   : {0}", diskHeader.TrackEncoding);
                    Console.WriteLine("Track encoding (track 0, side 0) : {0}", diskHeader.TrackEncoding0);
                    Console.WriteLine("Track encoding (track 0, side 1) : {0}", diskHeader.TrackEncoding1);
                    Console.WriteLine("Floppy interface mode            : {0}", diskHeader.FloppyInterface);
                    Console.WriteLine("Disk bit rate                    : {0}", diskHeader.DiskBitRate);
                    Console.WriteLine("Disk rotation speed              : {0}", diskHeader.DiskRotationSpeed);
                    Console.WriteLine("Disk write protected             : {0}", diskHeader.IsDiskWriteProtected);
                    Console.WriteLine("Track list block                 : {0}", diskHeader.TrackListBlock);
                    Console.WriteLine();

                    for (int track = 0; track < diskHeader.Tracks; track++)
                    {
                        for (int head = 0; head < diskHeader.Sides; head++)
                        {
                            var t = disk.GetTrack(track, head);
                            Console.WriteLine("Track {0,2}  Offset={1}  Length={2}", track, t.TrackOffset, t.TrackLength);
                            foreach (var s in t)
                            {
                                Console.WriteLine("  Sector Head={0} Track={1,-2} Sector={2,-2}  Size={3}", s.Head, s.Track, s.Sector, s.Size);
                            }                            
                        }
                    }

//                    DumpTrackData(diskStream, disk, 0, 0);
                }
            }
        }



        /// <summary>
        /// Dump raw track data after MFM encoding.
        /// </summary>
        /// <param name="diskStream">Stream for reading the disk image data.</param>
        /// <param name="disk">Disk.</param>
        /// <param name="headId">Head.</param>
        /// <param name="trackId">Track.</param>
        void DumpTrackData(Stream diskStream, HfeDisk disk, int headId, int trackId)
        {
            Console.WriteLine();
            Console.Write("Track data for track {0} side {1}", trackId, headId);
            Console.WriteLine();

            var t = disk.GetTrack(trackId, headId);

            var trackStream = new MfmStream(new HfeRawTrack(diskStream, t.TrackOffset, t.TrackLength, headId));
            var offset = 0;
            var done = false;
            var buffer = new byte[16];
            while (!done)
            {
                Console.Write(String.Format("{0:x4} : ", offset));
                var len = trackStream.Read(buffer, 0, buffer.Length);
                for (var i = 0; i < Math.Min(buffer.Length, len); i++)
                {
                    Console.Write(String.Format("{0:x2} ", buffer[i]));
                }
                Console.WriteLine();
                done = (len < buffer.Length);
                offset += len;
            }
            trackStream.Dispose();
        }
    }
}
