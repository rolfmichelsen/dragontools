DRAGON TOOLS 0.2
================

&copy; Rolf Michelsen, 2011-2012  
All rights reserved.



Introduction
------------

*Dragon Tools* is a collection of tools to work and interface with the 1980's Dragon series of home computers.  There are a number
of excellent Dragon emulators that replicate a Dragon environment on a modern PC.  The Dragon Tools build a bridge between the
emulated Dragon environment and the host operating system.

The project is hosted at GitHub.  Source and binaries are available in the [Dragon Tools repository](http://github.com/rolfmichelsen/dragontools)
All code and binaries are available under a BSD-style open source license.  See the file *License.txt* for licensing terms and conditions.

The Microsoft .NET Framework 4.0 or later must be installed to use any of the programs in this package.  Use Windows Update to install the .NET 4.0
Framework or install from the Microsoft downloads site.

Visit the [Dragon Tools home](http://www.rolfmichelsen.com/dragontools/) for more information and to contect the author.



Contents
--------

### DragonDos 0.2

A command line utility for accessing virtual DragonDos filesystems in VDK or DSK floppy disk images from a Windows environment.  The
utility supports functionality to list files, rename files, delete files, create an empty filesystem and copying files between the
DragonDos filesystem and the Windows filesystem.


### DragonTools 0.2

A .NET 4 library supporting development of tools for working with Dragon computers.  Currently, the library primarily supports
functionality to access disk and tape-based filesystems.



Development
-----------

*Dragon Tools* have been developed using Visual Studio 2010.  The repository contains a Visual Studio solution with multiple projects.

<table>
<tr>
    <td>DragonTools</td>
    <td>This project is the *Dragon Tools* .NET library.</td>
</tr>
<tr>
    <td>DragonTools.acceptance</td>
    <td>Acceptance tests for the *Dragon Tools* library.  The acceptance tests only use public parts of the library.</td>
</tr>
<tr>
    <td>DragonTools.unit</td>
    <td>Unit tests for the *Dragon Tools* library.  Unit tests aim to cover the internal functionality of the library and may use
    private classes and methods.</td>
</tr>
<tr>
    <td>DragonDosTools</td>
    <td>Command line program for accessing DragonDos virtual filesystems from a Windows environment.</td>
</tr>
</table>

The *Dragon Tools* library is still in early development.  Parts of the library are reaching some level of maturity, but expect API 
incompatibilities even between minor releases at this point.




