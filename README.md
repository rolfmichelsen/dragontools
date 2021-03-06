DRAGON TOOLS
============

&copy; Rolf Michelsen, 2011-2020
All rights reserved.



Introduction
------------

*Dragon Tools* is a collection of tools to work and interface with the 1980's
Dragon series of home computers.  There are a number of excellent Dragon
emulators that replicate a Dragon environment on a modern PC.  The Dragon Tools
build a bridge between the emulated Dragon environment and the host operating
system.

Download binary distributions from the
[Dragon Tools home](http://www.rolfmichelsen.com/dragontools/).
Source is available in the GitHub
[Dragon Tools repository](http://github.com/rolfmichelsen/dragontools).
Documentation for the tools and the library can be found in the corresponding
wiki.  All code and binaries are available under the Apache 2.0 open source
license.  See the file *LICENSE* for licensing terms and conditions.

The Microsoft .NET Framework 4.0 or later must be installed to use any of the
programs in this package.  Use Windows Update to install the .NET 4.0 Framework
or install from the Microsoft downloads site.

Visit the [Dragon Tools home](http://www.rolfmichelsen.com/dragontools/) for
more information and to contect the author.



Contents
--------

### DragonDos

A command line utility for accessing virtual DragonDos filesystems in VDK, DSK or HFE
floppy disk images from a Windows environment.  The utility supports
functionality to list files, rename files, delete files, create an empty
filesystem and copying files between the DragonDos filesystem and the Windows
filesystem.


### File2VDK

A command line utility for very simple writing of local files to a new VDK disk image.


### DragonTools

A .NET 4 library supporting development of tools for working with Dragon
computers.  Currently, the library primarily supports functionality to access
disk and tape-based filesystems.



Development
-----------

*Dragon Tools* have been developed using Visual Studio 2013.  The repository
contains a Visual Studio solution with multiple projects.

<table>
<tr>
    <td>DragonTools</td>
    <td>This project is the Dragon Tools .NET library.</td>
</tr>
<tr>
    <td>DragonTools.acceptance</td>
    <td>Acceptance tests for the *Dragon Tools* library.  The acceptance tests
        only use public parts of the library.</td>
</tr>
<tr>
    <td>DragonTools.unit</td>
    <td>Unit tests for the *Dragon Tools* library.  Unit tests aim to cover the
        internal functionality of the library and may use private classes and
        methods.</td>
</tr>
<tr>
    <td>DragonDosTools</td>
    <td>Command line program for accessing DragonDos virtual filesystems from a
        Windows environment.</td>
</tr>
<tr>
    <td>File2VDK</td>
    <td>The File2VDK command line utility.</td>
</tr>
</table>

The *Dragon Tools* library is still in early development.  Parts of the library
are reaching some level of maturity, but expect API incompatibilities even
between minor releases at this point.