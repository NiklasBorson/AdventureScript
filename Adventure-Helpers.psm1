$IsRelease = $false

$TestFilesDir = "$PSScriptRoot/EngineTest/TestFiles"
$BaselineDir = "$PSScriptRoot/EngineTest/Baseline"
$GamesDir = "$PSScriptRoot/Games"
$TestOutputDir = "$PSScriptRoot/EngineTest/bin/Output"

function Get-ExePath([string] $BaseName) {
    $buildType = $IsRelease ? 'Release' : 'Debug'
    "$PSScriptRoot/$BaseName/bin/$buildType/net8.0/$BaseName.exe"
}

<#
.SYNOPSIS
Sets the build configuration.

.DESCRIPTION
Sets the build configuration to either Debug or Release. This affects wither
other commands invoke debug or release versions of binaries.

.PARAMETER Release
Specifies release configuration.

.PARAMETER Debug
Specifies debug configuration. This is the default.

.EXAMPLE
Set-Config -Debug
Set-Config -Release
#>
function Set-Config([switch] $Release, [switch] $Debug) {
    if ($Release -and (-not $Debug)) {
        $script:IsRelease = $true
    }
    elseif ($Debug -and (-not $Release)) {
        $script:IsRelease = $false
    }
    else {
        Write-Error "Error: Must specify -release or -debug."
    }
}

<#
.SYNOPSIS
Runs tests.
#>
function Invoke-Tests {

    # Delete the previous output directory, if any
    if (Test-Path $TestOutputDir) {
        Remove-Item $TestOutputDir -Recurse
    }

    # Run the test
    $exePath = Get-ExePath('EngineTest')
    Write-Output "$exePath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $TestOutputDir"
    & $exePath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $TestOutputDir
}

<#
.SYNOPSIS
Updates baseline files used by EngineTest.

.DESCRIPTION
If EngineTest reports comparison failures, and the differences are expected, run this
command to copy the test output files to the baseline directory.
#>
function Update-Baselines {
    if (Test-Path $TestOutputDir) {
        Copy-Item -Path "$TestOutputDir\*" -Destination $BaselineDir -Force
    }
    else {
        Write-Error "Error: $TestOutputDirectory does not exist."
        Write-Error "       Try running Invoke-Test."
    }
}

<#
.SYNOPSIS
Gets a list of all the games in the project.
#>
function Get-Games {
    (Get-ChildItem -Directory $GamesDir | Where-Object { Test-Path (Join-Path $_ 'adventure.txt') }).Name
}

<#
.SYNOPSIS
Runs the specified game.

.DESCRIPTION
Runs the specified by by launching the TextAdventure console application.

.PARAMETER Name
Name of the game, as returened by Get-Games.

.EXAMPLE
Invoke-Game Demo
#>
function Invoke-Game([string] $Name) {
    $FilePath = Join-Path $GamesDir $Name 'adventure.txt'
    if (Test-Path $FilePath) {
        $exePath = Get-ExePath('TextAdventure')
        Write-Output "$exePath $FilePath"
        & $exePath $FilePath
    }
    else {
        Write-Error "Error: $FilePath does not exist."        
    }
}

<#
.SYNOPSIS
Copies a "compiled" version of a game to a destination directory.

.DESCRIPTION
The command loads the adventure.txt file in the specified input directory,
saves the initial game state as an adventure.txt file in the destination
directory, and copies any image files from the input directory to the
destination directory.

.PARAMETER Path
Path of the input directory containing adventure.txt.

.PARAMETER Destination
Path of the destination directory.

.EXAMPLE
Build-Game -Path .\Games\Demo -Destination .\OxbowCastle\Assets\Games\Demo

.NOTES
Compilation means loading the game and then saving the initial game state.
This eliminates dependencies on any include files.
#>
function Build-Game([string] $Path, [string] $Destination) {

    $sourceFile = Join-Path $Path 'adventure.txt'
    if (-not (Test-Path $sourceFile)) {
        Write-Error "Error: $sourceFile does not exist."
        return
    }

    if (-not (Test-Path $Destination)) {
        mkdir $Destination | Out-Null
    }

    $destFile = Join-Path $Destination 'adventure.txt'
    $exePath = Get-ExePath('TextAdventure')

    # Compile adventure.txt.
    Write-Host "$exePath -compile $sourceFile $destFile"
    & $exePath -compile $sourceFile $destFile

    # Copy other files except .txt and .md files.
    Get-ChildItem (Join-Path $Path '*') -File -Exclude '*.txt','*.md' | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $Destination
    }
}

<#
.SYNOPSIS
Adds compiled versions of all the games to the OxbowCastle app's assets.

.DESCRIPTION
Invokes BuildGame for each game in the Games directory. The destination directory
is the corresponding subdirectory under OxbowCastle\Assets\Games.
#>
function Build-AllGames {
    $destRoot = Join-Path $PSScriptRoot 'OxbowCastle' 'Assets' 'Games'

    Get-ChildItem $GamesDir -Directory | ForEach-Object {
        if (Test-Path (Join-Path $_.FullName 'adventure.txt')) {
            Build-Game -Path $_.FullName -Destination (Join-Path $destRoot $_.Name)
        }
    }
}

<#
.SYNOPSIS
Captures a trace of the specified game for testing.

.DESCRIPTION
Prompts the user to play through the specified game and captures a trace of the
commands and game output. The captured trace is used by EngineTest to detect if
the game behavior changes unexpectedly.

.PARAMETER Name
Name of the game to play. This should match the name of a subdirectory under
the Games directory.

.EXAMPLE
Build-GameTrace Demo
#>
function Build-GameTrace([string] $Name) {

    # Path of TextAdventure.exe
    $exePath = Get-ExePath('TextAdventure')

    # Path of the adventure.txt file
    $gameFilePath = Join-Path $PSScriptRoot 'Games' $Name 'adventure.txt'

    # Paths of the output files
    $Name = $Name.Replace(' ', '-');
    $traceFilePath = Join-Path $PSScriptRoot 'EngineTest' 'Baseline' "$Name-trace.txt"
    $commandFilePath = Join-Path $PSScriptRoot 'EngineTest' 'TestFiles' "$Name-input.txt"

    # Play the game, capturing a trace
    & $exePath -trace $gameFilePath $traceFilePath

    # Extract the commands from the trace and save as a test input file
    Get-Content $traceFilePath |
        Where-Object { $_.StartsWith('> ') } |
        ForEach-Object { $_.Substring(2) } |
        Set-Content $commandFilePath
}

<#
.SYNOPSIS
Get a list of functions exported by the Adventure-Helpers module.
#>
function Get-AdventureHelp {
    (Get-Module 'Adventure-Helpers').ExportedCommands.Values | 
        Where-Object { $_.CommandType -eq 'Function' } | 
        ForEach-Object { Write-Output $_.Name }
}

Export-ModuleMember -Function Set-Config
Export-ModuleMember -Function Invoke-Tests
Export-ModuleMember -Function Update-Baselines
Export-ModuleMember -Function Get-Games
Export-ModuleMember -Function Invoke-Game
Export-ModuleMember -Function Build-Game
Export-ModuleMember -Function Build-AllGames
Export-ModuleMember -Function Build-GameTrace
Export-ModuleMember -Function Get-AdventureHelp
