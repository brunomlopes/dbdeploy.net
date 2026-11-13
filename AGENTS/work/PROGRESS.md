## 2026-11-13 (0005)
- Worked on: 0005 (psake `Test` task)
- Done: 0005
- State: `./psake Test` builds and runs the entire suite with NUnit.Runners 2.6.4 restored as a NuGet package (ADR 0002). Result here: 92 run, 20 errors, 0 failures; psake fails with exit code 1. 0006 is committed on `0006-fix-psake-cmd-exit-code`; 0005 work is on branch `0005-add-psake-task-to-run-tests`, cut from it.
- Next: 0002 (triage the 20 errors; all are database fixture SetUp errors), then 0003
- Watch out: the test run takes about 2 minutes, mostly waiting on unreachable databases; a green run of `./psake Test` has never been observed; `nuget restore src/dbdeploy.net.sln` is needed before the first `./psake Test`

## 2026-11-13 (later)
- Worked on: 0004 (update vendored psake 4.2.0 → 5.0.4), 0006 (psake.cmd exit code)
- Done: 0004, 0006
- State: psake 5.0.4 is in `Tools/psake/`, `psake.ps1` is a repo-owned wrapper, `psakefile.ps1` (renamed from `default.ps1`) uses `Framework '4.8'`, ADR 0001 written. `./psake build`, `Clean` and `Help` pass. Branches stack: `retarget-net48` (0001) → `0004-update-psake` → `0006-fix-psake-cmd-exit-code`; none is pushed or merged to master.
- Next: 0004 is committed on branch `0004-update-psake`. Created backlog item 0005 (psake `Test` task); do 0005 before 0002 so 0002 can use `./psake Test`. Did 0006 on branch `0006-fix-psake-cmd-exit-code`: `psake.cmd` now returns 1 when psake fails (one-line fix, `exit /b %errorlevel%` instead of `goto :eof`); not committed yet
- Watch out: the build now runs on Visual Studio's MSBuild (found via the `VSSetup` module) instead of the Framework-folder one; `psakefile.ps1` has CR-only line endings, edit it with a tool that preserves them

## 2026-11-13
- Worked on: created backlog items 0001 (retarget to .NET Framework 4.8), 0002 (run tests, triage failures), 0003 (migrate to .NET 11); then did 0001; created backlog item 0004 (update psake from 4.2.0 to latest, 5.0.4)
- Done: 0001
- State: all six projects target v4.8 and `./psake build` succeeds (0 errors); changes are not committed. 0002 and 0003 are in `backlog/`; intended order is 0002 → fix items created by 0002 → 0003
- Next: start 0002
- Watch out: tests have never been run in this effort; a fresh clone needs `nuget restore src/dbdeploy.net.sln` before `./psake build`; the PowerShell project now needs the Windows PowerShell 3.0 reference assemblies installed; edit csproj/config files with tools that keep CRLF (plain `sed -i` in Git Bash strips it, use `sed -b`); no .NET 11 SDK installed (latest is 10.0.404); no NUnit runner is checked in; most tests need live SQL Server / MySQL / Oracle databases configured in `src/Test.Net.Sf.Dbdeploy/App.config`
