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



namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem
{
    /// <summary>
    /// Interface for accessing limited meta-information about a file.  A filesystem will typically
    /// provide an implementation to access file meta-information maintained in the filesystem
    /// directory through this interface.
    /// </summary>
    public interface IFileInfo
    {
        /// <summary>
        /// Filename.
        /// </summary>
        string Name { get; }


        /// <summary>
        /// Size of file, in bytes
        /// </summary>
        int Size { get; }

        
        /// <summary>
        /// Indicates that this file is actually a directory.
        /// </summary>
        bool IsDirectory { get; }


        /// <summary>
        /// Returns a brief string representation of any file attributes supported by the filesystem.  Subclasses will typically
        /// override this method to provide a summary of filesystem specific file attributes not represented by the base class.
        /// </summary>
        /// <returns>String representation of file attributes.</returns>
        string GetAttributes();

    }
}
