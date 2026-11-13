---
id: 0011
title: Make the MySQL tests runnable (6 tests)
status: done
type: chore
created: 2026-11-13
related: [0002, 0005, 0007, 0010, 0012]
---

## Goal
The six MySQL tests get past their SetUp on a developer machine, so they report real results instead of "unable to connect".

## Acceptance criteria
- [x] `./psake Test` shows no SetUp connection error for the six tests listed below.
- [x] The steps to get a usable MySQL test database are written down in the repo and can be followed from a fresh clone.
- [x] The committed MySQL connection string no longer points at a private LAN address, or the override mechanism is documented.
- [x] Any of the six tests that then fails for a code reason has its own new backlog item; it is not fixed here.

## Context
- Tests (all fail in SetUp with `MySql.Data.MySqlClient.MySqlException: Unable to connect to any of the specified MySQL hosts.`), all in `Net.Sf.Dbdeploy.Database.MySqlDatabaseSchemaVersionManagerTest`:
  - `ShouldNotThrowExceptionIfAllPreviousScriptsAreCompleted`
  - `TestCanRetrieveSchemaVersionFromDatabase`
  - `TestDoesNotRunSecondScriptIfFirstScriptFails`
  - `TestReturnsNoAppliedChangesWhenDatabaseTableDoesNotExist`
  - `TestShouldCreateChangeLogTableWhenToldToDoSo`
  - `TestShouldReturnEmptySetWhenTableHasNoRows`
- Cause: environment. `App.config` has `MySqlConnString` pointing at `Server=192.168.1.112`, database `DBDEPLOY`, user `root`. That host does not answer on port 3306 from the maintainer's machine (2026-11-13), and no MySQL service is installed locally. Docker is installed.
- The fixture uses the app setting `MySqlConnString-<MACHINENAME>` if present, else `MySqlConnString`.
- Client library: MySql.Data 6.9.8 (2015). It may not be able to authenticate against a current MySQL server with default settings (newer default authentication plugin). This is a risk, not something that was tested. If it happens, either run an older server version or legacy authentication for the tests, or create a separate item to upgrade MySql.Data; do not upgrade it here.
- The last commit on master before this effort is "Trying to get mysql compatibility", so MySQL support was work in progress. Expect real failures once the server is reachable; record them as new items.
- The failed connection attempts account for a large part of the roughly 2 minute test run.

## Plan
- [x] Ask the maintainer whether a MySQL container is acceptable as the test database, and which server version.
- [x] Start it, create `DBDEPLOY`, and point the tests at it (change `MySqlConnString` to localhost, or add a per-machine key).
- [x] Document the setup steps next to the SQL Server ones (0010).
- [x] Run `./psake Test`; record the six results in Notes; create items for any code failures.

## Notes / log
- 2026-11-13: Created by 0002 from the first full run (92 run, 20 errors).
- 2026-11-13: Renumbered from 0008 to make room for 0007. Maintainer decision the same day: the test databases will run in Docker Compose (item 0007), which is expected to deliver this item. Do 0007 first; then close this item, or keep only what 0007 left undone.
- 2026-11-13: Closed without separate work: 0007 delivered it. The plan steps above were carried out there (the maintainer chose containers; setup is documented in AGENTS.md and Readme.md).

## Outcome
Delivered by 0007 (Docker Compose test databases). None of the six tests fails in SetUp any more; all six now fail in the test body with a SQL syntax error and are item 0014.

Follow-up item: 0014.
