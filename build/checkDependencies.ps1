# Copyright (c) 2022 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Check solution/project dependencies for outdated/vulnerable/deprecated

.DESCRIPTION
    Runs all 'dotnet list package' reports and discovers new dependency problems by comparing
    the results to artifacts from previous workflow runs.The script will download the previous
    artifact, but the caller is responsible for uploading the new one.
    Creates an issue, if new dependency problems have been found.

.OUTPUTS
    None. Sets the following workflow step output parameters via workflow commands issued by Write-Host.
        ArtifactName - Artifact name for the result file to share between workflow runs.
        ArtifactPath - Artifact path for the result file.
        IssueNumber  - Issue with report of tool dependency problems.
#>

#Requires -Version 7

using namespace System
using namespace System.IO
using namespace System.Collections.Generic
using namespace System.Text
using module ./ListPackageHelper.psm1
using namespace ListPackageHelper

[CmdletBinding()]
param (
    # Project or solution files to process. Defaults to the solution or project
    # in the current directory
    [Parameter(Position = 0)]
    [Alias('p')]
    [string[]]$Projects,

    # 'list package' options to run for each project/solution
    [ValidateNotNullOrEmpty()]
    [Alias('o')]
    [string[][]]$OptionsMatrix = (
        ('--outdated', '--include-transitive'),
        ('--vulnerable', '--include-transitive'),
        ('--deprecated', '--include-transitive')
    ),

    [Parameter(Mandatory)]
    [Alias('Token', 't')]
    [securestring]$GitHubToken,

    [ValidateNotNullOrEmpty()]
    [Alias('Artifact', 'a')]
    [string]$ArtifactName = 'DependencyCheck',

    [Alias('Labels', 'l')]
    [string[]]$IssueLabels = @('dependencies')
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

# GitHub data about current workflow run and repository
if (-not $env:GITHUB_RUN_ID -or -not $env:GITHUB_RUN_NUMBER -or
    -not $env:GITHUB_REPOSITORY -or -not $env:GITHUB_OUTPUT) {
    throw 'GitHub environment variables are not defined.'
}

Import-Module "$PSScriptRoot/GitHubHelper.psm1" -Force

[ListPackageResult]$previousResult = $null
[ListPackageResult]$result = $null

# Download artifact from previous workflow run
[string]$artifactDirectory = Join-Path ([Path]::GetTempPath()) ([Path]::GetRandomFileName())
[string]$artifactFile = Join-Path $artifactDirectory 'LastResult.json'
$null = [Directory]::CreateDirectory($artifactDirectory)
[int]$runNumber = $env:GITHUB_RUN_NUMBER
[long]$workflowId = Get-WorkflowId $env:GITHUB_REPOSITORY $env:GITHUB_RUN_ID -Token $GitHubToken
if ($runNumber -gt 1) {
    $artifacts = Find-ArtifactsFromPreviousRun $env:GITHUB_REPOSITORY $ArtifactName -WorkflowId $workflowId `
        -MaxRunNumber ($runNumber - 1) -Token $GitHubToken
    if ($artifacts) {
        Write-Host "Found artifact '$ArtifactName' from workflow run #$(
            $artifacts.workflow_run.run_number) on $($artifacts.workflow_run.created_at) UTC."
        Expand-Artifact $artifacts.artifacts[0].archive_download_url $artifactDirectory -Token $GitHubToken
        $previousResult = [ListPackageResult]::CreateFromJson((Get-Content $artifactFile -Raw))
    }
}

Write-Host '::group::Raw tool output'
try {
    $result = Invoke-ListPackage -p $Projects -o $OptionsMatrix -InformationAction 'Continue'
}
finally {
    Write-Host '::endgroup::'
}

$result.ToJson($true) | Set-Content $artifactFile
Write-Output "ArtifactName=$ArtifactName" >>$env:GITHUB_OUTPUT
Write-Output "ArtifactPath=$artifactFile" >>$env:GITHUB_OUTPUT

[MergedPackageRef[]]$toplevel = [MergedPackageRef]::Create(($result.Packages.Values | Where-Object RefType -EQ TopLevel))
[MergedPackageRef[]]$transitive = [MergedPackageRef]::Create(($result.Packages.Values | Where-Object RefType -EQ Transitive))
if ($toplevel -or $transitive) {
    Write-Host '::group::All results'
    if ($toplevel) { [MergedPackageRef]::FormatTable($toplevel, 'Top-level Packages', $true) }
    if ($transitive) { [MergedPackageRef]::FormatTable($transitive, 'Transitive Packages', $true) }
    Write-Host '::endgroup::'
}
else {
    Write-Host 'All results: No packages matching the given criteria have been found.'
}

[MergedPackageRef[]]$diffToplevel = $null
[MergedPackageRef[]]$diffTransitive = $null
if ($previousResult) {
    [ListPackageComparison]$diff = [ListPackageComparison]::new($previousResult, $result)
    [HashSet[string]]$diffKeys = $diff.RightOnly
    $diffKeys.UnionWith($diff.Changed)
    [List[ParsedPackageRef]]$diffPackages = [List[ParsedPackageRef]]::new($diffKeys.Count)
    foreach ($key in $diffKeys) {
        $diffPackages.Add($result.Packages[$key])
    }
    [MergedPackageRef[]]$diffToplevel = [MergedPackageRef]::Create(($diffPackages | Where-Object RefType -EQ TopLevel))
    [MergedPackageRef[]]$diffTransitive = [MergedPackageRef]::Create(($diffPackages | Where-Object RefType -EQ Transitive))
    if ($diffToplevel -or $diffTransitive) {
        Write-Host '::group::New results'
        if ($diffToplevel) { [MergedPackageRef]::FormatTable($diffToplevel, 'Top-level Packages', $true) }
        if ($diffTransitive) { [MergedPackageRef]::FormatTable($diffTransitive, 'Transitive Packages', $true) }
        Write-Host '::endgroup::'
    }
    else {
        Write-Host 'New results: No new packages matching the given criteria have been found.'
    }
}

[List[string]]$notice = [List[string]]::new()
if ($diffToplevel -or $diffTransitive) {
    $notice.Add("New dependency problems: $(${diffToplevel}?.Count + 0)/$(${diffTransitive}?.Count + 0)")
}
if ($toplevel -or $transitive) {
    $notice.Add("All dependency problems: $(${toplevel}?.Count + 0)/$(${transitive}?.Count + 0)")
}

if ($diffToplevel -or $diffTransitive -or (-not $previousResult -and ($toplevel -or $transitive))) {
    [hashtable]$auth = @{
        Authentication = 'Bearer'
        Token          = $GitHubToken
    }

    [string]$uri = "https://api.github.com/repos/$($env:GITHUB_REPOSITORY)/actions/workflows/$workflowId"
    $workflow = Invoke-RestMethod -Uri $uri @auth
    $uri = "https://api.github.com/repos/$($env:GITHUB_REPOSITORY)/actions/runs/$($env:GITHUB_RUN_ID)"
    $run = Invoke-RestMethod -Uri $uri @auth
    # Note: $workflow.html_url points to the source file, not the overview of workflow runs.
    [string]$urlWorkflowRuns = "https://github.com/$($env:GITHUB_REPOSITORY)/actions/workflows/$(Split-Path $workflow.path -Leaf)"

    [StringBuilder]$body = [StringBuilder]::new()
    $null = $body.AppendLine("Workflow [$($workflow.name)]($urlWorkflowRuns) Run [#$($run.run_number)]($($run.html_url))").AppendLine()

    if ($diffToplevel -or $diffTransitive) {
        $null = $body.AppendLine("### New Dependency Problems ($(${diffToplevel}?.Count + 0)/$(${diffTransitive}?.Count + 0))")
        if ($diffToplevel) {
            $null = $body.AppendLine('<details><summary>Top-level Packages</summary>').AppendLine()
            $null = $body.AppendLine([MergedPackageRef]::FormatMarkdownHtmlTable($diffToplevel, 'Package', 1, $true))
            $null = $body.AppendLine('</details>').AppendLine()
        }
        if ($diffTransitive) {
            $null = $body.AppendLine('<details><summary>Transitive Packages</summary>').AppendLine()
            $null = $body.AppendLine([MergedPackageRef]::FormatMarkdownHtmlTable($diffTransitive, 'Package', 1, $true))
            $null = $body.AppendLine('</details>').AppendLine()
        }
    }
    if ($toplevel -or $transitive) {
        $null = $body.AppendLine("### All Dependency Problems ($(${toplevel}?.Count + 0)/$(${transitive}?.Count + 0))")
        if ($toplevel) {
            $null = $body.AppendLine('<details><summary>Top-level Packages</summary>').AppendLine()
            $null = $body.AppendLine([MergedPackageRef]::FormatMarkdownHtmlTable($toplevel, 'Package', 1, $true))
            $null = $body.AppendLine('</details>').AppendLine()
        }
        if ($transitive) {
            $null = $body.AppendLine('<details><summary>Transitive Packages</summary>').AppendLine()
            $null = $body.AppendLine([MergedPackageRef]::FormatMarkdownHtmlTable($transitive, 'Package', 1, $true))
            $null = $body.AppendLine('</details>').AppendLine()
        }
    }

    [hashtable]$params = @{
        title = 'Dependency Alert'
        body  = $body.ToString()
    }
    if ($IssueLabels) { $params.labels = $IssueLabels }
    $uri = "https://api.github.com/repos/$env:GITHUB_REPOSITORY/issues"
    $issue = $params | ConvertTo-Json -EscapeHandling EscapeNonAscii | Invoke-RestMethod -Uri $uri -Method Post @auth
    Write-Output "IssueNumber=$($issue.number)" >>$env:GITHUB_OUTPUT
    $notice.Add("Created Dependency Alert. Issue #$($issue.number): $($issue.html_url)")
}
if ($notice.Count -gt 0) {
    # Note: While %0A will appear as newline in the console log,
    # the annotation in the workflw run summary will just be a single line.
    Write-Host "::notice::$($notice -join '%0A')"
}
