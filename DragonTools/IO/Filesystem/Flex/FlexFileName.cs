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
