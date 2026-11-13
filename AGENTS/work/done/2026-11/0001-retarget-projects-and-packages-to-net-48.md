---
id: 0001
title: Retarget projects and packages to .NET Framework 4.8
status: done
type: chore
created: 2026-11-13
related: [0002, 0003]
---

## Goal
Every project in `src/dbdeploy.net.sln` targets .NET Framework 4.8 and its NuGet packages are restored and referenced at versions that support net48, so the solution builds on a current toolchain.

## Acceptance criteria
- [x] All six `.csproj` files have `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` (no `v4.0` left under `src/`).
- [x] `app.config` / `App.config` `supportedRuntime` entries say `sku=".NETFramework,Version=v4.8"`.
- [x] Every `packages.config` entry has `targetFramework="net48"` and its `HintPath` in the csproj points at a folder that exists after restore.
- [x] `./psake build` succeeds from a clean checkout (after package restore) with no new warnings about framework mismatches.
- [x] The test assembly `Test.Net.Sf.Dbdeploy.dll` is produced (running the tests is item 0002).

## Context
- Current state (surveyed 2026-11-13): all projects are old-style csproj, `ToolsVersion="4.0"`, `TargetFrameworkVersion` v4.0.
  - `src/Net.Sf.Dbdeploy` — NVelocity 1.0.3
  - `src/Dbdeploy.Console` — NDesk.Options 0.2.1, NVelocity 1.0.3
  - `src/Dbdeploy.NAnt` — `lib/NAnt.Core.dll` (checked-in binary)
  - `src/Dbdeploy.Powershell` — System.Management.Automation
  - `src/MSBuild.Dbdeploy.Task` — Microsoft.Build.*
  - `src/Test.Net.Sf.Dbdeploy` — Moq 4.1.1308.2321, MySql.Data 6.9.8, `lib/nunit.*.dll` and `lib/NAnt.Core.dll` (checked-in binaries)
- `default.ps1` sets `$framework = '4.0'` and calls ILMerge with `/v4`; `src/.nuget/NuGet.targets` is the legacy restore mechanism.
- `src/Test.Net.Sf.Dbdeploy/App.config` pins `MySql.Data, Version=6.9.8.0` in `DbProviderFactories`; it must follow any MySql.Data version change.
- AGENTS.md convention: dependency changes need a work item that says so. This item authorises retargeting and the package version bumps *required* for net48. It does not authorise swapping libraries (e.g. NUnit 2 → 3, NDesk.Options → Mono.Options); raise a new item if that turns out to be needed.

## Plan
- [x] Confirm a baseline: restore packages and run `./psake build` on the untouched tree; record the result in Notes.
- [x] Change `TargetFrameworkVersion` to `v4.8` in all six csproj files; update `supportedRuntime` in the config files.
- [x] Update `$framework` in `default.ps1` (and check `psake.psm1` knows the value) so the build script selects a 4.8-capable MSBuild.
- [x] For each package (NVelocity, NDesk.Options, Moq, MySql.Data): keep the current version if it loads on net48, otherwise bump to the lowest version that does; update `packages.config`, `HintPath`s and the `DbProviderFactories` entry.
- [x] Check the checked-in binaries in `lib/` (NAnt.Core, nunit.*, log4net) load under 4.8; leave them unless the build breaks.
- [x] Build with `./psake build`; fix compile errors caused by the retarget only.
- [x] Record in Notes every package version changed and why.

## Notes / log
- 2026-11-13: Item created. Nothing has been built or changed yet.
- 2026-11-13: Baseline on the untouched tree: `./psake build` failed. Five projects built with MSB3644 warnings (no v4.0 targeting pack on this machine, assemblies resolved from the GAC); `Dbdeploy.Powershell` failed with 31 errors because `System.Management.Automation, Version=1.0.0.0` could not be resolved, and `Test.Net.Sf.Dbdeploy` (which references it) was therefore not built.
- 2026-11-13: `$framework` in `default.ps1` stays `'4.0'`. In psake it only selects the CLR folder (`%windir%\Microsoft.NET\Framework64\v4.0.30319`), and the bundled `psake.psm1` accepts nothing newer. The MSBuild in that folder (4.8.x) builds v4.8 targets when the 4.8 targeting pack is installed.
- 2026-11-13: No package version was changed. NVelocity 1.0.3, NDesk.Options 0.2.1, Moq 4.1.1308.2321 and MySql.Data 6.9.8 all resolve and compile on net48. Only the `targetFramework` attributes changed, plus the MySql.Data `HintPath` from `lib\net40` to `lib\net45` (what NuGet selects for net48; same assembly version 6.9.8.0, so the `DbProviderFactories` entry is unchanged). Checked-in `lib/` binaries were left alone.
- 2026-11-13: `System.Management.Automation` now has a `HintPath` to `$(MSBuildProgramFiles32)\Reference Assemblies\Microsoft\WindowsPowerShell\3.0`. The checked-in `lib/powershell` copy is PowerShell 1.0 and does not compile (`VerbsCommon.Push` is missing), so it cannot be used. This makes the build depend on the Windows PowerShell 3.0 reference assemblies being installed on the build machine.
- 2026-11-13: Verified on a fresh clone with the changes applied: `nuget restore src/dbdeploy.net.sln` then `./psake build` gives 0 errors and 1 warning (CS0618, `System.Data.OracleClient` is obsolete, in `OracleDatabaseSchemaVersionManagerTest.cs`; it is not a framework-mismatch warning and was hidden before only because the test project did not compile).
- 2026-11-13: `supportedRuntime version` stays `v4.0` (that is the CLR version); only the `sku` moved to 4.8.

## Outcome
All six projects target .NET Framework 4.8 and `./psake build` succeeds, including `Dbdeploy.Powershell` and the test assembly, which did not build before. No package versions changed and no source code changed.

Tests were not run: there is no runner in the repo, and running and triaging them is item 0002.

Deferred / to pick up elsewhere:
- The build needs `nuget restore src/dbdeploy.net.sln` first on a fresh clone; `./psake build` does not restore by itself. Item 0003 removes `packages.config`.
- `lib/powershell/*.dll` (PowerShell 1.0) is now unused by the build; removal is left to 0003.
- The CS0618 warning for `System.Data.OracleClient` is left for 0003 (Oracle provider replacement).
