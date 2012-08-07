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
using System.Collections.Generic;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;

namespace RolfMichelsen.Dragon.StorageTool
{
    class StorageTool
    {
        private bool verbose = false;
        private bool longformat = false;
        private DiskFilesystemIdentifier fsid = DiskFilesystemIdentifier.DragonDos;


        static void Main(string[] args)
        {
            try
            {
                var p = new StorageTool();
                p.Run(args);                
            }
            catch (NotImplementedException e)
            {
                Console.Error.WriteLine("OOPS: The function you are trying to use has not yet been implemented.");
                Console.Error.WriteLine(e);
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("ERROR: The file \"{0}\" does not exist.", e.Message);
            }
            catch (FileExistsException e)
            {
                Console.Error.WriteLine("ERROR: The file \"{0}\" already exists.", e.Message);
            }
            catch (InvalidFilenameException e)
            {
                Console.Error.WriteLine("ERROR: The filename \"{0}\" is invalid.", e.Filename);
            }
            catch (UnsupportedGeometryException e)
            {
                Console.Error.WriteLine("ERROR: The disk image file geometry is not supported.");
            }
            catch (FilesystemConsistencyException e)
            {
                Console.Error.WriteLine("ERROR: The filesystem is in an inconsistent state.");
            }
            catch (DiskImageFormatException e)
            {
                Console.Error.WriteLine("ERROR: The disk image is invalid.");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("ERROR: Unexpected exception.");
                Console.Error.WriteLine(e);
            }
        }



        private void Run(string[] args)
        {
            List<string> arglist = ParseOptions(args);

            if (arglist.Count == 0)
            {
                Console.Error.WriteLine("Use \"dst help\" for program usage information.");
                return;
            }

            var command = arglist[0];
            if (verbose) Console.WriteLine("Command " + command);
            arglist.RemoveAt(0);
            switch (command.ToLowerInvariant())
            {
                case "check":
                    CheckFilesystem(arglist);
                    break;
                case "create":
                    CreateFilesystem(arglist);
                    break;
                case "dir":
                    ListFiles(arglist);
                    break;
                case "delete":
                    DeleteFile(arglist);
                    break;
                case "free":
                    ShowFreeSpace(arglist);
                    break;
                case "freemap":
                    FreeMap(arglist);
                    break;
                case "get":
                    GetFile(arglist);
                    break;
                case "put":
                    PutFile(arglist);
                    break;
                case "readsector":
                    ReadSector(arglist);
                    break;
                case "rename":
                    RenameFile(arglist);
                    break;
                case "checksum":
                    CreateChecksum(arglist);
                    break;
                case "help":
                    ShowHelp();
                    break;
                default:
                    Console.Error.WriteLine("ERROR: Unknown command \"{0}\".", command);
                    return;
            }
        }


        private void ShowBanner()
        {
            Console.WriteLine("Dragon Storage Tools 0.0");
            Console.WriteLine("(C) Rolf Michelsen, 2011-2012");
            Console.WriteLine("www.rolfmichelsen.com");
        }


        private void ShowHelp()
        {
            ShowBanner();
            Console.WriteLine();
            Console.WriteLine("Usage: dst <command> <parameters> <options>");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  check <disk>                Check the consistency of the disk filesystem.");
            Console.WriteLine("  create <disk> h t s ss      Create new filesystem.");
            Console.WriteLine("  delete <disk> <file>        Delete <file> from the filesystem.");
            Console.WriteLine("  dir <disk>                  List all files.");
            Console.WriteLine("  free <disk>                 Show the amount of free space in the filesystem.");
            Console.WriteLine("  freemap <disk>              Show graphical representation of free sectors.");
            Console.WriteLine("  get <disk> <src> <dest>     Copy file <src> from the filesystem to a local");
            Console.WriteLine("                              file <dest> in raw mode.");
            Console.WriteLine("  put <disk> <src> <dest>     Copy a local file <src> to the filesystem using");
            Console.WriteLine("                              name <dest> in raw mode.");
            Console.WriteLine("  readsector <disk> h t s     Read a sector from the disk and dislplay it.");
            Console.WriteLine("  rename <disk> <old> <new>   Rename file <old> to <new>.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -f <type>                   Filesystem type:");
            Console.WriteLine("                                ddos     DragonDos (default)");
            Console.WriteLine("                                os9      OS-9");
            Console.WriteLine("                                flex     FLEX");
            Console.WriteLine("                                rsdos    RSDos");
            Console.WriteLine("  -l                          Long format for dir command.");
            Console.WriteLine("  -v                          Verbose mode.");
            Console.WriteLine();
        }


        internal void CreateFilesystem(List<string> args)
        {
            if (args.Count != 5)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            int heads = Convert.ToInt32(args[1]);
            int tracks = Convert.ToInt32(args[2]);
            int sectors = Convert.ToInt32(args[3]);
            int sectorsize = Convert.ToInt32(args[4]);

            var disk = DiskFactory.CreateDisk(args[0], heads, tracks, sectors, sectorsize);
            if (disk == null)
            {
                Console.Error.WriteLine("ERROR: Unknown disk format.");
                return;
            }

            var fs = DiskFilesystemFactory.OpenFilesystem(fsid, disk, true);
            if (fs == null)
            {
                throw new NotImplementedException();
            }
            fs.Initialize();
            fs.Dispose();
        }


        private void GetFile(List<string> args)
        {
            if (args.Count != 3)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            var sourcefile = args[1];
            var destfile = args[2];

            var disk = DiskFactory.OpenDisk(args[0], false);
            if (disk == null)
            {
                Console.Error.WriteLine("ERROR: Unknown disk format.");
                return;
            }

            var fs = DiskFilesystemFactory.OpenFilesystem(fsid, disk, false);
            if (fs == null)
            {
                Console.Error.WriteLine("ERROR: Unsupported filesystem {0}", fsid);
                return;
            }

            var file = fs.ReadFile(sourcefile);
            var data = file.GetData();
            
            System.IO.File.WriteAllBytes(destfile, data);

            if (verbose) Console.WriteLine("{0} bytes written to file {1}", data.Length, destfile);
        }


        private void PutFile(List<string> args)
        {
            throw new NotImplementedException();
        }


        private void ShowFreeSpace(List<string> args)
        {
            if (args.Count != 1)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }


            using (var dos = DiskFilesystemFactory.OpenFilesystem(fsid, args[0], false))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: Unable to create filesystem of type {0}", fsid);
                    return;
                }
                if (verbose) Console.WriteLine(dos);
                Console.WriteLine(dos.Free());                
            }
        }


        private void CheckFilesystem(List<string> args)
        {
            if (args.Count != 1)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }
            var dos = DiskFilesystemFactory.OpenFilesystem(fsid, args[0], false);
            if (dos == null)
            {
                Console.Error.WriteLine("ERROR: Unable to create filesystem of type {0}", fsid);
                return;
            }
            if (verbose) Console.WriteLine(dos);
            try
            {
                dos.Check();
            }
            catch (FilesystemConsistencyException e)
            {
                Console.WriteLine("The filesystem has consistency problems : {0}", e.Message);
            }
            dos.Dispose();
        }


        private void ListFiles(List<string> args)
        {
            if (args.Count != 1 && args.Count != 2)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            var disk = DiskFactory.OpenDisk(args[0], false);
            if (disk == null)
            {
                Console.Error.WriteLine("ERROR: Unable to open disk image {0}.", args[0]);
                return;
            }
            if (verbose)
            {
                Console.Error.WriteLine("Opened disk image {0} : {1}", args[0], disk);
            }

            var dos = DiskFilesystemFactory.OpenFilesystem(fsid, disk, false);
            if (dos == null)
            {
                Console.Error.WriteLine("ERROR: Unable to create filesystem of type {0}", fsid);
                return;
            }
            if (verbose) Console.WriteLine(dos);

            var dir = args.Count == 2 ? args[1] : null;
            string[] files;
            if (args.Count == 2)
            {
                if (dos is IDiskHierarchicalFilesystem)
                {
                    files = ((IDiskHierarchicalFilesystem)dos).ListFiles(args[1]);
                }
                else
                {
                    throw new NotSupportedException("Filesystem is not hierarchical");
                }
            }
            else
            {
                files = dos.ListFiles();
            }
            
            foreach (var file in files)
            {
                if (longformat)
                {
                    //TODO Avoid hardcoding of path separator character
                    var info = dos.GetFileInfo(dir == null ? file : dir + "/" + file);
                    Console.WriteLine("{0,-15}  {1,6}  {2}", file, info.Size, info.GetAttributes());   
                }
                else 
                    Console.WriteLine(file);
            }
            dos.Dispose();
        }


        private void DeleteFile(List<string> args)
        {
            if (args.Count != 2)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            using (var dos = DiskFilesystemFactory.OpenFilesystem(fsid, args[0], true))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: Unable to create filesystem of type {0}", fsid);
                    return;
                }
                if (verbose) Console.WriteLine(dos);
                dos.DeleteFile(args[1]);                
            }
        }


        internal void RenameFile(List<string> args)
        {
            if (args.Count != 3)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            using (var dos = DiskFilesystemFactory.OpenFilesystem(fsid, args[0], true))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: Unable to create filesystem of type {0}", fsid);
                    return;
                }
                if (verbose) Console.WriteLine(dos);
                dos.RenameFile(args[1], args[2]);
            }
        }


        private void ReadSector(List<string> args)
        {
            if (args.Count != 4)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            using (var disk = DiskFactory.OpenDisk(args[0], false))
            {
                if (verbose) Console.WriteLine(disk);
                int head = Convert.ToInt32(args[1]);
                int track = Convert.ToInt32(args[2]);
                int sector = Convert.ToInt32(args[3]);
                var data = disk.ReadSector(head, track, sector);
                Console.WriteLine("Head={0}  Track={1}  Sector={2}", head, track, sector);
                PrintBinaryData(data);
            }
        }



        private void PrintBinaryData(byte[] data)
        {
            int offset = 0;
            while (offset < data.Length)
            {
                var hexout = "";
                var chrout = "";
                int linelength = Math.Min(16, data.Length - offset);
                for (int i = 0; i < linelength; i++ )
                {
                    hexout += String.Format("{0:x2} ", data[offset + i]);
                    chrout += data[offset + i] < 32 ? '.' : (char) data[offset + i];
                }
                Console.WriteLine("{0:x4} : {1:-48} {2}", offset, hexout, chrout);                
                offset += linelength;
            }
        }



        /// <summary>
        /// Compute a Fletcher-16 type checksum for a file.
        /// </summary>
        /// <param name="args"></param>
        private void CreateChecksum(List<string> args)
        {
            if (args.Count != 1)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            try
            {
                var data = System.IO.File.ReadAllBytes(args[0]);
                int check1 = 0;
                int check2 = 0;
                foreach (var b in data)
                {
                    check1 = (check1 + b)%255;
                    check2 = (check2 + check1)%255;
                }
                Console.WriteLine((check2<<8) | check1);
            }
            catch (System.IO.IOException e)
            {
                Console.Error.WriteLine("ERROR: Unable to read the file \"{0}\".", args[0]);
            }
        }



        private void FreeMap(List<string> args)
        {
            if (args.Count != 1)
            {
                Console.Error.WriteLine("ERROR: Incorrect number of arguments to command.");
                return;
            }

            using (var fs = DiskFilesystemFactory.OpenFilesystem(fsid, args[0], false))
            {
                if (fs == null)
                {
                    Console.Error.WriteLine("ERROR: Unable to create filesystem of type {0}", fsid);
                    return;
                }
                if (verbose) Console.WriteLine(fs);
                for (int t = 0; t < fs.Disk.Tracks; t++)
                {
                    Console.Write("{0:d2}  ",t+1);
                    for (int h=0; h<fs.Disk.Heads; h++)
                    {
                        for (int s=0; s<fs.Disk.Sectors; s++)
                        {
                            Console.Write(fs.IsSectorAllocated(h,t,s) ? "*" : ".");
                        }
                    }
                    Console.WriteLine();
                }
            }
        }

        
        private List<string> ParseOptions(IEnumerable<string> args)
        {
            var newargs = new List<string>();

            using (var r = args.GetEnumerator())
            {
                while (r.MoveNext())
                {
                    switch (r.Current.ToLowerInvariant())
                    {
                        case "-v":
                            verbose = true;
                            break;
                        case "-l":
                            longformat = true;
                            break;
                        case "-f":
                            if (r.MoveNext())
                            {
                                fsid = ParseFilesystemId(r.Current);
                            }
                            else
                            {
                                throw new OptionException("-f", "Missing argument to -f program option");
                            }
                            break;
                        default:
                            newargs.Add(r.Current);
                            break;
                    }
                }
            }

            return newargs;
        }

        private static DiskFilesystemIdentifier ParseFilesystemId(string fsidstr)
        {
            switch (fsidstr.ToLowerInvariant())
            {
                case "ddos":
                    return DiskFilesystemIdentifier.DragonDos;
                case "os9":
                    return DiskFilesystemIdentifier.OS9;
                case "flex":
                    return DiskFilesystemIdentifier.Flex;
                case "rsdos":
                    return DiskFilesystemIdentifier.RsDos;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
