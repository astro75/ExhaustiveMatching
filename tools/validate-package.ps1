param(
    [string]$Configuration = "Release",
    [string]$OutputDirectory = $(Join-Path ([System.IO.Path]::GetTempPath()) "ExhaustiveMatchingPackageValidation")
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$nuspec = Join-Path $repoRoot "ExhaustiveMatching.Analyzer.nuspec"
$nuget = Join-Path $repoRoot "tools\nuget.exe"

if (!(Test-Path $nuget)) {
    throw "NuGet executable not found: $nuget"
}

if (Test-Path $OutputDirectory) {
    Remove-Item -LiteralPath $OutputDirectory -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

dotnet build (Join-Path $repoRoot "ExhaustiveMatch.sln") -c $Configuration --no-incremental
& $nuget pack $nuspec -OutputDirectory $OutputDirectory -Properties "Configuration=$Configuration"

if ($LASTEXITCODE -ne 0) {
    throw "nuget pack failed with exit code $LASTEXITCODE"
}

$package = Get-ChildItem -Path $OutputDirectory -Filter "ExhaustiveMatching.Analyzer.*.nupkg" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if ($null -eq $package) {
    throw "No ExhaustiveMatching.Analyzer package was created in $OutputDirectory"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::OpenRead($package.FullName)
try {
    $entries = $zip.Entries | ForEach-Object { $_.FullName }
    $requiredEntries = @(
        "lib/netstandard2.0/ExhaustiveMatching.dll",
        "analyzers/netstandard2.0/cs/ExhaustiveMatching.Analyzer.dll",
        "tools/install.ps1",
        "tools/uninstall.ps1"
    )

    foreach ($entry in $requiredEntries) {
        if ($entries -notcontains $entry) {
            throw "Package is missing required entry: $entry"
        }
    }

    $removedEntries = @(
        "analyzers/netstandard2.0/cs/ExhaustiveMatching.Analyzer.Enums.dll"
    )

    foreach ($entry in $removedEntries) {
        if ($entries -contains $entry) {
            throw "Package contains removed separate analyzer entry: $entry"
        }
    }
}
finally {
    $zip.Dispose()
}

Write-Host "Validated package: $($package.FullName)"
