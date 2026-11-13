---
id: 0004
title: Update psake to the most recent version
status: done
type: chore
created: 2026-11-13
related: [0001, 0003]
---

## Goal
The build runs on the current stable psake release, vendored in the repo the same way the bundled 4.2.0 copy is today, so the build script can select a modern MSBuild and no longer relies on deprecated psake behaviour.

## Acceptance criteria
- [x] The psake files checked into the repo are the latest stable release at the time of the work (5.0.4 on 2026-11-13; re-check before starting).
- [x] The build needs no psake installed on the machine and no download at build time: a fresh clone runs on the vendored copy.
- [x] `./psake build` still works as the build command from a fresh clone, or AGENTS.md "Commands" is updated with the replacement.
- [x] The build output no longer shows "Using global variable $framework to set .NET framework version used is deprecated".
- [x] `./psake build` succeeds with 0 errors and produces the same six assemblies as before.
- [x] `./psake Clean` and `./psake Help` run without psake errors. (`BuildPackage` and `PublishPackage` are out of scope: they depend on ILMerge, `Write-Zip` and Bintray, see 0003.)
- [x] An ADR records that psake is vendored, which version, and where the files came from.

## Context
- Bundled today, checked in at the repo root: `psake.psm1` (`$psake.version = "4.2.0"`, line 684), `psake.ps1` (wrapper) and `psake.cmd` (launcher that calls Windows PowerShell, `powershell.exe`).
- Latest on the PowerShell Gallery on 2026-11-13: 5.0.4 stable (published 2026-04-30); 5.0.5-beta2 is a prerelease and should not be used.
- This is a jump of a major version. Read the psake release notes / migration guide for 4.x → 5.x before changing anything; do not assume `default.ps1` runs unchanged.
- `default.ps1` things that are known to be old-style:
  - `$framework = '4.0'` as a global variable at the top (deprecated even in 4.2.0; newer psake uses the `Framework` function or a config file).
  - `Exec { msbuild $solution }` relies on psake putting `%windir%\Microsoft.NET\Framework64\v4.0.30319` on the path. Newer psake resolves MSBuild differently (Visual Studio / Build Tools installs); the machine has VS 2022 and VS 18 MSBuild.
  - `$psake.build_script_file` is used to compute `$root_dir`.
- Finding from 0001: 4.2.0 accepts no framework value newer than `'4.0'`, which is why `$framework` could not be moved to 4.8 there. Setting it to 4.8 is expected to become possible with this item.
- Decided by the maintainer on 2026-11-13: psake stays vendored, as it is now. Do not switch to an installed module (`Install-Module`), a NuGet package restore or a bootstrap download.
- The 5.x release is unlikely to be a single `psake.psm1` any more. Vendor whatever file set the release ships (module manifest, `.psm1`, any `public`/`private` folders, license), and prefer a subfolder such as `Tools/psake/` over spreading many files across the repo root, keeping `psake.cmd` / `psake.ps1` at the root as the entry point.
- psake is a build dependency; per AGENTS.md this work item is the authorisation to change it.
- If 0003 replaces psake with plain `dotnet` commands, this item may become unnecessary. Check the state of 0003 before starting.

## Plan
- [x] Re-check the latest stable version (`Find-Module psake -Repository PSGallery`) and read the 4.x → 5.x release notes; record breaking changes that touch `default.ps1` in Notes.
- [x] Get the release files for the chosen version (e.g. `Save-Module psake -RequiredVersion <version>` into a temporary folder) and note the source and version in Notes.
- [x] Write the ADR recording that psake is vendored, the version, and the folder layout.
- [x] Record the baseline: output of `./psake build` and `./psake Help` on 4.2.0.
- [x] Replace the vendored 4.2.0 files with the new release's files; point `psake.ps1` at the vendored module; keep `./psake build` working as the entry point.
- [x] Update `default.ps1`: replace the `$framework` global with the supported mechanism and set it to 4.8; fix anything the new version rejects.
- [x] Check whether `psake.cmd` must call `pwsh` instead of `powershell` for the new version, and change it only if required.
- [x] Run `./psake build`, `./psake Clean`, `./psake Help`; compare with the baseline.
- [x] Update AGENTS.md "Commands" if the command or prerequisites changed.

## Notes / log
- 2026-11-13: Item created. Versions checked against the PowerShell Gallery; nothing was changed.
- 2026-11-13: Maintainer decision: keep vendoring psake in the repo as today. The vendored-versus-installed question is closed; the plan no longer stops for review on it.
- 2026-11-13: Latest stable re-checked: 5.0.4. Files taken with `Save-Module psake -Repository PSGallery -RequiredVersion 5.0.4` and copied unmodified to `Tools/psake/` (77 files; `PSGetModuleInfo.xml` left out). The release notes were not read online; the breaking changes below come from reading the 5.0.4 source and running it.
- 2026-11-13: 4.x → 5.x changes that affected this repo:
  - The gallery package ships no `psake.ps1` / `psake.cmd` wrapper, so `psake.ps1` at the root is now this repo's own script.
  - The default build file name is `psakefile.ps1`, not `default.ps1`. `.\psake build` was read as "build file named build" and failed; the wrapper now treats a first argument that is not a file as a task list for `default.ps1`.
  - `Invoke-psake` positional parameters changed (positions 8 and 9 are now `DetailedDocs` and `NoTimeReport`), so the wrapper passes arguments by name.
  - `Invoke-psake` returns a `PsakeBuildResult` object, which was dumped to the console; the wrapper filters it out.
  - The `$framework` global is ignored (no deprecation warning any more, it simply has no effect). `default.ps1` now calls `Framework '4.8'`.
  - Requires PowerShell 5.1 or later. `psake.cmd` calls `powershell.exe` (5.1 on this machine), so it was left unchanged.
  - Unchanged and still working: `Exec` (now an alias of `Execute`), `$psake.build_script_file`, `Write-Documentation` from the `Help` task, `$psake.build_success`.
- 2026-11-13: Baseline on 4.2.0: `build` 0 errors / 1 warning (CS0618 Oracle) with MSBuild 4.8.10001.0 from `Framework64\v4.0.30319`; `Clean` and `Help` succeeded; every run printed the `$framework` deprecation warning.
- 2026-11-13: On 5.0.4: `build` 0 errors / 1 warning (the same CS0618), same six assemblies; `Clean` and `Help` succeed; no deprecation warning. No psake module is installed on this machine, so the runs used the vendored copy.
- 2026-11-13: The build now uses a different MSBuild: 18.11.1 from Visual Studio 18 Community (Roslyn compiler) instead of the 4.8 one in the Framework folder. psake finds it through the `VSSetup` module, which is installed here. The fallback path without `VSSetup` was not tested.
- 2026-11-13: `cmd /c .\psake.cmd <failing task>` returns exit code 0 to a calling PowerShell, although `%errorlevel%` is 1 inside cmd. This is the same with the original 4.2.0 files, so it is not caused by this change and was left alone. Calling `psake.ps1` directly gives 1.
- 2026-11-13: At the maintainer's request the build file was renamed from `default.ps1` to `psakefile.ps1`, the psake 5 default name (`git mv`, content otherwise unchanged). The wrapper's default build file and the references in ADR 0001 and backlog items 0002 and 0003 were updated. Earlier notes in this file and in 0001 still say `default.ps1` because that was its name at the time. `build`, `Clean`, `Help`, a bare `.\psake` and `.\psake psakefile.ps1 Help` were re-run after the rename and succeed.
- 2026-11-13: AGENTS.md "Commands" not changed: the build command is still `./psake build`.

## Outcome
psake 4.2.0 (`psake.psm1` at the root) is replaced by psake 5.0.4 vendored in `Tools/psake/`. `psake.ps1` is rewritten as a thin wrapper around it, the build file is renamed from `default.ps1` to `psakefile.ps1` and uses `Framework '4.8'`, and ADR 0001 records the decision. `./psake build`, `./psake Clean` and `./psake Help` all succeed.

Tests were not run (item 0002). `BuildPackage` and `PublishPackage` were not run (out of scope, see 0003).

Not done / worth a follow-up if it matters:
- `psake.cmd` does not propagate a failing exit code through `cmd /c` (pre-existing).
- The build was only verified with `VSSetup` installed (Visual Studio MSBuild); the Framework-folder fallback is untested.
