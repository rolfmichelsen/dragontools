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
