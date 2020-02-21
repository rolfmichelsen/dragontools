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

using System.IO;

namespace RolfMichelsen.Dragon.DragonTools.IO.Disk
{
    /// <summary>
    /// This class provides a number of convenience methods for creating Disk objects.
    /// </summary>
    public static class DiskFactory
    {
        /// <summary>
        /// Returns a Disk object associated with an existing virtual disk image file.  The name (extension) of the file name determines the 
        /// type of Disk object actually returned.
        /// </summary>
        /// <param name="filename">File name of virtual disk image.</param>
        /// <param name="iswriteable">Returns a writeable Disk object if this is <value>true</value>.</param>
        /// <returns>A Disk object associated with the given file.</returns>
        public static IDisk OpenDisk(string filename, bool iswriteable)
        {
            filename = filename.ToLowerInvariant();
            if (filename.EndsWith(".vdk"))
            {
                return VdkDisk.Open(new FileStream(filename, FileMode.Open), iswriteable);
            }
            if (filename.EndsWith(".dsk"))
            {
                return JvcDisk.Open(new FileStream(filename, FileMode.Open), iswriteable);
            }
            if (filename.EndsWith(".hfe"))
            {
                return HfeDisk.Open(new FileStream(filename, FileMode.Open), iswriteable);
            }
            if (filename.EndsWith(".dmk"))
            {
                return DmkDisk.Open(new FileStream(filename, FileMode.Open), iswriteable);
            }
            return null;
        }


        /// <summary>
        /// Creates a new virtual disk image file and returns a Disk object associated with it.  The file name extension determines the
        /// type of Disk object actually returned.
        /// </summary>
        /// <param name="filename">File name of virtual disk image.</param>
        /// <param name="heads">Number of disk heads.</param>
        /// <param name="tracks">Number of disk tracks per head.</param>
        /// <param name="sectors">Number of disk sectors per track and per head.</param>
        /// <param name="sectorsize">The size of each sector measured in bytes.</param>
        /// <returns>A Disk object associated with the given file.</returns>
        public static IDisk CreateDisk(string filename, int heads, int tracks, int sectors, int sectorsize)
        {
            filename = filename.ToLowerInvariant();
            if (filename.EndsWith(".vdk"))
            {
                return VdkDisk.Create(new FileStream(filename, FileMode.Create), heads, tracks, sectors);
            }
            if (filename.EndsWith(".dsk"))
            {
                return JvcDisk.Create(new FileStream(filename, FileMode.Create), heads, tracks, sectors, sectorsize);
            }
            if (filename.EndsWith(".hfe"))
            {
                return HfeDisk.Create(new FileStream(filename, FileMode.Create), heads, tracks, sectors, sectorsize);
            }
            if (filename.EndsWith(".dmk"))
            {
                return DmkDisk.Create(new FileStream(filename, FileMode.Create), heads, tracks, sectors, sectorsize);
            }

            return null;
        }
    }
}
