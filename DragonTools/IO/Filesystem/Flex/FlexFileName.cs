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


namespace RolfMichelsen.Dragon.DragonTools.IO.Filesystem.Flex
{
    /// <summary>
    /// Class for working with Flex filenames.
    /// </summary>
    public class FlexFileName : IFileName
    {
        /// <summary>
        /// Character used to separate the base and the extension in a Flex file name.
        /// </summary>
        private const char ExtensionSeparator = '.';

        
        /// <summary>
        /// The name of a file, omitting any directory information for hierarchical filesystems.
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// The complete path of the file.  This is equal to Name for flat filesystems.
        /// </summary>
        public string Path { get { return Name; }}


        /// <summary>
        /// The name of a file, omitting any file name extensions.
        /// </summary>
        public string Base { get; private set; }


        /// <summary>
        /// The extension of the file.
        /// </summary>
        public string Extension { get; private set; }


        /// <summary>
        /// Returns the path of the directory containing this file.  Returns <value>null</value> if this file
        /// is the root directory or the filesystem is flat.
        /// </summary>
        public string Ascend()
        {
            return null;
        }


        /// <summary>
        /// Returns the path of the child file contained in the directory represented by the current file name.
        /// </summary>
        /// <param name="child">Name of file to descend to.</param>
        /// <exception cref="InvalidOperationException">Thrown for a flat filesystem.</exception>
        public string Descend(string child)
        {
            throw new InvalidOperationException(GetType().FullName + " is not a hierarchical filesystem.");
        }

        /// <summary>
        /// True for a valid filename.
        /// </summary>
        public bool IsValid()
        {
            throw new NotImplementedException();
        }


        public FlexFileName(string name)
        {
            Name = name;
            int extsep = name.LastIndexOf(ExtensionSeparator);
            if (extsep == -1)
            {
                Base = Name;
                Extension = null;
            }
            else
            {
                Base = Name.Substring(0, extsep);
                Extension = Name.Substring(extsep + 1, Name.Length - extsep - 1);
            }
        }


        public override string ToString()
        {
            return Name;
        }
    }
}
