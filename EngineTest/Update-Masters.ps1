param (
    [switch] $Release
)

# Determine the relative path of test binaries.
$buildType = $Release ? 'Release' : 'Debug'
$binDir = Join-Path 'bin' $buildType 'net8.0'

# Run the test
Write-Output "Running EngineTest.exe..."
Set-Location (Join-Path $PSScriptRoot $binDir)
.\EngineTest.exe

# Copy the output files to the baseline directory
Set-Location $PSScriptRoot
$sourceDir = Join-Path $binDir 'Output'
Get-ChildItem -File -Path $sourceDir | ForEach-Object {
    $sourcePath = Join-Path $sourceDir $_.Name
    $destPath = Join-Path 'Baseline' $_.Name

    if (-not (Test-Path $destPath)) {
        Write-Host "$sourcePath --> $destPath [new]"
        Copy-Item -Path $sourcePath -Destination $destPath -Force
    }
    else {
        Write-Host "$sourcePath --> $destPath [update]"
        Copy-Item -Path $sourcePath -Destination $destPath -Force
    }
}
