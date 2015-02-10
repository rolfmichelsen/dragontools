/*
Copyright (c) 2011-2015, Rolf Michelsen
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
using System.Reflection;


namespace RolfMichelsen.Dragon.DragonTools.Util
{
    public class ProgramInformation
    {
        public string Version { get; private set; }

        public string ProgramName { get; private set; }

        public string Description { get; private set; }

        public string Copyright { get; private set; }

        public ProgramInformation()
        {
            var assembly = Assembly.GetEntryAssembly();

            var program = (AssemblyTitleAttribute[]) assembly.GetCustomAttributes(typeof (AssemblyTitleAttribute), false);
            ProgramName = (program.Length == 0) ? "" : program[0].Title;

            var version = (AssemblyFileVersionAttribute[]) assembly.GetCustomAttributes(typeof (AssemblyFileVersionAttribute), false);
            Version = (version.Length == 0) ? "" : version[0].Version;

            var description = (AssemblyDescriptionAttribute[]) assembly.GetCustomAttributes(typeof (AssemblyDescriptionAttribute), false);
            Description = (description.Length == 0) ? "" : description[0].Description;

            var copyright = (AssemblyCopyrightAttribute[]) assembly.GetCustomAttributes(typeof (AssemblyCopyrightAttribute), false);
            Copyright = (copyright.Length == 0) ? "" : copyright[0].Copyright;
        }
    }
}
