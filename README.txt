DRAGON TOOLS
------------

(C) Rolf Michelsen, 2011
www.rolfmichelsen.com


INTRODUCTION
------------

Dragon Tools is a collection of tools to work and interface with the 1980's 
Dragon series of home computers.

All code and binaries are made available under a BSD-style open source 
license.  See the file License.txt for licensing terms and conditions.

The project is hosted at GitHub, and the repository is available at 
https://github.com/rolfmichelsen/dragontools.


PREREQUISITES
-------------

The Microsoft .NET Framework 4.0 or later must be installed to use any of the 
programs in this package.  Use Windows Update to install the .NET 4.0 
Framework or install from this link:
    http://www.microsoft.com/download/en/details.aspx?id=17851



PROJECTS
--------

The top level directory contains a Visual Studio solution.  The solution 
contains the following projects:

DragonTools
    A .NET library supporting rapid development of tools for working with 
    Dragon computers or emulators.

DragonTools.acceptance
    MSTest project testing externally available features of the DragonTools 
    library.

DragonDosTools
    A command line utility for manipulating DragonDos filesystems on virtual
    diskettes.
    
StorageTool
    A command line utility for manipulating virtual filesystems typically used 
    with Dragon emulators.
    
    