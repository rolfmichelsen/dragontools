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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos
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
