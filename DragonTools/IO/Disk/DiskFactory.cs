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
