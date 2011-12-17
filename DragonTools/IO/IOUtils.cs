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
using System.IO;

namespace RolfMichelsen.Dragon.DragonTools.IO
{
    /// <summary>
    /// This class contains various static utlility functions needed by this library.
    /// </summary>
    public sealed class IOUtils
    {
        /// <summary>
        /// Reads all the data from a stream and returns it.  Data is read from the current position in the stream
        /// until the end.
        /// </summary>
        /// <param name="stream">Stream to read from.</param>
        /// <returns>Stream data, or <value>null</value> if the stream does not contain any data.</returns>
        public static byte[] ReadStreamFully(Stream stream)
        {
            var data = new byte[100000];
            int dataSize = 0;
            int blockSize;
            while ((blockSize = stream.Read(data, dataSize, data.Length-dataSize)) > 0)
            {
                dataSize += blockSize;
                if (data.Length == dataSize)
                    Array.Resize<byte>(ref data, dataSize+20000);
            }

            if (dataSize == 0)
                return null;

            Array.Resize<byte>(ref data, dataSize);
            return data;
        }
    }
}
