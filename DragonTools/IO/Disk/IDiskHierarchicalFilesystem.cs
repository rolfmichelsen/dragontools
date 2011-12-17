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
