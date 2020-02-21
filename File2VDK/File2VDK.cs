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
using System.Collections.Generic;
using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;
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
                        dos.WriteFile(Path.GetFileName(file).ToUpper(), DragonDosFile.CreateDataFile(IOUtils.ReadStreamFully(new FileStream(file, FileMode.Open))));
                    }                    
                }

            }
            catch (DirectoryFullException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write file to filesystem.  The directory is full.");
                if (Debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (FileExistsException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write the file as a file with the same name already exists.");
                if (Debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (FilesystemFullException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write file as the filesystem is full.");
                if (Debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (InvalidFilenameException e)
            {
                Console.Error.WriteLine("ERROR: The filename {0} is invalid.", e.Filename);
                if (Debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (System.IO.IOException e)
            {
                Console.Error.WriteLine("ERROR: Local filesystem I/O error.");
                if (Debug)
                    Console.Error.WriteLine(e);
                return;
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

    }
}
