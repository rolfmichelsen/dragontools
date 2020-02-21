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
    /// Represents a file in an OS9 filesystem.
    /// </summary>
    public abstract class OS9File : IFile
    {
        /// <summary>
        /// Return the file payload data.
        /// </summary>
        public abstract byte[] GetData();

        /// <summary>
        /// Return file information from the disk directory.
        /// This property will be <value>null</value> if the file is not associated with a directory entry.
        /// </summary>
        public IFileInfo FileInfo { get; private set; }


        internal OS9File(OS9FileInfo fileinfo)
        {
            FileInfo = fileinfo;
        }



        /// <summary>
        /// Create an OS-9 file by parsing raw file data.
        /// </summary>
        /// <param name="fileinfo">File directory information.</param>
        /// <param name="data">Raw file data.</param>
        /// <returns></returns>
        public static OS9File CreateFile(OS9FileInfo fileinfo, byte[] data)
        {
            if (OS9ModuleFile.IsModuleFile(data))
            {
                return new OS9ModuleFile(fileinfo, data);
            }

            return new OS9DataFile(fileinfo, data);
        }
    }
}
