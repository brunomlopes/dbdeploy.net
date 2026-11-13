---
id: 0010
title: Make the SQL Server tests runnable (9 tests)
status: backlog
type: chore
created: 2026-11-13
related: [0002, 0005, 0007, 0011, 0012]
---

## Goal
The nine tests that need SQL Server get past their SetUp on a developer machine, so they report real results instead of "server was not found".

## Acceptance criteria
- [ ] `./psake Test` shows no SetUp connection error for the nine tests listed below.
- [ ] The steps to get a usable SQL Server test database (instance, `DBDEPLOY` database, login) are written down in the repo and can be followed from a fresh clone.
- [ ] No machine-specific connection string or password is committed beyond what `App.config` already contains, unless the maintainer agrees.
- [ ] Any of the nine tests that then fails for a code reason has its own new backlog item; it is not fixed here.

## Context
- Tests (all fail in SetUp with `System.Data.SqlClient.SqlException: A network-related or instance-specific error occurred while establishing a connection to SQL Server ... (provider: Named Pipes Provider, error: 40)`):
  - `Net.Sf.Dbdeploy.Appliers.SqlCmdApplierTest.ShouldApplySqlCmdModeScripts`
  - `Net.Sf.Dbdeploy.Appliers.SqlCmdApplierTest.ShouldThrowExceptionOnScriptFailure`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestCanRetrieveSchemaVersionFromDatabase`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestShouldCreateChangeLogTableWhenToldToDoSo`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestShouldHandleCreatingChangeLogTableWithSchema`
  - `Net.Sf.Dbdeploy.Database.MsSqlDatabaseSchemaVersionManagerTest.TestShouldReturnEmptySetWhenTableHasNoRows`
- Cause: environment. `src/Test.Net.Sf.Dbdeploy/App.config` has `ConnString` = `Server=.;Initial Catalog=DBDEPLOY;User Id=DBDeployUser;...`, i.e. a default instance on the local machine with a SQL login. On the maintainer's machine (2026-11-13) there is no default instance: the installed services are the named instance `SQLEXPRESS` (stopped, manual start) and LocalDB (`MSSQLLocalDB`). `sqlcmd` is on the path.
- How the tests pick a connection string: `MsSqlDatabaseSchemaVersionManagerTest` uses the app setting `ConnString-<MACHINENAME>` if present, else `ConnString`. `SqlCmdApplierTest` reads only `ConnString` and ignores the machine-specific key, so a per-machine override does not reach it today.
- `db/create.database.sql.template` creates the database; the old NAnt script (`dbdeploy.build`, target `db`) shows how it was applied, with tokens `@DB_SERVER@`, `@DB_NAME@`, `@DB_USER_NAME@`, `@DB_USER_PWD@`.
- Answered on 2026-11-13: a SQL Server container, see 0007. Original question: which SQL Server to standardise on for tests. Options: start the existing `SQLEXPRESS` instance, use LocalDB, or run a SQL Server container (Docker is installed). The first two need a per-machine connection string; a container on port 1433 matches `Server=.` only if TCP is used and the SQL login exists.
- Once SetUp passes, these tests may fail for other reasons that are hidden today. That is expected; record them as new items.

## Plan
- [ ] Ask the maintainer which SQL Server option to use.
- [ ] Bring the instance up and create `DBDEPLOY` and the `DBDeployUser` login from `db/create.database.sql.template`.
- [ ] Decide how the connection string reaches both fixtures: a per-machine key, or change `SqlCmdApplierTest` to use the same lookup as the other fixture (a test-code change authorised by this item).
- [ ] Document the setup steps (AGENTS.md or Readme.md).
- [ ] Run `./psake Test`; record the nine results in Notes; create items for any code failures.

## Notes / log
- 2026-11-13: Created by 0002 from the first full run (92 run, 20 errors).
- 2026-11-13: Renumbered from 0007 to make room for 0007. Maintainer decision the same day: the test databases will run in Docker Compose (item 0007), which is expected to deliver this item. Do 0007 first; then close this item, or keep only what 0007 left undone.

## Outcome
