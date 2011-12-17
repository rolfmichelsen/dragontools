/*
Copyright (c) 2011, Rolf Michelsen
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


namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// This class encapsulates all directory information for a single file in a DragonDos filesystem.
    /// </summary>
    internal sealed class DragonDosDirectoryItem
    {
        public string Filename { get; set; }

        public bool IsProtected { get; set; }

        public int LastSectorSize { get; set; }

        public DragonDosDirectoryEntry.Extent[] Extents { get; set; }

        /// <summary>
        /// The ordered set of indexes for the directory entries describing this item.  The first directory entry is always
        /// the filename entry.
        /// </summary>
        public int[] DirEntries { get; set; }

        public int Size
        {
            get 
            { 
                int sectors = 0;
                foreach (var extent in Extents)
                {
                    sectors += extent.Length;
                }
                return sectors == 0 ? 0 : (sectors - 1)*256 + (LastSectorSize == 0 ? 256 : LastSectorSize);
            }
        }

        public DragonDosDirectoryItem()
        {
            Filename = null;
            IsProtected = false;
            LastSectorSize = 0;
            Extents = null;
        }


    }
}
