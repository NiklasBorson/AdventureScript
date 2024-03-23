param (
    [switch] $Release
)

$buildType = $Release ? 'Release' : 'Debug'
$exePath = "$PSScriptRoot/EngineTest/bin/$buildType/net8.0/EngineTest.exe"
$testFilesDir = "$PSScriptRoot/EngineTest/TestFiles"
$baselineDir = "$PSScriptRoot/EngineTest/Baseline"
$gamesDir = "$PSScriptRoot/Games"
$outputDir = "$PSScriptRoot/EngineTest/bin/$buildType/net8.0/Output"

# Run the test
Write-Output "Running $exePath..."
& $exePath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $outputDir
