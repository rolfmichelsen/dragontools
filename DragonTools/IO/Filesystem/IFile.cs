﻿/*
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
    /// Abstract representation of a file.
    /// </summary>
    public interface IFile
    {
        /// <summary>
        /// Return the file payload data.
        /// Some filesystems may support multiple data "segments" or "forks".  For those filesystems, this method will
        /// return the default data segment or fork.
        /// </summary>
        byte[] GetData();


        /// <summary>
        /// Return file information from the disk directory.
        /// This property will be <value>null</value> if the file is not associated with a directory entry.
        /// </summary>
        IFileInfo FileInfo { get; }
    }
}
