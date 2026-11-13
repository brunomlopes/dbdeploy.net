## 2026-11-13 (later)
- Worked on: 0004 (update vendored psake 4.2.0 → 5.0.4)
- Done: 0004
- State: psake 5.0.4 is in `Tools/psake/`, `psake.ps1` is a repo-owned wrapper, `psakefile.ps1` (renamed from `default.ps1`) uses `Framework '4.8'`, ADR 0001 written. `./psake build`, `Clean` and `Help` pass. Changes are not committed (working tree is on branch `retarget-net48`, which holds 0001).
- Next: commit 0004 on its own branch per the AGENTS.md naming rule, then start 0002
- Watch out: the build now runs on Visual Studio's MSBuild (found via the `VSSetup` module) instead of the Framework-folder one; `cmd /c .\psake.cmd` returns 0 to the caller even when the build fails (pre-existing), so check the output or call `psake.ps1` directly; `psakefile.ps1` has CR-only line endings, edit it with a tool that preserves them

## 2026-11-13
- Worked on: created backlog items 0001 (retarget to .NET Framework 4.8), 0002 (run tests, triage failures), 0003 (migrate to .NET 11); then did 0001; created backlog item 0004 (update psake from 4.2.0 to latest, 5.0.4)
- Done: 0001
- State: all six projects target v4.8 and `./psake build` succeeds (0 errors); changes are not committed. 0002 and 0003 are in `backlog/`; intended order is 0002 → fix items created by 0002 → 0003
- Next: start 0002
- Watch out: tests have never been run in this effort; a fresh clone needs `nuget restore src/dbdeploy.net.sln` before `./psake build`; the PowerShell project now needs the Windows PowerShell 3.0 reference assemblies installed; edit csproj/config files with tools that keep CRLF (plain `sed -i` in Git Bash strips it, use `sed -b`); no .NET 11 SDK installed (latest is 10.0.404); no NUnit runner is checked in; most tests need live SQL Server / MySQL / Oracle databases configured in `src/Test.Net.Sf.Dbdeploy/App.config`
