# Copyright (c) 2011-2015, Rolf Michelsen
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

$msbuildProjectFile = "Dragon Tools.sln"

$packageFiles = @{
    "License.txt" = "License.txt";
    "Dist Readme.txt" = "README.txt";
    "DragonDosTools\bin\Release\DragonDos.exe" = "DragonDos.exe";
    "DragonDosTools\bin\Release\DragonTools.dll" = "DragonTools.dll";
    "File2VDK\bin\Release\File2VDK.exe" = "File2VDK.exe"
}



<#
    Test the script prerequisites.
#>
function Test-Prerequisites
{
    if (!(Test-Path $msbuildProjectFile))
    {
        Write-Output("Cannot find " + $msbuildProjectFile + " in current directory.")
        Write-Output("This scrips can only be run from the solution root.")
        exit 1
    }
}



<#
    Build a release version of the solution.
#>
function Build-Release
{
    msbuild $msbuildProjectFile /p:Configuration=Release  /t:Rebuild /verbosity:quiet
}



<#
    Return the distribution version identifier.  For a tagged commit, this
    corresponds to the tag name.  For an untagget commit, this corresponds to the
    commit identifier.
#>
function Get-VersionId
{
    $description = git describe --dirty
    if ($description -match "-dirty$") {
        Write-Output("Repository is dirty, cannot create distribution.")
        exit 1
    }
    if ($description -match "-([a-z0-9]{8})$") {
        return "snapshot-" + $Matches[1]
    }
    return $description
}



<#
    Create a ZIP archive containing the binary distribution.  The $packageFiles
    array lists the files to include in the distribution.
#>
function Package-Release($version)
{
    $targetPackageName = "DragonTools-" + $version + ".zip"

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
}


Test-Prerequisites
$version = Get-VersionId
Build-Release
Package-Release $version
