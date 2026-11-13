---
id: 0009
title: Add PostgreSQL support
status: backlog
type: feature
created: 2026-11-13
related: [0003, 0005, 0007, 0008, 0012]
---

## Goal
dbdeploy can deploy change scripts to a PostgreSQL database, with the same change-log tracking as the other engines, and this is covered by tests that run against the PostgreSQL container from 0008.

## Acceptance criteria
- [ ] A new dbms value for PostgreSQL is accepted wherever `mssql`, `mysql` and `ora` are (console, NAnt task, MSBuild task, PowerShell cmdlets, config), and the "Supported dbms" error messages and help text list it.
- [ ] Applying change scripts to PostgreSQL works in both modes the tool offers: direct execution, and generating apply and undo scripts.
- [ ] The change-log table can be created automatically on PostgreSQL, and a standalone script exists in `scripts/` next to the other engines'.
- [ ] A test fixture equivalent to the MySQL one runs against the PostgreSQL container from 0008 and passes with `./psake Test`.
- [ ] No test that passed before this item fails after it.
- [ ] An ADR records the PostgreSQL driver package and version.
- [ ] Readme.md and AGENTS.md mention PostgreSQL where they list supported databases and commands.

## Context
- Decided by the maintainer on 2026-11-13: full PostgreSQL support in the tool is this work item. The PostgreSQL container and its psake start task are 0008, which builds on 0007. Do 0007 and 0008 first; the fixture here connects to the 0008 container.
- This is a public behaviour change (a new dbms value) and a new dependency (a PostgreSQL ADO.NET driver). This work item is the authorisation for both, per AGENTS.md.
- Where the existing engines are wired in `src/Net.Sf.Dbdeploy` (to be mirrored):
  - `Database/DbmsFactory.cs`: `switch` on `ora` / `mssql` / `mysql` creating the syntax class; the connection type comes from `DbProviders`.
  - `Database/DbProviders.cs`, `Database/DbProviderFile.cs` and the embedded `src/dbproviders.xml`: provider assembly and connection class per dbms; error text lists the supported values.
  - `Database/MsSqlDbmsSyntax.cs`, `MySqlDbmsSyntax.cs`, `OracleDbmsSyntax.cs`: one syntax class per engine.
  - `Resources/<dbms>_apply.vm` and `<dbms>_undo.vm`: script templates, embedded resources in the csproj.
  - `Resources/CreateSchemaVersionTable.<dbms>.sql` and `scripts/createSchemaVersionTable.<dbms>.sql`: change-log table scripts.
  - `Database/DatabaseSchemaVersionManager.cs` line 73 has a comment mentioning "PGSQL" when reading a column type, so someone considered it before.
  - `Configuration/DbDeployConfig.cs` and `Database/DbmsSyntax.cs` document the dbms values.
- An `Npgsql.2.2.1` folder exists in the local `src/packages/` on the maintainer's machine, but no `packages.config` references it and nothing in the source uses it. Treat it as leftover, not as a chosen version.
- Driver choice: pick a version that supports .NET Framework 4.8 today and, if possible, .NET 11 later (0003). The provider is loaded by assembly name and connection class from `dbproviders.xml`, so check how the driver assembly reaches the output folder and the ILMerge'd packages.
- Tests: `src/Test.Net.Sf.Dbdeploy/Database/MySqlDatabaseSchemaVersionManagerTest.cs` (base class `AbstractDatabaseSchemaVersionManagerTest`) is the closest model. Test connection strings are in `App.config`.
- Suggested dbms value: `pgsql` (matches the existing comment); confirm with the maintainer.
- Size: this is larger than one session if done whole. Split it when starting, for example: (a) direct execution plus change-log table plus fixture, (b) script templates and undo, (c) hosts, help text and docs.

## Plan
- [ ] Confirm the dbms value with the maintainer; split into child items if the work does not fit one session.
- [ ] Choose the driver package, add it, register it in `dbproviders.xml`, and write the ADR.
- [ ] Add the syntax class, the `DbmsFactory` case and the change-log table scripts.
- [ ] Add the test fixture and connection string; get direct execution passing against the container.
- [ ] Add the apply and undo templates and cover them with tests.
- [ ] Update supported-dbms messages, help text in each host, Readme.md and AGENTS.md.
- [ ] Run `./psake Test` and compare with the result before this item.

## Notes / log
- 2026-11-13: Item created (first as 0008, covering the container too). Nothing was changed.
- 2026-11-13: Maintainer split it: the container and start task are 0008; this item, renumbered 0009, is the PostgreSQL support in the tool.

## Outcome
