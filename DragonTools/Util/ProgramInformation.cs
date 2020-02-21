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
