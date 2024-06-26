# Current configuration ('Debug' or 'Release')
$ConfigName = 'Debug'

# Well-known directories
$TestFilesDir = Join-Path $PSScriptRoot 'AdventureTest' 'TestFiles'
$BaselineDir = Join-Path $PSScriptRoot 'AdventureTest' 'Baseline'
$TestOutputDir = Join-Path $PSScriptRoot 'AdventureTest' 'bin' 'Output'
$GamesDir = Join-Path $PSScriptRoot 'Games'

function Get-DllPath([string] $BaseName) {
    Join-Path $PSScriptRoot $BaseName 'bin' $ConfigName 'net8.0' "$BaseName.dll"
}

<#
.SYNOPSIS
Gets the current configuration.
#>
function Get-Config {
    Write-Output "Current configuration is $ConfigName. Use Set-Config to change."
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
        $script:ConfigName = 'Release'
    }
    elseif ($Debug -and (-not $Release)) {
        $script:ConfigName = 'Debug'
    }
    else {
        Write-Error "Error: Must specify either -Release or -Debug."
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
    $dllPath = Get-DllPath('AdventureTest')
    Write-Output "dotnet $dllPath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $TestOutputDir"
    & dotnet $dllPath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $TestOutputDir
}

<#
.SYNOPSIS
Updates baseline files used by AdventureTest.

.DESCRIPTION
If AdventureTest reports comparison failures, and the differences are expected, run this
command to copy the test output files to the baseline directory.
#>
function Update-Baselines {
    if (Test-Path $TestOutputDir) {
        Copy-Item -Path "$TestOutputDir\*" -Destination $BaselineDir -Force
    }
    else {
        Write-Error "Error: $TestOutputDirectory does not exist."
        Write-Error "       Try running Invoke-Tests."
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
Name of the game, as returned by Get-Games.

.EXAMPLE
Invoke-Game Demo
#>
function Invoke-Game([string] $Name) {
    $FilePath = Join-Path $GamesDir $Name 'adventure.txt'
    if (Test-Path $FilePath) {
        $dllPath = Get-DllPath('TextAdventure')
        Write-Output "dotnet $dllPath $FilePath"
        & dotnet $dllPath $FilePath
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
Path of the destination .adventure file.

.EXAMPLE
Build-Game -Path .\Games\Demo -Destination .\OxbowCastle\Assets\Games\Demo.adventure
#>
function Build-Game([string] $Path, [string] $Destination) {

    # Make sure the source adventure.txt file exists.
    $sourceFile = Join-Path $Path 'adventure.txt'
    if (-not (Test-Path $sourceFile)) {
        Write-Error "Error: $sourceFile does not exist."
        return
    }

    # Create a temporary directory that will contain the files to be
    # stored in the archive.
    $tempDir = New-Guid
    mkdir $tempDir | Out-Null

    # Compile adventure.txt. Compiling loads the game and then saves the
    # initial game state. This eliminates any dependencies on include files.
    $tempFile = Join-Path $tempDir 'adventure.txt'
    $dllPath = Get-DllPath('TextAdventure')
    Write-Host "dotnet $dllPath -compile $sourceFile $tempFile"
    & dotnet $dllPath -compile $sourceFile $tempFile

    # Copy other files except .txt and .md files.
    Get-ChildItem (Join-Path $Path '*') -File -Exclude '*.txt','*.md' | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $tempDir
    }

    # Create the compressed file.
    Write-Host "Compress-Archive -Path $tempDir -DestinationPath $Destination"
    Compress-Archive -Path (Join-Path $tempDir '*') -DestinationPath $Destination

    # Delete the temporary directory.
    Remove-Item -Recurse $tempDir
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

    Remove-Item -Recurse (Join-Path $destRoot '*')

    Get-ChildItem $GamesDir -Directory | ForEach-Object {
        if (Test-Path (Join-Path $_.FullName 'adventure.txt')) {
            Build-Game -Path $_.FullName -Destination (Join-Path $destRoot "$($_.Name).adventure")
        }
    }
}

<#
.SYNOPSIS
Captures a trace of the specified game for testing.

.DESCRIPTION
Prompts the user to play through the specified game and captures a trace of the
commands and game output. The captured trace is used by AdventureTest to detect if
the game behavior changes unexpectedly.

.PARAMETER Name
Name of the game to play. This should match the name of a subdirectory under
the Games directory.

.EXAMPLE
Build-GameTrace Demo
#>
function Build-GameTrace([string] $Name) {

    # Path of TextAdventure.exe
    $dllPath = Get-DllPath('TextAdventure')

    # Path of the adventure.txt file
    $gameFilePath = Join-Path $PSScriptRoot 'Games' $Name 'adventure.txt'

    # Paths of the output files
    $Name = $Name.Replace(' ', '-');
    $traceFilePath = Join-Path $PSScriptRoot 'AdventureTest' 'Baseline' "$Name-trace.txt"
    $commandFilePath = Join-Path $PSScriptRoot 'AdventureTest' 'TestFiles' "$Name-input.txt"

    # Play the game, capturing a trace
    & dotnet $dllPath -trace $gameFilePath $traceFilePath

    # Extract the commands from the trace and save as a test input file
    Get-Content $traceFilePath |
        Where-Object { $_.StartsWith('> ') } |
        ForEach-Object { $_.Substring(2) } |
        Set-Content $commandFilePath
}

<#
.SYNOPSIS
Builds the AdventureScript API reference in HTML format.

.DESCRIPTION
Invokes the AdventureDoc tool to generate HTML documentation for all of the APIs in
the AdventureScript foundation library as well as intrinsics.
#>
function Build-Docs {
    $dllPath = Get-DllPath('AdventureDoc')
    $inputPath = Join-Path $GamesDir 'inc' 'all.txt'
    $outputDir = Join-Path $PSScriptRoot 'docs' 'ApiRef'
    $indexTitle = 'API Reference'
    $headingText = 'AdventureScript'
    $headingUrl = 'https://niklasborson.github.io/AdventureScript'

    Remove-Item -Path (Join-Path $outputDir '*.html')

    Write-Host "dotnet $dllPath $inputPath $outputDir $indexTitle $headingText $headingUrl"
    & dotnet $dllPath $inputPath $outputDir $indexTitle $headingText $headingUrl
}

<#
.SYNOPSIS
Builds the AdventureScript solution for the current configuration.
#>
function Build-AdventureScript {
    if ($IsWindows) {
        # Build the entire solution on Windows
        & dotnet build (Join-Path $PSScriptRoot 'AdventureScript.sln') --configuration $ConfigName
    }
    else {
        # Build selected directories on other platforms
        'AdventureScript', 'AdventureDoc', 'AdventureTest', 'TextAdventure' | Foreach-Object {
            & dotnet build (Join-Path $PSScriptRoot $_ "$_.csproj") --configuration $ConfigName
        }
    }
}
Set-Alias build Build-AdventureScript

function Get-PackageForArch([string] $arch) {
    $bestDate = [DateTime]0
    $bestMatch = $null

    Get-ChildItem -Recurse -Path (Join-Path $PSScriptRoot 'OxbowCastle' "*$arch.msix") | ForEach-Object {
        if ($_.CreationTimeUtc -gt $bestDate) {
            $bestDate = $_.CreationTime
            $bestMatch = $_.FullName
        }
    }

    Write-Output $bestMatch
}

<#
.SYNOPSIS
Gets the path of the most recent OxbowCastle MSIX file for each CPU architecture.
#>
function Get-Packages {
    Get-PackageForArch 'x64'
    Get-PackageForArch 'arm64'
}

<#
.SYNOPSIS
Get a list of functions exported by the Adventure-Helpers module.
#>
function Get-AdventureHelp {
    'Adventure helper commands:'
    (Get-Module 'Adventure-Helpers').ExportedCommands.Values | 
        Where-Object { $_.CommandType -eq 'Function' } | 
        ForEach-Object { '- ' + $_.Name }

    ''
    'Aliases:'
    (Get-Module 'Adventure-Helpers').ExportedCommands.Values | 
        Where-Object { $_.CommandType -eq 'Alias' } | 
        ForEach-Object { '- ' + $_.DisplayName }

    ''
    'Type Get-Help <name> -Detailed for help on a specific command'
    ''
}

Write-Host "Adventure Helpers module loaded."
Get-Config | Out-Host
Write-Host "Type Get-AdventureHelp to see a list of exported functions."
Write-Host

Export-ModuleMember -Function Get-Config
Export-ModuleMember -Function Set-Config
Export-ModuleMember -Function Invoke-Tests
Export-ModuleMember -Function Update-Baselines
Export-ModuleMember -Function Get-Games
Export-ModuleMember -Function Invoke-Game
Export-ModuleMember -Function Build-Game
Export-ModuleMember -Function Build-AllGames
Export-ModuleMember -Function Build-GameTrace
Export-ModuleMember -Function Build-Docs
Export-ModuleMember -Function Build-AdventureScript -Alias build
Export-ModuleMember -Function Get-Packages
Export-ModuleMember -Function Get-AdventureHelp
