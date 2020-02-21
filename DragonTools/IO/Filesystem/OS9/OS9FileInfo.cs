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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    /// <summary>
    /// This class contains meta-information from the filesystem directory for a file in an OS-9 filesystem.
    /// </summary>
    public sealed class OS9FileInfo : IFileInfo
    {
        //TODO Make class immutable

        public OS9FileAttributes Attributes;

        public int OwnerId;

        public Timestamp ModifiedTime;

        public Timestamp CreateTime;

        public int LinkCount;

        public OS9FileSegment[] Segments;


        public OS9FileInfo(string name, int size, OS9FileAttributes attributes, Timestamp createdTime, Timestamp modifiedTime, int owner, int links, OS9FileSegment[] segments)
        {
            Name = name;
            Size = size;
            Attributes = attributes;
            CreateTime = createdTime;
            ModifiedTime = modifiedTime;
            OwnerId = owner;
            LinkCount = links;
            Segments = segments;
        }


        /// <summary>
        /// Create a file information object by parsing a raw file descriptor sector.
        /// </summary>
        /// <param name="raw">Array containing the data from the file descriptor sector.</param>
        /// <param name="filename">Filename.</param>
        /// <returns>File information object.</returns>
        public static OS9FileInfo Parse(byte[] raw, string filename)
        {
            if (raw == null) throw new ArgumentNullException();
            byte attributes = raw[0];
            int owner = (raw[1] << 8) | raw[2];
            int links = raw[8];
            int size = (raw[9] << 24) | (raw[10] << 16) | (raw[11] << 8) | raw[12];
            var modifiedTime = new Timestamp(raw[3] + 1900, raw[4], raw[5], raw[6], raw[7], 0);
            var createdTime = new Timestamp(raw[13] + 1900, raw[14], raw[15], 0, 0, 0);

            var segments = new List<OS9FileSegment>();
            for (int i = 0; i < 48; i++ )
            {
                var segment = new OS9FileSegment(raw, 16 + i*OS9FileSegment.SegmentSize);
                if (segment.IsLastSegment) break;
                segments.Add(segment);
            }

            return new OS9FileInfo(filename, size, (OS9FileAttributes)attributes, createdTime, modifiedTime ,owner, links, segments.ToArray());
        }


        /// <summary>
        /// Filename.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Size of file, in bytes
        /// </summary>
        public int Size { get; private set; }


        /// <summary>
        /// Indicates that this file is actually a directory.
        /// </summary>
        public bool IsDirectory { get { return ((Attributes & OS9FileAttributes.Directory) != 0); } }


        /// <summary>
        /// Returns a brief string representation of any file attributes supported by the filesystem.  Subclasses will typically
        /// override this method to provide a summary of filesystem specific file attributes not represented by the base class.
        /// </summary>
        /// <returns>String representation of file attributes.</returns>
        public string GetAttributes()
        {
            return String.Format("{0}  {1}  {2}", OS9Utils.AttributeToString(Attributes), ModifiedTime, CreateTime);
        }


        


        public override string ToString()
        {
            var segments = "";
            foreach (var seg in Segments)
            {
                segments += seg.ToString();
            }
            return String.Format("OS9FileInfo Name={0} Size={1} Attributes={2} Segments={3}", Name, Size, OS9Utils.AttributeToString(Attributes), segments);
        }

    }


    /// <summary>
    /// An OS-9 file is stored on disk as a set of segments.  A segment is a continous sequence of sectors, and it is defined
    /// by the first sector and its size.
    /// </summary>
    public class OS9FileSegment
    {
        /// <summary>
        /// The number of bytes in an encoded file segment.
        /// </summary>
        public const int SegmentSize = 5;

        /// <summary>
        /// Location (LSN) of first sector in this segment.
        /// </summary>
        public int Lsn;

        /// <summary>
        /// Number of sectors in this segment.
        /// </summary>
        public int Size;

        public OS9FileSegment(int lsn, int size)
        {
            Lsn = lsn;
            Size = size;
        }

        public OS9FileSegment(byte[] raw, int offset)
        {
            Lsn = (raw[offset] << 16) | (raw[offset + 1] << 8) | raw[offset + 2];
            Size = (raw[offset + 3] << 8) | raw[offset + 4];
        }


        public bool IsLastSegment { get { return (Lsn == 0 && Size == 0); } }


        public override string ToString()
        {
            return String.Format("(Sector={0} Size={1})", Lsn, Size);
        }
    }
}
