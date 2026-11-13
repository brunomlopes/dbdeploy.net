---
id: 0002
title: Run the test suite, list failing tests and create fix work items
status: done
type: chore
created: 2026-11-13
related: [0001, 0003, 0005, 0010, 0011, 0012]
---

## Goal
Know exactly which tests fail on the .NET Framework 4.8 build and why, with one backlog work item per group of failures that share a solution. This item triages only; it fixes nothing.

## Acceptance criteria
- [x] The full test run of `Test.Net.Sf.Dbdeploy` has been executed and the exact command used is recorded in Notes.
- [x] Notes contain the totals (passed / failed / errored / ignored) and a list of every failing test by fully qualified name with its failure message.
- [x] Every failing test is assigned to exactly one group, where a group is a set of tests that one fix would resolve.
- [x] One backlog work item exists per group, listing its tests, the suspected cause and the proposed solution; their ids are listed in Outcome and in `related`.
- [x] No production or test code was changed under this item.

## Context
- Depends on 0001 (run against the net48 build). If 0001 is not done, run against whatever builds and say so in Notes.
- Test project: `src/Test.Net.Sf.Dbdeploy` (NUnit 2.x, referenced from checked-in `lib/nunit.*.dll`; no runner is checked in, so one has to be obtained, e.g. the NUnit 2 console runner or an adapter).
- Run the suite with `./psake Test` (item 0005, done). It uses the NUnit.Runners 2.6.4 console runner (ADR 0002) and writes `build\TestResult.xml`.
- First result, 2026-11-13, no databases prepared: 92 run, 20 errors, 0 failures. All 20 are SetUp errors: `SqlCmdApplierTest` (2), `MsSqlDatabaseSchemaVersionManagerTest` (7), `MySqlDatabaseSchemaVersionManagerTest` (6), `OracleDatabaseSchemaVersionManagerTest` (5). The Oracle message is "System.Data.OracleClient requires Oracle client software version 8.1.7 or greater"; the others still need to be read from the result file.
- The old NAnt `test` target excluded the NUnit category `DbIntegration`; `test-all` ran everything.
- Many tests need live databases, configured in `src/Test.Net.Sf.Dbdeploy/App.config`:
  - SQL Server: `Server=.;Initial Catalog=DBDEPLOY` with SQL login `DBDeployUser`
  - MySQL: `192.168.1.112`, database `DBDEPLOY`
  - Oracle: data source `ORACLE`
  - `db/` and `scripts/` hold the schema scripts for the change-log table.
- Likely groups (to be confirmed by the run, not assumed): database not reachable per engine (environment, not code), MySQL compatibility (last commit is "Trying to get mysql compatibility"), `SqlCmdApplierTest` needing `sqlcmd` on the path, path/culture/line-ending assumptions.

## Plan
- [x] Build the solution (`./psake build`) and locate the test assembly.
- [x] Pick a way to run the NUnit 2 tests and record it; if this needs a new tool or package in the repo, note it rather than committing it.
- [x] Run the whole suite once with no databases configured; save the result file outside the repo and copy the summary into Notes.
- [x] Separate environment failures (no SQL Server / MySQL / Oracle available) from code failures. Ask the human which engines are available locally before treating a database failure as a code bug.
- [x] Group the failures by shared solution.
- [x] Create one backlog work item per group using the template; number them with the next unused ids.
- [x] Add a test-run command to AGENTS.md "Commands" if a repeatable one now exists.

## Notes / log
- 2026-11-13: Item created. Tests have not been run yet.
- 2026-11-13: Plan steps 1, 2 and 7 were carried out under 0005 instead: the runner (NUnit.Runners 2.6.4 package) and the `./psake Test` command were added and committed there, and AGENTS.md lists the command.
- 2026-11-13: Command: `./psake Test` on branch `0005-add-psake-task-to-run-tests` (net48 build, commit 095d9a0), no databases prepared. Result file: `build\TestResult.xml` (git-ignored; not kept elsewhere).
- 2026-11-13: Totals: 92 run, 72 passed, 0 failed, 20 errors, 0 ignored, 0 not run. Duration about 107 s.
- 2026-11-13: Every one of the 20 errors is a SetUp error raised while opening a database connection; no test body ran for them. They fall into exactly three messages, one per database engine:
  - **Group A, SQL Server, 9 tests** → item 0010. `System.Data.SqlClient.SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. ... (provider: Named Pipes Provider, error: 40 - Could not open a connection to SQL Server)`
    - `Net.Sf.Dbdeploy.Appliers.SqlCmdApplierTest.ShouldApplySqlCmdModeScripts`
    - `Net.Sf.Dbdeploy.Appliers.SqlCmdApplierTest.ShouldThrowExceptionOnScriptFailure`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestCanRetrieveSchemaVersionFromDatabase`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestShouldCreateChangeLogTableWhenToldToDoSo`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestShouldHandleCreatingChangeLogTableWithSchema`
    - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestShouldReturnEmptySetWhenTableHasNoRows`
  - **Group B, MySQL, 6 tests** → item 0011. `MySql.Data.MySqlClient.MySqlException: Unable to connect to any of the specified MySQL hosts.`
    - `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest.ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
    - `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest.TestCanRetrieveSchemaVersionFromDatabase`
    - `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails`
    - `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest.TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
    - `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest.TestShouldCreateChangeLogTableWhenToldToDoSo`
    - `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest.TestShouldReturnEmptySetWhenTableHasNoRows`
  - **Group C, Oracle, 5 tests** → item 0012. `System.Exception: System.Data.OracleClient requires Oracle client software version 8.1.7 or greater.`
    - `Net.Sf.Dbdeploy.Database.OracleDatabaseSchemaVersionManagerTest.ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
    - `Net.Sf.Dbdeploy.Database.OracleDatabaseSchemaVersionManagerTest.TestCanRetrieveSchemaVersionFromDatabase`
    - `Net.Sf.Dbdeploy.Database.OracleDatabaseSchemaVersionManagerTest.TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
    - `Net.Sf.Dbdeploy.Database.OracleDatabaseSchemaVersionManagerTest.TestShouldCreateChangeLogTableWhenToldToDoSo`
    - `Net.Sf.Dbdeploy.Database.OracleDatabaseSchemaVersionManagerTest.TestShouldReturnEmptySetWhenTableHasNoRows`
- 2026-11-13: Environment check on the maintainer's machine instead of asking first (read-only, nothing was started or changed): SQL Server services present are the named instance `SQLEXPRESS` (stopped, manual) and LocalDB, no default instance, so `Server=.` cannot connect; no MySQL service, and 192.168.1.112:3306 does not answer; no Oracle client installed. Docker and `sqlcmd` are available. All three groups are therefore environment failures, not code failures. Which engines to provide is asked of the maintainer in each new item.
- 2026-11-13: The guesses in Context did not show up: `SqlCmdApplierTest` fails on the connection, not on a missing `sqlcmd`; no path, culture or line-ending failure appeared among the 72 passing tests.
- 2026-11-13: Limit of this triage: the 20 tests never reached their bodies, so real defects behind them (in particular MySQL compatibility) are still unknown. Each new item says to record those as further items once SetUp passes.
- 2026-11-13: The `DbIntegration` category covers the MsSql, MySql and Oracle fixtures (18 tests). `SqlCmdApplierTest` (2 tests) needs SQL Server but has no category.

## Outcome
The suite was run with `./psake Test`: 92 run, 72 passed, 20 errors. All 20 are database connection errors in SetUp, in three groups with one backlog item each:

- 0010: SQL Server tests (9)
- 0011: MySQL tests (6)
- 0012: Oracle tests (5)

No production or test code was changed under this item. The test command and runner were delivered by 0005.

The three items were first numbered 0007 to 0009 and were renumbered to 0010 to 0012 the same day, when 0007 became the Docker Compose item.

Deferred: failures hidden behind the SetUp errors cannot be triaged until 0010 to 0012 make the databases reachable.
