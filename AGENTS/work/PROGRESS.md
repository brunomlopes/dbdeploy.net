## 2026-11-13 (0015)
- Worked on: 0015 (SQL Server: second script runs after the first fails)
- Done: 0015
- State: product defect in `mssql_apply.vm` (the test was written failing in 2013 and never fixed). The template now checks the change log after each change script and, on failure, rolls back, raises an error and sets `NOEXEC ON`. `./psake test` with containers up: 92 run, 11 errors (MySQL 0014, Oracle 0012), 0 failures. Committed on branch `0015-fix-mssql-second-script`, cut from the 0007 branch.
- Next: 0014 (6 MySQL tests), then 0008, 0013, 0009, 0003; 0016 (new: failures in earlier batches of a change script, and the undo template) when convenient
- Watch out: the MySQL test of the same name (`MySqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails`) is one of the 0014 errors and may fail for the same template reason once 0014 is fixed

## 2026-11-13 (0007)
- Worked on: 0007 (Docker Compose test databases)
- Done: 0007; 0010 and 0011 closed as delivered by it
- State: `Tools/docker-compose.yml` (SQL Server 2022 on 127.0.0.1:14330, MySQL 8.0 on 127.0.0.1:13306), psake tasks `start-sqlserver`, `start-mysql`, `start-databases`, `stop-databases`, tests pointed at them through `App.config`. `./psake test` with containers up: 92 run, 80 passed, 11 errors, 1 failure, about 8 s. Work is on branch `0007-docker-compose-test-databases`, not committed.
- Next: 0014 (6 MySQL tests, SQL syntax error near '.ChangeLog'), 0015 (1 SQL Server test, second script runs after the first fails), then 0008 (PostgreSQL container), 0013 (task renames), 0009, 0003. 0012 (Oracle) stays parked.
- Watch out: use `127.0.0.1`, not `localhost`, for the container ports (localhost resolves to IPv6 here); inside psake `Exec`, run docker through `cmd /c "... 2>&1"` because its stderr progress output is treated as an error by Windows PowerShell; the test containers were left running at the end of this session

## 2026-11-13 (0002)
- Worked on: 0002 (run tests, triage failures)
- Done: 0002
- State: `./psake Test` gives 92 run, 72 passed, 20 errors. All 20 are database connection errors in SetUp. Created backlog items 0010 (SQL Server, 9 tests), 0011 (MySQL, 6 tests), 0012 (Oracle, 5 tests). Work is on branch `0002-run-tests-and-triage-failures`, cut from the 0005 branch.
- Next: 0007 (new: `Tools/docker-compose.yml` with SQL Server and MySQL containers, psake start tasks, tests pointed at them), then 0008 (new: PostgreSQL container and start task), then 0009 (new: PostgreSQL support in the tool). It is expected to deliver 0010 and 0011. Oracle (0012) is out of scope for now by maintainer decision. 0003 should wait for 0007
- Also created: 0013 (rename psake tasks to lowercase kebab-case, e.g. `build-package`); new tasks from 0007 and 0008 should already use that style
- Watch out: the triage items were renumbered from 0007-0009 to 0010-0012; PostgreSQL has no support in the code yet (0009); real test failures may be hidden behind the SetUp errors; the Oracle tests need a native Oracle client, not just a database

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
