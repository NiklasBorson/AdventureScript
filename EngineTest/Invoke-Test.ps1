param (
    [switch] $Release
)

# Determine the relative path of test binaries.
$buildType = $Release ? 'Release' : 'Debug'
$binDir = Join-Path 'bin' $buildType 'net8.0'

# Run the test
Write-Output "Running EngineTest.exe..."
Push-Location (Join-Path $PSScriptRoot $binDir)
.\EngineTest.exe
Pop-Location
