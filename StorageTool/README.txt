DRAGON STORAGE TOOLS
====================

(C) Rolf Michelsen, 2011
www.rolfmichelsen.com



INTRODUCTION
------------

This tool allows enthusiasts of the 1980's Dragon home computer to access 
filesystems maintained on virtual storage media.  There is some support for CoCo 
filesystems as well since these are somewhat relevant to the Dragon community.

DST currently supports VDK and DSK virtual floppy disks.  Support for other 
virtual storage media will be added.

The table below lists the supported filesystems and summarizes the level of 
support for each of them.

Filesystem  Description
----------  -----------
DragonDos   Full read and write support of raw files.
OS-9        Read-only support.
FLEX        Experimental support.  Currently only provides directory listings.
DRDOS       Read-only support.



USAGE
-----

DST is a command line utility to be run from a Command Prompt window.  The 
general command line format is:

    dst <command> <params> <options>
    
Here, <command> identifies the function you want to perform.  All functions are 
described below.  <params> is a list of mandatory parameters particular to the 
function you want to perform.  These parameters are described with the commands.  
Finally, <options> is a set of optional parameters that can further influence 
how the command is performed.  Options can occur anywhere on the command line, 
and they are common across all commands.

The supported options are:

-fstype=<type>  DST will not automatically determine the filesystem contained 
                within a virtual storage media.  Use this option to specify the 
                filesystem.  If the filesystem isn't specified, DST will assume 
                that the filesystem is DragonDos.  Supported types are:
                    ddos    DragonDos (default)
                    os9     OS-9
                    flex    FLEX
                    rsdos   RSDOS

-l              Display more information about each file for the dir command.

-v              Verbose mode.  Display some more information as the program is 
                executed.  This information is currently not very structured or 
                useful and mostly for debugging purposes.



COMMANDS
--------

DST supports the following commands.

help
                Display a summary of program usage information.
                
check <disk>
                Check a filesystem for consistency problems.  DST will only flag 
                consistency issues that can lead to future damage to the 
                filesystem, e.g. writing a new file may potentially overwrite 
                the data of another file.  Consistency issues that cannot lead 
                to corruption of the filesystem will not be reported, e.g., an 
                unused sector is marked as allocated by the filesystem.  <disk> 
                is the filename of the virtual storage media.
                
create <disk> <heads> <tracks> <sectors> <sectorsize>
                Create a new virtual storage media in file <disk> and initialize 
                an empty fiesystem in it.  Additional parameters specify the 
                disk geometry.  See the examples below for some typical 
                settings.

delete <disk> <file>
                Delete a named file from a filesystem.  Some filesystems may do 
                case-sensitive filename matching, but most don't.

dir <disk>
                List the files in a filesystem.  Normally, only the filenames 
                are listed.  Use the -l option to display more information with 
                each file.  The type of additional information depends on the 
                filesystem.

free <disk>
                Display the free space for a filesystem.

freemap <disk>
                Displays a graphical representation of the disk with allocated 
                sectors marked.

get <disk> <source> <destination>
                Get a <source> file from the virtual filesystem and write it to 
                the Windows filesystem using <destination> as the new filename.  
                The file is copied in raw mode, i.e., all filesystem headers and 
                encodings are preserved.

put <disk> <source> <destination>
                Read <source> file from the Windows filesystem and write it to 
                the virtual vilesystem using <destination> as the new filename.  
                The source file must be in raw format, i.e., it must contain all 
                headers and encodings that are required by the filesystem to 
                make up a valid file.

readsector <disk> <head> <track> <sector>
                Display a raw dump of data from a given disk sector in a virtual 
                storage media.  This command bypasses the filesystem and it is 
                not necessary to specify the -fstype parameter.  All 
                parameters are zero-based.

rename <disk> <old> <new>
                Rename a filesystem file from <old> to <new>.



EXAMPLES
--------

create disk.vdk 1 40 18 256
                Create an empty DragonDos filesystem in a virtual diskette in 
                the file "disk.vdk".  The disk will be single-sided with 40 
                tracks and 18 sectors per track.  Each sector will have a 
                capacity of 256 bytes.
                
create disk.vdk 1 40 18 256 -fstype=os9
                Just as the example above, except that an OS-9 filesystem is 
                created on the virtual diskette.
                
delete disk.vdk foo.bar
                Delete the file named "foo.bar" from the DragonDos filesystem 
                hosted in the virtual diskette in file "disk.vdk".
                
get disk.vdk foo.bar foowin.bar
                Read the file "foo.bar" from the DragonDos filesystem hosted in 
                the virtual diskette in the file "disk.vdk".  Write the file 
                data to the Windows file "foowin.bar".
                