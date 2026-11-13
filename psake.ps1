# Helper script to run the psake copy vendored in Tools\psake without installing the module.
# Example:
# .\psake.ps1 "psakefile.ps1" "Build"
# .\psake.cmd build

# Mirrors the parameters of Invoke-psake (Tools\psake\public\Invoke-psake.ps1).
param(
    [Parameter(Position=0,Mandatory=0)]
    [string]$buildFile = 'psakefile.ps1',
    [Parameter(Position=1,Mandatory=0)]
    [string[]]$taskList = @(),
    [Parameter(Position=2,Mandatory=0)]
    [string]$framework,
    [Parameter(Position=3,Mandatory=0)]
    [switch]$docs = $false,
    [Parameter(Position=4,Mandatory=0)]
    [System.Collections.Hashtable]$parameters = @{},
    [Parameter(Position=5, Mandatory=0)]
    [System.Collections.Hashtable]$properties = @{},
    [Parameter(Position=6, Mandatory=0)]
    [alias("init")]
    [scriptblock]$initialization = {},
    [Parameter(Position=7, Mandatory=0)]
    [switch]$nologo = $false,
    [Parameter(Position=8, Mandatory=0)]
    [switch]$help = $false,
    [Parameter(Position=9, Mandatory=0)]
    [string]$scriptPath = $(Split-Path -parent $MyInvocation.MyCommand.path)
)

# '[p]sake' is the same as 'psake' but $Error is not polluted
remove-module [p]sake
import-module (join-path $scriptPath 'Tools\psake\psake.psd1')
if ($help) {
  Get-Help Invoke-psake -full
  return
}

# The build file is 'psakefile.ps1', the psake 5 default name, next to this script.
$defaultBuildFile = join-path $scriptPath 'psakefile.ps1'

if (-not(test-path $buildFile -pathType Leaf)) {
    $absoluteBuildFile = (join-path $scriptPath $buildFile)
    if (test-path $absoluteBuildFile -pathType Leaf) {
        $buildFile = $absoluteBuildFile
    } elseif (test-path $defaultBuildFile -pathType Leaf) {
        # '.\psake build': the first argument is a task list, not a build file
        $taskList = $buildFile.Split(', ', [System.StringSplitOptions]::RemoveEmptyEntries)
        $buildFile = $defaultBuildFile
    }
}

$invokeArgs = @{
    BuildFile      = $buildFile
    Docs           = $docs
    Parameters     = $parameters
    Properties     = $properties
    Initialization = $initialization
    NoLogo         = $nologo
}
if ($taskList.Count -gt 0) { $invokeArgs.TaskList = $taskList }
if ($framework) { $invokeArgs.Framework = $framework }

# Invoke-psake 5 returns a PsakeBuildResult object; keep it off the console.
invoke-psake @invokeArgs | Where-Object { $_ -eq $null -or $_.GetType().Name -ne 'PsakeBuildResult' }
