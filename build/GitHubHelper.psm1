# Copyright (c) 2022 Matthias Wolf, Mawosoft.

<#
.SYNOPSIS
    Helpers for GitHub REST API.
#>

#Requires -Version 7

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

# .SYNOPSIS
#   Gets the workflow id for a given run id
function Get-WorkflowId {
    [CmdletBinding()]
    [OutputType([long])]
    param (
        # Github repository as owner/repo
        [Parameter(Mandatory, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [string]$OwnerRepo,

        # Run Id number
        [Parameter(Mandatory, Position = 1)]
        [ValidateRange([System.Management.Automation.ValidateRangeKind]::Positive)]
        [long]$RunId,

        # GitHub token
        [Alias('t')]
        [securestring]$Token
    )
    $auth = @{}
    if ($Token) {
        $auth.Authentication = 'Bearer'
        $auth.Token = $Token
    }
    $result = Invoke-RestMethod -Uri "https://api.github.com/repos/$OwnerRepo/actions/runs/$RunId" @auth
    return $result.workflow_id
}

# .SYNOPSIS
#   Finds the newest artifacts from a range of workflow runs
# .OUTPUTS
#   [PSCustomObject]@{
#       workflow_run = @{...}     # workflow_run object as returned by GitHub REST API
#       artifacts    = @(@{...})  # array of artifact objects as returned by GitHub REST API
#   }
function Find-ArtifactsFromPreviousRun {
    [CmdletBinding()]
    [OutputType([PSCustomObject])]
    param (
        # Github repository as owner/repo
        [Parameter(Mandatory, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [string]$OwnerRepo,

        # Artifact name. May contain wildcards.
        [Parameter(Mandatory, Position = 1)]
        [SupportsWildcards()]
        [ValidateNotNullOrEmpty()]
        [Alias('Artifact', 'a')]
        [string]$ArtifactName,

        # Workflow Id, either as number or as yaml file name
        [Parameter(Mandatory)]
        [string]$WorkflowId,

        # Number of the workflow run to start the search with
        [Parameter(Mandatory)]
        [ValidateRange([System.Management.Automation.ValidateRangeKind]::Positive)]
        [int]$MaxRunNumber,

        # Minimum run number to go back to until a matching artifact is found. Defaults to 1.
        [ValidateRange([System.Management.Automation.ValidateRangeKind]::Positive)]
        [int]$MinRunNumber = 1,

        # GitHub token
        [Alias('t')]
        [securestring]$Token
    )

    function GetArtifacts {
        [OutputType([PSCustomObject])]
        param (
            [PSCustomObject]$WorkflowRun
        )
        if ($WorkflowRun.artifacts_url) {
            $artifacts = Invoke-RestMethod -Uri $WorkflowRun.artifacts_url -FollowRelLink @auth
            $artifacts = $artifacts.artifacts | Where-Object {
                $_.name -Like $ArtifactName -and -not $_.expired
            }
            if ($artifacts) {
                return [PSCustomObject]@{
                    workflow_run = $WorkflowRun
                    artifacts    = if ($artifacts -is [array]) { $artifacts } else { @($artifacts) }
                }
            }
        }
    }

    if ($MaxRunNumber -lt $MinRunNumber) {
        throw "MaxRunNumber must be greater or equal MinRunNumber."
    }
    $auth = @{}
    if ($Token) {
        $auth.Authentication = 'Bearer'
        $auth.Token = $Token
    }
    $params = @{
        Uri           = "https://api.github.com/repos/$OwnerRepo/actions/workflows/$WorkflowId/runs"
        FollowRelLink = $false
    }

    $result = Invoke-RestMethod @params @auth
    [int]$runNumber = $MaxRunNumber
    $runs = $result.workflow_runs | Where-Object {
        $_.run_number -le $runNumber -and $_.run_number -ge $MinRunNumber
    } | Sort-Object -Property run_number -Descending
    foreach ($run in $runs) {
        if ($runNumber -ne $run.run_number) { break }
        $retVal = GetArtifacts $run
        if ($retVal) { return $retVal }
        $runNumber--
    }

    if ($runNumber -ge $MinRunNumber) {
        if ($result.total_count -gt $result.workflow_runs.Count) {
            # Lessons in paranoid coding...
            # Results returned by the GitHub REST API *seem* to always be in descending order, but the
            # docs make no mention of it. In real life, traversing the first page of the results above
            # is most likely enough when looking for the artifacts of the previous run.
            # Only if that page had gaps in the run mumbers and/or not enough entries to get a hit, we
            # will end up here to request all data.
            $params.FollowRelLink = $true
            Write-Verbose "Artifacts not found on first GitHub API result page. Retrieving all results."
            $result = Invoke-RestMethod @params @auth
            $runs = $result.workflow_runs | Where-Object {
                $_.run_number -le $runNumber -and $_.run_number -ge $MinRunNumber
            } | Sort-Object -Property run_number -Descending
        }
        foreach ($run in $runs) {
            $retVal = GetArtifacts $run
            if ($retVal) { return $retVal }
        }
    }
}

# .SYNOPSIS
#   Download and expand an artifact
function Expand-Artifact {
    [CmdletBinding()]
    param (
        # Artifact archive download URL
        [Parameter(Mandatory, Position = 0)]
        [ValidateNotNullOrEmpty()]
        [string]$Uri,

        # Output directory
        [Parameter(Mandatory, Position = 1)]
        [ValidateNotNullOrEmpty()]
        [string]$DestinationPath,

        # GitHub token
        [Parameter(Mandatory)]
        [Alias('t')]
        [securestring]$Token
    )

    [string]$archiveFile = New-TemporaryFile
    $null = Invoke-WebRequest -Uri $Uri -OutFile $archiveFile -Authentication Bearer -Token $Token
    $null = Expand-Archive -LiteralPath $archiveFile -DestinationPath $DestinationPath
    Remove-Item $archiveFile -ErrorAction SilentlyContinue
}

Export-ModuleMember -Function @(
    'Get-WorkflowId',
    'Find-ArtifactsFromPreviousRun',
    'Expand-Artifact'
)
