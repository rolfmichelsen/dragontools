/*
Copyright (c) 2011, Rolf Michelsen
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
using System.Reflection;
using RolfMichelsen.Dragon.DragonTools.IO.Disk;
using RolfMichelsen.Dragon.DragonTools.IO.Tape;
using RolfMichelsen.Dragon.DragonTools.IO;
using System.IO;
using FileNotFoundException = RolfMichelsen.Dragon.DragonTools.IO.FileNotFoundException;


namespace RolfMichelsen.Dragon.DragonTools.DragonDosTools
{
    /// <summary>
    /// Program for manipulation of virtual DragonDos filesystems.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The default number of disk heads when creating a DragonDos filesystem.
        /// </summary>
        private const int DefaultHeads = 1;

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
        /// Set for quiet program operation.
        /// This flag is controlled by the -q command line option.
        /// </summary>
        private bool quiet = false;

        /// <summary>
        /// Set to force command execution even for inconsistent filesystems.
        /// This flag is controlled by the -f command line option.
        /// </summary>
        private bool force = false;

        /// <summary>
        /// Set for the PUT command to write a BASIC program file.
        /// This flag is controlled by the -basic command line option.
        /// </summary>
        private bool basic = false;

        /// <summary>
        /// Set for the PUT command to specify the loading address of machine code programs.
        /// This value is controlled by the -load command line option
        /// </summary>
        private int loadAddress = 0;

        /// <summary>
        /// Set for the PUT command to specify the execution address of machine code programs.
        /// This value is controlled by the -exec command line option.
        /// </summary>
        private int execAddress = 0;


        static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
        }


        private void Run(string[] args)
        {
            var commands = ParseOptions(args);

            if (commands.Count == 0)
            {
                ShowUsage();
                return;
            }

            try
            {
                var command = commands[0].ToLowerInvariant();
                commands.RemoveAt(0);
                switch (command)
                {
                    case "create":
                        CreateFilesystem(commands);
                        break;
                    case "delete":
                        DeleteFile(commands);
                        break;
                    case "dir":
                        ListDirectory(commands);
                        break;
                    case "get":
                        GetFile(commands);
                        break;
                    case "put":
                        PutFile(commands);
                        break;
                    default:
                        Console.Error.WriteLine("ERROR: Unknown command {0}.", commands[0]);
                        break;
                }        
                
            }
            catch (FileNotFoundException e)
            {
                Console.Error.WriteLine("ERROR: The file does not exist.");
                return;
            }
            catch (DirectoryFullException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write file to filesystem.  The directory is full.");
                return;
            }
            catch (FileExistsException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write the file as a file with the same name already exists.");
                return;
            }
            catch (FilesystemConsistencyException e)
            {
                Console.Error.WriteLine("ERROR: Cannot complete operation as the filesystem is corrupt.");
                return;
            }
            catch (FilesystemFullException e)
            {
                Console.Error.WriteLine("ERROR: Cannot write file as the filesystem is full.");
                return;
            }
            catch (InvalidFileException e)
            {
                Console.Error.WriteLine("ERROR: The file is invalid.");
                return;
            }
            catch (InvalidFileTypeException e)
            {
                Console.Error.WriteLine("ERROR: The file type is invalid.");
                return;
            }
            catch (FilesystemNotWriteableException e)
            {
                Console.Error.WriteLine("ERROR: The filesystem is write protected or does not support write operations.");
                return;
            }
            catch (InvalidFilenameException e)
            {
                Console.Error.WriteLine("ERROR: The filename {0} is invalid.", e.Filename);
                return;
            }
            catch (DiskImageFormatException e)
            {
                Console.Error.WriteLine("ERROR: Virtual disk image format error.");
                return;
            }
            catch (DiskNotWriteableException e)
            {
                Console.Error.WriteLine("ERROR: The disk is write protected or does not support write operations.");
                return;
            }
            catch (UnsupportedGeometryException e)
            {
                Console.Error.WriteLine("ERROR: The disk geometry is not valid or supported for this filesystem.");
                return;
            }
            catch (System.IO.IOException e)
            {
                Console.Error.WriteLine("ERROR: Local filesystem I/O error.");
                return;
            }
        }


        /// <summary>
        /// Return the file version attribute for the program's assembly.
        /// </summary>
        /// <returns>File version attribute.</returns>
        private string GetVersionInfo()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = (AssemblyFileVersionAttribute[]) assembly.GetCustomAttributes(typeof (AssemblyFileVersionAttribute), false);
            return (version.Length == 0) ? "" : version[0].Version;
        }


        /// <summary>
        /// Output program usage information to the console.
        /// </summary>
        private void ShowUsage()
        {
            Console.WriteLine("ddos {0} - DragonDos Tools", GetVersionInfo());
            Console.WriteLine("(C) Rolf Michelsen, 2011");
            Console.WriteLine();
            Console.WriteLine("Usage: ddos COMMAND COMMAND-ARGS [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  create <diskimage>");
            Console.WriteLine("  delete <diskimage> {<filename>}");
            Console.WriteLine("  dir <diskimage>");
            Console.WriteLine("  get <diskimage> <filename> [<local filename>]");
            Console.WriteLine("  put <diskimage> <filename> [<local filename>]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -v           Enable more verbose operation.");
            Console.WriteLine("  -q           Enable really quiet operation.");
            Console.WriteLine("  -f           Force command even when filesystem is damaged.");
            Console.WriteLine("  -basic       PUT command will write a BASIC program file.");
            Console.WriteLine("  -load <addr> PUT command will write a machine code program file with given");
            Console.WriteLine("               load address.");
            Console.WriteLine("  -exec <addr> PUT command will write a machine code program file with given");
            Console.WriteLine("               exec address.");
            Console.WriteLine();
            Console.WriteLine("Visit www.rolfmichelsen.com for more information.");
            Console.WriteLine();
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
                        case "-v":
                            verbose = true;
                            break;
                        case "-q":
                            quiet = true;
                            break;
                        case "-f":
                            force = true;
                            break;
                        case "-basic":
                            basic = true;
                            break;
                        case "-load":
                            if (!r.MoveNext()) throw new ApplicationException("Missing argument to -load");
                            loadAddress = Convert.ToInt32(r.Current);
                            break;
                        case "-exec":
                            if (!r.MoveNext()) throw new ApplicationException("Missing argument to -exec");
                            execAddress = Convert.ToInt32(r.Current);
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
        /// Check the filesystem consistency.
        /// Outputs a suitable error or warning if the consistency check fails, depending on the value of the <see cref="force">force</see> flag.
        /// </summary>
        /// <param name="dos">Filesystem to check.</param>
        /// <returns>True if the filesystem passes the consistency checks or command execution is forced.</returns>
        private bool CheckFilesystem(IDiskFilesystem dos)
        {
            try
            {
                dos.Check();
                return true;
            }
            catch (FilesystemConsistencyException e)
            {
                if (force)
                {
                    Console.WriteLine("WARNING: Filesystem is inconsistent but command execution is forced to continue.");
                    return true;
                }
                Console.Error.WriteLine("ERROR: Filesystem is inconsistent and command execution is aborted.");
                return false;
            }
        }


        /// <summary>
        /// Write a file from the local filesystem to the DragonDos filesystem.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        private void PutFile(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            /* Get the disk image name from the argument list. */
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            /* Get the name of the target DragonDos file from the argument list. */
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Missing name of target DragonDos file.");
                return;
            }
            var filename = ai.Current;

            /* Get the (optional) name of the local file to write to the DragonDos filesystem from the argument list.  If no filename
             * is specified, use the name of the DragonDos file. */
            var localfilename = ai.MoveNext() ? ai.Current : filename;

            if (!File.Exists(localfilename))
            {
                Console.Error.WriteLine("ERROR: Local file {0} does not exist.", localfilename);
                return;
            }

            /* Read the local file data and create an IFile object. */
            var data = File.ReadAllBytes(localfilename);
            IFile file;
            if (basic)
            {
                file = new DragonDosBasicFile(data);
            }
            else if (loadAddress != 0 || execAddress != 0)
            {
                file = new DragonDosMachineCodeFile(data, loadAddress, execAddress);
            }
            else
            {
                file = new DragonDosDataFile(data);                
            }
            
            
            /* Write the file to the DragonDos filesystem. */
            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, true))
            {
                if (!CheckFilesystem(dos)) return;
                dos.WriteFile(filename, file);
                if (!quiet)
                {
                    Console.WriteLine("Wrote file {0} -- {1}", filename, file);
                }
            }
        }


        /// <summary>
        /// Read file from DragonDos filesystem and write to local filesystem.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        private void GetFile(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            /* Get the disk image name from the argument list. */
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            /* Get the name of the DragonDos file to read from the argument list. */
            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Missing name of file to read from DragonDos filesystem.");
                return;
            }
            var filename = ai.Current;

            /* Get the (optional) name of the local file to write to from the argument list.  If no local file name is given,
             * use the DragonDos filename.  If the local filename ends with ".cas", then the file will be written to a CAS
             * filesystem with an optional fourth parameter specifying the filename within the CAS filesystem. */

            string casfilename = null;
            string targetfilename;

            var arg1 = ai.MoveNext() ? ai.Current : null;
            var arg2 = ai.MoveNext() ? ai.Current : null;
            if (arg1 != null && arg1.EndsWith(".cas", StringComparison.InvariantCultureIgnoreCase))
            {
                casfilename = arg1;
                targetfilename = arg2 ?? filename;
                if (File.Exists(casfilename))
                {
                    Console.Error.WriteLine("ERROR: Local filesystem file {0} already exists.", casfilename);
                    return;
                }
            }
            else
            {
                targetfilename = arg1 ?? filename;
                if (File.Exists(targetfilename))
                {
                    Console.Error.WriteLine("ERROR: Local filesystem file {0} already exists.", targetfilename);
                    return;
                }
            }

            /* Read the file from the DragonDos filesystem and write it to the local filesystem. */
            using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, diskname, false))
            {
                if (!CheckFilesystem(dos)) return;
                var file = (DragonDosFile) dos.ReadFile(filename);
                if (casfilename == null)
                {
                    File.WriteAllBytes(targetfilename, file.GetData());                    
                }
                else
                {
                    WriteCassetteFile(casfilename, targetfilename, file);
                }
                if (!quiet)
                {
                    Console.WriteLine("Read file {0} -- {1}",filename,file); 
                }
            }
        }



        private void WriteCassetteFile(string casfilename, string targetfilename, DragonDosFile file)
        {
            IFile tapefile;
            if (file is DragonDosBasicFile)
                tapefile = new DragonBasicFile(targetfilename, file.GetData(), false, false);
            else if (file is DragonDosMachineCodeFile)
                tapefile = new DragonMachineCodeFile(targetfilename, file.GetData(), false, false, ((DragonDosMachineCodeFile)file).LoadAddress, ((DragonDosMachineCodeFile)file).ExecAddress);
                
            else
                tapefile = new DragonDataFile(targetfilename, file.GetData(), false, false);

            using (var tape = new DragonTapeFilesystem(new CasWriter(new System.IO.FileStream(casfilename, FileMode.CreateNew))))
            {
                tape.WriteFile(tapefile);
            }
        }


        /// <summary>
        /// Output directory listing to console.
        /// </summary>
        /// <param name="args">Command arugments.</param>
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
                if (!CheckFilesystem(dos)) return;
                var filecount = 0;
                var files = dos.ListFiles();
                foreach (var file in files)
                {
                    var fileinfo = dos.GetFileInfo(file);
                    Console.WriteLine("{0,-15} {1,6} {2}", file, fileinfo.Size, fileinfo.GetAttributes());
                    filecount++;
                }
                Console.WriteLine();
                Console.WriteLine("{1} files, {0} bytes free.", dos.Free(), filecount);
            }
        }


        /// <summary>
        /// Delete one or more files.
        /// </summary>
        /// <param name="args">Command arguments.</param>
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
                if (!CheckFilesystem(dos)) return;
                var deleteCount = 0;
                while (ai.MoveNext())
                {
                    var filename = ai.Current;
                    dos.DeleteFile(filename);
                    deleteCount++;
                    if (!quiet)
                    {
                        Console.WriteLine("Deleted file {0}", filename);
                    }
                }
                if (!quiet)
                {
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
        }



        /// <summary>
        /// Create empty DragonDos filesystem.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        private void CreateFilesystem(IEnumerable<string> args)
        {
            var ai = args.GetEnumerator();

            if (!ai.MoveNext())
            {
                Console.Error.WriteLine("ERROR: Disk image name missing.");
                return;
            }
            var diskname = ai.Current;

            if (File.Exists(diskname))
            {
                Console.Error.WriteLine("ERROR: Target file {0} already exists.", diskname);
                return;
            }

            using (var disk = DiskFactory.CreateDisk(diskname, DefaultHeads, DefaultTracks, DefaultSectors, DefaultSectorSize))
            {
                using (var dos = DiskFilesystemFactory.OpenFilesystem(DiskFilesystemIdentifier.DragonDos, disk, true))
                {
                    if (!CheckFilesystem(dos)) return;
                    dos.Initialize();
                    if (!quiet)
                    {
                        Console.WriteLine("Created empty filesystem in {0}. Capacity {1} bytes.", diskname, dos.Free());
                    }
                }
            }
        }
    }
}
