---
id: 0007
title: Run the database tests against Docker Compose instances
status: done
type: feature
created: 2026-11-13
related: [0002, 0005, 0008, 0009, 0010, 0011, 0012, 0013, 0014, 0015]
---

## Goal
A developer with Docker can start the test databases with psake and run the database tests against them, instead of depending on a locally installed SQL Server and a MySQL host on someone's LAN.

## Acceptance criteria
- [x] `Tools/docker-compose.yml` defines two services, each in its own container: SQL Server and MySQL. Image versions are pinned (no `latest`).
- [x] `psakefile.ps1` has one task per instance to start it (SQL Server, MySQL), each with a description so `./psake Help` lists it. A task returns only when its database accepts connections, or fails with a clear message.
- [x] Starting an instance from a clean state leaves it ready for the tests with no manual step: the `DBDEPLOY` database and whatever login the tests use exist.
- [x] The SQL Server tests (9) and MySQL tests (6) listed in 0002 connect to the Docker instances: with the instances started, `./psake Test` shows no SetUp connection error for those 15 tests.
- [x] The connection settings the tests use match the Compose file, come from one place, and no longer point at `Server=.` or `192.168.1.112`.
- [x] Container ports are published on localhost only and do not collide with the SQL Server instances already present on the maintainer's machine.
- [x] Any of the 15 tests that then fails for a code reason is listed in Notes and has its own new backlog item; it is not fixed here.
- [x] AGENTS.md "Commands" lists the start tasks and the prerequisite (Docker).
- [x] The five Oracle tests are untouched and still fail in SetUp; that is expected (out of scope).
- [x] The Compose file and task names leave room for the PostgreSQL service that 0008 adds (no restructuring needed to add a third service).

## Context
- Decided by the maintainer on 2026-11-13: test databases run in Docker Compose, each engine in a separate container; psake targets start each instance; the tests change to run against them. The Compose file lives in the `Tools` folder: `Tools/docker-compose.yml`. Oracle is out of scope for now because it needs native Oracle clients (see 0012). PostgreSQL was first part of this item and was moved out the same day: 0008 adds its container and start task, 0009 adds PostgreSQL support to the tool.
- Triage that led here: 0002. `./psake Test` gives 92 run, 72 passed, 20 errors, all SetUp connection errors: SQL Server 9, MySQL 6, Oracle 5.
- Test command and runner: `./psake Test`, NUnit.Runners 2.6.4 (0005, ADR 0002). `psake.cmd` returns 1 on failure (0006). `psakefile.ps1` has CR-only line endings; keep them.
- Current connection settings, `src/Test.Net.Sf.Dbdeploy/App.config`:
  - `ConnString`: `Server=.;Initial Catalog=DBDEPLOY`, SQL login `DBDeployUser`.
  - `MySqlConnString`: `Server=192.168.1.112;Initial Catalog=DBDEPLOY`, user `root`.
  - `OracleConnString`: leave as is.
  - The MsSql and MySql fixtures first look for a key suffixed with the machine name (`ConnString-<MACHINENAME>`, `MySqlConnString-<MACHINENAME>`); `SqlCmdApplierTest` reads only `ConnString`.
- SQL Server specifics to handle:
  - The container needs SQL authentication with a password that meets SQL Server's complexity rules; the existing `DBDeployUser` password may not qualify for `sa`, but the tests can keep using `DBDeployUser` if an init step creates it.
  - The official image does not run init scripts by itself; creating `DBDEPLOY` and the login needs a step after start-up (for example `sqlcmd` from the psake task or a one-shot init container). `db/create.database.sql.template` and the `db` target in `dbdeploy.build` show what was created before.
  - `SqlCmdApplierTest` shells out to `sqlcmd` (a copy is in `src/Net.Sf.Dbdeploy/Resources`); check how it passes the server name when the port is not 1433.
- MySQL specifics to handle:
  - The tests use MySql.Data 6.9.8 (2015). Whether it can authenticate against a current MySQL image is untested. Pick a server version and authentication setting that this client works with, and record it. Upgrading MySql.Data is not authorised by this item.
- Machine facts (2026-11-13): Docker Desktop is installed and running; SQL Server Express (stopped) and LocalDB are installed; `sqlcmd` is on the path.
- The Compose file is not at the repo root, so the psake tasks must pass its path explicitly (`docker compose -f Tools/docker-compose.yml ...`) and should set a fixed project name so container names do not depend on the folder name.
- Credentials in the Compose file are for throwaway local test databases. Keep them obviously test-only and out of anything reused elsewhere.
- Changing `App.config` and test fixtures is test-code change authorised by this item. Production code under `src/Net.Sf.Dbdeploy` is not to be changed here.
- Effect on other items: this item is expected to deliver what 0010 (SQL Server) and 0011 (MySQL) describe. When it is done, close or shrink those two accordingly.

## Plan
- [x] Confirm with the maintainer: image versions, host ports, and task names (suggest `start-sqlserver`, `start-mysql`, `start-databases` for both, plus `stop-databases`; lowercase kebab-case per 0013).
- [x] Write `Tools/docker-compose.yml` with the two services, pinned images, localhost-only ports, health checks and named volumes or no persistence (decide which).
- [x] Add the SQL Server initialisation (database + login) and the MySQL database creation so a clean start is test-ready.
- [x] Add the psake start tasks, each waiting for its instance to be healthy.
- [x] Point the tests at the containers: update `App.config`, and make `SqlCmdApplierTest` use the same connection lookup as the other SQL Server fixture if needed.
- [x] Start the instances and run `./psake Test`; record the result of each of the 15 tests in Notes.
- [x] Create backlog items for code failures that surface; update 0010 and 0011.
- [x] Update AGENTS.md "Commands" and add a short "running the database tests" note to Readme.md.

## Notes / log
- 2026-11-13: Item created. Nothing was changed; no container was started.
- 2026-11-13: Maintainer changes: PostgreSQL moved out to 0008 (container and start task) and 0009 (support in the tool); the Compose file goes in `Tools/docker-compose.yml`.
- 2026-11-13: Maintainer answers before starting: set `ACCEPT_EULA=Y` for SQL Server in the Compose file; images SQL Server 2022 and MySQL 8.0 (`mcr.microsoft.com/mssql/server:2022-CU28-ubuntu-22.04` on `127.0.0.1:14330`, `mysql:8.0.46` on `127.0.0.1:13306`). Task names follow 0013: `start-sqlserver`, `start-mysql`, `start-databases`, `stop-databases`.
- 2026-11-13: `Tools/docker-compose.yml` written with Compose project name `dbdeploy-tests`, health checks, no volumes (nothing persists; `stop-databases` runs `down --volumes`). SQL Server initialisation is a one-shot service `sqlserver-init` that runs `Tools/docker-init/sqlserver-init.sql` with the sqlcmd inside the SQL Server image (idempotent: creates `DBDEPLOY`, the `DBDeployUser` login and makes it `db_owner`). MySQL creates `DBDEPLOY` from `MYSQL_DATABASE`.
- 2026-11-13: psake tasks added. Docker writes progress to stderr, and Windows PowerShell 5.1 turned that into a terminating error inside `Exec`, so the tasks run docker through `cmd /c "... 2>&1"`. Hyphenated task names parse and run without quoting.
- 2026-11-13: Connection problems found and fixed in `App.config` (no test code changed):
  - `Server=localhost,14330` was refused: `localhost` resolves to IPv6 on this machine and the port is published on 127.0.0.1 only. Both connection strings use `127.0.0.1`.
  - MySql.Data 6.9.8 failed with `AuthenticationException: A call to SSPI failed` (TLS negotiation with MySQL 8). `SslMode=None` added to the MySQL connection string. The server also runs with `--default-authentication-plugin=mysql_native_password` and `utf8mb4_general_ci`; whether the client would work without those two settings was not tested.
- 2026-11-13: `SqlCmdApplierTest` did not need changing: it reads `ConnString`, which now points at the container, and sqlcmd accepts `127.0.0.1,14330`.
- 2026-11-13: Verified: `stop-databases` leaves no containers; `start-databases` from that clean state returns 0 in about 33 s with both containers healthy; a second `start-databases` while running returns 0; `./psake help` lists the four tasks.
- 2026-11-13: `./psake test` with both containers running: 92 run, 11 errors, 1 failure, about 8 s (was 20 errors, about 107 s). Results for the 15 tests in scope:
  - SQL Server, 8 of 9 pass: both `SqlCmdApplierTest` tests and six `MsSqlDatabaseSchemaVersionManagerTest` tests. `MsSqlDatabaseSchemaVersionManagerTest.TestDoesNotRunSecondScriptIfFirstScriptFails` fails with `TableWeWillUse table was created.` → item 0015.
  - MySQL, 0 of 6 pass, but none fails in SetUp any more: all six fail in the test body with `MySqlException: You have an error in your SQL syntax ... near '.ChangeLog'` (one says `'.TableWeWillUse'`) → item 0014.
  - Oracle, 5 errors, unchanged and expected (0012).
- 2026-11-13: The "one place" for connection settings is `App.config`; the same ports and credentials also appear in the Compose file and the init script, so a port or password change has to be made in both.
- 2026-11-13: The existing `ConnString-2ARMITAGE` entry in `App.config` (another machine's override) was left as it was.

## Outcome
`Tools/docker-compose.yml` defines SQL Server 2022 and MySQL 8.0 test containers, `psakefile.ps1` has `start-sqlserver`, `start-mysql`, `start-databases` and `stop-databases`, and `App.config` points the tests at the containers. No SQL Server or MySQL test fails in SetUp any more.

Test result with the containers running: 92 run, 80 passed, 11 errors, 1 failure. The 12 are: 6 MySQL tests (item 0014), 1 SQL Server test (item 0015) and the 5 Oracle tests (0012, out of scope).

Items 0010 and 0011 were closed as delivered by this item. AGENTS.md and Readme.md document the commands.

Not done: the PostgreSQL container (0008); the remaining task renames (0013).
