/*
Copyright (c) 2011-2012, Rolf Michelsen
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

namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.OS9
{
    /// <summary>
    /// Encapsulates the OS9 disk allocation map.  The allocation map maintains information
    /// about which filesystem clusters are currently in used by the filesystem.
    /// </summary>
    public sealed class OS9AllocationMap
    {
        private byte[] allocationMap;


        /// <summary>
        /// The size of the cluster allocation map (in bytes).
        /// </summary>
        public int AllocationMapSize { get; private set; }


        /// <summary>
        /// Create an allocation map and initialize it from the encoded format used on disk.
        /// The allocation map is usually created by just passing the data from the allocation map
        /// sector directly.
        /// </summary>
        /// <param name="raw">Encoded allocation map.</param>
        /// <param name="size">Size in bytes of the allocation map.</param>
        public OS9AllocationMap(byte[] raw, int size)
        {
            if (raw == null) throw new ArgumentNullException("raw");
            if (size < 0 || size > raw.Length) throw new ArgumentOutOfRangeException("size");

            allocationMap = raw;
            AllocationMapSize = size;
        }


        /// <summary>
        /// Create an allocation map and mark all clusters av unused by the filesystem.
        /// </summary>
        /// <param name="size">Size of the allocation map in bytes.</param>
        public OS9AllocationMap(int size)
        {
            allocationMap = new byte[size];
            Array.Clear(allocationMap, 0, size);
            AllocationMapSize = size;
        }


        /// <summary>
        /// Returns the allocation status of a given filesystem cluster.
        /// </summary>
        /// <param name="cluster">Cluster.</param>
        /// <returns><value>true</value> if the cluster is marked as allocated by the filesystem.</returns>
        public bool IsAllocated(int cluster)
        {
            return ((allocationMap[cluster/8] & (1 << (7-cluster%8))) != 0);
        }


        /// <summary>
        /// Set the allocation status of a given filesystem cluster.
        /// </summary>
        /// <param name="cluster">Cluster.</param>
        /// <param name="isallocated">The cluster is marked as allocated by the filesystem when this parameter is <value>true</value>.</param>
        public void SetAllocated(int cluster, bool isallocated)
        {
            if (isallocated)
            {
                allocationMap[cluster/8] |= (byte) (1 << (7-cluster%8));
            }
            else
            {
                allocationMap[cluster/8] &= (byte) ~(1 << (7-cluster%8));
            }
        }
    
    }
}
