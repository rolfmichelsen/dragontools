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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.RsDos
{
    /// <summary>
    /// This class contains meta-information about a file in a RsDos filesystem.
    /// </summary>
    public class RsDosFileInfo : IFileInfo
    {
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
        public bool IsDirectory { get { return false; }}


        public RsDosFileInfo(string name, int size)
        {
            Name = name;
            Size = size;
        }


        /// <summary>
        /// Returns a brief string representation of any file attributes supported by the filesystem.  Subclasses will typically
        /// override this method to provide a summary of filesystem specific file attributes not represented by the base class.
        /// </summary>
        /// <returns>String representation of file attributes.</returns>
        public string GetAttributes()
        {
            return "";
        }
    }
}
