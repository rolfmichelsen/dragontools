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
    /// Provides functionality for parsing and encoding files for a specific filesystem.
    /// Subclasses provide functionality for parsing and encoding files for specific filesystems.  Parser objects are
    /// independent of IDiskFilesystem instances and can be used multiple times to parse or encode files for
    /// multuple filesystem instances.
    /// </summary>
    public interface IFileParser
    {
        /// <summary>
        /// Parse a raw file and return the corresponding file object.  The raw file data is typically obtained by calling
        /// <c>IDiskFilesystem.ReadFile</c>.
        /// </summary>
        /// <param name="raw">Raw file data.</param>
        /// <returns>A file object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        IFile Parse(byte[] raw);

        /// <summary>
        /// Parse a raw file and return the corresponding file object.  The raw file data is typically obtained by calling
        /// <c>IDiskFilesystem.ReadFile</c>.
        /// </summary>
        /// <param name="raw">Raw file data.</param>
        /// <param name="info">File meta-information from the filesystem.</param>
        /// <returns>A file object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        IFile Parse(byte[] raw, IFileInfo info);

        /// <summary>
        /// Encode a file object as a raw byte stream suitable for writing to a concrete filesystem.
        /// </summary>
        /// <param name="file">File to encode.</param>
        /// <returns>Raw byte stream for file.</returns>
        /// <exception cref="FileFormatException">The file object cannot be encoded for this file system.</exception>
        byte[] Encode(IFile file);
    }
}
