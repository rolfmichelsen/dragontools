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
