# 0001. Vendor psake in the repository

- Status: accepted
- Date: 2026-11-13

## Context
The build is driven by psake (`./psake build`). The repository has always carried its own copy of psake (4.2.0, a single `psake.psm1` at the root) so that a fresh clone builds without installing anything. psake 5.x is no longer a single file: the module is a manifest plus `public/`, `private/`, `classes/`, `enums/` and localisation folders.

The alternatives to carrying a copy are installing the module (`Install-Module psake`), restoring it as a package, or downloading it at build time. All of them add a network step and a machine-level prerequisite to the build.

## Decision
psake stays vendored, as decided by the maintainer on 2026-11-13.

- The vendored copy is psake **5.0.4**, the unmodified contents of the PowerShell Gallery package (`Save-Module psake -Repository PSGallery -RequiredVersion 5.0.4`), minus the `PSGetModuleInfo.xml` file that `Save-Module` adds.
- It lives in `Tools/psake/`. Do not edit files in that folder; repo-specific behaviour belongs in the wrapper.
- `psake.cmd` and `psake.ps1` stay at the repository root as the entry point. `psake.ps1` is this repository's own wrapper (the gallery package does not ship one). It imports `Tools\psake\psake.psd1`, maps `.\psake <task>` onto `default.ps1`, and keeps the `PsakeBuildResult` object that `Invoke-psake` 5 returns off the console.
- To upgrade: replace the contents of `Tools/psake/` with the new package, update the version here (or supersede this ADR if the approach changes), and re-run `./psake build`, `./psake Clean` and `./psake Help`.

## Consequences
- A fresh clone builds with no psake installed and no download at build time.
- Upgrades are manual, and 77 third-party files sit in the repository (psake is MIT licensed; the notice is at the top of `Tools/psake/psake.psm1`).
- psake 5 needs Windows PowerShell 5.1 or PowerShell 7; `psake.cmd` still calls `powershell.exe`.
- With `Framework '4.8'` psake 5 looks for Visual Studio / Build Tools MSBuild (16.0 or newer) through the `VSSetup` PowerShell module and puts it on the path ahead of the .NET Framework folder. Where `VSSetup` is not installed it skips that lookup and the build falls back to the MSBuild in `%windir%\Microsoft.NET\Framework64\v4.0.30319`, as before. `VSSetup` is therefore an optional machine prerequisite that changes which MSBuild is used.
