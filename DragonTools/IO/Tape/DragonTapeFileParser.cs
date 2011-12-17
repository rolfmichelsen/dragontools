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

using System;

namespace RolfMichelsen.Dragon.DragonTools.IO.Tape
{
    /// <summary>
    /// Provides functionality for parsing and encoding Dragon tape files.
    /// </summary>
    public class DragonTapeFileParser : IFileParser
    {
        /// <summary>
        /// Parse a raw file and return the corresponding file object.  The raw file data is typically obtained by calling
        /// <c>IDiskFilesystem.ReadFile</c>.
        /// </summary>
        /// <param name="raw">Raw file data.</param>
        /// <returns>A file object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile Parse(byte[] raw)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parse a raw file and return the corresponding file object.  The raw file data is typically obtained by calling
        /// <c>IDiskFilesystem.ReadFile</c>.
        /// </summary>
        /// <param name="raw">Raw file data.</param>
        /// <param name="info">File meta-information from the filesystem.</param>
        /// <returns>A file object.</returns>
        /// <exception cref="FileFormatException">The file format is invalid.</exception>
        public IFile Parse(byte[] raw, FileInfo info)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Encode a file object as a raw byte stream suitable for writing to a concrete filesystem.
        /// </summary>
        /// <param name="file">File to encode.</param>
        /// <returns>Raw byte stream for file.</returns>
        /// <exception cref="FileFormatException">The file object cannot be encoded for this file system.</exception>
        public byte[] Encode(IFile file)
        {
            throw new NotImplementedException();
        }
    }
}
