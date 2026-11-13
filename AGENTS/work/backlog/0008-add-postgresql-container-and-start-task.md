---
id: 0008
title: Add a PostgreSQL container and psake start task
status: backlog
type: feature
created: 2026-11-13
related: [0007, 0009, 0013]
---

## Goal
A developer can start a PostgreSQL test database with one psake task, alongside the SQL Server and MySQL ones from 0007, ready for the PostgreSQL support work in 0009.

## Acceptance criteria
- [ ] `Tools/docker-compose.yml` (created by 0007) has a PostgreSQL service in its own container, with a pinned image version (no `latest`).
- [ ] Its port is published on localhost only and does not collide with the `postgres:17` container already on the maintainer's machine (`127.0.0.1:15432`) or with a default local install (5432).
- [ ] `psakefile.ps1` has a start task for the PostgreSQL instance, named and built like the start tasks from 0007 (lowercase kebab-case per 0013, e.g. `start-postgres`, and included in `start-databases` if that task exists), with a description so `./psake Help` lists it.
- [ ] The task returns only when the database accepts connections, or fails with a clear message. A clean start leaves a `DBDEPLOY` database and a test login in place with no manual step.
- [ ] The stop task from 0007 (if it exists) also stops this container.
- [ ] Starting PostgreSQL does not start or disturb the SQL Server and MySQL containers, and `./psake Test` gives the same result as before this item.
- [ ] AGENTS.md "Commands" lists the new task.

## Context
- Decided by the maintainer on 2026-11-13: the PostgreSQL container and start task are this item; full PostgreSQL support in the tool is 0009. PostgreSQL was first part of 0007 and was moved out.
- Depends on 0007, which creates `Tools/docker-compose.yml`, the start-task pattern (explicit `-f Tools/docker-compose.yml`, fixed Compose project name, wait until healthy) and the conventions for ports and test credentials. Follow those rather than inventing new ones.
- Nothing in the code or tests uses PostgreSQL yet: `DbmsFactory` accepts only `ora`, `mssql` and `mysql` and there is no PostgreSQL test fixture. No test will connect to this container until 0009. Do not add a connection string to `App.config`, a driver package or any code here.
- The official PostgreSQL image creates a database and user from environment variables on first start and ships `pg_isready`, so no separate init step should be needed, unlike SQL Server in 0007.
- Credentials are for a throwaway local test database; keep them obviously test-only.
- `psakefile.ps1` has CR-only line endings; keep them.

## Plan
- [ ] Confirm the image version and host port with the maintainer (or reuse what was agreed for 0007).
- [ ] Add the PostgreSQL service to `Tools/docker-compose.yml` with a health check.
- [ ] Add the psake start task and extend the stop task.
- [ ] Verify: clean start reaches healthy, the database and login exist (`docker compose exec` with `psql`), `./psake Help` lists the task, `./psake Test` result is unchanged.
- [ ] Update AGENTS.md "Commands".

## Notes / log
- 2026-11-13: Item created. Nothing was changed; no container was started.

## Outcome
