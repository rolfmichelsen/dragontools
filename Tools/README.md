TOOLS
=====

This directory contains various support tools for maintaining this project.


Testdisk
--------

This is a BASIC program to be run on a Dragon computer or emulator to create disk images for testing the disk abstraction in the DragonTools library.  The program prompts for a disk drive that contains an empty DragonDos formatted disk.  It will then overwrite all sectors of this disk.  Each sector starts with two bytes containing the logical sector number (LSN) in big-endian format followed by 254 zero-valued bytes.

The program code can be loaded directly into [xroar](http://www.6809.org.uk/xroar/) in source form using the File | Load function and then typing CLOAD on the Dragon prompt.