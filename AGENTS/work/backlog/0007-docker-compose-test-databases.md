---
id: 0007
title: Run the database tests against Docker Compose instances
status: backlog
type: feature
created: 2026-11-13
related: [0002, 0005, 0008, 0009, 0010, 0011, 0012]
---

## Goal
A developer with Docker can start the test databases with psake and run the database tests against them, instead of depending on a locally installed SQL Server and a MySQL host on someone's LAN.

## Acceptance criteria
- [ ] `Tools/docker-compose.yml` defines two services, each in its own container: SQL Server and MySQL. Image versions are pinned (no `latest`).
- [ ] `psakefile.ps1` has one task per instance to start it (SQL Server, MySQL), each with a description so `./psake Help` lists it. A task returns only when its database accepts connections, or fails with a clear message.
- [ ] Starting an instance from a clean state leaves it ready for the tests with no manual step: the `DBDEPLOY` database and whatever login the tests use exist.
- [ ] The SQL Server tests (9) and MySQL tests (6) listed in 0002 connect to the Docker instances: with the instances started, `./psake Test` shows no SetUp connection error for those 15 tests.
- [ ] The connection settings the tests use match the Compose file, come from one place, and no longer point at `Server=.` or `192.168.1.112`.
- [ ] Container ports are published on localhost only and do not collide with the SQL Server instances already present on the maintainer's machine.
- [ ] Any of the 15 tests that then fails for a code reason is listed in Notes and has its own new backlog item; it is not fixed here.
- [ ] AGENTS.md "Commands" lists the start tasks and the prerequisite (Docker).
- [ ] The five Oracle tests are untouched and still fail in SetUp; that is expected (out of scope).
- [ ] The Compose file and task names leave room for the PostgreSQL service that 0008 adds (no restructuring needed to add a third service).

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
- [ ] Confirm with the maintainer: image versions, host ports, and task names (suggest `StartSqlServer`, `StartMySql`, plus a `StopDatabases` task).
- [ ] Write `Tools/docker-compose.yml` with the two services, pinned images, localhost-only ports, health checks and named volumes or no persistence (decide which).
- [ ] Add the SQL Server initialisation (database + login) and the MySQL database creation so a clean start is test-ready.
- [ ] Add the psake start tasks, each waiting for its instance to be healthy.
- [ ] Point the tests at the containers: update `App.config`, and make `SqlCmdApplierTest` use the same connection lookup as the other SQL Server fixture if needed.
- [ ] Start the instances and run `./psake Test`; record the result of each of the 15 tests in Notes.
- [ ] Create backlog items for code failures that surface; update 0010 and 0011.
- [ ] Update AGENTS.md "Commands" and add a short "running the database tests" note to Readme.md.

## Notes / log
- 2026-11-13: Item created. Nothing was changed; no container was started.
- 2026-11-13: Maintainer changes: PostgreSQL moved out to 0008 (container and start task) and 0009 (support in the tool); the Compose file goes in `Tools/docker-compose.yml`.

## Outcome
