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

$index=0
$groups = $csprojcontents.SelectNodes("//PropertyGroup")
"Found $($groups.Count) 'PropertyGroup' nodes"
if ($groups.Count -gt 0) {
  For ($i=0; $i -le $groups.Count; $i++) {
    if($groups[$i].Version) {
      "Found 'Version' in the 'PropertyGroup' node $($i+1)"
      $index = $i
    }
  }
}

"Current version number is" + $csprojcontents.Project.PropertyGroup[$index].Version
$oldversionNumber = $csprojcontents.Project.PropertyGroup[$index].Version
$csprojcontents.Project.PropertyGroup[$index].Version = $Env:BUILD_BUILDNUMBER
$csprojcontents.Save($csprojfilename)
"Version number has been udated from " + $oldversionNumber + " to " + $Env:BUILD_BUILDNUMBER

Write-Host "Finished"