# Copyright (c) 2011-2013, Rolf Michelsen
# All rights reserved.
#
# Create a binary distribution package.  Run from the solution root.  It will
# build the release configuration of the solution and package specific files
# in a ZIP archive.
#
# Requires an external program "zip" to be available for creating zip files.
# I'm using the InfoZIP utilities from http://info-zip.org/.

<#
.SYNOPSIS
Create a DragonTools binary distribution package.

.DESCRIPTION
Create-Distribution creates a DragonTools binary distribution as a ZIP archive.
The command must be run from the DragonTools solution root, and the binary
distribution file will also be written to this directory.

The script will first build the release version of the DragonTools solution
and then package a set of predefined files in the distribution archive.

.EXAMPLE
Create-Distribution

Build the Dragon Tools solution and create a binary release package.

.NOTES
Create-Distribution depends on an external program for creating the ZIP archive.
It has been tested with the Info-ZIP distribution.  See http://info-zip.org/.
#>

$targetPackageName = "DragonTools.zip"
$msbuildProjectFile = "Dragon Tools.sln"

$packageFiles = @{
    "License.txt" = "License.txt";
    "Dist Readme.txt" = "README.txt";
    "DragonDosTools\bin\Release\DragonDos.exe" = "DragonDos.exe";
    "DragonDosTools\bin\Release\DragonTools.dll" = "DragonTools.dll"
}

if (!(Test-Path $msbuildProjectFile))
{
    Write-Output("Cannot find " + $msbuildProjectFile + " in current directory.")
    Write-Output("This scrips can only be run from the solution root.")
    return; 
}

msbuild $msbuildProjectFile /p:Configuration=Release  /t:Rebuild /verbosity:quiet

Write-Output ""

$tmpDir = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
$out = New-Item -Path $tmpDir -ItemType Directory
Write-Output ("Using temporary directory " + $tmpDir)

foreach ($sourceFile in $packageFiles.keys) 
{
    $destinationFile = Join-Path $tmpDir ($packageFiles[$sourceFile])
    Copy-Item $sourceFile -Destination $destinationFile
}

Write-Output ("Creating binary distribution package " + $targetPackageName)
remove-item $targetPackageName -ErrorAction SilentlyContinue
zip -j $targetPackageName ($tmpDir + "\*.*")

Remove-Item -Recurse $tmpDir
