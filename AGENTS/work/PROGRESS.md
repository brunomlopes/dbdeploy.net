## 2026-11-13
- Worked on: created backlog items 0001 (retarget to .NET Framework 4.8), 0002 (run tests, triage failures), 0003 (migrate to .NET 11); then did 0001
- Done: 0001
- State: all six projects target v4.8 and `./psake build` succeeds (0 errors); changes are not committed. 0002 and 0003 are in `backlog/`; intended order is 0002 → fix items created by 0002 → 0003
- Next: start 0002
- Watch out: tests have never been run in this effort; a fresh clone needs `nuget restore src/dbdeploy.net.sln` before `./psake build`; the PowerShell project now needs the Windows PowerShell 3.0 reference assemblies installed; edit csproj/config files with tools that keep CRLF (plain `sed -i` in Git Bash strips it, use `sed -b`); no .NET 11 SDK installed (latest is 10.0.404); no NUnit runner is checked in; most tests need live SQL Server / MySQL / Oracle databases configured in `src/Test.Net.Sf.Dbdeploy/App.config`
