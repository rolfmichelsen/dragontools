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
    /// Thrown if a file is internally inconsistent.
    /// </summary>
    public class InvalidFileException : FilesystemException
    {
        public string Filename { get; private set; }

        public InvalidFileException(string filename, string message) : base(message)
        {
            Filename = filename;
        }

        public InvalidFileException() : base()
        {
            Filename = null;
        }
    }
}
