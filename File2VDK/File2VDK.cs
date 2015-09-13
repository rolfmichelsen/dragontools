/*
Copyright (c) 2013-2015, Rolf Michelsen
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
using System.Collections.Generic;
using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using RolfMichelsen.Dragon.DragonTools.Util;


namespace RolfMichelsen.Dragon.DragonTools.File2VDK
{
    sealed class File2VDK
    {

        /// <summary>
        /// Set for verbose program operation.
        /// This flag is controlled by the -v command line option.
        /// </summary>
        private bool Verbose = false;

        /// <summary>
        /// Set for program debug output.
        /// This flag is controlled by the -d command line option.
        /// </summary>
        private bool Debug = false;


        public readonly int DiskHeads = 1;
        public readonly int DiskTracks = 40;
        public readonly int DiskSectors = 18;
        public readonly int DiskSectorSize = 256;


        static void Main(string[] args)
        {
            var p = new File2VDK();
            p.Run(args);
        }



        private void Run(string[] args)
        {
            try
            {
                var files = ParseOptions(args);
                if (files.Count == 0)
                {
                    ShowUsage();
                    return;
                }

                var diskFileName = files[0] + ".vdk";
                if (Verbose)
                    Console.WriteLine("Writing output VDK disk image \"{0}\".", diskFileName);

                using (var dos = DragonDos.Initialize(DiskFactory.CreateDisk(diskFileName, DiskHeads, DiskTracks, DiskSectors, DiskSectorSize)))
                {
                    foreach (var file in files)
                    {
                        if (Verbose)
                            Console.WriteLine("Writing file \"{0}\" to disk image.", file);
                        dos.WriteFile(file, DragonDosFile.CreateDataFile(IOUtils.ReadStreamFully(new FileStream(file, FileMode.Open))));
                    }                    
                }

            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR: Unexpected error.");
                if (Debug)
                    Console.Error.WriteLine(e);
                return;
            }
        }



        /// <summary>
        /// Parse command line options.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private IList<string> ParseOptions(IEnumerable<string> args)
        {
            var newargs = new List<string>();

            using (var r = args.GetEnumerator())
            {
                while (r.MoveNext())
                {
                    switch (r.Current.ToLowerInvariant())
                    {
                        case "-d":
                            Debug = true;
                            break;
                        case "-v":
                            Verbose = true;
                            break;
                        default:
                            newargs.Add(r.Current);
                            break;
                    }
                }
            }
            return newargs;
        }



        /// <summary>
        /// Output program usage information to the console.
        /// </summary>
        private void ShowUsage()
        {
            var programinfo = new ProgramInformation();

            Console.WriteLine("{0} {1} - {2}", programinfo.ProgramName, programinfo.Version, programinfo.Description);
            Console.WriteLine("{0}", programinfo.Copyright);
            Console.WriteLine();
            Console.WriteLine("Usage: File2VDK filename {filename}");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -d   Enable debug output.");
            Console.WriteLine("  -v   Enable more verbose operation.");
            Console.WriteLine();
            Console.WriteLine("Visit www.rolfmichelsen.com/dragontools for more information.");
            Console.WriteLine();
        }


        /// <summary>
        /// Create a VDK disk image 
        /// </summary>
        /// <param name="diskName"></param>
        /// <param name="fileNames"></param>
        public void CreateVDKDisk(string diskName, IEnumerable<string> fileNames)
        {
            using (var disk = DiskFactory.CreateDisk(diskName, DiskHeads, DiskTracks, DiskSectors, DiskSectorSize))
            {
                using (var dos = DragonDos.Initialize(disk))
                {
                    foreach (var fileName in fileNames)
                    {
                        using (var localFile = new FileStream(fileName, FileMode.Open))
                        {
                            var filePayload = IOUtils.ReadStreamFully(localFile);
                            dos.WriteFile(fileName, DragonDosFile.CreateDataFile(filePayload));
                        }
                    }
                }
            }
        }
    }
}
