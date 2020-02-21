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
    [Flags]
    public enum OS9FileAttributes : byte
    {
        Directory = 0x80,
        Shared = 0x40,
        PublicExecute = 0x20,
        PublicWrite = 0x10,
        PublicRead = 0x08,
        Execute = 0x04,
        Write = 0x02,
        Read = 0x01
    }
}
