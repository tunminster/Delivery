param
(
    [string] $projectName = $(throw "The project name is required")
)

Write-Host "Versioning started"

"Sources directory " + $Env:BUILD_SOURCESDIRECTORY
"Build number " + $Env:BUILD_BUILDNUMBER
$csprojfilename = $Env:BUILD_SOURCESDIRECTORY+"\"+$projectName
"Project file to update " + $csprojfilename
[xml]$csprojcontents = Get-Content -Path $csprojfilename;
"Current version number is" + $csprojcontents.Project.PropertyGroup.Version
$oldversionNumber = $csprojcontents.Project.PropertyGroup.Version
$csprojcontents.Project.PropertyGroup.Version = $Env:BUILD_BUILDNUMBER
$csprojcontents.Save($csprojfilename)
"Version number has been udated from " + $oldversionNumber + " to " + $Env:BUILD_BUILDNUMBER

Write-Host "Finished"