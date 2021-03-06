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

using RolfMichelsen.Dragon.DragonTools.Basic;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem;
using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonDos;
using RolfMichelsen.Dragon.DragonTools.IO.Filesystem.DragonTape;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;
using RolfMichelsen.Dragon.DragonTools.Util;
using FileNotFoundException = RolfMichelsen.Dragon.DragonTools.IO.Filesystem.FileNotFoundException;


namespace RolfMichelsen.Dragon.DragonTools.DragonDosTools
{
    /// <summary>
    /// Program for accessing and manipulation of virtual DragonDos filesystems.
    /// </summary>
    public sealed class DragonDosTools
    {
        /// <summary>
        /// The default number of disk tracks  when creating a DragonDos filesystem.
        /// </summary>
        private const int DefaultTracks = 40;

        /// <summary>
        /// The default number of sectors per track and per head when creating a DragonDos filesystem.
        /// </summary>
        private const int DefaultSectors = 18;

        /// <summary>
        /// The defaut sector size (in bytes) when creating a DragonDos filesystem.
        /// </summary>
        private const int DefaultSectorSize = 256;
        
        /// <summary>
        /// Set for verbose program operation.
        /// This flag is controlled by the -v command line option.
        /// </summary>
        private bool verbose = false;


        /// <summary>
        /// Set for program debug output.
        /// This flag is controlled by the -d command line option.
        /// </summary>
        private bool debug = false;

        private bool raw = false;

        /// <summary>
        /// Set when certain operations should produce ASCII output.
        /// This flag is controlled by the -ascii command line option.
        /// </summary>
        private bool ascii = false;

        /// <summary>
        /// Manually specified file type when writing files to a DragonDos filesystem.
        /// Holds the file type when writing files using the WRITE command.
        /// </summary>
        private FileType filetype = FileType.Data;


        /// <summary>
        /// Manually specified load address for machine code programs.
        /// </summary>
        private int LoadAddress = 0;


        /// <summary>
        /// Manually specified execution start address for machine code programs.
        /// </summary>
        private int StartAddress = 0;



        static void Main(string[] args)
        {
            var p = new DragonDosTools();
            p.Run(args);
        }


        private void Run(string[] args)
        {

            try
            {
                var commands = ParseOptions(args);

                if (commands.Count == 0)
                {
                    ShowUsage();
                    return;
                }
                
                var command = commands[0].ToLowerInvariant();
                commands.RemoveAt(0);
                switch (command)
                {
                    case "check":
                        CheckFilesystem(commands);
                        break;
                    case "create":
                        CreateFilesystem(commands);
                        break;
                    case "delete":
                        DeleteFile(commands);
                        break;
                    case "dir":
                        ListDirectory(commands);
                        break;
                    case "dump":
                        DumpSector(commands);
                        break;
                    case "freemap":
                        Freemap(commands);
                        break;
                    case "read":
                        ReadFile(commands);
                        break;
                    case "write":
                        WriteFile(commands);
                        break;
                    default:
                        Console.Error.WriteLine("ERROR: Unknown command {0}.", command);
                        break;
                }        
                
            }
            catch (CommandArgumentsException e)
            {
                Console.Error.WriteLine("ERROR: Command line argument error.");
                if (debug)
                    Console.WriteLine(e);
                return;
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("ERROR: The file does not exist.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (DirectoryFullException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write file to filesystem.  The directory is full.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (FileExistsException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write the file as a file with the same name already exists.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (FilesystemConsistencyException e)
            {
                Console.Error.WriteLine("ERROR: Cannot complete operation as the filesystem is corrupt.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (FilesystemFullException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write file as the filesystem is full.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (InvalidFileException e)
            {
                Console.Error.WriteLine("ERROR: The file is invalid.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (InvalidFileTypeException e)
            {
                Console.Error.WriteLine("ERROR: The file type is invalid.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (FilesystemNotWriteableException e)
            {
                Console.Error.WriteLine("ERROR: The filesystem is write protected or does not support write operations.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (InvalidFilenameException e)
            {
                Console.Error.WriteLine("ERROR: The filename {0} is invalid.", e.Filename);
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (DiskImageFormatException e)
            {
                Console.Error.WriteLine("ERROR: Virtual disk image format error.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (DiskNotWriteableException e)
            {
                Console.Error.WriteLine("ERROR: The disk is write protected or does not support write operations.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (UnsupportedGeometryException e)
            {
                Console.Error.WriteLine("ERROR: The disk geometry is not valid or supported for this filesystem.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
            catch (System.IO.IOException e)
            {
                Console.Error.WriteLine("ERROR: Local filesystem I/O error.");
                if (debug)
                    Console.Error.WriteLine(e);
                return;
            }
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
            Console.WriteLine("Usage: DragonDos COMMAND COMMAND-ARGS [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  check <diskimage>");
            Console.WriteLine("  create <diskimage> [<tracks> [<sectors>]]");
            Console.WriteLine("  dump <diskimage> <head> <track> <sector>");
            Console.WriteLine("  delete <diskimage> {<filename>}");
            Console.WriteLine("  dir <diskimage> [-raw]");
            Console.WriteLine("  freemap <diskimage>");
            Console.WriteLine("  read <diskimage> <filename> [<local filename>] [-ascii]");
            Console.WriteLine("  read <diskimage> <filename> <tape image>.CAS [<local filename>] [-ascii}");
            Console.WriteLine("  write <diskimage> <filename> [<local filename>] [-basic] [-native load start]");
            Console.WriteLine("  write <diskimage> <filename> <tape image>.CAS [<localfilename>]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -d   Enable debug output.");
            Console.WriteLine("  -v   Enable more verbose operation.");
            Console.WriteLine();
            Console.WriteLine("Visit www.rolfmichelsen.com/dragontools for more information.");
            Console.WriteLine();
        }


        /// <summary>
        /// Parse command line options.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="CommandArgumentsException">A command line option cannot be parsed as expected.</exception>
        private IList<string> ParseOptions(IEnumerable<string> args)
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
                        case "-d":
                            verbose = debug = true;
                            break;
                        case "-raw":
                            raw = true;
                            break;
                        case "-ascii":
                            ascii = true;
                            break;
                        case "-basic":
                            filetype = FileType.Basic;
                            break;
                        case "-native":
                            //TODO Refactor to improve readability, too much error handling clutter here...
                            filetype = FileType.Native;
                            if (!r.MoveNext())
                                throw new CommandArgumentsException("Missing argument to -native option");
                            try
                            {
                                LoadAddress = Convert.ToInt32(r.Current);
                            }
                            catch (FormatException)
                            {
                                throw new CommandArgumentsException("Invalid format of argument to -native option");
                            }
                            catch (OverflowException)
                            {
                                throw new CommandArgumentsException("Invalid format of argument to -native option");
                            }
                            if (!r.MoveNext())
                                throw new CommandArgumentsException("Missing argument to -native option");
                            try
                            {
                                StartAddress = Convert.ToInt32(r.Current);
                            }
                            catch (FormatException)
                            {
                                throw new CommandArgumentsException("Invalid format of argument to -native option");
                            }
                            catch (OverflowException)
                            {
                                throw new CommandArgumentsException("Invalid format of argument to -native option");
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




        /// <summary>
        /// Check the consistency of the filesystem and output a summary to the console.
        /// </summary>
        /// <param name="args">Command arguments: &lt;disk image name&gt;</param>
        private void CheckFilesystem(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing");
                return;
            }
            var diskname = ai.Current;

            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, false))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: DragonDos disk image file \"{0}\" does not exist.", diskname);
                    return;
                }
                try
                {
                    dos.Check();
                    Console.WriteLine("The filesystem in {0} is healthy.", diskname);
                }
                catch (FilesystemConsistencyException e)
                {
                    Console.WriteLine("The filesystem in {0} is inconsistent.", diskname);
                    if (debug)
                        Console.WriteLine(e.Message);
                }
            }
        }



        /// <summary>
        /// Dump content of a sector to console.
        /// </summary>
        /// <param name="args">Command arguments; disk image name, head, track, sector</param>
        private void DumpSector(IEnumerable<string> args)
        {            
            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing");
                return;
            }
            var diskname = ai.Current;

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Head number missing");
                return;
            }
            var head = Convert.ToInt32(ai.Current);
 
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Track number missing");
                return;
            }
            var track = Convert.ToInt32(ai.Current); 

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Sector number missing");
                return;
            }
            var sector = Convert.ToInt32(ai.Current); 

            using (var disk = DiskFactory.OpenDisk(diskname, false))
            {
                if (disk == null)
                {
                    Console.Error.WriteLine("ERROR: Disk image file {0} is not in a supported format.", diskname);
                    return;
                }
                if (verbose)
                    Console.Error.WriteLine("Reading sector data at head={0} track={1} sector={2}", head, track, sector);
                var data = disk.ReadSector(head, track, sector);
                int offset = 0;
                while (offset < data.Length)
                {
                    Console.Write("{0:x4} : ",offset);
                    for (int i=0; i<16 && offset < data.Length; i++)
                        Console.Write("{0:x2} ",data[offset++]);
                    Console.WriteLine();
                }
            }
        }


        /// <summary>
        /// Output directory listing to console.
        /// </summary>
        /// <param name="args">Command arugments: &lt;disk image name&gt;.</param>
        private void ListDirectory(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing");
                return;
            }
            var diskname = ai.Current;

            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, false))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: DragonDos disk image file \"{0}\" does not exist.", diskname);
                    return;
                }
                if (raw)
                {
                    var directoryEntries = ((DragonDos) dos).GetDirectoryEntries();
                    for (var i = 0; i < directoryEntries.Length; i++)
                    {
                        Console.WriteLine("{0} {1}", i, directoryEntries[i]);
                    }
                }
                else
                {
                    var filecount = 0;
                    var files = dos.ListFiles();
                    foreach (var file in files)
                    {
                        var fileinfo = dos.GetFileInfo(file);
                        Console.WriteLine("{0,-15} {1,6} {2}", file, fileinfo.Size, fileinfo.GetAttributes());
                        filecount++;
                    }
                    if (filecount > 0)
                        Console.WriteLine();
                    Console.WriteLine("{1} files, {0} bytes free.", dos.Free(), filecount);
                    
                }
            }
        }



        /// <summary>
        /// Delete one or more files.
        /// </summary>
        /// <param name="args">Command arguments: &lt;disk image name&gt; {&lt;file name&gt;}.</param>
        private void DeleteFile(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, true))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: DragonDos disk image file \"{0}\" does not exist.", diskname);
                    return;
                }
                var deleteCount = 0;
                while (ai.MoveNext())
                {
                    var filename = ai.Current;
                    try
                    {
                        dos.DeleteFile(filename);                        
                    }
                    catch (FileNotFoundException)
                    {
                        Console.Error.WriteLine("WARNING: File {0} not found", filename);
                        continue;
                    }
                    deleteCount++;
                    Console.WriteLine("Deleted file {0}", filename);
                }
                if (deleteCount == 0)
                {
                    Console.WriteLine("No files to delete.");
                }
                else
                {
                    Console.WriteLine("Deleted {0} files.", deleteCount);
                }
            }
        }



        /// <summary>
        /// Create empty DragonDos filesystem.
        /// </summary>
        /// <param name="args">Command arguments: &lt;disk image name&gt; [&lt;tracks&gt; [&lt;sectors&gt;]].</param>
        private void CreateFilesystem(IEnumerable<string> args)
        {
            int tracks = DefaultTracks;
            int sectors = DefaultSectors;

            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            if (ai.MoveNext())
            {
                tracks = Convert.ToInt32(ai.Current);
                if (tracks != 40 && tracks != 80)
                {
                    Console.Error.WriteLine("ERROR: DragonDos only supports 40 and 80 track diskettes.");
                    return;
                }
            }

            if (ai.MoveNext())
            {
                sectors = Convert.ToInt32(ai.Current);
                if (sectors != 18 && sectors != 36)
                {
                    Console.Error.WriteLine("ERROR: DragonDos only supports 18 or 36 sectors per track.");
                    return;
                }
            }

            int heads = sectors/18;
            sectors -= (heads-1)*18;

            if (File.Exists(diskname))
            {
                Console.Error.WriteLine("ERROR: Target file {0} already exists.", diskname);
                return;
            }

            using (var disk = DiskFactory.CreateDisk(diskname, heads, tracks, sectors, DefaultSectorSize))
            {
                using (var dos = DragonDos.Initialize(disk))
                {
                    Console.WriteLine("Created empty filesystem in {0}. Capacity {1} bytes.", diskname, dos.Free());     
                }
            }
        }




        /// <summary>
        /// Read a file from the virtual DragonDos filesystem and write it to the host filesystem.
        /// </summary>
        /// <param name="args">Command arguments: &lt;disk image&gt; &lt;DragonDos filename&gt; [&lt;local filename&gt;]</param>
        private void ReadFile(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: DragonDos filename missing.");
                return;
            }
            var dragonFilename = ai.Current;
            var localFilename = ai.MoveNext() ? ai.Current : dragonFilename;
            
            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, true))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: DragonDos disk image file \"{0}\" does not exist.", diskname);
                    return;
                }
                var file = dos.ReadFile(dragonFilename);
                PrintFileInformation((DragonDosFile) file);
                SaveLocalFile((DragonDosFile) file, localFilename, ai);
            }
        }



        /// <summary>
        /// Write a DragonDos file to a file container.
        /// </summary>
        /// <param name="file">DragonDos file to write to a new container.</param>
        /// <param name="localFilename">Filename of new container.</param>
        /// <param name="ai">Enumerator for accessing any container-specific arguments.</param>
        private void SaveLocalFile(DragonDosFile file, string localFilename, IEnumerator<string> ai)
        {
            if (localFilename.EndsWith(".CAS", StringComparison.InvariantCultureIgnoreCase))
            {
                SaveToCasFile(file, localFilename, ai);
            }
            else
            {
                SaveToPlainFile(file, localFilename);
            }
        }



        /// <summary>
        /// Write a DragonDos file to a plain data file in the local file system.
        /// </summary>
        /// <param name="file">DragonDos file.</param>
        /// <param name="localFilename">Local filename.</param>
        private void SaveToPlainFile(DragonDosFile file, string localFilename)
        {
            if (file.FileType == DragonDosFileType.Basic && ascii)
            {
                var basicTokenizer = new DragonBasicTokenizer(DragonBasicDialect.DragonDos);
                var basicText = basicTokenizer.Decode(file.GetData());
                var output = new StreamWriter(new FileStream(localFilename, FileMode.Create), Encoding.ASCII);
                output.Write(basicText);
                output.Close();
            }
            else
            {
                File.WriteAllBytes(localFilename, file.GetData());
            }
        }



        /// <summary>
        /// Write a DragonDos file to a Dragon virtual tape in the CAS format.  If the CAS file already exists, write the
        /// file to the end of the virtual tape.
        /// </summary>
        /// <param name="file">DragonDos file.</param>
        /// <param name="tapeFilename">Name of the CAS file.</param>
        /// <param name="ai">Additional arguments: Optional name of file within virtual tape container.</param>
        private void SaveToCasFile(DragonDosFile file, string tapeFilename, IEnumerator<string> ai)
        {
            var localFilename = ai.MoveNext() ? ai.Current : file.FileInfo.Name;

            IFile dragonFile;
            switch (file.FileType)
            {
                case DragonDosFileType.MachineCode:
                    dragonFile = DragonFile.CreateMachineCodeFile(localFilename, file.GetData(), file.LoadAddress, file.StartAddress, false, false);
                    break;
                case DragonDosFileType.Basic:
                    if (ascii)
                    {
                        var basicTokenizer = new DragonBasicTokenizer(DragonBasicDialect.DragonDos);
                        var basicText = basicTokenizer.DecodeToBytes(file.GetData());
                        dragonFile = DragonFile.CreateBasicFile(localFilename, basicText, true, true);
                    }
                    else
                        dragonFile = DragonFile.CreateBasicFile(localFilename, file.GetData(), false, false);
                    break;
                default:
                    dragonFile = DragonFile.CreateDataFile(localFilename, file.GetData(), false, false);
                    break;
            }
            
            using (var tape = new DragonTape(new CasTape(new FileStream(tapeFilename, FileMode.Append))))
            {
                tape.WriteFile(dragonFile);
            }
        }



        /// <summary>
        /// Output information about a DragonDos file to the console.
        /// </summary>
        /// <param name="file">File to output information about.</param>
        private static void PrintFileInformation(DragonDosFile file)
        {
            switch (file.FileType)
            {
                case DragonDosFileType.MachineCode:
                    Console.WriteLine("DragonDos machine code program: Load address={0} Length={1} Start address={2}", file.LoadAddress, file.Length, file.StartAddress);
                    break;
                case DragonDosFileType.Basic:
                    Console.WriteLine("DragonDos BASIC program: Length={0}", file.Length);
                    break;
                default:
                    Console.WriteLine("DragonDos data file: Length={0}", file.Length);
                    break;
            }
        }




        /// <summary>
        /// Read a file from the host filesystem and write it to the virtual DragonDos filesystem.
        /// </summary>
        /// <param name="args">Command arguments: DiskImage DragonDosFilename LocalFilename.</param>
        private void WriteFile(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: DragonDos filename missing.");
                return;
            }
            var dragonFilename = ai.Current;
            var localFilename = ai.MoveNext() ? ai.Current : dragonFilename;

            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, true))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: DragonDos disk image file \"{0}\" does not exist.", diskname);
                    return;
                }
                var file = ReadLocalFile(localFilename, ai);
                PrintFileInformation(file);
                dos.WriteFile(dragonFilename, file);
            }            
        }



        private DragonDosFile ReadLocalFile(string localFilename, IEnumerator<string> ai)
        {
            if (localFilename.EndsWith(".CAS", StringComparison.InvariantCultureIgnoreCase))
            {
                return ReadLocalCasFile(localFilename, ai);
            }
            else
            {
                var data = File.ReadAllBytes(localFilename);
                DragonDosFile file = null;
                switch (filetype)
                {
                    case FileType.Data:
                        file = DragonDosFile.CreateDataFile(data);
                        break;
                    case FileType.Basic:
                        file = DragonDosFile.CreateBasicFile(data);
                        break;
                    case FileType.Native:
                        file = DragonDosFile.CreateMachineCodeFile(data, LoadAddress, StartAddress);
                        break;
                    default:
                        throw new Exception("Invalid file type specified");
                }
                return file;
            }
        }




        /// <summary>
        /// Read a file from a virtual Dragon cassette and return the corresponding DragonDosFile
        /// object.
        /// </summary>
        /// <param name="tapeFilename">Name of the virtual Dragon tape.</param>
        /// <param name="ai">Additional parameters: Filename</param>
        /// <returns></returns>
        private DragonDosFile ReadLocalCasFile(string tapeFilename, IEnumerator<string> ai)
        {
            var filename = ai.MoveNext() ? ai.Current : null;

            using (var tape = new DragonTape(new CasTape(new FileStream(tapeFilename, FileMode.Open))))
            {
                var file = (DragonFile) tape.ReadFile(filename);
                switch (file.FileType)
                {
                    case DragonFileType.Basic:
                        return DragonDosFile.CreateBasicFile(file.GetData());
                    case DragonFileType.MachineCode:
                        return DragonDosFile.CreateMachineCodeFile(file.GetData(), file.LoadAddress, file.StartAddress);
                    case DragonFileType.Data:
                        return DragonDosFile.CreateDataFile(file.GetData());
                    default:
                        throw new InvalidFileTypeException();
                }
            }
        }




        /// <summary>
        /// Display a map of the free disk sectors.
        /// </summary>
        /// <param name="args">Command arguments: &lt;disk image name&gt;.</param>
        private void Freemap(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing");
                return;
            }
            var diskname = ai.Current;

            using (var dos = (DragonDos) DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, false))
            {
                if (dos == null)
                {
                    Console.Error.WriteLine("ERROR: DragonDos disk image file \"{0}\" does not exist.", diskname);
                    return;
                }

                int freeSectors = 0;
                int allocatedSectors = 0;

                for (int track = 0; track < dos.Disk.Tracks; track++)
                {
                    Console.Write("{0,2} : ", track+1);
                    for (int head=0; head<dos.Disk.Heads; head++)
                    {
                        for (int sector=0; sector<DragonDos.SectorsPerHead; sector++)
                        {
                            if (dos.IsSectorAllocated(head, track, sector))
                            {
                                Console.Write('*');
                                allocatedSectors++;
                            }
                            else
                            {
                                Console.Write('.');
                                freeSectors++;
                            }
                        }
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
                Console.WriteLine("{0} allocated sectors.", allocatedSectors);
                Console.WriteLine("{0} free sectors.", freeSectors);
            }
        }

    }



    internal enum FileType
    {
        Data,
        Basic,
        Native
    } 
}
