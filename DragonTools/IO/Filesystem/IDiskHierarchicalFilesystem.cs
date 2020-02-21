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

using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9;

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem
{
    /// <summary>
    /// Abstract representation of a disk based filesystem with a hierarchical directory structure.  
    /// Subclasses provide support for actual filesystems.
    /// </summary>
    /// <see cref="OS9"/>
    public interface IDiskHierarchicalFilesystem : IDiskFilesystem
    {
        /// <summary>
        /// Returns a list of all files in a given filesystem directory.
        /// </summary>
        /// <param name="directory">The directory to list, or <value>null</value> for the root directory.</param>
        /// <returns>A list containing the filenames of all files in the given directory.</returns>
        string[] ListFiles(string directory);


        /// <summary>
        /// Create a new directory.
        /// </summary>
        /// <param name="directory">Name of directory to create.</param>
        void CreateDirectory(string directory);


        /// <summary>
        ///  Remove a directory.  The directory must be empty.
        /// </summary>
        /// <param name="directory">Name of directory to remove.</param>
        void DeleteDirectory(string directory);

    }
}
