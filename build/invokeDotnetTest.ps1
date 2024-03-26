# Copyright (c) 2022-2024 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Run dotnet test for projects * configs * frameworks.
#>

#Requires -Version 7.4

Param (
    [Parameter(Mandatory)]
    [Alias('p')]
    [string[]]$Projects,

    [ValidateSet('Debug', 'Release')]
    [Alias('c')]
    [string[]]$Configs = @('Debug', 'Release'),

    [Parameter(Mandatory)]
    # Subset of Microsoft.NET.SupportedTargetFrameworks.props
    [ValidateSet('net461', 'net462', 'net472', 'net48', 'netstandard2.0', 'netcoreapp3.1', 'net5.0', 'net6.0', 'net7.0', 'net8.0', 'net9.0')]
    [Alias('f')]
    [string[]]$Frameworks,

    [ValidateSet('quiet', 'minimal', 'normal', 'detailed')]
    [Alias('v')]
    [string]$Verbosity = 'minimal',

    [Alias('b')]
    # Doesn't add --no-build to dotnet test args
    [switch]$Build,

    [Alias('ff')]
    # Stop after first failing test
    [switch]$FailFast,

    [Alias('r')]
    # Defaults to ./TestResults
    [string]$ResultsDirectory,

    [Alias('cc')]
    [switch]$CodeCoverage,

    [Alias('s')]
    # If provided, turns on -cc automatically
    [string]$Settings,

    [Alias('a')]
    # Any additional dotnet test args.
    [string[]]$AdditionalArguments
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

[int]$matrixCount = $Projects.Count * $Configs.Count * $Frameworks.Count
[int]$failedTestCount = 0

# Init CI log grouping if available
[string]$groupPrefix = [System.Environment]::NewLine
[string]$groupSuffix = ''

if ($env:GITHUB_ACTIONS -and $matrixCount -gt 1) {
    $groupPrefix = '::group::'
    $groupSuffix = '::endgroup::'
}
elseif ($env:TF_BUILD -and $matrixCount -gt 1) {
    $groupPrefix = '##[group]'
    $groupSuffix = '##[endgroup]'
}

if ($ResultsDirectory -eq '') {
    $ResultsDirectory = Join-Path '.' 'TestResults'
}

# Run test matrix
foreach ($project in $Projects) {
    if ($Projects.Count -eq 1) {
        [string]$projectResultsDirectory = $ResultsDirectory
    }
    else {
        [string]$projectResultsDirectory =
        Join-Path $ResultsDirectory ([System.IO.Path]::GetFileNameWithoutExtension($project))
    }
    foreach ($config in $Configs) {
        foreach ($framework in $Frameworks) {

            [string]$currentResultsDirectory = Join-Path $projectResultsDirectory $config $framework

            # Clear result directory
            Remove-Item (Join-Path $currentResultsDirectory '*') -Recurse -ErrorAction Ignore

            # Create dotnet command line
            [string[]] $params = @('test', "$project", '--nologo')
            if (-not $Build.IsPresent) {
                $params += '--no-build'
            }
            # Dont't use aliases like -r, they could change (-r is now --runtime in SDK >= 7.0)
            # See https://github.com/dotnet/sdk/issues/21952
            $params += '--configuration', $config
            $params += '--framework', $framework
            $params += '--logger', "console;verbosity=$Verbosity"
            $params += '--results-directory', $currentResultsDirectory
            $params += '--logger', 'trx'
            if ($CodeCoverage.IsPresent -or $Settings -ne '') {
                $params += '--collect', 'XPlat Code Coverage'
                if ($Settings -ne '') {
                    $params += '--settings', $Settings
                }
            }
            $params += '--diag', $(Join-Path $currentResultsDirectory 'diag.log')
            if ($AdditionalArguments) {
                $params += $AdditionalArguments
            }

            # Execute and log dotnet test
            Write-Host ($groupPrefix + 'dotnet') $params -Separator ' '
            [bool]$hasTestErrors = $false
            try {
                dotnet $params
                if ($LASTEXITCODE -ne 0) { $hasTestErrors = $true }
            }
            catch {
                $hasTestErrors = $true
            }
            Write-Host $groupSuffix
            if ($hasTestErrors) {
                $failedTestCount++
                if ($FailFast.IsPresent) {
                    throw 'Test completed with failures and -FailFast was specified.'
                }
                if ($matrixCount -gt 1) {
                    Write-Warning 'Test completed with failures.'
                }
            }

            # Copy coverage results (if any) from random subdir into results dir, then remove subdirs
            # Directory may not exist if the test didn't run
            if ([System.IO.Directory]::Exists($currentResultsDirectory)) {
                Get-ChildItem (Join-Path $currentResultsDirectory '*' '*') -File |
                    Copy-Item -Destination $currentResultsDirectory
                Get-ChildItem $currentResultsDirectory -Directory | Remove-Item -Recurse
            }
        }
    }
}

if ($failedTestCount -gt 0) {
    throw 'Some tests were not successful.'
}
