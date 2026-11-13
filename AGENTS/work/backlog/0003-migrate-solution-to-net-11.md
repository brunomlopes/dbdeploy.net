---
id: 0003
title: Migrate the solution to .NET 11
status: backlog
type: migration
created: 2026-11-13
related: [0001, 0002]
---

## Goal
The solution builds, tests and packages on .NET 11 using SDK-style projects, with the same change-script deployment behaviour for SQL Server, Oracle and MySQL.

## Acceptance criteria
- [ ] Every project that is kept is an SDK-style csproj targeting `net11.0` (or multi-targeting where an ADR says so) and `dotnet build src/dbdeploy.net.sln` succeeds.
- [ ] `dotnet test` runs the suite and no test that passed on net48 (baseline from 0002 and its fix items) fails.
- [ ] No `packages.config`, `src/.nuget/` or checked-in dependency binaries in `lib/` are needed to build.
- [ ] The fate of each host (console, NAnt, PowerShell, MSBuild task) is recorded in an ADR.
- [ ] The packaging step in `psakefile.ps1` produces a usable package without ILMerge, or its replacement is recorded in an ADR.
- [ ] AGENTS.md "Commands" is updated with the new build and test commands.

## Context
- Start only after 0001 is done and the failures found by 0002 are fixed or consciously accepted, so there is a green baseline to compare against.
- Installed SDKs on the dev machine on 2026-11-13: 3.1, 6.0, 8.0, 9.0, 10.0. No .NET 11 SDK is installed; installing it is the first step.
- This parent item holds the strategy and inventory. Each inventory row is expected to become its own work item, one batch per session.
- Decisions that need an ADR before code is written:
  - **NAnt task**: `NAnt.Core` is .NET Framework only. Drop the project, or keep it on net48.
  - **PowerShell cmdlets**: move from Windows PowerShell's `System.Management.Automation` to the PowerShell 7 reference package, or keep on net48.
  - **MSBuild task**: reference `Microsoft.Build.Utilities.Core`; decide whether it must still load in Framework MSBuild (which would mean multi-targeting).
  - **Core library target**: `net11.0` only, or also `netstandard2.0` / `net48` for the hosts that stay on Framework.
  - **ILMerge**: does not support modern .NET. Replace with plain NuGet/zip output or single-file publish.
  - **Test framework**: NUnit 2 (checked-in dlls) has no modern runner; move to NUnit 3/4 packages with `Microsoft.NET.Test.Sdk`.
- Known code-level work:
  - `System.Configuration.ConfigurationManager` and `DbProviderFactories` config in `app.config` (used in `Net.Sf.Dbdeploy/Configuration`, `Dbdeploy.Console/OptionsManager.cs` and the tests) need the compatibility package or a replacement, with provider factories registered in code.
  - Dependencies need versions with modern .NET support: NVelocity 1.0.3, NDesk.Options 0.2.1, Moq 4.1, MySql.Data 6.9.8, the SQL Server client (`System.Data.SqlClient` → `Microsoft.Data.SqlClient`) and an Oracle provider.
  - `psakefile.ps1` calls `msbuild` directly and publishes to Bintray, which no longer exists.
- These are public API and dependency changes; per AGENTS.md each child item must say so explicitly.

## Plan
Strategy: convert in place, bottom-up, keeping the solution building after every step.

- [ ] Install the .NET 11 SDK and confirm `dotnet --list-sdks` shows it.
- [ ] Write the ADRs listed in Context and stop for human review.
- [ ] Create child work items from the inventory below and link them in `related`.
- [ ] Convert projects to SDK-style with `PackageReference` while still targeting net48 (no behaviour change).
- [ ] Migrate the tests to a supported NUnit version on net48 and confirm the baseline still holds.
- [ ] Retarget the core library and replace unsupported dependencies.
- [ ] Retarget or retire each host per its ADR.
- [ ] Replace the packaging and publish tasks; update AGENTS.md.

### Inventory
- [ ] `src/Net.Sf.Dbdeploy` (core library; NVelocity, System.Configuration, DB providers)
- [ ] `src/Test.Net.Sf.Dbdeploy` (NUnit 2, Moq, MySql.Data, App.config connection strings)
- [ ] `src/Dbdeploy.Console` (NDesk.Options, app.config)
- [ ] `src/MSBuild.Dbdeploy.Task`
- [ ] `src/Dbdeploy.Powershell`
- [ ] `src/Dbdeploy.NAnt`
- [ ] `psakefile.ps1` / `psake.*` (Build, Clean, BuildPackage, PublishPackage), `Tools/ILMerge.exe`, `internalize_exclusions.txt`
- [ ] Legacy build files: `dbdeploy.build`, `go.bat`, `Tools/nant`, `src/.nuget`, `lib/`
- [ ] `Readme.md` and `example_scripts/` usage instructions

## Notes / log
- 2026-11-13: Item created from a read-only survey of the repository. Nothing has been built or changed.

## Outcome
