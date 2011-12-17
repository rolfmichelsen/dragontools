DRAGONDOS TOOLS
===============

(C) Rolf Michelsen, 2011
www.rolfmichelsen.com


NAME
    ddos - manipulate DragonDos filesystems on virtual diskettes

    
SYNOPSIS
    ddos COMMAND COMMAND-ARGS [OPTIONS]


DESCRIPTION

    Program for manipulating virtual DragonDos filesystems hosted inside .vdk 
    or .dsk virtual diskette images.  See the COMMANDS section for a 
    description of all the operations that can be performed.
    
    The program requires the Microsoft .NET Framework version 4.0 to be 
    installed.  The .NET Framework can be installed through Windows Update or 
    by downloading from the Microsoft downloads site.


COMMANDS

    create <disk>
        Create an empty DragonDos filesystem.  Currently, this command will 
        only create single sided disks with 40 tracks and 18 sectors per 
        track.

    delete <disk> {<file>}
        Delete one or more files from the filesystem.
        
    dir <disk> 
        Output a directory listing for the filesystem.
        
    get <disk> <file> [<localname>]
        Read a file from the filesystem and store it as a local file.  If the 
        local filename is omitted, the DragonDos filename will be used.
        
    put <disk> <file> [<localname>]
        Write a local file to the filesystem.  If the local filename is 
        omitted, the DragonDos filename will be used.  By default, the file 
        will be written as a DragonDos file.  See the program options for how 
        to write BASIC and machine code program files.
     
     
OPTIONS

    -v           Enable more verbose operation.  (Currently, this option does 
                 not have any effect.)
    -q           Enable really quiet operation.
    -f           Force command even when filesystem is damaged.
    -basic       PUT command will write a BASIC program file.
    -load <addr> PUT command will write a machine code program file 
                 with given load address.
    -exec <addr> PUT command will write a machine code program file with   
                 given exec address.


BUGS

    The program has very little error handling.  Expect to see uncaught 
    exceptions and stack traces if an operation fails for any reason.  Don't 
    panic!
    