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


        /// <summary>
        /// Read a block of data of specific size from a stream.
        /// </summary>
        /// <param name="block">Byte buffer to receive the data.</param>
        /// <param name="offset">Offset into to buffer of where to store the first byte.</param>
        /// <param name="length">Number of bytes to read from the stream.</param>
        /// <exception cref="EndOfStreamException">Thrown if the required number of bytes cannot be read from the stream.</exception>
        public static void ReadBlock(Stream stream, byte[] block, int offset, int length)
        {
            while (length > 0)
            {
                var chunksize = stream.Read(block, offset, length);
                if (chunksize == 0) throw new EndOfStreamException();
                offset += chunksize;
                length -= chunksize;
            }
        }

    }

}
