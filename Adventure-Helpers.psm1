$IsRelease = $false

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

$TestFilesDir = "$PSScriptRoot/EngineTest/TestFiles"
$BaselineDir = "$PSScriptRoot/EngineTest/Baseline"
$GamesDir = "$PSScriptRoot/Games"
$TestOutputDir = "$PSScriptRoot/EngineTest/bin/Output"

function Get-ExePath([string] $BaseName) {
    $buildType = $IsRelease ? 'Release' : 'Debug'
    "$PSScriptRoot/$BaseName/bin/$buildType/net8.0/$BaseName.exe"
}

function Invoke-Test {

    # Delete the previous output directory, if any
    if (Test-Path $TestOutputDir) {
        Remove-Item $TestOutputDir -Recurse
    }

    # Run the test
    $exePath = Get-ExePath('EngineTest')
    Write-Output "$exePath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $TestOutputDir"
    & $exePath -testfiles $testFilesDir -baseline $baselineDir -games $gamesDir -output $TestOutputDir
}

function Update-Masters {
    if (Test-Path $TestOutputDir) {
        Copy-Item -Path "$TestOutputDir\*" -Destination $BaselineDir -Force
    }
    else {
        Write-Error "Error: $TestOutputDirectory does not exist."
        Write-Error "       Try running Invoke-Test."
    }
}

function Get-Games {
    (Get-ChildItem -Path $GamesDir -File).Name
}

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

function Build-AllGames {
    $destRoot = Join-Path $PSScriptRoot 'OxbowCastle' 'Assets' 'Games'

    Get-ChildItem $GamesDir -Directory | ForEach-Object {
        if (Test-Path (Join-Path $_.FullName 'adventure.txt')) {
            Build-Game -Path $_.FullName -Destination (Join-Path $destRoot $_.Name)
        }
    }
}

function Get-CommandsFromTrace([string] $TraceFile)
{
    Get-Content $TraceFile | Where-Object { $_.StartsWith('> ') } | ForEach-Object { $_.Substring(2) }
}

Export-ModuleMember -Function Set-Config
Export-ModuleMember -Function Invoke-Test
Export-ModuleMember -Function Update-Masters
Export-ModuleMember -Function Get-Games
Export-ModuleMember -Function Invoke-Game
Export-ModuleMember -Function Build-Game
Export-ModuleMember -Function Build-AllGames
Export-ModuleMember -Function Get-CommandsFromTrace
