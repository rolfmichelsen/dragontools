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
    //TODO Implement OS9FileName class and test vectors

    public class OS9FileName : IFileName
    {
        /// <summary>
        /// The name of a file, omitting any directory information for hierarchical filesystems.
        /// </summary>
        public string Name
        {
            get { throw new NotImplementedException(); }
        }


        /// <summary>
        /// The complete path of the file.  This is equal to Name for flat filesystems.
        /// </summary>
        public string Path { get; private set; }


        /// <summary>
        /// The name of a file, omitting any file name extensions.
        /// </summary>
        public string Base
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The extension of the file.
        /// </summary>
        public string Extension
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Returns the path of the directory containing this file.  Returns <value>null</value> if this file
        /// is the root directory or the filesystem is flat.
        /// </summary>
        public string Ascend()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the path of the child file contained in the directory represented by the current file name.
        /// </summary>
        /// <param name="child">Name of file to descend to.</param>
        /// <exception cref="InvalidOperationException">Thrown for a flat filesystem.</exception>
        public string Descend(string child)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// True for a valid filename.
        /// </summary>
        public bool IsValid()
        {
            throw new NotImplementedException();
        }


        public OS9FileName(string pathname)
        {
            Path = pathname;
        }


        public override string ToString()
        {
            return Path;
        }
    }
}
